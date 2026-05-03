using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class BotPanelService : IBotPanelService, IInitializable, IDisposable
    {
        [Inject] private IBotViewController _botViewController;
        [InjectOptional] private BotDefaultSettings _defaultSettings;
        [InjectOptional] private Kruty1918.Moyva.HomeMenu.API.ISelectedGameModeService _selectedGameModeService;

        public void Initialize()
        {
            var ds = _defaultSettings;
            if (ds.BotCount <= 0) ds.BotCount = 1; // fallback

            _botViewController.AllowBotCheating = ds.AllowBotCheating;
            _botViewController.BotCount = ds.BotCount;
            _botViewController.Difficulty = ds.Difficulty;
            _botViewController.Strategy = ds.Strategy;
            _botViewController.OnButtonNextClicked -= OnNext;
            _botViewController.OnButtonNextClicked += OnNext;
            _botViewController.Refresh();
        }

        public void Dispose()
        {
            if (_botViewController != null)
                _botViewController.OnButtonNextClicked -= OnNext;
        }

        private void OnNext()
        {
            _selectedGameModeService.SetSelectedGameMode(Kruty1918.Moyva.HomeMenu.API.GameMode.Bot);
        }
    }

}