using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    /// <summary>
    /// Контролер базового multiplayer-екрану з вибором Create Room / Join Room.
    /// </summary>
    public class MultiplayerViewController : MonoBehaviour, IMultiplayerViewController, IInitializable
    {
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _joinRoomButton;
        public Button ButtonCreateRoom { get => _createRoomButton; set => _createRoomButton = value; }
        public Button ButtonJoinToRoom { get => _joinRoomButton; set => _joinRoomButton = value; }

        public event Action OnCreateRoomClicked;
        public event Action OnJoinRoomClicked;
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

            if (_createRoomButton != null)
                _createRoomButton.onClick.AddListener(HandleCreateRoomClicked);
            if (_joinRoomButton != null)
                _joinRoomButton.onClick.AddListener(HandleJoinRoomClicked);

            _bound = true;
        }

        private void OnDestroy()
        {
            if (_createRoomButton != null)
                _createRoomButton.onClick.RemoveListener(HandleCreateRoomClicked);
            if (_joinRoomButton != null)
                _joinRoomButton.onClick.RemoveListener(HandleJoinRoomClicked);

            _bound = false;
        }

        private void HandleCreateRoomClicked()
        {
            OnCreateRoomClicked?.Invoke();
        }

        private void HandleJoinRoomClicked()
        {
            OnJoinRoomClicked?.Invoke();
        }
    }
}