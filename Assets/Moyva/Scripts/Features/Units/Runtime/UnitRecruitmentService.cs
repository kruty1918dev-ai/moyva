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
    internal sealed class UnitRecruitmentService :
        IUnitRecruitmentService,
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

        [Inject]
        public UnitRecruitmentService(
            IUnitClassConfig unitClassConfig,
            [InjectOptional] IBuildingRegistry buildingRegistry = null,
            [InjectOptional] IConstructionSaveSnapshotSource constructionSnapshot = null,
            [InjectOptional] IConstructionLifecycle constructionLifecycle = null,
            [InjectOptional] IEconomyInfoMediator economy = null,
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] SignalBus signalBus = null)
        {
            _unitClassConfig = unitClassConfig;
            _buildingRegistry = buildingRegistry;
            _constructionSnapshot = constructionSnapshot;
            _constructionLifecycle = constructionLifecycle;
            _economy = economy;
            _turns = turns;
            _signalBus = signalBus;
        }

        public int TurnOrder => 30;

        public void Initialize()
            => _signalBus?.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);

        public void Dispose()
            => _signalBus?.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);

        public bool TryEnqueue(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            string unitTypeId,
            out string reason)
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
            if (_constructionSnapshot == null
                || _buildingRegistry == null
                || _constructionLifecycle == null)
            {
                reason = "Construction recruitment context is unavailable.";
                return false;
            }

            if (!TryFindRecruitingPlacement(
                    recruitingBuildingPosition,
                    out ConstructionSavedPlacement placement))
            {
                reason = "Recruiting building is not a committed construction placement.";
                return false;
            }
            if (!string.Equals(
                    NormalizeRequiredId(placement.OwnerId),
                    owner,
                    StringComparison.Ordinal))
            {
                reason = "Recruiting building belongs to another owner.";
                return false;
            }
            if (!_constructionLifecycle.IsOperational(recruitingBuildingPosition))
            {
                reason = "Recruiting building is still under construction.";
                return false;
            }

            BuildingDefinition definition =
                _buildingRegistry.GetById(placement.BuildingId);
            if (definition == null
                || !BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out UnitRecruitmentBuildingModule recruitmentModule))
            {
                reason = "Building has no enabled unit recruitment module.";
                return false;
            }

            if (!TryFindRecipe(
                    recruitmentModule,
                    unitType,
                    out UnitRecruitmentRecipeDefinition recipe))
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
            if (!_queue.CanEnqueue(
                    owner,
                    recruitingBuildingPosition,
                    capacity,
                    out reason))
            {
                return false;
            }

            Dictionary<string, float> costs = BuildCostMap(recipe.Costs);
            if (!TryConsumeRecruitmentCosts(
                    owner,
                    recruitingBuildingPosition,
                    costs,
                    out reason))
            {
                return false;
            }

            _queue.EnqueueValidated(
                owner,
                recruitingBuildingPosition,
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

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetQueue(
            string ownerId,
            Vector2Int recruitingBuildingPosition)
        {
            string owner = NormalizeRequiredId(ownerId);
            return owner == null
                ? Array.Empty<UnitRecruitmentQueueItemSnapshot>()
                : _queue.GetQueue(owner, recruitingBuildingPosition);
        }

        public bool TryPeekReady(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            out UnitRecruitmentQueueItemSnapshot item)
        {
            string owner = NormalizeRequiredId(ownerId);
            if (owner != null)
                return _queue.TryPeekReady(owner, recruitingBuildingPosition, out item);
            item = default;
            return false;
        }

        public void OnTurnStarted(TurnContext context)
        {
            string owner = NormalizeRequiredId(context.Faction.OwnerId);
            if (owner == null)
                return;
            _queue.AdvanceOwnerTurn(owner, context.GlobalTurn);
        }

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
        {
            if (_queue.RemoveBuildingQueues(signal.Position))
            {
                Debug.Log(
                    $"[UnitRecruitment] Dropped paid recruitment queue at demolished building {signal.Position}.");
            }
        }

        private bool TryFindRecruitingPlacement(
            Vector2Int position,
            out ConstructionSavedPlacement placement)
        {
            IReadOnlyList<ConstructionSavedPlacement> placements =
                _constructionSnapshot.GetSavedPlacements();
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
                        || !string.Equals(
                            NormalizeRequiredId(candidate.UnitTypeId),
                            unitTypeId,
                            StringComparison.Ordinal))
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

        internal static Dictionary<string, float> BuildCostMap(
            IReadOnlyList<BuildingResourceAmount> source)
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
            {
                return _economy.TryConsumeOwnerPoolResources(
                    ownerId,
                    costs,
                    out reason);
            }

            if (!_economy.TryResolveConstructionSettlement(
                    buildingPosition,
                    ownerId,
                    out EconomySettlementContext settlement)
                || string.IsNullOrWhiteSpace(settlement.SettlementId)
                || !string.Equals(
                    NormalizeRequiredId(settlement.OwnerId),
                    ownerId,
                    StringComparison.Ordinal))
            {
                reason = "No owned settlement is available to fund recruitment at this building.";
                return false;
            }

            return _economy.TryConsumeSettlementResources(
                settlement.SettlementId,
                costs,
                out reason);
        }

        private static string NormalizeRequiredId(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
