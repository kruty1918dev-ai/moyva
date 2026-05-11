using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class HeightAwareVisionService : IHeightAwareVisionService
    {
        private static readonly Vector2[] TargetSampleOffsets =
        {
            new Vector2(0.5f, 0.5f),
            new Vector2(0.15f, 0.15f),
            new Vector2(0.85f, 0.15f),
            new Vector2(0.15f, 0.85f),
            new Vector2(0.85f, 0.85f),
            new Vector2(0.5f, 0.15f),
            new Vector2(0.85f, 0.5f),
            new Vector2(0.5f, 0.85f),
            new Vector2(0.15f, 0.5f),
        };

        private readonly FogOfWarSettings _settings;
        private readonly Dictionary<VisibilityCacheKey, float> _visibilityCache = new();
        private float[,] _heightMap;
        private int _cachedSettingsSignature;

        public HeightAwareVisionService([Zenject.InjectOptional] FogOfWarSettings settings = null)
        {
            _settings = settings;
            _cachedSettingsSignature = ComputeSettingsSignature();
        }

        public void SetHeightMap(float[,] heightMap)
        {
            _heightMap = heightMap;
            _visibilityCache.Clear();
        }

        public int GetSearchRadius(Vector2Int origin, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default)
        {
            int safeBaseRange = Mathf.Max(1, baseVisionRange);
            if (_heightMap == null)
                return Mathf.Clamp(safeBaseRange, 1, maxVisionRange);

            int observerBonus = GetObserverBonus(origin);
            int downSlopeBonus = Mathf.CeilToInt(observerModifiers.EffectiveDownSlopeVisionBonus);
            return Mathf.Clamp(safeBaseRange + observerBonus + downSlopeBonus, 1, maxVisionRange);
        }

        public float GetVisibilityFactor(Vector2Int origin, Vector2Int target, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default, FogVisionModifiers targetModifiers = default)
        {
            int safeBaseRange = Mathf.Max(1, baseVisionRange);
            int safeMaxRange = Mathf.Max(safeBaseRange, maxVisionRange);
            if (origin == target)
                return 1f;

            int distance = Mathf.Max(Mathf.Abs(target.x - origin.x), Mathf.Abs(target.y - origin.y));
            if (distance == 0)
                return 1f;

            if (_heightMap == null)
                return distance <= Mathf.Min(safeBaseRange, safeMaxRange) ? 1f : 0f;

            if (!IsHeightMapInBounds(origin) || !IsHeightMapInBounds(target))
                return 0f;

            ClearVisibilityCacheIfSettingsChanged();

            var cacheKey = new VisibilityCacheKey(origin, target, safeBaseRange, safeMaxRange, observerModifiers.GetSignature(), targetModifiers.GetSignature());
            if (_visibilityCache.TryGetValue(cacheKey, out float cached))
                return cached;

            float visibility = ComputeVisibilityFactorUncached(origin, target, safeBaseRange, safeMaxRange, distance, observerModifiers, targetModifiers);
            CacheVisibility(cacheKey, visibility);
            return visibility;
        }

        public bool IsTargetVisible(Vector2Int origin, Vector2Int target, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default, FogVisionModifiers targetModifiers = default)
        {
            return GetVisibilityFactor(origin, target, baseVisionRange, maxVisionRange, observerModifiers, targetModifiers) >= GetVisibilityThreshold();
        }

        private float ComputeVisibilityFactorUncached(Vector2Int origin, Vector2Int target, int safeBaseRange, int safeMaxRange, int distance, FogVisionModifiers observerModifiers, FogVisionModifiers targetModifiers)
        {
            int effectiveRange = Mathf.Clamp(
                safeBaseRange + GetObserverBonus(origin) + GetDownhillBonus(origin, target) + GetDirectionalDownSlopeVisionBonus(origin, target, observerModifiers) - GetUphillPenalty(origin, target, observerModifiers, targetModifiers),
                0,
                safeMaxRange);

            if (distance > effectiveRange)
                return 0f;

            if (TryResolveDownhillEdgeOcclusion(origin, target, distance, out int ignoredTerrainSteps))
                return 0f;

            int sampleCount = ResolveRaySampleCount(distance, safeBaseRange, effectiveRange);
            int passed = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                Vector2 offset = TargetSampleOffsets[i];
                var targetPoint = new Vector2(target.x + offset.x, target.y + offset.y);
                if (RayHasLineOfSight(origin, targetPoint, ignoredTerrainSteps, targetModifiers))
                    passed++;
            }

            if (passed <= 0)
                return 0f;

            float coverage = passed / (float)sampleCount;
            float rangeFactor = ResolveRangeFactor(distance, safeBaseRange, effectiveRange);
            float visibility = coverage * rangeFactor;
            if (visibility < 1f)
                visibility *= GetPartialVisibilityDetectionMultiplier();

            return Mathf.Clamp01(visibility);
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

        private int GetDirectionalDownSlopeVisionBonus(Vector2Int origin, Vector2Int target, FogVisionModifiers observerModifiers)
        {
            float bonus = observerModifiers.EffectiveDownSlopeVisionBonus;
            if (bonus <= 0f)
                return 0;

            if (GetHeight(origin) - GetHeight(target) < GetTerrainEdgeHeightThreshold())
                return 0;

            if (!TryFindDownhillEdge(origin, target, out _, out int distanceToEdge))
                return 0;

            return distanceToEdge <= GetTerrainEdgePeekDistanceTiles()
                ? Mathf.CeilToInt(bonus)
                : 0;
        }

        private int GetUphillPenalty(Vector2Int origin, Vector2Int target, FogVisionModifiers observerModifiers, FogVisionModifiers targetModifiers)
        {
            float delta = GetHeight(target) - GetHeight(origin);
            if (delta <= 0f)
                return 0;

            int rawPenalty = Mathf.CeilToInt(delta / GetElevationStep()) * GetUphillVisionPenaltyPerStep();
            int penalty = Mathf.Min(rawPenalty, GetMaxUphillVisionPenalty());
            if (penalty <= 0 || !IsTerrainEdgeLineOfSightEnabled())
                return penalty;

            float edgePeekFactor = ResolveUphillEdgePeekFactor(origin, target);
            if (edgePeekFactor <= 0f)
                return penalty;

            float crestStrength = ResolveCrestVisibilityStrength(observerModifiers, targetModifiers);
            if (crestStrength <= 0f)
                return penalty;

            float reduction = GetTerrainEdgeUphillPeekStrength() * edgePeekFactor * crestStrength;
            return Mathf.Max(0, Mathf.RoundToInt(penalty * (1f - reduction)));
        }

        private static float ResolveCrestVisibilityStrength(FogVisionModifiers observerModifiers, FogVisionModifiers targetModifiers)
        {
            float observerCrestStrength = observerModifiers.EffectiveCanSeeCrest
                ? observerModifiers.EffectiveCrestVisibilityFactor
                : 0f;
            return Mathf.Clamp01(Mathf.Max(observerCrestStrength, targetModifiers.EffectiveSilhouettePenalty));
        }

        private bool HasLineOfSight(Vector2Int origin, Vector2Int target, int distance)
        {
            if (_heightMap == null)
                return true;

            float originHeight = GetHeight(origin);
            float targetHeight = GetHeight(target);
            int ignoredTerrainSteps = 0;
            if (IsTerrainEdgeLineOfSightEnabled() && originHeight - targetHeight >= GetTerrainEdgeHeightThreshold())
            {
                if (TryFindDownhillEdge(origin, target, out int downhillEdgeStep, out int distanceToEdge))
                {
                    if (IsHiddenByDownhillEdge(distance, downhillEdgeStep, distanceToEdge))
                        return false;

                    ignoredTerrainSteps = downhillEdgeStep;
                }
            }

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

                if (stepIndex <= ignoredTerrainSteps)
                    continue;

                float sampleHeight = GetHeight(current);
                float sampleSlope = (sampleHeight - originHeight) / stepIndex;
                if (sampleSlope > targetSlope + bias)
                    return false;
            }

            return true;
        }

        private bool TryResolveDownhillEdgeOcclusion(Vector2Int origin, Vector2Int target, int distance, out int ignoredTerrainSteps)
        {
            ignoredTerrainSteps = 0;
            if (!IsTerrainEdgeLineOfSightEnabled())
                return false;

            float originHeight = GetHeight(origin);
            float targetHeight = GetHeight(target);
            if (originHeight - targetHeight < GetTerrainEdgeHeightThreshold())
                return false;

            if (!TryFindDownhillEdge(origin, target, out int downhillEdgeStep, out int distanceToEdge))
                return false;

            if (IsHiddenByDownhillEdge(distance, downhillEdgeStep, distanceToEdge))
                return true;

            ignoredTerrainSteps = downhillEdgeStep;
            return false;
        }

        private bool RayHasLineOfSight(Vector2Int origin, Vector2 targetPoint, int ignoredTerrainSteps, FogVisionModifiers targetModifiers)
        {
            var originPoint = new Vector2(origin.x + 0.5f, origin.y + 0.5f);
            float deltaX = targetPoint.x - originPoint.x;
            float deltaY = targetPoint.y - originPoint.y;
            float chebyshevDistance = Mathf.Max(Mathf.Abs(deltaX), Mathf.Abs(deltaY));
            if (chebyshevDistance <= 0.0001f)
                return true;

            float rayLength = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
            int steps = Mathf.Clamp(Mathf.CeilToInt(rayLength / GetTerrainRayStepTiles()), 2, 512);
            float ignoredTerrainT = chebyshevDistance > 0f
                ? Mathf.Clamp01((ignoredTerrainSteps + 0.1f) / chebyshevDistance)
                : 0f;

            float originSightHeight = GetHeight(origin) + GetObserverEyeHeightOffset();
            float targetSightHeight = SampleTerrainHeight(targetPoint) + GetTargetSampleHeightOffset() + GetSilhouetteTargetHeightOffset(targetModifiers);
            float bias = GetOcclusionSlopeBias();

            for (int i = 1; i < steps; i++)
            {
                float t = i / (float)steps;
                if (t <= ignoredTerrainT)
                    continue;

                var samplePoint = Vector2.Lerp(originPoint, targetPoint, t);
                if (!IsHeightMapPointInBounds(samplePoint))
                    continue;

                float terrainHeight = SampleTerrainHeight(samplePoint);
                float expectedSightHeight = Mathf.Lerp(originSightHeight, targetSightHeight, t);
                if (terrainHeight > expectedSightHeight + bias)
                    return false;
            }

            return true;
        }

        private int ResolveRaySampleCount(int distance, int baseVisionRange, int effectiveRange)
        {
            int requested = Mathf.Clamp(GetTerrainRaySamplesPerTile(), 1, TargetSampleOffsets.Length);
            if (requested <= 1)
                return 1;

            float farDistance = Mathf.Max(1f, effectiveRange * GetTerrainFarSampleDistanceRatio());
            if (distance >= farDistance)
                requested = Mathf.Min(requested, 3);

            if (distance > Mathf.Max(1, baseVisionRange) * 2)
                requested = Mathf.Min(requested, 3);

            return Mathf.Clamp(requested, 1, TargetSampleOffsets.Length);
        }

        private static float ResolveRangeFactor(int distance, int baseVisionRange, int effectiveRange)
        {
            if (distance <= baseVisionRange)
                return 1f;

            if (effectiveRange <= baseVisionRange)
                return 0f;

            float bonusRange = Mathf.Max(1f, effectiveRange - baseVisionRange + 1f);
            return Mathf.Clamp01(1f - (distance - baseVisionRange - 0.25f) / bonusRange);
        }

        private void ClearVisibilityCacheIfSettingsChanged()
        {
            int signature = ComputeSettingsSignature();
            if (signature == _cachedSettingsSignature)
                return;

            _cachedSettingsSignature = signature;
            _visibilityCache.Clear();
        }

        private void CacheVisibility(VisibilityCacheKey key, float visibility)
        {
            int capacity = GetTerrainVisibilityCacheCapacity();
            if (capacity <= 0)
                return;

            if (_visibilityCache.Count >= capacity)
                _visibilityCache.Clear();

            _visibilityCache[key] = visibility;
        }

        private int ComputeSettingsSignature()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Mathf.RoundToInt(GetElevationStep() * 10000f);
                hash = hash * 31 + GetObserverHeightBonusPerStep();
                hash = hash * 31 + GetDownhillVisionBonusPerStep();
                hash = hash * 31 + GetUphillVisionPenaltyPerStep();
                hash = hash * 31 + GetMaxObserverHeightBonus();
                hash = hash * 31 + GetMaxDownhillVisionBonus();
                hash = hash * 31 + GetMaxUphillVisionPenalty();
                hash = hash * 31 + Mathf.RoundToInt(GetOcclusionSlopeBias() * 10000f);
                hash = hash * 31 + GetTerrainRaySamplesPerTile();
                hash = hash * 31 + Mathf.RoundToInt(GetVisibilityThreshold() * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(GetPartialVisibilityDetectionMultiplier() * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(GetTerrainRayStepTiles() * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(GetObserverEyeHeightOffset() * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(GetTargetSampleHeightOffset() * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(GetTerrainFarSampleDistanceRatio() * 10000f);
                hash = hash * 31 + (IsTerrainEdgeLineOfSightEnabled() ? 1 : 0);
                hash = hash * 31 + Mathf.RoundToInt(GetTerrainEdgeHeightThreshold() * 10000f);
                hash = hash * 31 + GetTerrainEdgePeekDistanceTiles();
                hash = hash * 31 + GetTerrainEdgeBlindZoneTiles();
                hash = hash * 31 + Mathf.RoundToInt(GetTerrainEdgeBlindZoneDistanceScale() * 10000f);
                hash = hash * 31 + GetTerrainEdgeMaxBlindZoneTiles();
                hash = hash * 31 + Mathf.RoundToInt(GetTerrainEdgeUphillPeekStrength() * 10000f);
                return hash;
            }
        }

        private bool TryFindDownhillEdge(Vector2Int origin, Vector2Int target, out int edgeStep, out int distanceToEdge)
        {
            edgeStep = -1;
            distanceToEdge = 0;

            float threshold = GetTerrainEdgeHeightThreshold();
            float previousHeight = GetHeight(origin);
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
                float currentHeight = GetHeight(current);
                if (previousHeight - currentHeight >= threshold)
                {
                    edgeStep = stepIndex;
                    distanceToEdge = Mathf.Max(0, stepIndex - 1);
                    return true;
                }

                previousHeight = currentHeight;
            }

            return false;
        }

        private bool IsHiddenByDownhillEdge(int distance, int edgeStep, int distanceToEdge)
        {
            int blindZone = ResolveDownhillBlindZoneTiles(distanceToEdge);
            if (blindZone <= 0)
                return false;

            int distancePastEdge = Mathf.Max(1, distance - edgeStep + 1);
            return distancePastEdge <= blindZone;
        }

        private int ResolveDownhillBlindZoneTiles(int distanceToEdge)
        {
            int peekDistance = GetTerrainEdgePeekDistanceTiles();
            if (distanceToEdge <= peekDistance)
                return 0;

            int baseBlindZone = GetTerrainEdgeBlindZoneTiles();
            int maxBlindZone = Mathf.Max(baseBlindZone, GetTerrainEdgeMaxBlindZoneTiles());
            float extraBlindZone = Mathf.Max(0, distanceToEdge - peekDistance) * GetTerrainEdgeBlindZoneDistanceScale();
            return Mathf.Clamp(Mathf.RoundToInt(baseBlindZone + extraBlindZone), 0, maxBlindZone);
        }

        private float ResolveUphillEdgePeekFactor(Vector2Int origin, Vector2Int target)
        {
            if (_heightMap == null || !IsTerrainEdgeLineOfSightEnabled())
                return 0f;

            float targetHeight = GetHeight(target);
            if (targetHeight - GetHeight(origin) < GetTerrainEdgeHeightThreshold())
                return 0f;

            int stepX = origin.x == target.x ? 0 : (origin.x > target.x ? 1 : -1);
            int stepY = origin.y == target.y ? 0 : (origin.y > target.y ? 1 : -1);
            if (stepX == 0 && stepY == 0)
                return 0f;

            int peekDistance = GetTerrainEdgePeekDistanceTiles();
            int maxSteps = Mathf.Max(1, peekDistance + 1);
            for (int step = 1; step <= maxSteps; step++)
            {
                var sample = new Vector2Int(target.x + stepX * step, target.y + stepY * step);
                if (!IsHeightMapInBounds(sample))
                    return 1f;

                float sampleHeight = GetHeight(sample);
                if (targetHeight - sampleHeight < GetTerrainEdgeHeightThreshold())
                    continue;

                int distanceToEdge = Mathf.Max(0, step - 1);
                if (distanceToEdge > peekDistance)
                    return 0f;

                return 1f - distanceToEdge / (peekDistance + 1f);
            }

            return 0f;
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

        private float SampleTerrainHeight(Vector2 point)
        {
            if (_heightMap == null)
                return 0f;

            int width = _heightMap.GetLength(0);
            int height = _heightMap.GetLength(1);
            int x = Mathf.Clamp(Mathf.FloorToInt(point.x), 0, width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(point.y), 0, height - 1);
            return _heightMap[x, y];
        }

        private bool IsHeightMapInBounds(Vector2Int position)
        {
            if (_heightMap == null)
                return false;

            int width = _heightMap.GetLength(0);
            int height = _heightMap.GetLength(1);
            return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
        }

        private bool IsHeightMapPointInBounds(Vector2 point)
        {
            if (_heightMap == null)
                return false;

            int width = _heightMap.GetLength(0);
            int height = _heightMap.GetLength(1);
            return point.x >= 0f && point.x < width && point.y >= 0f && point.y < height;
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

        private int GetTerrainRaySamplesPerTile()
            => _settings != null ? Mathf.Clamp(_settings.TerrainRaySamplesPerTile, 1, TargetSampleOffsets.Length) : 5;

        private float GetVisibilityThreshold()
            => _settings != null ? Mathf.Clamp(_settings.TerrainVisibilityThreshold, 0.01f, 1f) : 0.5f;

        private float GetPartialVisibilityDetectionMultiplier()
            => _settings != null ? Mathf.Clamp01(_settings.PartialVisibilityDetectionMultiplier) : 1f;

        private float GetTerrainRayStepTiles()
            => _settings != null ? Mathf.Clamp(_settings.TerrainRayStepTiles, 0.25f, 1f) : 0.5f;

        private float GetObserverEyeHeightOffset()
            => _settings != null ? Mathf.Max(0f, _settings.ObserverEyeHeightOffset) : 0.35f;

        private float GetTargetSampleHeightOffset()
            => _settings != null ? Mathf.Max(0f, _settings.TargetSampleHeightOffset) : 0.1f;

        private float GetSilhouetteTargetHeightOffset(FogVisionModifiers targetModifiers)
            => targetModifiers.EffectiveSilhouettePenalty * Mathf.Max(0.05f, GetObserverEyeHeightOffset());

        private float GetTerrainFarSampleDistanceRatio()
            => _settings != null ? Mathf.Clamp(_settings.TerrainFarSampleDistanceRatio, 0.1f, 1f) : 0.65f;

        private int GetTerrainVisibilityCacheCapacity()
            => _settings != null ? Mathf.Max(0, _settings.TerrainVisibilityCacheCapacity) : 24576;

        private bool IsTerrainEdgeLineOfSightEnabled()
            => _settings == null || _settings.EnableTerrainEdgeLineOfSight;

        private float GetTerrainEdgeHeightThreshold()
            => _settings != null ? Mathf.Max(0.001f, _settings.TerrainEdgeHeightThreshold) : 0.12f;

        private int GetTerrainEdgePeekDistanceTiles()
            => _settings != null ? Mathf.Max(0, _settings.TerrainEdgePeekDistanceTiles) : 1;

        private int GetTerrainEdgeBlindZoneTiles()
            => _settings != null ? Mathf.Max(0, _settings.TerrainEdgeBlindZoneTiles) : 2;

        private float GetTerrainEdgeBlindZoneDistanceScale()
            => _settings != null ? Mathf.Max(0f, _settings.TerrainEdgeBlindZoneDistanceScale) : 0.35f;

        private int GetTerrainEdgeMaxBlindZoneTiles()
            => _settings != null ? Mathf.Max(GetTerrainEdgeBlindZoneTiles(), _settings.TerrainEdgeMaxBlindZoneTiles) : 4;

        private float GetTerrainEdgeUphillPeekStrength()
            => _settings != null ? Mathf.Clamp01(_settings.TerrainEdgeUphillPeekStrength) : 0.65f;

        private readonly struct VisibilityCacheKey : IEquatable<VisibilityCacheKey>
        {
            private readonly int _originX;
            private readonly int _originY;
            private readonly int _targetX;
            private readonly int _targetY;
            private readonly int _baseRange;
            private readonly int _maxRange;
            private readonly int _observerModifiersSignature;
            private readonly int _targetModifiersSignature;

            public VisibilityCacheKey(Vector2Int origin, Vector2Int target, int baseRange, int maxRange, int observerModifiersSignature, int targetModifiersSignature)
            {
                _originX = origin.x;
                _originY = origin.y;
                _targetX = target.x;
                _targetY = target.y;
                _baseRange = baseRange;
                _maxRange = maxRange;
                _observerModifiersSignature = observerModifiersSignature;
                _targetModifiersSignature = targetModifiersSignature;
            }

            public bool Equals(VisibilityCacheKey other)
            {
                return _originX == other._originX
                    && _originY == other._originY
                    && _targetX == other._targetX
                    && _targetY == other._targetY
                    && _baseRange == other._baseRange
                    && _maxRange == other._maxRange
                    && _observerModifiersSignature == other._observerModifiersSignature
                    && _targetModifiersSignature == other._targetModifiersSignature;
            }

            public override bool Equals(object obj)
                => obj is VisibilityCacheKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + _originX;
                    hash = hash * 31 + _originY;
                    hash = hash * 31 + _targetX;
                    hash = hash * 31 + _targetY;
                    hash = hash * 31 + _baseRange;
                    hash = hash * 31 + _maxRange;
                    hash = hash * 31 + _observerModifiersSignature;
                    hash = hash * 31 + _targetModifiersSignature;
                    return hash;
                }
            }
        }
    }
}