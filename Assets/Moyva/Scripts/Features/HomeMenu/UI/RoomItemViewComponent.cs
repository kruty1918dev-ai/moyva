using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public class RoomItemViewComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private Button _selectButton;

        private RoomInfo _roomInfo;

        public void Initialize(RoomInfo roomInfo, Action<RoomInfo> onSelect)
        {
            _roomInfo = roomInfo;

            if (_labelText != null)
            {
                var id = !string.IsNullOrEmpty(roomInfo.RoomName) ? roomInfo.RoomName : (roomInfo.JoinCode ?? "—");
                _labelText.text = $"Кімната #{id} ({roomInfo.CurrentPlayers}/{roomInfo.MaxPlayers})";
            }

            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(() => onSelect?.Invoke(_roomInfo));
            }
        }
    }
}
