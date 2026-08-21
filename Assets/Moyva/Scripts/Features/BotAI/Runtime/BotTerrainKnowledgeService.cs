using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotTerrainKnowledgeService :
        IBotTerrainKnowledge,
        IInitializable,
        IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IWorldGenerationSignalState _worldState;

        private WorldGeneratedDataSignal _snapshot;
        private bool _ready;

        [Inject]
        public BotTerrainKnowledgeService(
            SignalBus signalBus,
            [InjectOptional] IWorldGenerationSignalState worldState = null)
        {
            _signalBus = signalBus;
            _worldState = worldState;
        }

        public bool IsReady => _ready;
        public int Width => _ready ? Math.Max(0, _snapshot.Width) : 0;
        public int Height => _ready ? Math.Max(0, _snapshot.Height) : 0;
        public long StartupSequence => _ready ? _snapshot.StartupSequence : 0;

        public void Initialize()
        {
            _signalBus?.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);

            if (_worldState != null &&
                _worldState.TryGetWorldGeneratedData(out WorldGeneratedDataSignal cached))
            {
                OnWorldGenerated(cached);
            }
        }

        public void Dispose()
        {
            _signalBus?.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        public bool Contains(Vector2Int cell)
            => _ready &&
               cell.x >= 0 &&
               cell.y >= 0 &&
               cell.x < Width &&
               cell.y < Height;

        public bool TryGetCell(
            Vector2Int cell,
            out BotTerrainCellSnapshot snapshot)
        {
            if (!Contains(cell))
            {
                snapshot = default;
                return false;
            }

            string tileId = Read(_snapshot.TileMap, cell);
            string objectId = Read(_snapshot.ObjectMap, cell);
            float height = Read(_snapshot.HeightMap, cell);
            int terrainLevel = Read(_snapshot.TerrainLevelMap, cell);

            snapshot = new BotTerrainCellSnapshot(
                cell,
                tileId,
                objectId,
                height,
                terrainLevel);
            return true;
        }

        public IReadOnlyList<Vector2Int> GetNeighbors(
            Vector2Int cell,
            int radius)
        {
            radius = Math.Max(1, radius);
            var result = new List<Vector2Int>();

            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    var candidate = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (Contains(candidate))
                        result.Add(candidate);
                }
            }

            result.Sort(CompareCell);
            return result;
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            if (signal.Width <= 0 || signal.Height <= 0)
                return;

            _snapshot = signal;
            _ready = true;
        }

        private static string Read(string[,] map, Vector2Int cell)
        {
            if (map == null ||
                cell.x < 0 ||
                cell.y < 0 ||
                cell.x >= map.GetLength(0) ||
                cell.y >= map.GetLength(1))
            {
                return string.Empty;
            }

            return map[cell.x, cell.y] ?? string.Empty;
        }

        private static float Read(float[,] map, Vector2Int cell)
        {
            if (map == null ||
                cell.x < 0 ||
                cell.y < 0 ||
                cell.x >= map.GetLength(0) ||
                cell.y >= map.GetLength(1))
            {
                return 0f;
            }

            return map[cell.x, cell.y];
        }

        private static int Read(int[,] map, Vector2Int cell)
        {
            if (map == null ||
                cell.x < 0 ||
                cell.y < 0 ||
                cell.x >= map.GetLength(0) ||
                cell.y >= map.GetLength(1))
            {
                return 0;
            }

            return map[cell.x, cell.y];
        }

        private static int CompareCell(Vector2Int left, Vector2Int right)
        {
            int x = left.x.CompareTo(right.x);
            return x != 0 ? x : left.y.CompareTo(right.y);
        }
    }
}
