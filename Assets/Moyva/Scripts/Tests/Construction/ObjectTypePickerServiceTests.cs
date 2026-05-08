using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public class ObjectTypePickerServiceTests
    {
        private sealed class FakeRegistry : IBuildingRegistry
        {
            public BuildingDefinition[] BuildingsSource;
            public WallCollectionDefinition Collection;

            public BuildingDefinition[] GetAll() => BuildingsSource;
            public WallCollectionDefinition[] GetWallCollections()
            {
                if (Collection == null)
                    return System.Array.Empty<WallCollectionDefinition>();

                return new[] { Collection };
            }

            public BuildingDefinition GetById(string id)
            {
                if (BuildingsSource == null)
                    return null;

                for (int i = 0; i < BuildingsSource.Length; i++)
                {
                    var b = BuildingsSource[i];
                    if (b != null && b.Id == id)
                        return b;
                }

                return null;
            }

            public BuildingDefinition[] GetByCategory(BuildingCategory category) => BuildingsSource;
            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
            {
                if (Collection == null)
                    return null;

                return Collection.ContainsBuilding(buildingId) ? Collection : null;
            }
        }

        [Test]
        public void TryPickId_ShouldReturnConfiguredCaseVariant_ByAtlasPositionSemantics()
        {
            var collection = new WallCollectionDefinition
            {
                WallBuildingId = "wall",
                GateBuildingId = "gate",
                TopologyBindings = new List<TopologyCaseBinding>
                {
                    new()
                    {
                        CaseType = TopologyCaseType.CornerNorthEast,
                        VariantBuildingIds = new List<string> { "wall_corner_ne_01" },
                    },
                },
            };

            var registry = new FakeRegistry { Collection = collection };
            var picker = new ObjectTypePickerService(registry, new AutoTileVariantResolver());

            var mask = new TopologyNeighborMask(
                north: false,
                northEast: false,
                east: false,
                southEast: false,
                south: true,
                southWest: false,
                west: true,
                northWest: false);

            var ok = picker.TryPickId("wall", mask, out var resolved);

            Assert.IsTrue(ok);
            Assert.AreEqual("wall_corner_ne_01", resolved);
        }

        [Test]
        public void TryPickId_ShouldFallbackToHorizontal_WhenCornerNotConfigured()
        {
            var collection = new WallCollectionDefinition
            {
                WallBuildingId = "wall",
                GateBuildingId = "gate",
                TopologyBindings = new List<TopologyCaseBinding>
                {
                    new()
                    {
                        CaseType = TopologyCaseType.Horizontal,
                        VariantBuildingIds = new List<string> { "wall_horizontal" },
                    },
                },
            };

            var registry = new FakeRegistry { Collection = collection };
            var picker = new ObjectTypePickerService(registry, new AutoTileVariantResolver());

            var mask = new TopologyNeighborMask(
                north: true,
                northEast: false,
                east: true,
                southEast: false,
                south: false,
                southWest: false,
                west: false,
                northWest: false);

            var ok = picker.TryPickId("wall", mask, out var resolved);

            Assert.IsTrue(ok);
            Assert.AreEqual("wall_horizontal", resolved);
        }

        [Test]
        public void TryPickId_ShouldReturnFalse_WhenCollectionHasNoBindings()
        {
            var collection = new WallCollectionDefinition
            {
                WallBuildingId = "wall",
                GateBuildingId = "gate",
                TopologyBindings = new List<TopologyCaseBinding>(),
            };

            var registry = new FakeRegistry { Collection = collection };
            var picker = new ObjectTypePickerService(registry, new AutoTileVariantResolver());

            var mask = new TopologyNeighborMask(
                north: false,
                northEast: false,
                east: false,
                southEast: false,
                south: false,
                southWest: false,
                west: false,
                northWest: false);

            var ok = picker.TryPickId("wall", mask, out var resolved);

            Assert.IsFalse(ok);
            Assert.IsNull(resolved);
        }
    }
}
