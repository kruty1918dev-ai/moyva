using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(HeightMapSettings))]
    public class HeightMapSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _layersProp;
        private bool[] _layerFoldouts = Array.Empty<bool>();

        private static readonly Color[] _segmentColors =
        {
            new Color(0.26f, 0.52f, 0.90f), // blue  – main
            new Color(0.20f, 0.73f, 0.42f), // green
            new Color(0.94f, 0.76f, 0.20f), // yellow
            new Color(0.90f, 0.38f, 0.28f), // red
            new Color(0.67f, 0.37f, 0.87f), // purple
            new Color(0.25f, 0.82f, 0.85f), // cyan
            new Color(0.95f, 0.56f, 0.20f), // orange
            new Color(0.85f, 0.27f, 0.62f), // pink
        };

        private void OnEnable()
        {
            _layersProp = serializedObject.FindProperty("HeightLayers");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawValidationWarnings((HeightMapSettings)target);

            // Sync foldout array
            if (_layerFoldouts.Length != _layersProp.arraySize)
            {
                var old = _layerFoldouts;
                _layerFoldouts = new bool[_layersProp.arraySize];
                Array.Copy(old, _layerFoldouts, Math.Min(old.Length, _layerFoldouts.Length));
            }

            // Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Height Layers", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ Add Layer", GUILayout.Width(88)))
            {
                _layersProp.InsertArrayElementAtIndex(_layersProp.arraySize);
                var elem = _layersProp.GetArrayElementAtIndex(_layersProp.arraySize - 1);
                elem.FindPropertyRelative("TileID").stringValue = string.Empty;
                elem.FindPropertyRelative("TileIDChance").floatValue = 1f;
                elem.FindPropertyRelative("WeightedVariants").ClearArray();
                var legacy = elem.FindPropertyRelative("VariantTileIDs");
                if (legacy != null) legacy.ClearArray();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);

            for (int i = 0; i < _layersProp.arraySize; i++)
            {
                if (!DrawLayer(i))
                    break; // layer was deleted
            }

            serializedObject.ApplyModifiedProperties();
        }

        // Returns false when the layer was deleted (caller must break the loop)
        private bool DrawLayer(int index)
        {
            var layerProp = _layersProp.GetArrayElementAtIndex(index);
            var minProp   = layerProp.FindPropertyRelative("MinHeight");
            var maxProp   = layerProp.FindPropertyRelative("MaxHeight");

            EditorGUILayout.BeginVertical(GUI.skin.box);

            // ── Foldout header ──────────────────────────────────────────
            EditorGUILayout.BeginHorizontal();
            _layerFoldouts[index] = EditorGUILayout.Foldout(
                _layerFoldouts[index],
                $"Layer {index}   [{minProp.floatValue:F2} – {maxProp.floatValue:F2}]",
                true, EditorStyles.foldoutHeader);
            GUILayout.FlexibleSpace();
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
            if (GUILayout.Button("✕", GUILayout.Width(24)))
            {
                GUI.backgroundColor = prevBg;
                _layersProp.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return false;
            }
            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndHorizontal();

            if (_layerFoldouts[index])
            {
                int savedIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = savedIndent + 1;

                EditorGUILayout.PropertyField(minProp, new GUIContent("Min Height"));
                EditorGUILayout.PropertyField(maxProp, new GUIContent("Max Height"));

                EditorGUI.indentLevel = savedIndent;
                EditorGUILayout.Space(6);

                DrawProbabilitySection(layerProp);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
            return true;
        }

        private void DrawProbabilitySection(SerializedProperty layerProp)
        {
            var tileIDProp     = layerProp.FindPropertyRelative("TileID");
            var tileChanceProp = layerProp.FindPropertyRelative("TileIDChance");
            var variantsProp   = layerProp.FindPropertyRelative("WeightedVariants");

            // ── Compute totals ──────────────────────────────────────────
            float mainChance  = Mathf.Clamp01(tileChanceProp.floatValue);
            float variantsSum = 0f;
            for (int i = 0; i < variantsProp.arraySize; i++)
                variantsSum += Mathf.Clamp01(variantsProp.GetArrayElementAtIndex(i)
                                                          .FindPropertyRelative("Chance").floatValue);
            float total = mainChance + variantsSum;

            // ── Header ──────────────────────────────────────────────────
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel = 1;
            EditorGUILayout.LabelField("Probability Distribution", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel = 0;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("🎲", "Випадковий розподіл: основний тайл отримує більший шанс, варіанти — рівнозначно ділять решту"), GUILayout.Width(26f), GUILayout.Height(18f)))
                RandomizeChances(tileChanceProp, variantsProp);
            EditorGUILayout.EndHorizontal();

            // ── Stacked bar ─────────────────────────────────────────────
            DrawStackedBar(mainChance, variantsProp);
            EditorGUILayout.Space(3);

            // ── Tile rows ───────────────────────────────────────────────
            // Main tile
            {
                float maxMain = Mathf.Clamp01(1f - variantsSum);
                DrawTileEntry("Main", tileIDProp, tileChanceProp, maxMain, 0, null);
            }

            // Variant rows
            bool variantDeleted = false;
            for (int i = 0; i < variantsProp.arraySize; i++)
            {
                var entryProp   = variantsProp.GetArrayElementAtIndex(i);
                var eIDProp     = entryProp.FindPropertyRelative("TileID");
                var eChanceProp = entryProp.FindPropertyRelative("Chance");

                float ec         = Mathf.Clamp01(eChanceProp.floatValue);
                float sumWithout = total - ec;
                float maxEntry   = Mathf.Clamp01(1f - sumWithout);

                int capturedIdx = i;
                DrawTileEntry($"V.{i}", eIDProp, eChanceProp, maxEntry, i + 1, () =>
                {
                    variantsProp.DeleteArrayElementAtIndex(capturedIdx);
                    variantDeleted = true;
                });

                if (variantDeleted) break;
            }

            EditorGUILayout.Space(2);

            // ── Footer: add button + remaining indicator ────────────────
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Variant", GUILayout.Width(80)))
            {
                int ni = variantsProp.arraySize;
                variantsProp.InsertArrayElementAtIndex(ni);
                var ne = variantsProp.GetArrayElementAtIndex(ni);
                ne.FindPropertyRelative("TileID").stringValue = string.Empty;
                ne.FindPropertyRelative("Chance").floatValue  = 0f;
            }
            GUILayout.FlexibleSpace();

            float remaining = 1f - total;
            if (remaining > 0.005f)
            {
                EditorGUILayout.LabelField(
                    $"Залишок: {remaining:F2}",
                    EditorStyles.miniLabel, GUILayout.Width(90));
            }
            else if (remaining < -0.005f)
            {
                Color prev = GUI.color;
                GUI.color = new Color(1f, 0.4f, 0.4f);
                EditorGUILayout.LabelField(
                    $"Перевищення: {-remaining:F2}",
                    EditorStyles.miniLabel, GUILayout.Width(115));
                GUI.color = prev;
            }
            else
            {
                EditorGUILayout.LabelField("✓  1.00", EditorStyles.miniLabel, GUILayout.Width(55));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = savedIndent;
        }

        // ── Single tile entry (2 rows: TileID row + Slider row) ────────
        private void DrawTileEntry(
            string label,
            SerializedProperty tileIDProp,
            SerializedProperty chanceProp,
            float maxChance,
            int colorIdx,
            Action onRemove)
        {
            float chance = Mathf.Clamp01(chanceProp.floatValue);
            Color dotColor = _segmentColors[colorIdx % _segmentColors.Length];

            // Row 1: color dot + label + TileID field (auto-height for warnings)
            EditorGUILayout.BeginHorizontal();

            // Color dot (small square)
            Rect dotRect = GUILayoutUtility.GetRect(12f, EditorGUIUtility.singleLineHeight, GUILayout.Width(12f));
            EditorGUI.DrawRect(new Rect(dotRect.x, dotRect.y + 3f, 10f, 10f), dotColor);

            EditorGUILayout.LabelField(label, GUILayout.Width(42f));
            EditorGUILayout.PropertyField(tileIDProp, GUIContent.none, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            // Row 2: indent + constrained slider + [remove]
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(58f); // align with TileID field

            float newChance = EditorGUILayout.Slider(chance, 0f, maxChance);
            // Clamp in case user typed a value directly in the text field
            newChance = Mathf.Clamp(newChance, 0f, maxChance);
            if (!Mathf.Approximately(newChance, chance))
                chanceProp.floatValue = newChance;

            if (onRemove != null)
            {
                Color prevBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
                if (GUILayout.Button("✕", GUILayout.Width(24f)))
                    onRemove();
                GUI.backgroundColor = prevBg;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2f);
        }

        // ── Stacked probability bar ────────────────────────────────────
        private void DrawStackedBar(float mainChance, SerializedProperty variantsProp)
        {
            Rect fullRect = EditorGUILayout.GetControlRect(false, 14f);
            // Apply a small manual indent (level 0 context here)
            fullRect.x     += 4f;
            fullRect.width -= 8f;

            // Background (unassigned probability)
            EditorGUI.DrawRect(fullRect, new Color(0.15f, 0.15f, 0.15f));

            float px = fullRect.x;

            // Main tile segment
            if (mainChance > 0.001f)
            {
                float w = mainChance * fullRect.width;
                EditorGUI.DrawRect(new Rect(px, fullRect.y, w, fullRect.height), _segmentColors[0]);
                px += w;
            }

            // Variant segments
            for (int i = 0; i < variantsProp.arraySize; i++)
            {
                float c = Mathf.Clamp01(variantsProp.GetArrayElementAtIndex(i)
                                                     .FindPropertyRelative("Chance").floatValue);
                if (c <= 0.001f) continue;
                float w = c * fullRect.width;
                EditorGUI.DrawRect(new Rect(px, fullRect.y, w, fullRect.height),
                                   _segmentColors[(i + 1) % _segmentColors.Length]);
                px += w;
            }

            // Thin border
            EditorGUI.DrawRect(new Rect(fullRect.x,     fullRect.y,               fullRect.width, 1f), new Color(0.4f, 0.4f, 0.4f));
            EditorGUI.DrawRect(new Rect(fullRect.x,     fullRect.yMax - 1f,       fullRect.width, 1f), new Color(0.4f, 0.4f, 0.4f));
            EditorGUI.DrawRect(new Rect(fullRect.x,     fullRect.y,               1f, fullRect.height), new Color(0.4f, 0.4f, 0.4f));
            EditorGUI.DrawRect(new Rect(fullRect.xMax - 1f, fullRect.y,           1f, fullRect.height), new Color(0.4f, 0.4f, 0.4f));
        }

        // ── Validation warnings ────────────────────────────────────────
        private static void DrawValidationWarnings(HeightMapSettings settings)
        {
            var knownIds = LoadKnownTileIds();
            if (knownIds.Count == 0) return;

            var unknownIds = new HashSet<string>();

            if (settings.HeightLayers != null)
            {
                foreach (var layer in settings.HeightLayers)
                {
                    if (!string.IsNullOrEmpty(layer.TileID) && !knownIds.Contains(layer.TileID))
                        unknownIds.Add(layer.TileID);

                    if (layer.WeightedVariants != null)
                    {
                        foreach (var entry in layer.WeightedVariants)
                        {
                            if (entry != null && !string.IsNullOrEmpty(entry.TileID) && !knownIds.Contains(entry.TileID))
                                unknownIds.Add(entry.TileID);
                        }
                    }

                    if (layer.VariantTileIDs == null) continue;
                    foreach (var vid in layer.VariantTileIDs)
                        if (!string.IsNullOrEmpty(vid) && !knownIds.Contains(vid))
                            unknownIds.Add(vid);
                }
            }

            if (unknownIds.Count > 0)
            {
                var sb = new StringBuilder("Невідомі Tile ID у HeightMapSettings:\n");
                foreach (var id in unknownIds)
                    sb.AppendLine($"  • {id}");
                EditorGUILayout.HelpBox(sb.ToString().TrimEnd(), MessageType.Warning);
            }
        }

        // ── Randomize chances ──────────────────────────────────────────
        // Main tile: random in [0.4, 0.7]
        // Variants: remaining split equally with small individual noise
        private static void RandomizeChances(SerializedProperty mainChanceProp, SerializedProperty variantsProp)
        {
            int varCount = 0;
            for (int i = 0; i < variantsProp.arraySize; i++)
                varCount++;

            // Main gets a bigger random slice: [0.4 .. 0.7]
            float mainChance = UnityEngine.Random.Range(0.40f, 0.70f);
            mainChance = (float)System.Math.Round(mainChance, 2);
            mainChanceProp.floatValue = mainChance;

            if (varCount == 0) return;

            float remaining = 1f - mainChance;
            float baseEach  = remaining / varCount;

            // Apply small noise ±20% of baseEach, then re-normalize to keep total = 1
            float[] raw = new float[varCount];
            for (int i = 0; i < varCount; i++)
                raw[i] = Mathf.Max(0f, baseEach + UnityEngine.Random.Range(-baseEach * 0.2f, baseEach * 0.2f));

            float rawSum = 0f;
            for (int i = 0; i < varCount; i++) rawSum += raw[i];

            for (int i = 0; i < varCount; i++)
            {
                float c = rawSum > 0f ? (float)System.Math.Round(raw[i] / rawSum * remaining, 2) : 0f;
                variantsProp.GetArrayElementAtIndex(i).FindPropertyRelative("Chance").floatValue = c;
            }

            // Fix floating-point drift: adjust last variant so total == 1
            float finalSum = mainChance;
            for (int i = 0; i < varCount; i++)
                finalSum += variantsProp.GetArrayElementAtIndex(i).FindPropertyRelative("Chance").floatValue;
            float drift = (float)System.Math.Round(1f - finalSum, 2);
            if (varCount > 0)
            {
                var lastChanceProp = variantsProp.GetArrayElementAtIndex(varCount - 1).FindPropertyRelative("Chance");
                lastChanceProp.floatValue = Mathf.Clamp01((float)System.Math.Round(lastChanceProp.floatValue + drift, 2));
            }
        }

        private static HashSet<string> _cachedKnownIds;
        private static double _knownIdsTime;

        private static HashSet<string> LoadKnownTileIds()
        {
            double now = EditorApplication.timeSinceStartup;
            if (_cachedKnownIds != null && now - _knownIdsTime < 2.0)
                return _cachedKnownIds;

            _cachedKnownIds = LoadKnownTileIdsInternal();
            _knownIdsTime = now;
            return _cachedKnownIds;
        }

        private static HashSet<string> LoadKnownTileIdsInternal()
        {
            var result = new HashSet<string>();
            string[] guids = AssetDatabase.FindAssets("t:TileRegistrySO");
            if (guids.Length == 0) return result;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var registry = AssetDatabase.LoadAssetAtPath<TileRegistrySO>(path);
            if (registry?.Definitions == null) return result;

            foreach (var def in registry.Definitions)
                if (!string.IsNullOrEmpty(def.Id))
                    result.Add(def.Id);

            return result;
        }
    }
}
