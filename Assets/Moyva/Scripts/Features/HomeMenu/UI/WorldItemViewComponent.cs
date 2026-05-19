using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    /// <summary>
    /// View-компонент одного слота світу у списку збережень.
    /// Рендерить назву, дату зміни та делегує вибір слота назовні.
    /// </summary>
    public class WorldItemViewComponent : MonoBehaviour
    {
        /// <summary>Назва слота.</summary>
        [SerializeField] private TextMeshProUGUI _slotNameText;

        /// <summary>Дата останньої модифікації слота.</summary>
        [SerializeField] private TextMeshProUGUI _lastModifiedText;

        /// <summary>Кнопка вибору слота.</summary>
        [SerializeField] private Button _selectButton;

        private GameSlotInfo _slotInfo;
        private UnityAction _selectAction;

        private void Awake()
        {
            DisableTextRaycasts();
        }

        public void Initialize(GameSlotInfo slotInfo, Action<GameSlotInfo> onSelect)
        {
            // 1: Оновлюємо локальний стан і гарантуємо вимкнені raycast-таргети текстів.
            _slotInfo = slotInfo;
            DisableTextRaycasts();

            // 2: Рендеримо назву і час останнього редагування слота.
            if (_slotNameText != null)
                _slotNameText.text = slotInfo.SlotName;
            if (_lastModifiedText != null)
                _lastModifiedText.text = slotInfo.LastModified.ToString("g");

            // 3: Замінюємо попередню click-підписку на нову для актуального slotInfo.
            if (_selectButton != null && _selectAction != null)
                _selectButton.onClick.RemoveListener(_selectAction);

            _selectAction = () => onSelect?.Invoke(_slotInfo);
            if (_selectButton != null)
            {
                // 4: Робимо кнопку активною і підписуємо обробник вибору.
                _selectButton.interactable = true;
                _selectButton.onClick.AddListener(_selectAction);
            }
        }

        /// <summary>Вимикає перехоплення raycast для текстів, щоб кліки проходили на кнопку.</summary>
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