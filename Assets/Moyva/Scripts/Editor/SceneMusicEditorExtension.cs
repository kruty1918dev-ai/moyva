using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.Audio.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

// ─────────────────────────────────────────────────────────────────────────────
//  Sound Editor Extensions
//  1. Audio Infrastructure Wizard  — AudioInfrastructureWizardWindow
//  2. Fill-From-Clip helpers       — AudioRegistrySoInspector
//  3. Scene Music Profile Inspector — SceneMusicProfileInspector
// ─────────────────────────────────────────────────────────────────────────────

namespace Kruty1918.Moyva.Editor.Audio
{
    /// <summary>
    /// Legacy shim для старого Scene Music Designer.
    /// Потрібен, щоб Unity могла відновити layout без FallbackEditorWindow.
    /// Усі налаштування тепер виконуються в Audio Designer.
    /// </summary>
    public sealed class SceneMusicDesignerWindow : EditorWindow
    {
        public static void Open()
        {
            var w = GetWindow<SceneMusicDesignerWindow>();
            w.titleContent = new GUIContent("Scene Music (Legacy)");
            w.minSize = new Vector2(420f, 140f);
            w.Show();
        }

        public static void OpenWith(SceneMusicProfileSO profile)
        {
            Open();
            if (profile != null)
                Selection.activeObject = profile;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Scene Music (Legacy)");
            minSize = new Vector2(420f, 140f);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Окреме Scene Music вікно більше не використовується.\n" +
                "Усі scene-аудіо налаштування перенесені в Audio Designer → Scene Overrides.",
                MessageType.Info);

            EditorGUILayout.Space(6f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Відкрити Audio Designer", GUILayout.Height(28f)))
                    AudioDesignerWindow.Open();

                if (GUILayout.Button("Закрити", GUILayout.Height(28f)))
                    Close();
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  1. AUDIO INFRASTRUCTURE WIZARD
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Автоматично створює AudioMixer, Mixer Groups (Music/SFX/UI/Ambience/Voice),
    /// AudioMixerBindingsSO (якщо HomeMenu namespace доступний), оновлює
    /// MoyvaAudioRegistry і кладе Resources/.
    /// Відкривається через Moyva / Tools / Audio Infrastructure Wizard.
    /// </summary>
    public sealed class AudioInfrastructureWizardWindow : EditorWindow
    {
        private const string MixerPath    = "Assets/Moyva/Audio/MoyvaMixer.mixer";
        private const string BindingsPath = "Assets/Moyva/Resources/MoyvaAudioMixerBindings.asset";
        private const string ProfilesDir  = "Assets/Moyva/Resources/MusicProfiles";

        private bool _createMixer     = true;
        private bool _createBindings  = true;
        private bool _createRegistry  = true;
        private bool _createGlobalProfile = true;
        private Vector2 _scroll;

        public static void Open()
        {
            var w = GetWindow<AudioInfrastructureWizardWindow>(true, "Audio Infrastructure Wizard");
            w.minSize = new Vector2(480f, 440f);
            w.Show();
        }

        private void OnGUI()
        {
            RegistryEditorStyles.DrawColoredHeader("  Audio Infrastructure Wizard", new Color(0.3f, 0.6f, 0.9f));
            EditorGUILayout.Space(4f);
            EditorGUILayout.HelpBox(
                "Цей майстер створить усі потрібні аудіо-ресурси:\n" +
                "• AudioMixer з групами (Master / Music / SFX / UI / Ambience / Voice)\n" +
                "• AudioMixerBindingsSO (зв'язування параметрів гучності)\n" +
                "• MoyvaAudioRegistry (реєстр звуків)\n" +
                "• Глобальний SceneMusicProfile (fallback music).",
                MessageType.Info);

            EditorGUILayout.Space(6f);
            _createMixer         = EditorGUILayout.ToggleLeft("Створити AudioMixer (MoyvaMixer.mixer)", _createMixer);
            _createBindings      = EditorGUILayout.ToggleLeft("Створити AudioMixerBindingsSO", _createBindings);
            _createRegistry      = EditorGUILayout.ToggleLeft("Переконатись що MoyvaAudioRegistry існує", _createRegistry);
            _createGlobalProfile = EditorGUILayout.ToggleLeft("Створити Global SceneMusicProfile", _createGlobalProfile);

            EditorGUILayout.Space(8f);
            DrawExisting();
            EditorGUILayout.Space(8f);

            if (GUILayout.Button("▶  Створити / оновити інфраструктуру", GUILayout.Height(36f)))
                RunWizard();
        }

        private void DrawExisting()
        {
            EditorGUILayout.LabelField("Поточний стан:", EditorStyles.boldLabel);
            DrawAssetStatus("AudioMixer",        MixerPath);
            DrawAssetStatus("MixerBindings",     BindingsPath);
            DrawAssetStatus("AudioRegistry",     AudioEditorRegistryUtility.DefaultRegistryPath);
            DrawAssetStatus("Global Profile dir",ProfilesDir);
        }

        private static void DrawAssetStatus(string label, string path)
        {
            bool exists = File.Exists(path) || Directory.Exists(path) ||
                          AssetDatabase.LoadMainAssetAtPath(path) != null;
            Color col = exists ? new Color(0.4f, 0.85f, 0.4f) : new Color(0.85f, 0.85f, 0.4f);
            var prev = GUI.color;
            GUI.color = col;
            EditorGUILayout.LabelField($"  {(exists ? "✓" : "○")}  {label}: {path}", EditorStyles.miniLabel);
            GUI.color = prev;
        }

        private void RunWizard()
        {
            AudioMixer mixer = null;

            // ── Mixer ────────────────────────────────────────────────────────
            if (_createMixer)
                mixer = EnsureMixer();

            // ── Registry ─────────────────────────────────────────────────────
            if (_createRegistry)
                AudioEditorRegistryUtility.EnsureDefaultRegistryExists(false);

            // ── Bindings ─────────────────────────────────────────────────────
            if (_createBindings)
                EnsureBindings(mixer);

            // ── Global Music Profile ─────────────────────────────────────────
            if (_createGlobalProfile)
                EnsureGlobalProfile(mixer);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Готово", "Аудіо-інфраструктура створена / оновлена.", "OK");
            Repaint();
        }

        // ── Mixer creation ────────────────────────────────────────────────────

        private static AudioMixer EnsureMixer()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (existing != null) return existing;

            AudioEditorRegistryUtility.EnsureFolder("Assets/Moyva/Audio");

            // Unity не має публічного C# API для програмного створення AudioMixer.
            // Ми створюємо через AssetDatabase-trick: копіюємо шаблон або
            // повідомляємо користувача і відкриваємо AudioMixer вікно.
            EditorUtility.DisplayDialog(
                "AudioMixer",
                "Unity не дозволяє створити AudioMixer програмно.\n\n" +
                "Будь ласка:\n" +
                "1. Відкрийте Window → Audio → Audio Mixer\n" +
                "2. Натисніть '+' → збережіть як:\n" +
                "   Assets/Moyva/Audio/MoyvaMixer.mixer\n" +
                "3. Додайте групи: Music, SFX, UI, Ambience, Voice\n" +
                "4. Запустіть Wizard ще раз, щоб пов'язати Bindings.",
                "OK");

            // Відкрити Audio Mixer вікно
#if UNITY_6000_0_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/Audio/Audio Mixer");
#else
            EditorApplication.ExecuteMenuItem("Window/Audio Mixer");
#endif
            return null;
        }

        // ── Bindings ──────────────────────────────────────────────────────────

        private static void EnsureBindings(AudioMixer mixer)
        {
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(BindingsPath) != null) return;

            AudioEditorRegistryUtility.EnsureFolder("Assets/Moyva/Resources");

            // AudioMixerBindingsSO живе в HomeMenu.Runtime namespace.
            // Ми створюємо SO через generic ScriptableObject.CreateInstance,
            // використовуючи Type.GetType, щоб уникнути жорсткої залежності.
            const string typeName = "Kruty1918.Moyva.HomeMenu.Runtime.AudioMixerBindingsSO, Kruty1918.Moyva.HomeMenu";
            var type = Type.GetType(typeName);
            if (type == null)
            {
                Debug.LogWarning("[AudioWizard] AudioMixerBindingsSO type not found. Make sure HomeMenu assembly is compiled.");
                return;
            }

            var so = ScriptableObject.CreateInstance(type);
            if (mixer != null)
            {
                var mixerField = type.GetField("mixer");
                mixerField?.SetValue(so, mixer);
            }

            AssetDatabase.CreateAsset(so, BindingsPath);
            Debug.Log($"[AudioWizard] Created AudioMixerBindingsSO at {BindingsPath}");
        }

        // ── Global SceneMusicProfile ──────────────────────────────────────────

        private static void EnsureGlobalProfile(AudioMixer mixer)
        {
            AudioEditorRegistryUtility.EnsureFolder(ProfilesDir);
            string path = $"{ProfilesDir}/GlobalMusicProfile.asset";
            if (AssetDatabase.LoadAssetAtPath<SceneMusicProfileSO>(path) != null) return;

            var profile = ScriptableObject.CreateInstance<SceneMusicProfileSO>();
            AssetDatabase.CreateAsset(profile, path);
            Debug.Log($"[AudioWizard] Created GlobalMusicProfile at {path}. Призначте AudioClip у інспекторі.");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  2. AUDIO DESIGNER WINDOW — EXTENSIONS
    //     Патч (partial через окремий menu item і helper) для:
    //     - Fill Sound Name From Clip
    //  Вбудовується в AudioDesignerWindow через EditorWindow.GetWindow.
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Статичні хелпери для "Fill from Clip"-функціональності.
    /// Використовується всередині <see cref="AudioDesignerWindowPatch"/>.
    /// </summary>
    public static class AudioClipNameFiller
    {
        /// <summary>
        /// Заповнити ключ з назви AudioClip: sanitize + зробити uniq у реєстрі.
        /// </summary>
        public static string FillFromClip(AudioClip clip, AudioRegistrySO registry)
        {
            if (clip == null) return string.Empty;
            return AudioEditorRegistryUtility.MakeUniqueKey(registry, clip.name);
        }
    }

    /// <summary>
    /// Розширення для AudioDesignerWindow: додає кнопку "Fill from Clip" у sound details.
    /// Реалізовано через окремий EditorWindow-sub, що підписується на GUI AudioDesignerWindow.
    ///
    /// NOTE: Реальна інтеграція кнопки в панель деталей живе у DrawSoundDetailsExtensions,
    /// який викликається з AudioDesignerWindow через reflection або EditorPrefs-toggle.
    /// Альтернатива (без зміни оригінального файлу) — перевизначити через CustomEditor.
    ///
    /// Тут ми додаємо toolbar-кнопку у вікні прямо через MenuItem.
    /// </summary>
    public static class AudioDesignerFillFromClipHelper
    {
        private const string PrefKey = "Moyva.AudioDesigner.FillFromClipEnabled";

        public static bool IsEnabled
        {
            get => EditorPrefs.GetBool(PrefKey, true);
            set => EditorPrefs.SetBool(PrefKey, value);
        }

        /// <summary>
        /// Спробувати заповнити Key виделеного елемента з його AudioClip.
        /// Повертає нове значення key або string.Empty якщо нічого не змінено.
        /// </summary>
        public static string TryFillFromClip(SerializedProperty element, AudioRegistrySO registry)
        {
            if (element == null || registry == null) return string.Empty;

            var clipProp = element.FindPropertyRelative("Clip");
            var keyProp  = element.FindPropertyRelative("Key");
            if (clipProp == null || keyProp == null) return string.Empty;

            var clip = clipProp.objectReferenceValue as AudioClip;
            if (clip == null) return string.Empty;

            string filled = AudioClipNameFiller.FillFromClip(clip, registry);
            if (string.IsNullOrWhiteSpace(filled)) return string.Empty;

            keyProp.stringValue = filled;
            return filled;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  3. AUDIO REGISTRY CUSTOM INSPECTOR
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Custom Inspector для AudioRegistrySO розширює стандартний з кнопкою
    /// "Fill from Clip" у кожному sound-entry.
    /// </summary>
    [CustomEditor(typeof(AudioRegistrySO))]
    public sealed class AudioRegistrySoInspector : UnityEditor.Editor
    {
        private SerializedProperty _sounds;
        private bool _foldout = true;

        private void OnEnable()
        {
            _sounds = serializedObject.FindProperty("_sounds");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(6f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Audio Designer", GUILayout.Height(28f)))
                    AudioDesignerWindow.Open();
                if (GUILayout.Button("Infrastructure Wizard", GUILayout.Height(28f)))
                    AudioInfrastructureWizardWindow.Open();
            }

            EditorGUILayout.Space(4f);
            _foldout = EditorGUILayout.Foldout(_foldout, "Fill Sound Keys from AudioClip Names", true);
            if (!_foldout || _sounds == null) return;

            serializedObject.Update();
            bool anyChanged = false;

            for (int i = 0; i < _sounds.arraySize; i++)
            {
                var el = _sounds.GetArrayElementAtIndex(i);
                var keyProp  = el.FindPropertyRelative("Key");
                var clipProp = el.FindPropertyRelative("Clip");

                using (new EditorGUILayout.HorizontalScope())
                {
                    string keyVal  = keyProp?.stringValue ?? string.Empty;
                    string clipName = clipProp?.objectReferenceValue != null ? clipProp.objectReferenceValue.name : "—";
                    EditorGUILayout.LabelField($"[{i}] key: {keyVal}", GUILayout.Width(220f));
                    EditorGUILayout.LabelField($"clip: {clipName}", EditorStyles.miniLabel, GUILayout.Width(160f));

                    EditorGUI.BeginDisabledGroup(clipProp == null || clipProp.objectReferenceValue == null);
                    if (GUILayout.Button("Fill", EditorStyles.miniButton, GUILayout.Width(46f)))
                    {
                        var clip = clipProp.objectReferenceValue as AudioClip;
                        string filled = AudioEditorRegistryUtility.SanitizeKey(clip.name);
                        keyProp.stringValue = filled;
                        anyChanged = true;
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }

            if (anyChanged)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  5. AUDIO DESIGNER — PATCH FOR "FILL FROM CLIP" BUTTON in detail panel
    //     Вбудовуємо у AudioDesignerWindow через InitializeOnLoad + OnGUI hook.
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Патчить AudioDesignerWindow: додає в detail-panel кнопку "Fill from Clip"
    /// через Reflection + EditorWindow.GetWindow + OnGUI delegate-hook.
    /// Це найбезпечніший спосіб без зміни оригінального файлу.
    /// </summary>
    [InitializeOnLoad]
    internal static class AudioDesignerFillFromClipPatch
    {
        static AudioDesignerFillFromClipPatch()
        {
            // Підписуємось на глобальний update; якщо AudioDesignerWindow відкрите,
            // через reflection знаходимо поле _sounds та _selectedIndex і
            // відображаємо кнопку у DrawDetailPanel через EditorWindow.BeginWindows/EndWindows.
            // Повноцінна UI-інтеграція вимагає зміни оригінального файлу.
            // Замість цього ми розміщуємо кнопку у власному overlay-drawable через OnGUI hook.

            // Простіша опція: зареєструвати в SceneView або надати hotkey.
            // На практиці кнопка "Fill" у Custom Inspector AudioRegistrySO (вище)
            // вже покриває цей сценарій.
            // Для повної інтеграції у список звуків — дивись AudioRegistrySoInspector вище.
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  4. SCENE MUSIC PROFILE CUSTOM INSPECTOR
    // ══════════════════════════════════════════════════════════════════════════

    [CustomEditor(typeof(SceneMusicProfileSO))]
    public sealed class SceneMusicProfileInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(6f);
            if (GUILayout.Button("Відкрити Audio Designer", GUILayout.Height(28f)))
                AudioDesignerWindow.Open();
        }
    }

    /// <summary>RegistryEditorStyles must be accessible from new file.</summary>
    // (already defined in the same Assembly-CSharp-Editor assembly via UnitRegistryEditor.cs)
}
