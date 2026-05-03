using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Kruty1918.Moyva.HomeMenu.UI;

namespace Kruty1918.Moyva.HomeMenu.Editor
{
    [CustomEditor(typeof(NavigationButton))]
    [CanEditMultipleObjects]
    public class NavigationButtonEditor : UnityEditor.Editor
    {
        private SerializedProperty _menuToOpenProp;
        private SerializedProperty _menuToCloseProp;
        private SerializedProperty _openLastProp;

        private void OnEnable()
        {
            _menuToOpenProp = serializedObject.FindProperty("_menuToOpen");
            _menuToCloseProp = serializedObject.FindProperty("_menuToClose");
            _openLastProp = serializedObject.FindProperty("_openLast");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "_menuToOpen", "_menuToClose", "_openLast");

            // Draw toggle for Open Last
            if (_openLastProp != null)
            {
                EditorGUILayout.PropertyField(_openLastProp, new GUIContent("Open Last Closed"));
            }

            var menuNames = CollectMenuNamesFromOpenScenes();

            // If open-last is selected, hide menu selectors
            bool useOpenLast = _openLastProp != null && _openLastProp.boolValue;
            if (!useOpenLast)
            {
                DrawPopupForProperty(_menuToOpenProp, menuNames, "Menu To Open");
                DrawPopupForProperty(_menuToCloseProp, menuNames, "Menu To Close");
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> CollectMenuNamesFromOpenScenes()
        {
            var names = new List<string>();

            var monos = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            foreach (var mb in monos)
            {
                if (mb == null) continue;
                if (mb.gameObject == null) continue;

                var scene = mb.gameObject.scene;
                if (!scene.IsValid()) continue;

                if (mb is INavigationPanel panel)
                {
                    var name = panel.MenuName ?? string.Empty;
                    if (!names.Contains(name))
                        names.Add(name);
                }
            }

            names.Sort();
            return names;
        }

        private void DrawPopupForProperty(SerializedProperty prop, List<string> menuNames, string label)
        {
            var displayList = new List<string> { "— None —" };
            displayList.AddRange(menuNames);

            string current = prop.stringValue ?? string.Empty;
            int idx = 0;
            if (!string.IsNullOrEmpty(current))
            {
                int found = menuNames.IndexOf(current);
                if (found >= 0) idx = found + 1;
                else
                {
                    displayList.Insert(1, current + " (Custom)");
                    idx = 1;
                }
            }

            EditorGUI.BeginChangeCheck();
            int newIdx = EditorGUILayout.Popup(label, idx, displayList.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                if (newIdx <= 0)
                    prop.stringValue = string.Empty;
                else
                {
                    string selected = displayList[newIdx];
                    if (selected.EndsWith(" (Custom)"))
                        prop.stringValue = selected.Substring(0, selected.Length - " (Custom)".Length);
                    else
                        prop.stringValue = selected;
                }
            }
        }
    }
}
