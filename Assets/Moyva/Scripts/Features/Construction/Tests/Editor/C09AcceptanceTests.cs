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
    /// C09 — read-only placement queries (hover/audit) never mutate pending
    /// placements; only the command path (click → TryPreviewAt) adds them.
    /// Batch-building is preserved: repeated commands on distinct cells all
    /// become pending.
    /// </summary>
    [TestFixture]
    public sealed class C09AcceptanceTests
    {
        private static readonly Vector2Int PosA = new Vector2Int(5, 5);
        private static readonly Vector2Int PosB = new Vector2Int(7, 7);
        private static readonly Vector2Int PosC = new Vector2Int(9, 9);

        private DiContainer _container;
        private SignalBus _signals;
        private C09Fakes.ObjectsMap _objects;
        private C09Fakes.Grid _grid;
        private C09Fakes.Terrain _terrain;
        private C09Fakes.Tiles _tiles;
        private C09Fakes.Rules _rules;
        private C09Fakes.Fog _fog;
        private ConstructionService _service;
        private List<BuildingPreviewChangedSignal> _previewSignals;

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

            _previewSignals = new List<BuildingPreviewChangedSignal>();
            _signals.Subscribe<BuildingPreviewChangedSignal>(
                s => _previewSignals.Add(s));

            _objects = new C09Fakes.ObjectsMap();
            _grid = new C09Fakes.Grid();
            _terrain = new C09Fakes.Terrain();
            _tiles = new C09Fakes.Tiles();
            _fog = new C09Fakes.Fog { DefaultState = FogStateType.Visible };
            _rules = new C09Fakes.Rules
            {
                EnableTerrainRules = true,
                AllowBuildingAnywhereExceptWater = true,
            };

            for (var x = 0; x <= 12; x++)
            for (var y = 0; y <= 12; y++)
                _grid.SetTileData(new Vector2Int(x, y), "grass");

            _service = new ConstructionService(
                _objects,
                new C09Fakes.Registry(),
                _signals,
                minSpacing: 4,
                townHallBuildRadius: 3,
                fogOfWarService: _fog,
                wallTopologyService: null,
                wallGateReplacementValidator: null,
                economyInfoMediator: null,
                gridService: _grid,
                generatedTerrainLevelQuery: _terrain,
                tileSettings: _tiles,
                placementRulesProvider: _rules,
                placementAuthorityPolicy: null,
                placementRuleEvaluators: null,
                turns: null,
                progressClock: new C09Fakes.Clock(),
                fogSettings: null);

            _service.Initialize();
            _signals.Fire(new GameModeChangedSignal
            {
                NewMode = GameModeType.Construction,
            });
            _service.SetActiveOwner("p1");
            _service.SelectBuilding("house");
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            _container?.UnbindAll();
        }

        private ConstructionPlacementQueryResult Hover(Vector2Int pos)
            => _service.EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    "house", pos,
                    includeDetails: true,
                    ownerId: "p1",
                    attemptSource:
                        ConstructionPlacementAttemptSource.PointerHover));

        [Test]
        public void HoverQuery_NeverAddsPendingPlacements()
        {
            Assert.IsTrue(Hover(PosA).CanPreview);
            Hover(PosB);
            Hover(PosC);
            Hover(PosA); // repeat hover

            Assert.AreEqual(
                0, _service.GetPendingPlacements().Count,
                "Hover scanning must not queue pending placements");
        }

        [Test]
        public void CommandClick_AddsPendingPlacement()
        {
            Assert.IsTrue(_service.TryPreviewAt(PosA));

            var pending = _service.GetPendingPlacements();
            Assert.AreEqual(1, pending.Count);
            Assert.IsTrue(
                _service.HasPendingPlacementAt(PosA));
            Assert.IsTrue(
                _service.TryGetPendingBuildingIdAt(PosA, out string id));
            Assert.AreEqual("house", id);
        }

        [Test]
        public void BatchCommands_OnDistinctCells_AllBecomePending()
        {
            Assert.IsTrue(_service.TryPreviewAt(PosA));
            Assert.IsTrue(_service.TryPreviewAt(PosB));
            Assert.IsTrue(_service.TryPreviewAt(PosC));

            var pending = _service.GetPendingPlacements();
            Assert.AreEqual(3, pending.Count,
                "Batch-building: each click adds its own pending placement");
        }

        [Test]
        public void HoverOnPendingCell_DoesNotRemoveOrReplaceIt()
        {
            _service.TryPreviewAt(PosA);

            Hover(PosA);
            Hover(PosA);

            Assert.IsTrue(
                _service.HasPendingPlacementAt(PosA),
                "Querying an occupied pending cell must not evict it");
            Assert.AreEqual(1, _service.GetPendingPlacements().Count);
        }

        [Test]
        public void HoverBlockedCell_OnlyReports_NeverMutates()
        {
            _grid.SetTileData(PosB, "lake");
            _tiles.Tags["lake"] = new HashSet<string> { "water" };

            var result = Hover(PosB);

            Assert.IsFalse(result.CanPreview);
            Assert.IsFalse(result.SpatialValid);
            Assert.AreEqual(0, _service.GetPendingPlacements().Count);
        }

        [Test]
        public void PendingView_IsReadOnlySnapshot()
        {
            _service.TryPreviewAt(PosA);
            var first = _service.GetPendingPlacements();
            _service.TryPreviewAt(PosB);

            Assert.AreEqual(
                1, first.Count,
                "A previously returned pending view is frozen");
            Assert.AreEqual(2, _service.GetPendingPlacements().Count);
        }
    }

    internal static class C09Fakes
    {
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
            private readonly Dictionary<Vector2Int, int> _levels = new();
            private readonly HashSet<Vector2Int> _water = new();
            public void Set(Vector2Int position, int level)
                => _levels[position] = level;
            public bool TryGetTerrainLevel(Vector2Int position, out int level)
                => _levels.TryGetValue(position, out level);
            public bool HasExplicitTerrainSurfaceMap => false;
            public bool TryGetTerrainSurfaceY(Vector2Int position, out float y)
            {
                y = 0f;
                return false;
            }
            public bool HasWaterMap => true;
            public bool TryGetWaterCell(Vector2Int position, out bool isWater)
            {
                isWater = _water.Contains(position);
                return true;
            }
        }

        internal sealed class Tiles : ITileSettingsService, ITerrainTagQuery
        {
            public readonly Dictionary<string, HashSet<string>> Tags = new();
            public readonly HashSet<string> BuildBlocked = new();
            public float GetTileWeight(string tileId) => 1f;
            public bool IsBuildBlocked(string tileId) => BuildBlocked.Contains(tileId);
            public float GetSurfaceOffset(string tileId) => 0f;
            public bool HasTerrainTag(string tileId, string tag)
                => Tags.TryGetValue(tileId, out var tags) && tags.Contains(tag);
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

        internal sealed class Fog : IFogOfWarService
        {
            public FogStateType DefaultState = FogStateType.Visible;
            public FogStateType GetFogState(Vector2Int position) => DefaultState;
            public bool IsVisible(Vector2Int position) => true;
            public bool IsExplored(Vector2Int position) => true;
            public string LocalPerspectiveOwnerId => null;
            public void SetLocalPerspectiveOwnerId(string ownerId) { }
            public bool IsLocalPerspectiveOwner(string ownerId) => false;
            public void Initialize(int width, int height) { }
            public void RegisterUnit(string unitId, Vector2Int position, int visionRange) { }
            public void UpdateUnitVisionRange(string unitId, int visionRange) { }
            public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape) { }
            public void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null) { }
            public void UpdateUnitPosition(string unitId, Vector2Int newPosition) { }
            public void UnregisterUnit(string unitId) { }
            public bool[,] GetExploredSnapshot() => null;
            public void LoadFromSnapshot(bool[,] explored) { }
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => Array.Empty<Vector2Int>();
        }
    }
}
