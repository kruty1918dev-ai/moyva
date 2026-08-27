using System;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контролер базового multiplayer-екрану з вибором Create Room / Join Room.
    /// </summary>
    public class MultiplayerViewController : MonoBehaviour, IMultiplayerViewController, IInitializable
    {
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _joinRoomButton;
        [SerializeField] private NetworkProviderType _provider = NetworkProviderType.Relay;
        public Button ButtonCreateRoom { get => _createRoomButton; set => _createRoomButton = value; }
        public Button ButtonJoinToRoom { get => _joinRoomButton; set => _joinRoomButton = value; }

        public event Action<NetworkProviderType> OnCreateRoomClicked;
        public event Action<NetworkProviderType> OnJoinRoomClicked;
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
            OnCreateRoomClicked?.Invoke(_provider);
        }

        private void HandleJoinRoomClicked()
        {
            OnJoinRoomClicked?.Invoke(_provider);
        }
    }
}
