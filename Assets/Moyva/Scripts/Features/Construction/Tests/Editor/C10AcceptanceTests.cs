using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.FogOfWar.API;
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
    /// C10 — the confirm step is the single owner of the resource spend:
    /// preview never pays, commit pays exactly once, failed commits pay
    /// nothing, and cancellation releases supply reservations without
    /// fabricating refunds.
    /// </summary>
    [TestFixture]
    public sealed class C10AcceptanceTests
    {
        private static readonly Vector2Int PosA = new Vector2Int(5, 5);
        private static readonly Vector2Int PosB = new Vector2Int(9, 9);

        private DiContainer _container;
        private SignalBus _signals;
        private C10Fakes.ObjectsMap _objects;
        private C10Fakes.Grid _grid;
        private C10Fakes.Mediator _mediator;
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

            _objects = new C10Fakes.ObjectsMap();
            _grid = new C10Fakes.Grid();
            _mediator = new C10Fakes.Mediator
            {
                SettlementResources = new Dictionary<string, float>
                {
                    ["wood"] = 100f
                }
            };

            for (var x = 0; x <= 12; x++)
            for (var y = 0; y <= 12; y++)
                _grid.SetTileData(new Vector2Int(x, y), "grass");

            _service = new ConstructionService(
                _objects,
                new C10Fakes.Registry(),
                _signals,
                minSpacing: 4,
                townHallBuildRadius: 3,
                fogOfWarService: null,
                wallTopologyService: null,
                wallGateReplacementValidator: null,
                economyInfoMediator: _mediator,
                gridService: _grid,
                generatedTerrainLevelQuery: new C10Fakes.Terrain(),
                tileSettings: new C10Fakes.Tiles(),
                placementRulesProvider: new C10Fakes.Rules
                {
                    EnableTerrainRules = true,
                    AllowBuildingAnywhereExceptWater = true,
                },
                placementAuthorityPolicy: null,
                placementRuleEvaluators: null,
                turns: null,
                progressClock: new C10Fakes.Clock(),
                fogSettings: null);

            _service.Initialize();
            _signals.Fire(new GameModeChangedSignal
            {
                NewMode = GameModeType.Construction,
            });
            _service.SetActiveOwner("p1");
            _service.SelectBuilding("house"); // costs 10 wood
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            _container?.UnbindAll();
        }

        [Test]
        public void Preview_NeverSpends_AndCommit_SpendsExactlyOnce()
        {
            Assert.IsTrue(_service.TryPreviewAt(PosA));
            Assert.AreEqual(
                0, _mediator.ConsumeCalls,
                "Queueing a preview must not consume resources");

            _service.Confirm();

            Assert.AreEqual(1, _mediator.ConsumeCalls);
            Assert.AreEqual(
                10f, _mediator.TotalConsumed.GetValueOrDefault("wood"),
                0.0001f,
                "Commit consumes the building's cost once");
        }

        [Test]
        public void CancelledPreview_ReleasesSupply_NeverRefunds()
        {
            _service.TryPreviewAt(PosA);
            _service.RemovePendingAt(PosA);

            Assert.IsTrue(
                _mediator.ReleasedSupplyPositions.Contains(PosA),
                "Cancelling frees the placement's supply reservations");
            Assert.AreEqual(
                0, _mediator.ConsumeCalls,
                "Nothing was spent — nothing may be refunded");
            Assert.AreEqual(0, _mediator.RefundCalls);
        }

        [Test]
        public void FailedCommit_SpendsNothing_AndKeepsPending()
        {
            _objects.Register(PosA, "foreign-object");
            _service.TryPreviewAt(PosA); // preview OK: objects map checked at commit

            _objects.Register(PosB, "foreign-object");
            _service.TryPreviewAt(PosB);

            // Re-block PosA so its commit path fails CanPlaceAt.
            _service.Confirm();

            Assert.AreEqual(
                0, _mediator.ConsumeCalls,
                "Occupied tiles must reject before any resource is spent");
        }

        [Test]
        public void PendingPlacements_ReserveAgainstFuturePreviews()
        {
            // Only 12 wood: two 10-wood houses can never both commit.
            _mediator.SettlementResources = new Dictionary<string, float>
            {
                ["wood"] = 12f
            };

            Assert.IsTrue(_service.TryPreviewAt(PosA));

            var second = _service.EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    "house",
                    PosB,
                    includeResources: true,
                    includeDetails: true,
                    ownerId: "p1",
                    attemptSource:
                        ConstructionPlacementAttemptSource.PointerClick));

            Assert.IsFalse(
                second.ResourcesValid,
                "Second preview must see pool minus the pending placement's cost");
        }

        [Test]
        public void UndoRedo_TouchesOnlyPreviews_NeverEconomy()
        {
            _service.TryPreviewAt(PosA);
            _service.TryPreviewAt(PosB);
            _service.UndoLast();

            Assert.AreEqual(1, _service.GetPendingPlacements().Count);
            _service.RedoLast();
            Assert.AreEqual(2, _service.GetPendingPlacements().Count);

            Assert.AreEqual(0, _mediator.ConsumeCalls);
            Assert.AreEqual(0, _mediator.RefundCalls);
        }
    }

    internal static class C10Fakes
    {
        internal sealed class Mediator : IEconomyInfoMediator
        {
            public Dictionary<string, float> SettlementResources = new();
            public int ConsumeCalls;
            public int RefundCalls;
            public readonly Dictionary<string, float> TotalConsumed =
                new(StringComparer.Ordinal);
            public readonly List<Vector2Int> ReleasedSupplyPositions = new();

            public RecruitmentPopulationSnapshot GetRecruitmentPopulation(
                string ownerId, Vector2Int position)
                => new RecruitmentPopulationSnapshot(
                    total: 0, available: 0, training: 0, military: 0,
                    constructionSpeed: 1f);
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
                context = new EconomySettlementContext("s1", "Test", "p1");
                return true;
            }
            public bool TryResolveConstructionSettlement(
                Vector2Int position, string ownerId,
                out EconomySettlementContext context)
                => TryGetSettlementContext(position, out context);
            public bool TryGetBuildingContext(
                Vector2Int position, out string buildingId, out string ownerId)
            {
                buildingId = null;
                ownerId = null;
                return false;
            }
            public bool TryConsumeSettlementResources(
                string settlementId,
                IReadOnlyDictionary<string, float> resourceCosts,
                out string errorMessage)
            {
                ConsumeCalls++;
                foreach (var pair in resourceCosts)
                {
                    float have = SettlementResources.GetValueOrDefault(pair.Key);
                    if (have + 0.0001f < pair.Value)
                    {
                        errorMessage = $"not enough {pair.Key}";
                        return false;
                    }
                }
                foreach (var pair in resourceCosts)
                {
                    SettlementResources[pair.Key] -= pair.Value;
                    TotalConsumed[pair.Key] =
                        TotalConsumed.GetValueOrDefault(pair.Key) + pair.Value;
                }
                errorMessage = null;
                return true;
            }
            public bool TryConsumeOwnerPoolResources(
                string ownerId,
                IReadOnlyDictionary<string, float> resourceCosts,
                out string errorMessage)
            {
                errorMessage = "no owner pool in fixture";
                return false;
            }
            public void RefundOwnerPoolResources(
                string ownerId, IReadOnlyDictionary<string, float> resources)
                => RefundCalls++;
            public void RefundRecruitmentResources(
                string ownerId, string settlementId,
                IReadOnlyDictionary<string, float> resources)
                => RefundCalls++;
            public bool OwnerHasAnyWarehouse(string ownerId) => true;
            public IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(
                Vector2Int warehousePosition) => SettlementResources;
            public IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(
                string settlementId) => SettlementResources;
            public IReadOnlyDictionary<string, float> GetSettlementResourceTotals(
                string settlementId) => SettlementResources;
            public IReadOnlyDictionary<string, float>
                GetSettlementReservedResourceTotals(string settlementId)
                => new Dictionary<string, float>();
            public IReadOnlyDictionary<string, float>
                GetSettlementAvailableResourceTotals(string settlementId)
                => SettlementResources;
            public void ReleaseConstructionSupplyReservations(
                Vector2Int placementPosition)
                => ReleasedSupplyPositions.Add(placementPosition);
            public IReadOnlyDictionary<string, float>
                GetSettlementResourcesForPlacement(
                    string settlementId, Vector2Int placementPosition)
                => SettlementResources;
            public IReadOnlyDictionary<string, float>
                GetOwnerPoolResourceTotals(string ownerId)
                => new Dictionary<string, float>();
            public IReadOnlyDictionary<string, float> GetOwnerResourceTotals(
                string ownerId) => SettlementResources;
            public string GetResourceDisplayName(string resourceId)
                => resourceId;
        }

        internal sealed class ObjectsMap : IObjectsMapService
        {
            private readonly Dictionary<Vector2Int, string> _occupants = new();
            public bool IsOccupied(Vector2Int position)
                => _occupants.ContainsKey(position);
            public bool TryGetOccupant(Vector2Int position, out string occupantId)
                => _occupants.TryGetValue(position, out occupantId);
            public void Register(Vector2Int position, string occupantId)
                => _occupants[position] = occupantId;
            public void Move(Vector2Int from, Vector2Int to)
            {
                string id = _occupants[from];
                _occupants.Remove(from);
                _occupants[to] = id;
            }
            public void Unregister(Vector2Int position)
                => _occupants.Remove(position);
            public bool TryGetPosition(string occupantId, out Vector2Int position)
            {
                foreach (var pair in _occupants)
                {
                    if (pair.Value != occupantId) continue;
                    position = pair.Key;
                    return true;
                }
                position = default;
                return false;
            }
        }

        internal sealed class Registry : IBuildingRegistry
        {
            private readonly BuildingDefinition _house = new BuildingDefinition
            {
                Id = "house",
                DisplayName = "House",
                Footprint = new BuildingFootprint
                {
                    Size = Vector2Int.one,
                    Anchor = BuildingFootprintAnchor.Center,
                },
                ConstructionCost = new List<BuildingDefinition.BuildingConstructionCostEntry>
                {
                    new BuildingDefinition.BuildingConstructionCostEntry
                        { ResourceId = "wood", Amount = 10 }
                }
            };

            public BuildingDefinition[] GetAll() => new[] { _house };
            public BuildingDefinition GetById(string id)
                => id == _house.Id ? _house : null;
            public BuildingDefinition[] GetByCategory(BuildingCategory category)
                => Array.Empty<BuildingDefinition>();
            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();
            public WallCollectionDefinition GetWallCollectionByBuildingId(
                string buildingId) => null;
        }

        internal sealed class Clock : IGameplayProgressClock
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
            public void Configure(GameplayProgressMode mode, float sandboxRoundSeconds, float speed) { }
            public void SetSpeed(float speed) { }
        }

        internal sealed class Grid : IGridService
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

        internal sealed class Terrain
            : IGeneratedTerrainLevelQuery, IGeneratedTerrainWaterQuery
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
            public bool HasWaterMap => true;
            public bool TryGetWaterCell(Vector2Int position, out bool isWater)
            {
                isWater = false;
                return true;
            }
        }

        internal sealed class Tiles : ITileSettingsService, ITerrainTagQuery
        {
            public float GetTileWeight(string tileId) => 1f;
            public bool IsBuildBlocked(string tileId) => false;
            public float GetSurfaceOffset(string tileId) => 0f;
            public bool HasTerrainTag(string tileId, string tag) => false;
        }

        internal sealed class Rules : IConstructionPlacementRulesProvider
        {
            public int MinSpacing { get; set; }
            public int TownHallBuildRadius { get; set; }
            public bool EnableInfluenceZoneRules { get; set; }
            public bool EnableTerrainRules { get; set; }
            public bool EnableFogRules { get; set; }
            public bool RequireVisibleFogTile { get; set; }
            public bool AllowBuildingOnWater { get; set; }
            public bool AllowBuildingAnywhereExceptWater { get; set; }
            public bool AllowBuildingOnHills { get; set; }
            public bool BlockEdgeTerrainTiles { get; set; }
            public string[] BlockedTileIds { get; set; }
            public string[] AllowedTileIds { get; set; }
            public TerrainLevelRestrictionRange[] BlockedTerrainLevelRanges { get; set; }
        }
    }
}
