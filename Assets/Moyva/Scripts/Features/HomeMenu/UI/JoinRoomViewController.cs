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
        public event Action<RoomInfo> OnRoomSelected;

        // Track spawned room items by provider-aware display key
        private readonly Dictionary<string, RoomItemViewComponent> _spawned = new Dictionary<string, RoomItemViewComponent>(StringComparer.Ordinal);
        private readonly Dictionary<string, RoomInfo> _roomInfos = new Dictionary<string, RoomInfo>(StringComparer.Ordinal);
        private UnityEngine.Events.UnityAction _refreshButtonAction;
        private bool _bound;

        public string JoinCode { get; set; }
        public Button JoinToRoomButton { get => _joinRoomButton; set => _joinRoomButton = value; }

        public void Initialize()
        {
            Bind();
        }

        void Awake()
        {
            Bind();
        }

        private void Bind()
        {
            if (_bound)
                return;

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

            _bound = true;
        }

        private void OnDestroy()
        {
            if (_joinCodeInput != null)
                _joinCodeInput.onValueChanged.RemoveListener(OnJoinCodeEdited);
            if (_refreshButton != null && _refreshButtonAction != null)
                _refreshButton.onClick.RemoveListener(_refreshButtonAction);

            _bound = false;
        }

        private void OnJoinCodeEdited(string value)
        {
            JoinCode = value;
            OnJoinCodeChanged?.Invoke();
        }

        public void AddRoomToList(RoomInfo room)
        {
            if (_roomPrefab == null || _roomsContainer == null) return;

            var key = BuildRoomKey(room);

            if (_spawned.TryGetValue(key, out var existing) && existing != null)
            {
                _roomInfos[key] = room;
                existing.Initialize(room, r => HandleRoomSelected(r));
                return;
            }

            var instance = Instantiate(_roomPrefab, _roomsContainer);
            instance.name = $"room-{ToSafeGameObjectName(key)}";
            instance.Initialize(room, r => HandleRoomSelected(r));
            _spawned[key] = instance;
            _roomInfos[key] = room;
        }

        private void HandleRoomSelected(RoomInfo room)
        {
            JoinCode = room.HasJoinCode ? room.JoinCode : string.Empty;
            if (_joinCodeInput != null)
                _joinCodeInput.SetTextWithoutNotify(JoinCode);
            OnJoinCodeChanged?.Invoke();
            OnRoomSelected?.Invoke(room);
        }

        private static string BuildRoomKey(RoomInfo room)
        {
            var displayKey = room.DisplayKey;
            if (string.IsNullOrWhiteSpace(displayKey))
                displayKey = $"{room.RoomName}:{room.MaxPlayers}";

            return $"{room.ProviderType}:{displayKey}";
        }

        private static string ToSafeGameObjectName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Guid.NewGuid().ToString("N");

            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '-' && chars[i] != '_')
                    chars[i] = '-';
            }

            return new string(chars);
        }

        public void ClearRoomList()
        {
            _spawned.Clear();
            _roomInfos.Clear();
            if (_roomsContainer == null) return;
            for (int i = _roomsContainer.childCount - 1; i >= 0; i--)
            {
                var child = _roomsContainer.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        public void RefreshRoomList()
        {
            OnListRoomsRefresh?.Invoke();
        }

        // This class would be implemented by the actual MonoBehaviour that has the UI elements.
    }
}