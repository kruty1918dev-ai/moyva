using Kruty1918.Moyva.Generator.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(TileHeightTableSO))]
    public class TileHeightTableSOEditor : UnityEditor.Editor
    {
        private const float SpriteSize = 48f;
        private const float RowPadding = 4f;
        private SerializedProperty _entries;

        private void OnEnable()
        {
            _entries = serializedObject.FindProperty("Entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Таблиця тайлів і висот", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            DrawTableHeader();

            for (int i = 0; i < _entries.arraySize; i++)
                DrawEntryRow(i, _entries.GetArrayElementAtIndex(i));

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Додати запис"))
            {
                _entries.InsertArrayElementAtIndex(_entries.arraySize);
                var newEntry = _entries.GetArrayElementAtIndex(_entries.arraySize - 1);
                newEntry.FindPropertyRelative("TileId").stringValue = string.Empty;
                newEntry.FindPropertyRelative("MinHeight").floatValue = 0f;
                newEntry.FindPropertyRelative("MaxHeight").floatValue = 1f;
                newEntry.FindPropertyRelative("Sprite").objectReferenceValue = null;
                newEntry.FindPropertyRelative("Description").stringValue = string.Empty;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawTableHeader()
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Спрайт", headerStyle, GUILayout.Width(SpriteSize + 8));
                GUILayout.Label("Tile ID", headerStyle, GUILayout.Width(140));
                GUILayout.Label("Min", headerStyle, GUILayout.Width(44));
                GUILayout.Label("Max", headerStyle, GUILayout.Width(44));
                GUILayout.Label("Діапазон", headerStyle, GUILayout.Width(90));
                GUILayout.Label("Опис", headerStyle);
            }
        }

        private void DrawEntryRow(int index, SerializedProperty entry)
        {
            var spriteProp = entry.FindPropertyRelative("Sprite");
            var tileIdProp = entry.FindPropertyRelative("TileId");
            var minHeightProp = entry.FindPropertyRelative("MinHeight");
            var maxHeightProp = entry.FindPropertyRelative("MaxHeight");
            var descriptionProp = entry.FindPropertyRelative("Description");

            float rowHeight = SpriteSize + RowPadding * 2;
            var rowRect = EditorGUILayout.GetControlRect(false, rowHeight);

            // Alternating row background
            if (index % 2 == 0)
                EditorGUI.DrawRect(rowRect, new Color(0f, 0f, 0f, 0.05f));

            float x = rowRect.x;
            float y = rowRect.y + RowPadding;

            // Sprite thumbnail
            var spriteRect = new Rect(x + 2, y, SpriteSize, SpriteSize);
            var sprite = spriteProp.objectReferenceValue as Sprite;
            if (sprite != null)
            {
                var tex = AssetPreview.GetAssetPreview(sprite);
                if (tex != null)
                    GUI.DrawTexture(spriteRect, tex, ScaleMode.ScaleToFit);
                else
                    EditorGUI.ObjectField(spriteRect, spriteProp, typeof(Sprite), GUIContent.none);
            }
            else
            {
                EditorGUI.ObjectField(spriteRect, spriteProp, typeof(Sprite), GUIContent.none);
            }
            x += SpriteSize + 10;

            float fieldY = y + (SpriteSize - EditorGUIUtility.singleLineHeight) * 0.5f;

            // Tile ID
            EditorGUI.PropertyField(new Rect(x, fieldY, 136, EditorGUIUtility.singleLineHeight), tileIdProp, GUIContent.none);
            x += 144;

            // Min / Max heights
            EditorGUI.PropertyField(new Rect(x, fieldY, 40, EditorGUIUtility.singleLineHeight), minHeightProp, GUIContent.none);
            x += 48;
            EditorGUI.PropertyField(new Rect(x, fieldY, 40, EditorGUIUtility.singleLineHeight), maxHeightProp, GUIContent.none);
            x += 48;

            // Height bar
            float min = minHeightProp.floatValue;
            float max = maxHeightProp.floatValue;
            float barWidth = 86f;
            var barBg = new Rect(x, fieldY, barWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.DrawRect(barBg, new Color(0.2f, 0.2f, 0.2f, 0.4f));
            var barFill = new Rect(x + min * barWidth, fieldY, (max - min) * barWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.DrawRect(barFill, GetHeightColor(min, max));
            EditorGUI.LabelField(barBg, $"{min:0.##}–{max:0.##}", new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });
            x += 96;

            // Description
            float descWidth = rowRect.xMax - x - 40;
            EditorGUI.PropertyField(new Rect(x, fieldY, descWidth, EditorGUIUtility.singleLineHeight), descriptionProp, GUIContent.none);

            // Delete button
            if (GUI.Button(new Rect(rowRect.xMax - 36, fieldY, 32, EditorGUIUtility.singleLineHeight), "✕", EditorStyles.miniButton))
                _entries.DeleteArrayElementAtIndex(index);
        }

        private static Color GetHeightColor(float min, float max)
        {
            float mid = (min + max) * 0.5f;
            if (mid < 0.2f) return new Color(0.1f, 0.2f, 0.8f); // deep water
            if (mid < 0.35f) return new Color(0.2f, 0.5f, 0.9f); // shallow water
            if (mid < 0.42f) return new Color(0.9f, 0.85f, 0.55f); // beach/sand
            if (mid < 0.56f) return new Color(0.5f, 0.75f, 0.3f); // lowland/grass
            if (mid < 0.68f) return new Color(0.35f, 0.6f, 0.25f); // forest/hill base
            if (mid < 0.80f) return new Color(0.55f, 0.45f, 0.35f); // hill
            if (mid < 0.90f) return new Color(0.45f, 0.4f, 0.4f); // mountain
            return new Color(0.85f, 0.9f, 0.95f); // snow
        }
    }
}
