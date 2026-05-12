using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.SaveSystem;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    public sealed class ConstructionInstaller : MonoInstaller
    {
        [SerializeField] private BuildingRegistrySO buildingRegistry;

        [Header("Будівництво — налаштування")]
        [Tooltip("Мінімальна відстань (в тайлах) між будівлями. 0 = можна ставити впритул.")]
        [SerializeField] private int _minSpacingBetweenBuildings = 0;

        [Header("Підписи поселень")]
        [SerializeField] private SettlementLabelSettings _settlementLabelSettings = new();

        public override void InstallBindings()
        {
            if (buildingRegistry == null)
            {
                Debug.LogError("[ConstructionInstaller] Поле 'buildingRegistry' не призначено.", this);
                return;
            }

            Container.BindInstance(buildingRegistry).AsSingle();
            Container.Bind<IBuildingRegistry>().FromInstance(buildingRegistry).AsSingle();

            Container.BindInstance(_minSpacingBetweenBuildings).WithId("minSpacing");

            int townHallBuildRadius;
            try
            {
                townHallBuildRadius = ResolveTownHallBuildRadiusFromEconomy();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConstructionInstaller] КРИТИЧНА ПОМИЛКА: не вдалося отримати townHallBuildRadius з Economy. {ex.GetType().Name}: {ex.Message}", this);
                return;
            }

            Container.BindInstance(townHallBuildRadius).WithId("townHallBuildRadius");

            Container.Bind<IScreenToGridConverter>()
                .To<ScreenToGridConverter>()
                .AsSingle();

            Container.Bind<IAutoTileVariantResolver>()
                .To<AutoTileVariantResolver>()
                .AsSingle();

            Container.Bind<IObjectTypePicker>()
                .To<ObjectTypePickerService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ConstructionService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<WallPlacementService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<ConstructionInputService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<ConstructionVisualService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<BuildingWorldInfoPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<CastleDetailedInfoPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInstance(_settlementLabelSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<SettlementLabelService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<ISaveModule>()
                .To<ConstructionSaveModule>()
                .AsSingle();

            // Явний порядок Initialize() — виконується ПІСЛЯ GameMode (-10).
            Container.BindExecutionOrder<ConstructionService>(0);
            Container.BindExecutionOrder<ConstructionInputService>(5);
            Container.BindExecutionOrder<ConstructionVisualService>(10);
            Container.BindExecutionOrder<BuildingWorldInfoPresenter>(15);
            Container.BindExecutionOrder<CastleDetailedInfoPresenter>(16);
            Container.BindExecutionOrder<SettlementLabelService>(20);
        }

        private int ResolveTownHallBuildRadiusFromEconomy()
        {
            Type economyInstallerType = Type.GetType("Kruty1918.Moyva.Economy.EconomyInstaller, Kruty1918.Moyva.Economy");
            if (economyInstallerType == null)
                throw new InvalidOperationException("EconomyInstaller type not found.");

            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            MonoBehaviour economyInstaller = null;
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null)
                    continue;

                if (economyInstallerType.IsInstanceOfType(behaviours[i]))
                {
                    economyInstaller = behaviours[i];
                    break;
                }
            }

            if (economyInstaller == null)
                throw new InvalidOperationException("EconomyInstaller not found in scene.");

            FieldInfo databaseField = economyInstallerType.GetField("_database", BindingFlags.Instance | BindingFlags.NonPublic);
            if (databaseField == null)
                throw new InvalidOperationException("Field '_database' not found on EconomyInstaller.");

            object economyDatabase = databaseField.GetValue(economyInstaller);
            if (economyDatabase == null)
                throw new InvalidOperationException("EconomyInstaller._database is null.");

            PropertyInfo rulesConfigProp = economyDatabase.GetType().GetProperty("RulesConfig", BindingFlags.Instance | BindingFlags.Public);
            if (rulesConfigProp == null)
                throw new InvalidOperationException("RulesConfig property not found on EconomyDatabaseSO.");

            object rulesConfig = rulesConfigProp.GetValue(economyDatabase);
            if (rulesConfig == null)
                throw new InvalidOperationException("EconomyDatabaseSO.RulesConfig is null.");

            PropertyInfo settlementRulesProp = rulesConfig.GetType().GetProperty("Settlement", BindingFlags.Instance | BindingFlags.Public);
            if (settlementRulesProp == null)
                throw new InvalidOperationException("Settlement property not found on EconomyRulesConfigSO.");

            object settlementRules = settlementRulesProp.GetValue(rulesConfig);
            if (settlementRules == null)
                throw new InvalidOperationException("EconomyRulesConfigSO.Settlement is null.");

            PropertyInfo minDistanceProp = settlementRules.GetType().GetProperty("MinTownHallDistance", BindingFlags.Instance | BindingFlags.Public);
            if (minDistanceProp == null)
                throw new InvalidOperationException("MinTownHallDistance property not found on EconomySettlementRules.");

            object rawValue = minDistanceProp.GetValue(settlementRules);
            if (rawValue is not int minDistance)
                throw new InvalidOperationException("MinTownHallDistance has invalid type.");

            int resolved = Mathf.Max(0, minDistance);
            Debug.Log($"[ConstructionInstaller] townHallBuildRadius resolved from Economy.Settlement.MinTownHallDistance = {resolved}");
            return resolved;
        }
    }
}
