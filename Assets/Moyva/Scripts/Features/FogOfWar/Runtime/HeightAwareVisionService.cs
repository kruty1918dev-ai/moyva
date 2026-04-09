using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class HeightAwareVisionService : IHeightAwareVisionService
    {
        private readonly FogOfWarSettings _settings;
        private float[,] _heightMap;

        public HeightAwareVisionService([Zenject.InjectOptional] FogOfWarSettings settings = null)
        {
            _settings = settings;
        }

        public void SetHeightMap(float[,] heightMap)
        {
            _heightMap = heightMap;
        }

        public int GetSearchRadius(Vector2Int origin, int baseVisionRange, int maxVisionRange)
        {
            int safeBaseRange = Mathf.Max(1, baseVisionRange);
            if (_heightMap == null)
                return Mathf.Clamp(safeBaseRange, 1, maxVisionRange);

            int observerBonus = GetObserverBonus(origin);
            return Mathf.Clamp(safeBaseRange + observerBonus, 1, maxVisionRange);
        }

        public bool IsTargetVisible(Vector2Int origin, Vector2Int target, int baseVisionRange, int maxVisionRange)
        {
            int safeBaseRange = Mathf.Max(1, baseVisionRange);
            if (origin == target)
                return true;

            int distance = Mathf.Max(Mathf.Abs(target.x - origin.x), Mathf.Abs(target.y - origin.y));
            if (distance == 0)
                return true;

            int effectiveRange = Mathf.Clamp(
                safeBaseRange + GetObserverBonus(origin) + GetDownhillBonus(origin, target) - GetUphillPenalty(origin, target),
                0,
                maxVisionRange);

            if (distance > effectiveRange)
                return false;

            return HasLineOfSight(origin, target, distance);
        }

        private int GetObserverBonus(Vector2Int origin)
        {
            float observerHeight = GetHeight(origin);
            int rawBonus = Mathf.FloorToInt(observerHeight / GetElevationStep()) * GetObserverHeightBonusPerStep();
            return Mathf.Min(rawBonus, GetMaxObserverHeightBonus());
        }

        private int GetDownhillBonus(Vector2Int origin, Vector2Int target)
        {
            float delta = GetHeight(origin) - GetHeight(target);
            if (delta <= 0f)
                return 0;

            int rawBonus = Mathf.CeilToInt(delta / GetElevationStep()) * GetDownhillVisionBonusPerStep();
            return Mathf.Min(rawBonus, GetMaxDownhillVisionBonus());
        }

        private int GetUphillPenalty(Vector2Int origin, Vector2Int target)
        {
            float delta = GetHeight(target) - GetHeight(origin);
            if (delta <= 0f)
                return 0;

            int rawPenalty = Mathf.CeilToInt(delta / GetElevationStep()) * GetUphillVisionPenaltyPerStep();
            return Mathf.Min(rawPenalty, GetMaxUphillVisionPenalty());
        }

        private bool HasLineOfSight(Vector2Int origin, Vector2Int target, int distance)
        {
            if (_heightMap == null)
                return true;

            float originHeight = GetHeight(origin);
            float targetHeight = GetHeight(target);
            float targetSlope = (targetHeight - originHeight) / distance;
            float bias = GetOcclusionSlopeBias();

            var current = origin;
            int dx = Mathf.Abs(target.x - origin.x);
            int dy = Mathf.Abs(target.y - origin.y);
            int sx = origin.x < target.x ? 1 : -1;
            int sy = origin.y < target.y ? 1 : -1;
            int error = dx - dy;
            int stepIndex = 0;

            while (current != target)
            {
                int twiceError = error * 2;
                if (twiceError > -dy)
                {
                    error -= dy;
                    current.x += sx;
                }

                if (twiceError < dx)
                {
                    error += dx;
                    current.y += sy;
                }

                stepIndex++;

                if (current == target)
                    break;

                if (stepIndex <= 0)
                    continue;

                float sampleHeight = GetHeight(current);
                float sampleSlope = (sampleHeight - originHeight) / stepIndex;
                if (sampleSlope > targetSlope + bias)
                    return false;
            }

            return true;
        }

        private float GetHeight(Vector2Int position)
        {
            if (_heightMap == null)
                return 0f;

            int width = _heightMap.GetLength(0);
            int height = _heightMap.GetLength(1);
            if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
                return 0f;

            return _heightMap[position.x, position.y];
        }

        private float GetElevationStep()
            => _settings != null ? Mathf.Max(0.01f, _settings.ElevationStep) : 0.15f;

        private int GetObserverHeightBonusPerStep()
            => _settings != null ? _settings.ObserverHeightBonusPerStep : 1;

        private int GetDownhillVisionBonusPerStep()
            => _settings != null ? _settings.DownhillVisionBonusPerStep : 1;

        private int GetUphillVisionPenaltyPerStep()
            => _settings != null ? _settings.UphillVisionPenaltyPerStep : 1;

        private int GetMaxObserverHeightBonus()
            => _settings != null ? _settings.MaxObserverHeightBonus : 4;

        private int GetMaxDownhillVisionBonus()
            => _settings != null ? _settings.MaxDownhillVisionBonus : 2;

        private int GetMaxUphillVisionPenalty()
            => _settings != null ? _settings.MaxUphillVisionPenalty : 6;

        private float GetOcclusionSlopeBias()
            => _settings != null ? _settings.OcclusionSlopeBias : 0.02f;
    }
}