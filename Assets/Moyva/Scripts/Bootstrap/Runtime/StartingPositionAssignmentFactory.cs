using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionAssignmentFactory
    {
        SpawnPositionAssignment[] BuildSpawnAssignments(
            IReadOnlyList<Vector2Int> positions,
            IReadOnlyList<Participant> participants,
            string localPlayerId,
            bool hasWorldSettings,
            int maxPlayers);

        SpawnPositionAssignment[] CopySpawnAssignments(IReadOnlyList<SpawnPositionAssignment> assignments);
    }

    internal sealed class StartingPositionAssignmentFactory
        : IStartingPositionAssignmentFactory
    {
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";

        public SpawnPositionAssignment[] BuildSpawnAssignments(
            IReadOnlyList<Vector2Int> positions,
            IReadOnlyList<Participant> participants,
            string localPlayerId,
            bool hasWorldSettings,
            int maxPlayers)
        {
            bool isDirectGameplay = GameplayLaunchTopology.IsDirectGameplay(
                Kruty1918.Moyva.SaveSystem.GameLaunchContext.Mode,
                Kruty1918.Moyva.SaveSystem.GameLaunchContext.Source,
                hasWorldSettings,
                maxPlayers,
                localPlayerId);

            int participantCount = participants?.Count ?? 0;
            int launchParticipantCount =
                GameplayLaunchTopology.ResolveLaunchParticipantCount(
                    isDirectGameplay,
                    participantCount);

            int assignmentCount =
                GameplayLaunchTopology.ResolveAssignmentCount(
                    positions.Count,
                    isDirectGameplay,
                    launchParticipantCount);

            var assignments = new SpawnPositionAssignment[assignmentCount];

            for (int index = 0; index < assignmentCount; index++)
            {
                Participant participant =
                    participants != null && index < participantCount
                        ? participants[index]
                        : null;

                GameplayParticipantSpec spec =
                    GameplayLaunchTopology.ResolveParticipant(
                        index,
                        isDirectGameplay,
                        participant,
                        localPlayerId,
                        launchParticipantCount);
                string participantId = ResolveAssignmentParticipantId(
                    index,
                    isDirectGameplay,
                    spec.ParticipantId,
                    localPlayerId);

                assignments[index] = new SpawnPositionAssignment
                {
                    SlotIndex = index,
                    ParticipantId = participantId,
                    Position = positions[index],
                };
            }

            return assignments;
        }

        private static string ResolveAssignmentParticipantId(
            int slotIndex,
            bool isDirectGameplay,
            string resolvedParticipantId,
            string localPlayerId)
        {
            if (!isDirectGameplay
                && slotIndex == 0
                && Kruty1918.Moyva.SaveSystem.GameLaunchContext.HasLocalPlayerRole
                && Kruty1918.Moyva.SaveSystem.GameLaunchContext.IsLocalPlayerHost
                && !string.IsNullOrWhiteSpace(localPlayerId))
            {
                return localPlayerId.Trim();
            }

            return resolvedParticipantId;
        }

        public SpawnPositionAssignment[] CopySpawnAssignments(IReadOnlyList<SpawnPositionAssignment> assignments)
        {
            var copy = new SpawnPositionAssignment[assignments.Count];
            for (int index = 0; index < assignments.Count; index++)
                copy[index] = assignments[index];

            return copy;
        }
    }
}
