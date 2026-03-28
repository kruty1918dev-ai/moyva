using Kruty1918.Moyva.Animations.API;
using Zenject;

namespace Kruty1918.Moyva.Animations.Runtime
{
    public class AnimationsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IMovementAnimationService>()
                .To<MovementAnimationService>()
                .AsSingle();
        }
    }
}