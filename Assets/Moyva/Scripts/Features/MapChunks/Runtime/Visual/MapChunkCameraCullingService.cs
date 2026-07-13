using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapChunkCameraCullingService : IInitializable, ITickable
    {
        private readonly IMapChunkSettingsProvider _settings;
        private readonly IMapChunkLayoutService _layout;
        private readonly IMapChunkVisibilityService _visibility;
        private readonly UnityEngine.Camera _camera;
        private readonly HashSet<MapChunkCoord> _visible = new();
        private readonly Plane[] _planes = new Plane[6];
        private float _nextUpdateAt;

        public MapChunkCameraCullingService(
            IMapChunkSettingsProvider settings,
            IMapChunkLayoutService layout,
            IMapChunkVisibilityService visibility,
            [InjectOptional] UnityEngine.Camera camera = null)
        {
            _settings = settings;
            _layout = layout;
            _visibility = visibility;
            _camera = camera;
        }

        public void Initialize()
        {
            _nextUpdateAt = 0f;
        }

        public void Tick()
        {
            if (!_settings.EnableCameraCulling || !_layout.IsConfigured)
                return;

            if (Time.unscaledTime < _nextUpdateAt)
                return;

            var camera = ResolveCamera();
            if (camera == null)
                return;

            GeometryUtility.CalculateFrustumPlanes(camera, _planes);
            _visible.Clear();
            foreach (var chunk in _layout.Chunks)
            {
                if (GeometryUtility.TestPlanesAABB(_planes, Expand(chunk.WorldBounds)))
                    _visible.Add(chunk.Coord);
            }

            _visibility.SetCameraVisible(_visible);
            _nextUpdateAt = Time.unscaledTime + _settings.CameraCullingIntervalSeconds;
        }

        private UnityEngine.Camera ResolveCamera()
            => _camera != null ? _camera : UnityEngine.Camera.main;

        private Bounds Expand(Bounds bounds)
        {
            float padding = _settings.CameraCullingPaddingCells * Mathf.Max(0.0001f, _layout.CellSize);
            bounds.Expand(new Vector3(padding, padding, padding));
            return bounds;
        }
    }
}
