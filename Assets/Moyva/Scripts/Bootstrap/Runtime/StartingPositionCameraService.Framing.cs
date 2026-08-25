using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class StartingPositionCameraService
    {
        public void TeleportMainCamera(Vector2Int startPos, WorldGeneratedDataSignal signal)
        {
            if (TryTeleportCameraToStartupFocus(startPos, signal))
            {
                return;
            }
            _cameraMovement.TeleportCamera(new Vector3(startPos.x, startPos.y, _settings.cameraZ));
        }

        public bool TryTeleportCameraToStartupFocus(Vector2Int startPos, WorldGeneratedDataSignal signal)
        {
            if (_cameraMovement == null)
            {
                return false;
            }
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
            {
                return;
            }
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

        private string FormatCameraState()
        {
            if (_camera == null)
                return "camera=null";

            return $"pos={FormatVector(_camera.transform.position)}, rot={FormatVector(_camera.transform.eulerAngles)}, orthographic={_camera.orthographic}, orthoSize={_camera.orthographicSize:0.###}, fov={_camera.fieldOfView:0.###}";
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
    }
}
