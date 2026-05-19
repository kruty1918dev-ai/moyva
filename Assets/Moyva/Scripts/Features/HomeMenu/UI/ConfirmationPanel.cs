using System;
using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Панель підтвердження дій у HomeMenu.
    /// Відображає <see cref="ConfirmationRequest"/> і керує confirm/cancel callback-ланцюгом.
    /// </summary>
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
        private static readonly bool VerboseLogging = false;

        public Action OnConfirme { get; set; }
        public Action OnCancled { get; set; }

        private void Awake()
        {
            if (_okButton != null)
            {
                _okButton.onClick.RemoveListener(OnButtonOK);
                _okButton.onClick.AddListener(OnButtonOK);
            }

            if (_cancleButton != null)
            {
                _cancleButton.onClick.RemoveListener(OnButtonCancled);
                _cancleButton.onClick.AddListener(OnButtonCancled);
            }
            Refresh();
        }

        private void OnDestroy()
        {
            if (_okButton != null)
                _okButton.onClick.RemoveListener(OnButtonOK);
            if (_cancleButton != null)
                _cancleButton.onClick.RemoveListener(OnButtonCancled);
        }

        #region API
        /// <summary>Примусово сховати панель і скинути активний запит.</summary>
        public void ForeceHide()
        {
            _request = null;

            OpenOrClose(false);
            Refresh();
            LogWithSufix("Force hide was called.");
        }

        /// <summary>Показати панель з переданим запитом підтвердження.</summary>
        public void Show(ConfirmationRequest request)
        {
            _request = request;
            OpenOrClose(true);
            Refresh();
            LogWithSufix("Panel was showed");
        }

        /// <summary>Повертає поточний запит, якщо він встановлений.</summary>
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
            if (_panel != null)
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
                if (_labelText != null)
                    _labelText.text = request.Value.LabelText;
                if (_msgText != null)
                    _msgText.text = request.Value.MessageText;

                if (_okButton != null)
                    _okButton.interactable = true;
                if (_cancleButton != null)
                    _cancleButton.interactable = true;

                LogWithSufix("Panel was refreshed with new request.");
            }
            else
            {
                if (_labelText != null)
                    _labelText.text = string.Empty;
                if (_msgText != null)
                    _msgText.text = string.Empty;

                if (_okButton != null)
                    _okButton.interactable = false;
                if (_cancleButton != null)
                    _cancleButton.interactable = false;

                LogWithSufix("Panel was refreshed, but request was null. Label was cleared.");
            }
        }

        #region Log
        private void LogWithSufix(string msg)
        {
            if (!VerboseLogging)
                return;

            Debug.Log($"[ConfirmationPanel] {msg}");
        }
        #endregion

        #endregion
    }
 }