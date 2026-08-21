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
            bool isDirectGameplay =
                Kruty1918.Moyva.SaveSystem.GameLaunchContext.Mode ==
                Kruty1918.Moyva.SaveSystem.GameLaunchMode.DirectGameplayTest;

            int participantCount = participants?.Count ?? 0;
            int launchParticipantCount = isDirectGameplay
                ? Mathf.Max(2, maxPlayers)
                : hasWorldSettings
                    ? Mathf.Max(1, maxPlayers)
                    : 1;

            int assignmentCount = isDirectGameplay
                ? Mathf.Min(positions.Count, launchParticipantCount)
                : positions.Count;

            Debug.Log(
                $"{DirectDiagTag} AssignmentFactory.ENTER positions={positions.Count}, " +
                $"participants={participantCount}, mode={Kruty1918.Moyva.SaveSystem.GameLaunchContext.Mode}, " +
                $"localPlayerId={localPlayerId}, launchParticipantCount={launchParticipantCount}, " +
                $"assignmentCount={assignmentCount}, direct={isDirectGameplay}.");

            var assignments = new SpawnPositionAssignment[assignmentCount];

            for (int index = 0; index < assignmentCount; index++)
            {
                string participantId = string.Empty;
                bool isBot = false;

                if (isDirectGameplay)
                {
                    // Direct Gameplay intentionally ignores stale editor/session participants.
                    if (index == 0)
                    {
                        participantId = "player_0";
                        isBot = false;
                    }
                    else
                    {
                        participantId = $"bot-{index:00}";
                        isBot = true;
                    }
                }
                else if (participants != null && index < participantCount)
                {
                    participantId =
                        participants[index].Identity?.PlayerId ??
                        string.Empty;
                    isBot = participants[index].IsBot;
                }
                else if (index == 0)
                {
                    participantId =
                        !string.IsNullOrEmpty(localPlayerId)
                            ? localPlayerId
                            : "local-player";
                }
                else if (index < launchParticipantCount)
                {
                    participantId = $"bot-{index:00}";
                    isBot = true;
                }

                assignments[index] = new SpawnPositionAssignment
                {
                    SlotIndex = index,
                    ParticipantId = participantId,
                    IsBot = isBot,
                    Position = positions[index],
                };
            }

            int botAssignments = 0;
            int humanAssignments = 0;
            string localAssignment = "<none>";

            for (int index = 0; index < assignments.Length; index++)
            {
                if (assignments[index].IsBot)
                {
                    botAssignments++;
                }
                else
                {
                    humanAssignments++;
                    if (localAssignment == "<none>")
                        localAssignment = assignments[index].Position.ToString();
                }
            }

            Debug.Log(
                $"{DirectDiagTag} AssignmentFactory.RESULT assignments={assignments.Length}, " +
                $"humans={humanAssignments}, bots={botAssignments}, localAssignment={localAssignment}.");

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
