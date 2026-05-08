using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// MonoBehaviour-реалізація інформаційної панелі з кнопкою OK.
    /// Розташуйте у сцені на GameObject "InfoPanel" і призначте посилання у інспекторі.
    /// </summary>
    public sealed class InfoPanelViewController : MonoBehaviour, IInfoPanelViewController
    {
        [Header("References")]
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _okButton;

        public event Action OnAcknowledged;

        public bool IsVisible => _root != null && _root.activeSelf;

        private void Awake()
        {
            Bind();
            // Стартуємо у схованому стані
            if (_root != null) _root.SetActive(false);
        }

        internal void ConfigureReferences(GameObject root, TextMeshProUGUI titleText, TextMeshProUGUI messageText, Button okButton)
        {
            _root = root;
            _titleText = titleText;
            _messageText = messageText;
            _okButton = okButton;
            Bind();
            if (_root != null) _root.SetActive(false);
        }

        private void Bind()
        {
            if (_okButton != null)
            {
                _okButton.onClick.RemoveListener(OnOkClicked);
                _okButton.onClick.AddListener(OnOkClicked);
            }
        }

        private void OnDestroy()
        {
            if (_okButton != null)
                _okButton.onClick.RemoveListener(OnOkClicked);
        }

        public void Show(InfoMessage message)
        {
            if (_titleText != null) _titleText.text = message.Title;
            if (_messageText != null) _messageText.text = message.Message;
            if (_root != null) _root.SetActive(true);
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        private void OnOkClicked()
        {
            OnAcknowledged?.Invoke();
        }
    }
}
