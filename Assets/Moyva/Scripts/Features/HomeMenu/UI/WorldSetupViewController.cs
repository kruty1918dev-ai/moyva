using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.WorldCreation.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контролер панелі налаштування світу: ім'я, seed (з генератором), розмір, тип мапи, складність.
    /// </summary>
    public class WorldSetupViewController : MonoBehaviour, IWorldSetupViewController, IInitializable
    {
        [Header("Inputs")]
        [SerializeField] private TMP_InputField _worldNameInput;
        [SerializeField] private TMP_InputField _seedInput;
        [SerializeField] private Button _randomSeedButton;

        [Header("Dropdowns")]
        [SerializeField] private TMP_Dropdown _sizeDropdown;
        [SerializeField] private TMP_Dropdown _mapTypeDropdown;
        [SerializeField] private TMP_Dropdown _difficultyDropdown;

        [Header("Actions")]
        [SerializeField] private Button _nextButton;

        public string WorldName
        {
            get => _worldNameInput != null ? _worldNameInput.text : string.Empty;
            set
            {
                if (_worldNameInput == null) return;
                _worldNameInput.SetTextWithoutNotify(value ?? string.Empty);
                NotifySettingsChanged();
            }
        }

        public int Seed
        {
            get => int.TryParse(_seedInput != null ? _seedInput.text : string.Empty, out var v) ? v : 0;
            set
            {
                if (_seedInput == null) return;
                _seedInput.SetTextWithoutNotify(value.ToString());
                NotifySettingsChanged();
            }
        }

        public WorldSize Size
        {
            get => _sizeDropdown != null ? (WorldSize)_sizeDropdown.value : WorldSize.Medium;
            set
            {
                if (_sizeDropdown == null) return;
                _sizeDropdown.SetValueWithoutNotify((int)value);
                NotifySettingsChanged();
            }
        }

        public MapType MapType
        {
            get => _mapTypeDropdown != null ? (MapType)_mapTypeDropdown.value : MapType.Continents;
            set
            {
                if (_mapTypeDropdown == null) return;
                _mapTypeDropdown.SetValueWithoutNotify((int)value);
                NotifySettingsChanged();
            }
        }

        public Difficulty Difficulty
        {
            get => _difficultyDropdown != null ? (Difficulty)_difficultyDropdown.value : Difficulty.Normal;
            set
            {
                if (_difficultyDropdown == null) return;
                _difficultyDropdown.SetValueWithoutNotify((int)value);
                NotifySettingsChanged();
            }
        }

        public Button CreateWorldButton => _nextButton;

        public event Action OnButtonNextClicked;
        public event Action OnRandomSeedClicked;
        public event Action OnSettingsChanged;

        private UnityAction _onNextClicked;
        private UnityAction _onRandomClicked;
        private UnityAction<string> _onTextChanged;
        private UnityAction<int> _onDropdownChanged;

        public void Initialize() { EnsureRuntimeControls(); AttachListeners(); PopulateDropdowns(); }
        private void Awake() { EnsureRuntimeControls(); AttachListeners(); PopulateDropdowns(); }

        private void AttachListeners()
        {
            if (_onNextClicked == null) _onNextClicked = () => OnButtonNextClicked?.Invoke();
            if (_onRandomClicked == null) _onRandomClicked = HandleRandomSeed;
            if (_onTextChanged == null) _onTextChanged = _ => NotifySettingsChanged();
            if (_onDropdownChanged == null) _onDropdownChanged = _ => NotifySettingsChanged();

            if (_nextButton != null)
            {
                _nextButton.onClick.RemoveListener(_onNextClicked);
                _nextButton.onClick.AddListener(_onNextClicked);
            }

            if (_randomSeedButton != null)
            {
                _randomSeedButton.onClick.RemoveListener(_onRandomClicked);
                _randomSeedButton.onClick.AddListener(_onRandomClicked);
            }

            AttachValueListeners();
        }

        private void AttachValueListeners()
        {
            AttachTextListener(_worldNameInput);
            AttachTextListener(_seedInput);
            AttachDropdownListener(_sizeDropdown);
            AttachDropdownListener(_mapTypeDropdown);
            AttachDropdownListener(_difficultyDropdown);
        }

        private void AttachTextListener(TMP_InputField input)
        {
            if (input == null) return;
            input.onValueChanged.RemoveListener(_onTextChanged);
            input.onValueChanged.AddListener(_onTextChanged);
        }

        private void AttachDropdownListener(TMP_Dropdown dropdown)
        {
            if (dropdown == null) return;
            dropdown.onValueChanged.RemoveListener(_onDropdownChanged);
            dropdown.onValueChanged.AddListener(_onDropdownChanged);
        }

        /// <summary>Заповнити dropdown-и значеннями enum'ів. Безпечно викликати повторно.</summary>
        private void PopulateDropdowns()
        {
            FillEnum<WorldSize>(_sizeDropdown);
            FillEnum<MapType>(_mapTypeDropdown);
            FillEnum<Difficulty>(_difficultyDropdown);
        }

        private static void FillEnum<TEnum>(TMP_Dropdown dropdown) where TEnum : Enum
        {
            if (dropdown == null) return;
            dropdown.ClearOptions();
            var names = Enum.GetNames(typeof(TEnum));
            var options = new List<TMP_Dropdown.OptionData>(names.Length);
            foreach (var n in names) options.Add(new TMP_Dropdown.OptionData(n));
            dropdown.AddOptions(options);
        }

        private void HandleRandomSeed()
        {
            // Генеруємо випадковий seed і заповнюємо input.
            var newSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            Seed = newSeed;
            OnRandomSeedClicked?.Invoke();
        }

        private void OnDestroy()
        {
            if (_nextButton != null && _onNextClicked != null) _nextButton.onClick.RemoveListener(_onNextClicked);
            if (_randomSeedButton != null && _onRandomClicked != null) _randomSeedButton.onClick.RemoveListener(_onRandomClicked);
            if (_onTextChanged != null)
            {
                if (_worldNameInput != null) _worldNameInput.onValueChanged.RemoveListener(_onTextChanged);
                if (_seedInput != null) _seedInput.onValueChanged.RemoveListener(_onTextChanged);
            }
            if (_onDropdownChanged != null)
            {
                if (_sizeDropdown != null) _sizeDropdown.onValueChanged.RemoveListener(_onDropdownChanged);
                if (_mapTypeDropdown != null) _mapTypeDropdown.onValueChanged.RemoveListener(_onDropdownChanged);
                if (_difficultyDropdown != null) _difficultyDropdown.onValueChanged.RemoveListener(_onDropdownChanged);
            }
        }

        private void NotifySettingsChanged()
        {
            OnSettingsChanged?.Invoke();
        }

        private void EnsureRuntimeControls()
        {
            var parent = _sizeDropdown != null && _sizeDropdown.transform.parent != null ? _sizeDropdown.transform.parent : transform;
            _mapTypeDropdown ??= CloneDropdown(parent, _sizeDropdown, "Dropdown_MapType");
            _difficultyDropdown ??= CloneDropdown(parent, _sizeDropdown, "Dropdown_Difficulty");
            _randomSeedButton ??= HomeMenuRuntimeUiFactory.CreateButton(parent, "Button_RandomSeed", "Random", new Vector2(120f, 40f));
        }

        private static TMP_Dropdown CloneDropdown(Transform parent, TMP_Dropdown source, string name)
        {
            if (source == null)
                return null;

            var clone = Instantiate(source.gameObject, parent, false);
            clone.name = name;
            clone.SetActive(true);
            return clone.GetComponent<TMP_Dropdown>();
        }
    }
}
