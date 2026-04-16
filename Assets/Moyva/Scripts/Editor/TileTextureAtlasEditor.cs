using Kruty1918.Moyva.Visuals;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    [CustomEditor(typeof(TileTextureAtlasSO))]
    public sealed class TileTextureAtlasEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var atlas = (TileTextureAtlasSO)target;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Зібрати атлас", GUILayout.Height(30)))
            {
                atlas.BuildAtlas();
                EditorUtility.SetDirty(atlas);
            }

            if (atlas.Atlas != null)
            {
                EditorGUILayout.LabelField("Атлас побудований", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Розмір: {atlas.Atlas.width}×{atlas.Atlas.height}");

                var previewRect = GUILayoutUtility.GetRect(200, 200, GUILayout.ExpandWidth(true));
                EditorGUI.DrawPreviewTexture(previewRect, atlas.Atlas);
            }
        }
    }
}
