using System;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface ISelectedGameModeService
    {
        GameMode SelectedGameMode { get; }
        void SetSelectedGameMode(GameMode mode);

        event Action<GameMode> OnSelectedGameModeChanged;
    }
}