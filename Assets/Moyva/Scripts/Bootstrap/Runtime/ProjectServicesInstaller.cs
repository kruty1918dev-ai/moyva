using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Shared;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap
{
    /// <summary>
    /// ProjectContext-level installer for services shared across all scenes.
    /// </summary>
    public sealed class ProjectServicesInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Install shared services (connectivity etc.) first
            SharedInstaller.Install(Container);

            SaveSystemInstaller.Install(Container);
            MultiplayerInstaller.Install(Container);
        }
    }
}