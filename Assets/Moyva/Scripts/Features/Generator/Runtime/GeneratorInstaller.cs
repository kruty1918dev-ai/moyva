using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using Kruty1918.Moyva.SaveSystem;
using GiantGrey.TileWorldCreator;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
	public class GeneratorInstaller : MonoInstaller
	{
		[SerializeField] private GenerationRules _generationRules;
		[SerializeField] private HeightMapSettings _heightMapSettings;
		[SerializeField] private RiverDataConfig _riverConfig;
		[SerializeField] private DataNoiseSettings _noiseSettings;
		[SerializeField] private DataBiomesSettings _biomesSettings;
		[SerializeField] private WFCDataSettings _wfcDataSettings;
		[SerializeField] private MapObjectRegistrySO _mapObjectRegistry;

		[Header("Graph-Based Generator")]
		[SerializeField] private GraphAsset _graphAsset;

		[Header("Water Runtime Setup")]
		[SerializeField] private WaterLayerMaterialSettings _waterLayerMaterialSettings = new();

		[Header("TileWorldCreator Integration")]
		[SerializeField] private bool _useTileWorldCreatorVisuals;
		[SerializeField] private TileWorldCreatorManager _tileWorldCreatorManager;
		[SerializeField] private TileWorldCreatorIdMappingSO _tileWorldCreatorIdMapping;
		[SerializeField] private TileWorldCreatorBuildOptions _tileWorldCreatorBuildOptions = new();

		[Header("TileWorldCreator Graph Pipeline (NEW)")]
		[Tooltip("Граф компілюється у TWC Configuration, а генерацію виконує TileWorldCreator. " +
			"Має пріоритет над класичним граф-генератором. Потребує GraphAsset + TileWorldCreatorManager.")]
		[SerializeField] private bool _useTwcGraphPipeline;

		public override void InstallBindings()
		{
			var graphAsset = ResolveGraphAsset();

			Container.BindInstance(_mapObjectRegistry).AsSingle();
			Container.BindInstance(_wfcDataSettings).AsSingle();
			Container.BindInstance(_riverConfig).AsSingle();
			Container.BindInstance(_noiseSettings).AsSingle();
			Container.BindInstance(_biomesSettings).AsSingle();
			Container.BindInstance(_generationRules).AsSingle();
			Container.BindInstance(_heightMapSettings).AsSingle();

			if (_useTwcGraphPipeline && graphAsset != null && _tileWorldCreatorManager != null)
			{
				// Новий конвеєр: граф -> TWC Configuration -> TileWorldCreator будує мапу.
				Container.BindInstance(graphAsset).AsSingle();
				Container.BindInstance(_tileWorldCreatorManager).IfNotBound();
				if (_tileWorldCreatorManager.configuration != null)
					Container.BindInstance(_tileWorldCreatorManager.configuration).IfNotBound();
				Container.BindInterfacesAndSelfTo<GraphTwcMapDataGenerator>().AsSingle();
			}
			else
			{
				Debug.LogWarning("[GeneratorInstaller] TWC graph pipeline is disabled or incomplete. Binding disabled fallback map generator.");
				Container.Bind<IMapDataGenerator>().To<DisabledMapDataGenerator>().AsSingle();
			}

			Container.Bind<IMapObjectRegistryService>().To<MapObjectRegistryService>().AsSingle();
			Container.Bind<IMapObjectVisualRegistryService>().To<MapObjectVisualRegistryService>().AsSingle();
			Container.Bind<IMapLayerRegistry>().To<MapLayerRegistry>().AsSingle();
			Container.Bind<IGeneratorDataRegistry>().To<GeneratorDataRegistry>().AsSingle();
			Container.Bind<IGeneratorTerrainLevelService>().To<GeneratorTerrainLevelService>().AsSingle();
			Container.Bind<IGeneratedTerrainLevelQuery>().To<GeneratedTerrainLevelQueryService>().AsSingle();

			if (_useTileWorldCreatorVisuals && _tileWorldCreatorManager != null && _tileWorldCreatorIdMapping != null)
			{
				Container.BindInstance(_tileWorldCreatorManager).IfNotBound();
				Container.BindInstance(_tileWorldCreatorIdMapping).AsSingle();
				Container.BindInstance(_tileWorldCreatorBuildOptions ?? new TileWorldCreatorBuildOptions()).AsSingle();
				Container.Bind<TileWorldCreatorWorldBuildBridge>().AsSingle();

				// Прокидаємо TWC Configuration з менеджера як окремий binding,
				// щоб граф і будь-який внутрішній сервіс міг резолвити його через DI.
				if (_tileWorldCreatorManager.configuration != null)
					Container.BindInstance(_tileWorldCreatorManager.configuration).IfNotBound();
			}

			Container.BindInterfacesAndSelfTo<MapVisualInstantiator>().AsSingle();
			Container.BindInterfacesAndSelfTo<GeneratedWorldSaveModule>().AsSingle();
			Container.BindInterfacesTo<SaveModuleRegistrar<GeneratedWorldSaveModule>>().AsSingle().NonLazy();
			Container.BindInstance(_waterLayerMaterialSettings).AsSingle();
		}

		private GraphAsset ResolveGraphAsset()
		{
			if (_graphAsset != null)
				return _graphAsset;

			if (_tileWorldCreatorManager != null
				&& _tileWorldCreatorManager.TryGetComponent<MoyvaTileWorldCreatorGraphBinding>(out var binding)
				&& binding.GraphAsset != null)
			{
				return binding.GraphAsset;
			}

			return null;
		}

		public override void Start()
		{
			base.Start();

			try
			{
				if (GameLaunchContext.TryGetSeed(out int launchSeed))
					GlobalSeed.Set(launchSeed);

				Debug.Log("[GeneratorInstaller] Старт побудови світу...");
				Container.Resolve<IMapInstantiator>().BuildWorld();
				Debug.Log("[GeneratorInstaller] Побудову світу завершено.");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[GeneratorInstaller] BuildWorld впав: {ex}");
				throw;
			}
		}
	}
}
