using Kruty1918.Moyva.Clouds.API;
using Kruty1918.Moyva.Grid.API;
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
                return;
            }

            Container.BindInstance(_settings).AsSingle();
            Container.BindInstance(new CloudsSceneReferences(_sceneCamera, _cloudsRoot)).AsSingle();
            Container.BindInterfacesAndSelfTo<CloudsWorldPresenter>()
                .FromMethod(ctx => new CloudsWorldPresenter(
                    _settings,
                    () => ResolveMapBounds(ctx.Container),
                    _sceneCamera,
                    _cloudsRoot))
                .AsSingle()
                .NonLazy();
        }

        private Rect ResolveMapBounds(DiContainer container)
        {
            IGridService grid = container.TryResolve<IGridService>();
            IGridProjection projection = container.TryResolve<IGridProjection>();
            if (grid != null && projection != null)
            {
                Bounds bounds = projection.GetWorldBounds(grid.GridWidth, grid.GridHeight);
                return Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
            }

            Vector2 halfSize = _settings.ManualMapSize * 0.5f;
            return Rect.MinMaxRect(
                _settings.ManualMapCenter.x - halfSize.x,
                _settings.ManualMapCenter.y - halfSize.y,
                _settings.ManualMapCenter.x + halfSize.x,
                _settings.ManualMapCenter.y + halfSize.y);
        }
    }
}
