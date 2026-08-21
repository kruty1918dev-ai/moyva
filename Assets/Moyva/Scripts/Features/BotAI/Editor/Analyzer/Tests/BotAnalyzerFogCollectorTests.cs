using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer.Tests
{
    public sealed class BotAnalyzerFogCollectorTests
    {
        [Test]
        public void CollectFromReader_SeparatesVisibleExploredAndOmitsUnexplored()
        {
            var states = new Dictionary<Vector2Int, FogStateType>
            {
                [new Vector2Int(0, 0)] = FogStateType.Visible,
                [new Vector2Int(1, 0)] = FogStateType.Explored,
                [new Vector2Int(0, 1)] = FogStateType.Unexplored,
                [new Vector2Int(1, 1)] = FogStateType.Visible,
            };

            BotAnalyzerFogState result = BotAnalyzerFogCollector.CollectFromReader(
                new FakeGrid(2, 2),
                new FakeFog(states));

            Assert.That(result.VisibleCells, Is.EquivalentTo(new[] { new Vector2Int(0, 0), new Vector2Int(1, 1) }));
            Assert.That(result.ExploredCells, Is.EquivalentTo(new[] { new Vector2Int(1, 0) }));
            Assert.That(result.KnownCount, Is.EqualTo(3));
            Assert.That(result.TotalCells, Is.EqualTo(4));
        }

        private sealed class FakeGrid : IGridService
        {
            public FakeGrid(int width, int height) { GridWidth = width; GridHeight = height; }
            public int GridWidth { get; }
            public int GridHeight { get; }
            public string GetTileData(Vector2Int position) => "tile";
            public bool TryGetTileData(Vector2Int position, out string tileTypeId) { tileTypeId = "tile"; return true; }
            public void SetTileData(Vector2Int position, string tileTypeId) { }
        }

        private sealed class FakeFog : IFogStateReader
        {
            private readonly Dictionary<Vector2Int, FogStateType> _states;
            public FakeFog(Dictionary<Vector2Int, FogStateType> states) => _states = states;
            public FogStateType GetFogState(Vector2Int position) => _states.TryGetValue(position, out var state) ? state : FogStateType.Unexplored;
            public bool IsVisible(Vector2Int position) => GetFogState(position) == FogStateType.Visible;
            public bool IsExplored(Vector2Int position) => GetFogState(position) != FogStateType.Unexplored;
        }
    }
}
