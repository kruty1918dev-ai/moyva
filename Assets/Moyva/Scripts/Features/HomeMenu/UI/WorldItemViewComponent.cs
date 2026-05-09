using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
        private UnityAction _selectAction;

        private void Awake()
        {
            DisableTextRaycasts();
        }

        public void Initialize(GameSlotInfo slotInfo, Action<GameSlotInfo> onSelect)
        {
            _slotInfo = slotInfo;
            DisableTextRaycasts();

            if (_slotNameText != null)
                _slotNameText.text = slotInfo.SlotName;
            if (_lastModifiedText != null)
                _lastModifiedText.text = slotInfo.LastModified.ToString("g");

            if (_selectButton != null && _selectAction != null)
                _selectButton.onClick.RemoveListener(_selectAction);

            _selectAction = () => onSelect?.Invoke(_slotInfo);
            if (_selectButton != null)
            {
                _selectButton.interactable = true;
                _selectButton.onClick.AddListener(_selectAction);
            }
        }

        private void DisableTextRaycasts()
        {
            if (_slotNameText != null)
                _slotNameText.raycastTarget = false;
            if (_lastModifiedText != null)
                _lastModifiedText.raycastTarget = false;
        }

        private void OnDestroy()
        {
            if (_selectButton != null && _selectAction != null)
                _selectButton.onClick.RemoveListener(_selectAction);
        }
    }
}