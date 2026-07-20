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
                tileTypeId = exists ? "grass" : null;
                return exists;
            }

            public void SetTileData(Vector2Int position, string tileTypeId) { }
        }

        private sealed class TestPlacementQuery : IConstructionPlacementQuery
        {
            public readonly HashSet<Vector2Int> ValidPositions = new();
            public int CallCount { get; private set; }
            public ConstructionPlacementQueryRequest LastRequest { get; private set; }

            public ConstructionPlacementQueryResult EvaluatePlacement(ConstructionPlacementQueryRequest request)
            {
                CallCount++;
                LastRequest = request;
                bool valid = ValidPositions.Contains(request.Position);
                return new ConstructionPlacementQueryResult(valid, true, false);
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
    }
}
