using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    internal static class RegistryEditorStyles
    {
        // ── Кольори ──────────────────────────────────────────
        public static readonly Color Accent      = new(0.22f, 0.70f, 0.67f);
        public static readonly Color AccentDark  = new(0.14f, 0.45f, 0.44f);
        public static readonly Color ErrorCol    = new(0.94f, 0.27f, 0.27f);
        public static readonly Color WarningCol  = new(0.96f, 0.62f, 0.04f);
        public static readonly Color SuccessCol  = new(0.06f, 0.73f, 0.51f);

        public static Color CardBg    => EditorGUIUtility.isProSkin ? new Color(0.24f, 0.24f, 0.24f) : new Color(0.86f, 0.86f, 0.86f);
        public static Color CardBgAlt => EditorGUIUtility.isProSkin ? new Color(0.21f, 0.21f, 0.21f) : new Color(0.89f, 0.89f, 0.89f);
        public static Color HeaderBg  => EditorGUIUtility.isProSkin ? new Color(0.16f, 0.19f, 0.24f) : new Color(0.30f, 0.38f, 0.50f);
        public static Color SidebarBg => EditorGUIUtility.isProSkin ? new Color(0.17f, 0.17f, 0.17f) : new Color(0.78f, 0.78f, 0.78f);
        public static Color CreateBg  => EditorGUIUtility.isProSkin ? new Color(0.19f, 0.22f, 0.19f) : new Color(0.82f, 0.88f, 0.82f);

        // ── Текстури (лінивий кеш) ──────────────────────────
        private static Texture2D _accentTex, _headerTex, _cardTex, _cardAltTex, _sidebarTex, _createBgTex;

        public static Texture2D AccentTex   => Ensure(ref _accentTex,   Accent);
        public static Texture2D HeaderTex   => Ensure(ref _headerTex,   HeaderBg);
        public static Texture2D CardTex     => Ensure(ref _cardTex,     CardBg);
        public static Texture2D CardAltTex  => Ensure(ref _cardAltTex,  CardBgAlt);
        public static Texture2D SidebarTex  => Ensure(ref _sidebarTex,  SidebarBg);
        public static Texture2D CreateBgTex => Ensure(ref _createBgTex, CreateBg);

        // ── Стилі (лінивий кеш) ─────────────────────────────
        private static GUIStyle _header, _subHeader, _card, _cardAlt;
        private static GUIStyle _entryTitle, _entryDetail, _richLabel;
        private static GUIStyle _sidebarTab, _sidebarTabActive;
        private static GUIStyle _createBtn, _dangerBtn;
        private static GUIStyle _sectionBox, _centeredMini;

        public static GUIStyle Header => _header ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize     = 16,
            fontStyle    = FontStyle.Bold,
            alignment    = TextAnchor.MiddleLeft,
            padding      = new RectOffset(12, 12, 6, 6),
            margin       = new RectOffset(0, 0, 0, 2),
            normal       = { textColor = Color.white, background = HeaderTex },
        };

        public static GUIStyle SubHeader => _subHeader ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 13,
            fontStyle = FontStyle.Bold,
            padding   = new RectOffset(6, 6, 4, 4),
            normal    = { textColor = Accent },
        };

        public static GUIStyle Card => _card ??= new GUIStyle("box")
        {
            padding = new RectOffset(10, 10, 6, 6),
            margin  = new RectOffset(2, 2, 1, 1),
            normal  = { background = CardTex },
        };

        public static GUIStyle CardAlt => _cardAlt ??= new GUIStyle("box")
        {
            padding = new RectOffset(10, 10, 6, 6),
            margin  = new RectOffset(2, 2, 1, 1),
            normal  = { background = CardAltTex },
        };

        public static GUIStyle EntryTitle => _entryTitle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            richText = true,
        };

        public static GUIStyle EntryDetail => _entryDetail ??= new GUIStyle(EditorStyles.miniLabel)
        {
            richText = true,
            wordWrap = true,
        };

        public static GUIStyle RichLabel => _richLabel ??= new GUIStyle(EditorStyles.label)
        {
            richText = true,
            wordWrap = true,
        };

        public static GUIStyle SidebarTab => _sidebarTab ??= new GUIStyle(GUI.skin.button)
        {
            alignment   = TextAnchor.MiddleLeft,
            padding     = new RectOffset(14, 10, 6, 6),
            fontSize    = 12,
            fixedHeight = 34,
            normal      = { textColor = new Color(0.7f, 0.7f, 0.7f) },
        };

        public static GUIStyle SidebarTabActive => _sidebarTabActive ??= new GUIStyle(SidebarTab)
        {
            fontStyle = FontStyle.Bold,
            normal    = { textColor = Color.white, background = AccentTex },
        };

        public static GUIStyle CreateButton => _createBtn ??= new GUIStyle(GUI.skin.button)
        {
            fontStyle   = FontStyle.Bold,
            fontSize    = 12,
            fixedHeight = 28,
            normal      = { textColor = Color.white },
        };

        public static GUIStyle DangerButton => _dangerBtn ??= new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            normal    = { textColor = ErrorCol },
        };

        public static GUIStyle SectionBox => _sectionBox ??= new GUIStyle("box")
        {
            padding = new RectOffset(10, 10, 8, 8),
            margin  = new RectOffset(0, 0, 4, 4),
            normal  = { background = CreateBgTex },
        };

        public static GUIStyle CenteredMini => _centeredMini ??= new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            fontSize = 11,
        };

        // ── Утиліти для малювання ────────────────────────────

        public static void DrawColoredHeader(string text, Color color)
        {
            Rect r = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(r, color * 0.85f);
            var s = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 14,
                alignment = TextAnchor.MiddleLeft,
                padding   = new RectOffset(10, 0, 0, 0),
                normal    = { textColor = Color.white },
            };
            GUI.Label(r, text, s);
        }

        public static void DrawSeparator()
        {
            EditorGUILayout.Space(2);
            Rect r = EditorGUILayout.GetControlRect(false, 1);
            r.x      += 4;
            r.width  -= 8;
            EditorGUI.DrawRect(r, new Color(0.35f, 0.35f, 0.35f));
            EditorGUILayout.Space(2);
        }

        /// <summary>Валідація ID. Повертає текст помилки або null якщо OK.</summary>
        public static string ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "ID не може бути порожнім.";
            if (id.Contains('_'))
                return "Символ '_' заборонений в ID (зарезервований для генерації та інших систем). Використовуйте '-' або camelCase.";
            return null;
        }

        /// <summary>Повна валідація: формат + перевірка дублікатів у масиві.</summary>
        public static string ValidateIdFull(string id, SerializedProperty arr, string idProp)
        {
            string err = ValidateId(id);
            if (err != null) return err;

            if (arr != null && !string.IsNullOrWhiteSpace(id))
            {
                for (int i = 0; i < arr.arraySize; i++)
                {
                    string existing = arr.GetArrayElementAtIndex(i).FindPropertyRelative(idProp)?.stringValue;
                    if (string.Equals(existing, id.Trim(), System.StringComparison.OrdinalIgnoreCase))
                        return $"ID '{id.Trim()}' вже використовується. Оберіть інше ім'я.";
                }
            }

            return null;
        }

        public static void DrawIdValidation(string id)
        {
            string err = ValidateId(id);
            if (err != null)
            {
                Color prev = GUI.color;
                GUI.color = ErrorCol;
                EditorGUILayout.HelpBox(err, MessageType.Error);
                GUI.color = prev;
            }
        }

        public static void DrawFullIdValidation(string id, SerializedProperty arr, string idProp)
        {
            string err = ValidateIdFull(id, arr, idProp);
            if (err == null) return;

            Color prev = GUI.color;
            bool isDuplicate = err.Contains("використовується");
            GUI.color = isDuplicate ? WarningCol : ErrorCol;
            EditorGUILayout.HelpBox(err, isDuplicate ? MessageType.Warning : MessageType.Error);
            GUI.color = prev;
        }

        public static string IdField(string label, string current)
        {
            string val = EditorGUILayout.TextField(label, current);
            DrawIdValidation(val);
            return val;
        }

        /// <summary>ID поле з перевіркою дублікатів у реальному часі.</summary>
        public static string IdFieldWithDuplicateCheck(string label, string current, SerializedProperty arr, string idProp)
        {
            string val = EditorGUILayout.TextField(label, current);
            DrawFullIdValidation(val, arr, idProp);
            return val;
        }

        public static void DrawBadge(string text, Color color)
        {
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUILayout.Label(text, EditorStyles.miniButton, GUILayout.ExpandWidth(false));
            GUI.backgroundColor = prev;
        }

        // ── Внутрішні хелпери ────────────────────────────────

        private static Texture2D Ensure(ref Texture2D tex, Color color)
        {
            if (tex == null) tex = MakeTex(color);
            return tex;
        }

        private static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }
    }
}
