using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Audio.Runtime;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime;
using Kruty1918.Moyva.Shared;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap
{
    /// <summary>
    /// ProjectContext-level installer for services shared across all scenes.
    /// </summary>
    public sealed class ProjectServicesInstaller : MonoInstaller
    {
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";

        [Header("Audio")]
        [Tooltip("Реєстр звуків. Якщо порожньо — завантажується з Resources/MoyvaAudioRegistry.")]
        [SerializeField] private AudioRegistrySO _audioRegistry;

        [Tooltip("Per-scene overrides для звуків. Якщо порожньо — завантажується з Resources/MoyvaSceneAudioOverrides.")]
        [SerializeField] private SceneAudioOverridesSO _sceneOverrides;

        [Tooltip("Профілі музики сцен. Може бути порожнім — тоді музика не налаштовується автоматично.")]
        [SerializeField] private SceneMusicProfileSO[] _musicProfiles;

        public override void InstallBindings()
        {
            DiagnosticsInstaller.InstallProjectCore(Container);
            Container.BindInterfacesTo<ProjectDiagnosticsEnvironmentBootstrap>().AsSingle().NonLazy();
            Debug.Log($"{DirectDiagTag} ProjectServicesInstaller.InstallBindings mode={GameLaunchContext.Mode}, maxPlayers={GameLaunchContext.MaxPlayers}.");
            SharedInstaller.Install(Container);

            AudioInstaller.Install(Container, _audioRegistry, _musicProfiles, _sceneOverrides);

            SaveSystemInstaller.Install(Container);

            MultiplayerInstaller.Install(Container);
            Debug.Log($"{DirectDiagTag} ProjectServicesInstaller bound ISessionManager=via MultiplayerInstaller, IGameplaySession=not-bound-in-ProjectContext.");
        }

        private sealed class ProjectDiagnosticsEnvironmentBootstrap : IInitializable
        {
            private readonly IDiagnosticsEnvironmentState _environmentState;

            public ProjectDiagnosticsEnvironmentBootstrap(IDiagnosticsEnvironmentState environmentState)
            {
                _environmentState = environmentState;
            }

            public void Initialize()
            {
                _environmentState.MarkProjectContextInstalled(
                    $"observedFrom=ProjectServicesInstaller, mode={GameLaunchContext.Mode}, maxPlayers={GameLaunchContext.MaxPlayers}");
            }
        }
    }
}
