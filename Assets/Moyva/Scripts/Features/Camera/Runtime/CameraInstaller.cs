using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    public class CameraInstaller : MonoInstaller
    {
        [Header("References")]
        [SerializeField] private UnityEngine.Camera _sceneCamera;
        
        [Header("Settings")]
        [SerializeField] private CameraSettingsSO _cameraSettings;
        [SerializeField] private InputActionAsset _cameraInputAsset;

        public override void InstallBindings()
        {
            // 1. Якщо камера не призначена в інспекторі, шукаємо MainCamera
            var camera = _sceneCamera != null ? _sceneCamera : UnityEngine.Camera.main;
            Container.BindInstance(camera).AsSingle();

            // 2. Біндимо налаштування
            Container.BindInstance(_cameraSettings).AsSingle();
            Container.BindInstance(_cameraInputAsset).AsSingle();

            // 3. Біндимо сервіси. 
            // Використовуємо BindInterfacesAndSelfTo, щоб підхопити ITickable, IInitializable та самі інтерфейси API
            Container.BindInterfacesAndSelfTo<CameraProjectSettingsAdapter>().AsSingle();
            Container.BindInterfacesAndSelfTo<TilemapCameraBoundsProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<CameraMovement>().AsSingle();
            Container.BindInterfacesAndSelfTo<CameraZoom>().AsSingle();
            Container.BindInterfacesAndSelfTo<CameraMapRenderMaskService>().AsSingle();
            
            // CameraFocused не має Tick/Initializable, тому можна просто до інтерфейсу
            Container.BindInterfacesTo<CameraFocused>().AsSingle();

            // 4. Біндимо контролер гравця
            Container.BindInterfacesAndSelfTo<CameraPlayerController>().AsSingle();

            Container.BindExecutionOrder<CameraProjectSettingsAdapter>(-100);
        }
    }

    internal sealed class CameraProjectSettingsAdapter : IInitializable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _cameraSettings;
        private readonly MoyvaProjectSettingsSO _projectSettings;
        private readonly IGridProjection _gridProjection;
        private readonly IGridService _gridService;

        public CameraProjectSettingsAdapter(
            UnityEngine.Camera camera,
            CameraSettingsSO cameraSettings,
            [InjectOptional] MoyvaProjectSettingsSO projectSettings = null,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IGridService gridService = null)
        {
            _camera = camera;
            _cameraSettings = cameraSettings;
            _projectSettings = projectSettings;
            _gridProjection = gridProjection;
            _gridService = gridService;
        }

        public void Initialize()
        {
            if (_camera == null || _cameraSettings == null)
                return;

            bool hasProjectCameraOverride = _projectSettings != null && _projectSettings.CameraPolicy != MoyvaCameraProjectPolicy.AutoFromGrid;
            if (!_cameraSettings.ResolveAdaptToProject3DMode() && !hasProjectCameraOverride)
                return;

            bool autoUse3D = ShouldUse3DCameraByGridMode();
            bool use3D = _projectSettings != null
                ? _projectSettings.ResolveUse3DCamera(autoUse3D)
                : autoUse3D;
            if (!use3D)
                return;

            GridProjectionMode projectionMode = ResolveProjectionMode();
            Vector3 euler = Resolve3DCameraEuler(projectionMode);

            _camera.transform.rotation = Quaternion.Euler(euler);
            bool usePerspective = ResolveUsePerspectiveCamera();
            _camera.orthographic = !usePerspective;
            if (_camera.orthographic)
                _camera.orthographicSize = ResolveDefault3DOrthographicSize();
            else
                _camera.fieldOfView = ResolveDefault3DFieldOfView();

            Vector3 focusPoint = ResolveWorldFocusPoint();
            _camera.transform.position = focusPoint - _camera.transform.forward * ResolveDefault3DCameraDistance();
        }

        private bool ShouldUse3DCameraByGridMode()
        {
            if (_gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ)
                return true;

            GridProjectionMode projectionMode = ResolveProjectionMode();
            if (projectionMode == GridProjectionMode.Orthographic3D || projectionMode == GridProjectionMode.Isometric3DPreview)
                return true;

            GridRenderMode renderMode = _projectSettings != null
                ? _projectSettings.DefaultRenderMode
                : GridRenderMode.Mesh3D;
            return renderMode == GridRenderMode.Mesh3D || renderMode == GridRenderMode.Mesh3DPreview;
        }

        private Vector3 Resolve3DCameraEuler(GridProjectionMode projectionMode)
        {
            if (_projectSettings != null)
                return _projectSettings.Resolve3DCameraEuler(projectionMode);

            return projectionMode == GridProjectionMode.Orthographic3D
                ? _cameraSettings.orthographic3DEuler
                : _cameraSettings.isometric3DEuler;
        }

        private bool ResolveUsePerspectiveCamera()
        {
            bool autoUseOrthographic = _cameraSettings.ResolveUseOrthographicCameraIn3D();
            return _projectSettings != null
                ? _projectSettings.ResolveUsePerspectiveCamera(autoUseOrthographic)
                : !autoUseOrthographic;
        }

        private float ResolveDefault3DOrthographicSize()
        {
            return _projectSettings != null
                ? Mathf.Max(_cameraSettings.ResolveMinZoom(), _projectSettings.ResolveProject3DOrthographicSize())
                : _cameraSettings.ResolveDefault3DOrthographicSize();
        }

        private float ResolveDefault3DFieldOfView()
        {
            return _projectSettings != null
                ? _projectSettings.ResolveProject3DFieldOfView()
                : _cameraSettings.ResolveDefault3DFieldOfView();
        }

        private float ResolveDefault3DCameraDistance()
        {
            return _projectSettings != null
                ? _projectSettings.ResolveProject3DCameraDistance()
                : _cameraSettings.ResolveDefault3DCameraDistance();
        }

        private GridProjectionMode ResolveProjectionMode()
        {
            if (_gridProjection != null)
                return _gridProjection.ProjectionMode;

            return _projectSettings != null
                ? _projectSettings.DefaultProjectionMode
                : GridProjectionMode.Orthographic3D;
        }

        private Vector3 ResolveWorldFocusPoint()
        {
            if (_gridProjection == null || _gridService == null || _gridService.GridWidth <= 0 || _gridService.GridHeight <= 0)
                return Vector3.zero;

            return _gridProjection.GetWorldBounds(_gridService.GridWidth, _gridService.GridHeight).center;
        }
    }
}