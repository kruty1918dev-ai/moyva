using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Кореневий view HomeMenu. Містить посилання на усі панелі та оверлеї
    /// та перемикає їхню видимість за поточним <see cref="HomeMenuPanel"/>.
    /// Якщо призначено <see cref="HomeMenuPanelAnimator"/> — переходи анімовані
    /// (fade через CanvasGroup), інакше використовується миттєвий SetActive.
    /// </summary>
    public sealed class HomeMenuRootView : MonoBehaviour
    {
        [Header("Панелі")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject worldCreationPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Оверлеї")]
        [SerializeField] private LoadingOverlayView loadingOverlay;
        [SerializeField] private ConfirmDialogView  confirmDialog;

        [Header("Анімація (опціонально)")]
        [SerializeField] private HomeMenuPanelAnimator panelAnimator;

        private HomeMenuPanel _current = HomeMenuPanel.Loading;

        /// <summary>Оверлей завантаження.</summary>
        public LoadingOverlayView LoadingOverlay => loadingOverlay;

        /// <summary>Діалог підтвердження.</summary>
        public ConfirmDialogView ConfirmDialog => confirmDialog;

        /// <summary>Застосовує стан відображення під заданий панель.</summary>
        public void ApplyPanelState(HomeMenuPanel panel)
        {
            if (loadingOverlay != null)
                loadingOverlay.SetVisible(panel == HomeMenuPanel.Loading);

            // Confirm-оверлей керується самим діалогом (Show/Hide).
            if (panel == HomeMenuPanel.Confirm) return;

            var next = panel;

            if (panelAnimator != null)
            {
                panelAnimator.TransitionTo(_current, next);
            }
            else
            {
                SetActive(mainPanel,          next == HomeMenuPanel.Main);
                SetActive(worldCreationPanel, next == HomeMenuPanel.WorldCreation);
                SetActive(settingsPanel,      next == HomeMenuPanel.Settings);
            }

            _current = next;
        }

        /// <summary>Встановлює початковий стан без анімації (викликається при Initialize).</summary>
        public void SetInitialPanel(HomeMenuPanel panel)
        {
            _current = panel;
            if (panelAnimator != null)
            {
                panelAnimator.SetInstant(panel);
            }
            else
            {
                SetActive(mainPanel,          panel == HomeMenuPanel.Main);
                SetActive(worldCreationPanel, panel == HomeMenuPanel.WorldCreation);
                SetActive(settingsPanel,      panel == HomeMenuPanel.Settings);
            }

            if (loadingOverlay != null)
                loadingOverlay.SetVisible(panel == HomeMenuPanel.Loading);
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null && go.activeSelf != active)
                go.SetActive(active);
        }
    }
}
