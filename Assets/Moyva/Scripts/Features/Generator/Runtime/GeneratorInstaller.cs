using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Visuals;
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
        [SerializeField] private bool _useGraphGenerator;
        [SerializeField] private GraphAsset _graphAsset;

        [Header("Tile Rendering (Optional)")]
        [SerializeField] private Material _waterMaterial;
        [SerializeField] private Material _tileBlendMaterial;
        [SerializeField] private TileTextureAtlasSO _tileTextureAtlas;
        [SerializeField] private string[] _waterTileIds = { "water" };

        public override void InstallBindings()
        {
            Container.BindInstance(_wfcDataSettings).AsSingle();
            Container.BindInstance(_riverConfig).AsSingle();
            Container.BindInstance(_noiseSettings).AsSingle();
            Container.BindInstance(_biomesSettings).AsSingle();
            Container.BindInstance(_generationRules).AsSingle();
            Container.BindInstance(_heightMapSettings).AsSingle();
            Container.BindInstance(_mapObjectRegistry).AsSingle();
            Container.Bind<IVirtualHeightMapGenerator>().To<VirtualHeightMapGenerator>().AsSingle();

            Container.Bind<IWFCService>().To<WFCService>().AsSingle();
            Container.Bind<IRiverPathfinder>().To<RiverPathfinder>().AsSingle();
            Container.Bind<INoiseProvider>().To<NoiseMapGeneratorService>().AsSingle();
            Container.Bind<IBiomeResolver>().To<BiomeResolver>().AsSingle();

            if (_useGraphGenerator && _graphAsset != null)
            {
                Container.BindInstance(_graphAsset).AsSingle();
                Container.Bind<IGraphRunner>().To<GraphRunner>().AsSingle();
                Container.Bind<IMapDataGenerator>()
                    .To<GraphBasedMapDataGenerator>().AsSingle();
            }
            else
            {
                Container.Bind<IMapDataGenerator>()
                    .To<MapDataGenerator>().AsSingle();
            }

            Container.Bind<IMapFeatureGenerator>().To<RiverFeatureGenerator>().AsTransient();
            Container.Bind<IMapFeatureGenerator>().To<WaterPostProcessor>().AsTransient();
            Container.Bind<IMapObjectRegistryService>().To<MapObjectRegistryService>().AsSingle();
            Container.Bind<IMapLayerRegistry>().To<MapLayerRegistry>().AsSingle();

            if (_waterMaterial != null)
                Container.BindInstance(_waterMaterial).WithId("WaterMaterial");

            if (_tileBlendMaterial != null)
                Container.BindInstance(_tileBlendMaterial).WithId("TileBlendMaterial");

            if (_tileTextureAtlas != null)
                Container.BindInstance(_tileTextureAtlas).AsSingle();

            if (_waterTileIds != null && _waterTileIds.Length > 0)
                Container.BindInstance(_waterTileIds).WithId("WaterTileIds");

            if (_waterMaterial == null)
                Debug.LogWarning("[GeneratorInstaller] Water material is not assigned. Water shader fallback will be used.");

            if (_tileBlendMaterial == null)
                Debug.LogWarning("[GeneratorInstaller] Tile blend material is not assigned. Terrain blend shader fallback will be used.");

            if (_tileTextureAtlas == null)
                Debug.LogWarning("[GeneratorInstaller] Tile texture atlas is not assigned. Terrain blend shader fallback will be used.");

            if (_waterTileIds == null || _waterTileIds.Length == 0)
                Debug.LogWarning("[GeneratorInstaller] Water tile IDs are not configured. Falling back to 'water'.");

            Container.BindInterfacesAndSelfTo<MapVisualInstantiator>().AsSingle();
            Container.Bind<ISaveModule>().To<GeneratedWorldSaveModule>().AsSingle();
        }

        public override void Start()
        {
            base.Start();

            try
            {
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
