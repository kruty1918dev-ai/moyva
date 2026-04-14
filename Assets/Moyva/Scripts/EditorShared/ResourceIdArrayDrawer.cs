using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    [CustomPropertyDrawer(typeof(ResourceIdArrayAttribute))]
    public class ResourceIdArrayDrawer : PropertyDrawer
    {
        private ReorderableListSurrogate _listSurrogate;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String && 
                !property.name.Contains("[]") && 
                property.isArray)
            {
                EnsureListSurrogate(property);
                return _listSurrogate.GetPropertyHeight(property, label);
            }

            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.isArray && property.propertyType == SerializedPropertyType.String)
            {
                EnsureListSurrogate(property);
                _listSurrogate.OnGUI(position, property, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        private void EnsureListSurrogate(SerializedProperty property)
        {
            if (_listSurrogate == null)
            {
                _listSurrogate = new ReorderableListSurrogate();
            }
        }

        private sealed class ReorderableListSurrogate
        {
            private UnityEditorInternal.ReorderableList _list;
            private SerializedProperty _currentProperty;
            private Dictionary<string, EconomyDatabaseSO> _dbCache = new();
            private Dictionary<string, string[]> _idsCache = new();

            public float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                EnsureList(property);
                return _list?.GetHeight() ?? EditorGUIUtility.singleLineHeight;
            }

            public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EnsureList(property);
                if (_list != null)
                    _list.DoList(position);
            }

            private void EnsureList(SerializedProperty property)
            {
                if (_list != null && _currentProperty?.propertyPath == property.propertyPath)
                    return;

                _currentProperty = property;

                var elementProp = property.GetArrayElementAtIndex(0);
                _list = new UnityEditorInternal.ReorderableList(
                    property.serializedObject,
                    property,
                    draggable: true,
                    displayHeader: true,
                    displayAddButton: true,
                    displayRemoveButton: true);

                _list.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, property.displayName ?? property.name, EditorStyles.boldLabel);
                };

                _list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index < 0 || index >= property.arraySize)
                        return;

                    var elementProp = property.GetArrayElementAtIndex(index);
                    DrawResourceIdElement(rect, elementProp, index);
                };

                _list.onAddCallback = (UnityEditorInternal.ReorderableList list) =>
                {
                    int idx = property.arraySize;
                    property.arraySize++;
                    property.GetArrayElementAtIndex(idx).stringValue = string.Empty;
                };
            }

            private void DrawResourceIdElement(Rect rect, SerializedProperty elementProperty, int index)
            {
                string[] ids = GetResourceIds();
                string currentValue = elementProperty.stringValue;
                bool isValid = !string.IsNullOrEmpty(currentValue) && System.Array.IndexOf(ids, currentValue) >= 0;

                float lineH = EditorGUIUtility.singleLineHeight;
                float spriteSize = lineH;
                Rect popupRect = new Rect(rect.x, rect.y, rect.width - spriteSize - 4, lineH);
                Rect spriteRect = new Rect(rect.xMax - spriteSize, rect.y, spriteSize, spriteSize);

                Color prevColor = GUI.color;
                if (!isValid && !string.IsNullOrEmpty(currentValue))
                    GUI.color = Color.red;

                var displayOptions = BuildDisplayOptions(ids, currentValue, isValid, out int selectedIndex);

                int newIndex = EditorGUI.Popup(popupRect, $"[{index}]", selectedIndex, displayOptions);
                if (newIndex != selectedIndex)
                {
                    string selected = displayOptions[newIndex];
                    if (selected == "(none)")
                        elementProperty.stringValue = string.Empty;
                    else if (!selected.StartsWith("\u26a0 "))
                        elementProperty.stringValue = selected;
                }

                GUI.color = prevColor;

                // Sprite preview
                Sprite sprite = GetSpriteForResourceId(currentValue);
                if (sprite != null)
                    GUI.DrawTexture(spriteRect, sprite.texture, ScaleMode.ScaleToFit);
            }

            private string[] BuildDisplayOptions(string[] ids, string currentValue, bool isValid, out int selectedIndex)
            {
                var list = new List<string> { "(none)" };

                for (int i = 0; i < ids.Length; i++)
                {
                    string rid = ids[i];
                    string displayName = GetResourceDisplayName(rid);
                    list.Add($"{rid} — {displayName}");
                }

                if (!isValid && !string.IsNullOrEmpty(currentValue))
                {
                    list.Insert(0, $"\u26a0 {currentValue}");
                    selectedIndex = 0;
                }
                else
                {
                    if (string.IsNullOrEmpty(currentValue))
                        selectedIndex = 0;
                    else
                    {
                        selectedIndex = -1;
                        for (int i = 0; i < ids.Length; i++)
                        {
                            if (ids[i] == currentValue)
                            {
                                selectedIndex = i + 1;
                                break;
                            }
                        }
                        if (selectedIndex < 0) selectedIndex = 0;
                    }
                }

                return list.ToArray();
            }

            private string[] GetResourceIds()
            {
                var db = FindDatabase();
                if (db == null)
                    return System.Array.Empty<string>();

                var ids = new List<string>();
                foreach (var resource in db.Resources)
                {
                    if (!string.IsNullOrEmpty(resource.Id))
                        ids.Add(resource.Id);
                }
                ids.Sort();
                return ids.ToArray();
            }

            private Sprite GetSpriteForResourceId(string id)
            {
                if (string.IsNullOrEmpty(id))
                    return null;

                var db = FindDatabase();
                if (db == null)
                    return null;

                foreach (var resource in db.Resources)
                {
                    if (resource.Id == id)
                        return resource.Icon;
                }

                return null;
            }

            private string GetResourceDisplayName(string id)
            {
                if (string.IsNullOrEmpty(id))
                    return id;

                var db = FindDatabase();
                if (db == null)
                    return id;

                foreach (var resource in db.Resources)
                {
                    if (resource.Id == id)
                        return resource.DisplayName ?? id;
                }

                return id;
            }

            private EconomyDatabaseSO FindDatabase()
            {
                string[] guids = AssetDatabase.FindAssets("t:EconomyDatabaseSO");
                if (guids.Length == 0)
                    return null;

                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<EconomyDatabaseSO>(path);
            }
        }
    }
}
