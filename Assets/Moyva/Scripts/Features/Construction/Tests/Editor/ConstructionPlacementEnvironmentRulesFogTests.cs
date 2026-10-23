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
    /// Owner-aware fog checks: authoritative placement for a remote owner must
    /// read that owner's per-owner fog grid, not the local perspective grid.
    /// </summary>
    public class ConstructionPlacementEnvironmentRulesFogTests
    {
        private static readonly Vector2Int Pos = new Vector2Int(4, 4);

        private FakePlacementRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = new FakePlacementRules();
        }

        [Test]
        public void RemoteOwner_UsesOwnerGrid_WhenLocalPerspectiveDiffers()
        {
            var fog = new FakeOwnerFogService
            {
                LocalOwner = "host",
                LocalState = FogStateType.Unexplored,
            };
            fog.OwnerStates["client"] = FogStateType.Visible;

            var rules = CreateRules(fog);

            Assert.IsFalse(
                rules.IsBlockedByFog("client", Pos),
                "Host must validate the client's request against the client's per-owner fog grid.");
        }

        [Test]
        public void RemoteOwner_Blocked_WhenOwnerGridUnexplored()
        {
            var fog = new FakeOwnerFogService
            {
                LocalOwner = "host",
                LocalState = FogStateType.Visible,
            };
            fog.OwnerStates["client"] = FogStateType.Unexplored;

            var rules = CreateRules(fog);

            Assert.IsTrue(rules.IsBlockedByFog("client", Pos));
        }

        [Test]
        public void LocalOwner_UsesLocalGrid()
        {
            var fog = new FakeOwnerFogService
            {
                LocalOwner = "host",
                LocalState = FogStateType.Visible,
            };
            fog.OwnerStates["host"] = FogStateType.Unexplored;

            var rules = CreateRules(fog);

            Assert.IsFalse(
                rules.IsBlockedByFog("host", Pos),
                "The local perspective owner reads the local grid even when its owner grid disagrees.");
        }

        [Test]
        public void UnknownOwner_Blocked_WhenOwnerGridHasNoState()
        {
            var fog = new FakeOwnerFogService
            {
                LocalOwner = "host",
                LocalState = FogStateType.Visible,
            };

            var rules = CreateRules(fog);

            // Owner grid returns Unexplored for unknown owners; authoritative
            // validation must fail closed rather than read the local grid.
            Assert.IsTrue(rules.IsBlockedByFog("ghost", Pos));
        }

        [Test]
        public void GetFogState_ReturnsOwnerState_ForRemoteOwner()
        {
            var fog = new FakeOwnerFogService
            {
                LocalOwner = "host",
                LocalState = FogStateType.Visible,
            };
            fog.OwnerStates["client"] = FogStateType.Explored;

            var rules = CreateRules(fog);

            Assert.AreEqual(
                FogStateType.Explored,
                rules.GetFogState("client", Pos));
        }

        [Test]
        public void NonOwnerReader_FallsBackToLocalGrid()
        {
            var fog = new FakeLocalOnlyFogService
            {
                LocalState = FogStateType.Visible,
            };

            var rules = CreateRules(fog);

            Assert.IsFalse(rules.IsBlockedByFog("client", Pos));
        }

        private ConstructionPlacementEnvironmentRules CreateRules(IFogOfWarService fog)
            => new ConstructionPlacementEnvironmentRules(
                fog,
                gridService: null,
                generatedTerrainLevelQuery: null,
                tileSettings: null,
                placementRulesProvider: _rules);

        private sealed class FakePlacementRules : IConstructionPlacementRulesProvider
        {
            public int MinSpacing { get; set; } = 1;
            public int TownHallBuildRadius { get; set; } = 10;
            public bool EnableInfluenceZoneRules { get; set; } = true;
            public bool EnableTerrainRules { get; set; } = true;
            public bool EnableFogRules { get; set; } = true;
            public bool RequireVisibleFogTile { get; set; } = true;
            public bool AllowBuildingOnWater { get; set; }
            public bool AllowBuildingAnywhereExceptWater { get; set; }
            public bool AllowBuildingOnHills { get; set; } = true;
            public bool BlockEdgeTerrainTiles { get; set; } = true;
            public string[] BlockedTileIds { get; set; }
            public string[] AllowedTileIds { get; set; }
            public TerrainLevelRestrictionRange[] BlockedTerrainLevelRanges { get; set; }
        }

        private abstract class FakeFogServiceBase : IFogOfWarService
        {
            public string LocalOwner;
            public FogStateType LocalState = FogStateType.Visible;

            public virtual FogStateType GetFogState(Vector2Int position) => LocalState;
            public bool IsVisible(Vector2Int position) => GetFogState(position) == FogStateType.Visible;
            public bool IsExplored(Vector2Int position) => GetFogState(position) != FogStateType.Unexplored;

            public string LocalPerspectiveOwnerId => LocalOwner;
            public void SetLocalPerspectiveOwnerId(string ownerId) { }
            public bool IsLocalPerspectiveOwner(string ownerId)
                => string.Equals(ownerId, LocalOwner, StringComparison.Ordinal);

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

        private sealed class FakeOwnerFogService : FakeFogServiceBase, IFogOwnerStateReader
        {
            public readonly Dictionary<string, FogStateType> OwnerStates = new();

            public FogStateType GetFogState(string ownerId, Vector2Int position)
                => OwnerStates.TryGetValue(ownerId, out var state)
                    ? state
                    : FogStateType.Unexplored;

            public bool IsVisible(string ownerId, Vector2Int position)
                => GetFogState(ownerId, position) == FogStateType.Visible;

            public bool IsExplored(string ownerId, Vector2Int position)
                => GetFogState(ownerId, position) != FogStateType.Unexplored;
        }

        private sealed class FakeLocalOnlyFogService : FakeFogServiceBase
        {
        }
    }
}
