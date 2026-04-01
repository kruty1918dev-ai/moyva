using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using Kruty1918.Moyva.Visibility.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Visibility.Runtime
{
    /// <summary>
    /// Реалізація туману війни на основі спільного двовимірного масиву лічильників.
    ///
    /// Логіка:
    ///  - _grid[x, y] = скільки юнітів зараз бачать цей тайл.
    ///  - Юніт створено  → +1 для всіх тайлів у радіусі зору.
    ///  - Юніт зробив крок → −1 для старих тайлів, +1 для нових.
    ///  - Юніт знищено  → −1 для всіх тайлів у радіусі зору.
    ///  - Перехід 0→1 або 1→0 оновлює піксель текстури та надсилає OnVisibilityChangedSignal.
    /// </summary>
    internal sealed class VisibilityService : IVisibilityService, IInitializable, IDisposable
    {
        private readonly IGridService _gridService;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly SignalBus _signalBus;

        private int[,] _grid;
        private Texture2D _texture;

        private readonly Dictionary<string, Vector2Int> _unitPositions = new();
        private readonly Dictionary<string, string> _unitTypeIds = new();

        public VisibilityService(
            IGridService gridService,
            IUnitClassConfig unitClassConfig,
            SignalBus signalBus)
        {
            _gridService = gridService;
            _unitClassConfig = unitClassConfig;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            int w = _gridService.GridWidth;
            int h = _gridService.GridHeight;

            _grid = new int[w, h];

            _texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            _texture.filterMode = FilterMode.Point;

            var black = new Color[w * h];
            for (int i = 0; i < black.Length; i++)
                black[i] = Color.black;

            _texture.SetPixels(black);
            _texture.Apply();

            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Unsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Unsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        // ── Signal handlers ──────────────────────────────────────────────────

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            _unitTypeIds[signal.UnitId] = signal.UnitTypeId;
            _unitPositions[signal.UnitId] = signal.Position;

            int radius = GetVisionRadius(signal.UnitTypeId);
            AddVisibility(signal.Position, radius);
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (!_unitPositions.TryGetValue(signal.UnitId, out var oldPos)) return;
            if (!_unitTypeIds.TryGetValue(signal.UnitId, out var typeId)) return;

            int radius = GetVisionRadius(typeId);
            RemoveVisibility(oldPos, radius);
            AddVisibility(signal.NewPosition, radius);
            _unitPositions[signal.UnitId] = signal.NewPosition;
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (!_unitPositions.TryGetValue(signal.UnitId, out var pos)) return;

            _unitTypeIds.TryGetValue(signal.UnitId, out var typeId);
            int radius = GetVisionRadius(typeId);
            RemoveVisibility(pos, radius);

            _unitPositions.Remove(signal.UnitId);
            _unitTypeIds.Remove(signal.UnitId);
        }

        // ── Public API ───────────────────────────────────────────────────────

        public bool IsVisible(Vector2Int position)
        {
            if (!IsValidPosition(position)) return false;
            return _grid[position.x, position.y] > 0;
        }

        public int GetVisibilityCount(Vector2Int position)
        {
            if (!IsValidPosition(position)) return 0;
            return _grid[position.x, position.y];
        }

        public Texture2D GetVisibilityTexture() => _texture;

        public int[,] GetRawGrid()
        {
            int w = _gridService.GridWidth;
            int h = _gridService.GridHeight;
            var copy = new int[w, h];
            Array.Copy(_grid, copy, _grid.Length);
            return copy;
        }

        public void LoadFromGrid(int[,] grid)
        {
            int w = _gridService.GridWidth;
            int h = _gridService.GridHeight;
            Array.Copy(grid, _grid, Math.Min(grid.Length, _grid.Length));

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                _texture.SetPixel(x, y, _grid[x, y] > 0 ? Color.white : Color.black);

            _texture.Apply();
        }

        // ── Internal helpers ─────────────────────────────────────────────────

        private void AddVisibility(Vector2Int center, int radius)
        {
            bool textureChanged = false;
            foreach (var pos in GetTilesInRadius(center, radius))
            {
                int prev = _grid[pos.x, pos.y];
                _grid[pos.x, pos.y]++;
                if (prev == 0)
                {
                    _texture.SetPixel(pos.x, pos.y, Color.white);
                    textureChanged = true;
                    _signalBus.Fire(new OnVisibilityChangedSignal { Position = pos, IsVisible = true });
                }
            }
            if (textureChanged) _texture.Apply();
        }

        private void RemoveVisibility(Vector2Int center, int radius)
        {
            bool textureChanged = false;
            foreach (var pos in GetTilesInRadius(center, radius))
            {
                if (_grid[pos.x, pos.y] <= 0) continue;
                _grid[pos.x, pos.y]--;
                if (_grid[pos.x, pos.y] == 0)
                {
                    _texture.SetPixel(pos.x, pos.y, Color.black);
                    textureChanged = true;
                    _signalBus.Fire(new OnVisibilityChangedSignal { Position = pos, IsVisible = false });
                }
            }
            if (textureChanged) _texture.Apply();
        }

        private IEnumerable<Vector2Int> GetTilesInRadius(Vector2Int center, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                var pos = new Vector2Int(center.x + dx, center.y + dy);
                if (IsValidPosition(pos))
                    yield return pos;
            }
        }

        private bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < _gridService.GridWidth &&
                   position.y >= 0 && position.y < _gridService.GridHeight;
        }

        private int GetVisionRadius(string typeId)
        {
            UnitClassConfig config = _unitClassConfig.GetConfig(typeId);
            return config?.VisionRadius ?? 3;
        }
    }
}
