using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.GameMode.API
{
    public interface IGameModeService
    {
        /// <summary>Поточний активний режим гри.</summary>
        GameModeType CurrentMode { get; }

        /// <summary>
        /// Перемикає режим гри. Якщо newMode == CurrentMode — нічого не відбувається.
        /// При зміні надсилає GameModeChangedSignal.
        /// </summary>
        void SetMode(GameModeType newMode);
    }
}
