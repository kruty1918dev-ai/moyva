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

            // Save system installer belongs to the SaveSystem library and is
            // safe to install here because it's a project-level orchestration.
            SaveSystemInstaller.Install(Container);

            // The composition root (ProjectServicesInstaller) is responsible for
            // orchestrating which feature libraries are installed at startup.
            // It's acceptable for the bootstrap to invoke a library's installer
            // so that the library can register its own dependencies in the
            // project's DI container. Call the multiplayer install helper to
            // ensure minimal switchable wrappers (ILobbyService / INetworkProvider)
            // are available synchronously for UI services during startup.
            MultiplayerInstaller.Install(Container);
        }
    }
}