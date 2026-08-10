using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    public sealed class BuildingFootprintRotationTests
    {
        [Test]
        public void RectangularFootprint_RotatesClockwiseAroundOrigin()
        {
            var definition = new BuildingDefinition
            {
                Footprint = new BuildingFootprint
                {
                    Size = new Vector2Int(2, 3),
                    Anchor = BuildingFootprintAnchor.SouthWest,
                },
            };
            var cells = new HashSet<Vector2Int>();

            for (int index = 0;
                 index < BuildingFootprintUtility.GetOccupiedCellCount(definition);
                 index++)
            {
                cells.Add(BuildingFootprintUtility.GetOccupiedCell(
                    definition,
                    Vector2Int.zero,
                    index,
                    ConstructionRotation.Degrees90));
            }

            CollectionAssert.AreEquivalent(
                new[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, -1),
                    new Vector2Int(1, 0),
                    new Vector2Int(1, -1),
                    new Vector2Int(2, 0),
                    new Vector2Int(2, -1),
                },
                cells);
        }

        [TestCase(-1, ConstructionRotation.Degrees270)]
        [TestCase(4, ConstructionRotation.Degrees0)]
        [TestCase(5, ConstructionRotation.Degrees90)]
        public void Normalize_WrapsQuarterTurns(
            int quarterTurns,
            ConstructionRotation expected)
            => Assert.AreEqual(
                expected,
                ConstructionRotationUtility.Normalize(quarterTurns));
    }
}
