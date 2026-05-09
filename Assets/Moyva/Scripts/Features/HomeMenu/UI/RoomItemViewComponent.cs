using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public class RoomItemViewComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private Button _selectButton;
        [Tooltip("Іконка замка — вмикається для кімнат з паролем.")]
        [SerializeField] private GameObject _lockIcon;

        private RoomInfo _roomInfo;
    private UnityAction _selectAction;

        public void Initialize(RoomInfo roomInfo, Action<RoomInfo> onSelect)
        {
            _roomInfo = roomInfo;

            if (_labelText != null)
            {
                var roomName = string.IsNullOrWhiteSpace(roomInfo.RoomName) ? "Без назви" : roomInfo.RoomName.Trim();
                _labelText.text = $"{roomName}\n{roomInfo.CurrentPlayers}/{roomInfo.MaxPlayers} - {roomInfo.ProviderLabel}";
            }

            if (_lockIcon != null)
                _lockIcon.SetActive(roomInfo.HasPassword);

            if (_selectButton != null)
            {
                if (_selectAction != null)
                    _selectButton.onClick.RemoveListener(_selectAction);

                _selectAction = () => onSelect?.Invoke(_roomInfo);
                _selectButton.onClick.AddListener(_selectAction);
                _selectButton.interactable = roomInfo.IsJoinable;
            }
        }

        private void OnDestroy()
        {
            if (_selectButton != null && _selectAction != null)
                _selectButton.onClick.RemoveListener(_selectAction);
        }
    }
}
