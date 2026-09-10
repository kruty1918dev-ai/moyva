using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Combat;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.WorldCreation.API;

namespace Kruty1918.Moyva.Units.Runtime
{
    public class UnitsInstaller : MonoInstaller
    {
        [SerializeField] private UnitRegistrySO _unitRegistry;
        [SerializeField] private WorldCreationDefaultsSO _worldDefaults;

        public static void InstallPreviewBindings(DiContainer container, UnitRegistrySO registry)
        {
            container.BindInstance(registry);
            CombatInstaller.Install(container);
            container.BindInterfacesAndSelfTo<UnitService>().AsSingle();
            container.Bind<IUnitFactory>().To<UnitFactory>().AsSingle();
            container.Bind<IUnitClassConfig>().To<UnitClassConfigService>().AsSingle();
            container.Bind<IUnitGameplayProfileService>().To<UnitGameplayProfileService>().AsSingle();
            container.Bind<IUnitWorldPositionResolver>().To<UnitWorldPositionResolver>().AsSingle();
            container.BindInterfacesAndSelfTo<UnitMovementService>().AsSingle();
            container.BindInterfacesAndSelfTo<UnitCombatService>().AsSingle();
        }

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

                Container.Bind<WorldCreationDefaultsSO>()
                    .FromInstance(_worldDefaults)
                    .WhenInjectedInto<UnitPlacementValidator>();
            }

            Container.BindInterfacesAndSelfTo<UnitService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<UnitsSaveModule>()
                .AsSingle();

            Container.BindInterfacesTo<SaveModuleRegistrar<UnitsSaveModule>>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IUnitFactory>()
                .To<UnitFactory>()
                .AsSingle();

            Container.Bind<IUnitPlacementValidator>()
                .To<UnitPlacementValidator>()
                .AsSingle();

            Container.Bind<IUnitWorldPositionResolver>()
                .To<UnitWorldPositionResolver>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<UnitTraversalPolicy>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<UnitMovementRangeQuery>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<UnitMovementService>()
                .AsSingle();

            // P04: all command-facing movement resolutions receive the authoritative
            // turn/owner lease decorator. The raw UnitMovementService remains the
            // ITurnBlocker so end-turn waits for its active movement set.
            Container.Decorate<IUnitMovementService>()
                .With<UnitTurnAuthorityMovementService>();

            Container.BindInterfacesAndSelfTo<UnitAuthorityEndpointBridge>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<UnitTurnParticipant>()
                .AsSingle();

            Container.Bind<IUnitClassConfig>()
                .To<UnitClassConfigService>()
                .AsSingle();

            // P07: recruitment queue consumes data-driven building recipes and
            // advances as a deterministic turn participant. Deployment is P08.
            Container.BindInterfacesAndSelfTo<UnitRecruitmentService>()
                .AsSingle();

            Container.Bind<IUnitGameplayProfileService>()
                .To<UnitGameplayProfileService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<UnitTurnActionStateService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<UnitCombatService>()
                .AsSingle();

            Container.Bind<ICombatCommandService>()
                .To<UnitCombatCommandService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<UnitCombatPresentationService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<UnitWorldInfoPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<UnitSelectionVisualService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<UnitMovementGridPresenter>()
                .AsSingle()
                .NonLazy();
        }
    }
}
