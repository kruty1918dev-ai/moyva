#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiRedesign
{
    public sealed class GameplayUiRedesignWindow : EditorWindow
    {
        private string _last;
        private Vector2 _scroll;

        [MenuItem("Moyva/Tools/Unity CLI/Gameplay UI Redesign", priority = 22)]
        public static void Open() => GetWindow<GameplayUiRedesignWindow>("Gameplay UI Redesign");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Moyva · Gameplay UI Pass73", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Pass73 is a foundation pass based on the live Gameplay audit. Apply mutates the loaded scene in memory only; saving remains explicit and externally backed up.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Plan")) _last = GameplayUiRedesignService.Plan();
                if (GUILayout.Button("Validate")) _last = GameplayUiRedesignService.Validate();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Preview Normal")) _last = GameplayUiRedesignService.PreviewMode("normal");
                if (GUILayout.Button("Preview Construction")) _last = GameplayUiRedesignService.PreviewMode("construction");
                if (GUILayout.Button("Restore Normal")) _last = GameplayUiRedesignService.PreviewMode("restore");
            }

            EditorGUILayout.Space(8);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(string.IsNullOrEmpty(_last) ? "Use Plan first." : _last, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }
}

#endif
