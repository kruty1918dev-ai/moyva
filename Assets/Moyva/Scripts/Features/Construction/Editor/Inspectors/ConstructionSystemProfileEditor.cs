#if UNITY_EDITOR
using Kruty1918.Moyva.Construction.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    [CustomEditor(typeof(ConstructionSystemProfileSO))]
    internal sealed class ConstructionSystemProfileEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            DrawValidationMessages();
            DrawActions();
            base.OnInspectorGUI();
        }

        private void DrawValidationMessages()
        {
            var profile = (ConstructionSystemProfileSO)target;
            var issues = ConstructionProfileValidator.Validate(profile);
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("Construction system profile is valid.", MessageType.Info);
                return;
            }

            for (int i = 0; i < issues.Count; i++)
                EditorGUILayout.HelpBox(issues[i], MessageType.Warning);
        }

        private void DrawActions()
        {
            var profile = (ConstructionSystemProfileSO)target;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping Registry") && profile.BuildingRegistry != null)
                    EditorGUIUtility.PingObject(profile.BuildingRegistry);

                if (GUILayout.Button("Open Building Designer"))
                    BuildingDesignerWindow.Open();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Validate Profile"))
                    ShowValidationDialog(profile);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping Economy Rules") && profile.EconomyRulesProfile != null)
                    EditorGUIUtility.PingObject(profile.EconomyRulesProfile);

                if (GUILayout.Button("Ping Fog Settings") && profile.FogOfWarSettings != null)
                    EditorGUIUtility.PingObject(profile.FogOfWarSettings);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Dashboard"))
                    ConstructionDashboardWindow.Open();

                if (GUILayout.Button("Open Validation Window"))
                    ConstructionValidationWindow.Open();
            }

            EditorGUILayout.Space(6f);
        }

        private static void ShowValidationDialog(ConstructionSystemProfileSO profile)
        {
            var issues = ConstructionProfileValidator.Validate(profile);
            string message = issues.Count == 0
                ? "Construction system profile is valid."
                : string.Join("\n", issues);

            EditorUtility.DisplayDialog("Construction Profile Validation", message, "OK");
        }
    }
}
#endif
