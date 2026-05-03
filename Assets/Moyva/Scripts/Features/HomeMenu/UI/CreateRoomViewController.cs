using System;
using Kruty1918.Moyva.HomeMenu.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public class CreateRoomViewController : MonoBehaviour, ICreateRoomViewController, IInitializable
    {
        [SerializeField] private TMP_InputField _roomNameInput;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private Toggle _isPublicToggle;
        [SerializeField] private Slider _maxPlayersInput;
        [SerializeField] private TMP_Text _maxPlayersLabel;
        [SerializeField] private Button _nextButton;

        public string RoomName { get; set; }
        public string Password { get; set; }
        public bool IsPublic { get; set; }
        public int MaxPlayers { get; set; }
        public Button NextButton => _nextButton;

        public event Action OnButtonNextClicked;

        private bool _bound;

        public void Initialize()
        {
            Bind();
        }

        private void Awake()
        {
            Bind();
        }

        private void Bind()
        {
            if (_bound)
                return;

            EnsureRuntimeControls();

            if (_roomNameInput != null)
                _roomNameInput.onEndEdit.AddListener(OnRoomNameEdited);
            if (_passwordInput != null)
                _passwordInput.onEndEdit.AddListener(OnPasswordEdited);
            if (_isPublicToggle != null)
                _isPublicToggle.onValueChanged.AddListener(OnIsPublicChanged);
            if (_maxPlayersInput != null)
                _maxPlayersInput.onValueChanged.AddListener(OnMaxPlayersChanged);
            if (_nextButton != null)
                _nextButton.onClick.AddListener(OnNextClicked);
        
            // Ініціалізувати значення і стан кнопки
            RoomName = _roomNameInput != null ? _roomNameInput.text : string.Empty;
            Password = _passwordInput != null ? _passwordInput.text : string.Empty;
            IsPublic = _isPublicToggle != null ? _isPublicToggle.isOn : true;
            MaxPlayers = _maxPlayersInput != null ? Mathf.RoundToInt(_maxPlayersInput.value) : 4;
            UpdateMaxPlayersLabel();
            UpdateNextButtonState();

            _bound = true;
        }

        private void OnDestroy()
        {
            if (!_bound)
                return;

            if (_roomNameInput != null)
                _roomNameInput.onEndEdit.RemoveListener(OnRoomNameEdited);
            if (_passwordInput != null)
                _passwordInput.onEndEdit.RemoveListener(OnPasswordEdited);
            if (_isPublicToggle != null)
                _isPublicToggle.onValueChanged.RemoveListener(OnIsPublicChanged);
            if (_maxPlayersInput != null)
                _maxPlayersInput.onValueChanged.RemoveListener(OnMaxPlayersChanged);
            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(OnNextClicked);

            _bound = false;
        }

        private void OnRoomNameEdited(string value)
        {
            RoomName = value;
            UpdateNextButtonState();
        }

        private void OnPasswordEdited(string value)
        {
            Password = value;
            UpdateNextButtonState();
        }

        private void OnIsPublicChanged(bool value)
        {
            IsPublic = value;
            UpdateNextButtonState();
        }

        private void OnMaxPlayersChanged(float value)
        {
            MaxPlayers = Mathf.RoundToInt(value);
            UpdateMaxPlayersLabel();
            UpdateNextButtonState();
        }

        /// <summary>
        /// Оновлює текст-підпис біля слайдера у форматі "N (2..8)".
        /// </summary>
        private void UpdateMaxPlayersLabel()
        {
            if (_maxPlayersLabel == null)
                return;

            int min = _maxPlayersInput != null ? Mathf.RoundToInt(_maxPlayersInput.minValue) : 2;
            int max = _maxPlayersInput != null ? Mathf.RoundToInt(_maxPlayersInput.maxValue) : 8;
            _maxPlayersLabel.text = $"{MaxPlayers} ({min}..{max})";
        }

        private void OnNextClicked()
        {
            if (_roomNameInput != null) RoomName = _roomNameInput.text;
            if (_passwordInput != null) Password = _passwordInput.text;
            if (_isPublicToggle != null) IsPublic = _isPublicToggle.isOn;
            if (_maxPlayersInput != null) MaxPlayers = Mathf.RoundToInt(_maxPlayersInput.value);
            UpdateNextButtonState();
            OnButtonNextClicked?.Invoke();
        }

        /// <summary>
        /// Локальна валідація стану форми — визначає, чи можна перейти далі.
        /// Правило: ім'я кімнати не пусте, та (публічна кімната або вказано пароль).
        /// </summary>
        private void UpdateNextButtonState()
        {
            bool canProceed = !string.IsNullOrEmpty(RoomName) && (IsPublic || !string.IsNullOrEmpty(Password));
            if (_nextButton != null)
                _nextButton.interactable = canProceed;
        }

        private void EnsureRuntimeControls()
        {
            if (_maxPlayersLabel != null || _maxPlayersInput == null)
                return;

            var parent = _maxPlayersInput.transform.parent != null ? _maxPlayersInput.transform.parent : transform;
            _maxPlayersLabel = HomeMenuRuntimeUiFactory.CreateText(parent, "Label_MaxPlayersValue", string.Empty, 16, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        }
        // This class would be implemented by the actual MonoBehaviour that has the UI elements.
    }


}