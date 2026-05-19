using Kruty1918.Moyva.Camera.API;
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
            Container.BindInterfacesAndSelfTo<TilemapCameraBoundsProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<CameraMovement>().AsSingle();
            Container.BindInterfacesAndSelfTo<CameraZoom>().AsSingle();
            Container.BindInterfacesAndSelfTo<CameraMapRenderMaskService>().AsSingle();
            
            // CameraFocused не має Tick/Initializable, тому можна просто до інтерфейсу
            Container.BindInterfacesTo<CameraFocused>().AsSingle();

            // 4. Біндимо контролер гравця
            Container.BindInterfacesAndSelfTo<CameraPlayerController>().AsSingle();
        }
    }
}