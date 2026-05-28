using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IBootstrapOwnerIdResolver
    {
        string ResolveActiveOwnerId();
        bool CanRunBootstrapLogic();
    }

    internal sealed class BootstrapOwnerIdResolver : IBootstrapOwnerIdResolver
    {
        private const string DefaultOwnerId = "player_0";

        private readonly IConstructionService _constructionService;
        private readonly BootstrapStartingPositionState _startingPositionState;

    #pragma warning disable CS0649
        [InjectOptional] private ISessionManager _sessionManager;
    #pragma warning restore CS0649

        public BootstrapOwnerIdResolver(
            IConstructionService constructionService,
            BootstrapStartingPositionState startingPositionState)
        {
            _constructionService = constructionService;
            _startingPositionState = startingPositionState;
        }

        public string ResolveActiveOwnerId()
        {
            var assignments = _startingPositionState.SpawnAssignments;
            if (assignments != null && assignments.Count > 0)
                return NormalizeOwnerId(ResolveLocalActiveOwnerId(assignments));

            if (!string.IsNullOrWhiteSpace(_sessionManager?.LocalPlayerId))
                return NormalizeOwnerId(_sessionManager.LocalPlayerId);

            return NormalizeOwnerId(_constructionService.GetActiveOwner());
        }

        public bool CanRunBootstrapLogic()
        {
            var participants = _sessionManager?.Participants;
            if (participants == null || participants.Count == 0)
                return true;

            return _startingPositionState.IsSet;
        }

        private string ResolveLocalActiveOwnerId(IReadOnlyList<SpawnPositionAssignment> targets)
        {
            string localPlayerId = _sessionManager?.LocalPlayerId;

            if (targets != null)
            {
                for (int index = 0; index < targets.Count; index++)
                {
                    var target = targets[index];
                    if (target.IsBot)
                        continue;

                    if (!string.IsNullOrWhiteSpace(localPlayerId)
                        && string.Equals(target.ParticipantId, localPlayerId, StringComparison.Ordinal))
                    {
                        return ResolveSpawnOwnerId(target, index);
                    }
                }

                for (int index = 0; index < targets.Count; index++)
                {
                    var target = targets[index];
                    if (!target.IsBot)
                        return ResolveSpawnOwnerId(target, index);
                }

                if (targets.Count > 0)
                    return ResolveSpawnOwnerId(targets[0], 0);
            }

            return DefaultOwnerId;
        }

        private static string ResolveSpawnOwnerId(SpawnPositionAssignment assignment, int fallbackIndex)
        {
            if (!string.IsNullOrWhiteSpace(assignment.ParticipantId))
                return assignment.ParticipantId;

            if (assignment.IsBot)
                return $"bot-{assignment.SlotIndex:00}";

            return fallbackIndex == 0 ? DefaultOwnerId : $"spawn-slot-{assignment.SlotIndex:00}";
        }

        private static string NormalizeOwnerId(string ownerId)
            => string.IsNullOrWhiteSpace(ownerId) ? DefaultOwnerId : ownerId.Trim();
    }
}