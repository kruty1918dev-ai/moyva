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

                Container.Bind<WorldCreationDefaultsSO>()
                    .FromInstance(_worldDefaults)
                    .WhenInjectedInto<UnitPlacementValidator>();
            }

            Container.BindInterfacesAndSelfTo<UnitService>()
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

            Container.BindInterfacesAndSelfTo<UnitMovementService>()
                .AsSingle();

            // P04: all command-facing movement resolutions receive the authoritative
            // turn/owner lease decorator. The raw UnitMovementService remains the
            // ITurnBlocker so end-turn waits for its active movement set.
            Container.Decorate<IUnitMovementService>()
                .With<UnitTurnAuthorityMovementService>();

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

            Container.BindInterfacesAndSelfTo<UnitCombatService>()
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
