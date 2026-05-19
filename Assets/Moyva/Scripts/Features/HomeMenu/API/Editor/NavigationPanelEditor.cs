using Kruty1918.Moyva.HomeMenu.UI;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Editor
{
    /// <summary>
    /// Кастомний інспектор для <see cref="NavigationPanel"/>.
    /// Автоматично заповнює порожній <c>MenuName</c> ім'ям GameObject як fallback.
    /// </summary>
    [CustomEditor(typeof(NavigationPanel))]
    [CanEditMultipleObjects]
    public class NavigationPanelEditor : UnityEditor.Editor
    {
        private SerializedProperty _menuNameProp;

        private void OnEnable()
        {
            // Avoid creating SerializedObject when any target is null (can happen if a component was removed)
            if (targets == null || targets.Length == 0)
                return;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null)
                    return;
            }

            _menuNameProp = serializedObject.FindProperty("_menuName");
        }

        public override void OnInspectorGUI()
        {
            // Ensure targets are valid before touching serializedObject
            if (targets == null || targets.Length == 0)
                return;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null)
                {
                    EditorGUILayout.HelpBox("One or more target objects are missing or destroyed; select a valid object.", MessageType.Warning);
                    return;
                }
            }

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