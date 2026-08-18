using System;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Owns raw gameplay fog grid state: visibility counters, explored cells, snapshots and counters.
    /// </summary>
    internal sealed class FogStateGrid
    {
        private int[,] _visibilityCounters;
        private bool[,] _exploredTiles;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool IsReady => _visibilityCounters != null && _exploredTiles != null && Width > 0 && Height > 0;

        public void Initialize(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            _visibilityCounters = new int[Width, Height];
            _exploredTiles = new bool[Width, Height];
        }

        public bool IsInBounds(Vector2Int position)
            => position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;

        public FogStateType GetState(Vector2Int position)
        {
            if (!IsReady || !IsInBounds(position))
                return FogStateType.Unexplored;

            if (_visibilityCounters[position.x, position.y] >= 1)
                return FogStateType.Visible;

            return _exploredTiles[position.x, position.y]
                ? FogStateType.Explored
                : FogStateType.Unexplored;
        }

        public bool IsVisible(Vector2Int position)
            => IsReady && IsInBounds(position) && _visibilityCounters[position.x, position.y] >= 1;

        public bool IsExplored(Vector2Int position)
            => IsReady && IsInBounds(position) && _exploredTiles[position.x, position.y];

        public void ClearVisibility()
        {
            if (_visibilityCounters != null)
                Array.Clear(_visibilityCounters, 0, _visibilityCounters.Length);
        }

        public void LoadExploredSnapshot(bool[,] explored)
        {
            if (!IsReady || explored == null)
                return;

            Array.Clear(_exploredTiles, 0, _exploredTiles.Length);

            int copyW = Mathf.Min(explored.GetLength(0), Width);
            int copyH = Mathf.Min(explored.GetLength(1), Height);
            for (int x = 0; x < copyW; x++)
                for (int y = 0; y < copyH; y++)
                    _exploredTiles[x, y] = explored[x, y];
        }

        public bool[,] GetExploredSnapshot()
        {
            if (!IsReady)
                return null;

            var snapshot = new bool[Width, Height];
            Array.Copy(_exploredTiles, snapshot, _exploredTiles.Length);

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (_visibilityCounters[x, y] > 0)
                        snapshot[x, y] = true;

            return snapshot;
        }

        public void IncrementVisible(Vector2Int tile)
        {
            if (!IsReady || !IsInBounds(tile))
                return;

            _visibilityCounters[tile.x, tile.y]++;
            _exploredTiles[tile.x, tile.y] = true;
        }

        public void DecrementVisible(Vector2Int tile)
        {
            if (!IsReady || !IsInBounds(tile))
                return;

            _visibilityCounters[tile.x, tile.y] = Mathf.Max(0, _visibilityCounters[tile.x, tile.y] - 1);
        }

        public void MarkExplored(Vector2Int tile)
        {
            if (!IsReady || !IsInBounds(tile))
                return;

            _exploredTiles[tile.x, tile.y] = true;
        }

        public int CountVisibleTiles()
        {
            if (!IsReady)
                return 0;

            int count = 0;
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (_visibilityCounters[x, y] > 0)
                    count++;

            return count;
        }

        public int CountExploredTiles()
        {
            if (!IsReady)
                return 0;

            int count = 0;
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (_exploredTiles[x, y] || _visibilityCounters[x, y] > 0)
                    count++;

            return count;
        }

        public void CountStates(out int visible, out int explored, out int unexplored)
        {
            visible = 0;
            explored = 0;
            unexplored = 0;

            if (!IsReady)
                return;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    switch (GetState(new Vector2Int(x, y)))
                    {
                        case FogStateType.Visible:
                            visible++;
                            break;
                        case FogStateType.Explored:
                            explored++;
                            break;
                        default:
                            unexplored++;
                            break;
                    }
                }
            }
        }

        public static bool[,] CloneSnapshot(bool[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var copy = new bool[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    copy[x, y] = source[x, y];

            return copy;
        }
    }
}
