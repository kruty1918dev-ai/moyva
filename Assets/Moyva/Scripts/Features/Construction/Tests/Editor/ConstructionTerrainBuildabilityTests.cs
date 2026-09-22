using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.WorldCreation.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    /// <summary>
    /// Focused rules coverage for placement buildability: terrain level,
    /// blocked/allowed tile ids, water handling, edge tiles and fog gating.
    /// ConstructionTerrainBuildabilityUtility is a static rules component —
    /// these tests pin the exact reason strings and bypass combinations.
    /// </summary>
    public class ConstructionTerrainBuildabilityTests
    {
        private static readonly Vector2Int Pos = new Vector2Int(5, 5);

        private FakeGridService _grid;
        private FakeTerrainLevelQuery _terrain;
        private FakeTileSettings _tiles;
        private FakePlacementRules _rules;

        [SetUp]
        public void SetUp()
        {
            _grid = new FakeGridService();
            _terrain = new FakeTerrainLevelQuery();
            _tiles = new FakeTileSettings();
            _rules = new FakePlacementRules();

            // Default world: 3x3 flat grass around Pos, no levels.
            for (var x = 4; x <= 6; x++)
            for (var y = 4; y <= 6; y++)
                _grid.SetTileData(new Vector2Int(x, y), "grass");
        }

        private bool IsBlocked(Vector2Int pos, out string reason)
            => ConstructionTerrainBuildabilityUtility.IsTerrainBlocked(
                pos, _grid, _terrain, _tiles, _rules, out reason);

        private bool IsBlocked(Vector2Int pos)
            => IsBlocked(pos, out _);

        [Test]
        public void TerrainRulesDisabled_NeverBlocks()
        {
            _rules.EnableTerrainRules = false;
            _grid.SetTileData(Pos, "void");
            _terrain.Set(Pos, 3);
            _rules.AllowBuildingOnHills = false;

            Assert.IsFalse(IsBlocked(Pos));
        }

        [Test]
        public void HillLevel_WhenHillsDisabled_BlocksWithReason()
        {
            _terrain.Set(Pos, 2);
            _rules.AllowBuildingOnHills = false;

            Assert.IsTrue(IsBlocked(Pos, out var reason));
            StringAssert.Contains("hill", reason);
        }

        [Test]
        public void HillLevel_InBlockedRange_BlocksWithReason()
        {
            _terrain.Set(Pos, 3);
            _rules.BlockedTerrainLevelRanges = new[]
            {
                new TerrainLevelRestrictionRange { MinLevel = 2, MaxLevel = 4 }
            };

            Assert.IsTrue(IsBlocked(Pos, out var reason));
            StringAssert.Contains("terrain level 3", reason);
        }

        [Test]
        public void HillLevel_OutsideBlockedRange_DoesNotBlock()
        {
            // Flat plateau — same level on all cells so this is not an edge tile.
            for (var x = 4; x <= 6; x++)
            for (var y = 4; y <= 6; y++)
                _terrain.Set(new Vector2Int(x, y), 1);
            _rules.BlockedTerrainLevelRanges = new[]
            {
                new TerrainLevelRestrictionRange { MinLevel = 3, MaxLevel = 5 }
            };

            Assert.IsFalse(IsBlocked(Pos));
        }

        [Test]
        public void OutsideGrid_BlocksWithReason()
        {
            Assert.IsTrue(IsBlocked(new Vector2Int(99, 99), out var reason));
            StringAssert.Contains("outside generated grid", reason);
        }

        [Test]
        public void BlockedTileId_BlocksWithReason()
        {
            _grid.SetTileData(Pos, "rock");
            _rules.BlockedTileIds = new[] { "rock" };

            Assert.IsTrue(IsBlocked(Pos, out var reason));
            StringAssert.Contains("rock", reason);
        }

        [Test]
        public void TileNotInAllowedList_BlocksWithReason()
        {
            _rules.AllowedTileIds = new[] { "meadow" };

            Assert.IsTrue(IsBlocked(Pos, out var reason));
            StringAssert.Contains("not in allowed build list", reason);
        }

        [Test]
        public void WaterTile_WhenWaterDisallowed_Blocks()
        {
            _grid.SetTileData(Pos, "lake");
            _tiles.Tags["lake"] = new HashSet<string> { "water" };
            _rules.AllowBuildingOnWater = false;

            Assert.IsTrue(IsBlocked(Pos, out var reason));
            StringAssert.Contains("water", reason);
        }

        [Test]
        public void WaterTile_WhenWaterAllowed_DoesNotBlock()
        {
            _grid.SetTileData(Pos, "lake");
            _tiles.Tags["lake"] = new HashSet<string> { "water" };
            _rules.AllowBuildingOnWater = true;

            Assert.IsFalse(IsBlocked(Pos));
        }

        [Test]
        public void WaterTile_WhenAllowed_BypassesBuildBlockedLayer()
        {
            // Water profiles are build-blocked by default; the explicit opt-in
            // must win for water only.
            _grid.SetTileData(Pos, "lake");
            _tiles.Tags["lake"] = new HashSet<string> { "water" };
            _tiles.BuildBlocked.Add("lake");
            _rules.AllowBuildingOnWater = true;

            Assert.IsFalse(IsBlocked(Pos));
        }

        [Test]
        public void BuildBlockedLayer_NonWater_BlocksWithReason()
        {
            _grid.SetTileData(Pos, "cliff");
            _tiles.BuildBlocked.Add("cliff");

            Assert.IsTrue(IsBlocked(Pos, out var reason));
            StringAssert.Contains("build-blocked layer", reason);
        }

        [Test]
        public void EdgeTerrainTile_WhenEdgeBlocked_Blocks()
        {
            _terrain.Set(Pos, 0);
            _terrain.Set(new Vector2Int(6, 5), 1); // east neighbor higher

            Assert.IsTrue(IsBlocked(Pos, out var reason));
            StringAssert.Contains("edge terrain tile", reason);
        }

        [Test]
        public void EdgeTerrainTile_WhenEdgeRuleDisabled_DoesNotBlock()
        {
            _terrain.Set(new Vector2Int(6, 5), 1);
            _rules.BlockEdgeTerrainTiles = false;

            Assert.IsFalse(IsBlocked(Pos));
        }

        [Test]
        public void FlatGrassTile_NotBlocked()
        {
            Assert.IsFalse(IsBlocked(Pos));
        }

        [Test]
        public void FogDisabled_NeverBlocks()
        {
            var fog = new FakeFogService { DefaultState = FogStateType.Unexplored };
            _rules.EnableFogRules = false;
            var rules = Rules(fog);

            Assert.IsFalse(rules.IsBlockedByFog(Pos));
        }

        [Test]
        public void FogServiceNull_NeverBlocks()
        {
            var rules = Rules(null);
            Assert.IsFalse(rules.IsBlockedByFog(Pos));
        }

        [Test]
        public void VisibleTile_NotBlockedByFog()
        {
            var fog = new FakeFogService { DefaultState = FogStateType.Visible };
            Assert.IsFalse(Rules(fog).IsBlockedByFog(Pos));
        }

        [Test]
        public void UnexploredTile_BlockedByFog()
        {
            var fog = new FakeFogService { DefaultState = FogStateType.Unexplored };
            Assert.IsTrue(Rules(fog).IsBlockedByFog(Pos));
        }

        [Test]
        public void ExploredTile_BlockedByFog()
        {
            var fog = new FakeFogService { DefaultState = FogStateType.Explored };
            Assert.IsTrue(Rules(fog).IsBlockedByFog(Pos));
        }

        [Test]
        public void FogRulesDisabledViaRequireVisibleFlag_NeverBlocks()
        {
            var fog = new FakeFogService { DefaultState = FogStateType.Unexplored };
            _rules.RequireVisibleFogTile = false;
            Assert.IsFalse(Rules(fog).IsBlockedByFog(Pos));
        }

        private ConstructionPlacementEnvironmentRules Rules(IFogOfWarService fog)
            => new ConstructionPlacementEnvironmentRules(
                fog, _grid, _terrain, _tiles, _rules);

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

            public void Set(Vector2Int position, int level) => _levels[position] = level;

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
            public int MinSpacing { get; set; } = 1;
            public int TownHallBuildRadius { get; set; } = 10;
            public bool EnableInfluenceZoneRules { get; set; } = true;
            public bool EnableTerrainRules { get; set; } = true;
            public bool EnableFogRules { get; set; } = true;
            public bool RequireVisibleFogTile { get; set; } = true;
            public bool AllowBuildingOnWater { get; set; }
            public bool AllowBuildingOnHills { get; set; } = true;
            public bool BlockEdgeTerrainTiles { get; set; } = true;
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
