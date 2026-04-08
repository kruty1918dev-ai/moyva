using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.UI;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.ConstructionUI
{
    /// <summary>
    /// Юніт-тести для <see cref="ConstructionUIState"/>.
    /// Перевіряє властивості snapshot-у UI-стану (чистий C#, без MonoBehaviour).
    /// </summary>
    [TestFixture]
    public class ConstructionUIStateTests
    {
        [Test]
        public void IsPlacing_ShouldBeTrue_WhenStatePlacing()
        {
            var state = new ConstructionUIState(
                BuildingPlacementState.Placing, "barracks",
                BuildingPreviewState.None, Vector2Int.zero);

            Assert.IsTrue(state.IsPlacing);
        }

        [Test]
        public void IsPlacing_ShouldBeFalse_WhenStateIdle()
        {
            var state = new ConstructionUIState(
                BuildingPlacementState.Idle, null,
                BuildingPreviewState.None, Vector2Int.zero);

            Assert.IsFalse(state.IsPlacing);
        }

        [Test]
        public void HasSelection_ShouldBeTrue_WhenBuildingSelected()
        {
            var state = new ConstructionUIState(
                BuildingPlacementState.Placing, "tower",
                BuildingPreviewState.None, Vector2Int.zero);

            Assert.IsTrue(state.HasSelection);
        }

        [Test]
        public void HasSelection_ShouldBeFalse_WhenNoBuildingSelected()
        {
            var state = new ConstructionUIState(
                BuildingPlacementState.Idle, null,
                BuildingPreviewState.None, Vector2Int.zero);

            Assert.IsFalse(state.HasSelection);
        }

        [Test]
        public void HasSelection_ShouldBeFalse_WhenEmptyString()
        {
            var state = new ConstructionUIState(
                BuildingPlacementState.Placing, string.Empty,
                BuildingPreviewState.None, Vector2Int.zero);

            Assert.IsFalse(state.HasSelection);
        }

        [Test]
        public void IsDemolishMode_ShouldBeFalse_ByDefault()
        {
            var state = new ConstructionUIState(
                BuildingPlacementState.Idle, null,
                BuildingPreviewState.None, Vector2Int.zero);

            Assert.IsFalse(state.IsDemolishMode);
        }

        [Test]
        public void IsDemolishMode_ShouldBeTrue_WhenSetInConstructor()
        {
            var state = new ConstructionUIState(
                BuildingPlacementState.Idle, null,
                BuildingPreviewState.None, Vector2Int.zero,
                isDemolishMode: true);

            Assert.IsTrue(state.IsDemolishMode);
        }

        [Test]
        public void IsConstructionModeActive_ShouldBeTrue_WhenSetInConstructor()
        {
            var state = new ConstructionUIState(
                BuildingPlacementState.Idle, null,
                BuildingPreviewState.None, Vector2Int.zero,
                isDemolishMode: false, isConstructionModeActive: true);

            Assert.IsTrue(state.IsConstructionModeActive);
        }

        [Test]
        public void Default_ShouldReturnIdleStateWithNoSelection()
        {
            var state = ConstructionUIState.Default;

            Assert.AreEqual(BuildingPlacementState.Idle, state.PlacementState);
            Assert.IsFalse(state.HasSelection);
            Assert.IsFalse(state.IsPlacing);
            Assert.AreEqual(BuildingPreviewState.None, state.LastPreviewState);
            Assert.AreEqual(Vector2Int.zero, state.LastPreviewPosition);
            Assert.IsFalse(state.IsDemolishMode);
            Assert.IsFalse(state.IsConstructionModeActive);
        }

        [Test]
        public void LastPreviewState_ShouldMatchGivenValue()
        {
            var state = new ConstructionUIState(
                BuildingPlacementState.Placing, "barracks",
                BuildingPreviewState.Valid, new Vector2Int(3, 4));

            Assert.AreEqual(BuildingPreviewState.Valid, state.LastPreviewState);
            Assert.AreEqual(new Vector2Int(3, 4), state.LastPreviewPosition);
        }
    }
}
