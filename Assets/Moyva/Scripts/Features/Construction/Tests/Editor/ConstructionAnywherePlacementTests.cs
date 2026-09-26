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
    /// "Build anywhere on free dry land" coverage: the canonical placement
    /// query (shared by preview and confirm) must allow every dry surface —
    /// sand, rock, hills, multi-level footprints, hidden fog, outside the
    /// castle influence — and reject only water overlap, occupied cells,
    /// out-of-bounds and resource/authority failures.
    /// </summary>
    [TestFixture]
    public sealed class ConstructionAnywherePlacementTests
    {
        private static readonly Vector2Int Pos = new Vector2Int(5, 5);

        private DiContainer _container;
        private SignalBus _signals;
        private FakeObjectsMap _objects;
        private FakeGridService _grid;
        private FakeTerrainLevelQuery _terrain;
        private FakeTileSettings _tiles;
        private FakePlacementRules _rules;
        private FakeFogService _fog;
        private ConstructionService _service;
        private List<BuildingPlacedSignal> _placedSignals;

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

            _placedSignals = new List<BuildingPlacedSignal>();
            _signals.Subscribe<BuildingPlacedSignal>(
                signal => _placedSignals.Add(signal));

            _objects = new FakeObjectsMap();
            _grid = new FakeGridService();
            _terrain = new FakeTerrainLevelQuery();
            _tiles = new FakeTileSettings();
            _fog = new FakeFogService { DefaultState = FogStateType.Unexplored };
            _rules = new FakePlacementRules
            {
                EnableFogRules = true,
                RequireVisibleFogTile = true,
                EnableInfluenceZoneRules = true,
                EnableTerrainRules = true,
                AllowBuildingOnHills = false,
                BlockEdgeTerrainTiles = true,
                AllowBuildingAnywhereExceptWater = true,
            };

            for (var x = 0; x <= 12; x++)
            for (var y = 0; y <= 12; y++)
                _grid.SetTileData(new Vector2Int(x, y), "grass");

            _service = new ConstructionService(
                _objects,
                new FakeBuildingRegistry(),
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
                progressClock: new FakeProgressClock(),
                fogSettings: null);

            _service.Initialize();
            _signals.Fire(new GameModeChangedSignal
            {
                NewMode = GameModeType.Construction,
            });
            _service.SetActiveOwner("p1");
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            _container?.UnbindAll();
        }

        private ConstructionPlacementQueryResult Query(
            string buildingId,
            Vector2Int position,
            ConstructionRotation rotation = ConstructionRotation.Degrees0)
        {
            return _service.EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    position,
                    includeDetails: true,
                    rotation: rotation));
        }

        private static void AssertSpatiallyValid(
            ConstructionPlacementQueryResult result)
        {
            Assert.IsTrue(
                result.SpatialValid,
                $"Expected dry land to allow placement, got reason '{result.Reason}' " +
                $"with blockers: {DescribeBlockers(result)}");
        }

        private static string DescribeBlockers(
            ConstructionPlacementQueryResult result)
        {
            if (result.EvaluationResult == null
                || result.EvaluationResult.Blockers.Count == 0)
            {
                return "<none>";
            }

            var parts = new List<string>();
            foreach (var blocker in result.EvaluationResult.Blockers)
                parts.Add($"{blocker.Kind}:{blocker.Message}");
            return string.Join("; ", parts);
        }

        [Test]
        public void AnywhereMode_FlatGrass_Allows()
        {
            AssertSpatiallyValid(Query("house", Pos));
        }

        [Test]
        public void AnywhereMode_SandNoBuildTile_Allows()
        {
            _grid.SetTileData(Pos, "sand");
            _tiles.Tags["sand"] = new HashSet<string> { "no-build" };

            AssertSpatiallyValid(Query("house", Pos));
        }

        [Test]
        public void AnywhereMode_RockTile_Allows()
        {
            _grid.SetTileData(Pos, "rock");
            _rules.BlockedTileIds = new[] { "rock" };

            AssertSpatiallyValid(Query("house", Pos));
        }

        [Test]
        public void AnywhereMode_HillLevels_Allows()
        {
            _terrain.Set(Pos, 3);
            AssertSpatiallyValid(Query("house", Pos));
        }

        [Test]
        public void AnywhereMode_MultiLevelFootprint_Allows()
        {
            // "hall" requires flat ground and spans 2x2; both terrain levels
            // differ across the footprint — allowed under the new policy.
            _terrain.Set(new Vector2Int(6, 5), 2);
            _terrain.Set(new Vector2Int(5, 6), 1);

            AssertSpatiallyValid(Query("hall", Pos));
        }

        [Test]
        public void AnywhereMode_UnexploredFog_Allows()
        {
            _fog.DefaultState = FogStateType.Unexplored;
            AssertSpatiallyValid(Query("house", Pos));
        }

        [Test]
        public void AnywhereMode_OutsideInfluenceRadius_Allows()
        {
            // No castle anywhere — every cell is outside the influence zone.
            AssertSpatiallyValid(Query("house", new Vector2Int(11, 11)));
        }

        [Test]
        public void AnywhereMode_WaterTaggedTile_BlocksWithWaterReason()
        {
            _grid.SetTileData(Pos, "lake");
            _tiles.Tags["lake"] = new HashSet<string> { "water" };

            var result = Query("house", Pos);
            Assert.IsFalse(result.SpatialValid);
            StringAssert.Contains("water", result.Reason);
        }

        [Test]
        public void AnywhereMode_WaterMaskOverLandTile_Blocks()
        {
            // River sheet rendered over a grass gameplay tile.
            _terrain.SetWater(Pos, true);

            var result = Query("house", Pos);
            Assert.IsFalse(result.SpatialValid, "Wet-mask river cell must block placement.");
        }

        [Test]
        public void AnywhereMode_SingleWetEdgeOfFootprint_Blocks()
        {
            _terrain.SetWater(new Vector2Int(6, 5), true);

            var result = Query("hall", Pos);
            Assert.IsFalse(result.SpatialValid);
        }

        [Test]
        public void AnywhereMode_RotatedWetCorner_Blocks()
        {
            // 90° maps offset (x,y) -> (y,-x): the rotated 2x2 footprint
            // occupies (5,5),(5,4),(6,5),(6,4); a dry check at (5,4)
            // still leaves the origin wet-free, so wet (5,4) must block.
            _terrain.SetWater(new Vector2Int(5, 4), true);

            var result = Query("hall", Pos, ConstructionRotation.Degrees90);
            Assert.IsFalse(result.SpatialValid);
        }

        [Test]
        public void AnywhereMode_DryCellNextToWater_Allows()
        {
            _terrain.SetWater(new Vector2Int(6, 5), true);

            AssertSpatiallyValid(Query("house", Pos));
        }

        [Test]
        public void AnywhereMode_OccupiedCell_Blocks()
        {
            _objects.Register(Pos, "other-object");

            var result = Query("house", Pos);
            Assert.IsFalse(result.SpatialValid);
        }

        [Test]
        public void AnywhereMode_FootprintOutsideMap_Blocks()
        {
            var result = Query("house", new Vector2Int(99, 99));
            Assert.IsFalse(result.SpatialValid);
        }

        [Test]
        public void AnywhereMode_ConfirmOnWater_KeepsPendingAndReportsBlock()
        {
            _grid.SetTileData(Pos, "lake");
            _tiles.Tags["lake"] = new HashSet<string> { "water" };
            _service.SelectBuilding("house");

            Assert.IsFalse(_service.TryPreviewAt(Pos));
        }

        [Test]
        public void AnywhereMode_ConfirmOnDryLand_Places()
        {
            _terrain.Set(Pos, 2);
            _service.SelectBuilding("house");

            Assert.IsTrue(_service.TryPreviewAt(Pos));
            _service.Confirm();

            Assert.AreEqual(1, _placedSignals.Count);
            Assert.AreEqual(Pos, _placedSignals[0].Position);
        }

        [Test]
        public void LegacyMode_SandNoBuildTile_StillBlocks()
        {
            _rules.AllowBuildingAnywhereExceptWater = false;
            _grid.SetTileData(Pos, "sand");
            _tiles.Tags["sand"] = new HashSet<string> { "no-build" };

            var result = Query("house", Pos);
            Assert.IsFalse(result.SpatialValid);
        }

        private sealed class FakeObjectsMap : IObjectsMapService
        {
            private readonly Dictionary<Vector2Int, string> _occupants = new();

            public bool IsOccupied(Vector2Int position)
                => _occupants.ContainsKey(position);

            public bool TryGetOccupant(Vector2Int position, out string occupantId)
                => _occupants.TryGetValue(position, out occupantId);

            public void Register(Vector2Int position, string occupantId)
            {
                _occupants[position] = occupantId;
            }

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
                    if (pair.Value != occupantId)
                        continue;
                    position = pair.Key;
                    return true;
                }
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
                Footprint = new BuildingFootprint
                {
                    Size = Vector2Int.one,
                    Anchor = BuildingFootprintAnchor.Center,
                },
            };

            private readonly BuildingDefinition _hall = new BuildingDefinition
            {
                Id = "hall",
                DisplayName = "Hall",
                Footprint = new BuildingFootprint
                {
                    Size = new Vector2Int(2, 2),
                    Anchor = BuildingFootprintAnchor.SouthWest,
                    RequiresFlatGround = true,
                },
            };

            public BuildingDefinition[] GetAll() => new[] { _house, _hall };

            public BuildingDefinition GetById(string id)
                => id == _house.Id ? _house
                    : id == _hall.Id ? _hall
                    : null;

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
                GameplayProgressMode mode,
                float sandboxRoundSeconds,
                float speed)
            {
            }

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

        private sealed class FakeTerrainLevelQuery
            : IGeneratedTerrainLevelQuery, IGeneratedTerrainWaterQuery
        {
            private readonly Dictionary<Vector2Int, int> _levels = new();
            private readonly HashSet<Vector2Int> _water = new();

            public void Set(Vector2Int position, int level)
                => _levels[position] = level;

            public void SetWater(Vector2Int position, bool isWater)
            {
                if (isWater)
                    _water.Add(position);
                else
                    _water.Remove(position);
            }

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

        private sealed class FakeTileSettings : ITileSettingsService, ITerrainTagQuery
        {
            public readonly Dictionary<string, HashSet<string>> Tags = new();
            public readonly HashSet<string> BuildBlocked = new();

            public float GetTileWeight(string tileId) => 1f;
            public bool IsBuildBlocked(string tileId) => BuildBlocked.Contains(tileId);
            public float GetSurfaceOffset(string tileId) => 0f;
            public bool HasTerrainTag(string tileId, string tag)
                => Tags.TryGetValue(tileId, out var tags) && tags.Contains(tag);
        }

        private sealed class FakePlacementRules : IConstructionPlacementRulesProvider
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

        private sealed class FakeFogService : IFogOfWarService
        {
            public FogStateType DefaultState = FogStateType.Visible;
            private readonly Dictionary<Vector2Int, FogStateType> _states = new();

            public FogStateType GetFogState(Vector2Int position)
                => _states.TryGetValue(position, out var s) ? s : DefaultState;

            public bool IsVisible(Vector2Int position) => GetFogState(position) == FogStateType.Visible;
            public bool IsExplored(Vector2Int position) => GetFogState(position) != FogStateType.Unexplored;

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
