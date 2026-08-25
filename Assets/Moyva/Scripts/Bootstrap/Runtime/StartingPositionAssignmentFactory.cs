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

            Debug.Log(
                $"{DirectDiagTag} AssignmentFactory.ENTER positions={positions.Count}, " +
                $"participants={participantCount}, mode={Kruty1918.Moyva.SaveSystem.GameLaunchContext.Mode}, source={Kruty1918.Moyva.SaveSystem.GameLaunchContext.Source}, " +
                $"localPlayerId={localPlayerId}, launchParticipantCount={launchParticipantCount}, " +
                $"assignmentCount={assignmentCount}, direct={isDirectGameplay}.");

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

                assignments[index] = new SpawnPositionAssignment
                {
                    SlotIndex = index,
                    ParticipantId = spec.ParticipantId,
                    Position = positions[index],
                };
            }

            return assignments;
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
