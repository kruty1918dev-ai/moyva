using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class WorldCreationPanelService : IWorldCreationPanelService, IInitializable, IDisposable
    {
        [Inject] private IWorldSetupViewController _viewController;
        [Inject] private INavigation _navigation;
        [Inject(Id = "LobbyPanelName")] private string _lobbyPanelName;
        private WolrdCreationMode _mode;

        public void Initialize()
        {
            _viewController.OnButtonNextClicked -= OnCreteWorldClicked;
            _viewController.OnButtonNextClicked += OnCreteWorldClicked;
        }

        public void Dispose()
        {
            _viewController.OnButtonNextClicked -= OnCreteWorldClicked;
            Refresh();
        }

        private void OnCreteWorldClicked()
        {
            if (_mode == WolrdCreationMode.Solo)
            {
                // Запуск одиночної гри з параметрами, отриманими з _viewController
                // автоматичне збереження світу після створення
            }
            else if (_mode == WolrdCreationMode.Multiplayer)
            {
                // перекидуємо на панель з лобі зберігаючи параметри світу,
                // тримані з _viewController,
                // для подальшого створення кімнати та запуску гри після налаштування лобі

                _navigation.Open(_lobbyPanelName);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported world creation mode: {_mode}");
            }
        }

        public void Refresh()
        {
            bool canProceed = !string.IsNullOrEmpty(_viewController.WorldName) && _viewController.Seed != 0;
            if (_viewController.CreateWorldButton != null)
                _viewController.CreateWorldButton.interactable = canProceed;
        }

        public void SetupMode(WolrdCreationMode mode)
        {
            _mode = mode;
        }
    }
}