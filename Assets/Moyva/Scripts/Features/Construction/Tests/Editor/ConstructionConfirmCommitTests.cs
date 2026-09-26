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
    /// Service-level coverage of the confirm path: a batch of pending
    /// placements is committed partially — every rejected preview keeps its
    /// pending entry, reports a user-facing reason through
    /// <see cref="ConstructionService.GetLastActionMessage"/> and marks the
    /// tile via <see cref="BuildingPreviewChangedSignal"/> Blocked instead of
    /// silently counting as placed.
    /// </summary>
    [TestFixture]
    public sealed class ConstructionConfirmCommitTests
    {
        private static readonly Vector2Int TileA = new Vector2Int(5, 5);
        private static readonly Vector2Int TileB = new Vector2Int(7, 7);

        private DiContainer _container;
        private SignalBus _signals;
        private FakeObjectsMap _objects;
        private FakeGridService _grid;
        private FakeTerrainLevelQuery _terrain;
        private FakeTileSettings _tiles;
        private FakePlacementRules _rules;
        private FakeProgressClock _clock;
        private ConstructionService _service;
        private List<BuildingPreviewChangedSignal> _previewSignals;
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

            _previewSignals = new List<BuildingPreviewChangedSignal>();
            _placedSignals = new List<BuildingPlacedSignal>();
            _signals.Subscribe<BuildingPreviewChangedSignal>(
                signal => _previewSignals.Add(signal));
            _signals.Subscribe<BuildingPlacedSignal>(
                signal => _placedSignals.Add(signal));

            _objects = new FakeObjectsMap();
            _grid = new FakeGridService();
            _terrain = new FakeTerrainLevelQuery();
            _tiles = new FakeTileSettings();
            _rules = new FakePlacementRules
            {
                EnableFogRules = false,
                EnableInfluenceZoneRules = false,
                EnableTerrainRules = true,
                AllowBuildingOnHills = true,
                BlockEdgeTerrainTiles = false,
            };
            _clock = new FakeProgressClock();

            for (var x = 0; x <= 10; x++)
            for (var y = 0; y <= 10; y++)
                _grid.SetTileData(new Vector2Int(x, y), "grass");

            _service = new ConstructionService(
                _objects,
                new FakeBuildingRegistry(),
                _signals,
                minSpacing: 0,
                townHallBuildRadius: 10,
                fogOfWarService: null,
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
                progressClock: _clock,
                fogSettings: null);

            _service.Initialize();
            _signals.Fire(new GameModeChangedSignal
            {
                NewMode = GameModeType.Construction,
            });
            _service.SetActiveOwner("p1");
            _service.SelectBuilding("house");
            Assert.AreEqual(
                BuildingPlacementState.Placing,
                _service.State,
                "Selection failed: " + _service.GetLastActionMessage());
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            _container?.UnbindAll();
        }

        [Test]
        public void Confirm_AllValid_CommitsEveryPending()
        {
            Assert.IsTrue(_service.TryPreviewAt(TileA));
            Assert.IsTrue(_service.TryPreviewAt(TileB));

            _service.Confirm();

            Assert.AreEqual(0, _service.GetPendingPlacements().Count);
            Assert.AreEqual(2, _placedSignals.Count);
            Assert.IsFalse(HasBlockedPreview());
        }

        [Test]
        public void Confirm_OneTileOccupiedBetweenPreviewAndConfirm_KeepsRejectedPending()
        {
            Assert.IsTrue(_service.TryPreviewAt(TileA));
            Assert.IsTrue(_service.TryPreviewAt(TileB));

            // P026 scenario: another action occupies a previewed cell before
            // the batch is confirmed.
            _objects.Register(TileA, "other-object");
            _previewSignals.Clear();

            _service.Confirm();

            var remaining = _service.GetPendingPlacements();
            Assert.AreEqual(1, remaining.Count);
            Assert.IsTrue(remaining.ContainsKey(TileA));
            Assert.AreEqual(1, _placedSignals.Count);
            Assert.AreEqual(TileB, _placedSignals[0].Position);

            Assert.IsTrue(HasBlockedPreviewFor(TileA));
            StringAssert.Contains(
                "occupied",
                _service.GetLastActionMessage());
            Assert.AreEqual(
                BuildingPlacementState.Placing,
                _service.State,
                "Partial commit must keep the session alive for retries.");
        }

        [Test]
        public void Confirm_TerrainRejectedBetweenPreviewAndConfirm_ReportsTerrainReason()
        {
            Assert.IsTrue(_service.TryPreviewAt(TileA));

            _rules.BlockedTileIds = new[] { "rock" };
            _grid.SetTileData(TileA, "rock");
            _previewSignals.Clear();

            _service.Confirm();

            Assert.AreEqual(1, _service.GetPendingPlacements().Count);
            Assert.AreEqual(0, _placedSignals.Count);
            Assert.IsTrue(HasBlockedPreviewFor(TileA));
            StringAssert.Contains(
                "Terrain",
                _service.GetLastActionMessage());
        }

        private bool HasBlockedPreview()
        {
            for (int index = 0; index < _previewSignals.Count; index++)
                if (_previewSignals[index].PreviewState
                    == BuildingPreviewState.Blocked)
                    return true;
            return false;
        }

        private bool HasBlockedPreviewFor(Vector2Int position)
        {
            for (int index = 0; index < _previewSignals.Count; index++)
            {
                BuildingPreviewChangedSignal signal = _previewSignals[index];
                if (signal.Position == position
                    && signal.PreviewState == BuildingPreviewState.Blocked)
                    return true;
            }
            return false;
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
                if (_occupants.ContainsKey(position))
                    throw new InvalidOperationException(
                        $"Position {position} is already occupied.");
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

        private sealed class FakeTerrainLevelQuery : IGeneratedTerrainLevelQuery
        {
            private readonly Dictionary<Vector2Int, int> _levels = new();

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
