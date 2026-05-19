using Kruty1918.Moyva.Camera.API;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>
    /// Resolves camera world bounds by aggregating <see cref="TilemapRenderer"/>
    /// bounds in the active scene. Results are cached and refreshed at most once
    /// per <see cref="RefreshIntervalSeconds"/> seconds to avoid scanning every frame.
    /// </summary>
    internal sealed class TilemapCameraBoundsProvider : ICameraBoundsProvider
    {
        private const float RefreshIntervalSeconds = 1.5f;

        private readonly CameraSettingsSO _settings;
        private CameraWorldBounds _cached;
        private float _nextRefreshTime;

        public TilemapCameraBoundsProvider(CameraSettingsSO settings)
        {
            _settings = settings;
        }

        public CameraWorldBounds GetWorldBounds()
        {
            float now = Time.unscaledTime;
            if (_cached.HasValue && now < _nextRefreshTime)
                return _cached;

            if (TryResolveFromTilemaps(out var bounds))
            {
                _cached = bounds;
                _nextRefreshTime = now + RefreshIntervalSeconds;
                return _cached;
            }

            if (_settings != null && _settings.manualMapMaskSize.sqrMagnitude > 0.0001f)
            {
                Vector2 halfSize = _settings.manualMapMaskSize * 0.5f;
                _cached = new CameraWorldBounds(
                    _settings.manualMapMaskCenter.x - halfSize.x,
                    _settings.manualMapMaskCenter.x + halfSize.x,
                    _settings.manualMapMaskCenter.y - halfSize.y,
                    _settings.manualMapMaskCenter.y + halfSize.y);
                _nextRefreshTime = now + RefreshIntervalSeconds;
                return _cached;
            }

            _cached = default;
            _nextRefreshTime = now + 0.25f;
            return _cached;
        }

        private bool TryResolveFromTilemaps(out CameraWorldBounds bounds)
        {
            var renderers = UnityEngine.Object.FindObjectsByType<TilemapRenderer>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            bool hasAny = false;
            float minX = 0f, maxX = 0f, minY = 0f, maxY = 0f;

            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || !renderer.gameObject.scene.IsValid())
                    continue;
                if (!IsLayerAllowed(renderer.gameObject.layer))
                    continue;

                Bounds wb = renderer.bounds;
                if (!hasAny)
                {
                    minX = wb.min.x; maxX = wb.max.x;
                    minY = wb.min.y; maxY = wb.max.y;
                    hasAny = true;
                    continue;
                }

                if (wb.min.x < minX) minX = wb.min.x;
                if (wb.max.x > maxX) maxX = wb.max.x;
                if (wb.min.y < minY) minY = wb.min.y;
                if (wb.max.y > maxY) maxY = wb.max.y;
            }

            if (!hasAny)
            {
                bounds = default;
                return false;
            }

            bounds = new CameraWorldBounds(minX, maxX, minY, maxY);
            return true;
        }

        private bool IsLayerAllowed(int layer)
        {
            if (_settings == null) return true;
            int bit = 1 << layer;
            return (_settings.mapMaskLayers.value & bit) != 0;
        }
    }
}
