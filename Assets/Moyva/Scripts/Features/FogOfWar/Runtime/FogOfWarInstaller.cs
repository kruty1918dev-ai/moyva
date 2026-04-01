using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    public class FogOfWarInstaller : MonoInstaller
    {
        [SerializeField] private FogOfWarSettings _settings;

        public override void InstallBindings()
        {
            if (_settings != null)
                Container.BindInstance(_settings).AsSingle();

            Container.Bind<IFogSaveDataProvider>().To<FogSaveDataStub>().AsSingle();
            Container.Bind<IFogVisibilityResolver>().To<FogVisibilityResolver>().AsSingle();
            Container.Bind<IFogTextureUpdater>().To<FogTextureUpdater>().AsSingle();

            Container.BindInterfacesAndSelfTo<FogOfWarService>()
                .AsSingle()
                .NonLazy();

            Container.BindExecutionOrder<FogOfWarService>(-5);
        }
    }
}
