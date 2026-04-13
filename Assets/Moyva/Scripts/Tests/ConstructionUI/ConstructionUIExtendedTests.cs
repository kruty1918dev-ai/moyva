using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.ConstructionUI
{
    // ====================================================================
    // BuildingPreviewStateEnumTests — 3 tests
    // ====================================================================
    [TestFixture]
    public sealed class BuildingPreviewStateEnumTests
    {
        [Test]
        public void None_IsZero()
            => Assert.AreEqual(0, (int)BuildingPreviewState.None);

        [Test]
        public void Valid_IsOne()
            => Assert.AreEqual(1, (int)BuildingPreviewState.Valid);

        [Test]
        public void Blocked_IsTwo()
            => Assert.AreEqual(2, (int)BuildingPreviewState.Blocked);
    }

    // ====================================================================
    // ConstructionSignalTests — 12 tests
    // ====================================================================
    [TestFixture]
    public sealed class ConstructionSignalTests
    {
        [Test]
        public void BuildingPlacedSignal_SetsAllFields()
        {
            var s = new BuildingPlacedSignal
            {
                BuildingId = "house",
                Position = new Vector2Int(3, 4),
                OwnerId = "f1",
                SourceFactionId = "f1"
            };
            Assert.AreEqual("house", s.BuildingId);
            Assert.AreEqual(new Vector2Int(3, 4), s.Position);
            Assert.AreEqual("f1", s.OwnerId);
            Assert.AreEqual("f1", s.SourceFactionId);
        }

        [Test]
        public void BuildingCancelledSignal_Default()
        {
            var s = new BuildingCancelledSignal();
            Assert.IsNotNull(s);
        }

        [Test]
        public void BuildingDemolishedSignal_SetsPosition()
        {
            var s = new BuildingDemolishedSignal
            {
                Position = new Vector2Int(1, 2),
                BuildingId = "wall"
            };
            Assert.AreEqual(new Vector2Int(1, 2), s.Position);
            Assert.AreEqual("wall", s.BuildingId);
        }

        [Test]
        public void BuildingPreviewChangedSignal_SetsPreviewState()
        {
            var s = new BuildingPreviewChangedSignal
            {
                PreviewState = BuildingPreviewState.Valid,
                Position = new Vector2Int(5, 5),
                BuildingId = "tavern"
            };
            Assert.AreEqual(BuildingPreviewState.Valid, s.PreviewState);
            Assert.AreEqual("tavern", s.BuildingId);
        }

        [Test]
        public void BuildingPreviewChangedSignal_BlockedState()
        {
            var s = new BuildingPreviewChangedSignal
            {
                PreviewState = BuildingPreviewState.Blocked
            };
            Assert.AreEqual(BuildingPreviewState.Blocked, s.PreviewState);
        }

        [Test]
        public void BuildingPreviewChangedSignal_NoneState()
        {
            var s = new BuildingPreviewChangedSignal
            {
                PreviewState = BuildingPreviewState.None
            };
            Assert.AreEqual(BuildingPreviewState.None, s.PreviewState);
        }

        [Test]
        public void ShowWallHandlesSignal_SetsCenter()
        {
            var s = new ShowWallHandlesSignal
            {
                Center = new Vector2Int(10, 10)
            };
            Assert.AreEqual(new Vector2Int(10, 10), s.Center);
        }

        [Test]
        public void BuildingPlacedSignal_NullOwnerId_Allowed()
        {
            var s = new BuildingPlacedSignal { OwnerId = null };
            Assert.IsNull(s.OwnerId);
        }

        [Test]
        public void BuildingDemolishedSignal_NullBuildingId()
        {
            var s = new BuildingDemolishedSignal { BuildingId = null };
            Assert.IsNull(s.BuildingId);
        }

        [Test]
        public void BuildingPlacedSignal_DefaultPosition_IsZero()
        {
            var s = new BuildingPlacedSignal();
            Assert.AreEqual(Vector2Int.zero, s.Position);
        }

        [Test]
        public void BuildingPreviewChangedSignal_DefaultPosition_IsZero()
        {
            var s = new BuildingPreviewChangedSignal();
            Assert.AreEqual(Vector2Int.zero, s.Position);
        }

        [Test]
        public void ShowWallHandlesSignal_DefaultCenter_IsZero()
        {
            var s = new ShowWallHandlesSignal();
            Assert.AreEqual(Vector2Int.zero, s.Center);
        }
    }
}
