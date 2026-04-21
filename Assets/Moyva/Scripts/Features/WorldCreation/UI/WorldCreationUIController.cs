using System;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.WorldCreation.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.WorldCreation.UI
{
    /// <summary>
    /// Головний контролер екрану створення / налаштування нового світу.
    /// Діє як адаптер між UI-елементами та <see cref="IWorldCreationService"/>.
    ///
    /// ФУНКЦІОНАЛЬНІСТЬ:
    /// — Заповнює всі поля з поточного <see cref="WorldCreationConfig"/>.
    /// — Синхронізує зміни в полях зі службою в реальному часі.
    /// — Генерує випадковий seed за натисканням кнопки.
    /// — Показує/ховає поля Custom-розміру залежно від вибраного пресету.
    /// — Валідує конфіг перед підтвердженням; відображає повідомлення про помилки.
    /// — Скидає всі поля до дефолтів.
    /// — Надсилає <see cref="WorldCreationConfirmedSignal"/> або <see cref="WorldCreationCancelledSignal"/>.
    ///
    /// ЯК ПІДКЛЮЧИТИ В UNITY:
    /// 1. Додай компонент до кореневого GameObject екрану WorldCreation.
    /// 2. Перетягни всі UI-елементи у відповідні поля Inspector.
    /// 3. Додай <see cref="WorldCreationUIInstaller"/> до SceneContext і призначи цей контролер.
    ///
    /// ЗАЛЕЖНОСТІ (Zenject):
    ///   - <c>IWorldCreationService</c>  (надається WorldCreationInstaller)
    ///   - <c>SignalBus</c>              (надається SignalBusInstaller)
    /// </summary>
    public sealed class WorldCreationUIController : MonoBehaviour, IInitializable, IDisposable
    {
        // ── Основні параметри ────────────────────────────────────────────
        [Header("Основні параметри")]
        [Tooltip("Поле вводу назви світу.")]
        [SerializeField] private TMP_InputField worldNameField;

        [Tooltip("Поле вводу seed генератора.")]
        [SerializeField] private TMP_InputField seedField;

        [Tooltip("Кнопка «Випадковий seed».")]
        [SerializeField] private Button randomSeedButton;

        [Tooltip("Випадаючий список розміру карти (Small / Medium / Large / Custom).")]
        [SerializeField] private TMP_Dropdown sizePresetDropdown;

        [Header("Custom розмір (відображається тільки при вибраному Custom)")]
        [Tooltip("Контейнер полів Custom Width / Height. Приховується якщо не Custom.")]
        [SerializeField] private GameObject customSizeGroup;

        [Tooltip("Поле ширини (тільки для Custom).")]
        [SerializeField] private TMP_InputField customWidthField;

        [Tooltip("Поле висоти (тільки для Custom).")]
        [SerializeField] private TMP_InputField customHeightField;

        [Tooltip("Випадаючий список типу карти.")]
        [SerializeField] private TMP_Dropdown mapTypeDropdown;

        // ── Правила гри ──────────────────────────────────────────────────
        [Header("Правила гри")]
        [Tooltip("Випадаючий список складності.")]
        [SerializeField] private TMP_Dropdown difficultyDropdown;

        [Tooltip("Перемикач увімкнення ботів.")]
        [SerializeField] private Toggle enableBotsToggle;

        [Tooltip("Контейнер налаштувань ботів (ховається якщо боти вимкнені).")]
        [SerializeField] private GameObject botSettingsGroup;

        [Tooltip("Слайдер кількості людських гравців (1–4).")]
        [SerializeField] private Slider humanPlayerCountSlider;

        [Tooltip("Текст, що відображає поточне значення humanPlayerCountSlider.")]
        [SerializeField] private TMP_Text humanPlayerCountLabel;

        [Tooltip("Слайдер кількості ботів (0–4).")]
        [SerializeField] private Slider botCountSlider;

        [Tooltip("Текст, що відображає поточне значення botCountSlider.")]
        [SerializeField] private TMP_Text botCountLabel;

        [Tooltip("Поле вводу стартового золота.")]
        [SerializeField] private TMP_InputField startingGoldField;

        [Tooltip("Поле вводу стартової їжі.")]
        [SerializeField] private TMP_InputField startingFoodField;

        // ── Параметри генерації ──────────────────────────────────────────
        [Header("Параметри генерації")]
        [Tooltip("Слайдер щільності лісів [0..1].")]
        [SerializeField] private Slider forestDensitySlider;

        [Tooltip("Слайдер щільності гір [0..1].")]
        [SerializeField] private Slider mountainDensitySlider;

        [Tooltip("Слайдер щільності водних зон [0..1].")]
        [SerializeField] private Slider waterDensitySlider;

        [Tooltip("Слайдер щільності POI [0..1].")]
        [SerializeField] private Slider villageDensitySlider;

        [Tooltip("Перемикач генерації річок.")]
        [SerializeField] private Toggle generateRiversToggle;

        [Tooltip("Перемикач генерації біомів.")]
        [SerializeField] private Toggle generateBiomesToggle;

        [Tooltip("Перемикач WFC-полірування тайлів.")]
        [SerializeField] private Toggle applyWFCToggle;

        // ── Дії ──────────────────────────────────────────────────────────
        [Header("Кнопки дій")]
        [Tooltip("Кнопка «Створити світ».")]
        [SerializeField] private Button createWorldButton;

        [Tooltip("Кнопка «Скасувати» (повернутись у головне меню).")]
        [SerializeField] private Button cancelButton;

        [Tooltip("Кнопка «Скинути до стандартних».")]
        [SerializeField] private Button resetDefaultsButton;

        [Header("Зворотній зв'язок")]
        [Tooltip("Текст для відображення помилок валідації.")]
        [SerializeField] private TMP_Text validationErrorText;

        // ── Zenject ──────────────────────────────────────────────────────
        private IWorldCreationService _service;
        private SignalBus _signalBus;

        // ── Стан ─────────────────────────────────────────────────────────
        /// <summary>Блокує запис у сервіс поки UI заповнюється програмно.</summary>
        private bool _isPopulating;

        // ────────────────────────────────────────────────────────────────
        // Zenject
        // ────────────────────────────────────────────────────────────────

        /// <summary>Точка ін'єкції Zenject. Не викликати вручну.</summary>
        [Inject]
        public void Construct(IWorldCreationService service, SignalBus signalBus)
        {
            _service  = service;
            _signalBus = signalBus;
        }

        /// <summary>Викликається Zenject після ін'єкції. Підписує кнопки та заповнює UI.</summary>
        public void Initialize()
        {
            if (_service == null || _signalBus == null)
            {
                Debug.LogError("[WorldCreationUIController] Zenject не ін'єктував усі залежності. " +
                               "Перевір SceneContext installers для WorldCreation та Signals.", this);
                return;
            }

            AutoWireMissingReferences();

            BindButtons();
            BindInputs();
            PopulateFromConfig(_service.CurrentConfig);
        }

        private void AutoWireMissingReferences()
        {
            worldNameField ??= FindByName<TMP_InputField>("Input_W_Name");
            seedField ??= FindByName<TMP_InputField>("Input_W_Seed");
            randomSeedButton ??= FindByName<Button>("NextBtn");
            sizePresetDropdown ??= FindByName<TMP_Dropdown>("DD_W_Size");
            customSizeGroup ??= FindByName<Transform>("Row_W_CustomSize")?.gameObject;
            customWidthField ??= FindByName<TMP_InputField>("Input_W_Width");
            customHeightField ??= FindByName<TMP_InputField>("Input_W_Height");
            mapTypeDropdown ??= FindByName<TMP_Dropdown>("DD_W_MapType");
            difficultyDropdown ??= FindByName<TMP_Dropdown>("DD_W_Difficulty");

            enableBotsToggle ??= FindToggleInRow("Row_W_Bots");
            botSettingsGroup ??= FindByName<Transform>("Row_W_BotSettings")?.gameObject;
            humanPlayerCountSlider ??= FindByName<Slider>("Slider_W_Humans");
            humanPlayerCountLabel ??= FindValueLabelFor("Slider_W_Humans");
            botCountSlider ??= FindByName<Slider>("Slider_W_Bots");
            botCountLabel ??= FindValueLabelFor("Slider_W_Bots");

            startingGoldField ??= FindByName<TMP_InputField>("Input_W_Gold");
            startingFoodField ??= FindByName<TMP_InputField>("Input_W_Food");
            forestDensitySlider ??= FindByName<Slider>("Slider_W_Forest");
            mountainDensitySlider ??= FindByName<Slider>("Slider_W_Mountain");
            waterDensitySlider ??= FindByName<Slider>("Slider_W_Water");
            villageDensitySlider ??= FindByName<Slider>("Slider_W_Village");

            generateRiversToggle ??= FindToggleInRow("Row_W_Rivers");
            generateBiomesToggle ??= FindToggleInRow("Row_W_Biomes");
            applyWFCToggle ??= FindToggleInRow("Row_W_WFC");

            createWorldButton ??= FindByName<Button>("StartBtn");
            cancelButton ??= FindByName<Button>("CancelBtn");
            resetDefaultsButton ??= FindByName<Button>("ResetDefaultsBtn");
            validationErrorText ??= FindByName<TMP_Text>("ValidationError");
        }

        private T FindByName<T>(string objectName) where T : Component
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return null;

            var root = transform;
            if (root.name == objectName)
                return root.GetComponent<T>();

            var allChildren = root.GetComponentsInChildren<Transform>(true);
            foreach (var child in allChildren)
            {
                if (child.name != objectName)
                    continue;

                var component = child.GetComponent<T>();
                if (component != null)
                    return component;
            }

            return null;
        }

        private Toggle FindToggleInRow(string rowObjectName)
        {
            var row = FindByName<Transform>(rowObjectName);
            return row != null ? row.GetComponentInChildren<Toggle>(true) : null;
        }

        private TMP_Text FindValueLabelFor(string sliderObjectName)
        {
            var slider = FindByName<Slider>(sliderObjectName);
            if (slider == null)
                return null;

            var parent = slider.transform.parent;
            if (parent == null)
                return null;

            foreach (var text in parent.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text != null && text.name == "Val")
                    return text;
            }

            return null;
        }

        /// <summary>Викликається Zenject при знищенні. Відписує всі слухачі.</summary>
        public void Dispose()
        {
            if (createWorldButton  != null) createWorldButton.onClick.RemoveListener(OnCreateWorldClicked);
            if (cancelButton       != null) cancelButton.onClick.RemoveListener(OnCancelClicked);
            if (resetDefaultsButton!= null) resetDefaultsButton.onClick.RemoveListener(OnResetDefaultsClicked);
            if (randomSeedButton   != null) randomSeedButton.onClick.RemoveListener(OnRandomSeedClicked);

            UnbindInputs();
        }

        // ────────────────────────────────────────────────────────────────
        // Ініціалізація підписок
        // ────────────────────────────────────────────────────────────────

        private void BindButtons()
        {
            if (createWorldButton != null)
                createWorldButton.onClick.AddListener(OnCreateWorldClicked);
            else
                Debug.LogWarning("[WorldCreationUIController] 'createWorldButton' не призначено.", this);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
            else
                Debug.LogWarning("[WorldCreationUIController] 'cancelButton' не призначено.", this);

            if (resetDefaultsButton != null)
                resetDefaultsButton.onClick.AddListener(OnResetDefaultsClicked);
            else
                Debug.LogWarning("[WorldCreationUIController] 'resetDefaultsButton' не призначено.", this);

            if (randomSeedButton != null)
                randomSeedButton.onClick.AddListener(OnRandomSeedClicked);
            else
                Debug.LogWarning("[WorldCreationUIController] 'randomSeedButton' не призначено.", this);
        }

        private void BindInputs()
        {
            if (worldNameField      != null) worldNameField.onEndEdit.AddListener(_ => OnAnyFieldChanged());
            if (seedField           != null) seedField.onEndEdit.AddListener(_ => OnAnyFieldChanged());
            if (sizePresetDropdown  != null) sizePresetDropdown.onValueChanged.AddListener(_ => OnSizePresetChanged());
            if (customWidthField    != null) customWidthField.onEndEdit.AddListener(_ => OnAnyFieldChanged());
            if (customHeightField   != null) customHeightField.onEndEdit.AddListener(_ => OnAnyFieldChanged());
            if (mapTypeDropdown     != null) mapTypeDropdown.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            if (difficultyDropdown  != null) difficultyDropdown.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            if (enableBotsToggle    != null) enableBotsToggle.onValueChanged.AddListener(_ => OnEnableBotsChanged());
            if (humanPlayerCountSlider != null) humanPlayerCountSlider.onValueChanged.AddListener(OnHumanCountChanged);
            if (botCountSlider      != null) botCountSlider.onValueChanged.AddListener(OnBotCountChanged);
            if (startingGoldField   != null) startingGoldField.onEndEdit.AddListener(_ => OnAnyFieldChanged());
            if (startingFoodField   != null) startingFoodField.onEndEdit.AddListener(_ => OnAnyFieldChanged());
            if (forestDensitySlider    != null) forestDensitySlider.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            if (mountainDensitySlider  != null) mountainDensitySlider.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            if (waterDensitySlider     != null) waterDensitySlider.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            if (villageDensitySlider   != null) villageDensitySlider.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            if (generateRiversToggle   != null) generateRiversToggle.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            if (generateBiomesToggle   != null) generateBiomesToggle.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            if (applyWFCToggle         != null) applyWFCToggle.onValueChanged.AddListener(_ => OnAnyFieldChanged());
        }

        private void UnbindInputs()
        {
            if (worldNameField      != null) worldNameField.onEndEdit.RemoveAllListeners();
            if (seedField           != null) seedField.onEndEdit.RemoveAllListeners();
            if (sizePresetDropdown  != null) sizePresetDropdown.onValueChanged.RemoveAllListeners();
            if (customWidthField    != null) customWidthField.onEndEdit.RemoveAllListeners();
            if (customHeightField   != null) customHeightField.onEndEdit.RemoveAllListeners();
            if (mapTypeDropdown     != null) mapTypeDropdown.onValueChanged.RemoveAllListeners();
            if (difficultyDropdown  != null) difficultyDropdown.onValueChanged.RemoveAllListeners();
            if (enableBotsToggle    != null) enableBotsToggle.onValueChanged.RemoveAllListeners();
            if (humanPlayerCountSlider != null) humanPlayerCountSlider.onValueChanged.RemoveAllListeners();
            if (botCountSlider      != null) botCountSlider.onValueChanged.RemoveAllListeners();
            if (startingGoldField   != null) startingGoldField.onEndEdit.RemoveAllListeners();
            if (startingFoodField   != null) startingFoodField.onEndEdit.RemoveAllListeners();
            if (forestDensitySlider    != null) forestDensitySlider.onValueChanged.RemoveAllListeners();
            if (mountainDensitySlider  != null) mountainDensitySlider.onValueChanged.RemoveAllListeners();
            if (waterDensitySlider     != null) waterDensitySlider.onValueChanged.RemoveAllListeners();
            if (villageDensitySlider   != null) villageDensitySlider.onValueChanged.RemoveAllListeners();
            if (generateRiversToggle   != null) generateRiversToggle.onValueChanged.RemoveAllListeners();
            if (generateBiomesToggle   != null) generateBiomesToggle.onValueChanged.RemoveAllListeners();
            if (applyWFCToggle         != null) applyWFCToggle.onValueChanged.RemoveAllListeners();
        }

        // ────────────────────────────────────────────────────────────────
        // Заповнення UI з конфігу
        // ────────────────────────────────────────────────────────────────

        private void PopulateFromConfig(WorldCreationConfig cfg)
        {
            _isPopulating = true;

            SetText(worldNameField, cfg.WorldName);
            SetText(seedField,      cfg.Seed != 0 ? cfg.Seed.ToString() : string.Empty);
            SetDropdown(sizePresetDropdown,  (int)cfg.SizePreset);
            SetText(customWidthField,  cfg.CustomWidth.ToString());
            SetText(customHeightField, cfg.CustomHeight.ToString());
            SetDropdown(mapTypeDropdown, (int)cfg.MapType);
            SetDropdown(difficultyDropdown, (int)cfg.Difficulty);
            SetToggle(enableBotsToggle, cfg.EnableBots);
            SetSlider(humanPlayerCountSlider, cfg.HumanPlayerCount);
            SetSlider(botCountSlider, cfg.BotCount);
            SetText(startingGoldField, cfg.StartingGold.ToString());
            SetText(startingFoodField, cfg.StartingFood.ToString());
            SetSlider(forestDensitySlider,   cfg.ForestDensity);
            SetSlider(mountainDensitySlider, cfg.MountainDensity);
            SetSlider(waterDensitySlider,    cfg.WaterDensity);
            SetSlider(villageDensitySlider,  cfg.VillageDensity);
            SetToggle(generateRiversToggle,  cfg.GenerateRivers);
            SetToggle(generateBiomesToggle,  cfg.GenerateBiomes);
            SetToggle(applyWFCToggle,        cfg.ApplyWFC);

            RefreshSliderLabels(cfg.HumanPlayerCount, cfg.BotCount);
            RefreshCustomSizeVisibility(cfg.SizePreset);
            // RefreshBotSettingsVisibility(cfg.EnableBots);
            ClearValidationError();

            _isPopulating = false;
        }

        // ────────────────────────────────────────────────────────────────
        // Обробники змін полів
        // ────────────────────────────────────────────────────────────────

        private void OnAnyFieldChanged()
        {
            if (_isPopulating) return;
            _service.UpdateConfig(ReadConfigFromUI());
            ClearValidationError();
        }

        private void OnSizePresetChanged()
        {
            if (_isPopulating) return;
            var preset = (WorldSizePreset)(sizePresetDropdown != null ? sizePresetDropdown.value : 0);
            RefreshCustomSizeVisibility(preset);
            _service.UpdateConfig(ReadConfigFromUI());
            ClearValidationError();
        }

        private void OnEnableBotsChanged()
        {
            if (_isPopulating) return;
            bool enabled = enableBotsToggle != null && enableBotsToggle.isOn;
            RefreshBotSettingsVisibility(enabled);
            _service.UpdateConfig(ReadConfigFromUI());
            ClearValidationError();
        }

        private void OnHumanCountChanged(float value)
        {
            if (_isPopulating) return;
            if (humanPlayerCountLabel != null)
                humanPlayerCountLabel.text = Mathf.RoundToInt(value).ToString();
            _service.UpdateConfig(ReadConfigFromUI());
            ClearValidationError();
        }

        private void OnBotCountChanged(float value)
        {
            if (_isPopulating) return;
            if (botCountLabel != null)
                botCountLabel.text = Mathf.RoundToInt(value).ToString();
            _service.UpdateConfig(ReadConfigFromUI());
            ClearValidationError();
        }

        // ────────────────────────────────────────────────────────────────
        // Обробники кнопок
        // ────────────────────────────────────────────────────────────────

        private void OnRandomSeedClicked()
        {
            int seed = _service.GenerateRandomSeed();
            if (seedField != null)
                seedField.SetTextWithoutNotify(seed.ToString());
        }

        private void OnResetDefaultsClicked()
        {
            _service.ResetToDefaults();
            PopulateFromConfig(_service.CurrentConfig);
        }

        private void OnCreateWorldClicked()
        {
            var config = ReadConfigFromUI();
            _service.UpdateConfig(config);

            if (!_service.ValidateConfig(config, out string error))
            {
                ShowValidationError(error);
                return;
            }

            var signalData = _service.ToSignalData(config);
            _signalBus.Fire(new WorldCreationConfirmedSignal { Config = signalData });
        }

        private void OnCancelClicked()
        {
            _signalBus.Fire(new WorldCreationCancelledSignal());
        }

        // ────────────────────────────────────────────────────────────────
        // Зчитування конфігу з UI
        // ────────────────────────────────────────────────────────────────

        private WorldCreationConfig ReadConfigFromUI()
        {
            int.TryParse(seedField        != null ? seedField.text : "0",          out int seed);
            int.TryParse(customWidthField  != null ? customWidthField.text  : "64", out int cw);
            int.TryParse(customHeightField != null ? customHeightField.text : "64", out int ch);
            int.TryParse(startingGoldField != null ? startingGoldField.text : "0",  out int gold);
            int.TryParse(startingFoodField != null ? startingFoodField.text : "0",  out int food);

            return new WorldCreationConfig
            {
                WorldName        = worldNameField      != null ? worldNameField.text : string.Empty,
                Seed             = seed,
                SizePreset       = (WorldSizePreset)(sizePresetDropdown  != null ? sizePresetDropdown.value  : 1),
                CustomWidth      = Mathf.Max(16, cw),
                CustomHeight     = Mathf.Max(16, ch),
                MapType          = (MapTypePreset)(mapTypeDropdown     != null ? mapTypeDropdown.value     : 0),
                Difficulty       = (DifficultyLevel)(difficultyDropdown  != null ? difficultyDropdown.value  : 1),
                EnableBots       = enableBotsToggle          != null && enableBotsToggle.isOn,
                HumanPlayerCount = humanPlayerCountSlider    != null ? Mathf.RoundToInt(humanPlayerCountSlider.value) : 1,
                BotCount         = botCountSlider            != null ? Mathf.RoundToInt(botCountSlider.value)         : 0,
                StartingGold     = Mathf.Max(0, gold),
                StartingFood     = Mathf.Max(0, food),
                ForestDensity    = forestDensitySlider    != null ? forestDensitySlider.value    : 0.4f,
                MountainDensity  = mountainDensitySlider  != null ? mountainDensitySlider.value  : 0.3f,
                WaterDensity     = waterDensitySlider     != null ? waterDensitySlider.value     : 0.25f,
                VillageDensity   = villageDensitySlider   != null ? villageDensitySlider.value   : 0.2f,
                GenerateRivers   = generateRiversToggle   != null && generateRiversToggle.isOn,
                GenerateBiomes   = generateBiomesToggle   != null && generateBiomesToggle.isOn,
                ApplyWFC         = applyWFCToggle         != null && applyWFCToggle.isOn
            };
        }

        // ────────────────────────────────────────────────────────────────
        // Відображення помилок / підказок
        // ────────────────────────────────────────────────────────────────

        private void ShowValidationError(string message)
        {
            if (validationErrorText != null)
            {
                validationErrorText.text    = message;
                validationErrorText.enabled = true;
            }
        }

        private void ClearValidationError()
        {
            if (validationErrorText != null)
                validationErrorText.enabled = false;
        }

        // ────────────────────────────────────────────────────────────────
        // Відображення залежних груп
        // ────────────────────────────────────────────────────────────────

        private void RefreshCustomSizeVisibility(WorldSizePreset preset)
        {
            if (customSizeGroup != null)
                customSizeGroup.SetActive(preset == WorldSizePreset.Custom);
        }

        private void RefreshBotSettingsVisibility(bool botsEnabled)
        {
            if (botSettingsGroup != null)
                botSettingsGroup.SetActive(botsEnabled);
        }

        private void RefreshSliderLabels(int humanCount, int botCount)
        {
            if (humanPlayerCountLabel != null) humanPlayerCountLabel.text = humanCount.ToString();
            if (botCountLabel         != null) botCountLabel.text         = botCount.ToString();
        }

        // ────────────────────────────────────────────────────────────────
        // Null-safe хелпери
        // ────────────────────────────────────────────────────────────────

        private static void SetText(TMP_InputField field, string value)
        {
            if (field != null) field.SetTextWithoutNotify(value);
        }

        private static void SetDropdown(TMP_Dropdown dropdown, int index)
        {
            if (dropdown != null) dropdown.SetValueWithoutNotify(index);
        }

        private static void SetToggle(Toggle toggle, bool value)
        {
            if (toggle != null) toggle.SetIsOnWithoutNotify(value);
        }

        private static void SetSlider(Slider slider, float value)
        {
            if (slider != null) slider.SetValueWithoutNotify(value);
        }
    }
}
