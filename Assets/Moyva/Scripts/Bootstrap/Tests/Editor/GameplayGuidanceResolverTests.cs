using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>P061: the shared guidance model must turn a canonical rejection
    /// (placement projection / recruitment shortages) into honest blockers and
    /// achievable options — real producers when they exist, an achievable
    /// prerequisite step when the direct producer is blocked, and an explicit
    /// "unobtainable" entry instead of a bogus suggestion.</summary>
    [TestFixture]
    internal sealed class GameplayGuidanceResolverTests
    {
        private const string Owner = "p1";
        private const string Settlement = "s1";
        private static readonly Vector2Int Tile = new Vector2Int(3, 4);

        private static BuildingDefinition Producer(string id, string producedResourceId)
        {
            var definition = new BuildingDefinition { Id = id, DisplayName = id };
            definition.Modules.Add(new ProductionBuildingModule
            { ResourceId = producedResourceId });
            return definition;
        }

        private static BuildingDefinition Housing(string id)
        {
            var definition = new BuildingDefinition { Id = id, DisplayName = id };
            definition.Modules.Add(new HousingBuildingModule());
            return definition;
        }

        private static GameplayGuidanceResolver Resolver(
            FakeRegistry registry, FakeAvailability availability,
            FakeConstructionCommands construction, FakeEconomy economy,
            FakePortfolio portfolio = null, FakeLifecycle lifecycle = null,
            FakeEconomyRuntime runtime = null, IUnitRecruitmentService recruitment = null)
            => new GameplayGuidanceResolver(
                registry, construction, availability,
                portfolio ?? new FakePortfolio(), lifecycle ?? new FakeLifecycle(),
                economy, runtime, null, null, recruitment);

        private static Dictionary<Vector2Int, string> Pending(string buildingId)
            => new Dictionary<Vector2Int, string> { [Tile] = buildingId };

        private static ConstructionResourceProjection DeficitProjection(
            string resourceId, float required, float available)
            => new ConstructionResourceProjection(
                Owner, Settlement, "Town", true, true, "short",
                new List<ConstructionResourceBalance>
                {
                    new ConstructionResourceBalance(resourceId, available, required),
                });

        [Test]
        public void Placement_ResourceDeficit_ListsBlocker_AndSuggestsBuildableProducer()
        {
            var registry = new FakeRegistry(Producer("prod-a", "res-a"));
            var construction = new FakeConstructionCommands();
            construction.Costs["prod-a"] = new Dictionary<string, float>();
            construction.Projections[Tile] = DeficitProjection("res-a", 10f, 4f);
            construction.Statuses[Tile] = new ConstructionPendingPlacementStatus(
                Tile, "forge", Settlement, "Town", true, false, null);
            var economy = new FakeEconomy();

            GuidanceModel model = Resolver(
                registry, new FakeAvailability(), construction, economy)
                .BuildPlacement(Owner, Pending("forge"));

            Assert.AreEqual(GuidanceGoalKind.Placement, model.Goal.Kind);
            Assert.AreEqual("forge", model.Goal.BuildingId);
            Assert.AreEqual(Tile, model.Goal.Position);
            Assert.AreEqual(1, model.Blockers.Count);
            var blocker = model.Blockers[0];
            Assert.AreEqual(GuidanceBlockerKind.Resource, blocker.Kind);
            Assert.AreEqual("res-a", blocker.ResourceId);
            Assert.AreEqual(10f, blocker.Required);
            Assert.AreEqual(4f, blocker.Available);
            Assert.IsFalse(blocker.Resolved);
            Assert.IsTrue(model.Blockers[0].Options.Exists(
                o => o.Kind == GuidanceOptionKind.BuildProducer && o.BuildingId == "prod-a"));
        }

        [Test]
        public void Placement_MultipleDeficits_PreservesEveryBlocker()
        {
            var construction = new FakeConstructionCommands();
            var balances = new List<ConstructionResourceBalance>
            {
                new ConstructionResourceBalance("res-a", 0f, 10f),
                new ConstructionResourceBalance("res-b", 1f, 5f),
            };
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, Settlement, "Town", true, true, "short", balances);
            var economy = new FakeEconomy();

            GuidanceModel model = Resolver(
                new FakeRegistry(), new FakeAvailability(), construction, economy)
                .BuildPlacement(Owner, Pending("forge"));

            Assert.AreEqual(2, model.Blockers.Count);
            Assert.AreEqual(2, model.PendingCount);
        }

        [Test]
        public void Placement_ProducingProducer_OffersFocus_NotRebuild()
        {
            var registry = new FakeRegistry(Producer("prod-a", "res-a"));
            var portfolio = new FakePortfolio(
                new ConstructionSavedPlacement(new Vector2Int(9, 9), "prod-a", Owner));
            var lifecycle = new FakeLifecycle(); // operational by default
            var runtime = new FakeEconomyRuntime();
            runtime.Active["prod-a"] = 1;
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = DeficitProjection("res-a", 10f, 0f);

            GuidanceModel model = Resolver(
                registry, new FakeAvailability(), construction, economy: new FakeEconomy(),
                portfolio, lifecycle, runtime)
                .BuildPlacement(Owner, Pending("forge"));

            var options = model.Blockers[0].Options;
            Assert.IsTrue(options.Exists(
                o => o.Kind == GuidanceOptionKind.FocusProducer
                    && o.Position == new Vector2Int(9, 9)));
            Assert.IsFalse(options.Exists(
                o => o.Kind == GuidanceOptionKind.ProducerConstructing));
        }

        [Test]
        public void Placement_ProducerUnderConstruction_ReportsConstructing()
        {
            var registry = new FakeRegistry(Producer("prod-a", "res-a"));
            var portfolio = new FakePortfolio(
                new ConstructionSavedPlacement(new Vector2Int(9, 9), "prod-a", Owner));
            var lifecycle = new FakeLifecycle();
            lifecycle.NonOperational.Add(new Vector2Int(9, 9));
            lifecycle.Progress[new Vector2Int(9, 9)] = (2, 5);
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = DeficitProjection("res-a", 10f, 0f);

            GuidanceModel model = Resolver(
                registry, new FakeAvailability(), construction, new FakeEconomy(),
                portfolio, lifecycle)
                .BuildPlacement(Owner, Pending("forge"));

            var options = model.Blockers[0].Options;
            Assert.IsTrue(options.Exists(
                o => o.Kind == GuidanceOptionKind.ProducerConstructing
                    && o.Detail.Contains("2/5")));
        }

        [Test]
        public void Placement_IdleProducer_OffersFocusWithIdleHint()
        {
            var registry = new FakeRegistry(Producer("prod-a", "res-a"));
            var portfolio = new FakePortfolio(
                new ConstructionSavedPlacement(new Vector2Int(9, 9), "prod-a", Owner));
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = DeficitProjection("res-a", 10f, 0f);

            GuidanceModel model = Resolver(
                registry, new FakeAvailability(), construction, new FakeEconomy(),
                portfolio, new FakeLifecycle())
                .BuildPlacement(Owner, Pending("forge"));

            var options = model.Blockers[0].Options;
            Assert.IsTrue(options.Exists(
                o => o.Kind == GuidanceOptionKind.FocusProducer
                    && o.Detail.Contains("workers")));
        }

        [Test]
        public void Placement_NoRegisteredProducer_MarksUnobtainable()
        {
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = DeficitProjection("res-x", 10f, 0f);

            GuidanceModel model = Resolver(
                new FakeRegistry(), new FakeAvailability(), construction, new FakeEconomy())
                .BuildPlacement(Owner, Pending("forge"));

            var options = model.Blockers[0].Options;
            Assert.IsTrue(options.Exists(o => o.Kind == GuidanceOptionKind.Unobtainable));
            Assert.IsFalse(options.Exists(o => o.Kind == GuidanceOptionKind.BuildProducer));
        }

        [Test, Timeout(10000)]
        public void Placement_CyclicProducerPrerequisites_ReportsUnobtainable()
        {
            // prod-a needs res-b, prod-b needs res-a — with no stock the cycle
            // can never start; guidance must not suggest either.
            var registry = new FakeRegistry(
                Producer("prod-a", "res-a"), Producer("prod-b", "res-b"));
            var construction = new FakeConstructionCommands();
            construction.Costs["prod-a"] = new Dictionary<string, float> { ["res-b"] = 10f };
            construction.Costs["prod-b"] = new Dictionary<string, float> { ["res-a"] = 10f };
            construction.Projections[Tile] = DeficitProjection("res-a", 10f, 0f);

            GuidanceModel model = Resolver(
                registry, new FakeAvailability(), construction, new FakeEconomy())
                .BuildPlacement(Owner, Pending("forge"));

            var options = model.Blockers[0].Options;
            Assert.IsTrue(options.Exists(o => o.Kind == GuidanceOptionKind.Unobtainable));
            Assert.IsFalse(options.Exists(o => o.Kind == GuidanceOptionKind.BuildProducer));
        }

        [Test]
        public void Placement_SpatialError_AddsPlacementBlocker()
        {
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, Settlement, "Town", true, false, string.Empty,
                new List<ConstructionResourceBalance>());
            construction.Statuses[Tile] = new ConstructionPendingPlacementStatus(
                Tile, "forge", Settlement, "Town", true, true, "Tile occupied");

            GuidanceModel model = Resolver(
                new FakeRegistry(), new FakeAvailability(), construction, new FakeEconomy())
                .BuildPlacement(Owner, Pending("forge"));

            Assert.AreEqual(1, model.Blockers.Count);
            Assert.AreEqual(GuidanceBlockerKind.Placement, model.Blockers[0].Kind);
            Assert.AreEqual("Tile occupied", model.Blockers[0].Detail);
        }

        [Test]
        public void Refresh_ResourceNowCovered_MarksBlockerResolved()
        {
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = DeficitProjection("res-a", 10f, 0f);
            var economy = new FakeEconomy();

            var resolver = Resolver(
                new FakeRegistry(), new FakeAvailability(), construction, economy);
            GuidanceModel model = resolver.BuildPlacement(Owner, Pending("forge"));
            Assert.AreEqual(1, model.PendingCount);

            // Settlement stock grew while the popup was open.
            economy.SettlementStock["res-a"] = 12f;
            resolver.Refresh(model, Owner);

            Assert.IsTrue(model.Blockers[0].Resolved);
            Assert.AreEqual(0, model.PendingCount);
            Assert.IsTrue(model.AllResolved);
        }

        [Test]
        public void Recruitment_ResourceShortages_PreserveEveryShortage()
        {
            var recruitment = new FakeRecruitment();
            recruitment.Shortages = new List<UnitRecruitmentShortage>
            {
                new UnitRecruitmentShortage("res-a", false, 10f, 2f, 10f),
                new UnitRecruitmentShortage("res-b", false, 5f, 0f, 5f),
            };
            recruitment.Reason = "not enough";

            GuidanceModel model = Resolver(
                new FakeRegistry(), new FakeAvailability(),
                new FakeConstructionCommands(), new FakeEconomy(),
                recruitment: recruitment)
                .BuildRecruitment(Owner, Tile, "unit-x");

            Assert.AreEqual(GuidanceGoalKind.Recruitment, model.Goal.Kind);
            Assert.AreEqual("unit-x", model.Goal.UnitTypeId);
            Assert.AreEqual(2, model.Blockers.Count);
            Assert.AreEqual("res-a", model.Blockers[0].ResourceId);
            Assert.AreEqual("res-b", model.Blockers[1].ResourceId);
        }

        [Test]
        public void Recruitment_ReservedShortage_OffersQueueOption()
        {
            var recruitment = new FakeRecruitment();
            recruitment.Shortages = new List<UnitRecruitmentShortage>
            {
                new UnitRecruitmentShortage("res-a", false, 10f, 2f, reserved: 8f),
            };
            recruitment.Reason = "not enough";

            GuidanceModel model = Resolver(
                new FakeRegistry(), new FakeAvailability(),
                new FakeConstructionCommands(), new FakeEconomy(),
                recruitment: recruitment)
                .BuildRecruitment(Owner, Tile, "unit-x");

            Assert.IsTrue(model.Blockers[0].Options.Exists(
                o => o.Kind == GuidanceOptionKind.OpenQueue));
        }

        [Test]
        public void Recruitment_HousingShortage_SuggestsHousingBuilding()
        {
            var recruitment = new FakeRecruitment();
            recruitment.Shortages = new List<UnitRecruitmentShortage>
            {
                new UnitRecruitmentShortage(null, true, 3f, 0f, 0f,
                    PopulationGrowthBlocker.Housing),
            };
            recruitment.Reason = "no beds";
            var registry = new FakeRegistry(Housing("house"));

            GuidanceModel model = Resolver(
                registry, new FakeAvailability(),
                new FakeConstructionCommands(), new FakeEconomy(),
                recruitment: recruitment)
                .BuildRecruitment(Owner, Tile, "unit-x");

            Assert.AreEqual(GuidanceBlockerKind.Population, model.Blockers[0].Kind);
            Assert.IsTrue(model.Blockers[0].Options.Exists(
                o => o.Kind == GuidanceOptionKind.BuildHousing
                    && o.BuildingId == "house"));
        }

        [Test]
        public void Recruitment_NonShortageReason_BecomesGenericBlocker()
        {
            var recruitment = new FakeRecruitment
            {
                Shortages = new List<UnitRecruitmentShortage>(),
                Reason = "building queue is full",
            };

            GuidanceModel model = Resolver(
                new FakeRegistry(), new FakeAvailability(),
                new FakeConstructionCommands(), new FakeEconomy(),
                recruitment: recruitment)
                .BuildRecruitment(Owner, Tile, "unit-x");

            Assert.AreEqual(1, model.Blockers.Count);
            Assert.AreEqual(GuidanceBlockerKind.Generic, model.Blockers[0].Kind);
            Assert.AreEqual("building queue is full", model.Blockers[0].Detail);
        }

        [Test]
        public void Recruitment_NoQuerySupport_ReturnsEmptyModel()
        {
            GuidanceModel model = Resolver(
                new FakeRegistry(), new FakeAvailability(),
                new FakeConstructionCommands(), new FakeEconomy(),
                recruitment: new FakeRecruitmentNoQuery())
                .BuildRecruitment(Owner, Tile, "unit-x");

            Assert.AreEqual(0, model.Blockers.Count);
            Assert.IsFalse(model.AllResolved); // nothing to resume into
        }

        // ---------- fakes ----------

        private sealed class FakeRegistry : IBuildingRegistry
        {
            private readonly BuildingDefinition[] _definitions;
            public FakeRegistry(params BuildingDefinition[] definitions)
                => _definitions = definitions ?? Array.Empty<BuildingDefinition>();
            public BuildingDefinition[] GetAll() => _definitions;
            public BuildingDefinition GetById(string id)
            {
                foreach (var definition in _definitions)
                    if (string.Equals(definition?.Id, id, StringComparison.Ordinal))
                        return definition;
                return null;
            }
            public BuildingDefinition[] GetByCategory(BuildingCategory category)
                => Array.Empty<BuildingDefinition>();
            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();
            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId) => null;
        }

        private sealed class FakeAvailability : IConstructionSelectionAvailabilityQuery
        {
            public ConstructionSelectionAvailabilityResult EvaluateSelectionAvailability(
                string buildingId, string ownerId = null,
                Vector2Int? preferredFundingPosition = null,
                bool includePendingPlacements = true)
                => new ConstructionSelectionAvailabilityResult(true, true, true);
        }

        private sealed class FakeConstructionCommands : IConstructionSessionCommands
        {
            public readonly Dictionary<string, Dictionary<string, float>> Costs =
                new(StringComparer.Ordinal);
            public readonly Dictionary<Vector2Int, ConstructionResourceProjection> Projections =
                new();
            public readonly Dictionary<Vector2Int, ConstructionPendingPlacementStatus> Statuses =
                new();

            public BuildingPlacementState State => default;
            public bool IsDemolishMode => false;
            public int PendingDemolitionCount => 0;
            public void SelectBuilding(string buildingId) { }
            public string GetSelectedBuildingId() => null;
            public void SetActiveOwner(string ownerId) { }
            public string GetActiveOwner() => null;
            public bool TryPreviewAt(Vector2Int position) => false;
            public bool HasPendingPlacementAt(Vector2Int position) => false;
            public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
            { buildingId = null; return false; }
            public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements()
                => new Dictionary<Vector2Int, string>();
            public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition) => false;
            public bool RemovePendingAt(Vector2Int position) => false;
            public bool TryGetPendingPlacementStatus(Vector2Int position,
                out ConstructionPendingPlacementStatus status)
                => Statuses.TryGetValue(position, out status);
            public ConstructionResourceProjection GetResourceProjection(Vector2Int position)
                => Projections.TryGetValue(position, out var projection)
                    ? projection
                    : ConstructionResourceProjection.Empty;
            public IReadOnlyDictionary<string, float> GetBuildingResourceCosts(string buildingId)
                => Costs.TryGetValue(buildingId ?? string.Empty, out var costs)
                    ? costs
                    : new Dictionary<string, float>();
            public void Confirm() { }
            public void Cancel() { }
            public void UndoLast() { }
            public void RedoLast() { }
            public void ToggleDemolishMode() { }
            public bool TryDemolishAt(Vector2Int position) => false;
            public string GetLastActionMessage() => null;
        }

        private sealed class FakeEconomy : IEconomyInfoMediator
        {
            public readonly Dictionary<string, float> SettlementStock =
                new(StringComparer.Ordinal);
            public readonly Dictionary<string, float> OwnerStock =
                new(StringComparer.Ordinal);

            public RecruitmentPopulationSnapshot GetRecruitmentPopulation(
                string ownerId, Vector2Int position) => default;
            public bool TryReserveRecruitmentPopulation(string ownerId, Vector2Int position,
                long queueId, int count, out string reason)
            { reason = null; return false; }
            public void ReleaseRecruitmentPopulation(string ownerId, long queueId) { }
            public void DeployRecruitmentPopulation(string ownerId, long queueId, string unitId) { }
            public bool TryGetSettlementContext(Vector2Int position,
                out EconomySettlementContext context)
            { context = default; return false; }
            public bool TryResolveConstructionSettlement(Vector2Int position, string ownerId,
                out EconomySettlementContext context)
            { context = default; return false; }
            public bool TryGetBuildingContext(Vector2Int position,
                out string buildingId, out string ownerId)
            { buildingId = null; ownerId = null; return false; }
            public bool TryConsumeSettlementResources(string settlementId,
                IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            { errorMessage = null; return false; }
            public bool TryConsumeOwnerPoolResources(string ownerId,
                IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            { errorMessage = null; return false; }
            public void RefundOwnerPoolResources(string ownerId,
                IReadOnlyDictionary<string, float> resources) { }
            public void RefundRecruitmentResources(string ownerId, string settlementId,
                IReadOnlyDictionary<string, float> resources) { }
            public bool OwnerHasAnyWarehouse(string ownerId) => false;
            public IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(
                Vector2Int warehousePosition) => new Dictionary<string, float>();
            public IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(
                string settlementId) => SettlementStock;
            public IReadOnlyDictionary<string, float> GetSettlementResourceTotals(
                string settlementId) => SettlementStock;
            public IReadOnlyDictionary<string, float> GetSettlementReservedResourceTotals(
                string settlementId) => new Dictionary<string, float>();
            public IReadOnlyDictionary<string, float> GetSettlementAvailableResourceTotals(
                string settlementId) => SettlementStock;
            public void ReleaseConstructionSupplyReservations(Vector2Int placementPosition) { }
            public IReadOnlyDictionary<string, float> GetSettlementResourcesForPlacement(
                string settlementId, Vector2Int placementPosition) => SettlementStock;
            public IReadOnlyDictionary<string, float> GetOwnerPoolResourceTotals(
                string ownerId) => OwnerStock;
            public IReadOnlyDictionary<string, float> GetOwnerResourceTotals(
                string ownerId) => OwnerStock;
            public string GetResourceDisplayName(string resourceId) => resourceId;
        }

        private sealed class FakePortfolio : IConstructionPortfolioQuery
        {
            private readonly IReadOnlyList<ConstructionSavedPlacement> _placements;
            public FakePortfolio(params ConstructionSavedPlacement[] placements)
                => _placements = placements ?? Array.Empty<ConstructionSavedPlacement>();
            public IReadOnlyList<ConstructionSavedPlacement> GetOwnerPlacements(string ownerId)
                => _placements;
        }

        private sealed class FakeLifecycle : IConstructionLifecycle
        {
            public readonly HashSet<Vector2Int> NonOperational = new();
            public readonly Dictionary<Vector2Int, (int done, int total)> Progress = new();
            public bool IsOperational(Vector2Int position) => !NonOperational.Contains(position);
            public bool TryGetProgress(Vector2Int position,
                out int completedTurns, out int requiredTurns)
            {
                if (Progress.TryGetValue(position, out var progress))
                {
                    completedTurns = progress.done;
                    requiredTurns = progress.total;
                    return true;
                }
                completedTurns = 0;
                requiredTurns = 0;
                return false;
            }
        }

        private sealed class FakeEconomyRuntime : IEconomyRuntimeApi
        {
            public readonly Dictionary<string, int> Active = new(StringComparer.Ordinal);
            public IReadOnlyList<string> GetSettlementIdsForOwner(string ownerId)
                => new List<string>();
            public EconomyCategoryTotals GetOwnerCategoryTotals(string ownerId) => default;
            public EconomyFormattedCategoryTotals GetFormattedOwnerCategoryTotals(
                string ownerId) => default;
            public Dictionary<string, float> GetOwnerResourceTotals(string ownerId)
                => new Dictionary<string, float>();
            public EconomyCategoryTotals GetSettlementCategoryTotals(string settlementId) => default;
            public EconomyFormattedCategoryTotals GetFormattedSettlementCategoryTotals(
                string settlementId) => default;
            public Dictionary<string, float> GetSettlementResourceTotals(string settlementId)
                => new Dictionary<string, float>();
            public IReadOnlyList<EconomyWarehouseSnapshot> GetOwnerWarehouseSnapshots(
                string ownerId) => new List<EconomyWarehouseSnapshot>();
            public IReadOnlyList<EconomySettlementSnapshot> GetOwnerSettlementSnapshots(
                string ownerId) => new List<EconomySettlementSnapshot>();
            public EconomyProductionReadSnapshot GetOwnerProductionSnapshot(string ownerId)
                => new EconomyProductionReadSnapshot(
                    new Dictionary<string, float>(),
                    new Dictionary<string, int>(Active));
        }

        private class FakeRecruitment : IUnitRecruitmentService, IUnitRecruitmentQuery
        {
            public IReadOnlyList<UnitRecruitmentShortage> Shortages;
            public string Reason;

            public bool TryEnqueue(string ownerId, Vector2Int recruitingBuildingPosition,
                string unitTypeId, out string reason)
            { reason = Reason; return false; }
            public bool TryCancel(string ownerId, Vector2Int recruitingBuildingPosition,
                long queueId, out string reason)
            { reason = null; return false; }
            public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetQueue(
                string ownerId, Vector2Int recruitingBuildingPosition)
                => new List<UnitRecruitmentQueueItemSnapshot>();
            public bool TryPeekReady(string ownerId, Vector2Int recruitingBuildingPosition,
                out UnitRecruitmentQueueItemSnapshot item)
            { item = default; return false; }
            public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetReadyItems(string ownerId)
                => new List<UnitRecruitmentQueueItemSnapshot>();
            public IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> GetDeploymentTiles(
                string ownerId, Vector2Int recruitingBuildingPosition, long queueId)
                => new List<UnitRecruitmentDeploymentTileSnapshot>();
            public bool TryDeployReady(string ownerId, Vector2Int recruitingBuildingPosition,
                long queueId, Vector2Int targetPosition, out string unitId, out string reason)
            { unitId = null; reason = null; return false; }
            public IReadOnlyList<UnitRecruitmentOption> GetOptions(string ownerId)
                => new List<UnitRecruitmentOption>();
            public bool CanEnqueue(string ownerId, Vector2Int source, string unitTypeId,
                out string reason)
            { reason = Reason; return false; }
            public bool TryGetEnqueueShortages(string ownerId, Vector2Int source,
                string unitTypeId, out IReadOnlyList<UnitRecruitmentShortage> shortages,
                out string reason)
            { shortages = Shortages; reason = Reason; return false; }
        }

        private sealed class FakeRecruitmentNoQuery : IUnitRecruitmentService
        {
            public bool TryEnqueue(string ownerId, Vector2Int recruitingBuildingPosition,
                string unitTypeId, out string reason)
            { reason = null; return false; }
            public bool TryCancel(string ownerId, Vector2Int recruitingBuildingPosition,
                long queueId, out string reason)
            { reason = null; return false; }
            public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetQueue(
                string ownerId, Vector2Int recruitingBuildingPosition)
                => new List<UnitRecruitmentQueueItemSnapshot>();
            public bool TryPeekReady(string ownerId, Vector2Int recruitingBuildingPosition,
                out UnitRecruitmentQueueItemSnapshot item)
            { item = default; return false; }
            public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetReadyItems(string ownerId)
                => new List<UnitRecruitmentQueueItemSnapshot>();
            public IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> GetDeploymentTiles(
                string ownerId, Vector2Int recruitingBuildingPosition, long queueId)
                => new List<UnitRecruitmentDeploymentTileSnapshot>();
            public bool TryDeployReady(string ownerId, Vector2Int recruitingBuildingPosition,
                long queueId, Vector2Int targetPosition, out string unitId, out string reason)
            { unitId = null; reason = null; return false; }
        }
    }
}
