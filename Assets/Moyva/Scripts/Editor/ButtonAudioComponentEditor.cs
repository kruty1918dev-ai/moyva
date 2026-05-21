using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.Audio.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.Audio
{
    /// <summary>
    /// Кастомний інспектор для <see cref="ButtonAudioComponent"/>.
    ///
    /// Функціональність:
    /// - Показує стан кешованого реєстру звуків та кнопку для пошуку.
    /// - Кнопка-дропдавн відкриває вікно вибору звуку зі списком,
    ///   кнопками прослуховування і зупинки для кожного звуку.
    /// - Після вибору показує скорочену інформацію про звук.
    /// - Кнопка "Відкрити в Audio Designer" відкриває редактор і виділяє звук.
    /// </summary>
    [CustomEditor(typeof(ButtonAudioComponent))]
    public sealed class ButtonAudioComponentEditor : UnityEditor.Editor
    {
        private SerializedProperty _soundKeyProp;
        private SerializedProperty _cachedRegistryProp;

        private void OnEnable()
        {
            _soundKeyProp = serializedObject.FindProperty("_soundKey");
            _cachedRegistryProp = serializedObject.FindProperty("_cachedRegistry");

            // Автоматичний кеш при відкритті інспектора
            var component = (ButtonAudioComponent)target;
            if (component.CachedRegistry == null)
            {
                component.TryCacheRegistry();
                if (component.CachedRegistry != null)
                {
                    serializedObject.Update();
                    EditorUtility.SetDirty(target);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawRegistrySection();
            EditorGUILayout.Space(4f);
            DrawSoundSelectorSection();

            var registry = (AudioRegistrySO)_cachedRegistryProp.objectReferenceValue;
            var key = _soundKeyProp.stringValue;

            if (!string.IsNullOrWhiteSpace(key) && registry != null)
            {
                EditorGUILayout.Space(4f);
                DrawSoundSummarySection(registry, key);
            }

            serializedObject.ApplyModifiedProperties();
        }

        // ────────────────────────────────────────────────────────────────────────────
        //  Секція реєстру
        // ────────────────────────────────────────────────────────────────────────────

        private void DrawRegistrySection()
        {
            EditorGUILayout.LabelField("Реєстр звуків", EditorStyles.boldLabel);

            var registry = (AudioRegistrySO)_cachedRegistryProp.objectReferenceValue;

            if (registry == null)
            {
                EditorGUILayout.HelpBox(
                    "Реєстр звуків (AudioRegistrySO) не знайдено автоматично.\n" +
                    "Натисни «Знайти реєстр» або перетягни файл реєстру вручну у поле нижче.",
                    MessageType.Warning);

                if (GUILayout.Button("Знайти реєстр автоматично", GUILayout.Height(26f)))
                    TryAutoFindRegistry();
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(
                        new GUIContent("Реєстр", "Поточний AudioRegistrySO"),
                        registry, typeof(AudioRegistrySO), false);
                }
            }

            EditorGUILayout.PropertyField(
                _cachedRegistryProp,
                new GUIContent("Реєстр (вручну)", "Перетягни AudioRegistrySO сюди, якщо автопошук не допоміг."));
        }

        // ────────────────────────────────────────────────────────────────────────────
        //  Секція вибору звуку
        // ────────────────────────────────────────────────────────────────────────────

        private void DrawSoundSelectorSection()
        {
            EditorGUILayout.LabelField("Звук кнопки", EditorStyles.boldLabel);

            var registry = (AudioRegistrySO)_cachedRegistryProp.objectReferenceValue;
            var currentKey = _soundKeyProp.stringValue;

            string displayLabel = string.IsNullOrWhiteSpace(currentKey)
                ? "— не обрано —"
                : currentKey;

            using (new EditorGUILayout.HorizontalScope())
            {
                var btnRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(22f));
                if (GUI.Button(btnRect, new GUIContent($"  {displayLabel}", EditorGUIUtility.IconContent("d_Dropdown Icon").image)))
                {
                    if (registry == null)
                    {
                        EditorUtility.DisplayDialog(
                            "Реєстр не знайдено",
                            "Спочатку вкажи AudioRegistrySO у полі «Реєстр (вручну)» або натисни «Знайти реєстр автоматично».",
                            "Зрозуміло");
                    }
                    else
                    {
                        ShowSoundPickerWindow(btnRect, registry, currentKey);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(currentKey))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("✕ Очистити", "Скинути вибраний звук"),
                        GUILayout.Width(90f), GUILayout.Height(18f)))
                    {
                        _soundKeyProp.stringValue = string.Empty;
                        AudioEditorPreview.Stop();
                    }
                }
            }
        }

        // ────────────────────────────────────────────────────────────────────────────
        //  Скорочена інформація про звук
        // ────────────────────────────────────────────────────────────────────────────

        private void DrawSoundSummarySection(AudioRegistrySO registry, string key)
        {
            if (!registry.TryGet(key, out var sound))
            {
                EditorGUILayout.HelpBox(
                    $"Звук «{key}» не знайдено в реєстрі. Можливо, він був видалений або перейменований.",
                    MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Параметри звуку (скорочено)", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope(1))
            {
                DrawReadonlyRow("Ключ", sound.Key);
                DrawReadonlyRow("Кліп", sound.Clip != null ? sound.Clip.name : "— не задано —");
                DrawReadonlyRow("Шина", sound.Bus.ToString());
                DrawReadonlyRow("Гучність", $"{sound.Volume:F2}" + (sound.VolumeRandom > 0f ? $"  ±{sound.VolumeRandom:F2}" : ""));
                DrawReadonlyRow("Висота тону", $"{sound.Pitch:F2}" + (sound.PitchRandom > 0f ? $"  ±{sound.PitchRandom:F2}" : ""));
                DrawReadonlyRow("Петля (Loop)", sound.Loop ? "Так" : "Ні");
                DrawReadonlyRow("Просторовий", sound.SpatialBlend > 0.01f ? $"{sound.SpatialBlend:F2}" : "2D");
            }

            EditorGUILayout.Space(4f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("▶ Прослухати", "Відтворити звук у редакторі"), GUILayout.Height(24f)))
                    AudioEditorPreview.Play(sound);

                if (GUILayout.Button(new GUIContent("■ Зупинити", "Зупинити прослуховування"), GUILayout.Height(24f)))
                    AudioEditorPreview.Stop();

                if (GUILayout.Button(new GUIContent("Відкрити в Audio Designer →", "Відкрити повні налаштування звуку в Audio Designer"), GUILayout.Height(24f)))
                    AudioDesignerWindow.OpenAndSelect(key);
            }
        }

        // ────────────────────────────────────────────────────────────────────────────
        //  Допоміжні методи
        // ────────────────────────────────────────────────────────────────────────────

        private void TryAutoFindRegistry()
        {
            var component = (ButtonAudioComponent)target;

            // Спочатку через Resources
            var found = Resources.Load<AudioRegistrySO>("MoyvaAudioRegistry");

            // Як запасний варіант — через AssetDatabase
            if (found == null)
            {
                var guids = AssetDatabase.FindAssets("t:AudioRegistrySO");
                if (guids.Length > 0)
                    found = AssetDatabase.LoadAssetAtPath<AudioRegistrySO>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            if (found != null)
            {
                serializedObject.Update();
                _cachedRegistryProp.objectReferenceValue = found;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Реєстр не знайдено",
                    "AudioRegistrySO не знайдено ні через Resources, ні через пошук по проекту.\n\n" +
                    "Переконайся, що файл реєстру існує та знаходиться у папці Resources з назвою «MoyvaAudioRegistry».",
                    "Зрозуміло");
            }
        }

        private static void DrawReadonlyRow(string label, string value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(label);
                EditorGUILayout.SelectableLabel(value, EditorStyles.label,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        private void ShowSoundPickerWindow(Rect activatorRect, AudioRegistrySO registry, string currentKey)
        {
            var popup = new SoundPickerPopup(registry, currentKey, selectedKey =>
            {
                serializedObject.Update();
                _soundKeyProp.stringValue = selectedKey;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                Repaint();
            });

            PopupWindow.Show(activatorRect, popup);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────────
    //  Вікно вибору звуку (PopupWindowContent)
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Спливаюче вікно для вибору звуку з реєстру.
    /// Показує список звуків зі скороченою інформацією,
    /// кнопками прослуховування та зупинки.
    /// </summary>
    internal sealed class SoundPickerPopup : PopupWindowContent
    {
        private const float RowHeight = 24f;
        private const float PreviewBtnWidth = 26f;
        private const float StopBtnWidth = 26f;
        private const float PopupWidth = 360f;
        private const int MaxVisibleRows = 12;

        private readonly AudioRegistrySO _registry;
        private readonly System.Action<string> _onSelected;

        private string _currentKey;
        private string _search = string.Empty;
        private Vector2 _scroll;
        private AudioSoundDefinition[] _allSounds;
        private string[] _filteredKeys;

        public SoundPickerPopup(AudioRegistrySO registry, string currentKey, System.Action<string> onSelected)
        {
            _registry = registry;
            _currentKey = currentKey;
            _onSelected = onSelected;
            _allSounds = registry.Sounds ?? System.Array.Empty<AudioSoundDefinition>();
            RebuildFilter();
        }

        public override Vector2 GetWindowSize()
        {
            int visibleCount = Mathf.Min(_filteredKeys?.Length ?? 0, MaxVisibleRows);
            float listHeight = Mathf.Max(visibleCount, 1) * RowHeight;
            return new Vector2(PopupWidth, 28f + listHeight + 8f);
        }

        public override void OnGUI(Rect rect)
        {
            // Пошуковий рядок
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent(EditorGUIUtility.IconContent("Search Icon").image),
                    GUILayout.Width(18f), GUILayout.Height(18f));
                var newSearch = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
                if (newSearch != _search)
                {
                    _search = newSearch;
                    RebuildFilter();
                }
            }

            EditorGUILayout.Space(2f);

            if (_filteredKeys == null || _filteredKeys.Length == 0)
            {
                EditorGUILayout.LabelField("— звуків не знайдено —", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUIStyle.none, GUI.skin.verticalScrollbar);

            foreach (var key in _filteredKeys)
            {
                DrawSoundRow(key);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSoundRow(string key)
        {
            bool isSelected = key == _currentKey;

            var rowStyle = isSelected ? "selectionRect" : "box";

            using (new EditorGUILayout.HorizontalScope(rowStyle, GUILayout.Height(RowHeight)))
            {
                // Назва звуку / кнопка вибору
                var labelStyle = isSelected
                    ? new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft }
                    : new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };

                if (GUILayout.Button(key, labelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(RowHeight)))
                {
                    _currentKey = key;
                    _onSelected?.Invoke(key);
                    editorWindow?.Close();
                }

                // ▶ прослухати
                if (GUILayout.Button(new GUIContent("▶", "Прослухати звук"),
                    EditorStyles.miniButtonMid, GUILayout.Width(PreviewBtnWidth), GUILayout.Height(RowHeight - 2f)))
                {
                    if (_registry.TryGet(key, out var sound))
                        AudioEditorPreview.Play(sound);
                }

                // ■ зупинити
                if (GUILayout.Button(new GUIContent("■", "Зупинити прослуховування"),
                    EditorStyles.miniButtonRight, GUILayout.Width(StopBtnWidth), GUILayout.Height(RowHeight - 2f)))
                {
                    AudioEditorPreview.Stop();
                }
            }
        }

        public override void OnClose()
        {
            AudioEditorPreview.Stop();
        }

        private void RebuildFilter()
        {
            if (string.IsNullOrWhiteSpace(_search))
            {
                _filteredKeys = new string[_allSounds.Length];
                for (int i = 0; i < _allSounds.Length; i++)
                    _filteredKeys[i] = _allSounds[i].Key;
            }
            else
            {
                var lower = _search.ToLowerInvariant();
                var list = new System.Collections.Generic.List<string>(_allSounds.Length);
                foreach (var s in _allSounds)
                    if (!string.IsNullOrEmpty(s.Key) && s.Key.ToLowerInvariant().Contains(lower))
                        list.Add(s.Key);
                _filteredKeys = list.ToArray();
            }
        }
    }
}
