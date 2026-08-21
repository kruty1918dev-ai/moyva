using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal static class BotAnalyzerSceneStyles
    {
        public static readonly Color VisibleFog = new(0.18f, 0.72f, 1.00f, 1f);
        public static readonly Color ExploredFog = new(0.48f, 0.58f, 0.68f, 1f);
        public static readonly Color OwnUnit = new(0.20f, 1.00f, 0.45f, 1f);
        public static readonly Color OwnBuilding = new(1.00f, 0.74f, 0.18f, 1f);
        public static readonly Color Enemy = new(1.00f, 0.28f, 0.24f, 1f);
        public static readonly Color Movement = new(0.22f, 0.88f, 1.00f, 1f);
        public static readonly Color Goal = new(1.00f, 0.35f, 0.95f, 1f);
        public static readonly Color Candidate = new(0.78f, 0.48f, 1.00f, 1f);
        public static readonly Color Action = new(1.00f, 0.92f, 0.25f, 1f);

        private static GUIStyle _label;
        private static GUIStyle _smallLabel;
        private static GUIStyle _goalLabel;

        public static GUIStyle Label => _label ??= CreateLabel(12, FontStyle.Bold);
        public static GUIStyle SmallLabel => _smallLabel ??= CreateLabel(10, FontStyle.Normal);
        public static GUIStyle GoalLabel => _goalLabel ??= CreateLabel(12, FontStyle.Bold);

        public static Color WithAlpha(Color color, float alpha)
            => new(color.r, color.g, color.b, Mathf.Clamp01(alpha));

        private static GUIStyle CreateLabel(int size, FontStyle style)
        {
            var result = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = size,
                fontStyle = style,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Overflow,
            };
            result.normal.textColor = Color.white;
            return result;
        }
    }
}
