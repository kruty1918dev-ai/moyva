using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    public class UnitsInstaller : MonoInstaller
    {
        [SerializeField] private UnitRegistrySO _unitRegistry;

        public override void InstallBindings()
        {
            Container.BindInstance(_unitRegistry).AsSingle();
            Container.BindInterfacesAndSelfTo<UnitService>()
                .AsSingle();
        }
    }
}