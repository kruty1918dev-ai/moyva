using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.GameMode.API
{
    /// <summary>
    /// Vetoes a requested game mode transition. Implemented by features that
    /// own mode-specific invariants, e.g. construction blocking the exit
    /// while the initial castle has not been placed.
    /// </summary>
    public interface IGameModeTransitionPolicy
    {
        bool TryGetTransitionBlockReason(
            GameModeType currentMode,
            GameModeType requestedMode,
            out string reason);
    }
}
