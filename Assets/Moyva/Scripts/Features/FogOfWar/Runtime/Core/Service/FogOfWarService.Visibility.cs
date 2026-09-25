using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogOfWarService
    {
        private int ClampVisionRange(int range)
        {
            int min = _settings != null ? _settings.MinVisionRange : 1;
            int max = _settings != null ? _settings.MaxVisionRange : 12;
            return Mathf.Clamp(range, min, max);
        }

        private IReadOnlyList<Vector2Int> ComputeInitialVisibleTiles(string unitId, Vector2Int position, int range)
        {
            if (_fixedVisionShapes.TryGetValue(unitId, out var shape))
                return FilterToRevealableTiles(
                    FogRevealShapeTileCalculator.ComputeShapeTiles(position, range, shape, _width, _height));

            return FilterToRevealableTiles(
                FogRevealShapeTileCalculator.ComputePixelCircleTiles(position, range, _width, _height));
        }

        private IReadOnlyList<Vector2Int> ComputeVisibleTiles(string unitId, Vector2Int position, int range)
        {
            if (_fixedVisionShapes.TryGetValue(unitId, out var shape))
                return FilterToRevealableTiles(
                    FogRevealShapeTileCalculator.ComputeShapeTiles(position, range, shape, _width, _height));

            var modifiers = ResolveUnitVisionModifiers(unitId);
            var tiles = _resolver.ComputeVisibleTiles(position, range, _width, _height, modifiers);
            return FilterToRevealableTiles(
                AddSilhouetteTargetTiles(unitId, position, range, modifiers, tiles));
        }

        /*
         * Клітини біля межі карти ніколи не розкриваються, щоб туман
         * завжди приховував край світу. Фільтр застосовується до списків
         * тайлів перед збереженням у _unitVisibleTiles, тому add/remove
         * лишаються симетричними.
         */
        private int ResolveBoundaryMarginCells()
            => Mathf.Max(0, _settings != null ? _settings.BoundaryFogMarginCells : 0);

        private bool IsRevealableTile(Vector2Int tile)
        {
            int margin = ResolveBoundaryMarginCells();
            return tile.x >= margin
                && tile.y >= margin
                && tile.x < _width - margin
                && tile.y < _height - margin;
        }

        private IReadOnlyList<Vector2Int> FilterToRevealableTiles(IReadOnlyList<Vector2Int> tiles)
        {
            if (tiles == null || tiles.Count == 0 || ResolveBoundaryMarginCells() <= 0)
                return tiles;

            var filtered = new List<Vector2Int>(tiles.Count);
            for (int i = 0; i < tiles.Count; i++)
            {
                if (IsRevealableTile(tiles[i]))
                    filtered.Add(tiles[i]);
            }

            return filtered;
        }

        private IReadOnlyList<Vector2Int> AddSilhouetteTargetTiles(string observerUnitId, Vector2Int observerPosition, int range, FogVisionModifiers observerModifiers, IReadOnlyList<Vector2Int> sourceTiles)
        {
            if (_heightVisionService == null || _unitPositions.Count <= 1)
                return sourceTiles;

            int maxRange = _settings != null ? _settings.MaxVisionRange : 12;
            int searchRadius = _heightVisionService.GetSearchRadius(observerPosition, range, maxRange, observerModifiers);
            float threshold = _settings != null ? Mathf.Clamp(_settings.TerrainVisibilityThreshold, 0.01f, 1f) : 0.5f;
            HashSet<Vector2Int> visible = null;

            foreach (var targetEntry in _unitPositions)
            {
                if (string.Equals(targetEntry.Key, observerUnitId, StringComparison.Ordinal))
                    continue;

                var targetModifiers = ResolveUnitVisionModifiers(targetEntry.Key);
                if (targetModifiers.EffectiveSilhouettePenalty <= 0f)
                    continue;

                Vector2Int targetPosition = targetEntry.Value;
                if (!IsInBounds(targetPosition))
                    continue;

                int distance = Mathf.Max(Mathf.Abs(targetPosition.x - observerPosition.x), Mathf.Abs(targetPosition.y - observerPosition.y));
                if (distance > searchRadius)
                    continue;

                visible ??= new HashSet<Vector2Int>(sourceTiles);
                if (visible.Contains(targetPosition))
                    continue;

                float visibility = _heightVisionService.GetVisibilityFactor(observerPosition, targetPosition, range, maxRange, observerModifiers, targetModifiers);
                if (visibility >= threshold)
                    visible.Add(targetPosition);
            }

            return visible == null || visible.Count == sourceTiles.Count
                ? sourceTiles
                : new List<Vector2Int>(visible);
        }

        private FogVisionModifiers ResolveUnitVisionModifiers(string unitId)
            => !string.IsNullOrWhiteSpace(unitId) && _unitVisionModifiers.TryGetValue(unitId, out var modifiers)
                ? modifiers
                : default;
    }
}
