using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy
{
    /// <summary>
    /// Zenject MonoInstaller для економічної системи.
    /// Підключіть цей installer у сцені разом із <see cref="EconomyDatabaseSO"/>.
    ///
    /// Гейм-луп: Calendar.OnHourChanged → EconomyManager.OnTurnAdvanced()
    ///           → Population → WorkerAllocation → Production (для кожного поселення)
    ///           → EconomyTickCompletedSignal (для UI та інших систем)
    ///
    /// BuildingPlacedSignal → EconomyManager створює поселення (якщо TownHall) або додає будівлю.
    /// BuildingDemolishedSignal → EconomyManager прибирає будівлю / деактивує поселення.
    /// </summary>
    public sealed class EconomyInstaller : MonoInstaller
    {
        public static void InstallSimulationBindings(DiContainer container, EconomyDatabaseSO database)
        {
            if (database?.RulesConfig == null) throw new System.InvalidOperationException("Economy rules are missing.");
            container.BindInstance(database).IfNotBound();
            container.BindInterfacesAndSelfTo<EconomyManager>().AsSingle();
            container.Bind<ISettlementRegistry>().To<EconomySettlementRegistryService>().AsSingle();
            container.Bind<IEconomyOwnerResourcePoolService>().To<EconomyOwnerResourcePoolService>().AsSingle();
            container.Bind<IEconomyBuildingIntegration>().To<EconomyBuildingIntegrationService>().AsSingle();
            container.Bind<IEconomyTurnProcessor>().To<EconomyTurnProcessorService>().AsSingle();
            container.Bind<IEconomyInfoMediator>().To<EconomyInfoMediator>().AsSingle();
            container.Bind<IEconomyRuntimeApi>().To<EconomyRuntimeApi>().AsSingle();
            if (!container.HasBinding<ICaravanService>())
            {
                container.BindInterfacesAndSelfTo<CaravanService>().AsSingle().NonLazy();
            }
            if (!container.HasBinding<IConstructionSupplyService>())
            {
                container.BindInterfacesAndSelfTo<ConstructionSupplyService>().AsSingle().NonLazy();
            }
            InstallCaptureBindings(container);
        }

        private static void InstallCaptureBindings(DiContainer container)
        {
            if (!container.HasBinding<ISettlementCaptureService>())
                container.Bind<ISettlementCaptureService>().To<SettlementCaptureService>().AsSingle();
            if (!container.HasBinding<ISettlementCaptureQuery>())
                container.Bind<ISettlementCaptureQuery>().FromMethod(context => context.Container.Resolve<ISettlementCaptureService>()).AsSingle();
        }

        [SerializeField]
        [Tooltip("Основна база даних економіки. Створюється через Economy Hub (Moyva/Tools/Редактор Економіки).")]
        private EconomyDatabaseSO _database;

        [SerializeField]
        [Tooltip("Шаблон параметрів Economy Hub (EconomyRulesConfiguration). Використовується runtime API для централізованого форматування UI-даних.")]
        private EconomyRulesConfiguration _rulesTemplate;

        public override void InstallBindings()
        {
            if (_database == null)
            {
                Debug.LogError("[EconomyInstaller] Поле '_database' не призначено. Економіка не буде працювати.", this);
                return;
            }

            if (_database.RulesConfig == null)
            {
                Debug.LogError("[EconomyInstaller] EconomyDatabaseSO не має RulesConfig. Створіть через Economy Hub.", this);
                return;
            }

            // Bind database once (Zenject 6 disallows duplicate AsSingle for same contract)
            Container.BindInstance(_database)
                .IfNotBound();

            if (_rulesTemplate != null)
            {
                Container.BindInstance(_rulesTemplate)
                    .IfNotBound();
            }

            // Main facade — handles Calendar + Construction signals integration
            // Scene can accidentally contain multiple EconomyInstaller instances.
            // Guard against duplicate AsSingle bindings (Zenject 6+).
            if (!Container.HasBinding<EconomyManager>())
            {
                Container.BindInterfacesAndSelfTo<EconomyManager>()
                    .AsSingle()
                    .NonLazy();

                // Explicit execution order: Economy initializes AFTER Construction (0) and GameMode (-10)
                Container.BindExecutionOrder<EconomyManager>(20);
            }

            if (!Container.HasBinding<IEconomyRuntimeApi>())
            {
                Container.Bind<IEconomyRuntimeApi>()
                    .To<EconomyRuntimeApi>()
                    .AsSingle();
            }

            InstallCaptureBindings(Container);

            if (!Container.HasBinding<IEconomyInfoMediator>())
            {
                Container.Bind<IEconomyInfoMediator>()
                    .To<EconomyInfoMediator>()
                    .AsSingle();
            }

            if (!Container.HasBinding<IEconomyOwnerResourcePoolService>())
            {
                Container.Bind<IEconomyOwnerResourcePoolService>()
                    .To<EconomyOwnerResourcePoolService>()
                    .AsSingle();
            }

            if (!Container.HasBinding<ISettlementRegistry>())
            {
                Container.Bind<ISettlementRegistry>()
                    .To<EconomySettlementRegistryService>()
                    .AsSingle();
            }

            if (!Container.HasBinding<IEconomyBuildingIntegration>())
            {
                Container.Bind<IEconomyBuildingIntegration>()
                    .To<EconomyBuildingIntegrationService>()
                    .AsSingle();
            }

            if (!Container.HasBinding<IEconomyTurnProcessor>())
            {
                Container.Bind<IEconomyTurnProcessor>()
                    .To<EconomyTurnProcessorService>()
                    .AsSingle();
            }

            if (!Container.HasBinding<IMapObjectEconomyService>())
            {
                Container.Bind<IMapObjectEconomyService>()
                    .To<MapObjectEconomyService>()
                    .AsSingle();
            }

            if (!Container.HasBinding<ICaravanService>())
            {
                Container.BindInterfacesAndSelfTo<CaravanService>()
                    .AsSingle().NonLazy();
                Container.BindInterfacesTo<SaveModuleRegistrar<CaravanService>>()
                    .AsSingle().NonLazy();
            }

            if (!Container.HasBinding<EconomySaveModule>())
            {
                Container.BindInterfacesAndSelfTo<EconomySaveModule>()
                    .AsSingle();

                Container.BindInterfacesTo<SaveModuleRegistrar<EconomySaveModule>>()
                    .AsSingle()
                    .NonLazy();
            }

            if (!Container.HasBinding<IConstructionSupplyService>())
            {
                Container.BindInterfacesAndSelfTo<ConstructionSupplyService>()
                    .AsSingle()
                    .NonLazy();

                Container.BindInterfacesTo<SaveModuleRegistrar<ConstructionSupplyService>>()
                    .AsSingle()
                    .NonLazy();
            }
        }
    }
}
