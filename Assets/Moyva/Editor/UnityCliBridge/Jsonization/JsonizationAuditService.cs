using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    public static class JsonizationAuditService
    {
        [Serializable] private sealed class TypeRecord
        {
            public string type;
            public string source;
            public string schema;
            public string domain;
            public bool abstractType;
        }

        [Serializable] private sealed class AssetRecord
        {
            public string type;
            public string path;
            public string guid;
            public string id;
            public string name;
            public bool mainAsset;
        }

        [Serializable] private sealed class ReferenceRecord
        {
            public string hostKind;
            public string hostPath;
            public string hierarchy;
            public string componentType;
            public string propertyPath;
            public string targetType;
            public string targetId;
            public string targetAssetPath;
        }

        [Serializable] private sealed class AuditReport
        {
            public string generatedUtc;
            public int typeCount;
            public int assetCount;
            public int sceneReferenceCount;
            public int prefabReferenceCount;
            public int unsupportedNestedReferenceCount;
            public List<TypeRecord> types = new();
            public List<AssetRecord> assets = new();
            public List<ReferenceRecord> references = new();
            public List<string> warnings = new();
        }

        public static string Run(string reportPath)
        {
            var report = new AuditReport
            {
                generatedUtc = DateTime.UtcNow.ToString("O")
            };

            Type[] projectTypes = JsonizationEditorUtil.ProjectConfigTypes().ToArray();
            var projectTypeSet = new HashSet<Type>(projectTypes);

            foreach (Type type in projectTypes)
            {
                report.types.Add(new TypeRecord
                {
                    type = type.FullName,
                    source = JsonizationEditorUtil.FindScriptPath(type),
                    schema = JsonizationEditorUtil.SchemaName(type),
                    domain = JsonizationEditorUtil.DomainFolder(type),
                    abstractType = type.IsAbstract
                });
            }

            string[] assetGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Moyva" });
            var seenAssetObjects = new HashSet<int>();
            foreach (string guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path) || path.Contains("/Plugins/")) continue;
                UnityEngine.Object[] all;
                try { all = AssetDatabase.LoadAllAssetsAtPath(path); }
                catch { continue; }

                foreach (UnityEngine.Object obj in all)
                {
                    if (obj == null || !seenAssetObjects.Add(obj.GetInstanceID())) continue;
                    Type type = obj.GetType();
                    if (!JsonizationEditorUtil.IsProjectOwned(type) || !typeof(ScriptableObject).IsAssignableFrom(type)) continue;

                    report.assets.Add(new AssetRecord
                    {
                        type = type.FullName,
                        path = path,
                        guid = AssetDatabase.AssetPathToGUID(path),
                        id = JsonizationEditorUtil.StableId(obj, type),
                        name = obj.name,
                        mainAsset = JsonizationEditorUtil.IsMainAsset(obj)
                    });
                }
            }

            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Moyva" });
                foreach (string guid in sceneGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrWhiteSpace(path)) continue;
                    Scene scene;
                    try { scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single); }
                    catch (Exception ex)
                    {
                        report.warnings.Add($"Scene open failed {path}: {ex.GetType().Name}:{ex.Message}");
                        continue;
                    }
                    ScanGameObjects(scene.GetRootGameObjects(), "scene", path, projectTypeSet, report);
                }
            }
            finally
            {
                if (setup != null && setup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Moyva" });
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path)) continue;
                GameObject root = null;
                try
                {
                    root = PrefabUtility.LoadPrefabContents(path);
                    ScanGameObjects(new[] { root }, "prefab", path, projectTypeSet, report);
                }
                catch (Exception ex)
                {
                    report.warnings.Add($"Prefab scan failed {path}: {ex.GetType().Name}:{ex.Message}");
                }
                finally
                {
                    if (root != null) PrefabUtility.UnloadPrefabContents(root);
                }
            }

            report.typeCount = report.types.Count;
            report.assetCount = report.assets.Count;
            report.sceneReferenceCount = report.references.Count(x => x.hostKind == "scene");
            report.prefabReferenceCount = report.references.Count(x => x.hostKind == "prefab");
            report.unsupportedNestedReferenceCount = report.references.Count(x => !IsSupportedBindingPath(x.propertyPath));

            JsonizationEditorUtil.WriteJson(reportPath, report);

            string summary =
                $"MOYVA_JSON_AUDIT types={report.typeCount} assets={report.assetCount} " +
                $"sceneRefs={report.sceneReferenceCount} prefabRefs={report.prefabReferenceCount} " +
                $"unsupportedBindings={report.unsupportedNestedReferenceCount} warnings={report.warnings.Count}";
            Debug.Log("[MoyvaJson] " + summary);
            return summary;
        }


        /// <summary>
        /// Fast inventory used by Pass82 v5.
        /// It intentionally does NOT open scenes or prefabs.
        /// Unity is used here only because project-owned ScriptableObject
        /// types/assets and stable IDs are Unity serialization data.
        /// Scene/prefab binding inspection is performed separately in Pass03.
        /// </summary>
        public static string RunDataOnly(string reportPath)
        {
            var report = new AuditReport
            {
                generatedUtc = DateTime.UtcNow.ToString("O")
            };

            Type[] projectTypes =
                JsonizationEditorUtil.ProjectConfigTypes().ToArray();

            foreach (Type type in projectTypes)
            {
                report.types.Add(new TypeRecord
                {
                    type = type.FullName,
                    source = JsonizationEditorUtil.FindScriptPath(type),
                    schema = JsonizationEditorUtil.SchemaName(type),
                    domain = JsonizationEditorUtil.DomainFolder(type),
                    abstractType = type.IsAbstract
                });
            }

            string[] assetGuids = AssetDatabase.FindAssets(
                "t:ScriptableObject",
                new[] { "Assets/Moyva" });

            var seenAssetObjects = new HashSet<int>();

            foreach (string guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path) ||
                    path.Contains("/Plugins/"))
                    continue;

                UnityEngine.Object[] all;
                try
                {
                    all = AssetDatabase.LoadAllAssetsAtPath(path);
                }
                catch
                {
                    continue;
                }

                foreach (UnityEngine.Object obj in all)
                {
                    if (obj == null ||
                        !seenAssetObjects.Add(obj.GetInstanceID()))
                        continue;

                    Type type = obj.GetType();

                    if (!JsonizationEditorUtil.IsProjectOwned(type) ||
                        !typeof(ScriptableObject).IsAssignableFrom(type))
                        continue;

                    report.assets.Add(new AssetRecord
                    {
                        type = type.FullName,
                        path = path,
                        guid = AssetDatabase.AssetPathToGUID(path),
                        id = JsonizationEditorUtil.StableId(obj, type),
                        name = obj.name,
                        mainAsset = JsonizationEditorUtil.IsMainAsset(obj)
                    });
                }
            }

            report.typeCount = report.types.Count;
            report.assetCount = report.assets.Count;
            report.sceneReferenceCount = 0;
            report.prefabReferenceCount = 0;
            report.unsupportedNestedReferenceCount = 0;

            JsonizationEditorUtil.WriteJson(reportPath, report);

            string summary =
                $"MOYVA_JSON_DATA_AUDIT types={report.typeCount} " +
                $"assets={report.assetCount} sceneScan=deferred prefabScan=deferred";

            Debug.Log("[MoyvaJson] " + summary);
            return summary;
        }

        private static void ScanGameObjects(
            IEnumerable<GameObject> roots,
            string hostKind,
            string hostPath,
            HashSet<Type> projectConfigTypes,
            AuditReport report)
        {
            foreach (GameObject root in roots)
            {
                if (root == null) continue;
                Component[] components = root.GetComponentsInChildren<Component>(true);
                foreach (Component component in components)
                {
                    if (component == null) continue;
                    SerializedObject so;
                    try { so = new SerializedObject(component); }
                    catch { continue; }

                    SerializedProperty iterator = so.GetIterator();
                    bool enter = true;
                    while (iterator.NextVisible(enter))
                    {
                        enter = true;
                        if (iterator.propertyType != SerializedPropertyType.ObjectReference) continue;
                        UnityEngine.Object target = iterator.objectReferenceValue;
                        if (target == null) continue;
                        Type targetType = target.GetType();
                        if (!projectConfigTypes.Contains(targetType) && !JsonizationEditorUtil.IsProjectConfigType(targetType)) continue;

                        report.references.Add(new ReferenceRecord
                        {
                            hostKind = hostKind,
                            hostPath = hostPath,
                            hierarchy = JsonizationEditorUtil.HierarchyPath(component.transform),
                            componentType = component.GetType().FullName,
                            propertyPath = iterator.propertyPath,
                            targetType = targetType.FullName,
                            targetId = JsonizationEditorUtil.StableId(target, targetType),
                            targetAssetPath = AssetDatabase.GetAssetPath(target)
                        });
                    }
                }
            }
        }

        private static bool IsSupportedBindingPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "m_Script") return false;
            // The runtime marker supports direct fields and IList paths emitted by Unity.
            if (path.Contains("managedReferences[", StringComparison.Ordinal)) return false;
            return true;
        }
    }
}
