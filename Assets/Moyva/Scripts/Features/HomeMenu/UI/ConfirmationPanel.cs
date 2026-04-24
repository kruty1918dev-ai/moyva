using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public class ConfirmationPanel : MonoBehaviour, IConfiremationPanel
    {
        [Header("References")]
        [SerializeField] TextMeshProUGUI _labelText;
        [SerializeField] TextMeshProUGUI _msgText;
        [Space]
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _cancleButton;
        [Space]
        [SerializeField] private GameObject _panel;

        private ConfirmationRequest? _request;

        public Action OnConfirme { get; set; }
        public Action OnCancled { get; set; }

        private void Awake()
        {
            _okButton.onClick.AddListener(OnButtonOK);
            _cancleButton.onClick.AddListener(OnButtonCancled);
            Refresh();
        }

        #region API
        public void ForeceHide()
        {
            _request = null;

            OpenOrClose(false);
            Refresh();
            LogWithSufix("Force hide was called.");
        }

        public void Show(ConfirmationRequest request)
        {
            _request = request;
            OpenOrClose(true);
            Refresh();
            LogWithSufix("Panel was showed");
        }

        public bool TryGetReqest(out ConfirmationRequest? request)
        {
            request = _request;

            if (_request != null)
            {
                LogWithSufix("TryGetReqest: Request is present.");
                return true;
            }

            LogWithSufix("TryGetReqest: reques was null.");
            return false;
        }
        #endregion

        #region Private

        private void OpenOrClose(bool open)
        {
            _panel.SetActive(open);
            LogWithSufix($"Open or close. State is: {open}");
        }

        private void OnButtonOK()
        {
            if (TryGetReqest(out ConfirmationRequest? request))
            {
                request.Value.OnConfirm?.Invoke();
                LogWithSufix("OK button was clicked. Confirm action was invoked.");
            }
            else
            {
                LogWithSufix("OK button was clicked, but request was null.");
            }

            OnConfirme?.Invoke();
        }

        private void OnButtonCancled()
        {
            if (TryGetReqest(out ConfirmationRequest? request))
            {
                request.Value.OnCancel?.Invoke();
                LogWithSufix("Cancel button was clicked. Cancel action was invoked.");
            }
            else
            {
                LogWithSufix("OK button was clicked, but request was null.");
            }

            OnCancled?.Invoke();
        }

        public void Refresh()
        {
            if (TryGetReqest(out ConfirmationRequest? request))
            {
                _labelText.text = request.Value.LabelText;
                _msgText.text = request.Value.MessageText;

                _okButton.interactable = true;
                _cancleButton.interactable = true;

                LogWithSufix("Panel was refreshed with new request.");
            }
            else
            {
                _labelText.text = string.Empty;
                _msgText.text = string.Empty;

                _okButton.interactable = false;
                _cancleButton.interactable = false;

                LogWithSufix("Panel was refreshed, but request was null. Label was cleared.");
            }
        }

        #region Log
        private void LogWithSufix(string msg)
        {
            Debug.Log($"[ConfirmationPanel] {msg}");
        }
        #endregion

        #endregion
    }

    public interface IConfirmationButton
    {
        void SetInteractable(bool interactable);
        event Action<ConfirmationRequest> OnClicked;
    }
 }