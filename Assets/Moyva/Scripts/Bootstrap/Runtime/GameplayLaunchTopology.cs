using System;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Pure participant/topology policy shared by starting-position policy and assignment factory.
    /// It owns launch-mode interpretation only; it does not mutate gameplay state.
    /// </summary>
    internal static class GameplayLaunchTopology
    {
        public const string DirectLocalOwnerId = "player_0";

        public static bool IsDirectGameplay(
            GameLaunchMode mode,
            GameLaunchSource source,
            bool hasWorldSettings,
            int maxPlayers,
            string localPlayerId)
        {
            return mode == GameLaunchMode.DirectGameplayTest
                || source == GameLaunchSource.DirectGameplayTest
                || (!hasWorldSettings
                    && maxPlayers > 1
                    && string.IsNullOrWhiteSpace(localPlayerId));
        }

        public static int ResolveStartPositionCount(
            GameLaunchMode mode,
            int maxPlayers,
            bool hasWorldSettings,
            int sessionParticipantCount,
            bool isMultiplayerHost,
            int multiplayerStartSlots)
        {
            if (mode == GameLaunchMode.DirectGameplayTest)
                return Mathf.Max(2, maxPlayers);

            int participantCount = Mathf.Max(1, sessionParticipantCount);
            if (participantCount > 1 || isMultiplayerHost)
                return Mathf.Max(participantCount, multiplayerStartSlots);

            if (hasWorldSettings && maxPlayers > 1)
                return Mathf.Max(maxPlayers, multiplayerStartSlots);

            return 1;
        }

        public static string ResolveLocalPlayerId(
            GameLaunchMode mode,
            string sessionLocalPlayerId)
        {
            if (mode == GameLaunchMode.DirectGameplayTest)
                return DirectLocalOwnerId;

            return !string.IsNullOrWhiteSpace(sessionLocalPlayerId)
                ? sessionLocalPlayerId.Trim()
                : "local-player";
        }

        public static int ResolveLaunchParticipantCount(
            bool isDirectGameplay,
            bool hasWorldSettings,
            int maxPlayers)
        {
            if (isDirectGameplay)
                return Mathf.Max(2, maxPlayers);

            return hasWorldSettings
                ? Mathf.Max(1, maxPlayers)
                : 1;
        }

        public static int ResolveAssignmentCount(
            int availablePositions,
            bool isDirectGameplay,
            int launchParticipantCount)
        {
            if (availablePositions <= 0)
                return 0;

            return isDirectGameplay
                ? Mathf.Min(availablePositions, launchParticipantCount)
                : availablePositions;
        }

        public static GameplayParticipantSpec ResolveParticipant(
            int slotIndex,
            bool isDirectGameplay,
            Participant participant,
            string localPlayerId,
            int launchParticipantCount)
        {
            if (slotIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(slotIndex));

            if (isDirectGameplay)
            {
                return slotIndex == 0
                    ? new GameplayParticipantSpec(DirectLocalOwnerId, false)
                    : new GameplayParticipantSpec($"bot-{slotIndex:00}", true);
            }

            if (participant != null)
            {
                return new GameplayParticipantSpec(
                    participant.Identity?.PlayerId ?? string.Empty,
                    participant.IsBot);
            }

            if (slotIndex == 0)
            {
                return new GameplayParticipantSpec(
                    !string.IsNullOrWhiteSpace(localPlayerId)
                        ? localPlayerId.Trim()
                        : "local-player",
                    false);
            }

            return slotIndex < launchParticipantCount
                ? new GameplayParticipantSpec($"bot-{slotIndex:00}", true)
                : default;
        }
    }

    internal readonly struct GameplayParticipantSpec
    {
        public GameplayParticipantSpec(string participantId, bool isBot)
        {
            ParticipantId = participantId ?? string.Empty;
            IsBot = isBot;
        }

        public string ParticipantId { get; }
        public bool IsBot { get; }
    }
}
