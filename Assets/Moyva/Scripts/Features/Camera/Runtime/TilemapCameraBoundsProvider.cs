using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>
    /// Resolves camera world bounds from generated grid dimensions (tile count).
    /// This keeps camera limits deterministic and independent from scene renderers.
    /// </summary>
    internal sealed class TilemapCameraBoundsProvider : ICameraBoundsProvider
    {
        private const float RefreshIntervalSeconds = 1.5f;

        private readonly CameraSettingsSO _settings;
        private readonly IGridService _gridService;
        private readonly IGridProjection _gridProjection;
        private CameraWorldBounds _cached;
        private float _nextRefreshTime;

        public TilemapCameraBoundsProvider(
            CameraSettingsSO settings,
            [InjectOptional] IGridService gridService = null,
            [InjectOptional] IGridProjection gridProjection = null)
        {
            _settings = settings;
            _gridService = gridService;
            _gridProjection = gridProjection;
        }

        public CameraWorldBounds GetWorldBounds()
        {
            float now = Time.unscaledTime;
            if (_cached.HasValue && now < _nextRefreshTime)
                return _cached;

            if (TryResolveFromGrid(out var bounds))
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

        private bool TryResolveFromGrid(out CameraWorldBounds bounds)
        {
            if (_gridService == null)
            {
                bounds = default;
                return false;
            }

            int width = _gridService.GridWidth;
            int height = _gridService.GridHeight;
            if (width <= 0 || height <= 0)
            {
                bounds = default;
                return false;
            }

            if (_gridProjection != null)
            {
                Bounds worldBounds = _gridProjection.GetWorldBounds(width, height);
                float minPlaneY = _gridProjection.WorldPlane == GridWorldPlane.XZ ? worldBounds.min.z : worldBounds.min.y;
                float maxPlaneY = _gridProjection.WorldPlane == GridWorldPlane.XZ ? worldBounds.max.z : worldBounds.max.y;
                bounds = new CameraWorldBounds(
                    minX: worldBounds.min.x,
                    maxX: worldBounds.max.x,
                    minY: minPlaneY,
                    maxY: maxPlaneY);
                return true;
            }

            bounds = new CameraWorldBounds(
                minX: -0.5f,
                maxX: width - 0.5f,
                minY: -0.5f,
                maxY: height - 0.5f);
            return true;
        }
    }
}
