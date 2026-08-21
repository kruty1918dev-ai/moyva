using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotCastleSiteEvaluatorTests
    {
        [Test]
        public void HigherDefensibleSiteScoresAboveLowerOpenSite()
        {
            var terrain = new FakeTerrain(30, 30);
            terrain.Set(new Vector2Int(10, 10), 3, 3f, "grass");
            terrain.Set(new Vector2Int(15, 15), 0, 0f, "grass");

            var evaluator = new BotCastleSiteEvaluator(terrain);
            BotWorldSnapshot snapshot = Snapshot(new Vector2Int(10, 10));

            BotSiteEvaluation high = evaluator.Evaluate(
                snapshot,
                "castle",
                new Vector2Int(10, 10),
                true);

            BotSiteEvaluation low = evaluator.Evaluate(
                snapshot,
                "castle",
                new Vector2Int(15, 15),
                true);

            Assert.That(high.PlacementAllowed, Is.True);
            Assert.That(high.Factors.Count, Is.GreaterThan(4));
            Assert.That(high.TotalScore, Is.GreaterThan(low.TotalScore));
        }

        [Test]
        public void CanonicalPlacementRejectionAlwaysWins()
        {
            var evaluator = new BotCastleSiteEvaluator(new FakeTerrain(10, 10));

            BotSiteEvaluation result = evaluator.Evaluate(
                Snapshot(Vector2Int.zero),
                "castle",
                new Vector2Int(3, 3),
                false);

            Assert.That(result.PlacementAllowed, Is.False);
            Assert.That(result.TotalScore, Is.LessThan(-1000));
        }

        private static BotWorldSnapshot Snapshot(Vector2Int start)
            => new(
                "bot",
                1,
                2,
                TurnPhase.AwaitingInput,
                0,
                start,
                new List<BotUnitSnapshot>(),
                new List<BotUnitSnapshot>(),
                new List<BotBuildingSnapshot>(),
                null,
                null,
                new List<BotBuildingSnapshot>());

        private sealed class FakeTerrain : IBotTerrainKnowledge
        {
            private readonly Dictionary<Vector2Int, BotTerrainCellSnapshot> _custom = new();

            public FakeTerrain(int width, int height)
            {
                Width = width;
                Height = height;
            }

            public bool IsReady => true;
            public int Width { get; }
            public int Height { get; }
            public long StartupSequence => 1;

            public void Set(Vector2Int cell, int level, float height, string tile)
                => _custom[cell] = new BotTerrainCellSnapshot(
                    cell,
                    tile,
                    string.Empty,
                    height,
                    level);

            public bool Contains(Vector2Int cell)
                => cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;

            public bool TryGetCell(Vector2Int cell, out BotTerrainCellSnapshot snapshot)
            {
                if (!Contains(cell))
                {
                    snapshot = default;
                    return false;
                }

                if (_custom.TryGetValue(cell, out snapshot))
                    return true;

                snapshot = new BotTerrainCellSnapshot(
                    cell,
                    "grass",
                    string.Empty,
                    0f,
                    0);
                return true;
            }

            public IReadOnlyList<Vector2Int> GetNeighbors(Vector2Int cell, int radius)
            {
                var result = new List<Vector2Int>();
                for (int y = -radius; y <= radius; y++)
                for (int x = -radius; x <= radius; x++)
                {
                    if (x == 0 && y == 0) continue;
                    Vector2Int p = cell + new Vector2Int(x, y);
                    if (Contains(p)) result.Add(p);
                }
                return result;
            }
        }
    }
}
