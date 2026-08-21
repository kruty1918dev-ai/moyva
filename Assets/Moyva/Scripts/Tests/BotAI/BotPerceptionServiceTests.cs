using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotPerceptionServiceTests
    {
        [Test]
        public void Refresh_StartPositionCreatesPrivateVisibleAndExploredKnowledge()
        {
            var terrain = new FakeTerrain(20, 20);
            var service = new BotPerceptionService(terrain);

            service.Refresh(
                "bot-a",
                new Vector2Int(10, 10),
                new List<BotUnitSnapshot>(),
                new List<BotBuildingSnapshot>());

            Assert.That(service.IsVisible("bot-a", new Vector2Int(10, 10)), Is.True);
            Assert.That(service.IsVisible("bot-a", new Vector2Int(14, 10)), Is.True);
            Assert.That(service.IsVisible("bot-a", new Vector2Int(15, 10)), Is.False);
            Assert.That(service.IsExplored("bot-a", new Vector2Int(10, 10)), Is.True);
            Assert.That(service.IsVisible("bot-b", new Vector2Int(10, 10)), Is.False);
        }

        [Test]
        public void Refresh_MovedVisionTurnsOldVisibleIntoExploredMemory()
        {
            var terrain = new FakeTerrain(30, 30);
            var service = new BotPerceptionService(terrain);

            service.Refresh(
                "bot",
                new Vector2Int(5, 5),
                new[] { new BotUnitSnapshot("u", "bot", "scout", new Vector2Int(5, 5), 10) },
                new List<BotBuildingSnapshot>());

            service.Refresh(
                "bot",
                new Vector2Int(20, 20),
                new[] { new BotUnitSnapshot("u", "bot", "scout", new Vector2Int(20, 20), 10) },
                new List<BotBuildingSnapshot>());

            Assert.That(service.IsVisible("bot", new Vector2Int(5, 5)), Is.False);
            Assert.That(service.IsExplored("bot", new Vector2Int(5, 5)), Is.True);
            Assert.That(service.IsVisible("bot", new Vector2Int(20, 20)), Is.True);
        }

        private sealed class FakeTerrain : IBotTerrainKnowledge
        {
            public FakeTerrain(int width, int height) { Width = width; Height = height; }
            public bool IsReady => true;
            public int Width { get; }
            public int Height { get; }
            public long StartupSequence => 1;
            public bool Contains(Vector2Int cell)
                => cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;

            public bool TryGetCell(Vector2Int cell, out BotTerrainCellSnapshot snapshot)
            {
                if (!Contains(cell))
                {
                    snapshot = default;
                    return false;
                }

                snapshot = new BotTerrainCellSnapshot(cell, "grass", string.Empty, 0, 0);
                return true;
            }

            public IReadOnlyList<Vector2Int> GetNeighbors(Vector2Int cell, int radius)
            {
                var result = new List<Vector2Int>();
                for (int y = -radius; y <= radius; y++)
                for (int x = -radius; x <= radius; x++)
                {
                    if (x == 0 && y == 0) continue;
                    var p = cell + new Vector2Int(x, y);
                    if (Contains(p)) result.Add(p);
                }
                return result;
            }
        }
    }
}
