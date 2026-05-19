using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Combat;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.WorldCreation.API;

namespace Kruty1918.Moyva.Units.Runtime
{
    public class UnitsInstaller : MonoInstaller
    {
        [SerializeField] private UnitRegistrySO _unitRegistry;
        [SerializeField] private WorldCreationDefaultsSO _worldDefaults;

        public override void InstallBindings()
        {
            Container.BindInstance(_unitRegistry).AsSingle();

            // IHealthRegistry — забезпечується CombatInstaller
            if (!Container.HasBinding(typeof(IHealthRegistry)))
                CombatInstaller.Install(Container);

            if (_worldDefaults != null)
            {
                Container.Bind<WorldCreationDefaultsSO>()
                    .FromInstance(_worldDefaults)
                    .WhenInjectedInto<UnitMovementService>();
            }

            Container.BindInterfacesAndSelfTo<UnitService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IUnitFactory>()
                .To<UnitFactory>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<UnitMovementService>()
                .AsSingle();

            Container.Bind<IUnitClassConfig>()
                .To<UnitClassConfigService>()
                .AsSingle();

            Container.Bind<IUnitGameplayProfileService>()
                .To<UnitGameplayProfileService>()
                .AsSingle();

            Container.Bind<IUnitCombatService>()
                .To<UnitCombatService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<UnitWorldInfoPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<UnitSelectionVisualService>()
                .AsSingle()
                .NonLazy();
        }
    }
}