using System.Collections.Generic;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    /// <summary>
    /// Shared veto evaluation for game mode transition policies, used by the
    /// UI action handler and the request router so every entry point applies
    /// the same rules.
    /// </summary>
    internal static class GameModeTransitionPolicyGuard
    {
        public static bool TryGetBlockReason(
            IReadOnlyList<IGameModeTransitionPolicy> policies,
            GameModeType currentMode,
            GameModeType requestedMode,
            out string reason)
        {
            if (policies != null)
            {
                for (int i = 0; i < policies.Count; i++)
                {
                    if (policies[i] != null
                        && policies[i].TryGetTransitionBlockReason(currentMode, requestedMode, out reason))
                        return true;
                }
            }

            reason = null;
            return false;
        }
    }
}
