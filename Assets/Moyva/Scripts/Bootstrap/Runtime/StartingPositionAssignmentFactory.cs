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
            Debug.Log($"{DirectDiagTag} AssignmentFactory.ENTER positions={positions.Count}, participants={participants?.Count ?? 0}, mode={Kruty1918.Moyva.SaveSystem.GameLaunchContext.Mode}, localPlayerId={localPlayerId}.");
            var assignments = new SpawnPositionAssignment[positions.Count];
            int participantCount = participants?.Count ?? 0;
            int launchParticipantCount = hasWorldSettings
                ? Mathf.Max(1, maxPlayers)
                : 1;

            for (int index = 0; index < positions.Count; index++)
            {
                string participantId = string.Empty;
                bool isBot = false;

                if (participants != null && index < participantCount)
                {
                    participantId = participants[index].Identity?.PlayerId ?? string.Empty;
                    isBot = participants[index].IsBot;
                }
                else if (index == 0)
                {
                    participantId = !string.IsNullOrEmpty(localPlayerId) ? localPlayerId : "local-player";
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
            string localAssignment = "<none>";
            for (int index = 0; index < assignments.Length; index++)
            {
                if (assignments[index].IsBot)
                    botAssignments++;

                if (assignments[index].ParticipantId == localPlayerId || (string.IsNullOrEmpty(localPlayerId) && index == 0))
                    localAssignment = assignments[index].Position.ToString();
            }

            Debug.Log($"{DirectDiagTag} AssignmentFactory.RESULT assignments={assignments.Length}, localAssignment={localAssignment}, botAssignments={botAssignments}.");
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
