using Kruty1918.Moyva.Combat;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.WorldCreation.API;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    public sealed class ConstructionInstaller : MonoInstaller
    {
        [SerializeField] private BuildingRegistrySO buildingRegistry;
        [SerializeField] private WorldCreationDefaultsSO _worldDefaults;
        [SerializeField] private ConstructionSceneContext _sceneContext;

        [Header("Будівництво — налаштування")]
        [Tooltip("Мінімальна відстань (в тайлах) між будівлями. 0 = можна ставити впритул.")]
        [SerializeField] private int _minSpacingBetweenBuildings = 0;

        [Header("Підписи поселень")]
        [SerializeField] private SettlementLabelSettings _settlementLabelSettings = new();

        public override void InstallBindings()
        {
            _sceneContext ??= GetComponent<ConstructionSceneContext>();
            if (buildingRegistry == null && _sceneContext?.BuildingRegistry != null)
                buildingRegistry = _sceneContext.BuildingRegistry;
            if (buildingRegistry == null && _sceneContext?.SystemProfile?.BuildingRegistry != null)
                buildingRegistry = _sceneContext.SystemProfile.BuildingRegistry;

            if (buildingRegistry == null)
            {
                Debug.LogError("[ConstructionInstaller] Поле 'buildingRegistry' не призначено.", this);
                return;
            }

            Container.BindInstance(buildingRegistry).AsSingle();
            Container.Bind<IBuildingRegistry>().FromInstance(buildingRegistry).AsSingle();

            if (_sceneContext != null)
                Container.BindInstance(_sceneContext).AsSingle();

            BindIntegratedProfiles();

            if (_worldDefaults != null)
            {
                Container.Bind<WorldCreationDefaultsSO>()
                    .FromInstance(_worldDefaults)
                    .WhenInjectedInto<ConstructionService>();
            }

            Container.BindInstance(_minSpacingBetweenBuildings).WithId("fallbackMinSpacing");

            // IHealthRegistry — забезпечується CombatInstaller
            if (!Container.HasBinding(typeof(IHealthRegistry)))
                CombatInstaller.Install(Container);

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

            Container.BindInstance(townHallBuildRadius).WithId("fallbackTownHallBuildRadius");

            Container.BindInterfacesAndSelfTo<ConstructionSceneSettingsProvider>()
                .AsSingle();

            Container.Bind<int>().WithId("minSpacing")
                .FromResolveGetter<IConstructionPlacementRulesProvider>(provider => provider.MinSpacing)
                .AsCached();

            Container.Bind<int>().WithId("townHallBuildRadius")
                .FromResolveGetter<IConstructionPlacementRulesProvider>(provider => provider.TownHallBuildRadius)
                .AsCached();

            Container.Bind<IScreenToGridConverter>()
                .To<ScreenToGridConverter>()
                .AsSingle();

            Container.Bind<IConstructionPointerInputSource>()
                .To<InputSystemConstructionPointerInputSource>()
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

            Container.Bind<IConstructionConfirmRequestExecutor>()
                .To<ConstructionLocalConfirmExecutor>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ConstructionConfirmRequestRouter>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IWallTopologyService>()
                .To<WallTopologyService>()
                .AsSingle();

            Container.Bind<IWallGateReplacementValidator>()
                .To<WallGateReplacementValidator>()
                .AsSingle();

            Container.Bind<IWallPathfinder>()
                .To<WallPathfinder>()
                .AsSingle();

            Container.Bind<IWallDragPreviewService>()
                .To<WallDragPreviewService>()
                .AsSingle();

            Container.Bind<IWallVisualResolver>()
                .To<WallVisualResolver>()
                .AsSingle();

            Container.Bind<IWallPrefabResolver>()
                .To<WallPrefabResolver>()
                .AsSingle();

            Container.Bind<IWallHandleController>()
                .To<WallHandleController>()
                .AsSingle();

            Container.Bind<IWallPlacementService>()
                .To<WallPlacementService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IConstructionVisualStyleService>()
                .To<ConstructionVisualStyleService>()
                .AsSingle();

            Container.Bind<IConstructionTerrainAlignmentService>()
                .To<ConstructionTerrainAlignmentService>()
                .AsSingle();

            Container.Bind<IConstructionVisualFactory>()
                .To<ConstructionVisualFactory>()
                .AsSingle();

            Container.Bind<IConstructionBlockedFlashService>()
                .To<ConstructionBlockedFlashService>()
                .AsSingle();

            Container.Bind<IConstructionInteractiveUiHitTester>()
                .To<ConstructionInteractiveUiHitTester>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ConstructionInputService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<ConstructionVisualService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<BuildingWorldInfoPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInstance(_settlementLabelSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<SettlementLabelService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<ConstructionSaveModule>()
                .AsSingle();

            Container.BindInterfacesTo<SaveModuleRegistrar<ConstructionSaveModule>>()
                .AsSingle()
                .NonLazy();

            // Система здоров'я будівель — реєструє IHealth при BuildingPlacedSignal
            Container.BindInterfacesAndSelfTo<BuildingHealthService>()
                .AsSingle()
                .NonLazy();

            QueueSceneDebugViewInjection();

            // Явний порядок Initialize() — виконується ПІСЛЯ GameMode (-10).
            Container.BindExecutionOrder<ConstructionService>(0);
            Container.BindExecutionOrder<ConstructionConfirmRequestRouter>(2);
            Container.BindExecutionOrder<ConstructionInputService>(5);
            Container.BindExecutionOrder<ConstructionVisualService>(10);
            Container.BindExecutionOrder<BuildingWorldInfoPresenter>(15);
            Container.BindExecutionOrder<SettlementLabelService>(20);
        }

        private void BindIntegratedProfiles()
        {
            if (_sceneContext?.SystemProfile?.EconomyRulesProfile != null)
            {
                Container.BindInstance(_sceneContext.SystemProfile.EconomyRulesProfile)
                    .IfNotBound();
            }

            if (_sceneContext?.SystemProfile?.FogOfWarSettings != null)
            {
                Container.BindInstance(_sceneContext.SystemProfile.FogOfWarSettings)
                    .IfNotBound();
            }
        }

        private void QueueSceneDebugViewInjection()
        {
            var debugViews = FindObjectsByType<ConstructionDebugSceneView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < debugViews.Length; i++)
            {
                if (debugViews[i] != null)
                    Container.QueueForInject(debugViews[i]);
            }
        }

        private int ResolveTownHallBuildRadiusFromEconomy()
        {
            if (TryResolveTownHallBuildRadiusFromProfile(_sceneContext?.SystemProfile?.EconomyRulesProfile, out int profileRadius))
            {
                Debug.Log($"[ConstructionInstaller] townHallBuildRadius resolved from ConstructionSystemProfile.EconomyRulesProfile.Settlement.MinTownHallDistance = {profileRadius}");
                return profileRadius;
            }

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

        private static bool TryResolveTownHallBuildRadiusFromProfile(ScriptableObject economyRulesProfile, out int radius)
        {
            radius = 0;
            if (economyRulesProfile == null)
                return false;

            PropertyInfo settlementProperty = economyRulesProfile.GetType().GetProperty("Settlement", BindingFlags.Instance | BindingFlags.Public);
            object settlement = settlementProperty?.GetValue(economyRulesProfile);
            if (settlement == null)
                return false;

            PropertyInfo minTownHallDistanceProperty = settlement.GetType().GetProperty("MinTownHallDistance", BindingFlags.Instance | BindingFlags.Public);
            if (minTownHallDistanceProperty?.PropertyType != typeof(int))
                return false;

            radius = Mathf.Max(0, (int)minTownHallDistanceProperty.GetValue(settlement));
            return true;
        }
    }
}
