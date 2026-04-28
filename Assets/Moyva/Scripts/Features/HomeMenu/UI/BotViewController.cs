using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public class BotViewController : MonoBehaviour, IBotViewController, IInitializable
    {
        [SerializeField] private TMP_Dropdown _difficultyDropdown;
        [SerializeField] private TMP_Dropdown _strategyDropdown;
        [SerializeField] private Slider _botCountInput;
        [SerializeField] private Toggle _allowCheatingToggle;
        [SerializeField] private Button _nextButton;

        private BotDifficulty _difficulty = BotDifficulty.Medium;
        private BotStrategy _strategy = BotStrategy.Random;
        private int _botCount = 1;
        private bool _allowBotCheating = false;

        public BotDifficulty Difficulty
        {
            get => _difficulty;
            set
            {
                if (_difficulty == value) return;
                _difficulty = value;
                Refresh();
            }
        }

        public BotStrategy Strategy
        {
            get => _strategy;
            set
            {
                if (_strategy == value) return;
                _strategy = value;
                Refresh();
            }
        }

        public int BotCount
        {
            get => _botCount;
            set
            {
                if (_botCount == value) return;
                _botCount = value;
                Refresh();
            }
        }

        public bool AllowBotCheating
        {
            get => _allowBotCheating;
            set
            {
                if (_allowBotCheating == value) return;
                _allowBotCheating = value;
                Refresh();
            }
        }

        public event Action OnButtonNextClicked;
        private void Awake()
        {
            if (_difficultyDropdown != null)
                _difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
            if (_strategyDropdown != null)
                _strategyDropdown.onValueChanged.AddListener(OnStrategyChanged);
            if (_botCountInput != null)
                _botCountInput.onValueChanged.AddListener(OnBotCountChanged);
            if (_allowCheatingToggle != null)
                _allowCheatingToggle.onValueChanged.AddListener(OnAllowCheatingChanged);
            if (_nextButton != null)
                _nextButton.onClick.AddListener(OnNextClicked);

            Refresh();
        }

        private void OnDestroy()
        {
            if (_difficultyDropdown != null)
                _difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyChanged);
            if (_strategyDropdown != null)
                _strategyDropdown.onValueChanged.RemoveListener(OnStrategyChanged);
            if (_botCountInput != null)
                _botCountInput.onValueChanged.RemoveListener(OnBotCountChanged);
            if (_allowCheatingToggle != null)
                _allowCheatingToggle.onValueChanged.RemoveListener(OnAllowCheatingChanged);
            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(OnNextClicked);
        }

        private void OnDifficultyChanged(int value)
        {
            Difficulty = (BotDifficulty)value;
        }

        private void OnStrategyChanged(int value)
        {
            Strategy = (BotStrategy)value;
        }

        private void OnBotCountChanged(float value)
        {
            BotCount = Mathf.RoundToInt(value);
        }

        private void OnAllowCheatingChanged(bool value)
        {
            AllowBotCheating = value;
        }

        private void OnNextClicked()
        {
            OnButtonNextClicked?.Invoke();
        }

        public void Initialize()
        {
            Awake();
        }

        public void Refresh()
        {
            // Sync model -> UI. Use *WithoutNotify variants to avoid triggering listeners.
            if (_difficultyDropdown != null)
                _difficultyDropdown.SetValueWithoutNotify((int)Difficulty);

            if (_strategyDropdown != null)
                _strategyDropdown.SetValueWithoutNotify((int)Strategy);

            if (_botCountInput != null)
                _botCountInput.SetValueWithoutNotify(BotCount);

            if (_allowCheatingToggle != null)
                _allowCheatingToggle.SetIsOnWithoutNotify(AllowBotCheating);
        }
        // This class would be implemented by the actual MonoBehaviour that has the UI elements.
    }
}