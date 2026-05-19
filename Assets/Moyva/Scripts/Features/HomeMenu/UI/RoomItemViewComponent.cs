using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// View-компонент одного елемента списку мережевих кімнат.
    /// Відповідає за рендер метаданих кімнати й делегування select-події.
    /// </summary>
    public class RoomItemViewComponent : MonoBehaviour
    {
        /// <summary>Текст назви, заповненості та провайдера кімнати.</summary>
        [SerializeField] private TextMeshProUGUI _labelText;

        /// <summary>Кнопка вибору конкретної кімнати.</summary>
        [SerializeField] private Button _selectButton;

        /// <summary>Індикатор захисту кімнати паролем.</summary>
        [Tooltip("Іконка замка — вмикається для кімнат з паролем.")]
        [SerializeField] private GameObject _lockIcon;

        /// <summary>Поточна модель кімнати, прив'язана до view.</summary>
        private RoomInfo _roomInfo;

        /// <summary>Кешований click-handler для коректної відписки.</summary>
        private UnityAction _selectAction;

        /// <summary>Ініціалізувати елемент списку даними кімнати.</summary>
        public void Initialize(RoomInfo roomInfo, Action<RoomInfo> onSelect)
        {
            // 1: Зберігаємо поточну модель кімнати у локальний стан view.
            _roomInfo = roomInfo;

            // 2: Рендеримо основний текст: назва, слотність і мережевий провайдер.
            if (_labelText != null)
            {
                var roomName = string.IsNullOrWhiteSpace(roomInfo.RoomName) ? "Без назви" : roomInfo.RoomName.Trim();
                _labelText.text = $"{roomName}\n{roomInfo.CurrentPlayers}/{roomInfo.MaxPlayers} - {roomInfo.ProviderLabel}";
            }

            // 3: Показуємо/ховаємо іконку замка залежно від наявності пароля.
            if (_lockIcon != null)
                _lockIcon.SetActive(roomInfo.HasPassword);

            // 4: Оновлюємо click-підписку й стан interactable для joinability.
            if (_selectButton != null)
            {
                if (_selectAction != null)
                    _selectButton.onClick.RemoveListener(_selectAction);

                _selectAction = () => onSelect?.Invoke(_roomInfo);
                _selectButton.onClick.AddListener(_selectAction);
                _selectButton.interactable = roomInfo.IsJoinable;
            }
        }

        /// <summary>Очистка підписок кнопки при знищенні компонента.</summary>
        private void OnDestroy()
        {
            if (_selectButton != null && _selectAction != null)
                _selectButton.onClick.RemoveListener(_selectAction);
        }
    }
}
