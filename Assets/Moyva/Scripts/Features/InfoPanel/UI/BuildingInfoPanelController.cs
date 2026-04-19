using System;
using Kruty1918.Moyva.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.InfoPanel.UI
{
    public sealed class WorldInfoPanelController : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;

        private readonly GameObject _panelRoot;
        private readonly TMP_Text _titleText;
        private readonly TMP_Text _subtitleText;
        private readonly TMP_Text _resourcesText;
        private readonly Button _closeButton;

        public WorldInfoPanelController(
            SignalBus signalBus,
            [Inject(Id = "BuildingInfoPanelRoot")] GameObject panelRoot,
            [Inject(Id = "BuildingInfoTitleText")] TMP_Text titleText,
            [Inject(Id = "BuildingInfoSubtitleText")] TMP_Text subtitleText,
            [Inject(Id = "BuildingInfoResourcesText")] TMP_Text resourcesText,
            [Inject(Id = "BuildingInfoCloseButton")] Button closeButton)
        {
            _signalBus = signalBus;
            _panelRoot = panelRoot;
            _titleText = titleText;
            _subtitleText = subtitleText;
            _resourcesText = resourcesText;
            _closeButton = closeButton;
        }

        public void Initialize()
        {
            try
            {
                if (_signalBus == null)
                {
                    Debug.LogError("[WorldInfoPanel] Initialize: _signalBus == null");
                    return;
                }

                _signalBus.Subscribe<WorldInfoPanelRequestedSignal>(OnInfoRequested);
                _signalBus.Subscribe<WorldInfoPanelClosedSignal>(OnPanelClosed);

                if (_closeButton != null)
                    _closeButton.onClick.AddListener(ClosePanel);

                SetVisible(false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WorldInfoPanel] ПОМИЛКА в Initialize(): {ex.GetType().Name} - {ex.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                if (_signalBus == null)
                    return;

                _signalBus.TryUnsubscribe<WorldInfoPanelRequestedSignal>(OnInfoRequested);
                _signalBus.TryUnsubscribe<WorldInfoPanelClosedSignal>(OnPanelClosed);

                if (_closeButton != null)
                    _closeButton.onClick.RemoveListener(ClosePanel);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WorldInfoPanel] ПОМИЛКА в Dispose(): {ex.GetType().Name} - {ex.Message}");
            }
        }

        private void OnInfoRequested(WorldInfoPanelRequestedSignal signal)
        {
            try
            {
                _titleText.text = signal.Title ?? string.Empty;
                _subtitleText.text = signal.Subtitle ?? string.Empty;
                _resourcesText.text = signal.Content ?? string.Empty;

                SetVisible(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WorldInfoPanel] ПОМИЛКА в OnInfoRequested(): {ex.GetType().Name} - {ex.Message}");
                SetVisible(false);
            }
        }

        private void OnPanelClosed(WorldInfoPanelClosedSignal _)
        {
            SetVisible(false);
        }

        private void ClosePanel()
        {
            SetVisible(false);
            _signalBus.Fire<WorldInfoPanelClosedSignal>();
        }

        private void SetVisible(bool isVisible)
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(isVisible);
        }
    }
}
