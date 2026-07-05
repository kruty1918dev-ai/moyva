using System.Text;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public sealed class ConstructionDashboardWindow : OdinEditorWindow
    {
        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(-20)]
        public ConstructionSceneContext SceneContext { get; private set; }

        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(-19)]
        public ConstructionSystemProfileSO SystemProfile { get; private set; }

        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(-18)]
        [MultiLineProperty(8)]
        public string Summary { get; private set; }

        [MenuItem("Moyva/Tools/Construction/Dashboard", priority = 34)]
        public static void Open()
        {
            var window = GetWindow<ConstructionDashboardWindow>("Construction Dashboard");
            window.minSize = new Vector2(520f, 460f);
            window.Refresh();
            window.Show();
            window.Focus();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        [Button(ButtonSizes.Large)]
        public void Refresh()
        {
            SceneContext = FindFirstObjectByType<ConstructionSceneContext>(FindObjectsInactive.Include);
            SystemProfile = SceneContext != null ? SceneContext.SystemProfile : null;
            Summary = BuildSummary(SceneContext);
        }

        [Button]
        public void PingSceneContext()
        {
            if (SceneContext != null)
                EditorGUIUtility.PingObject(SceneContext);
        }

        [Button]
        public void PingSystemProfile()
        {
            if (SystemProfile != null)
                EditorGUIUtility.PingObject(SystemProfile);
        }

        [Button]
        public void OpenValidationWindow()
        {
            ConstructionValidationWindow.Open();
        }

        private static string BuildSummary(ConstructionSceneContext sceneContext)
        {
            if (sceneContext == null)
                return "No ConstructionSceneContext found in the currently loaded scene.";

            var builder = new StringBuilder();
            builder.AppendLine($"Scene Context: {sceneContext.name}");
            builder.AppendLine($"System Profile: {sceneContext.SystemProfile?.name ?? "Missing"}");
            builder.AppendLine($"Registry: {sceneContext.BuildingRegistry?.name ?? "Missing"}");
            builder.AppendLine($"Placement Profile: {sceneContext.ResolvePlacementRulesProfile()?.name ?? "Missing"}");
            builder.AppendLine($"Visual Profile: {sceneContext.ResolveVisualProfile()?.name ?? "Missing"}");
            builder.AppendLine($"Input Profile: {sceneContext.ResolveInputProfile()?.name ?? "Missing"}");
            builder.AppendLine($"Wall Profile: {sceneContext.ResolveWallProfile()?.name ?? "Missing"}");
            builder.AppendLine($"Diagnostics Profile: {sceneContext.ResolveDiagnosticsProfile()?.name ?? "Missing"}");
            builder.AppendLine($"Economy Rules: {sceneContext.SystemProfile?.EconomyRulesProfile?.name ?? "Missing"}");
            builder.AppendLine($"Fog Settings: {sceneContext.SystemProfile?.FogOfWarSettings?.name ?? "Missing"}");
            builder.AppendLine($"Preview Root: {sceneContext.SceneRoots?.PreviewRoot?.name ?? "Missing"}");
            builder.AppendLine($"Placed Root: {sceneContext.SceneRoots?.PlacedRoot?.name ?? "Missing"}");
            builder.AppendLine($"Radius Root: {sceneContext.SceneRoots?.RadiusRoot?.name ?? "Missing"}");
            builder.AppendLine($"UI Root: {sceneContext.SceneRoots?.UIRoot?.name ?? "Missing"}");
            builder.AppendLine($"Debug Root: {sceneContext.SceneRoots?.DebugRoot?.name ?? "Missing"}");
            return builder.ToString().TrimEnd();
        }
    }
}
