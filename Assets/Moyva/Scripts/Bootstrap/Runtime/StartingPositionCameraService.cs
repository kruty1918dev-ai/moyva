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

    internal sealed partial class StartingPositionCameraService
        : IStartingPositionCameraService
    {
        private const string StartupChainTag = "[MoyvaStartupChain]";

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






        private static string FormatVector(Vector3 value)
        {
            return $"({value.x:0.###}, {value.y:0.###}, {value.z:0.###})";
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
