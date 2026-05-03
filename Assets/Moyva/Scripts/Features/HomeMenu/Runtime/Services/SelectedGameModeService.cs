using System;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class SelectedGameModeService : Kruty1918.Moyva.HomeMenu.API.ISelectedGameModeService
    {
        public Kruty1918.Moyva.HomeMenu.API.GameMode SelectedGameMode { get; private set; }

        public event Action<Kruty1918.Moyva.HomeMenu.API.GameMode> OnSelectedGameModeChanged;

        public void SetSelectedGameMode(Kruty1918.Moyva.HomeMenu.API.GameMode gameMode)
        {
            SelectedGameMode = gameMode;
            OnSelectedGameModeChanged?.Invoke(gameMode);
        }
    }
}