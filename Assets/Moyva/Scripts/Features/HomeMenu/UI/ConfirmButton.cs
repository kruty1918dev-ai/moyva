using System;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Базова кнопка підтвердження, що піднімає <see cref="ConfirmationRequest"/> у confirmation flow.
    /// Залежності: використовується <see cref="IConfirmationService"/> та UI-панеллю підтвердження.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ConfirmButton : MonoBehaviour, IConfirmationButton
    {
        [Header("References")]
        [SerializeField] protected Button _button;

        [Space]

        [SerializeField] protected string _label;
        [SerializeField] protected string _message;

        public event Action<ConfirmationRequest> OnClicked;

        /// <summary>Ініціалізація кнопки й підписка на click.</summary>
        void Awake()
        {
            // 1: Якщо Button не проставлено вручну, беремо компонент з цього GameObject.
            if (_button == null)
                _button = GetComponent<Button>();

            // 2: Логуємо попередження для порожніх текстів запиту, щоб не губити UX-контекст.
            if (string.IsNullOrWhiteSpace(_label) || string.IsNullOrWhiteSpace(_message))
            {
                Debug.LogWarning($"[ConfirmButton] Label or message is empty. Please set them in the inspector.");
            }

            // 3: Підписуємо обробник натискання.
            _button.onClick.AddListener(OnButtonClicked);
        }

        /// <summary>Безпечна відписка від click-події.</summary>
        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);
        }

        /// <summary>Побудувати і відправити запит підтвердження за замовчуванням.</summary>
        protected virtual void OnButtonClicked()
        {
            // 1: Формуємо confirmation request з текстами і стандартними debug-колбеками.
            RaiseOnClicked(new ConfirmationRequest
            {
                LabelText = _label,
                MessageText = _message,
                OnConfirm = () => Debug.Log($"[ConfirmButton] Confirm action executed for button with label '{_label}'."),
                OnCancel = () => Debug.Log($"[ConfirmButton] Cancel action executed for button with label '{_label}'.")
            });
        }

        /// <summary>Підняти подію OnClicked з підготовленим запитом.</summary>
        protected void RaiseOnClicked(ConfirmationRequest request)
        {
            OnClicked?.Invoke(request);
        }

        /// <summary>Увімкнути або вимкнути кнопку.</summary>
        public void SetInteractable(bool interactable)
        {   
            _button.interactable = interactable;
            Debug.Log($"[ConfirmButton] Set interactable to {interactable} for button with label '{_label}'.");
        }
    }
}