using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public sealed class ConstructionValidationWindow : OdinEditorWindow
    {
        [ShowInInspector]
        [ReadOnly]
        public ConstructionSceneContext SceneContext { get; private set; }

        [ShowInInspector]
        [ReadOnly]
        public ConstructionSystemProfileSO SystemProfile { get; private set; }

        [ShowInInspector]
        [ReadOnly]
        [MultiLineProperty(16)]
        public string ValidationReport { get; private set; }

        [MenuItem("Moyva/Tools/Construction/Validation", priority = 35)]
        public static void Open()
        {
            var window = GetWindow<ConstructionValidationWindow>("Construction Validation");
            window.minSize = new Vector2(560f, 520f);
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
            ValidationReport = BuildValidationReport(SceneContext, SystemProfile);
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

        private static string BuildValidationReport(ConstructionSceneContext sceneContext, ConstructionSystemProfileSO systemProfile)
        {
            var builder = new StringBuilder();
            AppendIssues(builder, "Scene", ConstructionSceneValidator.Validate(sceneContext));
            AppendIssues(builder, "Profile", ConstructionProfileValidator.Validate(systemProfile));
            return builder.Length == 0 ? "Construction validation passed." : builder.ToString().TrimEnd();
        }

        private static void AppendIssues(StringBuilder builder, string label, IReadOnlyList<string> issues)
        {
            if (issues == null || issues.Count == 0)
            {
                builder.AppendLine($"{label}: OK");
                return;
            }

            builder.AppendLine($"{label}:");
            for (int i = 0; i < issues.Count; i++)
                builder.AppendLine($"- {issues[i]}");
        }
    }
}
