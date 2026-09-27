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
    /// <summary>U06 "next-action hints": the guidance model must produce
    /// distinct, actionable next steps for the real current state — fresh
    /// start funding, a pending castle, zero food, a forest-gated producer,
    /// supply already in transit, a ready recruit, turn ownership and an
    /// inactive settlement — and must never recommend a building the player
    /// cannot afford or a countdown it cannot prove.</summary>
    [TestFixture]
    internal sealed class U06AcceptanceTests
    {
        private const string Owner = "p1";
        private const string OtherOwner = "p2";
        private const string Settlement = "s-live";
        private const string DeadSettlement = "s-dead";
        private static readonly Vector2Int Tile = new Vector2Int(3, 4);
        private static readonly Vector2Int FarTile = new Vector2Int(9, 9);

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

        private static BuildingDefinition Recruiter(string id, string unitTypeId)
        {
            var definition = new BuildingDefinition { Id = id, DisplayName = id };
            definition.Modules.Add(new UnitRecruitmentBuildingModule
            {
                Recipes =
                {
                    new UnitRecruitmentRecipeDefinition
                    {
                        UnitTypeId = unitTypeId,
                        PopulationCost = 2,
                        TrainingTurns = 3,
                    },
                },
            });
            return definition;
        }

        private static GameplayGuidanceResolver Resolver(
            FakeRegistry registry = null,
            FakeAvailability availability = null,
            FakeConstructionCommands construction = null,
            FakeEconomy economy = null,
            FakePortfolio portfolio = null,
            FakeLifecycle lifecycle = null,
            FakeEconomyRuntime runtime = null,
            FakeSupply supply = null,
            IUnitRecruitmentService recruitment = null)
            => new GameplayGuidanceResolver(
                registry ?? new FakeRegistry(),
                construction ?? new FakeConstructionCommands(),
                availability ?? new FakeAvailability(),
                portfolio ?? new FakePortfolio(),
                lifecycle ?? new FakeLifecycle(),
                economy ?? new FakeEconomy(),
                runtime,
                null,
                supply,
                recruitment);

        private static Dictionary<Vector2Int, string> Pending(string buildingId)
            => new Dictionary<Vector2Int, string> { [Tile] = buildingId };

        // ---------- fresh start / pending castle ----------

        [Test]
        public void FreshStart_OwnerPoolCastle_NoFakeSettlementBlocker()
        {
            // Start state: no settlement exists yet and the castle is funded
            // from the owner pool — affordable, so "no settlement" is not a
            // blocker at all.
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, null, null, false, false, string.Empty,
                new List<ConstructionResourceBalance>());

            GuidanceModel model = Resolver(construction: construction)
                .BuildPlacement(Owner, Pending("castle"));

            Assert.IsFalse(model.Blockers.Exists(
                b => b.Kind == GuidanceBlockerKind.Eligibility));
            Assert.IsFalse(model.Blockers.Exists(
                b => b.Kind == GuidanceBlockerKind.InactiveSettlement));
        }

        [Test]
        public void PendingCastle_SpatialError_OffersMovePending()
        {
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, null, null, false, false, string.Empty,
                new List<ConstructionResourceBalance>());
            construction.Statuses[Tile] = new ConstructionPendingPlacementStatus(
                Tile, "castle", string.Empty, string.Empty, false, true,
                "Must be placed on explored territory.");

            GuidanceModel model = Resolver(construction: construction)
                .BuildPlacement(Owner, Pending("castle"));

            var blocker = model.Blockers.Find(
                b => b.Kind == GuidanceBlockerKind.Placement);
            Assert.IsNotNull(blocker);
            Assert.AreEqual("Must be placed on explored territory.", blocker.Detail);
            Assert.IsTrue(blocker.Options.Exists(
                o => o.Kind == GuidanceOptionKind.MovePending
                    && o.Position == Tile));
        }

        // ---------- inactive settlement ----------

        [Test]
        public void InactiveSettlement_RegisteredButUnfundable_IsExplained()
        {
            var economy = new FakeEconomy();
            economy.Contexts[Tile] =
                new EconomySettlementContext(DeadSettlement, "Dead Town", Owner);
            // Canonical funding resolver finds no active owned settlement.
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, null, null, false, true,
                "Не знайдено поселення/замок для ресурсів у цій зоні будівництва.",
                new List<ConstructionResourceBalance>());

            GuidanceModel model = Resolver(
                construction: construction, economy: economy)
                .BuildPlacement(Owner, Pending("forge"));

            Assert.IsTrue(model.Blockers.Exists(
                b => b.Kind == GuidanceBlockerKind.InactiveSettlement));
            Assert.IsFalse(model.Blockers.Exists(
                b => b.Kind == GuidanceBlockerKind.Eligibility));
        }

        [Test]
        public void InactiveSettlement_DeadStockNeverCountsAsCovered()
        {
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, DeadSettlement, "Dead Town", true, true, "short",
                new List<ConstructionResourceBalance>
                {
                    new ConstructionResourceBalance("res-a", 0f, 10f),
                });
            var economy = new FakeEconomy();
            // The dead settlement still reports its frozen stock...
            economy.SettlementStock["res-a"] = 100f;
            var runtime = new FakeEconomyRuntime();
            // ...but only s-live is an active settlement of this owner.
            runtime.Settlements.Add(new EconomySettlementSnapshot(
                Settlement, "Town", 5, 3, new Dictionary<string, float>()));

            var resolver = Resolver(
                construction: construction, economy: economy, runtime: runtime);
            GuidanceModel model = resolver.BuildPlacement(Owner, Pending("forge"));

            var blocker = model.Blockers.Find(
                b => b.Kind == GuidanceBlockerKind.Resource);
            Assert.IsNotNull(blocker);
            Assert.IsFalse(blocker.Resolved);

            // Once the settlement is active again the same stock counts.
            runtime.Settlements.Add(new EconomySettlementSnapshot(
                DeadSettlement, "Dead Town", 0, 3, new Dictionary<string, float>()));
            resolver.Refresh(model, Owner);
            Assert.IsTrue(blocker.Resolved);
        }

        [Test]
        public void Recruitment_InactiveSettlement_BlockerResolvesWhenFundable()
        {
            var economy = new FakeEconomy();
            economy.Contexts[Tile] =
                new EconomySettlementContext(DeadSettlement, "Dead Town", Owner);
            var recruitment = new FakeRecruitment
            {
                Shortages = new List<UnitRecruitmentShortage>
                {
                    new UnitRecruitmentShortage("res-a", false, 10f, 0f, 0f),
                },
                Reason = "not enough",
            };

            var resolver = Resolver(economy: economy, recruitment: recruitment);
            GuidanceModel model = resolver.BuildRecruitment(Owner, Tile, "unit-x");

            var inactive = model.Blockers.Find(
                b => b.Kind == GuidanceBlockerKind.InactiveSettlement);
            Assert.IsNotNull(inactive);
            Assert.IsFalse(inactive.Resolved);

            economy.Resolved[Tile] =
                new EconomySettlementContext(DeadSettlement, "Dead Town", Owner);
            resolver.Refresh(model, Owner);
            Assert.IsTrue(inactive.Resolved);
        }

        // ---------- food reserve / population ----------

        [Test]
        public void ZeroFood_PopulationShortage_ShowsReserveWithoutCountdown()
        {
            var economy = new FakeEconomy();
            economy.Population = new RecruitmentPopulationSnapshot(
                total: 4, available: 0, training: 2, military: 2,
                constructionSpeed: 1f, housingCapacity: 4, foodAvailable: 0f,
                growthBlocker: PopulationGrowthBlocker.Food);
            var recruitment = new FakeRecruitment
            {
                Shortages = new List<UnitRecruitmentShortage>
                {
                    new UnitRecruitmentShortage(null, true, 2f, 0f, 0f,
                        PopulationGrowthBlocker.Food),
                },
                Reason = "no free residents",
            };

            GuidanceModel model = Resolver(economy: economy, recruitment: recruitment)
                .BuildRecruitment(Owner, Tile, "unit-x");

            var blocker = model.Blockers.Find(
                b => b.Kind == GuidanceBlockerKind.Population);
            Assert.IsNotNull(blocker);
            StringAssert.Contains("Food reserve: 0", blocker.Detail);
            // No starvation countdown — no reliable forecast exists.
            StringAssert.DoesNotContain("turn", blocker.Detail.ToLowerInvariant());
        }

        [Test]
        public void PopShortage_UnselectableHousing_IsNeverSuggested()
        {
            var registry = new FakeRegistry(Housing("house"));
            var availability = new FakeAvailability { Selectable = false };
            var recruitment = new FakeRecruitment
            {
                Shortages = new List<UnitRecruitmentShortage>
                {
                    new UnitRecruitmentShortage(null, true, 3f, 0f, 0f,
                        PopulationGrowthBlocker.Housing),
                },
                Reason = "no beds",
            };

            GuidanceModel model = Resolver(
                registry: registry, availability: availability,
                recruitment: recruitment)
                .BuildRecruitment(Owner, Tile, "unit-x");

            var blocker = model.Blockers.Find(
                b => b.Kind == GuidanceBlockerKind.Population);
            Assert.IsNotNull(blocker);
            Assert.IsFalse(blocker.Options.Exists(
                o => o.Kind == GuidanceOptionKind.BuildHousing));
        }

        // ---------- forest-gated producer ----------

        [Test]
        public void BlockedWoodCamp_SpatialGate_OffersMovePendingNotEligibility()
        {
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, Settlement, "Town", true, false, string.Empty,
                new List<ConstructionResourceBalance>());
            construction.Statuses[Tile] = new ConstructionPendingPlacementStatus(
                Tile, "wood-camp", Settlement, "Town", true, true,
                "Requires forest on this tile.");

            GuidanceModel model = Resolver(construction: construction)
                .BuildPlacement(Owner, Pending("wood-camp"));

            var blocker = model.Blockers.Find(
                b => b.Kind == GuidanceBlockerKind.Placement);
            Assert.IsNotNull(blocker);
            Assert.AreEqual("Requires forest on this tile.", blocker.Detail);
            Assert.IsTrue(blocker.Options.Exists(
                o => o.Kind == GuidanceOptionKind.MovePending
                    && o.Position == Tile));
        }

        [Test]
        public void BlockedWoodCamp_WoodDeficit_SuggestsAlternativeProducer()
        {
            var registry = new FakeRegistry(
                Producer("wood-camp", "wood"), Producer("forager", "wood"));
            var availability = new FakeAvailability();
            // The forest-gated wood camp cannot be selected anywhere —
            // the alternative forager can.
            availability.Blocked.Add("wood-camp");
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, Settlement, "Town", true, true, "short",
                new List<ConstructionResourceBalance>
                {
                    new ConstructionResourceBalance("wood", 0f, 10f),
                });
            construction.Statuses[Tile] = new ConstructionPendingPlacementStatus(
                Tile, "wood-camp", Settlement, "Town", true, false, null);

            GuidanceModel model = Resolver(
                registry: registry, availability: availability,
                construction: construction)
                .BuildPlacement(Owner, Pending("wood-camp"));

            var blocker = model.Blockers.Find(
                b => b.Kind == GuidanceBlockerKind.Resource
                    && b.ResourceId == "wood");
            Assert.IsNotNull(blocker);
            Assert.IsFalse(blocker.Options.Exists(
                o => o.Kind == GuidanceOptionKind.BuildProducer
                    && o.BuildingId == "wood-camp"));
            Assert.IsTrue(blocker.Options.Exists(
                o => o.Kind == GuidanceOptionKind.BuildProducer
                    && o.BuildingId == "forager"));
        }

        // ---------- supply in transit ----------

        [Test]
        public void SupplyInTransit_CoveredGap_ShowsAwaitDelivery_NoDuplicateWagon()
        {
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, Settlement, "Town", true, true, "short",
                new List<ConstructionResourceBalance>
                {
                    new ConstructionResourceBalance("wood", 2f, 12f),
                });
            construction.Statuses[Tile] = new ConstructionPendingPlacementStatus(
                Tile, "forge", Settlement, "Town", true, false, null);
            var supply = new FakeSupply();
            supply.Orders[Tile] = new ConstructionSupplyOrderSnapshot(
                "o1", Owner, "forge", Tile, Settlement, "Town",
                ConstructionSupplyOrderStatus.Active,
                new Dictionary<string, float> { ["wood"] = 12f },
                new Dictionary<string, float> { ["wood"] = 2f },
                new Dictionary<string, float> { ["wood"] = 10f },
                new List<string> { "w1" });

            GuidanceModel model = Resolver(
                construction: construction, supply: supply)
                .BuildPlacement(Owner, Pending("forge"));

            var blocker = model.Blockers.Find(
                b => b.Kind == GuidanceBlockerKind.Resource);
            Assert.IsNotNull(blocker);
            Assert.IsTrue(blocker.Options.Exists(
                o => o.Kind == GuidanceOptionKind.AwaitDelivery));
            Assert.IsFalse(blocker.Options.Exists(
                o => o.Kind == GuidanceOptionKind.OpenSupply));
        }

        [Test]
        public void SupplyInTransit_PartialGap_KeepsDispatchOption()
        {
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, Settlement, "Town", true, true, "short",
                new List<ConstructionResourceBalance>
                {
                    new ConstructionResourceBalance("wood", 2f, 12f),
                });
            construction.Statuses[Tile] = new ConstructionPendingPlacementStatus(
                Tile, "forge", Settlement, "Town", true, false, null);
            var supply = new FakeSupply();
            supply.Orders[Tile] = new ConstructionSupplyOrderSnapshot(
                "o1", Owner, "forge", Tile, Settlement, "Town",
                ConstructionSupplyOrderStatus.Active,
                new Dictionary<string, float> { ["wood"] = 12f },
                new Dictionary<string, float> { ["wood"] = 2f },
                new Dictionary<string, float> { ["wood"] = 4f },
                new List<string> { "w1" });

            GuidanceModel model = Resolver(
                construction: construction, supply: supply)
                .BuildPlacement(Owner, Pending("forge"));

            var blocker = model.Blockers.Find(
                b => b.Kind == GuidanceBlockerKind.Resource);
            Assert.IsNotNull(blocker);
            Assert.IsTrue(blocker.Options.Exists(
                o => o.Kind == GuidanceOptionKind.AwaitDelivery));
            Assert.IsTrue(blocker.Options.Exists(
                o => o.Kind == GuidanceOptionKind.OpenSupply));
        }

        // ---------- ready recruit ----------

        [Test]
        public void ReadyRecruit_UnitSourceOffersDeploy()
        {
            var registry = new FakeRegistry(Recruiter("barracks", "unit-x"));
            var portfolio = new FakePortfolio(
                new ConstructionSavedPlacement(FarTile, "barracks", Owner));
            var recruitment = new FakeRecruitment
            {
                Shortages = new List<UnitRecruitmentShortage>
                {
                    new UnitRecruitmentShortage("res-a", false, 10f, 0f, 0f),
                },
                Reason = "not enough",
                Ready = new UnitRecruitmentQueueItemSnapshot(
                    7L, Owner, Tile, "barracks", "unit-x",
                    completedTurns: 3, trainingTurns: 3,
                    enqueuedGlobalTurn: 1L, lastProgressGlobalTurn: 3L,
                    UnitRecruitmentQueueStatus.Training),
            };

            GuidanceModel model = Resolver(
                registry: registry, portfolio: portfolio,
                recruitment: recruitment)
                .BuildRecruitment(Owner, Tile, "unit-x");

            var source = model.Blockers.Find(
                b => b.Kind == GuidanceBlockerKind.UnitSource);
            Assert.IsNotNull(source);
            Assert.IsTrue(source.Options.Exists(
                o => o.Kind == GuidanceOptionKind.DeployReady
                    && o.Position == Tile));
        }

        // ---------- turn ownership ----------

        [Test]
        public void TurnOwner_NotYourTurn_LeadsWithTurnWait_AndResolves()
        {
            var construction = new FakeConstructionCommands();
            construction.ActiveOwner = OtherOwner;
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, Settlement, "Town", true, true, "short",
                new List<ConstructionResourceBalance>
                {
                    new ConstructionResourceBalance("res-a", 0f, 10f),
                });

            var resolver = Resolver(construction: construction);
            GuidanceModel model = resolver.BuildPlacement(Owner, Pending("forge"));

            Assert.AreEqual(GuidanceBlockerKind.TurnWait, model.Blockers[0].Kind);
            Assert.IsFalse(model.Blockers[0].Resolved);

            construction.ActiveOwner = Owner;
            resolver.Refresh(model, Owner);
            Assert.IsTrue(model.Blockers[0].Resolved);
        }

        // ---------- read purity ----------

        [Test]
        public void GuidanceRead_DoesNotMutatePendingOrChangeAnswers()
        {
            var pending = Pending("forge");
            var construction = new FakeConstructionCommands();
            construction.Projections[Tile] = new ConstructionResourceProjection(
                Owner, Settlement, "Town", true, true, "short",
                new List<ConstructionResourceBalance>
                {
                    new ConstructionResourceBalance("res-a", 0f, 10f),
                });

            var resolver = Resolver(construction: construction);
            GuidanceModel first = resolver.BuildPlacement(Owner, pending);
            GuidanceModel second = resolver.BuildPlacement(Owner, pending);

            Assert.AreEqual(1, pending.Count);
            Assert.AreEqual("forge", pending[Tile]);
            Assert.AreEqual(first.Blockers.Count, second.Blockers.Count);
            for (int i = 0; i < first.Blockers.Count; i++)
                Assert.AreEqual(first.Blockers[i].Kind, second.Blockers[i].Kind);
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
            public bool Selectable = true;
            public readonly HashSet<string> Blocked = new(StringComparer.Ordinal);
            public ConstructionSelectionAvailabilityResult EvaluateSelectionAvailability(
                string buildingId, string ownerId = null,
                Vector2Int? preferredFundingPosition = null,
                bool includePendingPlacements = true)
            {
                bool selectable = Selectable
                    && !Blocked.Contains(buildingId ?? string.Empty);
                return new ConstructionSelectionAvailabilityResult(
                    selectable, selectable, selectable);
            }
        }

        private sealed class FakeConstructionCommands : IConstructionSessionCommands
        {
            public string ActiveOwner;
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
            public string GetActiveOwner() => ActiveOwner;
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
            public readonly Dictionary<Vector2Int, EconomySettlementContext> Contexts =
                new();
            public readonly Dictionary<Vector2Int, EconomySettlementContext> Resolved =
                new();
            public RecruitmentPopulationSnapshot Population;

            public RecruitmentPopulationSnapshot GetRecruitmentPopulation(
                string ownerId, Vector2Int position) => Population;
            public bool TryReserveRecruitmentPopulation(string ownerId, Vector2Int position,
                long queueId, int count, out string reason)
            { reason = null; return false; }
            public void ReleaseRecruitmentPopulation(string ownerId, long queueId) { }
            public void DeployRecruitmentPopulation(string ownerId, long queueId, string unitId) { }
            public bool TryGetSettlementContext(Vector2Int position,
                out EconomySettlementContext context)
                => Contexts.TryGetValue(position, out context);
            public bool TryResolveConstructionSettlement(Vector2Int position, string ownerId,
                out EconomySettlementContext context)
                => Resolved.TryGetValue(position, out context);
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
            public bool IsOperational(Vector2Int position) => !NonOperational.Contains(position);
            public bool TryGetProgress(Vector2Int position,
                out int completedTurns, out int requiredTurns)
            { completedTurns = 0; requiredTurns = 0; return false; }
        }

        private sealed class FakeEconomyRuntime : IEconomyRuntimeApi
        {
            public readonly List<EconomySettlementSnapshot> Settlements = new();
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
                string ownerId) => Settlements;
            public EconomyProductionReadSnapshot GetOwnerProductionSnapshot(string ownerId)
                => new EconomyProductionReadSnapshot(
                    new Dictionary<string, float>(),
                    new Dictionary<string, int>());
        }

        private sealed class FakeSupply : IConstructionSupplyService
        {
            public readonly Dictionary<Vector2Int, ConstructionSupplyOrderSnapshot> Orders =
                new();
            public event Action Changed { add { } remove { } }
            public ConstructionSupplyEvaluation Evaluate(string ownerId, string buildingId,
                Vector2Int position, IReadOnlyDictionary<string, float> requiredCosts)
                => default;
            public CaravanTransferResult DispatchSupply(ConstructionSupplyDispatchRequest request,
                IReadOnlyDictionary<string, float> requiredCosts) => default;
            public IReadOnlyDictionary<string, float> PreviewShipment(
                ConstructionSupplyDispatchRequest request,
                IReadOnlyDictionary<string, float> requiredCosts)
                => new Dictionary<string, float>();
            public IReadOnlyList<ConstructionSupplyOrderSnapshot> GetOrders(string ownerId)
                => new List<ConstructionSupplyOrderSnapshot>();
            public bool TryGetOrderAt(Vector2Int position,
                out ConstructionSupplyOrderSnapshot snapshot)
                => Orders.TryGetValue(position, out snapshot);
            public IReadOnlyDictionary<string, float> GetResourcesForPlacement(
                string settlementId, Vector2Int position)
                => new Dictionary<string, float>();
            public void CancelOrderAt(Vector2Int position) { }
            public CaravanTransferResult ApplyConfirmedDispatch(
                ConstructionSupplyDispatchRequest request,
                IReadOnlyDictionary<string, float> requiredCosts) => default;
            public void ApplyConfirmedCancelOrder(string ownerId, string unitId,
                Vector2Int position) { }
        }

        private sealed class FakeRecruitment : IUnitRecruitmentService, IUnitRecruitmentQuery
        {
            public IReadOnlyList<UnitRecruitmentShortage> Shortages;
            public string Reason;
            public UnitRecruitmentQueueItemSnapshot? Ready;

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
            {
                if (Ready.HasValue) { item = Ready.Value; return true; }
                item = default; return false;
            }
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
    }
}
