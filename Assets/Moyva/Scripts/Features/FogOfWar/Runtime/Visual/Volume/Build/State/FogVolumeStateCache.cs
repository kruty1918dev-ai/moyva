using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogVolumeStateCache : IFogVolumeStateCache
    {
        private readonly HashSet<Vector2> _unexploredCells = new HashSet<Vector2>();
        private readonly HashSet<Vector2> _exploredCells = new HashSet<Vector2>();
        private readonly Dictionary<int, HashSet<Vector2>> _unexploredCellsByHeight = new Dictionary<int, HashSet<Vector2>>();
        private readonly Dictionary<int, HashSet<Vector2>> _exploredCellsByHeight = new Dictionary<int, HashSet<Vector2>>();
        private string _runtimeLayerSignature;
        private int _mapWidth = 1;
        private int _mapHeight = 1;

        public int UnexploredCellCount => _unexploredCells.Count;
        public int ExploredCellCount => _exploredCells.Count;
        public string RuntimeLayerSignature => _runtimeLayerSignature;
        public bool RuntimeLayerSignatureChanged { get; private set; }
        public IReadOnlyCollection<Vector2> UnexploredCells => _unexploredCells;
        public IReadOnlyCollection<Vector2> ExploredCells => _exploredCells;
        public IReadOnlyDictionary<int, HashSet<Vector2>> UnexploredCellsByHeight => _unexploredCellsByHeight;
        public IReadOnlyDictionary<int, HashSet<Vector2>> ExploredCellsByHeight => _exploredCellsByHeight;

        public void InitializeMapSize(int width, int height)
        {
            _mapWidth = Mathf.Max(1, width);
            _mapHeight = Mathf.Max(1, height);
        }

        public void Rebuild(IFogOfWarService fogService, Func<Vector2Int, int> resolveHeightKey, bool unexploredEnabled, bool exploredEnabled)
        {
            _unexploredCells.Clear();
            _exploredCells.Clear();
            ClearHeightCaches();

            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                    AddStateCell(new Vector2Int(x, y), fogService.GetFogState(new Vector2Int(x, y)), resolveHeightKey, unexploredEnabled, exploredEnabled);
            }

            UpdateRuntimeLayerSignature();
        }

        public void ApplyDirty(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles, Func<Vector2Int, int> resolveHeightKey, bool unexploredEnabled, bool exploredEnabled)
        {
            foreach (var tile in dirtyTiles)
            {
                var cell = ToCell(tile);
                _unexploredCells.Remove(cell);
                _exploredCells.Remove(cell);
                RemoveCellFromHeightCaches(cell);
                AddStateCell(tile, fogService.GetFogState(tile), resolveHeightKey, unexploredEnabled, exploredEnabled);
            }

            UpdateRuntimeLayerSignature();
        }

        public void Clear()
        {
            _unexploredCells.Clear();
            _exploredCells.Clear();
            ClearHeightCaches();
            RuntimeLayerSignatureChanged = false;
            _runtimeLayerSignature = null;
        }

        public bool HasUnexploredCell(Vector2Int tile) => _unexploredCells.Contains(ToCell(tile));
        public bool HasExploredCell(Vector2Int tile) => _exploredCells.Contains(ToCell(tile));
        public HashSet<Vector2> ResolveUnexploredCells(int heightKey) => _unexploredCellsByHeight.TryGetValue(heightKey, out var cells) ? cells : null;
        public HashSet<Vector2> ResolveExploredCells(int heightKey) => _exploredCellsByHeight.TryGetValue(heightKey, out var cells) ? cells : null;

        public int CountNonEmptyHeightLayers(IReadOnlyDictionary<int, HashSet<Vector2>> cellsByHeight)
        {
            if (cellsByHeight == null)
                return 0;

            int count = 0;
            foreach (var pair in cellsByHeight)
            {
                if (pair.Value != null && pair.Value.Count > 0)
                    count++;
            }

            return count;
        }

        private void AddStateCell(Vector2Int tile, FogStateType state, Func<Vector2Int, int> resolveHeightKey, bool unexploredEnabled, bool exploredEnabled)
        {
            if (!IsInBounds(tile))
                return;

            switch (state)
            {
                case FogStateType.Unexplored:
                    if (unexploredEnabled)
                    {
                        _unexploredCells.Add(ToCell(tile));
                        AddHeightCell(_unexploredCellsByHeight, tile, resolveHeightKey);
                    }
                    break;
                case FogStateType.Explored:
                    if (exploredEnabled)
                    {
                        _exploredCells.Add(ToCell(tile));
                        AddHeightCell(_exploredCellsByHeight, tile, resolveHeightKey);
                    }
                    break;
            }
        }

        private static void AddHeightCell(Dictionary<int, HashSet<Vector2>> stateCellsByHeight, Vector2Int tile, Func<Vector2Int, int> resolveHeightKey)
        {
            int heightKey = resolveHeightKey(tile);
            if (!stateCellsByHeight.TryGetValue(heightKey, out var cells))
            {
                cells = new HashSet<Vector2>();
                stateCellsByHeight.Add(heightKey, cells);
            }

            cells.Add(ToCell(tile));
        }

        private void RemoveCellFromHeightCaches(Vector2 cell)
        {
            RemoveCellFromHeightCache(_unexploredCellsByHeight, cell);
            RemoveCellFromHeightCache(_exploredCellsByHeight, cell);
        }

        private static void RemoveCellFromHeightCache(Dictionary<int, HashSet<Vector2>> cache, Vector2 cell)
        {
            foreach (var cells in cache.Values)
                cells.Remove(cell);
        }

        private void ClearHeightCaches()
        {
            _unexploredCellsByHeight.Clear();
            _exploredCellsByHeight.Clear();
        }

        private void UpdateRuntimeLayerSignature()
        {
            string signature = BuildRuntimeLayerSignature();
            RuntimeLayerSignatureChanged = !string.Equals(signature, _runtimeLayerSignature, StringComparison.Ordinal);
            _runtimeLayerSignature = signature;
        }

        private string BuildRuntimeLayerSignature()
        {
            var parts = new List<string>();
            AppendLayerSignature(parts, "U", _unexploredCellsByHeight);
            AppendLayerSignature(parts, "E", _exploredCellsByHeight);
            parts.Sort(StringComparer.Ordinal);
            return string.Join("|", parts);
        }

        private static void AppendLayerSignature(List<string> parts, string prefix, Dictionary<int, HashSet<Vector2>> cellsByHeight)
        {
            foreach (var pair in cellsByHeight)
            {
                if (pair.Value != null && pair.Value.Count > 0)
                    parts.Add($"{prefix}{pair.Key}");
            }
        }

        private bool IsInBounds(Vector2Int tile)
            => tile.x >= 0 && tile.x < _mapWidth && tile.y >= 0 && tile.y < _mapHeight;

        private static Vector2 ToCell(Vector2Int tile)
            => new Vector2(tile.x, tile.y);
    }
}
