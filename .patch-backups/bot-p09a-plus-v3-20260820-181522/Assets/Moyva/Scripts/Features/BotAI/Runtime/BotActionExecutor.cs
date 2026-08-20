using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotActionExecutor : IBotActionExecutor
    {
        private readonly IConstructionService _construction;
        private readonly IConstructionPlacementQuery _placementQuery;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IUnitMovementService _movement;
        private readonly IUnitMovementQuery _movementQuery;
        private readonly ICombatCommandService _combat;

        [Inject]
        public BotActionExecutor(
            [InjectOptional] IConstructionService construction = null,
            [InjectOptional] IConstructionPlacementQuery placementQuery = null,
            [InjectOptional] IUnitRecruitmentService recruitment = null,
            [InjectOptional] IUnitMovementService movement = null,
            [InjectOptional] IUnitMovementQuery movementQuery = null,
            [InjectOptional] ICombatCommandService combat = null)
        {
            _construction = construction;
            _placementQuery = placementQuery;
            _recruitment = recruitment;
            _movement = movement;
            _movementQuery = movementQuery;
            _combat = combat;
        }

        public async Task<BotActionExecutionResult> ExecuteAsync(
            string ownerId,
            BotActionCandidate action,
            CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return BotActionExecutionResult.Rejected("Bot action execution was cancelled.");

            return action.Kind switch
            {
                BotActionKind.Build => ExecuteBuild(ownerId, action),
                BotActionKind.DeployReadyUnit => ExecuteDeploy(ownerId, action),
                BotActionKind.Move => await ExecuteMoveAsync(action, token),
                BotActionKind.ScoutMove => await ExecuteMoveAsync(action, token),
                BotActionKind.Attack => await ExecuteAttackAsync(ownerId, action, token),
                BotActionKind.Hold => BotActionExecutionResult.NoOp("Hold."),
                _ => BotActionExecutionResult.Rejected($"Unsupported bot action kind '{action.Kind}'."),
            };
        }

        private async Task<BotActionExecutionResult> ExecuteAttackAsync(
            string ownerId,
            BotActionCandidate action,
            CancellationToken token)
        {
            if (_combat == null)
                return BotActionExecutionResult.Rejected("Combat command service unavailable.");
            if (string.IsNullOrWhiteSpace(action.ActorId) || string.IsNullOrWhiteSpace(action.TargetId))
                return BotActionExecutionResult.Rejected("Attack action is missing attacker or target id.");

            CombatCommandResult result = await _combat.ExecuteAsync(ownerId, action.ActorId, action.TargetId, token);
            return result.Succeeded
                ? BotActionExecutionResult.Success("Attack executed.")
                : BotActionExecutionResult.Rejected(result.Reason);
        }

        private async Task<BotActionExecutionResult> ExecuteMoveAsync(
            BotActionCandidate action,
            CancellationToken token)
        {
            if (_movement == null || _movementQuery == null)
                return BotActionExecutionResult.Rejected("Movement services unavailable.");
            if (string.IsNullOrWhiteSpace(action.ActorId) || !action.TargetCell.HasValue)
                return BotActionExecutionResult.Rejected("Move action is missing unit id or target cell.");

            if (!IsReachable(action.ActorId, action.TargetCell.Value))
                return BotActionExecutionResult.Rejected("Movement target no longer reachable.");

            await _movement.MoveUnitAsync(action.ActorId, action.TargetCell.Value, token);
            return BotActionExecutionResult.Success("Unit moved.");
        }

        private bool IsReachable(string unitId, Vector2Int target)
        {
            IReadOnlyList<UnitMovementTileSnapshot> tiles = _movementQuery.GetMovementTiles(unitId);
            if (tiles == null)
                return false;

            for (int index = 0; index < tiles.Count; index++)
            {
                if (tiles[index].IsReachable && tiles[index].Position == target)
                    return true;
            }

            return false;
        }

        private BotActionExecutionResult ExecuteBuild(string ownerId, BotActionCandidate action)
        {
            if (_construction == null || _placementQuery == null)
                return BotActionExecutionResult.Rejected("Construction services unavailable.");
            if (string.IsNullOrWhiteSpace(action.DefinitionId) || !action.TargetCell.HasValue)
                return BotActionExecutionResult.Rejected("Build action is missing building id or target cell.");

            Vector2Int target = action.TargetCell.Value;
            var request = new ConstructionPlacementQueryRequest(
                action.DefinitionId,
                target,
                includeResources: true,
                includeDetails: false,
                ownerId: ownerId,
                attemptSource: ConstructionPlacementAttemptSource.DirectPlace);
            ConstructionPlacementQueryResult placement = _placementQuery.EvaluatePlacement(request);
            if (!placement.CanCommit)
                return BotActionExecutionResult.Rejected(placement.Reason ?? "Build placement no longer valid.");

            bool placed = _construction.TryDirectPlace(action.DefinitionId, target, ownerId);
            return placed
                ? BotActionExecutionResult.Success("Building placed.")
                : BotActionExecutionResult.Rejected(_construction.GetLastActionMessage() ?? "Construction commit failed.");
        }

        private BotActionExecutionResult ExecuteDeploy(string ownerId, BotActionCandidate action)
        {
            if (_recruitment == null)
                return BotActionExecutionResult.Rejected("Recruitment service unavailable.");
            if (!action.TargetCell.HasValue)
                return BotActionExecutionResult.Rejected("Deploy action is missing target cell.");

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> ready = _recruitment.GetReadyItems(ownerId);
            if (ready == null || ready.Count == 0)
                return BotActionExecutionResult.Rejected("No ready recruitment item remains.");

            UnitRecruitmentQueueItemSnapshot? selected = FindReadyItem(ready, action);
            if (!selected.HasValue)
                return BotActionExecutionResult.Rejected("Ready recruitment item no longer matches candidate.");

            UnitRecruitmentQueueItemSnapshot item = selected.Value;
            IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> tiles =
                _recruitment.GetDeploymentTiles(ownerId, item.RecruitingBuildingPosition, item.QueueId);
            if (!IsValidDeploymentTile(tiles, action.TargetCell.Value))
                return BotActionExecutionResult.Rejected("Deployment tile no longer valid.");

            bool deployed = _recruitment.TryDeployReady(
                ownerId,
                item.RecruitingBuildingPosition,
                item.QueueId,
                action.TargetCell.Value,
                out _,
                out string reason);
            return deployed
                ? BotActionExecutionResult.Success("Ready unit deployed.")
                : BotActionExecutionResult.Rejected(reason ?? "Deployment failed.");
        }

        private static UnitRecruitmentQueueItemSnapshot? FindReadyItem(
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> ready,
            BotActionCandidate action)
        {
            for (int index = 0; index < ready.Count; index++)
            {
                UnitRecruitmentQueueItemSnapshot item = ready[index];
                if (!item.IsReady)
                    continue;
                if (!string.IsNullOrWhiteSpace(action.DefinitionId)
                    && !string.Equals(item.UnitTypeId, action.DefinitionId, StringComparison.Ordinal))
                {
                    continue;
                }

                string expectedPrefix = $"deploy:{item.QueueId}:";
                if (action.CandidateId.StartsWith(expectedPrefix, StringComparison.Ordinal))
                    return item;
            }

            return null;
        }

        private static bool IsValidDeploymentTile(
            IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> tiles,
            Vector2Int target)
        {
            if (tiles == null)
                return false;

            for (int index = 0; index < tiles.Count; index++)
            {
                if (tiles[index].IsValid && tiles[index].Position == target)
                    return true;
            }

            return false;
        }
    }
}
