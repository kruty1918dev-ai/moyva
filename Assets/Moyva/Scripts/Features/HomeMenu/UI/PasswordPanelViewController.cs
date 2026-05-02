using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// MonoBehaviour для панелі введення пароля приватної кімнати.
    /// Розташуйте у сцені на GameObject "PasswordPanel" і призначте посилання у інспекторі.
    /// </summary>
    public sealed class PasswordPanelViewController : MonoBehaviour, IPasswordPanelViewController
    {
        [Header("References")]
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _cancelButton;

        public event Action<string> OnConfirmed;
        public event Action OnCancelled;

        public bool IsVisible => _root != null && _root.activeSelf;

        private void Awake()
        {
            Bind();
            if (_root != null) _root.SetActive(false);
        }

        internal void ConfigureReferences(
            GameObject root,
            TextMeshProUGUI titleText,
            TMP_InputField passwordInput,
            TextMeshProUGUI errorText,
            Button okButton,
            Button cancelButton)
        {
            _root = root;
            _titleText = titleText;
            _passwordInput = passwordInput;
            _errorText = errorText;
            _okButton = okButton;
            _cancelButton = cancelButton;
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
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(OnCancelClicked);
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }
        }

        private void OnDestroy()
        {
            if (_okButton != null) _okButton.onClick.RemoveListener(OnOkClicked);
            if (_cancelButton != null) _cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        public void Show(string roomDisplayName, string errorText)
        {
            if (_titleText != null)
                _titleText.text = string.IsNullOrWhiteSpace(roomDisplayName)
                    ? "Введіть пароль"
                    : $"Пароль для «{roomDisplayName}»";
            if (_passwordInput != null) _passwordInput.text = string.Empty;
            SetErrorText(errorText);
            if (_root != null) _root.SetActive(true);
            if (_passwordInput != null) _passwordInput.ActivateInputField();
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        public void SetErrorText(string errorText)
        {
            if (_errorText == null) return;
            _errorText.text = errorText ?? string.Empty;
            _errorText.gameObject.SetActive(!string.IsNullOrEmpty(errorText));
        }

        private void OnOkClicked()
        {
            var value = _passwordInput != null ? _passwordInput.text : string.Empty;
            OnConfirmed?.Invoke(value);
        }

        private void OnCancelClicked()
        {
            OnCancelled?.Invoke();
        }
    }
}
