using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Units.Runtime;
using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.Audio.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    [CustomEditor(typeof(UnitRegistrySO))]
    public sealed class UnitRegistryEditor : UnityEditor.Editor
    {
        private const string UnitPrefabFolder = "Assets/Moyva/Prefabs/Units";
        private const string UnitDesignerRegistryGuidPrefsKey = "Moyva.UnitDesigner.RegistryGuid";

        private SerializedProperty _configs;
        private Vector2 _scroll;
        private bool _createOpen;
        private string _newId = "";
        private float _newStamina = 100f;
        private Vector2 _newStaminaRange = new(-5f, 5f);
        private Sprite _newSprite;
        private GameObject _newPrefab;

        private void OnEnable()
        {
            _configs = serializedObject.FindProperty("Configs");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RegistryEditorStyles.DrawColoredHeader("  Unit Registry", RegistryEditorStyles.Accent);
            EditorGUILayout.Space(2);

            int count = _configs?.arraySize ?? 0;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{count} юніт(ів)", EditorStyles.boldLabel);
            if (GUILayout.Button("Unit Designer", GUILayout.Width(130)))
                OpenUnitDesignerForCurrentRegistry();
            if (GUILayout.Button("Відкрити Registry Hub", GUILayout.Width(160)))
                RegistryHubWindow.Open(2);
            EditorGUILayout.EndHorizontal();

            RegistryEditorStyles.DrawSeparator();

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MaxHeight(500));

            int removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                var el = _configs.GetArrayElementAtIndex(i);
                string typeId  = el.FindPropertyRelative("TypeId")?.stringValue ?? "?";
                float  stamina = el.FindPropertyRelative("BaseStamina")?.floatValue ?? 0f;
                var    pfb     = el.FindPropertyRelative("Prefab")?.objectReferenceValue;

                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                EditorGUILayout.BeginVertical(style);

                // Заголовок
                EditorGUILayout.BeginHorizontal();
                DrawIdLabel(typeId);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"Стаміна: {stamina:F0}", EditorStyles.miniLabel, GUILayout.Width(100));
                EditorGUILayout.LabelField(pfb ? $"\u2713" : "\u2717", EditorStyles.miniLabel, GUILayout.Width(16));
                Color prev = GUI.color;
                GUI.color = RegistryEditorStyles.ErrorCol;
                if (GUILayout.Button("\u00d7", GUILayout.Width(22), GUILayout.Height(18)))
                    removeIdx = i;
                GUI.color = prev;
                EditorGUILayout.EndHorizontal();

                // Inline editing
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(el.FindPropertyRelative("TypeId"), new GUIContent("Type ID"));
                ValidateInlineId(el.FindPropertyRelative("TypeId")?.stringValue);
                EditorGUILayout.PropertyField(el.FindPropertyRelative("BaseStamina"));
                EditorGUILayout.PropertyField(el.FindPropertyRelative("StaminaRandomRange"));
                EditorGUILayout.PropertyField(el.FindPropertyRelative("Prefab"));
                EditorGUILayout.PropertyField(el.FindPropertyRelative("AnimationSettings"), true);
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(1);
            }

            EditorGUILayout.EndScrollView();

            if (removeIdx >= 0)
            {
                string name = _configs.GetArrayElementAtIndex(removeIdx).FindPropertyRelative("TypeId")?.stringValue ?? "?";
                if (EditorUtility.DisplayDialog("Видалити", $"Видалити юніта '{name}'?", "Так", "Ні"))
                    _configs.DeleteArrayElementAtIndex(removeIdx);
            }

            RegistryEditorStyles.DrawSeparator();

            _createOpen = EditorGUILayout.Foldout(_createOpen, "\u2795 Створити нового юніта", true, EditorStyles.foldoutHeader);
            if (_createOpen)
            {
                EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
                _newId           = RegistryEditorStyles.IdFieldWithDuplicateCheck("Type ID", _newId, _configs, "TypeId");
                _newStamina      = EditorGUILayout.FloatField("Base Stamina", _newStamina);
                _newStaminaRange = EditorGUILayout.Vector2Field("Stamina Random Range", _newStaminaRange);
                _newSprite       = (Sprite)EditorGUILayout.ObjectField("Sprite", _newSprite, typeof(Sprite), false);
                _newPrefab       = (GameObject)EditorGUILayout.ObjectField("Prefab (override)", _newPrefab, typeof(GameObject), false);
                if (!_newPrefab && !_newSprite)
                    EditorGUILayout.HelpBox("Prefab буде створено автоматично (порожній).", MessageType.Info);
                EditorGUILayout.Space(4);

                bool valid = RegistryEditorStyles.ValidateIdFull(_newId, _configs, "TypeId") == null;
                EditorGUI.BeginDisabledGroup(!valid);
                if (GUILayout.Button("\u2713 Створити юніта", RegistryEditorStyles.CreateButton))
                    DoCreate();
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DoCreate()
        {
            string id = _newId.Trim();
            if (RegistryEditorStyles.ValidateIdFull(id, _configs, "TypeId") != null) return;

            GameObject pfb = _newPrefab ? _newPrefab : CreatePrefab(id, _newSprite);
            if (!pfb) pfb = CreateEmptyPrefab(id);

            int idx = _configs.arraySize;
            _configs.InsertArrayElementAtIndex(idx);
            var el = _configs.GetArrayElementAtIndex(idx);
            el.FindPropertyRelative("TypeId").stringValue = id;
            el.FindPropertyRelative("BaseStamina").floatValue = _newStamina;
            el.FindPropertyRelative("StaminaRandomRange").vector2Value = _newStaminaRange;
            el.FindPropertyRelative("Prefab").objectReferenceValue = pfb;

            var anim = el.FindPropertyRelative("AnimationSettings");
            if (anim != null)
            {
                var dur = anim.FindPropertyRelative("MoveDurationPerTile");
                if (dur != null) dur.floatValue = 0.3f;
                var delay = anim.FindPropertyRelative("DelayOnTile");
                if (delay != null) delay.floatValue = 0.05f;
            }

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            _newId = ""; _newSprite = null; _newPrefab = null;
        }

        private void OpenUnitDesignerForCurrentRegistry()
        {
            string path = AssetDatabase.GetAssetPath(target);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrWhiteSpace(guid))
                EditorPrefs.SetString(UnitDesignerRegistryGuidPrefsKey, guid);

            EditorApplication.ExecuteMenuItem("Moyva/Tools/Unit Designer");
        }

        private bool ContainsId(string id)
        {
            for (int i = 0; i < _configs.arraySize; i++)
            {
                string existing = _configs.GetArrayElementAtIndex(i).FindPropertyRelative("TypeId")?.stringValue;
                if (string.Equals(existing, id, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static GameObject CreatePrefab(string id, Sprite sprite)
        {
            if (!sprite) return null;
            EnsureFolder(UnitPrefabFolder);
            string safe = id.Replace('/', '-').Replace('\\', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{UnitPrefabFolder}/{safe}.prefab");
            var go = new GameObject(safe);
            go.AddComponent<SpriteRenderer>().sprite = sprite;
            var pfb = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            return pfb;
        }

        private static void DrawIdLabel(string id)
        {
            string err = RegistryEditorStyles.ValidateId(id);
            Color prev = GUI.color;
            if (err != null) GUI.color = RegistryEditorStyles.ErrorCol;
            EditorGUILayout.LabelField(err != null ? $"\u26a0 {id}" : id, RegistryEditorStyles.EntryTitle);
            GUI.color = prev;
        }

        private static void ValidateInlineId(string id)
        {
            if (id != null && id.Contains('_'))
            {
                Color prev = GUI.color;
                GUI.color = RegistryEditorStyles.ErrorCol;
                EditorGUILayout.HelpBox("'_' заборонений в ID.", MessageType.Error);
                GUI.color = prev;
            }
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Replace('\\', '/').TrimEnd('/').Split('/');
            if (parts.Length == 0 || parts[0] != "Assets") return;
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        private static GameObject CreateEmptyPrefab(string id)
        {
            EnsureFolder(UnitPrefabFolder);
            string safe = id.Replace('/', '-').Replace('\\', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{UnitPrefabFolder}/{safe}.prefab");
            var go = new GameObject(safe);
            var pfb = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            return pfb;
        }
    }
}

namespace Kruty1918.Moyva.Editor.Audio
{
    [InitializeOnLoad]
    internal static class AudioProjectBootstrapper
    {
        static AudioProjectBootstrapper()
        {
            EditorApplication.delayCall += () => AudioEditorRegistryUtility.EnsureDefaultRegistryExists(false);
        }
    }

    internal static class AudioEditorRegistryUtility
    {
        public const string DefaultRegistryPath = "Assets/Moyva/Resources/MoyvaAudioRegistry.asset";
        public const string DefaultRegistryResourcePath = "MoyvaAudioRegistry";

        public static AudioRegistrySO GetOrCreateDefaultRegistry()
            => EnsureDefaultRegistryExists(true);

        public static AudioRegistrySO FindRegistry()
        {
            var registry = AssetDatabase.LoadAssetAtPath<AudioRegistrySO>(DefaultRegistryPath);
            if (registry != null)
                return registry;

            string[] guids = AssetDatabase.FindAssets("t:AudioRegistrySO");
            if (guids == null || guids.Length == 0)
                return null;

            Array.Sort(guids, StringComparer.OrdinalIgnoreCase);
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<AudioRegistrySO>(path);
        }

        public static AudioRegistrySO EnsureDefaultRegistryExists(bool focus)
        {
            var registry = AssetDatabase.LoadAssetAtPath<AudioRegistrySO>(DefaultRegistryPath);
            if (registry != null)
            {
                if (focus)
                    EditorGUIUtility.PingObject(registry);
                return registry;
            }

            EnsureFolder("Assets/Moyva/Resources");
            registry = ScriptableObject.CreateInstance<AudioRegistrySO>();
            AssetDatabase.CreateAsset(registry, DefaultRegistryPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (focus)
            {
                Selection.activeObject = registry;
                EditorGUIUtility.PingObject(registry);
            }

            return registry;
        }

        public static List<AudioSoundDefinition> GetSounds(AudioRegistrySO registry)
        {
            var result = new List<AudioSoundDefinition>();
            if (registry == null || registry.Sounds == null)
                return result;

            for (int i = 0; i < registry.Sounds.Length; i++)
            {
                if (registry.Sounds[i] != null)
                    result.Add(registry.Sounds[i]);
            }

            result.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        public static string MakeUniqueKey(AudioRegistrySO registry, string baseKey)
        {
            baseKey = SanitizeKey(string.IsNullOrWhiteSpace(baseKey) ? "sound" : baseKey);
            if (!ContainsKey(registry, baseKey))
                return baseKey;

            for (int i = 2; i < 999; i++)
            {
                string key = $"{baseKey}-{i}";
                if (!ContainsKey(registry, key))
                    return key;
            }

            return $"{baseKey}-{Guid.NewGuid():N}";
        }

        public static string SanitizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return "sound";

            key = key.Trim().Replace(' ', '-').Replace('_', '-').ToLowerInvariant();
            var chars = key.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                bool ok = c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c == '-' || c == '/' || c == '.';
                if (!ok)
                    chars[i] = '-';
            }

            return new string(chars).Trim('-');
        }

        public static bool ContainsKey(AudioRegistrySO registry, string key)
        {
            if (registry == null || string.IsNullOrWhiteSpace(key))
                return false;

            var sounds = registry.Sounds;
            if (sounds == null)
                return false;

            for (int i = 0; i < sounds.Length; i++)
            {
                if (sounds[i] != null && string.Equals(sounds[i].Key, key, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static void EnsureFolder(string folder)
        {
            string[] parts = folder.Replace('\\', '/').TrimEnd('/').Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
                return;

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        public const string DefaultOverridesPath = "Assets/Moyva/Resources/MoyvaSceneAudioOverrides.asset";

        public static SceneAudioOverridesSO FindOrCreateOverridesSo()
        {
            var existing = AssetDatabase.LoadAssetAtPath<SceneAudioOverridesSO>(DefaultOverridesPath);
            if (existing != null) return existing;

            string[] guids = AssetDatabase.FindAssets("t:SceneAudioOverridesSO");
            if (guids != null && guids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<SceneAudioOverridesSO>(AssetDatabase.GUIDToAssetPath(guids[0]));

            EnsureFolder("Assets/Moyva/Resources");
            var so = ScriptableObject.CreateInstance<SceneAudioOverridesSO>();
            AssetDatabase.CreateAsset(so, DefaultOverridesPath);
            AssetDatabase.SaveAssets();
            return so;
        }
    }

    internal static class AudioEditorPreview
    {
        private static GameObject _previewRoot;
        private static AudioSource _source;
        private static double _stopTime;

        public static void Play(AudioSoundDefinition sound)
        {
            if (sound == null)
                return;

            var clip = ResolveClip(sound);
            if (clip == null)
                return;

            EnsureSource();
            ConfigureSource(sound);
            _source.clip = clip;
            _source.volume = Mathf.Clamp01(sound.Volume);
            _source.pitch = Mathf.Clamp(Mathf.Approximately(sound.Pitch, 0f) ? 1f : sound.Pitch, -3f, 3f);
            _source.loop = sound.Loop;
            _source.Play();
            _stopTime = EditorApplication.timeSinceStartup + Mathf.Max(0.1f, clip.length + 0.2f);
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        public static void Play(AudioClip clip)
        {
            if (clip == null)
                return;

            EnsureSource();
            ClearEffects();
            _source.clip = clip;
            _source.volume = 1f;
            _source.pitch = 1f;
            _source.loop = false;
            _source.Play();
            _stopTime = EditorApplication.timeSinceStartup + Mathf.Max(0.1f, clip.length + 0.2f);
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        public static void Stop()
        {
            if (_source != null)
                _source.Stop();
        }

        private static void Tick()
        {
            if (_source == null || !_source.isPlaying || EditorApplication.timeSinceStartup >= _stopTime)
            {
                Stop();
                EditorApplication.update -= Tick;
            }
        }

        private static void EnsureSource()
        {
            if (_source != null)
                return;

            _previewRoot = EditorUtility.CreateGameObjectWithHideFlags(
                "Moyva Audio Preview",
                HideFlags.HideAndDontSave,
                typeof(AudioSource));
            _source = _previewRoot.GetComponent<AudioSource>();
            _source.playOnAwake = false;
        }

        private static AudioClip ResolveClip(AudioSoundDefinition sound)
        {
            if (sound.Variants != null)
            {
                for (int i = 0; i < sound.Variants.Length; i++)
                    if (sound.Variants[i] != null)
                        return sound.Variants[i];
            }

            return sound.Clip;
        }

        private static void ConfigureSource(AudioSoundDefinition sound)
        {
            ClearEffects();
            _source.outputAudioMixerGroup = sound.MixerGroup;
            _source.spatialBlend = sound.SpatialBlend;
            _source.dopplerLevel = sound.DopplerLevel;
            _source.priority = sound.Priority;
            _source.reverbZoneMix = sound.ReverbZoneMix;

            var effects = sound.Effects;
            if (effects == null)
                return;

            if (effects.EnableLowPass)
            {
                var f = _previewRoot.AddComponent<AudioLowPassFilter>();
                f.cutoffFrequency = effects.LowPassCutoff;
                f.lowpassResonanceQ = effects.LowPassResonance;
            }

            if (effects.EnableHighPass)
            {
                var f = _previewRoot.AddComponent<AudioHighPassFilter>();
                f.cutoffFrequency = effects.HighPassCutoff;
                f.highpassResonanceQ = effects.HighPassResonance;
            }

            if (effects.EnableEcho)
            {
                var f = _previewRoot.AddComponent<AudioEchoFilter>();
                f.delay = effects.EchoDelay;
                f.decayRatio = effects.EchoDecayRatio;
                f.wetMix = effects.EchoWetMix;
                f.dryMix = effects.EchoDryMix;
            }

            if (effects.EnableReverb)
            {
                var f = _previewRoot.AddComponent<AudioReverbFilter>();
                f.reverbPreset = effects.ReverbPreset;
            }

            if (effects.EnableDistortion)
            {
                var f = _previewRoot.AddComponent<AudioDistortionFilter>();
                f.distortionLevel = effects.DistortionLevel;
            }

            if (effects.EnableChorus)
            {
                var f = _previewRoot.AddComponent<AudioChorusFilter>();
                f.dryMix = effects.ChorusDryMix;
                f.wetMix1 = effects.ChorusWetMix1;
                f.wetMix2 = effects.ChorusWetMix2;
                f.wetMix3 = effects.ChorusWetMix3;
                f.delay = effects.ChorusDelay;
                f.rate = effects.ChorusRate;
                f.depth = effects.ChorusDepth;
            }
        }

        private static void ClearEffects()
        {
            if (_previewRoot == null)
                return;

            Remove<AudioLowPassFilter>();
            Remove<AudioHighPassFilter>();
            Remove<AudioEchoFilter>();
            Remove<AudioReverbFilter>();
            Remove<AudioDistortionFilter>();
            Remove<AudioChorusFilter>();
        }

        private static void Remove<T>() where T : Component
        {
            var component = _previewRoot.GetComponent<T>();
            if (component != null)
                UnityEngine.Object.DestroyImmediate(component);
        }
    }

    public sealed class AudioDesignerWindow : EditorWindow
    {
        private const string RegistryGuidPrefsKey = "Moyva.AudioDesigner.RegistryGuid";

        private AudioRegistrySO _registry;
        private SerializedObject _serializedRegistry;
        private SerializedProperty _sounds;
        private string _search = string.Empty;
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private int _selectedIndex = -1;
        private AudioBus? _busFilter;
        private bool _showOnlyProblems;

        // Scene Overrides tab
        private int _activeDetailTab;
        private int _selectedSceneIdx;
        private SceneAudioOverridesSO _overridesSo;
        private SerializedObject _serializedOverrides;

        [MenuItem("Moyva/Tools/Audio Designer %#a", priority = 31)]
        public static void Open()
        {
            var window = GetWindow<AudioDesignerWindow>();
            window.titleContent = new GUIContent("Audio Designer");
            window.minSize = new Vector2(920f, 560f);
            window.Show();
        }

        /// <summary>
        /// Відкриває Audio Designer і виділяє звук за ключем.
        /// Якщо ключ не знайдено — просто відкриває вікно.
        /// </summary>
        public static void OpenAndSelect(string key)
        {
            var window = GetWindow<AudioDesignerWindow>();
            window.titleContent = new GUIContent("Audio Designer");
            window.minSize = new Vector2(920f, 560f);
            window.Show();

            if (string.IsNullOrWhiteSpace(key) || window._registry == null)
                return;

            var sounds = window._registry.Sounds;
            for (int i = 0; i < sounds.Length; i++)
            {
                if (sounds[i].Key == key)
                {
                    window._selectedIndex = i;
                    window._activeDetailTab = 0;
                    break;
                }
            }
        }

        private void OnEnable()
        {
            ResolveRegistry();
            LoadOrCreateOverridesSo();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_registry == null)
            {
                EditorGUILayout.HelpBox("AudioRegistry не знайдено. Створи дефолтний registry, щоб додавати звуки під ключі.", MessageType.Info);
                if (GUILayout.Button("Створити MoyvaAudioRegistry.asset", GUILayout.Height(32f)))
                    SetRegistry(AudioEditorRegistryUtility.GetOrCreateDefaultRegistry());
                return;
            }

            _serializedRegistry.Update();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawListPanel();
                DrawDetailPanel();
            }

            _serializedRegistry.ApplyModifiedProperties();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField("Audio Registry", GUILayout.Width(90f));
                var nextRegistry = (AudioRegistrySO)EditorGUILayout.ObjectField(_registry, typeof(AudioRegistrySO), false, GUILayout.MinWidth(220f));
                if (nextRegistry != _registry)
                    SetRegistry(nextRegistry);

                if (GUILayout.Button("Auto", EditorStyles.toolbarButton, GUILayout.Width(52f)))
                    SetRegistry(AudioEditorRegistryUtility.FindRegistry() ?? AudioEditorRegistryUtility.GetOrCreateDefaultRegistry());

                if (GUILayout.Button("Create", EditorStyles.toolbarButton, GUILayout.Width(64f)))
                    SetRegistry(AudioEditorRegistryUtility.GetOrCreateDefaultRegistry());

                if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(54f)))
                    SaveAllAudioAssets();

                GUILayout.FlexibleSpace();

                if (_registry != null && GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(50f)))
                    EditorGUIUtility.PingObject(_registry);

                if (GUILayout.Button("Infra Wizard", EditorStyles.toolbarButton, GUILayout.Width(88f)))
                    AudioInfrastructureWizardWindow.Open();
            }
        }

        private void DrawListPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(330f)))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _search = EditorGUILayout.TextField(_search, GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.textField);
                    if (GUILayout.Button("+", GUILayout.Width(34f), GUILayout.Height(22f)))
                        AddSound();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Toggle(!_busFilter.HasValue, "All", EditorStyles.miniButtonLeft, GUILayout.Width(45f)))
                        _busFilter = null;

                    foreach (AudioBus bus in Enum.GetValues(typeof(AudioBus)))
                    {
                        bool active = _busFilter == bus;
                        if (GUILayout.Toggle(active, bus.ToString(), EditorStyles.miniButton, GUILayout.MinWidth(42f)))
                            _busFilter = bus;
                    }
                }

                _showOnlyProblems = EditorGUILayout.ToggleLeft("Показати тільки проблемні", _showOnlyProblems);
                EditorGUILayout.Space(4f);

                _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
                for (int i = 0; i < _sounds.arraySize; i++)
                {
                    var element = _sounds.GetArrayElementAtIndex(i);
                    if (!PassesFilter(element))
                        continue;

                    DrawListRow(i, element);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawListRow(int index, SerializedProperty element)
        {
            var key = element.FindPropertyRelative("Key");
            var clip = element.FindPropertyRelative("Clip");
            var bus = element.FindPropertyRelative("Bus");

            Rect rect = EditorGUILayout.BeginVertical(index == _selectedIndex ? "flow node 0 on" : "box");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("▶", GUILayout.Width(28f)))
                    AudioEditorPreview.Play(GetSoundAt(index));

                if (GUILayout.Button(string.IsNullOrWhiteSpace(key.stringValue) ? "<без ключа>" : key.stringValue, EditorStyles.boldLabel))
                    _selectedIndex = index;

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(((AudioBus)bus.enumValueIndex).ToString(), EditorStyles.miniLabel, GUILayout.Width(62f));
            }

            string clipName = clip.objectReferenceValue != null ? clip.objectReferenceValue.name : "немає AudioClip";
            EditorGUILayout.LabelField(clipName, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                _selectedIndex = index;
                Repaint();
            }
        }

        private void DrawDetailPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                DrawRegistrySettings();
                EditorGUILayout.Space(6f);

                if (_sounds == null || _selectedIndex < 0 || _selectedIndex >= _sounds.arraySize)
                {
                    EditorGUILayout.HelpBox("Обери звук зі списку або натисни '+', щоб додати новий ключ.", MessageType.Info);
                    return;
                }

                try
                {
                    var element = _sounds.GetArrayElementAtIndex(_selectedIndex);
                    if (element == null)
                    {
                        EditorGUILayout.HelpBox("Обраний звук більше не доступний. Обери інший.", MessageType.Warning);
                        _selectedIndex = _sounds.arraySize > 0 ? 0 : -1;
                        return;
                    }

                    _activeDetailTab = GUILayout.Toolbar(_activeDetailTab, new[] { "Sound", "Scene Overrides" });
                    EditorGUILayout.Space(2f);
                    _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
                    if (_activeDetailTab == 0)
                        DrawSoundDetails(element);
                    else
                        DrawSceneOverridesPanel(element);
                    EditorGUILayout.EndScrollView();
                }
                catch (System.ObjectDisposedException)
                {
                    // SerializedProperty was disposed (e.g., after array modification)
                    EditorGUILayout.HelpBox("Реєстр був змінений. Обнови вибір.", MessageType.Warning);
                    _selectedIndex = -1;
                }
            }
        }

        private void DrawRegistrySettings()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_serializedRegistry.FindProperty("_defaultPoolSize"), new GUIContent("Default Pool Size"));
                EditorGUILayout.PropertyField(_serializedRegistry.FindProperty("_dontDestroyOnLoad"), new GUIContent("Dont Destroy On Load"));
                EditorGUILayout.PropertyField(_serializedRegistry.FindProperty("_verboseLogs"), new GUIContent("Verbose Logs"));
                EditorGUILayout.HelpBox("Runtime завантажує цей asset через Resources/MoyvaAudioRegistry і піднімає пул автоматично у ProjectServicesInstaller.", MessageType.None);
            }
        }

        private void DrawSoundDetails(SerializedProperty element)
        {
            if (element == null)
                return;

            try
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Sound", EditorStyles.boldLabel);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Preview", GUILayout.Width(82f)))
                            AudioEditorPreview.Play(GetSoundAt(_selectedIndex));
                        if (GUILayout.Button("Stop", GUILayout.Width(58f)))
                            AudioEditorPreview.Stop();
                        if (GUILayout.Button("Duplicate", GUILayout.Width(82f)))
                            DuplicateSelected();
                        if (GUILayout.Button("Delete", GUILayout.Width(70f)))
                            DeleteSelected();
                    }

                    var key = element.FindPropertyRelative("Key");
                    if (key != null)
                    {
                        EditorGUILayout.PropertyField(key, new GUIContent("Key"));
                        string sanitized = AudioEditorRegistryUtility.SanitizeKey(key.stringValue);
                        if (!string.Equals(key.stringValue, sanitized, StringComparison.Ordinal) && GUILayout.Button($"Нормалізувати ключ: {sanitized}"))
                            key.stringValue = sanitized;
                    }

                    // ── Fill from Clip ──────────────────────────────────────────────
                    var clipForFill = element.FindPropertyRelative("Clip");
                    if (clipForFill != null)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.BeginDisabledGroup(clipForFill.objectReferenceValue == null);
                            if (GUILayout.Button("Fill from Clip", EditorStyles.miniButton, GUILayout.Width(110f)))
                            {
                                var ac = clipForFill.objectReferenceValue as AudioClip;
                                if (ac != null && key != null)
                                    key.stringValue = AudioEditorRegistryUtility.MakeUniqueKey(_registry, ac.name);
                            }
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.LabelField("← заповнити Key із назви AudioClip", EditorStyles.miniLabel);
                        }
                    }

                    DrawValidation(element);

                    var clip = element.FindPropertyRelative("Clip");
                    if (clip != null)
                        EditorGUILayout.PropertyField(clip);

                    var variants = element.FindPropertyRelative("Variants");
                    if (variants != null)
                        EditorGUILayout.PropertyField(variants, true);

                    var bus = element.FindPropertyRelative("Bus");
                    if (bus != null)
                        EditorGUILayout.PropertyField(bus);

                    var mixer = element.FindPropertyRelative("MixerGroup");
                    if (mixer != null)
                        EditorGUILayout.PropertyField(mixer);
                }

                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField("Playback", EditorStyles.boldLabel);
                    TryDrawProperty(element, "Volume");
                    TryDrawProperty(element, "VolumeRandom");
                    TryDrawProperty(element, "Pitch");
                    TryDrawProperty(element, "PitchRandom");
                    TryDrawProperty(element, "SpatialBlend");
                    TryDrawProperty(element, "DopplerLevel");
                    TryDrawProperty(element, "ReverbZoneMix");
                    TryDrawProperty(element, "Priority");
                    TryDrawProperty(element, "Loop");
                    TryDrawProperty(element, "PoolWarmup");
                    TryDrawProperty(element, "MaxSimultaneous");
                }

                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
                    var effects = element.FindPropertyRelative("Effects");
                    if (effects != null)
                        EditorGUILayout.PropertyField(effects, true);
                }
            }
            catch (System.ObjectDisposedException)
            {
                EditorGUILayout.HelpBox("Властивості звуку більше не доступні. Виберіть ще раз.", MessageType.Warning);
            }
        }

        private void TryDrawProperty(SerializedProperty element, string propertyName)
        {
            try
            {
                var prop = element.FindPropertyRelative(propertyName);
                if (prop != null)
                    EditorGUILayout.PropertyField(prop);
            }
            catch { }
        }

        private void DrawValidation(SerializedProperty element)
        {
            if (element == null)
                return;

            try
            {
                var keyProp = element.FindPropertyRelative("Key");
                string key = keyProp != null ? keyProp.stringValue : string.Empty;

                if (string.IsNullOrWhiteSpace(key))
                    EditorGUILayout.HelpBox("Ключ порожній. Такий звук не буде доступний через IAudioService.", MessageType.Warning);

                var clipProp = element.FindPropertyRelative("Clip");
                var variantsProp = element.FindPropertyRelative("Variants");
                if (clipProp != null && clipProp.objectReferenceValue == null && !HasVariants(variantsProp))
                    EditorGUILayout.HelpBox("Немає AudioClip або variants. Preview і Play не спрацюють.", MessageType.Warning);

                if (CountKey(key) > 1)
                    EditorGUILayout.HelpBox("Дублікат ключа. Runtime використає перший знайдений звук.", MessageType.Error);
            }
            catch { }
        }

        private bool PassesFilter(SerializedProperty element)
        {
            string key = element.FindPropertyRelative("Key").stringValue ?? string.Empty;
            var clip = element.FindPropertyRelative("Clip").objectReferenceValue;
            var bus = (AudioBus)element.FindPropertyRelative("Bus").enumValueIndex;

            if (_busFilter.HasValue && bus != _busFilter.Value)
                return false;

            if (_showOnlyProblems && !HasProblem(element))
                return false;

            if (string.IsNullOrWhiteSpace(_search))
                return true;

            string needle = _search.Trim();
            return key.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0
                   || (clip != null && clip.name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                   || bus.ToString().IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool HasProblem(SerializedProperty element)
        {
            string key = element.FindPropertyRelative("Key").stringValue;
            return string.IsNullOrWhiteSpace(key)
                   || CountKey(key) > 1
                   || element.FindPropertyRelative("Clip").objectReferenceValue == null && !HasVariants(element.FindPropertyRelative("Variants"));
        }

        private bool HasVariants(SerializedProperty variants)
        {
            if (variants == null || !variants.isArray)
                return false;

            for (int i = 0; i < variants.arraySize; i++)
                if (variants.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    return true;

            return false;
        }

        private int CountKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return 0;

            int count = 0;
            for (int i = 0; i < _sounds.arraySize; i++)
            {
                string other = _sounds.GetArrayElementAtIndex(i).FindPropertyRelative("Key").stringValue;
                if (string.Equals(other, key, StringComparison.OrdinalIgnoreCase))
                    count++;
            }

            return count;
        }

        private void AddSound()
        {
            _serializedRegistry.Update();
            int index = _sounds.arraySize;
            _sounds.InsertArrayElementAtIndex(index);
            var element = _sounds.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("Key").stringValue = AudioEditorRegistryUtility.MakeUniqueKey(_registry, "sound");
            element.FindPropertyRelative("Clip").objectReferenceValue = null;
            element.FindPropertyRelative("Variants").arraySize = 0;
            element.FindPropertyRelative("Bus").enumValueIndex = (int)AudioBus.Sfx;
            element.FindPropertyRelative("MixerGroup").objectReferenceValue = null;
            element.FindPropertyRelative("Volume").floatValue = 1f;
            element.FindPropertyRelative("VolumeRandom").floatValue = 0f;
            element.FindPropertyRelative("Pitch").floatValue = 1f;
            element.FindPropertyRelative("PitchRandom").floatValue = 0f;
            element.FindPropertyRelative("SpatialBlend").floatValue = 0f;
            element.FindPropertyRelative("DopplerLevel").floatValue = 0f;
            element.FindPropertyRelative("ReverbZoneMix").floatValue = 1f;
            element.FindPropertyRelative("Priority").intValue = 128;
            element.FindPropertyRelative("Loop").boolValue = false;
            element.FindPropertyRelative("PoolWarmup").intValue = 1;
            element.FindPropertyRelative("MaxSimultaneous").intValue = 8;
            ResetEffects(element.FindPropertyRelative("Effects"));
            _selectedIndex = index;
            _serializedRegistry.ApplyModifiedProperties();
            EditorUtility.SetDirty(_registry);
        }

        private static void ResetEffects(SerializedProperty effects)
        {
            if (effects == null)
                return;

            SetBool(effects, "EnableLowPass", false);
            SetFloat(effects, "LowPassCutoff", 5000f);
            SetFloat(effects, "LowPassResonance", 1f);
            SetBool(effects, "EnableHighPass", false);
            SetFloat(effects, "HighPassCutoff", 120f);
            SetFloat(effects, "HighPassResonance", 1f);
            SetBool(effects, "EnableEcho", false);
            SetFloat(effects, "EchoDelay", 220f);
            SetFloat(effects, "EchoDecayRatio", 0.35f);
            SetFloat(effects, "EchoWetMix", 0.28f);
            SetFloat(effects, "EchoDryMix", 1f);
            SetBool(effects, "EnableReverb", false);
            SetBool(effects, "EnableDistortion", false);
            SetFloat(effects, "DistortionLevel", 0.18f);
            SetBool(effects, "EnableChorus", false);
            SetFloat(effects, "ChorusDryMix", 0.5f);
            SetFloat(effects, "ChorusWetMix1", 0.5f);
            SetFloat(effects, "ChorusWetMix2", 0.5f);
            SetFloat(effects, "ChorusWetMix3", 0.5f);
            SetFloat(effects, "ChorusDelay", 40f);
            SetFloat(effects, "ChorusRate", 0.8f);
            SetFloat(effects, "ChorusDepth", 0.03f);
        }

        private static void SetBool(SerializedProperty root, string relativeName, bool value)
        {
            var property = root.FindPropertyRelative(relativeName);
            if (property != null)
                property.boolValue = value;
        }

        private static void SetFloat(SerializedProperty root, string relativeName, float value)
        {
            var property = root.FindPropertyRelative(relativeName);
            if (property != null)
                property.floatValue = value;
        }

        private void DuplicateSelected()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _sounds.arraySize)
                return;

            int next = _sounds.arraySize;
            _sounds.InsertArrayElementAtIndex(next);
            var element = _sounds.GetArrayElementAtIndex(next);
            string currentKey = element.FindPropertyRelative("Key").stringValue;
            element.FindPropertyRelative("Key").stringValue = AudioEditorRegistryUtility.MakeUniqueKey(_registry, currentKey);
            _selectedIndex = next;
            _serializedRegistry.ApplyModifiedProperties();
            EditorUtility.SetDirty(_registry);
        }

        private void DeleteSelected()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _sounds.arraySize)
                return;

            if (!EditorUtility.DisplayDialog("Видалити звук", "Прибрати цей звук з AudioRegistry? AudioClip asset не видаляється.", "Видалити", "Скасувати"))
                return;

            _sounds.DeleteArrayElementAtIndex(_selectedIndex);
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _sounds.arraySize - 1);
            _serializedRegistry.ApplyModifiedProperties();
            EditorUtility.SetDirty(_registry);
        }

        private AudioSoundDefinition GetSoundAt(int index)
        {
            if (_registry == null || _registry.Sounds == null || index < 0 || index >= _registry.Sounds.Length)
                return null;

            _serializedRegistry.ApplyModifiedProperties();
            return _registry.Sounds[index];
        }

        private void ResolveRegistry()
        {
            var contextRegistry = MoyvaProjectEditorContext.Get<AudioRegistrySO>();
            if (contextRegistry != null)
            {
                SetRegistry(contextRegistry);
                return;
            }

            string guid = EditorPrefs.GetString(RegistryGuidPrefsKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var saved = AssetDatabase.LoadAssetAtPath<AudioRegistrySO>(path);
                if (saved != null)
                {
                    SetRegistry(saved);
                    return;
                }
            }

            SetRegistry(AudioEditorRegistryUtility.FindRegistry() ?? AudioEditorRegistryUtility.GetOrCreateDefaultRegistry());
        }

        private void SetRegistry(AudioRegistrySO registry)
        {
            _registry = registry;
            _serializedRegistry = registry != null ? new SerializedObject(registry) : null;
            _sounds = _serializedRegistry?.FindProperty("_sounds");
            _selectedIndex = _sounds != null && _sounds.arraySize > 0 ? Mathf.Clamp(_selectedIndex, 0, _sounds.arraySize - 1) : -1;

            MoyvaProjectEditorContext.Set(registry);

            if (registry != null)
            {
                string path = AssetDatabase.GetAssetPath(registry);
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (!string.IsNullOrWhiteSpace(guid))
                    EditorPrefs.SetString(RegistryGuidPrefsKey, guid);
            }
        }

        private void SaveAllAudioAssets()
        {
            _serializedRegistry?.ApplyModifiedProperties();
            _serializedOverrides?.ApplyModifiedProperties();

            if (_registry != null)
                EditorUtility.SetDirty(_registry);

            if (_overridesSo != null)
                EditorUtility.SetDirty(_overridesSo);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ── Scene Overrides tab ───────────────────────────────────────────────

        private void LoadOrCreateOverridesSo()
        {
            _overridesSo = AudioEditorRegistryUtility.FindOrCreateOverridesSo();
            _serializedOverrides = _overridesSo != null ? new SerializedObject(_overridesSo) : null;
        }

        private void DrawSceneOverridesPanel(SerializedProperty element)
        {
            if (_overridesSo == null)
            {
                EditorGUILayout.HelpBox("SceneAudioOverrides SO не знайдено.", MessageType.Warning);
                if (GUILayout.Button("Створити MoyvaSceneAudioOverrides"))
                    LoadOrCreateOverridesSo();
                return;
            }

            string soundKey = element.FindPropertyRelative("Key")?.stringValue ?? string.Empty;
            if (string.IsNullOrWhiteSpace(soundKey))
            {
                EditorGUILayout.HelpBox("Задайте ключ звуку на вкладці Sound.", MessageType.Info);
                return;
            }

            string[] sceneNames   = GetBuildSceneNames();
            string[] sceneDisplay = GetBuildSceneDisplayNames(sceneNames);
            if (_selectedSceneIdx >= sceneNames.Length) _selectedSceneIdx = 0;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Сцена:", GUILayout.Width(55f));
                _selectedSceneIdx = EditorGUILayout.Popup(_selectedSceneIdx, sceneDisplay);
            }

            string selectedScene = sceneNames[_selectedSceneIdx];
            string sceneLabel    = _selectedSceneIdx == 0 ? "Всі сцени (глобально)" : selectedScene;

            EditorGUILayout.Space(6f);

            if (!_overridesSo.HasOverride(selectedScene, soundKey))
            {
                EditorGUILayout.HelpBox(
                    $"Немає override для: {sceneLabel}\nВикористовуються базові параметри Sound definition.",
                    MessageType.None);
                EditorGUILayout.Space(4f);
                if (GUILayout.Button($"+ Додати override для \"{sceneLabel}\"", GUILayout.Height(28f)))
                {
                    Undo.RecordObject(_overridesSo, "Add Sound Scene Override");
                    _overridesSo.GetOrCreate(selectedScene, soundKey);
                    _serializedOverrides = new SerializedObject(_overridesSo);
                    EditorUtility.SetDirty(_overridesSo);
                }
            }
            else
            {
                DrawExistingOverride(selectedScene, soundKey, sceneLabel);
            }

            EditorGUILayout.Space(10f);
            DrawAllOverridesSummary(soundKey);
        }

        private void DrawExistingOverride(string sceneName, string soundKey, string sceneLabel)
        {
            if (_serializedOverrides == null)
                _serializedOverrides = new SerializedObject(_overridesSo);

            _serializedOverrides.Update();

            int idx = FindOverrideIndex(sceneName, soundKey);
            if (idx < 0) { EditorGUILayout.HelpBox("Внутрішня помилка: override не знайдено.", MessageType.Error); return; }

            var ov = _serializedOverrides.FindProperty("_overrides").GetArrayElementAtIndex(idx);

            EditorGUILayout.LabelField($"Override: {sceneLabel}", EditorStyles.boldLabel);
            EditorGUILayout.Space(2f);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Параметри які перевизначити:", EditorStyles.miniLabel);
                EditorGUILayout.Space(3f);
                DrawOverrideField(ov, "OverrideVolume",       "Volume",       "Volume");
                DrawOverrideField(ov, "OverridePitch",        "Pitch",        "Pitch");
                DrawOverrideField(ov, "OverrideMixerGroup",   "MixerGroup",   "Mixer Group");
                DrawOverrideField(ov, "OverrideLoop",         "Loop",         "Loop");
                DrawOverrideField(ov, "OverrideSpatialBlend", "SpatialBlend", "Spatial Blend");
                DrawOverrideField(ov, "OverridePriority",     "Priority",     "Priority");

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Поведінка при завантаженні сцени:", EditorStyles.miniLabel);
                var playOnAwakeProp = ov.FindPropertyRelative("PlayOnAwake");
                if (playOnAwakeProp != null)
                    playOnAwakeProp.boolValue = EditorGUILayout.ToggleLeft(
                        new GUIContent("Play On Awake", "Автоматично запустити цей звук при завантаженні сцени."),
                        playOnAwakeProp.boolValue);
            }

            _serializedOverrides.ApplyModifiedProperties();

            EditorGUILayout.Space(6f);
            var prevCol = GUI.color;
            GUI.color = new Color(0.85f, 0.35f, 0.35f);
            if (GUILayout.Button($"\u00d7  Видалити override для \"{sceneLabel}\""))
            {
                GUI.color = prevCol;
                if (EditorUtility.DisplayDialog("Видалити override",
                    $"Видалити override для: {sceneLabel}?", "Так", "Ні"))
                {
                    Undo.RecordObject(_overridesSo, "Remove Sound Scene Override");
                    _overridesSo.RemoveOverride(sceneName, soundKey);
                    _serializedOverrides = new SerializedObject(_overridesSo);
                    EditorUtility.SetDirty(_overridesSo);
                }
            }
            GUI.color = prevCol;
        }

        private static void DrawOverrideField(SerializedProperty ov, string toggleName, string valueName, string displayName)
        {
            var toggleProp = ov.FindPropertyRelative(toggleName);
            var valueProp  = ov.FindPropertyRelative(valueName);
            if (toggleProp == null || valueProp == null) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                toggleProp.boolValue = EditorGUILayout.ToggleLeft(displayName, toggleProp.boolValue, GUILayout.Width(130f));
                using (new EditorGUI.DisabledScope(!toggleProp.boolValue))
                    EditorGUILayout.PropertyField(valueProp, GUIContent.none);
            }
        }

        private void DrawAllOverridesSummary(string soundKey)
        {
            if (_overridesSo == null) return;

            var allOverrides = _overridesSo.Overrides;
            var relevant = new List<SoundSceneOverride>();
            foreach (var o in allOverrides)
                if (o != null && string.Equals(o.SoundKey, soundKey, StringComparison.OrdinalIgnoreCase))
                    relevant.Add(o);

            if (relevant.Count == 0) { EditorGUILayout.HelpBox("Немає жодного override для цього звуку.", MessageType.None); return; }

            EditorGUILayout.LabelField("Всі overrides для цього звуку:", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("helpbox"))
            {
                foreach (var o in relevant)
                {
                    string scLabel = string.IsNullOrEmpty(o.SceneName) ? "[Global]" : o.SceneName;
                    var parts = new List<string>();
                    if (o.OverrideVolume)       parts.Add($"Vol:{o.Volume:F2}");
                    if (o.OverridePitch)        parts.Add($"Pitch:{o.Pitch:F2}");
                    if (o.OverrideMixerGroup && o.MixerGroup != null) parts.Add($"Mixer:{o.MixerGroup.name}");
                    if (o.OverrideLoop)         parts.Add($"Loop:{o.Loop}");
                    if (o.OverrideSpatialBlend) parts.Add($"Spatial:{o.SpatialBlend:F2}");
                    if (o.OverridePriority)     parts.Add($"Prior:{o.Priority}");
                    if (o.PlayOnAwake)          parts.Add("PlayOnAwake");
                    string details = parts.Count > 0 ? string.Join(", ", parts) : "(немає активних override)";

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"  {scLabel}:", GUILayout.Width(120f));
                        EditorGUILayout.LabelField(details, EditorStyles.miniLabel);
                        if (GUILayout.Button("\u00d7", GUILayout.Width(20f)))
                        {
                            Undo.RecordObject(_overridesSo, "Remove Sound Scene Override");
                            _overridesSo.RemoveOverride(o.SceneName, soundKey);
                            _serializedOverrides = new SerializedObject(_overridesSo);
                            EditorUtility.SetDirty(_overridesSo);
                            break;
                        }
                    }
                }
            }
        }

        private int FindOverrideIndex(string sceneName, string soundKey)
        {
            if (_serializedOverrides == null) return -1;
            var array = _serializedOverrides.FindProperty("_overrides");
            if (array == null || !array.isArray) return -1;
            for (int i = 0; i < array.arraySize; i++)
            {
                var el = array.GetArrayElementAtIndex(i);
                string sn = el.FindPropertyRelative("SceneName")?.stringValue ?? string.Empty;
                string sk = el.FindPropertyRelative("SoundKey")?.stringValue  ?? string.Empty;
                if (string.Equals(sn, sceneName, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(sk, soundKey, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        private static string[] GetBuildSceneNames()
        {
            var names = new List<string> { string.Empty };
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (!s.enabled) continue;
                string n = Path.GetFileNameWithoutExtension(s.path);
                if (!string.IsNullOrEmpty(n) && !names.Contains(n))
                    names.Add(n);
            }
            string cur = EditorSceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(cur) && !names.Contains(cur))
                names.Add(cur);
            return names.ToArray();
        }

        private static string[] GetBuildSceneDisplayNames(string[] sceneNames)
        {
            var display = new string[sceneNames.Length];
            display[0] = "--- Всі сцени (глобально) ---";
            for (int i = 1; i < sceneNames.Length; i++)
                display[i] = sceneNames[i];
            return display;
        }
    }
    public sealed class AudioKeyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            position = EditorGUI.PrefixLabel(position, label);
            Rect previewRect = new Rect(position.x, position.y, 26f, position.height);
            Rect fieldRect = new Rect(position.x + 30f, position.y, position.width - 30f, position.height);

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(property.stringValue)))
            {
                if (GUI.Button(previewRect, "▶"))
                {
                    var registry = AudioEditorRegistryUtility.FindRegistry();
                    if (registry != null && registry.TryGet(property.stringValue, out var sound))
                        AudioEditorPreview.Play(sound);
                }
            }

            if (GUI.Button(fieldRect, string.IsNullOrWhiteSpace(property.stringValue) ? "<обрати звук>" : property.stringValue, EditorStyles.popup))
                PopupWindow.Show(fieldRect, new AudioKeyPopup(property.serializedObject.targetObject, property.propertyPath));
        }
    }

    internal sealed class AudioKeyPopup : PopupWindowContent
    {
        private readonly UnityEngine.Object _target;
        private readonly string _propertyPath;
        private string _search = string.Empty;
        private Vector2 _scroll;

        public AudioKeyPopup(UnityEngine.Object target, string propertyPath)
        {
            _target = target;
            _propertyPath = propertyPath;
        }

        public override Vector2 GetWindowSize()
            => new Vector2(420f, 420f);

        public override void OnGUI(Rect rect)
        {
            var registry = AudioEditorRegistryUtility.FindRegistry();
            if (registry == null)
            {
                EditorGUILayout.HelpBox("AudioRegistry не знайдено.", MessageType.Info);
                if (GUILayout.Button("Створити registry"))
                    AudioEditorRegistryUtility.GetOrCreateDefaultRegistry();
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _search = EditorGUILayout.TextField(_search, GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.textField);
                if (GUILayout.Button("Stop", GUILayout.Width(52f)))
                    AudioEditorPreview.Stop();
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            var sounds = AudioEditorRegistryUtility.GetSounds(registry);
            for (int i = 0; i < sounds.Count; i++)
            {
                var sound = sounds[i];
                if (!Matches(sound))
                    continue;

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("▶", GUILayout.Width(28f)))
                        AudioEditorPreview.Play(sound);

                    if (GUILayout.Button(sound.Key, EditorStyles.label))
                    {
                        Assign(sound.Key);
                        editorWindow.Close();
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(sound.Bus.ToString(), EditorStyles.miniLabel, GUILayout.Width(64f));
                }
            }
            EditorGUILayout.EndScrollView();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Clear"))
                {
                    Assign(string.Empty);
                    editorWindow.Close();
                }

                if (GUILayout.Button("Open Designer"))
                {
                    AudioDesignerWindow.Open();
                    editorWindow.Close();
                }
            }
        }

        private bool Matches(AudioSoundDefinition sound)
        {
            if (sound == null)
                return false;

            if (string.IsNullOrWhiteSpace(_search))
                return true;

            string needle = _search.Trim();
            return (sound.Key != null && sound.Key.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                   || (sound.Clip != null && sound.Clip.name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                   || sound.Bus.ToString().IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void Assign(string key)
        {
            if (_target == null)
                return;

            var serializedObject = new SerializedObject(_target);
            var property = serializedObject.FindProperty(_propertyPath);
            if (property == null || property.propertyType != SerializedPropertyType.String)
                return;

            property.stringValue = key;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_target);
        }
    }
}
