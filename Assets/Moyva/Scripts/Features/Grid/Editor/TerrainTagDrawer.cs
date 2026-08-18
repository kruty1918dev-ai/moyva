#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Editor
{
    /// <summary>
    /// Turns a <see cref="TerrainTagAttribute"/> string into a safe catalog picker.
    /// Unknown legacy values remain selected until the user explicitly changes them.
    /// </summary>
    [CustomPropertyDrawer(typeof(TerrainTagAttribute))]
    public sealed class TerrainTagDrawer : PropertyDrawer
    {
        private const float CustomButtonWidth = 25f;
        private const float Spacing = 3f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label, includeChildren: true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            Rect contentRect = EditorGUI.PrefixLabel(position, label);
            Rect pickerRect = new Rect(
                contentRect.x,
                contentRect.y,
                Mathf.Max(20f, contentRect.width - CustomButtonWidth - Spacing),
                contentRect.height);
            Rect customRect = new Rect(pickerRect.xMax + Spacing, contentRect.y, CustomButtonWidth, contentRect.height);

            string currentValue = property.hasMultipleDifferentValues ? "— різні значення —" : property.stringValue;
            string buttonLabel = string.IsNullOrWhiteSpace(currentValue) ? "Оберіть terrain tag…" : $"# {currentValue}";
            var targets = property.serializedObject.targetObjects;
            string propertyPath = property.propertyPath;

            if (EditorGUI.DropdownButton(
                    pickerRect,
                    new GUIContent(buttonLabel, "Оберіть тег із terrain-профілів. Поточне нестандартне значення не буде втрачено."),
                    FocusType.Keyboard))
            {
                ShowTagMenu(pickerRect, property.hasMultipleDifferentValues ? string.Empty : property.stringValue, value =>
                    AssignValue(targets, propertyPath, value));
            }

            if (GUI.Button(customRect, new GUIContent("+", "Створити власний terrain tag"), EditorStyles.miniButton))
            {
                PopupWindow.Show(customRect, new CreateTerrainTagPopup(value =>
                    AssignValue(targets, propertyPath, value)));
            }

            EditorGUI.EndProperty();
        }

        private static void ShowTagMenu(Rect anchorRect, string currentValue, Action<string> onSelected)
        {
            var menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("(без тегу)"),
                string.IsNullOrWhiteSpace(currentValue),
                () => onSelected(string.Empty));
            menu.AddSeparator(string.Empty);

            for (int index = 0; index < TerrainTagEditorCatalog.Suggestions.Length; index++)
            {
                var suggestion = TerrainTagEditorCatalog.Suggestions[index];
                string value = suggestion.Value;
                menu.AddItem(
                    new GUIContent($"Рекомендовані/{suggestion.MenuLabel}"),
                    string.Equals(currentValue?.Trim(), value, StringComparison.OrdinalIgnoreCase),
                    () => onSelected(value));
            }

            IReadOnlyList<string> customTags = TerrainTagEditorCatalog.GetCustomTags();
            if (customTags.Count > 0)
            {
                menu.AddSeparator("З terrain-профілів/");
                for (int index = 0; index < customTags.Count; index++)
                {
                    string value = customTags[index];
                    menu.AddItem(
                        new GUIContent($"З terrain-профілів/{value}"),
                        string.Equals(currentValue?.Trim(), value, StringComparison.OrdinalIgnoreCase),
                        () => onSelected(value));
                }
            }

            if (!string.IsNullOrWhiteSpace(currentValue) &&
                !TerrainTagEditorCatalog.IsSuggested(currentValue) &&
                !Contains(customTags, currentValue))
            {
                menu.AddSeparator(string.Empty);
                menu.AddDisabledItem(new GUIContent($"Поточний нестандартний: {currentValue}"), true);
            }

            menu.DropDown(anchorRect);
        }

        private static void AssignValue(UnityEngine.Object[] targets, string propertyPath, string value)
        {
            if (targets == null || targets.Length == 0)
                return;

            Undo.RecordObjects(targets, "Змінити terrain tag");
            var serializedTargets = new SerializedObject(targets);
            serializedTargets.Update();
            var currentProperty = serializedTargets.FindProperty(propertyPath);
            if (currentProperty == null || currentProperty.propertyType != SerializedPropertyType.String)
                return;

            currentProperty.stringValue = value ?? string.Empty;
            serializedTargets.ApplyModifiedProperties();
            for (int index = 0; index < targets.Length; index++)
                EditorUtility.SetDirty(targets[index]);
        }

        private static bool Contains(IReadOnlyList<string> values, string candidate)
        {
            for (int index = 0; index < values.Count; index++)
            {
                if (string.Equals(values[index], candidate?.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}

#endif
