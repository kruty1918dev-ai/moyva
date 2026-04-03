using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Editor
{
    [CustomPropertyDrawer(typeof(TileIdAttribute))]
    public class TileIdDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            string[] ids = GetTileIds();
            string currentValue = property.stringValue;
            bool isValid = !string.IsNullOrEmpty(currentValue) && System.Array.IndexOf(ids, currentValue) >= 0;

            Color prevColor = GUI.color;
            if (!isValid && !string.IsNullOrEmpty(currentValue))
                GUI.color = Color.red;

            var displayOptions = BuildDisplayOptions(ids, currentValue, isValid, out int selectedIndex);

            EditorGUI.BeginProperty(position, label, property);

            int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, displayOptions);

            if (newIndex != selectedIndex)
            {
                string selected = displayOptions[newIndex];
                if (selected == "(none)")
                    property.stringValue = string.Empty;
                else if (!selected.StartsWith("\u26a0 "))
                    property.stringValue = selected;
            }

            EditorGUI.EndProperty();
            GUI.color = prevColor;
        }

        private static string[] BuildDisplayOptions(string[] ids, string currentValue, bool isValid, out int selectedIndex)
        {
            var list = new List<string> { "(none)" };
            list.AddRange(ids);

            if (!isValid && !string.IsNullOrEmpty(currentValue))
            {
                list.Insert(0, $"\u26a0 {currentValue}");
                selectedIndex = 0;
            }
            else
            {
                selectedIndex = string.IsNullOrEmpty(currentValue) ? 0 : list.IndexOf(currentValue);
                if (selectedIndex < 0) selectedIndex = 0;
            }

            return list.ToArray();
        }

        private static string[] GetTileIds()
        {
            string[] guids = AssetDatabase.FindAssets("t:TileRegistrySO");
            if (guids.Length == 0) return System.Array.Empty<string>();

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var registry = AssetDatabase.LoadAssetAtPath<TileRegistrySO>(path);
            if (registry?.Definitions == null) return System.Array.Empty<string>();

            var ids = new List<string>();
            foreach (var def in registry.Definitions)
                if (!string.IsNullOrEmpty(def.Id))
                    ids.Add(def.Id);

            ids.Sort();
            return ids.ToArray();
        }
    }
}
