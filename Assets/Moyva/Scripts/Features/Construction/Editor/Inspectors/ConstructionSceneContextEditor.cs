#if UNITY_EDITOR
using Kruty1918.Moyva.Construction.Runtime;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    [CustomEditor(typeof(ConstructionSceneContext))]
    internal sealed class ConstructionSceneContextEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            DrawValidationMessages();
            DrawActions();
            base.OnInspectorGUI();
        }

        private void DrawValidationMessages()
        {
            var sceneContext = (ConstructionSceneContext)target;
            var issues = ConstructionSceneValidator.Validate(sceneContext);
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("Construction scene context is valid.", MessageType.Info);
                return;
            }

            for (int i = 0; i < issues.Count; i++)
                EditorGUILayout.HelpBox(issues[i], MessageType.Warning);
        }

        private void DrawActions()
        {
            var sceneContext = (ConstructionSceneContext)target;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Missing Roots"))
                {
                    Undo.RecordObject(sceneContext.gameObject, "Create Construction Roots");
                    sceneContext.CreateMissingRoots();
                    EditorUtility.SetDirty(sceneContext);
                }

                if (GUILayout.Button("Auto Find Roots"))
                {
                    Undo.RecordObject(sceneContext.gameObject, "Auto Find Construction Roots");
                    sceneContext.AutoFindRoots();
                    EditorUtility.SetDirty(sceneContext);
                }

                if (GUILayout.Button("Clear Empty Roots"))
                {
                    Undo.RecordObject(sceneContext.gameObject, "Clear Empty Construction Roots");
                    sceneContext.ClearEmptyRoots();
                    EditorUtility.SetDirty(sceneContext);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping Registry") && sceneContext.BuildingRegistry != null)
                    EditorGUIUtility.PingObject(sceneContext.BuildingRegistry);

                if (GUILayout.Button("Ping All Roots"))
                {
                    if (sceneContext.SceneRoots?.PreviewRoot != null)
                        EditorGUIUtility.PingObject(sceneContext.SceneRoots.PreviewRoot);
                    if (sceneContext.SceneRoots?.PlacedRoot != null)
                        EditorGUIUtility.PingObject(sceneContext.SceneRoots.PlacedRoot);
                    if (sceneContext.SceneRoots?.RadiusRoot != null)
                        EditorGUIUtility.PingObject(sceneContext.SceneRoots.RadiusRoot);
                    if (sceneContext.SceneRoots?.UIRoot != null)
                        EditorGUIUtility.PingObject(sceneContext.SceneRoots.UIRoot);
                    if (sceneContext.SceneRoots?.DebugRoot != null)
                        EditorGUIUtility.PingObject(sceneContext.SceneRoots.DebugRoot);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select Preview Root") && sceneContext.SceneRoots?.PreviewRoot != null)
                    Selection.activeTransform = sceneContext.SceneRoots.PreviewRoot;

                if (GUILayout.Button("Select Placed Root") && sceneContext.SceneRoots?.PlacedRoot != null)
                    Selection.activeTransform = sceneContext.SceneRoots.PlacedRoot;

                if (GUILayout.Button("Select Radius Root") && sceneContext.SceneRoots?.RadiusRoot != null)
                    Selection.activeTransform = sceneContext.SceneRoots.RadiusRoot;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select UI Root") && sceneContext.SceneRoots?.UIRoot != null)
                    Selection.activeTransform = sceneContext.SceneRoots.UIRoot;

                if (GUILayout.Button("Select Debug Root") && sceneContext.SceneRoots?.DebugRoot != null)
                    Selection.activeTransform = sceneContext.SceneRoots.DebugRoot;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Building Designer"))
                    BuildingDesignerWindow.Open();

                if (GUILayout.Button("Open Wall Registry"))
                    WallRegistryWindow.Open();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Validate Scene"))
                    ShowValidationDialog(sceneContext);

                if (GUILayout.Button("Ping System Profile") && sceneContext.SystemProfile != null)
                    EditorGUIUtility.PingObject(sceneContext.SystemProfile);
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

        private static void ShowValidationDialog(ConstructionSceneContext sceneContext)
        {
            var issues = ConstructionSceneValidator.Validate(sceneContext);
            string message = issues.Count == 0
                ? "Construction scene context is valid."
                : string.Join("\n", issues);

            EditorUtility.DisplayDialog("Construction Scene Validation", message, "OK");
        }
    }
}
#endif
