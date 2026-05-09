using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public sealed class KickPlayerPanelViewController : MonoBehaviour, IKickPlayerPanelViewController, IInitializable
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _refreshButton;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Transform _playersContainer;
        [SerializeField] private KickPlayerItemView _playerItemPrefab;

        private readonly List<KickPlayerItemView> _spawnedItems = new List<KickPlayerItemView>();
        private UnityAction _closeAction;
        private UnityAction _refreshAction;
        private bool _bound;
        private bool _interactable = true;

        public event Action OnCloseRequested;
        public event Action OnRefreshRequested;
        public event Action<KickPlayerInfo> OnKickRequested;

        private void Awake()
        {
            Bind();
        }

        public void Initialize()
        {
            Bind();
        }

        private void OnDestroy()
        {
            if (!_bound)
                return;

            if (_closeButton != null && _closeAction != null)
                _closeButton.onClick.RemoveListener(_closeAction);
            if (_refreshButton != null && _refreshAction != null)
                _refreshButton.onClick.RemoveListener(_refreshAction);

            _bound = false;
        }

        public void SetPlayers(IReadOnlyList<KickPlayerInfo> players)
        {
            ClearPlayers();

            if (_playersContainer == null || _playerItemPrefab == null || players == null)
                return;

            _playerItemPrefab.gameObject.SetActive(false);
            foreach (var player in players)
            {
                var item = Instantiate(_playerItemPrefab, _playersContainer);
                item.name = $"kick-player-{ToSafeGameObjectName(player.PlayerId)}";
                item.gameObject.SetActive(true);
                item.Initialize(player, HandleKickRequested);
                item.SetInteractable(_interactable);
                _spawnedItems.Add(item);
            }
        }

        public void ClearPlayers()
        {
            for (int index = _spawnedItems.Count - 1; index >= 0; index--)
            {
                var item = _spawnedItems[index];
                if (item != null)
                {
                    item.gameObject.SetActive(false);
                    Destroy(item.gameObject);
                }
            }

            _spawnedItems.Clear();

            if (_playersContainer == null)
                return;

            for (int index = _playersContainer.childCount - 1; index >= 0; index--)
            {
                var child = _playersContainer.GetChild(index);
                if (_playerItemPrefab != null && child == _playerItemPrefab.transform)
                    continue;

                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        public void SetStatus(string status)
        {
            if (_statusText != null)
                _statusText.text = status ?? string.Empty;
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;

            if (_refreshButton != null)
                _refreshButton.interactable = interactable;

            foreach (var item in _spawnedItems)
                item?.SetInteractable(interactable);
        }

        private void Bind()
        {
            if (_bound)
                return;

            if (_closeButton != null)
            {
                _closeAction = () => OnCloseRequested?.Invoke();
                _closeButton.onClick.AddListener(_closeAction);
            }

            if (_refreshButton != null)
            {
                _refreshAction = () => OnRefreshRequested?.Invoke();
                _refreshButton.onClick.AddListener(_refreshAction);
            }

            if (_playerItemPrefab != null)
                _playerItemPrefab.gameObject.SetActive(false);

            _bound = true;
        }

        private void HandleKickRequested(KickPlayerInfo playerInfo)
        {
            OnKickRequested?.Invoke(playerInfo);
        }

        private static string ToSafeGameObjectName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Guid.NewGuid().ToString("N");

            var chars = value.ToCharArray();
            for (int index = 0; index < chars.Length; index++)
            {
                if (!char.IsLetterOrDigit(chars[index]) && chars[index] != '-' && chars[index] != '_')
                    chars[index] = '-';
            }

            return new string(chars);
        }
    }
}