using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.SaveSystem;
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

        public override void InstallBindings()
        {
            Container.BindInstance(_wfcDataSettings).AsSingle();
            Container.BindInstance(_riverConfig).AsSingle();
            Container.BindInstance(_noiseSettings).AsSingle();
            Container.BindInstance(_biomesSettings).AsSingle();
            Container.BindInstance(_generationRules).AsSingle();
            Container.BindInstance(_heightMapSettings).AsSingle();
            Container.Bind<IVirtualHeightMapGenerator>().To<VirtualHeightMapGenerator>().AsSingle();

            Container.Bind<IWFCService>().To<WFCService>().AsSingle();
            Container.Bind<IRiverPathfinder>().To<RiverPathfinder>().AsSingle();
            Container.Bind<INoiseProvider>().To<NoiseMapGeneratorService>().AsSingle();
            Container.Bind<IBiomeResolver>().To<BiomeResolver>().AsSingle();
            Container.Bind<IMapDataGenerator>().To<MapDataGenerator>().AsSingle();
            Container.Bind<IMapFeatureGenerator>().To<RiverFeatureGenerator>().AsTransient();
            Container.Bind<IMapFeatureGenerator>().To<WaterPostProcessor>().AsTransient();

            Container.BindInterfacesAndSelfTo<MapVisualInstantiator>().AsSingle();
            Container.Bind<ISaveModule>().To<GeneratedWorldSaveModule>().AsSingle();
        }

        override public void Start()
        {
            base.Start();

            Container.Resolve<IMapInstantiator>().BuildWorld();
        }
    }
}
