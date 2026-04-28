using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO:    Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public class JoinRoomViewController : MonoBehaviour, IJoinRoomViewController, IInitializable
    {
        [SerializeField] private TMP_InputField _joinCodeInput;
        [SerializeField] private Button _joinRoomButton;
        [SerializeField] private Button _refreshButton;
        [SerializeField] private Transform _roomsContainer;
        [SerializeField] private RoomItemViewComponent _roomPrefab;

        public event Action OnJoinCodeChanged;
        public event Action OnListRoomsRefresh;

        // Track spawned room items by join code
        private readonly Dictionary<string, RoomItemViewComponent> _spawned = new Dictionary<string, RoomItemViewComponent>(StringComparer.Ordinal);
        private readonly Dictionary<string, RoomInfo> _roomInfos = new Dictionary<string, RoomInfo>(StringComparer.Ordinal);
        private UnityEngine.Events.UnityAction _refreshButtonAction;

        public string JoinCode { get; set; }
        public Button JoinToRoomButton { get => _joinRoomButton; set => _joinRoomButton = value; }

        public void Initialize()
        {
            Awake();
        }

        void Awake()
        {
            if (_joinCodeInput != null)
            {
                _joinCodeInput.onValueChanged.AddListener(OnJoinCodeEdited);
            }

            if (_joinRoomButton != null)
            {
                // keep field reference; service will attach actual join handler
            }

            if (_refreshButton != null)
            {
                _refreshButtonAction = () => OnListRoomsRefresh?.Invoke();
                _refreshButton.onClick.AddListener(_refreshButtonAction);
            }

            if (_roomsContainer == null)
                Debug.LogWarning("[JoinRoomViewController] _roomsContainer is not assigned.");
            if (_roomPrefab == null)
                Debug.LogWarning("[JoinRoomViewController] _roomPrefab is not assigned.");

                
        }

        private void OnDestroy()
        {
            if (_joinCodeInput != null)
                _joinCodeInput.onValueChanged.RemoveListener(OnJoinCodeEdited);
            if (_refreshButton != null && _refreshButtonAction != null)
                _refreshButton.onClick.RemoveListener(_refreshButtonAction);
        }

        private void OnJoinCodeEdited(string value)
        {
            JoinCode = value;
            OnJoinCodeChanged?.Invoke();
        }

        public void AddRoomToList(RoomInfo room)
        {
            if (_roomPrefab == null || _roomsContainer == null) return;

            var key = room.JoinCode ?? room.RoomName ?? Guid.NewGuid().ToString();

            if (_spawned.TryGetValue(key, out var existing) && existing != null)
            {
                _roomInfos[key] = room;
                existing.Initialize(room, r => OnRoomSelected(r));
                return;
            }

            var instance = Instantiate(_roomPrefab, _roomsContainer);
            instance.name = $"room-{key}";
            instance.Initialize(room, r => OnRoomSelected(r));
            _spawned[key] = instance;
            _roomInfos[key] = room;
        }

        private void OnRoomSelected(RoomInfo room)
        {
            JoinCode = room.JoinCode;
            if (_joinCodeInput != null)
                _joinCodeInput.SetTextWithoutNotify(room.JoinCode ?? string.Empty);
            OnJoinCodeChanged?.Invoke();
        }

        public void ClearRoomList()
        {
            _spawned.Clear();
            _roomInfos.Clear();
            if (_roomsContainer == null) return;
            for (int i = _roomsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_roomsContainer.GetChild(i).gameObject);
            }
        }

        public void RefreshRoomList()
        {
            OnListRoomsRefresh?.Invoke();
        }

        // This class would be implemented by the actual MonoBehaviour that has the UI elements.
    }
}