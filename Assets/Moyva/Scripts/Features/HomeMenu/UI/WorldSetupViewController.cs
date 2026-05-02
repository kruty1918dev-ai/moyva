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
            set { if (_worldNameInput != null) _worldNameInput.SetTextWithoutNotify(value ?? string.Empty); }
        }

        public int Seed
        {
            get => int.TryParse(_seedInput != null ? _seedInput.text : string.Empty, out var v) ? v : 0;
            set { if (_seedInput != null) _seedInput.SetTextWithoutNotify(value.ToString()); }
        }

        public WorldSize Size
        {
            get => _sizeDropdown != null ? (WorldSize)_sizeDropdown.value : WorldSize.Medium;
            set { if (_sizeDropdown != null) _sizeDropdown.SetValueWithoutNotify((int)value); }
        }

        public MapType MapType
        {
            get => _mapTypeDropdown != null ? (MapType)_mapTypeDropdown.value : MapType.Continents;
            set { if (_mapTypeDropdown != null) _mapTypeDropdown.SetValueWithoutNotify((int)value); }
        }

        public Difficulty Difficulty
        {
            get => _difficultyDropdown != null ? (Difficulty)_difficultyDropdown.value : Difficulty.Normal;
            set { if (_difficultyDropdown != null) _difficultyDropdown.SetValueWithoutNotify((int)value); }
        }

        public Button CreateWorldButton => _nextButton;

        public event Action OnButtonNextClicked;
        public event Action OnRandomSeedClicked;

        private UnityAction _onNextClicked;
        private UnityAction _onRandomClicked;

        public void Initialize() { EnsureRuntimeControls(); AttachListeners(); PopulateDropdowns(); }
        private void Awake() { EnsureRuntimeControls(); AttachListeners(); PopulateDropdowns(); }

        private void AttachListeners()
        {
            if (_onNextClicked == null) _onNextClicked = () => OnButtonNextClicked?.Invoke();
            if (_onRandomClicked == null) _onRandomClicked = HandleRandomSeed;

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
