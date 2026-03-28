using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Units.API;

namespace Kruty1918.Moyva.Units.Runtime
{
    public class UnitsInstaller : MonoInstaller
    {
        [SerializeField] private UnitRegistrySO _unitRegistry;

        public override void InstallBindings()
        {
            Container.BindInstance(_unitRegistry).AsSingle();

            Container.BindInterfacesAndSelfTo<UnitService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IUnitFactory>()
                .To<UnitFactory>()
                .AsSingle();
        }
    }
}