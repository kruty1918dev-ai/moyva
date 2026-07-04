using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Units.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Editor
{
    internal sealed class FogVisionTuningTool
    {
        private static readonly PresetDefinition[] Presets =
        {
            new PresetDefinition("01 Default", "Базовий збалансований профіль для поточної формули видимості.", 5, 1, 12, 0.15f, 1, 1, 1, 4, 2, 6, 0.02f),
            new PresetDefinition("02 Flat Map", "Майже рівна мапа: слабкий вплив висоти, м'яка оклюзія.", 5, 1, 11, 0.35f, 0, 0, 0, 0, 0, 1, 0.08f),
            new PresetDefinition("03 Gentle Hills", "Помірні пагорби, невеликі бонуси з висоти.", 5, 1, 12, 0.22f, 1, 1, 1, 2, 1, 3, 0.04f),
            new PresetDefinition("04 Rugged Hills", "Рельєф відчувається сильніше, але без різких штрафів.", 5, 1, 12, 0.16f, 1, 1, 1, 4, 2, 4, 0.03f),
            new PresetDefinition("05 Mountain Warfare", "Гірський профіль: висота критично впливає на дальність і LOS.", 4, 1, 13, 0.1f, 2, 2, 2, 5, 4, 7, 0.01f),
            new PresetDefinition("06 Scout Friendly", "Підходить для активної розвідки з помітним бонусом від висоти.", 6, 1, 14, 0.14f, 2, 2, 1, 5, 3, 4, 0.025f),
            new PresetDefinition("07 Defensive Ridge", "Оборона пагорбів: зверху видно далі, знизу штурм складніший.", 5, 1, 12, 0.12f, 2, 1, 2, 5, 2, 6, 0.02f),
            new PresetDefinition("08 Harsh Uphill", "Суворий penalty для погляду вгору, корисно для вузьких проходів і висот.", 5, 1, 12, 0.15f, 1, 1, 3, 4, 2, 8, 0.015f),
            new PresetDefinition("09 Long Sightlines", "Відкритий простір: вища максимальна дальність і слабше блокування.", 6, 1, 16, 0.25f, 1, 2, 1, 3, 4, 3, 0.06f),
            new PresetDefinition("10 Close Quarters", "Компактна видимість для щільних мап та частих засідок.", 4, 1, 9, 0.14f, 1, 0, 2, 2, 0, 5, 0.02f)
        };

        private static readonly GUIContent SettingsAssetLabel = new GUIContent(
            "FogOfWarSettings",
            "ScriptableObject з глобальними параметрами Fog of War та Height Vision.");

        private static readonly GUIContent UnitRegistryAssetLabel = new GUIContent(
            "UnitRegistry",
            "Реєстр класів юнітів. Тут редагується VisionRange для кожного UnitClassConfig.");

        private static readonly GUIContent DefaultVisionRangeLabel = new GUIContent(
            "Default Vision Range",
            "Fallback радіус, якщо юніт не передав власний VisionRange.");

        private static readonly GUIContent MinVisionRangeLabel = new GUIContent(
            "Min Vision Range",
            "Глобальний нижній ліміт радіуса видимості. Рекомендовано залишати 1.");

        private static readonly GUIContent MaxVisionRangeLabel = new GUIContent(
            "Max Vision Range",
            "Глобальна верхня межа ефективного радіуса після всіх бонусів/штрафів.");

        private static readonly GUIContent ElevationStepLabel = new GUIContent(
            "Elevation Step",
            "Крок різниці висоти для нарахування бонусу або штрафу. Менше значення = вища чутливість.");

        private static readonly GUIContent ObserverBonusPerStepLabel = new GUIContent(
            "Observer Height Bonus / Step",
            "Скільки радіуса додається за кожен крок висоти тайлу спостерігача.");

        private static readonly GUIContent DownhillBonusPerStepLabel = new GUIContent(
            "Downhill Bonus / Step",
            "Додатковий бонус за погляд вниз, коли ціль нижче спостерігача.");

        private static readonly GUIContent UphillPenaltyPerStepLabel = new GUIContent(
            "Uphill Penalty / Step",
            "Штраф за погляд вгору, коли ціль вище спостерігача.");

        private static readonly GUIContent MaxObserverBonusLabel = new GUIContent(
            "Max Observer Bonus",
            "Максимальний бонус від висоти самого спостерігача.");

        private static readonly GUIContent MaxDownhillBonusLabel = new GUIContent(
            "Max Downhill Bonus",
            "Максимальний бонус за видимість у напрямку вниз по схилу.");

        private static readonly GUIContent MaxUphillPenaltyLabel = new GUIContent(
            "Max Uphill Penalty",
            "Максимальний штраф за видимість у напрямку вгору по схилу.");

        private static readonly GUIContent OcclusionSlopeBiasLabel = new GUIContent(
            "Occlusion Slope Bias",
            "Допуск для line of sight. Більше значення зменшує агресивність блокування дрібними перепадами рельєфу.");

        private static readonly GUIContent PresetLabel = new GUIContent(
            "Preset",
            "Один із готових профілів для швидкого старту. Після застосування можна вручну редагувати будь-які значення.");

        private static readonly GUIContent UnitTypeLabel = new GUIContent(
            "Type",
            "Ідентифікатор типу юніта з UnitClassConfig.TypeId.");

        private static readonly GUIContent UnitVisionLabel = new GUIContent(
            "Vision",
            "Базовий радіус огляду конкретного типу юніта. Мінімум 1.");

        private static readonly GUIContent PreviewBaseRangeLabel = new GUIContent(
            "Base Vision Range",
            "Базова дальність до застосування height-бонусів, штрафів і clamp.");

        private static readonly GUIContent PreviewDistanceLabel = new GUIContent(
            "Target Distance (Chebyshev)",
            "Відстань до цілі у метриці Chebyshev (max(|dx|, |dy|)).");

        private static readonly GUIContent PreviewObserverHeightLabel = new GUIContent(
            "Observer Height",
            "Висота тайлу спостерігача у шкалі HeightMap.");

        private static readonly GUIContent PreviewTargetHeightLabel = new GUIContent(
            "Target Height",
            "Висота тайлу цілі у шкалі HeightMap.");

        private static readonly GUIContent PreviewUseBlockerLabel = new GUIContent(
            "Use Blocker Sample",
            "Увімкнути перевірку line-of-sight через проміжний зразок рельєфу.");

        private static readonly GUIContent PreviewBlockerHeightLabel = new GUIContent(
            "Blocker Height",
            "Висота проміжної точки на промені між спостерігачем і ціллю.");

        private static readonly GUIContent PreviewBlockerDistanceLabel = new GUIContent(
            "Blocker Distance",
            "Відстань до проміжної точки від спостерігача (має бути меншою за відстань до цілі).");

        private FogOfWarSettings _settings;
        private UnitRegistrySO _unitRegistry;
        private DesignerPresetLibrarySO _designerPresetLibrary;

        private SerializedObject _settingsSo;
        private SerializedObject _unitRegistrySo;

        private Vector2 _scroll;
        private int _selectedPresetIndex;
        private int _selectedDesignerFogPresetIndex;

        private int _previewBaseRange = 5;
        private int _previewDistance = 4;
        private float _previewObserverHeight = 0.4f;
        private float _previewTargetHeight = 0.55f;
        private bool _previewUseBlocker;
        private float _previewBlockerHeight = 0.8f;
        private int _previewBlockerDistance = 2;

        public void Initialize()
        {
            if (_settings == null)
                _settings = MoyvaProjectEditorContext.GetOrFindFirst<FogOfWarSettings>();

            if (_unitRegistry == null)
                _unitRegistry = MoyvaProjectEditorContext.GetOrFindFirst<UnitRegistrySO>();

            _designerPresetLibrary ??= MoyvaProjectEditorContext.GetOrFindFirst<DesignerPresetLibrarySO>();

            RebuildSerializedObjects();
        }

        public void Draw()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Fog Of War Vision Tuner", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Інструмент для підбору параметрів висотно-залежного зору (FogOfWarSettings) та VisionRange у UnitRegistrySO.",
                MessageType.Info);

            DrawAssetReferences();

            EditorGUILayout.Space();
            DrawFogSettingsSection();

            EditorGUILayout.Space();
            DrawUnitVisionSection();

            EditorGUILayout.Space();
            DrawPreviewSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawAssetReferences()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _settings = (FogOfWarSettings)EditorGUILayout.ObjectField(SettingsAssetLabel, _settings, typeof(FogOfWarSettings), false);
            _unitRegistry = (UnitRegistrySO)EditorGUILayout.ObjectField(UnitRegistryAssetLabel, _unitRegistry, typeof(UnitRegistrySO), false);
            if (EditorGUI.EndChangeCheck())
            {
                MoyvaProjectEditorContext.Set(_settings);
                MoyvaProjectEditorContext.Set(_unitRegistry);
                RebuildSerializedObjects();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Auto Find"))
            {
                if (_settings == null)
                    _settings = MoyvaProjectEditorContext.GetOrFindFirst<FogOfWarSettings>();

                if (_unitRegistry == null)
                    _unitRegistry = MoyvaProjectEditorContext.GetOrFindFirst<UnitRegistrySO>();

                RebuildSerializedObjects();
            }

            using (new EditorGUI.DisabledScope(_settings == null))
            {
                if (GUILayout.Button("Ping Settings"))
                    EditorGUIUtility.PingObject(_settings);
            }

            using (new EditorGUI.DisabledScope(_unitRegistry == null))
            {
                if (GUILayout.Button("Ping Unit Registry"))
                    EditorGUIUtility.PingObject(_unitRegistry);
            }

            EditorGUILayout.EndHorizontal();

            if (_settings == null)
                EditorGUILayout.HelpBox("FogOfWarSettings не знайдено. Створи через Create -> Moyva -> FogOfWarSettings.", MessageType.Warning);

            if (_unitRegistry == null)
                EditorGUILayout.HelpBox("UnitRegistrySO не знайдено. Признач існуючий реєстр юнітів.", MessageType.Warning);

            EditorGUILayout.EndVertical();
        }

        private void DrawFogSettingsSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("FogOfWarSettings", EditorStyles.boldLabel);

            if (_settingsSo == null)
            {
                EditorGUILayout.HelpBox("Признач FogOfWarSettings, щоб редагувати параметри.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            _settingsSo.Update();

            DrawPresetToolbar();
            DrawDesignerPresetToolbar();

            DrawIntProperty("DefaultVisionRange", DefaultVisionRangeLabel, 1);
            DrawIntProperty("MinVisionRange", MinVisionRangeLabel, 1);
            DrawIntProperty("MaxVisionRange", MaxVisionRangeLabel, 1);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Height Vision", EditorStyles.miniBoldLabel);

            DrawFloatProperty("ElevationStep", ElevationStepLabel, 0.01f);
            DrawIntProperty("ObserverHeightBonusPerStep", ObserverBonusPerStepLabel, 0);
            DrawIntProperty("DownhillVisionBonusPerStep", DownhillBonusPerStepLabel, 0);
            DrawIntProperty("UphillVisionPenaltyPerStep", UphillPenaltyPerStepLabel, 0);
            DrawIntProperty("MaxObserverHeightBonus", MaxObserverBonusLabel, 0);
            DrawIntProperty("MaxDownhillVisionBonus", MaxDownhillBonusLabel, 0);
            DrawIntProperty("MaxUphillVisionPenalty", MaxUphillPenaltyLabel, 0);
            DrawFloatProperty("OcclusionSlopeBias", OcclusionSlopeBiasLabel, 0f);

            if (_settingsSo.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_settings);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPresetToolbar()
        {
            string[] presetNames = new string[Presets.Length];
            for (int i = 0; i < Presets.Length; i++)
                presetNames[i] = Presets[i].Name;

            _selectedPresetIndex = EditorGUILayout.Popup(PresetLabel, Mathf.Clamp(_selectedPresetIndex, 0, Presets.Length - 1), presetNames);

            PresetDefinition preset = Presets[_selectedPresetIndex];
            EditorGUILayout.HelpBox(preset.Description, MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Preset"))
                ApplyPreset(preset);

            if (GUILayout.Button("Reset Settings"))
                ResetSettingsToDefaults();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4f);
        }

        private void DrawDesignerPresetToolbar()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Designer Presets", EditorStyles.miniBoldLabel);

            EditorGUI.BeginChangeCheck();
            _designerPresetLibrary = (DesignerPresetLibrarySO)EditorGUILayout.ObjectField(
                new GUIContent("Preset Library"),
                _designerPresetLibrary,
                typeof(DesignerPresetLibrarySO),
                false);
            if (EditorGUI.EndChangeCheck())
                MoyvaProjectEditorContext.Set(_designerPresetLibrary);

            if (_designerPresetLibrary == null)
            {
                EditorGUILayout.HelpBox("Призначте DesignerPresetLibrarySO, щоб застосовувати fog preset-и.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            var presets = _designerPresetLibrary.FogPresets;
            if (presets == null || presets.Count == 0)
            {
                EditorGUILayout.HelpBox("У бібліотеці немає Fog preset-ів.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            var names = new string[presets.Count];
            for (int i = 0; i < presets.Count; i++)
            {
                string presetName = presets[i] != null ? presets[i].Name : string.Empty;
                names[i] = string.IsNullOrWhiteSpace(presetName) ? $"Fog Preset {i + 1}" : presetName;
            }

            _selectedDesignerFogPresetIndex = Mathf.Clamp(_selectedDesignerFogPresetIndex, 0, presets.Count - 1);
            _selectedDesignerFogPresetIndex = EditorGUILayout.Popup("Library Preset", _selectedDesignerFogPresetIndex, names);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_settings == null))
                {
                    if (GUILayout.Button("Apply Library Preset"))
                    {
                        var preset = presets[_selectedDesignerFogPresetIndex];
                        if (preset == null || preset.Template == null)
                        {
                            EditorUtility.DisplayDialog("Fog Preset", "Обраний preset порожній.", "OK");
                        }
                        else
                        {
                            Undo.RecordObject(_settings, $"Apply Fog Preset: {preset.Name}");
                            if (DesignerPresetApplier.ApplyFogPreset(preset, _settings))
                            {
                                EditorUtility.SetDirty(_settings);
                                RebuildSerializedObjects();
                            }
                        }
                    }
                }

                if (GUILayout.Button("Ping Library"))
                    EditorGUIUtility.PingObject(_designerPresetLibrary);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4f);
        }

        private void DrawUnitVisionSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Unit Vision Ranges", EditorStyles.boldLabel);

            if (_unitRegistrySo == null)
            {
                EditorGUILayout.HelpBox("Признач UnitRegistrySO, щоб редагувати VisionRange по класах юнітів.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            _unitRegistrySo.Update();

            var configs = _unitRegistrySo.FindProperty("Configs");
            if (configs == null)
            {
                EditorGUILayout.HelpBox("Не знайдено поле Configs у UnitRegistrySO.", MessageType.Error);
                EditorGUILayout.EndVertical();
                return;
            }

            if (configs.arraySize == 0)
            {
                EditorGUILayout.HelpBox("UnitRegistrySO не містить жодної конфігурації.", MessageType.Info);
            }

            for (int i = 0; i < configs.arraySize; i++)
            {
                var element = configs.GetArrayElementAtIndex(i);
                if (element == null)
                    continue;

                var typeId = element.FindPropertyRelative("TypeId");
                var visionRange = element.FindPropertyRelative("VisionRange");

                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField(UnitTypeLabel, typeId != null ? typeId.stringValue : "<unknown>");
                }

                int rawValue = visionRange != null ? visionRange.intValue : 1;
                int newValue = EditorGUILayout.IntField(UnitVisionLabel, Mathf.Max(1, rawValue));
                if (visionRange != null)
                    visionRange.intValue = Mathf.Max(1, newValue);

                EditorGUILayout.EndHorizontal();

                if (typeId != null && !string.IsNullOrEmpty(typeId.stringValue) && typeId.stringValue.Contains("_"))
                {
                    EditorGUILayout.HelpBox(
                        "TypeId містить '_' (нижнє підкреслення). Для unit type id рекомендується дефіс або camelCase.",
                        MessageType.Warning);
                }
            }

            if (_unitRegistrySo.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_unitRegistry);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Height Vision Preview", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Швидка перевірка формул effective range та line-of-sight без запуску Play Mode.",
                MessageType.None);

            _previewBaseRange = Mathf.Max(1, EditorGUILayout.IntField(PreviewBaseRangeLabel, _previewBaseRange));
            _previewDistance = Mathf.Max(1, EditorGUILayout.IntField(PreviewDistanceLabel, _previewDistance));
            _previewObserverHeight = EditorGUILayout.FloatField(PreviewObserverHeightLabel, _previewObserverHeight);
            _previewTargetHeight = EditorGUILayout.FloatField(PreviewTargetHeightLabel, _previewTargetHeight);

            _previewUseBlocker = EditorGUILayout.Toggle(PreviewUseBlockerLabel, _previewUseBlocker);
            if (_previewUseBlocker)
            {
                _previewBlockerHeight = EditorGUILayout.FloatField(PreviewBlockerHeightLabel, _previewBlockerHeight);
                int maxBlockerDistance = Mathf.Max(1, _previewDistance - 1);
                _previewBlockerDistance = Mathf.Clamp(
                    EditorGUILayout.IntField(PreviewBlockerDistanceLabel, _previewBlockerDistance),
                    1,
                    maxBlockerDistance);

                if (_previewDistance <= 1)
                {
                    EditorGUILayout.HelpBox("Для blocker sample дистанція до цілі має бути > 1.", MessageType.Warning);
                }
            }

            var preview = FogVisionPreviewCalculator.Compute(new FogVisionPreviewCalculator.Input(
                _previewBaseRange,
                _previewDistance,
                _previewObserverHeight,
                _previewTargetHeight,
                _previewUseBlocker,
                _previewBlockerHeight,
                _previewBlockerDistance), new FogVisionPreviewCalculator.Settings(
                GetSettingFloat("ElevationStep", 0.15f, 0.01f),
                Mathf.Max(1, GetSettingInt("MaxVisionRange", 12, 1)),
                GetSettingInt("ObserverHeightBonusPerStep", 1, 0),
                GetSettingInt("DownhillVisionBonusPerStep", 1, 0),
                GetSettingInt("UphillVisionPenaltyPerStep", 1, 0),
                GetSettingInt("MaxObserverHeightBonus", 4, 0),
                GetSettingInt("MaxDownhillVisionBonus", 2, 0),
                GetSettingInt("MaxUphillVisionPenalty", 6, 0),
                GetSettingFloat("OcclusionSlopeBias", 0.02f, 0f)));

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Observer Bonus", preview.ObserverBonus.ToString());
            EditorGUILayout.LabelField("Downhill Bonus", preview.DownhillBonus.ToString());
            EditorGUILayout.LabelField("Uphill Penalty", preview.UphillPenalty.ToString());
            EditorGUILayout.LabelField("Effective Range", preview.EffectiveRange.ToString());
            EditorGUILayout.LabelField("In Range", preview.InRange ? "true" : "false");
            EditorGUILayout.LabelField("Occluded", preview.Occluded ? "true" : "false");

            bool visible = preview.InRange && !preview.Occluded;
            EditorGUILayout.LabelField("Visible", visible ? "true" : "false");

            if (visible)
                EditorGUILayout.HelpBox("Ціль видима за поточними параметрами.", MessageType.Info);
            else
                EditorGUILayout.HelpBox("Ціль НЕ видима: перевір effective range, penalty або occlusion.", MessageType.Warning);

            EditorGUILayout.EndVertical();
        }

        private int GetSettingInt(string name, int fallback, int minValue)
        {
            if (_settingsSo == null)
                return Mathf.Max(minValue, fallback);

            var prop = _settingsSo.FindProperty(name);
            if (prop == null)
                return Mathf.Max(minValue, fallback);

            return Mathf.Max(minValue, prop.intValue);
        }

        private float GetSettingFloat(string name, float fallback, float minValue)
        {
            if (_settingsSo == null)
                return Mathf.Max(minValue, fallback);

            var prop = _settingsSo.FindProperty(name);
            if (prop == null)
                return Mathf.Max(minValue, fallback);

            return Mathf.Max(minValue, prop.floatValue);
        }

        private void DrawIntProperty(string propertyName, GUIContent label, int minValue)
        {
            var prop = _settingsSo.FindProperty(propertyName);
            if (prop == null)
            {
                EditorGUILayout.HelpBox($"Не знайдено поле {propertyName}", MessageType.Error);
                return;
            }

            int newValue = EditorGUILayout.IntField(label, Mathf.Max(minValue, prop.intValue));
            prop.intValue = Mathf.Max(minValue, newValue);
        }

        private void DrawFloatProperty(string propertyName, GUIContent label, float minValue)
        {
            var prop = _settingsSo.FindProperty(propertyName);
            if (prop == null)
            {
                EditorGUILayout.HelpBox($"Не знайдено поле {propertyName}", MessageType.Error);
                return;
            }

            float newValue = EditorGUILayout.FloatField(label, Mathf.Max(minValue, prop.floatValue));
            prop.floatValue = Mathf.Max(minValue, newValue);
        }

        private void RebuildSerializedObjects()
        {
            _settingsSo = _settings != null ? new SerializedObject(_settings) : null;
            _unitRegistrySo = _unitRegistry != null ? new SerializedObject(_unitRegistry) : null;
        }

        private void ApplyPreset(PresetDefinition preset)
        {
            if (_settings == null || _settingsSo == null)
                return;

            Undo.RecordObject(_settings, $"Apply Fog Vision Preset: {preset.Name}");
            _settingsSo.Update();
            SetSettingsValues(preset);
            _settingsSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_settings);
        }

        private void ResetSettingsToDefaults()
        {
            ApplyPreset(Presets[0]);
        }

        private void SetSettingsValues(PresetDefinition preset)
        {
            SetIntProperty("DefaultVisionRange", preset.DefaultVisionRange);
            SetIntProperty("MinVisionRange", preset.MinVisionRange);
            SetIntProperty("MaxVisionRange", preset.MaxVisionRange);
            SetFloatProperty("ElevationStep", preset.ElevationStep);
            SetIntProperty("ObserverHeightBonusPerStep", preset.ObserverHeightBonusPerStep);
            SetIntProperty("DownhillVisionBonusPerStep", preset.DownhillVisionBonusPerStep);
            SetIntProperty("UphillVisionPenaltyPerStep", preset.UphillVisionPenaltyPerStep);
            SetIntProperty("MaxObserverHeightBonus", preset.MaxObserverHeightBonus);
            SetIntProperty("MaxDownhillVisionBonus", preset.MaxDownhillVisionBonus);
            SetIntProperty("MaxUphillVisionPenalty", preset.MaxUphillVisionPenalty);
            SetFloatProperty("OcclusionSlopeBias", preset.OcclusionSlopeBias);
        }

        private void SetIntProperty(string propertyName, int value)
        {
            var prop = _settingsSo.FindProperty(propertyName);
            if (prop != null)
                prop.intValue = value;
        }

        private void SetFloatProperty(string propertyName, float value)
        {
            var prop = _settingsSo.FindProperty(propertyName);
            if (prop != null)
                prop.floatValue = value;
        }

        private readonly struct PresetDefinition
        {
            public readonly string Name;
            public readonly string Description;
            public readonly int DefaultVisionRange;
            public readonly int MinVisionRange;
            public readonly int MaxVisionRange;
            public readonly float ElevationStep;
            public readonly int ObserverHeightBonusPerStep;
            public readonly int DownhillVisionBonusPerStep;
            public readonly int UphillVisionPenaltyPerStep;
            public readonly int MaxObserverHeightBonus;
            public readonly int MaxDownhillVisionBonus;
            public readonly int MaxUphillVisionPenalty;
            public readonly float OcclusionSlopeBias;

            public PresetDefinition(
                string name,
                string description,
                int defaultVisionRange,
                int minVisionRange,
                int maxVisionRange,
                float elevationStep,
                int observerHeightBonusPerStep,
                int downhillVisionBonusPerStep,
                int uphillVisionPenaltyPerStep,
                int maxObserverHeightBonus,
                int maxDownhillVisionBonus,
                int maxUphillVisionPenalty,
                float occlusionSlopeBias)
            {
                Name = name;
                Description = description;
                DefaultVisionRange = defaultVisionRange;
                MinVisionRange = minVisionRange;
                MaxVisionRange = maxVisionRange;
                ElevationStep = elevationStep;
                ObserverHeightBonusPerStep = observerHeightBonusPerStep;
                DownhillVisionBonusPerStep = downhillVisionBonusPerStep;
                UphillVisionPenaltyPerStep = uphillVisionPenaltyPerStep;
                MaxObserverHeightBonus = maxObserverHeightBonus;
                MaxDownhillVisionBonus = maxDownhillVisionBonus;
                MaxUphillVisionPenalty = maxUphillVisionPenalty;
                OcclusionSlopeBias = occlusionSlopeBias;
            }
        }
    }
}
