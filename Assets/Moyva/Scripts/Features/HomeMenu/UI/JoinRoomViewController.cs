using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Shared.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контролер екрана Join Room: керує кодом входу, списком кімнат і вибором кімнати.
    /// Залежності: <see cref="RoomItemViewComponent"/> для рендеру елементів списку.
    /// </summary>
    public class JoinRoomViewController : MonoBehaviour, IJoinRoomViewController, IInitializable
    {
        [SerializeField] private TMP_InputField _joinCodeInput;
        [SerializeField] private Button _joinRoomButton;
        [SerializeField] private Button _refreshButton;
        [SerializeField] private Transform _roomsContainer;
        [SerializeField] private RoomItemViewComponent _roomPrefab;

        public event Action OnJoinRequested;
        public event Action OnJoinCodeChanged;
        public event Action OnListRoomsRefresh;
        public event Action<RoomInfo> OnRoomSelected;

        // Track spawned room items by provider-aware display key
        private readonly Dictionary<string, RoomItemViewComponent> _spawned = new Dictionary<string, RoomItemViewComponent>(StringComparer.Ordinal);
        private readonly Dictionary<string, RoomInfo> _roomInfos = new Dictionary<string, RoomInfo>(StringComparer.Ordinal);
        private readonly Stack<RoomItemViewComponent> _pool = new Stack<RoomItemViewComponent>();
        private UnityEngine.Events.UnityAction _joinButtonAction;
        private UnityEngine.Events.UnityAction _refreshButtonAction;
        private bool _bound;

        public string JoinCode { get; set; }

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
            // 1: Не перев'язуємо UI повторно.
            if (_bound)
                return;

            // 2: Підписуємо інпут і кнопки на події API.
            if (_joinCodeInput != null)
            {
                _joinCodeInput.onValueChanged.AddListener(OnJoinCodeEdited);
            }

            if (_joinRoomButton != null)
            {
                _joinButtonAction = () => OnJoinRequested?.Invoke();
                _joinRoomButton.onClick.AddListener(_joinButtonAction);
            }

            if (_refreshButton != null)
            {
                _refreshButtonAction = () => OnListRoomsRefresh?.Invoke();
                _refreshButton.onClick.AddListener(_refreshButtonAction);
            }

            // 3: Позначаємо контролер як ініціалізований.
            _bound = true;
        }

        private void OnDestroy()
        {
            if (_joinCodeInput != null)
                _joinCodeInput.onValueChanged.RemoveListener(OnJoinCodeEdited);
            if (_joinRoomButton != null && _joinButtonAction != null)
                _joinRoomButton.onClick.RemoveListener(_joinButtonAction);
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
            // 1: Перевіряємо, чи маємо всі необхідні prefab/container залежності.
            if (_roomPrefab == null || _roomsContainer == null) return;

            var key = BuildRoomKey(room);

            // 2: Якщо елемент вже існує, просто оновлюємо його контент.
            if (_spawned.TryGetValue(key, out var existing) && existing != null)
            {
                _roomInfos[key] = room;
                existing.Initialize(room, r => HandleRoomSelected(r));
                return;
            }

            // 3: Інакше повторно використовуємо view-елемент або створюємо його один раз.
            var instance = GetOrCreateRoomItem();
            instance.name = $"room-{ToSafeGameObjectName(key)}";
            instance.transform.SetParent(_roomsContainer, false);
            instance.gameObject.SetActive(true);
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
                return MoyvaId.NewGuidN();

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
            foreach (var pair in _spawned)
                ReleaseRoomItem(pair.Value);

            _spawned.Clear();
            _roomInfos.Clear();
        }

        public void RefreshRoomList()
        {
            OnListRoomsRefresh?.Invoke();
        }

        public void SetJoinInteractable(bool interactable)
        {
            if (_joinRoomButton != null)
                _joinRoomButton.interactable = interactable;
        }

        private RoomItemViewComponent GetOrCreateRoomItem()
        {
            while (_pool.Count > 0)
            {
                var pooled = _pool.Pop();
                if (pooled != null)
                    return pooled;
            }

            return Instantiate(_roomPrefab, _roomsContainer);
        }

        private void ReleaseRoomItem(RoomItemViewComponent item)
        {
            if (item == null)
                return;

            item.gameObject.SetActive(false);
            _pool.Push(item);
        }

        // This class would be implemented by the actual MonoBehaviour that has the UI elements.
    }
}
