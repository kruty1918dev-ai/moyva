using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>P060: shortage advice must not recommend a producer whose own
    /// prerequisites form an unsatisfiable cycle — bounded search, honest
    /// "no achievable option" instead of infinite recursion.</summary>
    [TestFixture]
    internal sealed class ProducerFeasibilityResolverTests
    {
        private const string Owner = "p1";
        private const string Settlement = "s1";

        private static BuildingDefinition Producer(string id, string producedResourceId)
        {
            var definition = new BuildingDefinition { Id = id, DisplayName = id };
            definition.Modules.Add(new ProductionBuildingModule
            { ResourceId = producedResourceId });
            return definition;
        }

        private static ProducerFeasibilityResolver Resolver(
            FakeRegistry registry, FakeAvailability availability,
            FakeConstructionCommands construction, FakeEconomy economy)
            => new ProducerFeasibilityResolver(registry, availability, construction, economy);

        [Test, Timeout(10000)]
        public void CyclicProducers_NoStock_ReportsImpossible_WithoutHanging()
        {
            // A produces resA but costs resB; B produces resB but costs resA.
            // With no initial stock neither can ever be built.
            var registry = new FakeRegistry(
                Producer("prod-a", "res-a"), Producer("prod-b", "res-b"));
            var construction = new FakeConstructionCommands();
            construction.Costs["prod-a"] = new Dictionary<string, float> { ["res-b"] = 10f };
            construction.Costs["prod-b"] = new Dictionary<string, float> { ["res-a"] = 10f };
            var economy = new FakeEconomy(); // no stock anywhere

            ProducerSuggestion suggestion = Resolver(
                registry, new FakeAvailability(), construction, economy)
                .Suggest(Owner, Settlement, "res-a");

            Assert.AreEqual(ProducerSuggestionKind.Impossible, suggestion.Kind);
            Assert.IsNull(suggestion.BuildingId);
        }

        [Test]
        public void CyclicProducers_WithStock_ResolvesDirect()
        {
            var registry = new FakeRegistry(
                Producer("prod-a", "res-a"), Producer("prod-b", "res-b"));
            var construction = new FakeConstructionCommands();
            construction.Costs["prod-a"] = new Dictionary<string, float> { ["res-b"] = 10f };
            construction.Costs["prod-b"] = new Dictionary<string, float> { ["res-a"] = 10f };
            var economy = new FakeEconomy();
            economy.SettlementStock["res-b"] = 25f;

            ProducerSuggestion suggestion = Resolver(
                registry, new FakeAvailability(), construction, economy)
                .Suggest(Owner, Settlement, "res-a");

            Assert.AreEqual(ProducerSuggestionKind.Direct, suggestion.Kind);
            Assert.AreEqual("prod-a", suggestion.BuildingId);
            Assert.AreEqual("res-a", suggestion.ProducedResourceId);
        }

        [Test]
        public void BlockedProducer_SuggestsAchievablePrerequisiteStep()
        {
            // resA producer needs resB (missing), but a resB producer is free —
            // the honest achievable step is to build the resB producer first.
            var registry = new FakeRegistry(
                Producer("prod-a", "res-a"), Producer("prod-b", "res-b"));
            var construction = new FakeConstructionCommands();
            construction.Costs["prod-a"] = new Dictionary<string, float> { ["res-b"] = 10f };
            construction.Costs["prod-b"] = new Dictionary<string, float>();
            var economy = new FakeEconomy();

            ProducerSuggestion suggestion = Resolver(
                registry, new FakeAvailability(), construction, economy)
                .Suggest(Owner, Settlement, "res-a");

            Assert.AreEqual(ProducerSuggestionKind.ViaPrerequisite, suggestion.Kind);
            Assert.AreEqual("prod-b", suggestion.BuildingId);
            Assert.AreEqual("res-b", suggestion.ProducedResourceId);
        }

        [Test]
        public void DeepChain_SuggestsRootAchievableStep()
        {
            // resA needs resB needs resC; only the resC producer is affordable.
            var registry = new FakeRegistry(
                Producer("prod-a", "res-a"), Producer("prod-b", "res-b"),
                Producer("prod-c", "res-c"));
            var construction = new FakeConstructionCommands();
            construction.Costs["prod-a"] = new Dictionary<string, float> { ["res-b"] = 5f };
            construction.Costs["prod-b"] = new Dictionary<string, float> { ["res-c"] = 5f };
            construction.Costs["prod-c"] = new Dictionary<string, float>();
            var economy = new FakeEconomy();

            ProducerSuggestion suggestion = Resolver(
                registry, new FakeAvailability(), construction, economy)
                .Suggest(Owner, Settlement, "res-a");

            Assert.AreEqual(ProducerSuggestionKind.ViaPrerequisite, suggestion.Kind);
            Assert.AreEqual("prod-c", suggestion.BuildingId);
            Assert.AreEqual("res-c", suggestion.ProducedResourceId);
        }

        [Test]
        public void NoRegisteredProducer_ReportsNone()
        {
            var suggestion = Resolver(
                    new FakeRegistry(), new FakeAvailability(),
                    new FakeConstructionCommands(), new FakeEconomy())
                .Suggest(Owner, Settlement, "res-x");

            Assert.AreEqual(ProducerSuggestionKind.None, suggestion.Kind);
            Assert.IsNull(suggestion.BuildingId);
        }

        [Test]
        public void LockedProducer_ReportsImpossible()
        {
            var registry = new FakeRegistry(Producer("prod-a", "res-a"));
            var construction = new FakeConstructionCommands();
            construction.Costs["prod-a"] = new Dictionary<string, float>();
            var availability = new FakeAvailability();
            availability.Locked.Add("prod-a");

            ProducerSuggestion suggestion = Resolver(
                registry, availability, construction, new FakeEconomy())
                .Suggest(Owner, Settlement, "res-a");

            Assert.AreEqual(ProducerSuggestionKind.Impossible, suggestion.Kind);
        }

        [Test]
        public void PartialStock_DoesNotCoverCost()
        {
            // 5 in stock vs 10 required — producer is NOT achievable.
            var registry = new FakeRegistry(Producer("prod-a", "res-a"));
            var construction = new FakeConstructionCommands();
            construction.Costs["prod-a"] = new Dictionary<string, float> { ["res-b"] = 10f };
            var economy = new FakeEconomy();
            economy.SettlementStock["res-b"] = 5f;

            ProducerSuggestion suggestion = Resolver(
                registry, new FakeAvailability(), construction, economy)
                .Suggest(Owner, Settlement, "res-a");

            Assert.AreEqual(ProducerSuggestionKind.Impossible, suggestion.Kind);
        }

        [Test]
        public void SelfDependentProducer_NoStock_ReportsImpossible()
        {
            // Producer needs the very resource it produces — a degenerate cycle.
            var registry = new FakeRegistry(Producer("prod-a", "res-a"));
            var construction = new FakeConstructionCommands();
            construction.Costs["prod-a"] = new Dictionary<string, float> { ["res-a"] = 10f };

            ProducerSuggestion suggestion = Resolver(
                registry, new FakeAvailability(), construction, new FakeEconomy())
                .Suggest(Owner, Settlement, "res-a");

            Assert.AreEqual(ProducerSuggestionKind.Impossible, suggestion.Kind);
        }

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
            public readonly HashSet<string> Locked = new(StringComparer.Ordinal);
            public ConstructionSelectionAvailabilityResult EvaluateSelectionAvailability(
                string buildingId, string ownerId = null,
                Vector2Int? preferredFundingPosition = null,
                bool includePendingPlacements = true)
                => Locked.Contains(buildingId)
                    ? new ConstructionSelectionAvailabilityResult(false, true, true, "Locked")
                    : new ConstructionSelectionAvailabilityResult(true, true, true);
        }

        private sealed class FakeConstructionCommands : IConstructionSessionCommands
        {
            public readonly Dictionary<string, Dictionary<string, float>> Costs =
                new(StringComparer.Ordinal);

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
            { status = default; return false; }
            public ConstructionResourceProjection GetResourceProjection(Vector2Int position) => null;
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
    }
}
