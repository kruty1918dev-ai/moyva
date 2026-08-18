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
        private readonly IUnitPlacementValidator _placementValidator;

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
            [InjectOptional] IObjectsMapService objectsMap = null,
            [InjectOptional] IUnitPlacementValidator placementValidator = null)
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
            _placementValidator = placementValidator;
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

            UnitRecruitmentQueueItemSnapshot enqueued = _queue.EnqueueValidated(
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

            FireQueueChanged(enqueued);
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

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetReadyItems(string ownerId)
        {
            string owner = NormalizeRequiredId(ownerId);
            return owner == null
                ? Array.Empty<UnitRecruitmentQueueItemSnapshot>()
                : _queue.GetReadyHeads(owner);
        }

        public IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> GetDeploymentTiles(
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
                || !TryResolveDeploymentModule(
                    ready,
                    out UnitRecruitmentBuildingModule module,
                    out _))
            {
                return Array.Empty<UnitRecruitmentDeploymentTileSnapshot>();
            }

            List<Vector2Int> candidates = BuildSpawnCandidates(
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

                result[index] = new UnitRecruitmentDeploymentTileSnapshot(
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

            if (_turns == null)
            {
                reason = "Turn authority is unavailable for deployment.";
                return false;
            }

            if (!_turns.CanOwnerAct(owner, out reason))
                return false;

            if (!_queue.TryPeekReady(
                    owner,
                    recruitingBuildingPosition,
                    out UnitRecruitmentQueueItemSnapshot ready)
                || ready.QueueId != queueId)
            {
                reason = "Requested recruitment job is not the ready queue head.";
                return false;
            }

            if (!TryResolveDeploymentModule(
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
                reason = "Selected tile is outside the recruiting building deployment radius.";
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

            string forcedUnitId = BuildRecruitmentUnitId(
                ready.QueueId,
                ready.UnitTypeId);
            string existingType =
                _unitService.GetUnitTypeId(forcedUnitId);

            if (!string.IsNullOrWhiteSpace(existingType))
            {
                string existingOwner = NormalizeRequiredId(
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
                unitId = _unitFactory.CreateUnitWithId(
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
                        $"[UnitRecruitment] Spawned {unitId} but ready queue {ready.QueueId} could not be completed.");
                    return false;
                }

                _turns.TryRecordAction(
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
                    $"[UnitRecruitment] Manual deployment failed for queue {ready.QueueId}: {exception}");
                unitId = null;
                return false;
            }
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

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> readyBefore =
                _queue.GetReadyHeads(owner);
            var readyIdsBefore = new HashSet<long>();
            for (int index = 0; index < readyBefore.Count; index++)
                readyIdsBefore.Add(readyBefore[index].QueueId);

            bool progressed = _queue.AdvanceOwnerTurn(
                owner,
                context.GlobalTurn);

            if (!progressed)
                return;

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> state =
                _queue.CaptureAll();
            for (int index = 0; index < state.Count; index++)
            {
                UnitRecruitmentQueueItemSnapshot item = state[index];
                if (!string.Equals(
                        item.OwnerId,
                        owner,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                FireQueueChanged(item);
            }

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> readyAfter =
                _queue.GetReadyHeads(owner);
            for (int index = 0; index < readyAfter.Count; index++)
            {
                UnitRecruitmentQueueItemSnapshot ready = readyAfter[index];
                if (readyIdsBefore.Contains(ready.QueueId))
                    continue;

                _signalBus?.Fire(new UnitRecruitmentReadySignal
                {
                    OwnerId = ready.OwnerId,
                    BuildingPosition = ready.RecruitingBuildingPosition,
                    QueueId = ready.QueueId,
                    UnitTypeId = ready.UnitTypeId,
                });
            }

            // P24A: ready recruitment jobs deliberately persist in the queue.
            // Unit creation happens only through TryDeployReady after an explicit
            // player/bot tile selection.
        }

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }

        private bool ValidateDeploymentTile(
            string unitTypeId,
            Vector2Int position,
            out string reason)
        {
            if (_placementValidator != null)
            {
                return _placementValidator.CanDeployUnit(
                    unitTypeId,
                    position,
                    out reason);
            }

            // Conservative fallback for scenes/tests that have not yet bound
            // IUnitPlacementValidator.
            if (_grid == null || !_grid.ContainsCell(position))
            {
                reason = "Тайл знаходиться за межами карти.";
                return false;
            }

            if (!_grid.TryGetTileTypeId(
                    position,
                    out string tileTypeId)
                || string.IsNullOrWhiteSpace(tileTypeId))
            {
                reason = "На клітинці немає валідного типу тайла.";
                return false;
            }

            if (_objectsMap == null)
            {
                reason = "Карта зайнятості об'єктів недоступна.";
                return false;
            }

            if (_objectsMap.IsOccupied(position))
            {
                _objectsMap.TryGetOccupant(
                    position,
                    out string occupantId);

                reason = string.IsNullOrWhiteSpace(occupantId)
                    ? "Клітинка вже зайнята."
                    : $"Клітинка зайнята об'єктом '{occupantId}'.";
                return false;
            }

            reason = null;
            return true;
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

        private void FireQueueChanged(
            UnitRecruitmentQueueItemSnapshot item)
        {
            _signalBus?.Fire(new UnitRecruitmentQueueChangedSignal
            {
                OwnerId = item.OwnerId,
                BuildingPosition = item.RecruitingBuildingPosition,
                QueueId = item.QueueId,
                UnitTypeId = item.UnitTypeId,
                CompletedTurns = item.CompletedTurns,
                TrainingTurns = item.TrainingTurns,
                IsReady = item.IsReady,
            });
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
                _signalBus?.Fire(new UnitRecruitmentQueueChangedSignal
                {
                    OwnerId = signal.OwnerId,
                    BuildingPosition = signal.Position,
                    QueueId = 0,
                    UnitTypeId = string.Empty,
                    CompletedTurns = 0,
                    TrainingTurns = 0,
                    IsReady = false,
                });
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
