using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(HillGeneratorNode))]
    public sealed class HillGeneratorNodeEditor : ContourGeneratorNodeEditorBase
    {
        protected override string TileArrayPropertyName => "_hillTiles";
        protected override string CenterCellLabel       => "▲\nhill";

        protected override HashSet<string> GetExtraSkippedProperties() => new HashSet<string>
        {
            "_useCustomThresholds",
            "_levelThresholds"
        };

        // ── Hill-specific state ──

        private bool _thresholdsFoldout = true;
        private bool _previewFoldout    = true;

        private Texture2D _previewTexture;
        private int       _previewLevels = -1;
        private float     _previewFullHash;

        private GUIStyle _centeredWhiteMiniLocal;
        private GUIStyle CenteredWhiteMiniLocal => _centeredWhiteMiniLocal ??=
            new GUIStyle(EditorStyles.centeredGreyMiniLabel) { normal = { textColor = Color.white } };

        protected override void OnDisable()
        {
            base.OnDisable();
            DestroyPreviewTexture();
            _centeredWhiteMiniLocal = null;
        }

        protected override void DrawMiddleSections()
        {
            DrawThresholdEditor();
            EditorGUILayout.Space(8);
            DrawLevelPreview();
            EditorGUILayout.Space(8);
        }

        // ── Level preview ──

        private void DrawLevelPreview()
        {
            _previewFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(
                _previewFoldout, "Превью рівнів висоти");

            if (_previewFoldout)
            {
                EditorGUILayout.Space(4);

                int   levels    = serializedObject.FindProperty("_levels")?.intValue ?? 3;
                bool  useCustom = serializedObject.FindProperty("_useCustomThresholds")?.boolValue ?? false;
                var   threshProp = serializedObject.FindProperty("_levelThresholds");
                var   exclProp   = serializedObject.FindProperty("_excludedNeighborTileTypes");
                int   minZone    = serializedObject.FindProperty("_minZoneSize")?.intValue ?? 0;

                float fullHash = ComputeThresholdsHash(threshProp, levels)
                               + minZone * 997f
                               + ComputeStringArrayHash(exclProp)
                               + (useCustom ? 10000f : 0f);

                if (_previewTexture == null || _previewLevels != levels || _previewFullHash != fullHash)
                {
                    DestroyPreviewTexture();
                    float[] thresholds = ReadThresholds(threshProp, levels);
                    _previewTexture  = BuildLevelPreviewTexture(levels, useCustom ? thresholds : null);
                    _previewLevels   = levels;
                    _previewFullHash = fullHash;
                    Repaint();
                }

                const float previewHeight = 48f;
                float previewWidth = EditorGUIUtility.currentViewWidth - 32f;
                var texRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);
                if (_previewTexture != null)
                    GUI.DrawTexture(texRect, _previewTexture, UnityEngine.ScaleMode.StretchToFill, false);

                EditorGUILayout.Space(4);
                using (new EditorGUILayout.HorizontalScope())
                {
                    float swatchW = Mathf.Max(14f, (previewWidth - (levels - 1) * 4f) / levels);
                    for (int i = 0; i < levels; i++)
                    {
                        var color = LevelColor(i, levels);
                        var swatchRect = GUILayoutUtility.GetRect(swatchW, 16f, GUILayout.Width(swatchW));
                        EditorGUI.DrawRect(swatchRect, color);
                        GUI.Label(swatchRect, i.ToString(), CenteredWhiteMiniLocal);

                        if (i < levels - 1) GUILayout.Space(4f);
                    }
                }

                EditorGUILayout.Space(2);
                EditorGUILayout.HelpBox(
                    "Рівень 0 — найнижча зона (немає схилів під нею). " +
                    "Краї пагорба генеруються між сусідніми рівнями. " +
                    "Наприклад, тайл на рівні 1 з сусідом рівня 0 знизу → South.",
                    MessageType.None);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // ── Threshold editor ──

        private void DrawThresholdEditor()
        {
            var useCustomProp = serializedObject.FindProperty("_useCustomThresholds");
            var threshProp    = serializedObject.FindProperty("_levelThresholds");
            int levels        = serializedObject.FindProperty("_levels")?.intValue ?? 3;
            int needed        = levels - 1;

            _thresholdsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(
                _thresholdsFoldout, "Межі рівнів висоти");

            if (_thresholdsFoldout)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.PropertyField(useCustomProp,
                    new GUIContent("Ручне задання",
                                   "Увімкнути — задаєте межі вручну. Вимкнути — рівномірний поділ [0,1]."));

                if (useCustomProp.boolValue)
                {
                    if (threshProp.arraySize != needed)
                    {
                        float[] cur = ReadThresholds(threshProp, threshProp.arraySize);
                        threshProp.arraySize = needed;
                        for (int i = 0; i < needed; i++)
                        {
                            float def = (float)(i + 1) / levels;
                            threshProp.GetArrayElementAtIndex(i).floatValue =
                                i < cur.Length ? cur[i] : def;
                        }
                    }

                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField("Пороги (h ≥ поріг → рівень вищий)", EditorStyles.boldLabel);

                    for (int i = 0; i < needed; i++)
                    {
                        var   elem = threshProp.GetArrayElementAtIndex(i);
                        float min  = i > 0
                            ? threshProp.GetArrayElementAtIndex(i - 1).floatValue + 0.01f
                            : 0.01f;
                        float max  = i < needed - 1
                            ? threshProp.GetArrayElementAtIndex(i + 1).floatValue - 0.01f
                            : 0.99f;

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var swatch1 = GUILayoutUtility.GetRect(14f, EditorGUIUtility.singleLineHeight,
                                GUILayout.Width(14f));
                            EditorGUI.DrawRect(swatch1, LevelColor(i, levels));

                            EditorGUILayout.LabelField($"Рівні {i}→{i + 1}", GUILayout.Width(70f));

                            EditorGUI.BeginChangeCheck();
                            float val = EditorGUILayout.Slider(elem.floatValue, min, max);
                            if (EditorGUI.EndChangeCheck())
                                elem.floatValue = val;

                            var swatch2 = GUILayoutUtility.GetRect(14f, EditorGUIUtility.singleLineHeight,
                                GUILayout.Width(14f));
                            EditorGUI.DrawRect(swatch2, LevelColor(i + 1, levels));
                        }
                    }

                    if (GUILayout.Button("Розподілити рівномірно", GUILayout.Width(160f)))
                        for (int i = 0; i < needed; i++)
                            threshProp.GetArrayElementAtIndex(i).floatValue = (float)(i + 1) / levels;
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Вимкнено — діапазон [0,1] ділиться рівномірно на _levels частин.",
                        MessageType.None);
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // ── Preview helpers ──

        private static Texture2D BuildLevelPreviewTexture(int levels, float[] thresholds = null)
        {
            const int texW = 256;
            const int texH = 32;
            var tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Clamp
            };

            for (int x = 0; x < texW; x++)
            {
                float h = (float)x / (texW - 1);
                int   lvl;

                if (thresholds != null && thresholds.Length >= levels - 1)
                {
                    lvl = 0;
                    for (int i = 0; i < levels - 1; i++)
                        if (h >= thresholds[i]) lvl = i + 1;
                }
                else
                {
                    lvl = Mathf.Clamp(Mathf.FloorToInt(h * levels), 0, levels - 1);
                }

                var col = HillGeneratorNode.LevelIndexToColor(lvl, levels);
                for (int y = 0; y < texH; y++)
                    tex.SetPixel(x, y, col);
            }

            tex.Apply();
            return tex;
        }

        private static float[] ReadThresholds(SerializedProperty prop, int count)
        {
            if (prop == null) return System.Array.Empty<float>();
            var arr = new float[Mathf.Min(count, prop.arraySize)];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = prop.GetArrayElementAtIndex(i).floatValue;
            return arr;
        }

        private static float ComputeThresholdsHash(SerializedProperty prop, int levels)
        {
            if (prop == null) return levels;
            float h = levels * 100f;
            for (int i = 0; i < prop.arraySize; i++)
                h += prop.GetArrayElementAtIndex(i).floatValue * (i + 1);
            return h;
        }

        private static float ComputeStringArrayHash(SerializedProperty prop)
        {
            if (prop == null) return 0f;
            float h = prop.arraySize * 13f;
            for (int i = 0; i < prop.arraySize; i++)
            {
                var s = prop.GetArrayElementAtIndex(i).stringValue;
                h += (s?.GetHashCode() ?? 0) * (i + 1) * 0.001f;
            }
            return h;
        }

        private static Color LevelColor(int level, int totalLevels) =>
            HillGeneratorNode.LevelIndexToColor(level, totalLevels);

        private void DestroyPreviewTexture()
        {
            if (_previewTexture == null) return;
            Object.DestroyImmediate(_previewTexture);
            _previewTexture = null;
        }
    }
}
