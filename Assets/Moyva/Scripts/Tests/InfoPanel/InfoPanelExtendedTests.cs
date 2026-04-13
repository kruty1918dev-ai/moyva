using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.InfoPanel
{
    // ====================================================================
    // WorldInfoSelectionKindEnumTests — 3 tests
    // ====================================================================
    [TestFixture]
    public sealed class WorldInfoSelectionKindEnumTests
    {
        [Test]
        public void None_IsZero()
            => Assert.AreEqual(0, (int)WorldInfoSelectionKind.None);

        [Test]
        public void Unit_IsTwo()
            => Assert.AreEqual(2, (int)WorldInfoSelectionKind.Unit);

        [Test]
        public void Building_IsOne()
            => Assert.AreEqual(1, (int)WorldInfoSelectionKind.Building);
    }

    // ====================================================================
    // InfoPanelSignalTests — 12 tests
    // ====================================================================
    [TestFixture]
    public sealed class InfoPanelSignalTests
    {
        [Test]
        public void WorldInfoPanelRequested_SetsProperties()
        {
            var s = new WorldInfoPanelRequestedSignal
            {
                Title = "Village",
                Subtitle = "East",
                Content = "A small village"
            };
            Assert.AreEqual("Village", s.Title);
            Assert.AreEqual("A small village", s.Content);
        }

        [Test]
        public void WorldInfoPanelClosed_IsDefaultConstructable()
        {
            var s = new WorldInfoPanelClosedSignal();
            Assert.IsNotNull(s);
        }

        [Test]
        public void BuildingInfoPanelRequested_SetsBuildingId()
        {
            var s = new BuildingInfoPanelRequestedSignal
            {
                BuildingId = "house_01",
                Position = new Vector2Int(3, 3)
            };
            Assert.AreEqual("house_01", s.BuildingId);
            Assert.AreEqual(new Vector2Int(3, 3), s.Position);
        }

        [Test]
        public void UnitInfoPanelRequested_SetsUnitId()
        {
            var s = new UnitInfoPanelRequestedSignal
            {
                UnitId = "warrior_01",
                Position = new Vector2Int(7, 7)
            };
            Assert.AreEqual("warrior_01", s.UnitId);
        }

        [Test]
        public void WorldInfoSelectionChanged_SetsKindAndId()
        {
            var s = new WorldInfoSelectionChangedSignal
            {
                Kind = WorldInfoSelectionKind.Unit,
                ObjectId = "u1"
            };
            Assert.AreEqual(WorldInfoSelectionKind.Unit, s.Kind);
            Assert.AreEqual("u1", s.ObjectId);
        }

        [Test]
        public void WorldInfoSelectionChanged_None_HasNullId()
        {
            var s = new WorldInfoSelectionChangedSignal
            {
                Kind = WorldInfoSelectionKind.None,
                ObjectId = null
            };
            Assert.AreEqual(WorldInfoSelectionKind.None, s.Kind);
            Assert.IsNull(s.ObjectId);
        }

        [Test]
        public void TileClickedSignal_SetsPosition()
        {
            var s = new TileClickedSignal
            {
                Position = new Vector2Int(10, 10)
            };
            Assert.AreEqual(new Vector2Int(10, 10), s.Position);
        }

        [Test]
        public void TileClickedSignal_DefaultPos_IsZero()
        {
            var s = new TileClickedSignal();
            Assert.AreEqual(Vector2Int.zero, s.Position);
        }

        [Test]
        public void WorldInfoSelectionChanged_Building_Works()
        {
            var s = new WorldInfoSelectionChangedSignal
            {
                Kind = WorldInfoSelectionKind.Building,
                ObjectId = "b1"
            };
            Assert.AreEqual(WorldInfoSelectionKind.Building, s.Kind);
        }

        [Test]
        public void WorldInfoPanelRequested_NullFields_Allowed()
        {
            var s = new WorldInfoPanelRequestedSignal
            {
                Title = null,
                Content = null
            };
            Assert.IsNull(s.Title);
            Assert.IsNull(s.Content);
        }

        [Test]
        public void BuildingInfoPanelRequested_Default()
        {
            var s = new BuildingInfoPanelRequestedSignal();
            Assert.IsNull(s.BuildingId);
            Assert.AreEqual(Vector2Int.zero, s.Position);
        }

        [Test]
        public void UnitInfoPanelRequested_Default()
        {
            var s = new UnitInfoPanelRequestedSignal();
            Assert.IsNull(s.UnitId);
        }
    }
}
