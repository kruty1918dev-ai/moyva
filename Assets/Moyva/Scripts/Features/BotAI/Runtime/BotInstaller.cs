using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Optional scene-authored adapter.
    /// The real composition root lives in BotRuntimeBindings and is also installed
    /// by BootstrapInstaller, so direct Gameplay works without this component.
    /// </summary>
    public sealed class BotInstaller : MonoInstaller
    {
        public override void InstallBindings()
            => BotRuntimeBindings.Install(Container);
    }
}
