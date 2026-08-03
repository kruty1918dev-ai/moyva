using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public sealed class ConstructionBuildGridStateTests
    {
        private sealed class TestGridService : IGridService
        {
            private readonly string _tileId;

            public TestGridService(string tileId = "grass")
            {
                _tileId = tileId;
            }

            public int GridWidth => 2;
            public int GridHeight => 2;

            public string GetTileData(Vector2Int position)
                => TryGetTileData(position, out string tileId) ? tileId : null;

            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                bool exists = position.x >= 0
                    && position.y >= 0
                    && position.x < GridWidth
                    && position.y < GridHeight;
                tileTypeId = exists ? _tileId : null;
                return exists;
            }

            public void SetTileData(Vector2Int position, string tileTypeId) { }
        }

        private sealed class TestTileSettingsService :
            ITileSettingsService,
            ITerrainTagQuery
        {
            public string TaggedTileId { get; set; }
            public bool BuildBlocked { get; set; }

            public float GetTileWeight(string tileId) => 1f;
            public bool IsBuildBlocked(string tileId) => BuildBlocked;
            public float GetSurfaceOffset(string tileId) => 0f;

            public bool HasTerrainTag(string tileId, string tag)
            {
                return string.Equals(
                        tileId,
                        TaggedTileId,
                        System.StringComparison.Ordinal)
                    && string.Equals(
                        tag,
                        "water",
                        System.StringComparison.OrdinalIgnoreCase);
            }
        }

        private sealed class TestPlacementRulesProvider :
            IConstructionPlacementRulesProvider
        {
            public int MinSpacing => 0;
            public int TownHallBuildRadius => 0;
            public bool EnableInfluenceZoneRules => false;
            public bool EnableTerrainRules => true;
            public bool EnableFogRules => false;
            public bool RequireVisibleFogTile => false;
            public bool AllowBuildingOnWater { get; set; }
            public bool AllowBuildingOnHills => true;
            public bool BlockEdgeTerrainTiles => false;
            public string[] BlockedTileIds => System.Array.Empty<string>();
            public string[] AllowedTileIds => System.Array.Empty<string>();
            public Kruty1918.Moyva.WorldCreation.API.TerrainLevelRestrictionRange[]
                BlockedTerrainLevelRanges
                    => System.Array.Empty<Kruty1918.Moyva.WorldCreation.API.TerrainLevelRestrictionRange>();
        }

        private sealed class TestPlacementQuery : IConstructionPlacementQuery
        {
            public readonly HashSet<Vector2Int> ValidPositions = new();
            public readonly HashSet<Vector2Int> UnaffordablePositions = new();
            public readonly HashSet<Vector2Int> UnavailablePositions = new();
            public int CallCount { get; private set; }
            public ConstructionPlacementQueryRequest LastRequest { get; private set; }

            public ConstructionPlacementQueryResult EvaluatePlacement(ConstructionPlacementQueryRequest request)
            {
                CallCount++;
                LastRequest = request;
                bool availabilityValid = !UnavailablePositions.Contains(request.Position);
                bool spatialValid = ValidPositions.Contains(request.Position)
                    || UnaffordablePositions.Contains(request.Position);
                bool resourcesValid = !UnaffordablePositions.Contains(request.Position);
                return new ConstructionPlacementQueryResult(
                    availabilityValid,
                    spatialValid,
                    resourcesValid,
                    authorityValid: true,
                    isGateReplacement: false);
            }
        }

        [Test]
        public void Lifecycle_UsesGeneralSelectedGeneralHiddenStates()
        {
            var state = new BuildModeGridStateController();
            var query = new TestPlacementQuery();
            var filter = new ConstructionBuildGridTileFilter(new TestGridService(), query, state);
            Vector2Int valid = Vector2Int.zero;
            Vector2Int invalid = Vector2Int.right;
            query.ValidPositions.Add(valid);

            Assert.AreEqual(ConstructionBuildGridTileVisualState.Missing, filter.ResolveVisualState(valid));

            Assert.IsTrue(state.SetConstructionModeActive(true));
            Assert.AreEqual(BuildModeGridState.General, state.State);
            Assert.AreEqual(ConstructionBuildGridTileVisualState.General, filter.ResolveVisualState(valid));
            Assert.AreEqual(ConstructionBuildGridTileVisualState.General, filter.ResolveVisualState(invalid));
            Assert.AreEqual(0, query.CallCount, "General grid must not execute building placement rules.");

            Assert.IsTrue(state.SetSelection("house", isDemolishMode: false));
            Assert.AreEqual(BuildModeGridState.BuildingSelected, state.State);
            Assert.AreEqual(ConstructionBuildGridTileVisualState.Valid, filter.ResolveVisualState(valid));
            Assert.AreEqual(ConstructionBuildGridTileVisualState.Invalid, filter.ResolveVisualState(invalid));
            Assert.AreEqual(2, query.CallCount);
            Assert.IsTrue(
                query.LastRequest.IncludePendingPlacements,
                "Selected grid must use the same pending-preview rules as click validation.");

            Assert.IsTrue(state.SetSelection(null, isDemolishMode: false));
            Assert.AreEqual(BuildModeGridState.General, state.State);
            Assert.IsNull(state.SelectedBuildingId);
            Assert.AreEqual(ConstructionBuildGridTileVisualState.General, filter.ResolveVisualState(invalid));
            Assert.AreEqual(2, query.CallCount);

            Assert.IsTrue(state.SetConstructionModeActive(false));
            Assert.AreEqual(BuildModeGridState.Hidden, state.State);
            Assert.AreEqual(ConstructionBuildGridTileVisualState.Missing, filter.ResolveVisualState(valid));
        }

        [Test]
        public void Hover_IsIdempotent_AndSelectionOrExitClearsIt()
        {
            var state = new BuildModeGridStateController();
            state.SetConstructionModeActive(true);

            Assert.IsTrue(state.SetHover(Vector2Int.one, ConstructionBuildGridTileVisualState.General));
            Assert.IsFalse(state.SetHover(Vector2Int.one, ConstructionBuildGridTileVisualState.General));

            Assert.IsTrue(state.SetSelection("house", isDemolishMode: false));
            Assert.IsFalse(state.HoverPosition.HasValue);

            Assert.IsTrue(state.SetHover(Vector2Int.zero, ConstructionBuildGridTileVisualState.Valid));
            Assert.IsTrue(state.SetConstructionModeActive(false));
            Assert.IsFalse(state.HoverPosition.HasValue);
            Assert.AreEqual(ConstructionBuildGridTileVisualState.Missing, state.HoverVisualState);
        }

        [Test]
        public void SelectedGrid_DistinguishesUnavailableSpatialAndResourceFailures()
        {
            var state = new BuildModeGridStateController();
            var query = new TestPlacementQuery();
            var filter = new ConstructionBuildGridTileFilter(
                new TestGridService(),
                query,
                state);
            Vector2Int valid = Vector2Int.zero;
            Vector2Int unaffordable = Vector2Int.up;
            Vector2Int spatiallyBlocked = Vector2Int.right;
            Vector2Int unavailable = Vector2Int.one;
            query.ValidPositions.Add(valid);
            query.UnaffordablePositions.Add(unaffordable);
            query.UnavailablePositions.Add(unavailable);

            state.SetConstructionModeActive(true);
            state.SetSelection("house", isDemolishMode: false);

            Assert.AreEqual(
                ConstructionBuildGridTileVisualState.Valid,
                filter.ResolveVisualState(valid));
            Assert.AreEqual(
                ConstructionBuildGridTileVisualState.Unaffordable,
                filter.ResolveVisualState(unaffordable));
            Assert.AreEqual(
                ConstructionBuildGridTileVisualState.Invalid,
                filter.ResolveVisualState(spatiallyBlocked));
            Assert.AreEqual(
                ConstructionBuildGridTileVisualState.General,
                filter.ResolveVisualState(unavailable));
            Assert.IsTrue(filter.ShouldRenderForPlacement(unaffordable, "house"));
            Assert.IsFalse(filter.ShouldRenderForPlacement(spatiallyBlocked, "house"));
        }

        [Test]
        public void QueryResult_SeparatesSelectionPreviewAndCommitCapabilities()
        {
            var unaffordable = new ConstructionPlacementQueryResult(
                availabilityValid: true,
                spatialValid: true,
                resourcesValid: false,
                authorityValid: true,
                isGateReplacement: false,
                reason: "stone deficit");

            Assert.IsTrue(unaffordable.CanSelect);
            Assert.IsTrue(unaffordable.CanPreview);
            Assert.IsFalse(unaffordable.CanCommit);
            Assert.IsFalse(unaffordable.IsValid);

            var unavailable = new ConstructionPlacementQueryResult(
                availabilityValid: false,
                spatialValid: true,
                resourcesValid: true,
                authorityValid: true,
                isGateReplacement: false);
            Assert.IsFalse(unavailable.CanSelect);
            Assert.IsFalse(unavailable.CanPreview);
            Assert.IsFalse(unavailable.CanCommit);
        }

        [Test]
        public void TerrainBuildability_WaterTag_UsesAllowBuildingOnWater()
        {
            const string waterTileId = "lake-layer-guid";
            var grid = new TestGridService(waterTileId);
            var tileSettings = new TestTileSettingsService
            {
                TaggedTileId = waterTileId,
                BuildBlocked = true,
            };
            var rules = new TestPlacementRulesProvider
            {
                AllowBuildingOnWater = false,
            };

            Assert.IsTrue(
                ConstructionTerrainBuildabilityUtility.IsTerrainBlocked(
                    Vector2Int.zero,
                    grid,
                    null,
                    tileSettings,
                    rules,
                    out string blockedReason));
            StringAssert.Contains("water terrain", blockedReason);

            rules.AllowBuildingOnWater = true;
            Assert.IsFalse(
                ConstructionTerrainBuildabilityUtility.IsTerrainBlocked(
                    Vector2Int.zero,
                    grid,
                    null,
                    tileSettings,
                    rules,
                    out string allowedReason));
            Assert.IsNull(allowedReason);
        }

        [Test]
        public void TerrainBuildability_WaterOptIn_DoesNotBypassNonWaterBuildBlock()
        {
            const string rockTileId = "rock-layer-guid";
            var grid = new TestGridService(rockTileId);
            var tileSettings = new TestTileSettingsService
            {
                TaggedTileId = "different-water-layer",
                BuildBlocked = true,
            };
            var rules = new TestPlacementRulesProvider
            {
                AllowBuildingOnWater = true,
            };

            Assert.IsTrue(
                ConstructionTerrainBuildabilityUtility.IsTerrainBlocked(
                    Vector2Int.zero,
                    grid,
                    null,
                    tileSettings,
                    rules,
                    out string reason));
            StringAssert.Contains("build-blocked layer", reason);
        }
    }
}
