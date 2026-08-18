using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.Jsonization;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    public static class JsonizationSceneMigrationService
    {
        [Serializable] private sealed class HostResult
        {
            public string kind;
            public string path;
            public int bindings;
            public int cleared;
            public string error;
        }

        [Serializable] private sealed class Report
        {
            public string generatedUtc;
            public int scenes;
            public int prefabs;
            public int bindings;
            public int cleared;
            public int errors;
            public List<HostResult> hosts = new();
        }

        public static string CaptureBindings(string reportPath)
        {
            var report = new Report { generatedUtc = DateTime.UtcNow.ToString("O") };
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Moyva" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var host = new HostResult { kind = "scene", path = path };
                    report.hosts.Add(host);
                    try
                    {
                        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                        var records = CollectBindings(scene.GetRootGameObjects());
                        UpsertSceneMarker(scene, records);
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        host.bindings = records.Count;
                        report.scenes++;
                        report.bindings += records.Count;
                    }
                    catch (Exception ex)
                    {
                        host.error = ex.GetType().Name + ":" + ex.Message;
                        report.errors++;
                    }
                }
            }
            finally
            {
                if (setup != null && setup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Moyva" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var host = new HostResult { kind = "prefab", path = path };
                report.hosts.Add(host);
                GameObject root = null;
                try
                {
                    root = PrefabUtility.LoadPrefabContents(path);
                    var records = CollectBindings(new[] { root });
                    if (records.Count > 0)
                    {
                        var marker = root.GetComponent<MoyvaJsonBindingMarker>() ?? root.AddComponent<MoyvaJsonBindingMarker>();
                        marker.ReplaceBindings(records);
                        EditorUtility.SetDirty(marker);
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                    }
                    host.bindings = records.Count;
                    report.prefabs++;
                    report.bindings += records.Count;
                }
                catch (Exception ex)
                {
                    host.error = ex.GetType().Name + ":" + ex.Message;
                    report.errors++;
                }
                finally
                {
                    if (root != null) PrefabUtility.UnloadPrefabContents(root);
                }
            }

            JsonizationEditorUtil.WriteJson(reportPath, report);
            string summary = $"MOYVA_JSON_BINDINGS scenes={report.scenes} prefabs={report.prefabs} bindings={report.bindings} errors={report.errors}";
            if (report.errors > 0) throw new InvalidOperationException(summary);
            return summary;
        }

        public static string ClearSerializedConfigValues(string reportPath)
        {
            var report = new Report { generatedUtc = DateTime.UtcNow.ToString("O") };
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Moyva" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var host = new HostResult { kind = "scene", path = path };
                    report.hosts.Add(host);
                    try
                    {
                        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                        int cleared = 0;
                        foreach (GameObject root in scene.GetRootGameObjects())
                            foreach (MoyvaJsonBindingMarker marker in root.GetComponentsInChildren<MoyvaJsonBindingMarker>(true))
                                cleared += ClearMarkerTargets(marker);
                        if (cleared > 0)
                        {
                            EditorSceneManager.MarkSceneDirty(scene);
                            EditorSceneManager.SaveScene(scene);
                        }
                        host.cleared = cleared;
                        report.cleared += cleared;
                        report.scenes++;
                    }
                    catch (Exception ex)
                    {
                        host.error = ex.GetType().Name + ":" + ex.Message;
                        report.errors++;
                    }
                }
            }
            finally
            {
                if (setup != null && setup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Moyva" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = null;
                var host = new HostResult { kind = "prefab", path = path };
                report.hosts.Add(host);
                try
                {
                    root = PrefabUtility.LoadPrefabContents(path);
                    int cleared = 0;
                    foreach (MoyvaJsonBindingMarker marker in root.GetComponentsInChildren<MoyvaJsonBindingMarker>(true))
                        cleared += ClearMarkerTargets(marker);
                    if (cleared > 0) PrefabUtility.SaveAsPrefabAsset(root, path);
                    host.cleared = cleared;
                    report.cleared += cleared;
                    report.prefabs++;
                }
                catch (Exception ex)
                {
                    host.error = ex.GetType().Name + ":" + ex.Message;
                    report.errors++;
                }
                finally
                {
                    if (root != null) PrefabUtility.UnloadPrefabContents(root);
                }
            }

            AssetDatabase.SaveAssets();
            JsonizationEditorUtil.WriteJson(reportPath, report);
            string summary = $"MOYVA_JSON_SCENE_REWRITE cleared={report.cleared} errors={report.errors}";
            if (report.errors > 0) throw new InvalidOperationException(summary);
            return summary;
        }

        [Serializable] private sealed class HostSpec
        {
            public string kind;
            public string path;
        }

        [Serializable] private sealed class HostManifest
        {
            public List<HostSpec> hosts = new();
        }

        public static string CaptureBindingsBatch(string manifestPath, string reportPath)
        {
            return ProcessBatch(manifestPath, reportPath, clear: false);
        }

        public static string ClearSerializedConfigValuesBatch(string manifestPath, string reportPath)
        {
            return ProcessBatch(manifestPath, reportPath, clear: true);
        }

        private static string ProcessBatch(string manifestPath, string reportPath, bool clear)
        {
            if (string.IsNullOrWhiteSpace(manifestPath) || !File.Exists(manifestPath))
                throw new FileNotFoundException("Host manifest not found.", manifestPath);

            var manifest = JsonConvert.DeserializeObject<HostManifest>(File.ReadAllText(manifestPath))
                ?? new HostManifest();
            var report = new Report { generatedUtc = DateTime.UtcNow.ToString("O") };
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (HostSpec spec in manifest.hosts ?? new List<HostSpec>())
                {
                    if (spec == null || string.IsNullOrWhiteSpace(spec.path))
                        continue;
                    if (string.Equals(spec.kind, "scene", StringComparison.OrdinalIgnoreCase))
                        ProcessSceneHost(spec.path, clear, report);
                    else if (string.Equals(spec.kind, "prefab", StringComparison.OrdinalIgnoreCase))
                        ProcessPrefabHost(spec.path, clear, report);
                    else
                    {
                        report.hosts.Add(new HostResult { kind = spec.kind, path = spec.path, error = "unsupported-host-kind" });
                        report.errors++;
                    }
                }
            }
            finally
            {
                if (setup != null && setup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }

            // Per-host mutations are already persisted explicitly. Avoid a global
            // SaveAssets after every small batch: on large projects it can force an
            // unrelated import/save cycle and hit the Pipeline main-thread timeout.
            JsonizationEditorUtil.WriteJson(reportPath, report);
            string summary = clear
                ? $"MOYVA_JSON_SCENE_REWRITE_BATCH scenes={report.scenes} prefabs={report.prefabs} cleared={report.cleared} errors={report.errors}"
                : $"MOYVA_JSON_BINDINGS_BATCH scenes={report.scenes} prefabs={report.prefabs} bindings={report.bindings} errors={report.errors}";
            if (report.errors > 0) throw new InvalidOperationException(summary);
            return summary;
        }

        private static void ProcessSceneHost(string path, bool clear, Report report)
        {
            var host = new HostResult { kind = "scene", path = path };
            report.hosts.Add(host);
            try
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                if (clear)
                {
                    int cleared = 0;
                    foreach (GameObject root in scene.GetRootGameObjects())
                        foreach (MoyvaJsonBindingMarker marker in root.GetComponentsInChildren<MoyvaJsonBindingMarker>(true))
                            cleared += ClearMarkerTargets(marker);
                    if (cleared > 0)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                    host.cleared = cleared;
                    report.cleared += cleared;
                }
                else
                {
                    var records = CollectBindings(scene.GetRootGameObjects());
                    UpsertSceneMarker(scene, records);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    host.bindings = records.Count;
                    report.bindings += records.Count;
                }
                report.scenes++;
            }
            catch (Exception ex)
            {
                host.error = ex.GetType().Name + ":" + ex.Message;
                report.errors++;
            }
        }

        private static void ProcessPrefabHost(string path, bool clear, Report report)
        {
            var host = new HostResult { kind = "prefab", path = path };
            report.hosts.Add(host);
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(path);
                if (clear)
                {
                    int cleared = 0;
                    foreach (MoyvaJsonBindingMarker marker in root.GetComponentsInChildren<MoyvaJsonBindingMarker>(true))
                        cleared += ClearMarkerTargets(marker);
                    if (cleared > 0) PrefabUtility.SaveAsPrefabAsset(root, path);
                    host.cleared = cleared;
                    report.cleared += cleared;
                }
                else
                {
                    var records = CollectBindings(new[] { root });
                    var marker = root.GetComponent<MoyvaJsonBindingMarker>();
                    if (records.Count > 0)
                    {
                        marker ??= root.AddComponent<MoyvaJsonBindingMarker>();
                        marker.ReplaceBindings(records);
                        EditorUtility.SetDirty(marker);
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                    }
                    else if (marker != null)
                    {
                        UnityEngine.Object.DestroyImmediate(marker);
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                    }
                    host.bindings = records.Count;
                    report.bindings += records.Count;
                }
                report.prefabs++;
            }
            catch (Exception ex)
            {
                host.error = ex.GetType().Name + ":" + ex.Message;
                report.errors++;
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static List<MoyvaJsonBindingRecord> CollectBindings(IEnumerable<GameObject> roots)
        {
            var result = new List<MoyvaJsonBindingRecord>();
            foreach (GameObject root in roots)
            {
                if (root == null) continue;
                foreach (Component component in root.GetComponentsInChildren<Component>(true))
                {
                    if (component == null || component is MoyvaJsonBindingMarker) continue;
                    SerializedObject so;
                    try { so = new SerializedObject(component); }
                    catch { continue; }
                    SerializedProperty it = so.GetIterator();
                    bool enter = true;
                    while (it.NextVisible(enter))
                    {
                        enter = true;
                        if (it.propertyType != SerializedPropertyType.ObjectReference || it.propertyPath == "m_Script") continue;
                        UnityEngine.Object target = it.objectReferenceValue;
                        if (target == null || !JsonizationEditorUtil.IsProjectConfigType(target.GetType())) continue;
                        result.Add(new MoyvaJsonBindingRecord
                        {
                            Target = component,
                            PropertyPath = it.propertyPath,
                            ConfigType = target.GetType().FullName,
                            ConfigId = JsonizationEditorUtil.StableId(target, target.GetType())
                        });
                    }
                }
            }
            return result;
        }

        private static void UpsertSceneMarker(Scene scene, List<MoyvaJsonBindingRecord> records)
        {
            MoyvaJsonBindingMarker marker = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                marker = root.GetComponent<MoyvaJsonBindingMarker>();
                if (marker != null && root.name == "__MoyvaJsonBindings") break;
                marker = null;
            }

            if (records.Count == 0)
            {
                if (marker != null) UnityEngine.Object.DestroyImmediate(marker.gameObject);
                return;
            }

            if (marker == null)
            {
                var go = new GameObject("__MoyvaJsonBindings");
                SceneManager.MoveGameObjectToScene(go, scene);
                marker = go.AddComponent<MoyvaJsonBindingMarker>();
            }
            marker.ReplaceBindings(records);
            EditorUtility.SetDirty(marker);
        }

        private static int ClearMarkerTargets(MoyvaJsonBindingMarker marker)
        {
            int count = 0;
            foreach (MoyvaJsonBindingRecord binding in marker.Bindings)
            {
                if (binding?.Target == null || string.IsNullOrWhiteSpace(binding.PropertyPath)) continue;
                try
                {
                    if (SetNull(binding.Target, binding.PropertyPath)) count++;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[MoyvaJson] Could not clear {binding.Target.GetType().Name}.{binding.PropertyPath}: {ex.Message}");
                }
            }
            return count;
        }

        private static bool SetNull(object root, string propertyPath)
        {
            string normalized = propertyPath.Replace(".Array.data[", "[");
            string[] segments = normalized.Split('.');
            object current = root;
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                int bracket = segment.IndexOf('[');
                string fieldName = bracket >= 0 ? segment.Substring(0, bracket) : segment;
                FieldInfo field = JsonizationEditorUtil.FindField(current.GetType(), fieldName);
                if (field == null) return false;
                bool last = i == segments.Length - 1;
                object fieldValue = field.GetValue(current);
                if (bracket < 0)
                {
                    if (last)
                    {
                        if (field.FieldType.IsValueType) return false;
                        field.SetValue(current, null);
                        EditorUtility.SetDirty(root as UnityEngine.Object);
                        return true;
                    }
                    if (fieldValue == null) return false;
                    current = fieldValue;
                    continue;
                }

                int end = segment.IndexOf(']', bracket + 1);
                if (end < 0 || !int.TryParse(segment.Substring(bracket + 1, end - bracket - 1), out int index)) return false;
                if (!(fieldValue is IList list) || index < 0 || index >= list.Count) return false;
                if (last)
                {
                    list[index] = null;
                    EditorUtility.SetDirty(root as UnityEngine.Object);
                    return true;
                }
                current = list[index];
                if (current == null) return false;
            }
            return false;
        }
    }
}
