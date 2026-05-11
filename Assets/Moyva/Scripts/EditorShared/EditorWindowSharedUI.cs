using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    public static class EditorWindowSharedUI
    {
        private static GUIStyle _badgeStyle;
        private static GUIStyle _rowNormalStyle;
        private static GUIStyle _rowAltStyle;
        private static GUIStyle _rowSelectedStyle;
        private static bool _rowStylesForProSkin;

        public static GUIStyle PanelStyle(RectOffset padding = null, RectOffset margin = null)
        {
            return new GUIStyle(EditorStyles.helpBox)
            {
                padding = padding ?? new RectOffset(8, 8, 8, 8),
                margin = margin ?? new RectOffset(2, 2, 4, 4),
            };
        }

        public static void DrawWarning(string message, MessageType messageType = MessageType.Warning)
        {
            EditorGUILayout.HelpBox(message, messageType);
        }

        public static void DrawBadgeRect(Rect rect, string text, Color background, Color? textColor = null)
        {
            EditorGUI.DrawRect(rect, background);
            GUIStyle style = _badgeStyle ??= new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
            };

            Color previous = style.normal.textColor;
            style.normal.textColor = textColor ?? Color.white;
            GUI.Label(rect, text, style);
            style.normal.textColor = previous;
        }

        public static GUIStyle ListRowStyle(bool selected, bool alternate)
        {
            EnsureRowStyles();
            if (selected)
                return _rowSelectedStyle;

            return alternate ? _rowAltStyle : _rowNormalStyle;
        }

        private static void EnsureRowStyles()
        {
            bool isPro = EditorGUIUtility.isProSkin;
            if (_rowNormalStyle != null && _rowStylesForProSkin == isPro)
                return;

            _rowStylesForProSkin = isPro;
            _rowNormalStyle = CreateRowStyle(isPro ? new Color(0.24f, 0.24f, 0.24f) : new Color(0.87f, 0.87f, 0.87f));
            _rowAltStyle = CreateRowStyle(isPro ? new Color(0.21f, 0.21f, 0.21f) : new Color(0.90f, 0.90f, 0.90f));
            _rowSelectedStyle = CreateRowStyle(isPro ? new Color(0.20f, 0.38f, 0.41f) : new Color(0.66f, 0.83f, 0.86f));
        }

        private static GUIStyle CreateRowStyle(Color background)
        {
            return new GUIStyle("box")
            {
                padding = new RectOffset(10, 10, 6, 6),
                margin = new RectOffset(2, 2, 1, 1),
                normal = { background = MakeTex(background) },
            };
        }

        private static Texture2D MakeTex(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
