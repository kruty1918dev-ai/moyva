using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Editor.Shared;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    [CustomEditor(typeof(BuildingRegistrySO))]
    public sealed class BuildingRegistryEditor : UnityEditor.Editor
    {
        private SerializedProperty _buildingAssets;
        private SerializedProperty _legacyBuildings;
        private SerializedProperty _wallCollections;
        private bool _showLegacy;

        private void OnEnable()
        {
            _buildingAssets = serializedObject.FindProperty("_buildingAssets");
            _legacyBuildings = serializedObject.FindProperty("Buildings");
            _wallCollections = serializedObject.FindProperty("WallCollections");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RegistryEditorStyles.DrawColoredHeader("  Building Registry", RegistryEditorStyles.Accent);
            EditorGUILayout.Space(2);

            DrawSummary();
            DrawActions();

            RegistryEditorStyles.DrawSeparator();
            EditorGUILayout.PropertyField(_buildingAssets, new GUIContent("Building Definition Assets"), includeChildren: true);
            EditorGUILayout.PropertyField(_wallCollections, includeChildren: true);

            RegistryEditorStyles.DrawSeparator();
            DrawLegacySection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSummary()
        {
            var registry = (BuildingRegistrySO)target;
            int assetCount = registry.BuildingAssets.Length;
            int legacyCount = registry.LegacyBuildings.Length;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{assetCount} asset definition(s)", EditorStyles.boldLabel);
                if (legacyCount > 0)
                    EditorGUILayout.LabelField($"{legacyCount} legacy inline definition(s)", EditorStyles.miniLabel);
            }

            if (legacyCount > 0)
            {
                EditorGUILayout.HelpBox(
                    "Legacy inline definitions are kept only as migration source. New construction data should live in BuildingDefinition assets.",
                    MessageType.Info);
            }
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Odin Build Designer"))
                    EditorApplication.ExecuteMenuItem("Moyva/Tools/Building Designer");

                if (GUILayout.Button("Open Migration Tool"))
                    EditorApplication.ExecuteMenuItem("Moyva/Tools/Construction/Building Migration");

                if (GUILayout.Button("Rebuild From Assets"))
                    RebuildFromAssets();

                if (GUILayout.Button("Validate"))
                    ValidateRegistry();
            }
        }

        private void DrawLegacySection()
        {
            _showLegacy = EditorGUILayout.Foldout(_showLegacy, "Legacy Inline Definitions (deprecated)", true);
            if (!_showLegacy)
                return;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_legacyBuildings, includeChildren: true);
            }
        }

        private void RebuildFromAssets()
        {
            var guids = AssetDatabase.FindAssets("t:BuildingDefinitionAsset");
            var assets = new List<BuildingDefinitionAsset>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<BuildingDefinitionAsset>(path);
                if (asset != null)
                    assets.Add(asset);
            }

            Undo.RecordObject(target, "Rebuild Building Registry From Assets");
            ((BuildingRegistrySO)target).SetBuildingAssets(assets);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            serializedObject.Update();

            Debug.Log($"[BuildingRegistry] Rebuilt asset list with {assets.Count} BuildingDefinition asset(s).", target);
        }

        private void ValidateRegistry()
        {
            var registry = (BuildingRegistrySO)target;
            var issues = BuildingValidator.ValidateRegistry(registry);
            int errors = 0;
            int warnings = 0;
            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i].Severity == BuildingValidationSeverity.Error)
                    errors++;
                else
                    warnings++;
            }

            if (errors == 0 && warnings == 0)
            {
                Debug.Log("[BuildingRegistry] Validation passed.", target);
                return;
            }

            Debug.LogWarning($"[BuildingRegistry] Validation found {errors} error(s), {warnings} warning(s):\n{string.Join("\n", issues)}", target);
        }
    }
}
