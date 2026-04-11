using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
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
        }
    }
}
