using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public sealed class KickPlayerItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Button _kickButton;

        private KickPlayerInfo _playerInfo;
        private UnityAction _kickAction;

        public void Initialize(KickPlayerInfo playerInfo, Action<KickPlayerInfo> onKickRequested)
        {
            _playerInfo = playerInfo;

            if (_nameText != null)
                _nameText.text = string.IsNullOrWhiteSpace(playerInfo.DisplayName) ? "Гравець" : playerInfo.DisplayName.Trim();

            if (_statusText != null)
                _statusText.text = BuildStatusText(playerInfo);

            if (_kickButton == null)
                return;

            if (_kickAction != null)
                _kickButton.onClick.RemoveListener(_kickAction);

            _kickAction = () => onKickRequested?.Invoke(_playerInfo);
            _kickButton.onClick.AddListener(_kickAction);
            _kickButton.interactable = playerInfo.CanKick;
        }

        public void SetInteractable(bool interactable)
        {
            if (_kickButton != null)
                _kickButton.interactable = interactable && _playerInfo.CanKick;
        }

        private void OnDestroy()
        {
            if (_kickButton != null && _kickAction != null)
                _kickButton.onClick.RemoveListener(_kickAction);
        }

        private static string BuildStatusText(KickPlayerInfo playerInfo)
        {
            if (!string.IsNullOrWhiteSpace(playerInfo.StatusLabel))
                return playerInfo.StatusLabel;

            if (playerInfo.IsHost)
                return "Хост";

            return playerInfo.IsLocalPlayer ? "Це ти" : "Можна кікнути";
        }
    }
}