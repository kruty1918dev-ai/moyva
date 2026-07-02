using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionCameraService
    {
        void TeleportMainCamera(Vector2Int startPos, WorldGeneratedDataSignal signal);
    }

    internal sealed class StartingPositionCameraService
        : IStartingPositionCameraService
    {
        private readonly ICameraMovement _cameraMovement;
        private readonly ICameraZoom _cameraZoom;
        private readonly IGridProjection _gridProjection;
        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _cameraSettings;
        private readonly MoyvaProjectSettingsSO _projectSettings;
        private readonly StartingPositionInitializerSettings _settings;

        public StartingPositionCameraService(
            ICameraMovement cameraMovement,
            ICameraZoom cameraZoom,
            IGridProjection gridProjection,
            UnityEngine.Camera camera,
            CameraSettingsSO cameraSettings,
            MoyvaProjectSettingsSO projectSettings,
            StartingPositionInitializerSettings settings)
        {
            _cameraMovement = cameraMovement;
            _cameraZoom = cameraZoom;
            _gridProjection = gridProjection;
            _camera = camera;
            _cameraSettings = cameraSettings;
            _projectSettings = projectSettings;
            _settings = settings;
        }

        public void TeleportMainCamera(Vector2Int startPos, WorldGeneratedDataSignal signal)
        {
            if (TryTeleportCameraToStartupFocus(startPos, signal))
                return;

            _cameraMovement.TeleportCamera(new Vector3(startPos.x, startPos.y, _settings.cameraZ));
        }

        public bool TryTeleportCameraToStartupFocus(Vector2Int startPos, WorldGeneratedDataSignal signal)
        {
            if (_cameraMovement == null)
                return false;

            ApplyConfiguredStartupCameraPose();
            Vector3 focusPoint = ResolveStartupFocusPoint(startPos, signal);
            float distance = ResolveStartupCameraDistance();
            _cameraMovement.TeleportCameraToFocusPoint(focusPoint, distance);
            ApplyStartupCameraZoom(startPos, focusPoint, signal);
            return true;
        }

        public void ApplyConfiguredStartupCameraPose()
        {
            if (_camera == null)
                return;

            _camera.transform.rotation = Quaternion.Euler(ResolveStartupCameraEuler());
            bool usePerspective = ResolveUsePerspectiveStartupCamera();
            _camera.orthographic = !usePerspective;

            if (_camera.orthographic)
                _camera.orthographicSize = ResolveStartupOrthographicSize();
            else
                _camera.fieldOfView = ResolveStartupFieldOfView();
        }

        public Vector3 ResolveStartupCameraEuler()
        {
            GridProjectionMode projectionMode = ResolveProjectionMode();
            if (_projectSettings != null)
                return _projectSettings.Resolve3DCameraEuler(projectionMode);

            return projectionMode == GridProjectionMode.Orthographic3D
                ? (_cameraSettings != null ? _cameraSettings.orthographic3DEuler : new Vector3(90f, 0f, 0f))
                : (_cameraSettings != null ? _cameraSettings.isometric3DEuler : new Vector3(50f, 45f, 0f));
        }

        public bool ResolveUsePerspectiveStartupCamera()
        {
            bool autoOrthographic = _cameraSettings != null && _cameraSettings.ResolveUseOrthographicCameraIn3D();
            if (_projectSettings != null)
                return _projectSettings.ResolveUsePerspectiveCamera(autoOrthographic);

            return ResolveProjectionMode() == GridProjectionMode.Isometric3DPreview || !autoOrthographic;
        }

        public GridProjectionMode ResolveProjectionMode()
        {
            if (_gridProjection != null)
                return _gridProjection.ProjectionMode;

            return _projectSettings != null
                ? _projectSettings.DefaultProjectionMode
                : GridProjectionMode.Isometric3DPreview;
        }

        public float ResolveStartupFieldOfView()
        {
            if (_projectSettings != null)
                return _projectSettings.ResolveProject3DFieldOfView();

            return _cameraSettings != null ? _cameraSettings.ResolveDefault3DFieldOfView() : 30f;
        }

        public float ResolveStartupOrthographicSize()
        {
            if (_projectSettings != null)
                return _projectSettings.ResolveProject3DOrthographicSize();

            return _cameraSettings != null ? _cameraSettings.ResolveDefault3DOrthographicSize() : 20f;
        }

        public Vector3 ResolveStartupFocusPoint(Vector2Int startPos, WorldGeneratedDataSignal signal)
        {
            if (signal.CellSize > 0.0001f && _gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ)
                return new Vector3(startPos.x * signal.CellSize, StartingPositionMapUtility.ResolveHeight(signal, startPos), startPos.y * signal.CellSize);

            if (_gridProjection != null)
                return _gridProjection.GridToWorld(startPos, StartingPositionMapUtility.ResolveHeight(signal, startPos));

            return new Vector3(startPos.x, startPos.y, 0f);
        }

        public float ResolveStartupCameraDistance()
        {
            if (_projectSettings != null)
                return _projectSettings.ResolveProject3DCameraDistance();

            if (_cameraSettings != null)
                return _cameraSettings.ResolveDefault3DCameraDistance();

            if (TryResolveCurrentCameraPlaneDistance(out float currentDistance))
                return currentDistance;

            return 20f;
        }

        public bool TryResolveCurrentCameraPlaneDistance(out float distance)
        {
            distance = 0f;
            if (_camera == null)
                return false;

            Vector3 normal = _gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? Vector3.up
                : Vector3.forward;
            Vector3 direction = _camera.transform.forward;
            float denominator = Vector3.Dot(normal, direction);
            if (Mathf.Abs(denominator) <= 0.0001f)
                return false;

            distance = -Vector3.Dot(normal, _camera.transform.position) / denominator;
            return distance > 0.1f && !float.IsNaN(distance) && !float.IsInfinity(distance);
        }

        public void ApplyStartupCameraZoom(Vector2Int startPos, Vector3 focusPoint, WorldGeneratedDataSignal signal)
        {
            if (!ShouldEnsureStartupCameraShowsRevealedArea() || _cameraZoom == null || _camera == null)
                return;

            Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
            float radius = ResolveStartupCameraRadius(baseMapSize.x, baseMapSize.y) + ResolveStartupCameraPaddingTiles();
            Vector3[] corners = BuildStartupZoneCorners(startPos, focusPoint, radius, signal);
            if (_camera.orthographic)
            {
                float zoom = ResolveOrthographicZoomToFit(focusPoint, corners);
                _camera.orthographicSize = zoom;
                _cameraZoom.ForceZoomCamera(zoom);
                return;
            }

            float fieldOfView = ResolvePerspectiveFieldOfViewToFit(focusPoint, corners);
            _camera.fieldOfView = fieldOfView;
            _cameraZoom.ForceZoomCamera(fieldOfView);
        }

        public bool ShouldEnsureStartupCameraShowsRevealedArea()
        {
            return _projectSettings != null
                ? _projectSettings.EnsureStartupCameraShowsRevealedArea
                : _settings.ensureStartupCameraShowsRevealedArea;
        }

        public float ResolveStartupCameraPaddingTiles()
        {
            return _projectSettings != null
                ? _projectSettings.ResolveStartupCameraPaddingTiles()
                : Mathf.Max(0f, _settings.startupCameraPaddingTiles);
        }

        public int ResolveStartupCameraRadius(int width, int height)
        {
            MoyvaStartupCameraRadiusSource source = _projectSettings != null
                ? _projectSettings.StartupCameraRadiusSource
                : _settings.startupCameraRadiusSource;

            return source switch
            {
                MoyvaStartupCameraRadiusSource.CoreVisibleRadius => _settings.ResolveCoreVisibleRadius(width, height),
                MoyvaStartupCameraRadiusSource.ManualRadius => _projectSettings != null
                    ? _projectSettings.ResolveManualStartupCameraRadius()
                    : Mathf.Max(1, _settings.manualStartupCameraRadius),
                _ => _settings.ResolveRevealedRadius(width, height),
            };
        }

        public Vector3[] BuildStartupZoneCorners(Vector2Int startPos, Vector3 focusPoint, float radius, WorldGeneratedDataSignal signal)
        {
            Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
            if (_gridProjection == null)
            {
                return new[]
                {
                    focusPoint + new Vector3(-radius, -radius, 0f),
                    focusPoint + new Vector3(radius, -radius, 0f),
                    focusPoint + new Vector3(-radius, radius, 0f),
                    focusPoint + new Vector3(radius, radius, 0f),
                };
            }

            int tileRadius = Mathf.Max(1, Mathf.CeilToInt(radius));
            var min = StartingPositionMapUtility.ClampToMap(new Vector2Int(startPos.x - tileRadius, startPos.y - tileRadius), baseMapSize.x, baseMapSize.y);
            var max = StartingPositionMapUtility.ClampToMap(new Vector2Int(startPos.x + tileRadius, startPos.y + tileRadius), baseMapSize.x, baseMapSize.y);
            return new[]
            {
                ProjectStartupCorner(new Vector2Int(min.x, min.y), signal),
                ProjectStartupCorner(new Vector2Int(max.x, min.y), signal),
                ProjectStartupCorner(new Vector2Int(min.x, max.y), signal),
                ProjectStartupCorner(new Vector2Int(max.x, max.y), signal),
            };
        }

        public Vector3 ProjectStartupCorner(Vector2Int gridPosition, WorldGeneratedDataSignal signal)
        {
            if (signal.CellSize > 0.0001f && _gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ)
                return new Vector3(gridPosition.x * signal.CellSize, StartingPositionMapUtility.ResolveHeight(signal, gridPosition), gridPosition.y * signal.CellSize);

            return _gridProjection.GridToWorld(gridPosition, StartingPositionMapUtility.ResolveHeight(signal, gridPosition));
        }

        public float ResolveOrthographicZoomToFit(Vector3 focusPoint, Vector3[] corners)
        {
            float aspect = _camera != null && _camera.aspect > 0.0001f ? _camera.aspect : 1f;
            Quaternion worldToView = Quaternion.Inverse(_camera.transform.rotation);
            ResolveViewHalfExtents(focusPoint, corners, worldToView, out float halfWidth, out float halfHeight);
            float required = Mathf.Max(halfHeight, halfWidth / aspect, 0.1f);
            float minZoom = _cameraSettings != null ? _cameraSettings.ResolveMinZoom() : 0.1f;
            return Mathf.Max(minZoom, required);
        }

        public float ResolvePerspectiveFieldOfViewToFit(Vector3 focusPoint, Vector3[] corners)
        {
            float aspect = _camera != null && _camera.aspect > 0.0001f ? _camera.aspect : 1f;
            Quaternion worldToView = Quaternion.Inverse(_camera.transform.rotation);
            ResolveViewHalfExtents(focusPoint, corners, worldToView, out float halfWidth, out float halfHeight);
            float requiredHalfHeight = Mathf.Max(halfHeight, halfWidth / aspect, 0.01f);
            Vector3 localFocus = worldToView * (focusPoint - _camera.transform.position);
            float distance = Mathf.Max(0.1f, Mathf.Abs(localFocus.z));
            float requiredFov = Mathf.Atan(requiredHalfHeight / distance) * 2f * Mathf.Rad2Deg;
            float configuredFov = _projectSettings != null
                ? _projectSettings.ResolveProject3DFieldOfView()
                : (_cameraSettings != null ? _cameraSettings.ResolveDefault3DFieldOfView() : 30f);
            return Mathf.Clamp(Mathf.Max(configuredFov, Mathf.Min(requiredFov, 35f)), 25f, 35f);
        }

        public static void ResolveViewHalfExtents(Vector3 focusPoint, Vector3[] corners, Quaternion worldToView, out float halfWidth, out float halfHeight)
        {
            halfWidth = 0.01f;
            halfHeight = 0.01f;
            if (corners == null)
                return;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 view = worldToView * (corners[i] - focusPoint);
                halfWidth = Mathf.Max(halfWidth, Mathf.Abs(view.x));
                halfHeight = Mathf.Max(halfHeight, Mathf.Abs(view.y));
            }
        }
    }
}
