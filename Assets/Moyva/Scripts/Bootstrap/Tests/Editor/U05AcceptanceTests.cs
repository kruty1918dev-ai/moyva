using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;
using UnityEngine;
using UnityHTML.Runtime;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>
    /// U05: the HUD read model must present the same owner and the same
    /// resource figures as the authoritative economy services — concrete
    /// resource IDs stay concrete (no edible resource may wear the aggregate
    /// "Food" name), totals come straight from GetOwnerResourceTotals without
    /// a simulated tick, and a re-capture after state change (load/owner
    /// switch) reflects the new authoritative state exactly.
    /// </summary>
    [TestFixture]
    internal sealed class U05AcceptanceTests
    {
        private static GameplayHudReadModel CreateReadModel(
            FakeTurnService turns,
            FakeEconomyRuntimeApi economy,
            FakeEconomyInfoMediator mediator = null)
            => new GameplayHudReadModel(
                turns, economy, null, null, population: mediator);

        private static string DisplayResource(GameplayHudReadModel model, string id)
            => (string)typeof(GameplayHudReadModel)
                .GetMethod("DisplayResource", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(model, new object[] { id });

        [Test]
        public void Capture_Resources_MatchAuthoritativeOwnerTotals_Exactly()
        {
            var turns = new FakeTurnService { LocalOwnerId = "player_0", ActiveOwnerId = "player_0" };
            var economy = new FakeEconomyRuntimeApi();
            economy.Totals["player_0"] = new Dictionary<string, float>
            {
                ["steak-food-resources"] = 12.5f,
                ["cabbage-food-resources"] = 3f,
                ["walnut-wood-materials-resources"] = 40f,
            };
            var readModel = CreateReadModel(turns, economy);

            GameplayHtmlSnapshot snapshot = readModel.Capture(new GameplayHtmlState());

            Assert.AreEqual("player_0", snapshot.OwnerId);
            Assert.AreEqual(3, snapshot.Resources.Length,
                "Every concrete resource ID must remain its own row — no merging into an aggregate.");
            var byId = snapshot.Resources.ToDictionary(r => r.Id, r => r.Amount);
            Assert.AreEqual(12.5f, byId["steak-food-resources"], "Steak amount must equal the authoritative total цифра в цифру.");
            Assert.AreEqual(3f, byId["cabbage-food-resources"], "A second edible resource must appear as its own concrete entry.");
            Assert.AreEqual(40f, byId["walnut-wood-materials-resources"]);
        }

        [Test]
        public void Capture_Resources_OrderCanonicalFirst_ThenById()
        {
            var turns = new FakeTurnService { LocalOwnerId = "player_0" };
            var economy = new FakeEconomyRuntimeApi();
            economy.Totals["player_0"] = new Dictionary<string, float>
            {
                ["zebra-food-resources"] = 1f,
                ["walnut-wood-materials-resources"] = 5f,
                ["apple-food-resources"] = 2f,
                ["steak-food-resources"] = 7f,
            };
            var readModel = CreateReadModel(turns, economy);

            GameplayHtmlSnapshot snapshot = readModel.Capture(new GameplayHtmlState());

            CollectionAssert.AreEqual(
                new[]
                {
                    "steak-food-resources",
                    "walnut-wood-materials-resources",
                    "apple-food-resources",
                    "zebra-food-resources",
                },
                snapshot.Resources.Select(r => r.Id).ToArray(),
                "Canonical resources lead in their configured order, the rest sort by id.");
        }

        [Test]
        public void Capture_OwnerSwitch_ReadsNewOwnersAuthoritativeTotals()
        {
            var turns = new FakeTurnService { LocalOwnerId = "player_0", ActiveOwnerId = "player_0" };
            var economy = new FakeEconomyRuntimeApi();
            economy.Totals["player_0"] = new Dictionary<string, float> { ["steak-food-resources"] = 10f };
            economy.Totals["player_1"] = new Dictionary<string, float> { ["steak-food-resources"] = 99f };
            var readModel = CreateReadModel(turns, economy);
            var state = new GameplayHtmlState();

            GameplayHtmlSnapshot first = readModel.Capture(state);
            Assert.AreEqual("player_0", first.OwnerId);
            Assert.AreEqual(10f, first.Resources.Single(r => r.Id == "steak-food-resources").Amount);

            turns.LocalOwnerId = "player_1";
            turns.ActiveOwnerId = "player_1";
            GameplayHtmlSnapshot second = readModel.Capture(state);

            Assert.AreEqual("player_1", second.OwnerId, "Owner switch must re-resolve the local owner.");
            Assert.AreEqual(99f, second.Resources.Single(r => r.Id == "steak-food-resources").Amount,
                "After an owner switch the HUD must show that owner's totals, not the previous owner's.");
        }

        [Test]
        public void Capture_RecaptureAfterStateChange_ReflectsNewAuthoritativeState()
        {
            var turns = new FakeTurnService { LocalOwnerId = "player_0" };
            var economy = new FakeEconomyRuntimeApi();
            economy.Totals["player_0"] = new Dictionary<string, float> { ["steak-food-resources"] = 4f };
            var readModel = CreateReadModel(turns, economy);

            GameplayHtmlSnapshot before = readModel.Capture(new GameplayHtmlState());
            economy.Totals["player_0"]["steak-food-resources"] = 4f; // same value — second capture must not drift
            GameplayHtmlSnapshot repeat = readModel.Capture(new GameplayHtmlState());
            Assert.AreEqual(4f, repeat.Resources.Single(r => r.Id == "steak-food-resources").Amount,
                "A repeated capture must not simulate a tick: values stay identical while the service is unchanged.");
            Assert.AreEqual(before.Resources.Length, repeat.Resources.Length);

            // Simulated save/load: authoritative state replaced wholesale.
            economy.Totals["player_0"] = new Dictionary<string, float>
            {
                ["steak-food-resources"] = 0f,
                ["gold-coins-materials-resources"] = 7f,
            };
            GameplayHtmlSnapshot loaded = readModel.Capture(new GameplayHtmlState());
            Assert.AreEqual(0f, loaded.Resources.Single(r => r.Id == "steak-food-resources").Amount);
            Assert.AreEqual(7f, loaded.Resources.Single(r => r.Id == "gold-coins-materials-resources").Amount,
                "A post-load capture must mirror the loaded authoritative totals exactly.");
        }

        [Test]
        public void DisplayResource_ConcreteEdibleResource_UsesItsOwnName_NotAggregateFood()
        {
            var mediator = new FakeEconomyInfoMediator();
            mediator.DisplayNames["cabbage-food-resources"] = "Cabbage";
            var readModel = CreateReadModel(new FakeTurnService(), new FakeEconomyRuntimeApi(), mediator);

            Assert.AreEqual("Steak", DisplayResource(readModel, "steak-food-resources"),
                "steak-food-resources is one concrete resource; 'Food' is the aggregate category and must not be its label.");
            Assert.AreEqual("Cabbage", DisplayResource(readModel, "cabbage-food-resources"),
                "Non-canonical food resources must use the authoritative displayName, not leak the '-food-' category token.");
        }

        [Test]
        public void DisplayResource_UnknownId_FallsBackToDerivedName()
        {
            var readModel = CreateReadModel(new FakeTurnService(), new FakeEconomyRuntimeApi(),
                new FakeEconomyInfoMediator());

            Assert.AreEqual("Wood", DisplayResource(readModel, "walnut-wood-materials-resources"),
                "Canonical short labels stay authoritative for the curated top-bar set.");
            Assert.AreEqual("Mystery Thing", DisplayResource(readModel, "mystery-thing"),
                "Unrecognised ids still derive a readable name.");
            Assert.AreEqual("Unknown", DisplayResource(readModel, " "));
        }

        [Test]
        public void Capture_KingdomPanel_PopulationComesFromSettlementSnapshots()
        {
            var turns = new FakeTurnService { LocalOwnerId = "player_0" };
            var economy = new FakeEconomyRuntimeApi();
            economy.Settlements["player_0"] = new List<EconomySettlementSnapshot>
            {
                new EconomySettlementSnapshot("s1", "One", 5, 3,
                    new Dictionary<string, float> { ["steak-food-resources"] = 2f }),
                new EconomySettlementSnapshot("s2", "Two", 7, 4,
                    new Dictionary<string, float> { ["cabbage-food-resources"] = 1f }),
            };
            var readModel = CreateReadModel(turns, economy);
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Kingdom);

            GameplayHtmlSnapshot snapshot = readModel.Capture(state);

            Assert.AreEqual(2, snapshot.SettlementCount);
            Assert.AreEqual(12, snapshot.Population,
                "Population must be the sum of authoritative settlement residents, not a separate model.");
            CollectionAssert.AreEqual(
                new[] { "s1", "s2" },
                snapshot.Settlements.Select(s => s.Id).ToArray());
        }

        private sealed class FakeTurnService : ITurnService
        {
#pragma warning disable CS0067
            public event Action StateChanged;
#pragma warning restore CS0067
            public TurnPhase Phase { get; set; } = TurnPhase.AwaitingInput;
            public int Round { get; set; } = 3;
            public long GlobalTurn { get; set; } = 9;
            public int ActionsThisTurn { get; set; }
            public string ActiveOwnerId { get; set; } = "player_0";
            public string LocalOwnerId { get; set; } = "player_0";
            public IReadOnlyList<TurnFaction> Factions { get; set; }
            public bool IsOwnerActive(string ownerId) => true;
            public bool CanOwnerAct(string ownerId, out string reason) { reason = null; return true; }
            public bool TryRecordAction(string ownerId, string actionId) => true;
            public bool TryEndTurn(string requesterOwnerId, out string reason) { reason = null; return true; }
        }

        private sealed class FakeEconomyRuntimeApi : IEconomyRuntimeApi
        {
            public readonly Dictionary<string, Dictionary<string, float>> Totals = new();
            public readonly Dictionary<string, List<EconomySettlementSnapshot>> Settlements = new();

            public IReadOnlyList<string> GetSettlementIdsForOwner(string ownerId)
                => Settlements.TryGetValue(ownerId, out var list)
                    ? list.Select(s => s.SettlementId).ToList()
                    : new List<string>();
            public EconomyCategoryTotals GetOwnerCategoryTotals(string ownerId)
                => new EconomyCategoryTotals(0f, 0f, 0f);
            public EconomyFormattedCategoryTotals GetFormattedOwnerCategoryTotals(string ownerId)
                => new EconomyFormattedCategoryTotals("0", "0", "0");
            public Dictionary<string, float> GetOwnerResourceTotals(string ownerId)
                => Totals.TryGetValue(ownerId, out var totals)
                    ? totals
                    : new Dictionary<string, float>(StringComparer.Ordinal);
            public EconomyCategoryTotals GetSettlementCategoryTotals(string settlementId)
                => new EconomyCategoryTotals(0f, 0f, 0f);
            public EconomyFormattedCategoryTotals GetFormattedSettlementCategoryTotals(string settlementId)
                => new EconomyFormattedCategoryTotals("0", "0", "0");
            public Dictionary<string, float> GetSettlementResourceTotals(string settlementId)
                => new Dictionary<string, float>(StringComparer.Ordinal);
            public IReadOnlyList<EconomyWarehouseSnapshot> GetOwnerWarehouseSnapshots(string ownerId)
                => Array.Empty<EconomyWarehouseSnapshot>();
            public IReadOnlyList<EconomySettlementSnapshot> GetOwnerSettlementSnapshots(string ownerId)
                => Settlements.TryGetValue(ownerId, out var list)
                    ? list
                    : (IReadOnlyList<EconomySettlementSnapshot>)Array.Empty<EconomySettlementSnapshot>();
            public EconomyProductionReadSnapshot GetOwnerProductionSnapshot(string ownerId)
                => default;
        }

        private sealed class FakeEconomyInfoMediator : IEconomyInfoMediator
        {
            public readonly Dictionary<string, string> DisplayNames = new(StringComparer.Ordinal);

            public RecruitmentPopulationSnapshot GetRecruitmentPopulation(string ownerId, Vector2Int position)
                => default;
            public bool TryReserveRecruitmentPopulation(string ownerId, Vector2Int position, long queueId, int count, out string reason)
            { reason = null; return false; }
            public void ReleaseRecruitmentPopulation(string ownerId, long queueId) { }
            public void DeployRecruitmentPopulation(string ownerId, long queueId, string unitId) { }
            public bool TryGetSettlementContext(Vector2Int position, out EconomySettlementContext context)
            { context = default; return false; }
            public bool TryResolveConstructionSettlement(Vector2Int position, string ownerId, out EconomySettlementContext context)
            { context = default; return false; }
            public bool TryGetBuildingContext(Vector2Int position, out string buildingId, out string ownerId)
            { buildingId = null; ownerId = null; return false; }
            public bool TryConsumeSettlementResources(string settlementId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            { errorMessage = null; return false; }
            public bool TryConsumeOwnerPoolResources(string ownerId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            { errorMessage = null; return false; }
            public void RefundOwnerPoolResources(string ownerId, IReadOnlyDictionary<string, float> resources) { }
            public void RefundRecruitmentResources(string ownerId, string settlementId, IReadOnlyDictionary<string, float> resources) { }
            public bool OwnerHasAnyWarehouse(string ownerId) => false;
            public IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(Vector2Int warehousePosition)
                => new Dictionary<string, float>();
            public IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(string settlementId)
                => new Dictionary<string, float>();
            public IReadOnlyDictionary<string, float> GetSettlementResourceTotals(string settlementId)
                => new Dictionary<string, float>();
            public IReadOnlyDictionary<string, float> GetSettlementReservedResourceTotals(string settlementId)
                => new Dictionary<string, float>();
            public IReadOnlyDictionary<string, float> GetSettlementAvailableResourceTotals(string settlementId)
                => new Dictionary<string, float>();
            public void ReleaseConstructionSupplyReservations(Vector2Int placementPosition) { }
            public IReadOnlyDictionary<string, float> GetSettlementResourcesForPlacement(
                string settlementId, Vector2Int placementPosition) => new Dictionary<string, float>();
            public IReadOnlyDictionary<string, float> GetOwnerPoolResourceTotals(string ownerId)
                => new Dictionary<string, float>();
            public IReadOnlyDictionary<string, float> GetOwnerResourceTotals(string ownerId)
                => new Dictionary<string, float>();
            public string GetResourceDisplayName(string resourceId)
                => resourceId != null && DisplayNames.TryGetValue(resourceId.Trim(), out string name)
                    ? name
                    : resourceId?.Trim();
        }
    }
}
