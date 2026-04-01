using System;
using Kruty1918.Moyva.Buildings.API;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Buildings.UI
{
    /// <summary>
    /// Кнопка будівлі в меню будівництва.
    /// Відображає іконку та назву будівлі, надсилає подію при натисканні.
    /// </summary>
    public class BuildingButtonUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;

        private Action<string> _onSelected;

        /// <summary>
        /// Ініціалізувати кнопку за конфігом будівлі
        /// </summary>
        public void Setup(BuildingConfig config, Action<string> onSelected)
        {
            _onSelected = onSelected;

            if (_nameText != null)
                _nameText.text = config.DisplayName;

            if (_icon != null && config.PreviewSprite != null)
                _icon.sprite = config.PreviewSprite;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => _onSelected?.Invoke(config.TypeId));
        }
    }
}
