using System;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    [RequireComponent(typeof(Button))]
    public class ConfirmButton : MonoBehaviour, IConfirmationButton
    {
        [Header("References")]
        [SerializeField] protected Button _button;

        [Space]

        [SerializeField] protected string _label;
        [SerializeField] protected string _message;

        public event Action<ConfirmationRequest> OnClicked;

        void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (string.IsNullOrWhiteSpace(_label) || string.IsNullOrWhiteSpace(_message))
            {
                Debug.LogWarning($"[ConfirmButton] Label or message is empty. Please set them in the inspector.");
            }

            _button.onClick.AddListener(OnButtonClicked);
        }

        protected virtual void OnButtonClicked()
        {
            RaiseOnClicked(new ConfirmationRequest
            {
                LabelText = _label,
                MessageText = _message,
                OnConfirm = () => Debug.Log($"[ConfirmButton] Confirm action executed for button with label '{_label}'."),
                OnCancel = () => Debug.Log($"[ConfirmButton] Cancel action executed for button with label '{_label}'.")
            });
        }

        protected void RaiseOnClicked(ConfirmationRequest request)
        {
            OnClicked?.Invoke(request);
        }

        public void SetInteractable(bool interactable)
        {   
            _button.interactable = interactable;
            Debug.Log($"[ConfirmButton] Set interactable to {interactable} for button with label '{_label}'.");
        }
    }
}