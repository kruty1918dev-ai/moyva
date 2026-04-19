using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(BiomePainterNode))]
    public sealed class BiomePainterNodeEditor : UnityEditor.Editor
    {
        private const float CanvasSize = 256f;

        private SerializedProperty _paintTexture;
        private SerializedProperty _palette;
        private SerializedProperty _alphaThreshold;

        private int _selectedPaletteIndex;
        private int _brushRadius = 4;
        private bool _eraseMode;

        private void OnEnable()
        {
            _paintTexture = serializedObject.FindProperty("_paintTexture");
            _palette = serializedObject.FindProperty("_palette");
            _alphaThreshold = serializedObject.FindProperty("_alphaThreshold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_paintTexture);
            EditorGUILayout.PropertyField(_alphaThreshold);
            EditorGUILayout.PropertyField(_palette, includeChildren: true);

            var texture = _paintTexture.objectReferenceValue as Texture2D;
            if (texture == null)
            {
                EditorGUILayout.HelpBox("Assign a readable/writable texture to paint biome masks.", MessageType.Info);
                DrawCreateTextureButton();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            if (!texture.isReadable)
            {
                EditorGUILayout.HelpBox("Texture is not readable. Enable Read/Write in import settings.", MessageType.Warning);
                if (GUILayout.Button("Enable Read/Write"))
                    EnableReadWrite(texture);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            DrawPainterToolbar(texture);
            DrawPaintCanvas(texture);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCreateTextureButton()
        {
            if (!GUILayout.Button("Create New Paint Texture")) return;

            string path = EditorUtility.SaveFilePanelInProject(
                "Create Biome Paint Texture",
                "BiomePaintMask",
                "png",
                "Select destination for new mask texture");
            if (string.IsNullOrEmpty(path)) return;

            var tex = new Texture2D(256, 256, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[256 * 256];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0f, 0f, 0f, 0f);
            tex.SetPixels(pixels);
            tex.Apply();

            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var imported = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            EnableReadWrite(imported);

            _paintTexture.objectReferenceValue = imported;
            serializedObject.ApplyModifiedProperties();
            DestroyImmediate(tex);
        }

        private void DrawPainterToolbar(Texture2D texture)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Painter", EditorStyles.boldLabel);

            _brushRadius = EditorGUILayout.IntSlider("Brush Radius", _brushRadius, 1, 32);
            _eraseMode = EditorGUILayout.Toggle("Erase", _eraseMode);

            var options = BuildPaletteOptions();
            if (options.Count > 0)
                _selectedPaletteIndex = Mathf.Clamp(EditorGUILayout.Popup("Palette", _selectedPaletteIndex, options.ToArray()), 0, options.Count - 1);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Clear"))
                    ClearTexture(texture);

                if (GUILayout.Button("Save"))
                    SaveTexture(texture);
            }
        }

        private List<string> BuildPaletteOptions()
        {
            var list = new List<string>();
            for (int i = 0; i < _palette.arraySize; i++)
            {
                var e = _palette.GetArrayElementAtIndex(i);
                var tile = e.FindPropertyRelative("TileId")?.stringValue;
                if (string.IsNullOrEmpty(tile)) tile = "(empty)";
                list.Add(tile);
            }
            return list;
        }

        private void DrawPaintCanvas(Texture2D texture)
        {
            Rect rect = GUILayoutUtility.GetRect(CanvasSize, CanvasSize, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(rect, texture, null, UnityEngine.ScaleMode.ScaleToFit);

            var evt = Event.current;
            if (evt == null) return;
            if (!rect.Contains(evt.mousePosition)) return;

            if ((evt.type == EventType.MouseDrag || evt.type == EventType.MouseDown) && evt.button == 0)
            {
                PaintAt(texture, rect, evt.mousePosition);
                evt.Use();
            }
        }

        private void PaintAt(Texture2D texture, Rect canvas, Vector2 mouse)
        {
            float u = Mathf.Clamp01((mouse.x - canvas.x) / canvas.width);
            float v = 1f - Mathf.Clamp01((mouse.y - canvas.y) / canvas.height);
            int px = Mathf.RoundToInt(u * (texture.width - 1));
            int py = Mathf.RoundToInt(v * (texture.height - 1));

            Color color = _eraseMode ? new Color(0, 0, 0, 0) : GetSelectedPaletteColor();
            int r = _brushRadius;

            for (int ox = -r; ox <= r; ox++)
            {
                for (int oy = -r; oy <= r; oy++)
                {
                    if (ox * ox + oy * oy > r * r) continue;
                    int x = px + ox;
                    int y = py + oy;
                    if (x < 0 || y < 0 || x >= texture.width || y >= texture.height) continue;
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply(false, false);
            EditorUtility.SetDirty(texture);
            Repaint();
        }

        private Color GetSelectedPaletteColor()
        {
            if (_palette.arraySize == 0) return new Color(1f, 0f, 0f, 1f);
            _selectedPaletteIndex = Mathf.Clamp(_selectedPaletteIndex, 0, _palette.arraySize - 1);
            var e = _palette.GetArrayElementAtIndex(_selectedPaletteIndex);
            return e.FindPropertyRelative("Color")?.colorValue ?? new Color(1f, 0f, 0f, 1f);
        }

        private static void SaveTexture(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return;

            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        private static void ClearTexture(Texture2D texture)
        {
            var pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color(0f, 0f, 0f, 0f);

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            EditorUtility.SetDirty(texture);
        }

        private static void EnableReadWrite(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return;

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
    }
}
