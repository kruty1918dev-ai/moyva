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
        private int _visibleTileCount;
        private int _exploredFlagCount;
        private int _exploredOrVisibleCount;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool IsReady => _visibilityCounters != null && _exploredTiles != null && Width > 0 && Height > 0;

        public void Initialize(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            _visibilityCounters = new int[Width, Height];
            _exploredTiles = new bool[Width, Height];
            _visibleTileCount = 0;
            _exploredFlagCount = 0;
            _exploredOrVisibleCount = 0;
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
            if (_visibilityCounters == null)
                return;

            Array.Clear(
                _visibilityCounters,
                0,
                _visibilityCounters.Length);
            _visibleTileCount = 0;
            _exploredOrVisibleCount = _exploredFlagCount;
        }

        public void LoadExploredSnapshot(bool[,] explored)
        {
            if (!IsReady || explored == null)
                return;

            Array.Clear(
                _exploredTiles,
                0,
                _exploredTiles.Length);

            int copyW = Mathf.Min(
                explored.GetLength(0),
                Width);
            int copyH = Mathf.Min(
                explored.GetLength(1),
                Height);

            for (int x = 0; x < copyW; x++)
            {
                for (int y = 0; y < copyH; y++)
                    _exploredTiles[x, y] = explored[x, y];
            }

            RecountCachedState();
        }

        private void RecountCachedState()
        {
            _visibleTileCount = 0;
            _exploredFlagCount = 0;
            _exploredOrVisibleCount = 0;

            if (!IsReady)
                return;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    bool explored =
                        _exploredTiles[x, y];
                    bool visible =
                        _visibilityCounters[x, y] > 0;

                    if (visible)
                        _visibleTileCount++;
                    if (explored)
                        _exploredFlagCount++;
                    if (explored || visible)
                        _exploredOrVisibleCount++;
                }
            }
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

            int x = tile.x;
            int y = tile.y;
            int previousVisibility =
                _visibilityCounters[x, y];
            bool wasExplored =
                _exploredTiles[x, y];

            if (previousVisibility == 0)
            {
                _visibleTileCount++;
                if (!wasExplored)
                    _exploredOrVisibleCount++;
            }

            _visibilityCounters[x, y] =
                previousVisibility + 1;

            if (!wasExplored)
            {
                _exploredTiles[x, y] = true;
                _exploredFlagCount++;
            }
        }

        public void DecrementVisible(Vector2Int tile)
        {
            if (!IsReady || !IsInBounds(tile))
                return;

            int x = tile.x;
            int y = tile.y;
            int previousVisibility =
                _visibilityCounters[x, y];
            if (previousVisibility <= 0)
                return;

            int nextVisibility =
                previousVisibility - 1;
            _visibilityCounters[x, y] =
                nextVisibility;

            if (previousVisibility == 1)
            {
                _visibleTileCount--;
                if (!_exploredTiles[x, y])
                    _exploredOrVisibleCount--;
            }
        }

        public void MarkExplored(Vector2Int tile)
        {
            if (!IsReady || !IsInBounds(tile))
                return;

            int x = tile.x;
            int y = tile.y;
            if (_exploredTiles[x, y])
                return;

            bool alreadyCountedByVisibility =
                _visibilityCounters[x, y] > 0;

            _exploredTiles[x, y] = true;
            _exploredFlagCount++;

            if (!alreadyCountedByVisibility)
                _exploredOrVisibleCount++;
        }

        public int CountVisibleTiles()
            => IsReady ? _visibleTileCount : 0;

        public int CountExploredTiles()
            => IsReady ? _exploredOrVisibleCount : 0;

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
