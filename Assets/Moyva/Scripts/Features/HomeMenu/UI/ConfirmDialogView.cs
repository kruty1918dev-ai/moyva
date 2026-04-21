using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Універсальний діалог підтвердження з двома кнопками (Так/Ні).
    /// </summary>
    public sealed class ConfirmDialogView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text messageLabel;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private Action _onConfirm;
        private Action _onCancel;

        private void Awake()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(HandleConfirm);
            if (cancelButton  != null) cancelButton.onClick.AddListener(HandleCancel);
        }

        private void OnDestroy()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(HandleConfirm);
            if (cancelButton  != null) cancelButton.onClick.RemoveListener(HandleCancel);
        }

        /// <summary>Показує діалог з заданими текстами та обробниками.</summary>
        public void Show(string title, string message, Action onConfirm, Action onCancel)
        {
            if (titleLabel   != null) titleLabel.text   = title   ?? string.Empty;
            if (messageLabel != null) messageLabel.text = message ?? string.Empty;
            _onConfirm = onConfirm;
            _onCancel  = onCancel;
            gameObject.SetActive(true);
        }

        /// <summary>Приховує діалог.</summary>
        public void Hide()
        {
            _onConfirm = null;
            _onCancel  = null;
            gameObject.SetActive(false);
        }

        private void HandleConfirm()
        {
            var callback = _onConfirm;
            _onConfirm = null;
            _onCancel  = null;
            callback?.Invoke();
        }

        private void HandleCancel()
        {
            var callback = _onCancel;
            _onConfirm = null;
            _onCancel  = null;
            callback?.Invoke();
        }
    }
}
