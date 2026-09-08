using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    /// <summary>
    /// Handles ready-unit deployment for the canonical recruitment queue.
    /// It receives the same queue instance owned by UnitRecruitmentService;
    /// no parallel recruitment state or mutation authority is introduced.
    /// </summary>
    internal sealed class UnitRecruitmentDeploymentService
    {
        private readonly UnitRecruitmentQueueStateMachine _queue;
        private readonly UnitRecruitmentBuildingContextResolver _buildingContext;
        private readonly ITurnService _turns;
        private readonly IGameplayProgressClock _progressClock;
        private readonly SignalBus _signalBus;
        private readonly IUnitFactory _unitFactory;
        private readonly IUnitService _unitService;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IUnitPlacementValidator _placementValidator;

        public UnitRecruitmentDeploymentService(
            UnitRecruitmentQueueStateMachine queue,
            UnitRecruitmentBuildingContextResolver buildingContext,
            ITurnService turns,
            IGameplayProgressClock progressClock,
            SignalBus signalBus,
            IUnitFactory unitFactory,
            IUnitService unitService,
            IUnitOwnershipQuery ownership,
            IUnitPlacementValidator placementValidator)
        {
            _queue = queue
                ?? throw new ArgumentNullException(nameof(queue));
            _buildingContext = buildingContext
                ?? throw new ArgumentNullException(nameof(buildingContext));
            _turns = turns;
            _progressClock = progressClock;
            _signalBus = signalBus;
            _unitFactory = unitFactory;
            _unitService = unitService;
            _ownership = ownership;
            _placementValidator = placementValidator;
        }

        public IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot>
            GetDeploymentTiles(
                string ownerId,
                Vector2Int recruitingBuildingPosition,
                long queueId)
        {
            string owner = NormalizeRequiredId(ownerId);

            if (owner == null
                || queueId < 1
                || !_queue.TryPeekReady(
                    owner,
                    recruitingBuildingPosition,
                    out UnitRecruitmentQueueItemSnapshot ready)
                || ready.QueueId != queueId
                || !_buildingContext.TryResolveDeployment(
                    ready,
                    out UnitRecruitmentBuildingModule module,
                    out _))
            {
                return Array.Empty<UnitRecruitmentDeploymentTileSnapshot>();
            }

            List<Vector2Int> candidates =
                BuildSpawnCandidates(
                    ready.RecruitingBuildingPosition,
                    Math.Max(1, module.SpawnRadius));

            var result =
                new UnitRecruitmentDeploymentTileSnapshot[candidates.Count];

            for (int index = 0; index < candidates.Count; index++)
            {
                Vector2Int candidate = candidates[index];
                bool valid = ValidateDeploymentTile(
                    ready.UnitTypeId,
                    candidate,
                    out string reason);

                result[index] =
                    new UnitRecruitmentDeploymentTileSnapshot(
                        candidate,
                        valid,
                        reason);
            }

            return result;
        }

        public bool TryDeployReady(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            long queueId,
            Vector2Int targetPosition,
            out string unitId,
            out string reason)
        {
            unitId = null;
            reason = null;

            string owner = NormalizeRequiredId(ownerId);
            if (owner == null)
            {
                reason = "Recruitment owner is empty.";
                return false;
            }

            if (queueId < 1)
            {
                reason = "Recruitment queue id is invalid.";
                return false;
            }

            if (_progressClock?.IsRealtime != true && _turns == null)
            {
                reason = "Turn authority is unavailable for deployment.";
                return false;
            }

            if (_progressClock?.IsRealtime != true && !_turns.CanOwnerAct(owner, out reason))
                return false;

            if (!_queue.TryPeekReady(
                    owner,
                    recruitingBuildingPosition,
                    out UnitRecruitmentQueueItemSnapshot ready)
                || ready.QueueId != queueId)
            {
                reason =
                    "Requested recruitment job is not the ready queue head.";
                return false;
            }

            if (!_buildingContext.TryResolveDeployment(
                    ready,
                    out UnitRecruitmentBuildingModule module,
                    out reason))
            {
                return false;
            }

            if (!IsDeploymentCandidate(
                    ready.RecruitingBuildingPosition,
                    Math.Max(1, module.SpawnRadius),
                    targetPosition))
            {
                reason =
                    "Selected tile is outside the recruiting building deployment radius.";
                return false;
            }

            if (!ValidateDeploymentTile(
                    ready.UnitTypeId,
                    targetPosition,
                    out reason))
            {
                return false;
            }

            if (_unitFactory == null
                || _unitService == null
                || _ownership == null)
            {
                reason = "Unit deployment services are unavailable.";
                return false;
            }

            string forcedUnitId =
                BuildRecruitmentUnitId(
                    ready.QueueId,
                    ready.UnitTypeId);

            string existingType =
                _unitService.GetUnitTypeId(forcedUnitId);

            if (!string.IsNullOrWhiteSpace(existingType))
            {
                string existingOwner =
                    NormalizeRequiredId(
                        _ownership.GetUnitOwnerId(forcedUnitId));

                if (string.Equals(
                        existingType,
                        ready.UnitTypeId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        existingOwner,
                        ready.OwnerId,
                        StringComparison.Ordinal))
                {
                    unitId = forcedUnitId;

                    if (!_queue.TryTakeReady(
                            ready.OwnerId,
                            ready.RecruitingBuildingPosition,
                            ready.QueueId,
                            out UnitRecruitmentQueueItemSnapshot recovered))
                    {
                        reason =
                            "Existing deployment was found but the ready queue could not be reconciled.";
                        return false;
                    }

                    Vector2Int recoveredPosition = targetPosition;
                    _unitService.TryGetUnitPosition(
                        forcedUnitId,
                        out recoveredPosition);

                    FireDeployed(
                        recovered,
                        unitId,
                        recoveredPosition);
                    return true;
                }

                reason =
                    $"Stable deployment id collision for queue {ready.QueueId}.";
                return false;
            }

            try
            {
                unitId =
                    _unitFactory.CreateUnitWithId(
                        forcedUnitId,
                        ready.UnitTypeId,
                        targetPosition,
                        ready.OwnerId);

                if (string.IsNullOrWhiteSpace(unitId))
                {
                    reason =
                        "UnitFactory rejected the selected deployment tile.";
                    return false;
                }

                if (!_queue.TryTakeReady(
                        ready.OwnerId,
                        ready.RecruitingBuildingPosition,
                        ready.QueueId,
                        out UnitRecruitmentQueueItemSnapshot completed))
                {
                    reason =
                        "Unit was created but the ready queue could not be completed.";

                    Debug.LogError(
                        $"[UnitRecruitment] Spawned {unitId} but ready queue " +
                        $"{ready.QueueId} could not be completed.");
                    return false;
                }

                if (_progressClock?.IsRealtime != true)
                    _turns?.TryRecordAction(
                        owner,
                        "unit-recruit-deploy");

                FireDeployed(
                    completed,
                    unitId,
                    targetPosition);
                return true;
            }
            catch (Exception exception)
            {
                reason = exception.Message;

                Debug.LogError(
                    $"[UnitRecruitment] Manual deployment failed for queue " +
                    $"{ready.QueueId}: {exception}");

                unitId = null;
                return false;
            }
        }

        private bool ValidateDeploymentTile(
            string unitTypeId,
            Vector2Int position,
            out string reason)
        {
            if (_placementValidator == null)
            {
                reason = "Unit placement validator is unavailable.";
                return false;
            }

            return _placementValidator.CanDeployUnit(
                unitTypeId,
                position,
                out reason);
        }

        private static bool IsDeploymentCandidate(
            Vector2Int center,
            int spawnRadius,
            Vector2Int target)
        {
            int radius = Math.Max(1, spawnRadius);
            int dx = Math.Abs(target.x - center.x);
            int dy = Math.Abs(target.y - center.y);
            int ring = Math.Max(dx, dy);
            return ring >= 1 && ring <= radius;
        }

        private void FireDeployed(
            UnitRecruitmentQueueItemSnapshot item,
            string unitId,
            Vector2Int position)
        {
            _signalBus?.Fire(new UnitRecruitmentDeployedSignal
            {
                OwnerId = item.OwnerId,
                BuildingPosition = item.RecruitingBuildingPosition,
                QueueId = item.QueueId,
                UnitTypeId = item.UnitTypeId,
                UnitId = unitId,
                Position = position,
            });

            _signalBus?.Fire(new UnitRecruitmentQueueChangedSignal
            {
                OwnerId = item.OwnerId,
                BuildingPosition = item.RecruitingBuildingPosition,
                QueueId = item.QueueId,
                UnitTypeId = item.UnitTypeId,
                CompletedTurns = item.CompletedTurns,
                TrainingTurns = item.TrainingTurns,
                IsReady = false,
            });
        }

        internal static List<Vector2Int> BuildSpawnCandidates(
            Vector2Int center,
            int spawnRadius)
        {
            int radius = Math.Max(1, spawnRadius);
            var result = new List<Vector2Int>();

            for (int ring = 1; ring <= radius; ring++)
            {
                int minX = center.x - ring;
                int maxX = center.x + ring;
                int minY = center.y - ring;
                int maxY = center.y + ring;

                for (int x = minX; x <= maxX; x++)
                    result.Add(new Vector2Int(x, maxY));
                for (int y = maxY - 1; y >= minY; y--)
                    result.Add(new Vector2Int(maxX, y));
                for (int x = maxX - 1; x >= minX; x--)
                    result.Add(new Vector2Int(x, minY));
                for (int y = minY + 1; y < maxY; y++)
                    result.Add(new Vector2Int(minX, y));
            }

            return result;
        }

        internal static string BuildRecruitmentUnitId(
            long queueId,
            string unitTypeId)
        {
            string type =
                NormalizeRequiredId(unitTypeId)
                ?? "unit";

            return $"recruit_{Math.Max(1L, queueId):D10}_{type}";
        }

        private static string NormalizeRequiredId(string value)
            => string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
    }
}
