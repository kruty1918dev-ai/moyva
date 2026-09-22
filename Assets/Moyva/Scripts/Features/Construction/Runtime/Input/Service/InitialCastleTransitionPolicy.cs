using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Blocks leaving construction mode while the owner still owes the
    /// initial castle placement. Applies the same rule as the cancel path in
    /// ConstructionInputService so every exit route (Escape, close button,
    /// mode toggle) is guarded consistently.
    /// </summary>
    internal sealed class InitialCastleTransitionPolicy : IGameModeTransitionPolicy
    {
        private readonly IConstructionSessionCommands _constructionService;

        public InitialCastleTransitionPolicy(IConstructionSessionCommands constructionService)
        {
            _constructionService = constructionService;
        }

        public bool TryGetTransitionBlockReason(
            GameModeType currentMode,
            GameModeType requestedMode,
            out string reason)
        {
            reason = null;
            if (currentMode != GameModeType.Construction
                || requestedMode == GameModeType.Construction
                || _constructionService is not IConstructionBootstrapQuery bootstrap)
                return false;

            string ownerId = _constructionService.GetActiveOwner();
            if (string.IsNullOrWhiteSpace(ownerId)
                || !bootstrap.RequiresInitialCastle(ownerId.Trim(), out _))
                return false;

            reason = "Place your first castle before leaving construction mode.";
            return true;
        }
    }
}
