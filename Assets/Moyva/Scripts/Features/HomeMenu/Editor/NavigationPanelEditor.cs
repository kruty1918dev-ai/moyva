using Kruty1918.Moyva.HomeMenu.UI;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Editor
{
    [CustomEditor(typeof(NavigationPanel))]
    [CanEditMultipleObjects]
    public class NavigationPanelEditor : UnityEditor.Editor
    {
        private SerializedProperty _menuNameProp;

        private void OnEnable()
        {
            _menuNameProp = serializedObject.FindProperty("_menuName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            bool anyFilled = false;

            foreach (var obj in serializedObject.targetObjects)
            {
                if (obj is NavigationPanel panel)
                {
                    if (string.IsNullOrWhiteSpace(panel.MenuName))
                    {
                        var so = new SerializedObject(panel);
                        var prop = so.FindProperty("_menuName");
                        if (prop != null)
                        {
                            prop.stringValue = panel.gameObject.name;
                            so.ApplyModifiedProperties();
                            anyFilled = true;
                        }
                    }
                }
            }

            if (anyFilled)
                EditorGUILayout.HelpBox("Порожнє поле MenuName заповнено ім'ям GameObject як фолбек.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}