using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public class AutoTileVariantResolverTests
    {
        private AutoTileVariantResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _resolver = new AutoTileVariantResolver();
        }

        [Test]
        public void SupportedCases_ShouldContainCoreCases()
        {
            CollectionAssert.Contains(_resolver.SupportedCases, TopologyCaseType.CrossIntersection);
            CollectionAssert.Contains(_resolver.SupportedCases, TopologyCaseType.TJunctionOpenNorth);
            CollectionAssert.Contains(_resolver.SupportedCases, TopologyCaseType.CornerNorthEast);
            CollectionAssert.Contains(_resolver.SupportedCases, TopologyCaseType.VerticalLeft);
            CollectionAssert.Contains(_resolver.SupportedCases, TopologyCaseType.HorizontalBottom);
            CollectionAssert.Contains(_resolver.SupportedCases, TopologyCaseType.DiagonalNorthEastSouthWest);
        }

        [Test]
        public void TryResolveId_ShouldResolveCorner_ByAtlasPositionSemantics()
        {
            var map = new Dictionary<TopologyCaseType, string>
            {
                [TopologyCaseType.CornerNorthEast] = "corner-ne-id",
            };

            var mask = new TopologyNeighborMask(
                north: false,
                northEast: false,
                east: false,
                southEast: false,
                south: true,
                southWest: false,
                west: true,
                northWest: false);

            var ok = _resolver.TryResolveId(mask, map, out var resolvedId, out var resolvedCase);

            Assert.IsTrue(ok);
            Assert.AreEqual("corner-ne-id", resolvedId);
            Assert.AreEqual(TopologyCaseType.CornerNorthEast, resolvedCase);
        }

        [Test]
        public void TryResolveId_ShouldUseFallback_WhenPrimaryCaseMissing()
        {
            var map = new Dictionary<TopologyCaseType, string>
            {
                [TopologyCaseType.Vertical] = "vertical-id",
            };

            var mask = new TopologyNeighborMask(
                north: true,
                northEast: false,
                east: false,
                southEast: false,
                south: true,
                southWest: false,
                west: false,
                northWest: true);

            var ok = _resolver.TryResolveId(mask, map, out var resolvedId, out var resolvedCase);

            Assert.IsTrue(ok);
            Assert.AreEqual("vertical-id", resolvedId);
            Assert.AreEqual(TopologyCaseType.Vertical, resolvedCase);
        }

        [Test]
        public void TryResolveId_ShouldResolveDiagonal_WhenNoCardinalConnections()
        {
            var map = new Dictionary<TopologyCaseType, string>
            {
                [TopologyCaseType.DiagonalNorthWestSouthEast] = "diag-nw-se-id",
            };

            var mask = new TopologyNeighborMask(
                north: false,
                northEast: false,
                east: false,
                southEast: true,
                south: false,
                southWest: false,
                west: false,
                northWest: true);

            var ok = _resolver.TryResolveId(mask, map, out var resolvedId, out var resolvedCase);

            Assert.IsTrue(ok);
            Assert.AreEqual("diag-nw-se-id", resolvedId);
            Assert.AreEqual(TopologyCaseType.DiagonalNorthWestSouthEast, resolvedCase);
        }

        [Test]
        public void TryResolveId_ShouldReturnFalse_WhenNoCaseIsConfigured()
        {
            var map = new Dictionary<TopologyCaseType, string>();

            var mask = new TopologyNeighborMask(
                north: true,
                northEast: false,
                east: false,
                southEast: false,
                south: false,
                southWest: false,
                west: false,
                northWest: false);

            var ok = _resolver.TryResolveId(mask, map, out var resolvedId, out var _);

            Assert.IsFalse(ok);
            Assert.IsNull(resolvedId);
        }
    }
}
