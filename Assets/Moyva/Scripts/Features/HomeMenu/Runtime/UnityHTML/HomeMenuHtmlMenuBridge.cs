using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    public sealed class HomeMenuHtmlMenuBridge
    {
        private const string PlayModePanel = "PlayModePanel";
        private const string ContinuePanel = "ContinuePanel";
        private const string MultiplayerPanel = "SelectMultiplayerType";
        private const string SettingsPanel = "SettingsPanel";

        private readonly INavigation _navigation;
        private readonly IConfirmationService _confirmationService;

        public HomeMenuHtmlMenuBridge(INavigation navigation, IConfirmationService confirmationService)
        {
            _navigation = navigation;
            _confirmationService = confirmationService;
        }

        public void Play() => OpenPanel(PlayModePanel);

        public void Continue() => OpenPanel(ContinuePanel);

        public void Multiplayer() => OpenPanel(MultiplayerPanel);

        public void Settings() => OpenPanel(SettingsPanel);

        public void Exit()
        {
            if (_confirmationService == null)
            {
                Debug.LogError("[HomeMenuHtmlMenuBridge] Confirmation service is not available.");
                return;
            }

            _confirmationService.Show(new ConfirmationRequest
            {
                LabelText = "Вийти з гри",
                MessageText = "Ви дійсно хочете вийти ?",
                OnConfirm = Application.Quit
            });
        }

        private void OpenPanel(string panelName)
        {
            if (_navigation == null)
            {
                Debug.LogError($"[HomeMenuHtmlMenuBridge] Navigation service is not available for '{panelName}'.");
                return;
            }

            _navigation.Open(panelName);
        }
    }
}
