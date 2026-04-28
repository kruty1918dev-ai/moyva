using System;
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
        [SerializeField] private Button _nextButton;

        public string RoomName { get; set; }
        public string Password { get; set; }
        public bool IsPublic { get; set; }
        public int MaxPlayers { get; set; }

        public event Action OnButtonNextClicked;

  public void Initialize()
        {
            Awake();
        }

        private void Awake()
        {
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
        }

        private void OnDestroy()
        {
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
        }

        private void OnRoomNameEdited(string value)
        {
            RoomName = value;
        }

        private void OnPasswordEdited(string value)
        {
            Password = value;
        }

        private void OnIsPublicChanged(bool value)
        {
            IsPublic = value;
        }

        private void OnMaxPlayersChanged(float value)
        {
            MaxPlayers = Mathf.RoundToInt(value);
        }

        private void OnNextClicked()
        {
            if (_roomNameInput != null) RoomName = _roomNameInput.text;
            if (_passwordInput != null) Password = _passwordInput.text;
            if (_isPublicToggle != null) IsPublic = _isPublicToggle.isOn;
            if (_maxPlayersInput != null) MaxPlayers = Mathf.RoundToInt(_maxPlayersInput.value);

            OnButtonNextClicked?.Invoke();
        }

        // This class would be implemented by the actual MonoBehaviour that has the UI elements.
    }
}