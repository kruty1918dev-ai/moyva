using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class StartingPositionPolicy
    {
        public bool IsMultiplayerLaunchContext()
        {
            bool result = IsMultiplayerLaunchContextStatic();
            int participantCount = _sessionManager?.Participants?.Count ?? 0;
            bool hasSession = _sessionManager != null;
            bool isHost = _sessionManager != null && _sessionManager.IsLocalPlayerHost;
            string localPlayerId = _sessionManager?.LocalPlayerId ?? string.Empty;
            bool maxPlayersSuggestsMultiplayer = GameLaunchContext.MaxPlayers > 1;
            bool realMultiplayerMode = GameLaunchContext.Mode == GameLaunchMode.MenuJoinGame
                || GameLaunchContext.Mode == GameLaunchMode.MenuMultiplayerGame;

            return result;
        }

        public static bool IsMultiplayerLaunchContextStatic()
        {
            return GameLaunchContext.Mode == GameLaunchMode.MenuJoinGame
                || GameLaunchContext.Mode == GameLaunchMode.MenuMultiplayerGame;
        }

        private static void LogPolicyDecision(
            string methodName,
            int participantCount,
            bool hasSession,
            bool isHost,
            string localPlayerId,
            bool isMultiplayerContext,
            bool result,
            string reason)
        {
            bool maxPlayersSuggestsMultiplayer = GameLaunchContext.MaxPlayers > 1;
            bool realMultiplayerMode = GameLaunchContext.Mode == GameLaunchMode.MenuJoinGame
                || GameLaunchContext.Mode == GameLaunchMode.MenuMultiplayerGame;
        }
    }
}
