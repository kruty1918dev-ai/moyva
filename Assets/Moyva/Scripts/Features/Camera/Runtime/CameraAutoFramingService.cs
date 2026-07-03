using System;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraAutoFramingService : IInitializable, IDisposable
    {
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        private const string StartupChainTag = "[MoyvaStartupChain]";
        private readonly SignalBus _signalBus;
        private readonly ICameraMovement _cameraMovement;
        private readonly ICameraZoom _cameraZoom;
        private readonly IGridProjection _gridProjection;
        private readonly CameraSettingsSO _cameraSettings;
        private readonly MoyvaProjectSettingsSO _projectSettings;
        private readonly UnityEngine.Camera _camera;

        private WorldGeneratedDataSignal _lastWorld;
        private bool _hasWorld;
        private bool _hasAppliedStartupFrame;
        private SpawnPositionAssignment[] _lastSpawnAssignments;

        public CameraAutoFramingService(
            SignalBus signalBus,
            ICameraMovement cameraMovement,
            ICameraZoom cameraZoom,
            CameraSettingsSO cameraSettings,
            [InjectOptional] MoyvaProjectSettingsSO projectSettings = null,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] UnityEngine.Camera camera = null)
        {
            _signalBus = signalBus;
            _cameraMovement = cameraMovement;
            _cameraZoom = cameraZoom;
            _cameraSettings = cameraSettings;
            _projectSettings = projectSettings;
            _gridProjection = gridProjection;
            _camera = camera;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);
            Debug.Log($"{WorldGenDiagTag} Receiver.Camera.Initialize subscribed frame={Time.frameCount}");
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            Debug.Log($"{WorldGenDiagTag} Receiver.Camera.WorldGenerated RECEIVED frame={Time.frameCount}, map={signal.Width}x{signal.Height}");
            _lastWorld = signal;
            _hasWorld = signal.Width > 0 && signal.Height > 0;
            ApplyAutoFrame();
        }

        private void OnSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            Debug.Log($"{WorldGenDiagTag} Receiver.Camera.WorldSpawnPositions RECEIVED frame={Time.frameCount}, assignments={signal.Assignments?.Length ?? 0}");
            _lastSpawnAssignments = signal.Assignments;
            ApplyAutoFrame();
        }

        private void ApplyAutoFrame()
        {
            string skipReason = ResolveAutoFrameSkipReason();
            if (skipReason != null)
            {
                Debug.Log($"{StartupChainTag} Camera.AutoFrame SKIP reason={skipReason}, hasWorld={_hasWorld}, assignments={_lastSpawnAssignments?.Length ?? 0}, applied={_hasAppliedStartupFrame}, hasMovement={_cameraMovement != null}, camera={FormatCameraState()}.");
                return;
            }

            Debug.Log($"{StartupChainTag} Camera.AutoFrame ENTER before={FormatCameraState()}, assignments={_lastSpawnAssignments.Length}, world={_lastWorld.Width}x{_lastWorld.Height}.");
            ConfigureStartupCameraPose();

            Vector2Int focusGrid = ResolveFocusGridPosition(_lastWorld.Width, _lastWorld.Height);
            float elevation = ResolveHeight(focusGrid, _lastWorld.HeightMap);
            Vector3 focusPoint = _gridProjection != null
                ? _gridProjection.GridToWorld(focusGrid, elevation)
                : new Vector3(focusGrid.x, focusGrid.y, 0f);

            float distance = ResolveStartupCameraDistance();
            Debug.Log($"{StartupChainTag} Camera.AutoFrame CALL movement focusGrid={focusGrid}, elevation={elevation:0.###}, focusPoint={FormatVector(focusPoint)}, distance={distance:0.###}, afterPose={FormatCameraState()}.");
            _cameraMovement.TeleportCameraToFocusPoint(focusPoint, distance);
            _hasAppliedStartupFrame = true;

            if (_cameraZoom == null)
            {
                Debug.Log($"{StartupChainTag} Camera.AutoFrame EXIT zoomSkipped=true reason=no-camera-zoom, after={FormatCameraState()}.");
                return;
            }

            _cameraZoom.ForceZoomCamera(ResolveCurrentCameraZoomLevel());
            Debug.Log($"{StartupChainTag} Camera.AutoFrame EXIT focusGrid={focusGrid}, focusPoint={FormatVector(focusPoint)}, after={FormatCameraState()}.");
        }

        private string ResolveAutoFrameSkipReason()
        {
            if (_hasAppliedStartupFrame)
                return "already-applied";
            if (!_hasWorld)
                return "world-not-ready";
            if (_lastSpawnAssignments == null)
                return "spawn-assignments-null";
            if (_lastSpawnAssignments.Length == 0)
                return "spawn-assignments-empty";
            if (_cameraMovement == null)
                return "camera-movement-null";
            if (_camera == null)
                return "camera-null";

            return null;
        }

        private void ConfigureStartupCameraPose()
        {
            Debug.Log($"{StartupChainTag} Camera.AutoFramePose ENTER before={FormatCameraState()}.");
            _camera.transform.rotation = Quaternion.Euler(ResolveStartupCameraEuler());
            bool usePerspective = ResolveUsePerspectiveStartupCamera();
            _camera.orthographic = !usePerspective;
            if (_camera.orthographic)
                _camera.orthographicSize = ResolveStartupOrthographicSize();
            else
                _camera.fieldOfView = ResolveStartupFieldOfView();
            Debug.Log($"{StartupChainTag} Camera.AutoFramePose EXIT usePerspective={usePerspective}, after={FormatCameraState()}.");
        }

        private Vector3 ResolveStartupCameraEuler()
        {
            GridProjectionMode projectionMode = ResolveProjectionMode();
            if (_projectSettings != null)
                return _projectSettings.Resolve3DCameraEuler(projectionMode);

            return projectionMode == GridProjectionMode.Orthographic3D
                ? _cameraSettings.orthographic3DEuler
                : _cameraSettings.isometric3DEuler;
        }

        private bool ResolveUsePerspectiveStartupCamera()
        {
            if (_projectSettings != null)
                return _projectSettings.ResolveUsePerspectiveCamera(_cameraSettings.ResolveUseOrthographicCameraIn3D());

            return ResolveProjectionMode() == GridProjectionMode.Isometric3DPreview
                || !_cameraSettings.ResolveUseOrthographicCameraIn3D();
        }

        private float ResolveStartupCameraDistance()
        {
            if (_projectSettings != null)
                return _projectSettings.ResolveProject3DCameraDistance();

            return _cameraSettings != null ? _cameraSettings.ResolveDefault3DCameraDistance() : 35f;
        }

        private float ResolveCurrentCameraZoomLevel()
        {
            if (_camera == null)
                return ResolveStartupFieldOfView();

            if (_camera.orthographic)
                return _camera.orthographicSize;

            return _camera.fieldOfView;
        }

        private float ResolveStartupFieldOfView()
        {
            if (_projectSettings != null)
                return _projectSettings.ResolveProject3DFieldOfView();

            return _cameraSettings != null ? _cameraSettings.ResolveDefault3DFieldOfView() : 40f;
        }

        private float ResolveStartupOrthographicSize()
        {
            if (_projectSettings != null)
                return _projectSettings.ResolveProject3DOrthographicSize();

            return _cameraSettings != null ? _cameraSettings.ResolveDefault3DOrthographicSize() : 20f;
        }

        private GridProjectionMode ResolveProjectionMode()
        {
            if (_gridProjection != null)
                return _gridProjection.ProjectionMode;

            return _projectSettings != null
                ? _projectSettings.DefaultProjectionMode
                : GridProjectionMode.Isometric3DPreview;
        }

        private Vector2Int ResolveFocusGridPosition(int width, int height)
        {
            if (_lastSpawnAssignments != null && _lastSpawnAssignments.Length > 0)
            {
                Vector2Int spawn = _lastSpawnAssignments[0].Position;
                return new Vector2Int(
                    Mathf.Clamp(spawn.x, 0, Mathf.Max(0, width - 1)),
                    Mathf.Clamp(spawn.y, 0, Mathf.Max(0, height - 1)));
            }

            return new Vector2Int(Mathf.Max(0, width / 2), Mathf.Max(0, height / 2));
        }

        private float ResolveHeight(Vector2Int position, float[,] heightMap)
        {
            if (heightMap == null)
                return 0f;

            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            if (position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
                return 0f;

            return heightMap[position.x, position.y];
        }

        private string FormatCameraState()
        {
            if (_camera == null)
                return "camera=null";

            return $"pos={FormatVector(_camera.transform.position)}, rot={FormatVector(_camera.transform.eulerAngles)}, orthographic={_camera.orthographic}, orthoSize={_camera.orthographicSize:0.###}, fov={_camera.fieldOfView:0.###}";
        }

        private static string FormatVector(Vector3 value)
        {
            return $"({value.x:0.###}, {value.y:0.###}, {value.z:0.###})";
        }

    }
}
