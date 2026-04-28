using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public class WorldItemViewComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _slotNameText;
        [SerializeField] private TextMeshProUGUI _lastModifiedText;
        [SerializeField] private Button _selectButton;

        private GameSlotInfo _slotInfo;

        public void Initialize(GameSlotInfo slotInfo, Action<GameSlotInfo> onSelect)
        {
            _slotInfo = slotInfo;
            _slotNameText.text = slotInfo.SlotName;
            _lastModifiedText.text = slotInfo.LastModified.ToString("g");
            _selectButton.onClick.AddListener(() => onSelect?.Invoke(_slotInfo));
        }
    }
}