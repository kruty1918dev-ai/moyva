using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitRecruitmentService :
        IUnitRecruitmentService,
        IUnitRecruitmentStateStore,
        ITurnParticipant,
        IInitializable,
        IDisposable
    {
        private readonly UnitRecruitmentQueueStateMachine _queue = new();
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IConstructionSaveSnapshotSource _constructionSnapshot;
        private readonly IConstructionLifecycle _constructionLifecycle;
        private readonly IEconomyInfoMediator _economy;
        private readonly ITurnService _turns;
        private readonly SignalBus _signalBus;
        private readonly IUnitFactory _unitFactory;
        private readonly IUnitService _unitService;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IGridService _grid;
        private readonly IObjectsMapService _objectsMap;

        [Inject]
        public UnitRecruitmentService(
            IUnitClassConfig unitClassConfig,
            [InjectOptional] IBuildingRegistry buildingRegistry = null,
            [InjectOptional] IConstructionSaveSnapshotSource constructionSnapshot = null,
            [InjectOptional] IConstructionLifecycle constructionLifecycle = null,
            [InjectOptional] IEconomyInfoMediator economy = null,
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] SignalBus signalBus = null,
            [InjectOptional] IUnitFactory unitFactory = null,
            [InjectOptional] IUnitService unitService = null,
            [InjectOptional] IUnitOwnershipQuery ownership = null,
            [InjectOptional] IGridService grid = null,
            [InjectOptional] IObjectsMapService objectsMap = null)
        {
            _unitClassConfig = unitClassConfig;
            _buildingRegistry = buildingRegistry;
            _constructionSnapshot = constructionSnapshot;
            _constructionLifecycle = constructionLifecycle;
            _economy = economy;
            _turns = turns;
            _signalBus = signalBus;
            _unitFactory = unitFactory;
            _unitService = unitService;
            _ownership = ownership;
            _grid = grid;
            _objectsMap = objectsMap;
        }

        public int TurnOrder => 30;

        public void Initialize()
            => _signalBus?.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);

        public void Dispose()
            => _signalBus?.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);

        public bool TryEnqueue(string ownerId, Vector2Int recruitingBuildingPosition, string unitTypeId, out string reason)
        {
            reason = null;
            string owner = NormalizeRequiredId(ownerId);
            string unitType = NormalizeRequiredId(unitTypeId);
            if (owner == null)
            {
                reason = "Recruitment owner is empty.";
                return false;
            }
            if (unitType == null)
            {
                reason = "Unit type is empty.";
                return false;
            }
            if (_turns == null)
            {
                reason = "Turn authority is unavailable for recruitment.";
                return false;
            }
            if (!_turns.CanOwnerAct(owner, out reason))
                return false;
            if (_constructionSnapshot == null || _buildingRegistry == null || _constructionLifecycle == null)
            {
                reason = "Construction recruitment context is unavailable.";
                return false;
            }

            if (!TryFindRecruitingPlacement(recruitingBuildingPosition, out ConstructionSavedPlacement placement))
            {
                reason = "Recruiting building is not a committed construction placement.";
                return false;
            }
            if (!string.Equals(NormalizeRequiredId(placement.OwnerId), owner, StringComparison.Ordinal))
            {
                reason = "Recruiting building belongs to another owner.";
                return false;
            }
            if (!_constructionLifecycle.IsOperational(recruitingBuildingPosition))
            {
                reason = "Recruiting building is still under construction.";
                return false;
            }

            BuildingDefinition definition = _buildingRegistry.GetById(placement.BuildingId);
            if (definition == null
                || !BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out UnitRecruitmentBuildingModule recruitmentModule))
            {
                reason = "Building has no enabled unit recruitment module.";
                return false;
            }

            if (!TryFindRecipe(recruitmentModule, unitType, out UnitRecruitmentRecipeDefinition recipe))
            {
                reason = $"Building cannot recruit unit type '{unitType}'.";
                return false;
            }
            if (_unitClassConfig?.GetConfig(unitType) == null)
            {
                reason = $"Unit type '{unitType}' is not registered.";
                return false;
            }

            int capacity = Math.Max(1, recruitmentModule.QueueCapacity);
            if (!_queue.CanEnqueue(owner, recruitingBuildingPosition, capacity, out reason))
                return false;

            Dictionary<string, float> costs = BuildCostMap(recipe.Costs);
            if (!TryConsumeRecruitmentCosts(owner, recruitingBuildingPosition, costs, out reason))
                return false;

            _queue.EnqueueValidated(
                owner,
                recruitingBuildingPosition,
                placement.BuildingId,
                unitType,
                Math.Max(1, recipe.TrainingTurns),
                Math.Max(1L, _turns.GlobalTurn));

            if (!_turns.TryRecordAction(owner, "unit-recruit-enqueue"))
            {
                Debug.LogWarning(
                    "[UnitRecruitment] Queue commit succeeded but turn action telemetry was rejected after commit.");
            }
            return true;
        }

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetQueue(string ownerId, Vector2Int recruitingBuildingPosition)
        {
            string owner = NormalizeRequiredId(ownerId);
            return owner == null
                ? Array.Empty<UnitRecruitmentQueueItemSnapshot>()
                : _queue.GetQueue(owner, recruitingBuildingPosition);
        }

        public bool TryPeekReady(string ownerId, Vector2Int recruitingBuildingPosition, out UnitRecruitmentQueueItemSnapshot item)
        {
            string owner = NormalizeRequiredId(ownerId);
            if (owner != null)
                return _queue.TryPeekReady(owner, recruitingBuildingPosition, out item);
            item = default;
            return false;
        }

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> CaptureState()
            => _queue.CaptureAll();

        public void RestoreState(IReadOnlyList<UnitRecruitmentQueueItemSnapshot> items)
            => _queue.RestoreAll(items);

        public void OnTurnStarted(TurnContext context)
        {
            string owner = NormalizeRequiredId(context.Faction.OwnerId);
            if (owner == null)
                return;

            _queue.AdvanceOwnerTurn(owner, context.GlobalTurn);
            DeployReadyForOwner(owner);
        }

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }

        private void DeployReadyForOwner(string ownerId)
        {
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> readyHeads = _queue.GetReadyHeads(ownerId);
            for (int index = 0; index < readyHeads.Count; index++)
                TryDeployReadyItem(readyHeads[index]);
        }

        private bool TryDeployReadyItem(UnitRecruitmentQueueItemSnapshot ready)
        {
            if (_unitFactory == null || _unitService == null || _ownership == null || _grid == null || _objectsMap == null)
                return false;

            if (!TryResolveDeploymentModule(ready, out UnitRecruitmentBuildingModule module, out string reason))
            {
                Debug.LogWarning($"[UnitRecruitment] Ready queue {ready.QueueId} cannot deploy: {reason}");
                return false;
            }

            string forcedUnitId = BuildRecruitmentUnitId(ready.QueueId, ready.UnitTypeId);
            string existingType = _unitService.GetUnitTypeId(forcedUnitId);
            if (!string.IsNullOrWhiteSpace(existingType))
            {
                string existingOwner = NormalizeRequiredId(_ownership.GetUnitOwnerId(forcedUnitId));
                if (string.Equals(existingType, ready.UnitTypeId, StringComparison.Ordinal)
                    && string.Equals(existingOwner, ready.OwnerId, StringComparison.Ordinal))
                {
                    return _queue.TryTakeReady(
                        ready.OwnerId,
                        ready.RecruitingBuildingPosition,
                        ready.QueueId,
                        out _);
                }

                Debug.LogError($"[UnitRecruitment] Stable deployment id collision for queue {ready.QueueId}: {forcedUnitId}.");
                return false;
            }

            List<Vector2Int> candidates = BuildSpawnCandidates(
                ready.RecruitingBuildingPosition,
                Math.Max(1, module.SpawnRadius));
            var validCandidates = new List<Vector2Int>();
            for (int index = 0; index < candidates.Count; index++)
            {
                Vector2Int candidate = candidates[index];
                if (!_grid.ContainsCell(candidate) || _objectsMap.IsOccupied(candidate))
                    continue;
                validCandidates.Add(candidate);
            }
            if (validCandidates.Count == 0)
                return false;

            // Keep the ready entry until the deterministic unit has been created.
            // A re-entrant save during UnitCreatedSignal may therefore capture either
            // queue-only or queue+unit. Both are recoverable: queue+unit is reconciled
            // by the stable unit id on the next owner turn instead of spawning twice.
            try
            {
                for (int index = 0; index < validCandidates.Count; index++)
                {
                    Vector2Int candidate = validCandidates[index];
                    if (_objectsMap.IsOccupied(candidate))
                        continue;

                    string unitId = _unitFactory.CreateUnitWithId(
                        forcedUnitId,
                        ready.UnitTypeId,
                        candidate,
                        ready.OwnerId);
                    if (string.IsNullOrWhiteSpace(unitId))
                        continue;

                    if (!_queue.TryTakeReady(
                            ready.OwnerId,
                            ready.RecruitingBuildingPosition,
                            ready.QueueId,
                            out _))
                    {
                        Debug.LogError(
                            $"[UnitRecruitment] Spawned {unitId} but ready queue {ready.QueueId} could not be completed; stable-id reconciliation will retry cleanup.");
                    }
                    return true;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[UnitRecruitment] Deployment failed for queue {ready.QueueId}: {exception}");
            }

            return false;
        }

        private bool TryResolveDeploymentModule(
            UnitRecruitmentQueueItemSnapshot ready,
            out UnitRecruitmentBuildingModule module,
            out string reason)
        {
            module = null;
            reason = null;
            if (_constructionSnapshot == null || _buildingRegistry == null || _constructionLifecycle == null)
            {
                reason = "construction context is unavailable";
                return false;
            }
            if (!TryFindRecruitingPlacement(ready.RecruitingBuildingPosition, out ConstructionSavedPlacement placement))
            {
                reason = "recruiting building no longer exists";
                return false;
            }
            if (!string.Equals(NormalizeRequiredId(placement.OwnerId), ready.OwnerId, StringComparison.Ordinal))
            {
                reason = "recruiting building owner changed";
                return false;
            }
            if (!string.IsNullOrWhiteSpace(ready.RecruitingBuildingId)
                && !string.Equals(placement.BuildingId, ready.RecruitingBuildingId, StringComparison.Ordinal))
            {
                reason = "recruiting building identity changed";
                return false;
            }
            if (!_constructionLifecycle.IsOperational(ready.RecruitingBuildingPosition))
            {
                reason = "recruiting building is not operational";
                return false;
            }
            BuildingDefinition definition = _buildingRegistry.GetById(placement.BuildingId);
            if (definition == null
                || !BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out module))
            {
                reason = "recruiting module is unavailable";
                return false;
            }
            if (_unitClassConfig?.GetConfig(ready.UnitTypeId) == null)
            {
                reason = "unit type is no longer registered";
                return false;
            }
            return true;
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
        {
            if (_queue.RemoveBuildingQueues(signal.Position))
            {
                Debug.Log($"[UnitRecruitment] Dropped paid recruitment queue at demolished building {signal.Position}.");
            }
        }

        private bool TryFindRecruitingPlacement(Vector2Int position, out ConstructionSavedPlacement placement)
        {
            IReadOnlyList<ConstructionSavedPlacement> placements = _constructionSnapshot.GetSavedPlacements();
            if (placements != null)
            {
                for (int index = 0; index < placements.Count; index++)
                {
                    if (placements[index].Position != position)
                        continue;
                    placement = placements[index];
                    return true;
                }
            }
            placement = default;
            return false;
        }

        private static bool TryFindRecipe(
            UnitRecruitmentBuildingModule module,
            string unitTypeId,
            out UnitRecruitmentRecipeDefinition recipe)
        {
            if (module?.Recipes != null)
            {
                for (int index = 0; index < module.Recipes.Count; index++)
                {
                    UnitRecruitmentRecipeDefinition candidate = module.Recipes[index];
                    if (candidate == null
                        || !string.Equals(NormalizeRequiredId(candidate.UnitTypeId), unitTypeId, StringComparison.Ordinal))
                    {
                        continue;
                    }
                    recipe = candidate;
                    return true;
                }
            }
            recipe = null;
            return false;
        }

        internal static Dictionary<string, float> BuildCostMap(IReadOnlyList<BuildingResourceAmount> source)
        {
            var result = new Dictionary<string, float>(StringComparer.Ordinal);
            if (source == null)
                return result;
            for (int index = 0; index < source.Count; index++)
            {
                BuildingResourceAmount entry = source[index];
                string resourceId = NormalizeRequiredId(entry?.ResourceId);
                if (resourceId == null || entry.Amount <= 0)
                    continue;
                if (result.TryGetValue(resourceId, out float current))
                    result[resourceId] = current + entry.Amount;
                else
                    result.Add(resourceId, entry.Amount);
            }
            return result;
        }

        private bool TryConsumeRecruitmentCosts(
            string ownerId,
            Vector2Int buildingPosition,
            IReadOnlyDictionary<string, float> costs,
            out string reason)
        {
            reason = null;
            if (costs == null || costs.Count == 0)
                return true;
            if (_economy == null)
            {
                reason = "Economy is unavailable for recruitment costs.";
                return false;
            }
            if (!_economy.OwnerHasAnyWarehouse(ownerId))
                return _economy.TryConsumeOwnerPoolResources(ownerId, costs, out reason);

            if (!_economy.TryResolveConstructionSettlement(
                    buildingPosition,
                    ownerId,
                    out EconomySettlementContext settlement)
                || string.IsNullOrWhiteSpace(settlement.SettlementId)
                || !string.Equals(NormalizeRequiredId(settlement.OwnerId), ownerId, StringComparison.Ordinal))
            {
                reason = "No owned settlement is available to fund recruitment at this building.";
                return false;
            }
            return _economy.TryConsumeSettlementResources(settlement.SettlementId, costs, out reason);
        }

        internal static List<Vector2Int> BuildSpawnCandidates(Vector2Int center, int spawnRadius)
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

        internal static string BuildRecruitmentUnitId(long queueId, string unitTypeId)
        {
            string type = NormalizeRequiredId(unitTypeId) ?? "unit";
            return $"recruit_{Math.Max(1L, queueId):D10}_{type}";
        }

        private static string NormalizeRequiredId(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
