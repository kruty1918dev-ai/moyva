using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.WorldCreation.API;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Construction
{
    /// <summary>
    /// Regression coverage for building selection from the construction list.
    /// The reported failure: every building evaluated as unselectable with
    /// code 'resources' because funding resolved to an empty owner pool while
    /// the settlement that held the real stock was inactive.
    /// </summary>
    [TestFixture]
    public sealed class ConstructionSelectionAvailabilityTests
    {
        private DiContainer _container;
        private SignalBus _signals;
        private FakeEconomyMediator _economy;
        private ConstructionService _service;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            global::Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<GameModeChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<SettlementResourceChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingOperationalSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingPreviewChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingPlacedSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingSelectionChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingPreviewMovedSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingCancelledSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingOwnershipTransferredSignal>().OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();

            var grid = new FakeGridService();
            for (var x = 0; x <= 10; x++)
            for (var y = 0; y <= 10; y++)
                grid.SetTileData(new Vector2Int(x, y), "grass");

            _economy = new FakeEconomyMediator();
            _service = new ConstructionService(
                new FakeObjectsMap(),
                new FakeBuildingRegistry(),
                _signals,
                minSpacing: 0,
                townHallBuildRadius: 10,
                fogOfWarService: null,
                wallTopologyService: null,
                wallGateReplacementValidator: null,
                economyInfoMediator: _economy,
                gridService: grid,
                generatedTerrainLevelQuery: new FakeTerrainLevelQuery(),
                tileSettings: new FakeTileSettings(),
                placementRulesProvider: new FakePlacementRules
                {
                    EnableFogRules = false,
                    EnableInfluenceZoneRules = false,
                    EnableTerrainRules = true,
                    AllowBuildingOnHills = true,
                    BlockEdgeTerrainTiles = false,
                },
                placementAuthorityPolicy: null,
                placementRuleEvaluators: null,
                turns: null,
                progressClock: new FakeProgressClock(),
                fogSettings: null);
            _service.Initialize();
            _service.SetActiveOwner("p1");
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            _container?.UnbindAll();
        }

        [Test]
        public void SelectionAvailability_EmptyOwnerPool_DisablesCostedBuilding()
        {
            // Live failure mode: funding fell back to the drained owner pool.
            _economy.HasWarehouse = false;

            var result = _service.EvaluateSelectionAvailability("house", "p1");

            Assert.IsTrue(result.ResourceCheckPerformed);
            Assert.IsFalse(result.ResourcesValid);
            Assert.IsFalse(result.CanSelect);
            Assert.AreEqual("resources", result.ReasonCode);
        }

        [Test]
        public void SelectionAvailability_FundedOwnerPool_AllowsSelection()
        {
            _economy.HasWarehouse = false;
            _economy.OwnerPool["walnut-wood-materials-resources"] = 80f;

            var result = _service.EvaluateSelectionAvailability("house", "p1");

            Assert.IsTrue(result.CanSelect, result.Reason);
        }

        [Test]
        public void SelectionAvailability_ActiveSettlementStock_AllowsSelection()
        {
            _economy.HasWarehouse = true;
            _economy.SettlementResources["walnut-wood-materials-resources"] = 80f;
            Assert.IsTrue(
                _service.TryApplySetupPlacement("depot", new Vector2Int(3, 3), "p1"),
                "Setup placement failed: " + _service.GetLastActionMessage());

            var result = _service.EvaluateSelectionAvailability("house", "p1");

            Assert.IsTrue(result.ResourceCheckPerformed);
            Assert.IsTrue(result.CanSelect, result.Reason);
        }

        [Test]
        public void SelectionAvailability_SettlementShortage_DisablesSelection()
        {
            _economy.HasWarehouse = true;
            _economy.SettlementResources["walnut-wood-materials-resources"] = 5f;
            Assert.IsTrue(
                _service.TryApplySetupPlacement("depot", new Vector2Int(3, 3), "p1"),
                "Setup placement failed: " + _service.GetLastActionMessage());

            var result = _service.EvaluateSelectionAvailability("house", "p1");

            Assert.IsFalse(result.CanSelect);
            Assert.AreEqual("resources", result.ReasonCode);
        }

        [Test]
        public void SelectBuilding_FundedSettlement_EntersPlacingState()
        {
            _economy.HasWarehouse = true;
            _economy.SettlementResources["walnut-wood-materials-resources"] = 80f;
            Assert.IsTrue(
                _service.TryApplySetupPlacement("depot", new Vector2Int(3, 3), "p1"),
                "Setup placement failed: " + _service.GetLastActionMessage());
            _signals.Fire(new GameModeChangedSignal
            {
                NewMode = GameModeType.Construction,
            });

            _service.SelectBuilding("house");

            Assert.AreEqual(BuildingPlacementState.Placing, _service.State,
                "Selection failed: " + _service.GetLastActionMessage());
            Assert.AreEqual("house", _service.GetSelectedBuildingId());
        }

        private sealed class FakeEconomyMediator : IEconomyInfoMediator
        {
            public bool HasWarehouse;
            public bool HasSettlement = true;
            public readonly Dictionary<string, float> SettlementResources =
                new(StringComparer.Ordinal);
            public readonly Dictionary<string, float> OwnerPool =
                new(StringComparer.Ordinal);

            public RecruitmentPopulationSnapshot GetRecruitmentPopulation(
                string ownerId, Vector2Int position)
                => new RecruitmentPopulationSnapshot(0, 0, 0, 0, 1f);

            public bool TryReserveRecruitmentPopulation(
                string ownerId, Vector2Int position, long queueId, int count,
                out string reason)
            {
                reason = null;
                return true;
            }

            public void ReleaseRecruitmentPopulation(string ownerId, long queueId) { }
            public void DeployRecruitmentPopulation(string ownerId, long queueId, string unitId) { }

            public bool TryGetSettlementContext(
                Vector2Int position, out EconomySettlementContext context)
            {
                context = HasSettlement
                    ? new EconomySettlementContext("s1", "Alpha", "p1")
                    : default;
                return HasSettlement;
            }

            public bool TryResolveConstructionSettlement(
                Vector2Int position, string ownerId,
                out EconomySettlementContext context)
            {
                context = HasSettlement
                    ? new EconomySettlementContext("s1", "Alpha", ownerId)
                    : default;
                return HasSettlement;
            }

            public bool TryGetBuildingContext(
                Vector2Int position, out string buildingId, out string ownerId)
            {
                buildingId = null;
                ownerId = null;
                return false;
            }

            public bool TryConsumeSettlementResources(
                string settlementId, IReadOnlyDictionary<string, float> resourceCosts,
                out string errorMessage)
            {
                errorMessage = null;
                return true;
            }

            public bool TryConsumeOwnerPoolResources(
                string ownerId, IReadOnlyDictionary<string, float> resourceCosts,
                out string errorMessage)
            {
                errorMessage = null;
                return true;
            }

            public void RefundOwnerPoolResources(
                string ownerId, IReadOnlyDictionary<string, float> resources) { }

            public void RefundRecruitmentResources(
                string ownerId, string settlementId,
                IReadOnlyDictionary<string, float> resources) { }

            public bool OwnerHasAnyWarehouse(string ownerId) => HasWarehouse;

            public IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(
                Vector2Int warehousePosition) => SettlementResources;

            public IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(
                string settlementId) => SettlementResources;

            public IReadOnlyDictionary<string, float> GetSettlementResourceTotals(
                string settlementId) => SettlementResources;

            public IReadOnlyDictionary<string, float> GetSettlementReservedResourceTotals(
                string settlementId) => new Dictionary<string, float>();

            public IReadOnlyDictionary<string, float> GetSettlementAvailableResourceTotals(
                string settlementId) => SettlementResources;

            public void ReleaseConstructionSupplyReservations(
                Vector2Int placementPosition) { }

            public IReadOnlyDictionary<string, float> GetSettlementResourcesForPlacement(
                string settlementId, Vector2Int placementPosition) => SettlementResources;

            public IReadOnlyDictionary<string, float> GetOwnerPoolResourceTotals(
                string ownerId) => OwnerPool;

            public IReadOnlyDictionary<string, float> GetOwnerResourceTotals(
                string ownerId) => OwnerPool;

            public string GetResourceDisplayName(string resourceId) => resourceId;
        }

        private sealed class FakeObjectsMap : IObjectsMapService
        {
            public bool IsOccupied(Vector2Int position) => false;
            public bool TryGetOccupant(Vector2Int position, out string occupantId)
            {
                occupantId = null;
                return false;
            }

            public void Register(Vector2Int position, string occupantId) { }
            public void Move(Vector2Int from, Vector2Int to) { }
            public void Unregister(Vector2Int position) { }
            public bool TryGetPosition(string occupantId, out Vector2Int position)
            {
                position = default;
                return false;
            }
        }

        private sealed class FakeBuildingRegistry : IBuildingRegistry
        {
            private readonly BuildingDefinition _house = new BuildingDefinition
            {
                Id = "house",
                DisplayName = "House",
                ConstructionCost =
                {
                    new BuildingDefinition.BuildingConstructionCostEntry
                    {
                        ResourceId = "walnut-wood-materials-resources",
                        Amount = 10,
                    },
                },
            };

            private readonly BuildingDefinition _depot = new BuildingDefinition
            {
                Id = "depot",
                DisplayName = "Depot",
            };

            public BuildingDefinition[] GetAll() => new[] { _house, _depot };
            public BuildingDefinition GetById(string id)
                => id == _house.Id ? _house : id == _depot.Id ? _depot : null;
            public BuildingDefinition[] GetByCategory(BuildingCategory category)
                => Array.Empty<BuildingDefinition>();
            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();
            public WallCollectionDefinition GetWallCollectionByBuildingId(
                string buildingId) => null;
        }

        private sealed class FakeProgressClock : IGameplayProgressClock
        {
            public event Action<GameplayProgressTick> Progressed
            {
                add { }
                remove { }
            }

            public GameplayProgressMode Mode => GameplayProgressMode.SandboxRealtime;
            public bool IsRealtime => true;
            public float SandboxRoundSeconds => 1f;
            public float Speed => 1f;
            public long CurrentSequence => 0;
            public double ElapsedGameplaySeconds => 0;
            public float SecondsUntilNextProgress => 0f;

            public void Configure(
                GameplayProgressMode mode, float sandboxRoundSeconds, float speed) { }
            public void SetSpeed(float speed) { }
        }

        private sealed class FakeGridService : IGridService
        {
            private readonly Dictionary<Vector2Int, string> _tiles = new();

            public string GetTileData(Vector2Int position)
                => _tiles.TryGetValue(position, out var id) ? id : null;

            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
                => _tiles.TryGetValue(position, out tileTypeId);

            public void SetTileData(Vector2Int position, string tileTypeId)
                => _tiles[position] = tileTypeId;

            public int GridWidth => 100;
            public int GridHeight => 100;
        }

        private sealed class FakeTerrainLevelQuery : IGeneratedTerrainLevelQuery
        {
            public bool TryGetTerrainLevel(Vector2Int position, out int level)
            {
                level = 0;
                return false;
            }

            public bool HasExplicitTerrainSurfaceMap => false;

            public bool TryGetTerrainSurfaceY(Vector2Int position, out float y)
            {
                y = 0f;
                return false;
            }
        }

        private sealed class FakeTileSettings : ITileSettingsService, ITerrainTagQuery
        {
            public float GetTileWeight(string tileId) => 1f;
            public bool IsBuildBlocked(string tileId) => false;
            public float GetSurfaceOffset(string tileId) => 0f;
            public bool HasTerrainTag(string tileId, string tag) => false;
        }

        private sealed class FakePlacementRules : IConstructionPlacementRulesProvider
        {
            public int MinSpacing { get; set; }
            public int TownHallBuildRadius { get; set; } = 10;
            public bool EnableInfluenceZoneRules { get; set; }
            public bool EnableTerrainRules { get; set; }
            public bool EnableFogRules { get; set; }
            public bool RequireVisibleFogTile { get; set; }
            public bool AllowBuildingOnWater { get; set; }
            public bool AllowBuildingAnywhereExceptWater { get; set; }
            public bool AllowBuildingOnHills { get; set; } = true;
            public bool BlockEdgeTerrainTiles { get; set; }
            public string[] BlockedTileIds { get; set; }
            public string[] AllowedTileIds { get; set; }
            public TerrainLevelRestrictionRange[] BlockedTerrainLevelRanges { get; set; }
        }
    }
}
