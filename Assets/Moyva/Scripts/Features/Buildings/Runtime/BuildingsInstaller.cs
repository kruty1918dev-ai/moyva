using Kruty1918.Moyva.Buildings.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// Zenject інсталятор для системи будівництва.
    /// Додайте цей MonoInstaller до SceneContext у сцені.
    /// Призначте BuildingRegistrySO та (опціонально) WallConnectionPointPrefab у інспекторі.
    /// </summary>
    public class BuildingsInstaller : MonoInstaller
    {
        [SerializeField] private BuildingRegistrySO _buildingRegistry;

        [Tooltip("Префаб точки з'єднання стіни (коло з компонентом WallConnectionPoint). Опціонально.")]
        [SerializeField] private GameObject _wallConnectionPointPrefab;

        public override void InstallBindings()
        {
            // Реєстр будівель (ScriptableObject)
            Container.BindInstance(_buildingRegistry).AsSingle();

            // Сервіс підтверджених будівель
            Container.BindInterfacesAndSelfTo<BuildingService>()
                .AsSingle()
                .NonLazy();

            // Сервіс розміщення будівель (сесійний)
            Container.BindInterfacesAndSelfTo<BuildingPlacementService>()
                .AsSingle()
                .NonLazy();

            // Прив'язка префабу точки з'єднання стіни (якщо призначений)
            if (_wallConnectionPointPrefab != null)
            {
                Container.BindInstance(_wallConnectionPointPrefab)
                    .WithId("WallConnectionPointPrefab")
                    .AsSingle();
            }
            // Сервіс стін
            Container.BindInterfacesAndSelfTo<WallService>()
                .AsSingle()
                .NonLazy();
        }
    }
}
