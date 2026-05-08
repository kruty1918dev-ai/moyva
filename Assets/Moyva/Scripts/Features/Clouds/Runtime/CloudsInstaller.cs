using Kruty1918.Moyva.Clouds.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Clouds.Runtime
{
    public sealed class CloudsInstaller : MonoInstaller
    {
        [Header("Налаштування")]
        [Tooltip("Об'єкт налаштувань хмаринок.")]
        [SerializeField] private CloudsSettings _settings;

        [Header("Посилання сцени")]
        [Tooltip("Камера, відносно якої хмаринки спавняться за екраном. Якщо не задано, використовується Camera.main.")]
        [SerializeField] private UnityEngine.Camera _sceneCamera;

        [Tooltip("Батьківський Transform для створених хмаринок. Якщо не задано, система створить CloudsRoot автоматично.")]
        [SerializeField] private Transform _cloudsRoot;

        public override void InstallBindings()
        {
            if (_settings == null)
            {
                Debug.LogWarning("[Clouds] CloudsSettings не призначено. Система хмаринок не буде запущена.");
                return;
            }

            Container.BindInstance(_settings).AsSingle();
            Container.BindInstance(new CloudsSceneReferences(_sceneCamera, _cloudsRoot)).AsSingle();
            Container.BindInterfacesAndSelfTo<CloudsService>().AsSingle().NonLazy();
        }
    }
}