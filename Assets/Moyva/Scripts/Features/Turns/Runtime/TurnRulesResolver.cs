using Kruty1918.JsonConfig;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Turns.Runtime
{
    /// <summary>
    /// Resolve step of the turn-rules lifecycle: merges the
    /// <see cref="TurnRulesConfig"/> JSON preset with the explicit launch-context
    /// choice (host lobby selection propagated through the session flow) into
    /// the frozen <see cref="TurnRules"/> consumed by <see cref="TurnService"/>.
    /// </summary>
    internal static class TurnRulesResolver
    {
        public const string DocumentId = "turn-rules";
        private const int MinTimerSeconds = 5;
        private const int MaxTimerSeconds = 3600;

        public static TurnRules Resolve()
        {
            TurnRulesConfig config = LoadConfig();

            bool isMultiplayer = GameLaunchContext.Mode == GameLaunchMode.MenuMultiplayerGame
                                 || GameLaunchContext.Mode == GameLaunchMode.MenuJoinGame;

            TurnFlowMode mode = isMultiplayer
                ? ParseMode(config?.MultiplayerMode, TurnFlowMode.Simultaneous)
                : ParseMode(config?.SoloMode, TurnFlowMode.Sequential);

            int timerSeconds = config?.RoundTimerSeconds ?? 90;
            bool timerEnabled = config?.RoundTimerEnabled ?? true;
            bool holdTimer = config?.HoldTimerUntilInitialCastlesPlaced ?? true;

            // Explicit per-match choices from the host lobby (or synchronized
            // session settings on clients) override the JSON defaults.
            if (GameLaunchContext.TurnMode >= 0)
                mode = (TurnFlowMode)GameLaunchContext.TurnMode;
            if (GameLaunchContext.TurnTimerSeconds >= 0)
            {
                timerSeconds = GameLaunchContext.TurnTimerSeconds;
                timerEnabled = timerSeconds > 0;
            }

            if (mode != TurnFlowMode.Simultaneous)
                timerSeconds = 0;
            else if (timerEnabled)
                timerSeconds = Mathf.Clamp(timerSeconds, MinTimerSeconds, MaxTimerSeconds);
            else
                timerSeconds = 0;

            return new TurnRules(mode, timerSeconds, holdTimer);
        }

        private static TurnRulesConfig LoadConfig()
        {
            try
            {
                return JsonConfigRuntime.Get<TurnRulesConfig>(DocumentId);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Turns] Failed to load '{DocumentId}': {ex.Message}");
                return null;
            }
        }

        private static TurnFlowMode ParseMode(string value, TurnFlowMode fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            return value.Trim().Equals("simultaneous", System.StringComparison.OrdinalIgnoreCase)
                ? TurnFlowMode.Simultaneous
                : value.Trim().Equals("sequential", System.StringComparison.OrdinalIgnoreCase)
                    ? TurnFlowMode.Sequential
                    : fallback;
        }
    }
}
