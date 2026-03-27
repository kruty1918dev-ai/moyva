using Kruty1918.Moyva.Pathfinding.API;
using Zenject;

namespace Kruty1918.Moyva.Pathfinding.Runtime
{
    public class PathfinderInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IPathfinder>().To<Pathfinder>().AsSingle();
        }
    }
}