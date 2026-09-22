using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.SaveSystem;
using Kruty1918.Moyva.Audio.Runtime;
using Kruty1918.Moyva.Shared;
using UnityEngine;
using Zenject;
using Kruty1918.Moyva.SaveSystem;

namespace Kruty1918.Moyva.Bootstrap
{
    /// <summary>
    /// ProjectContext-level installer for services shared across all scenes.
    /// </summary>
    public sealed class ProjectServicesInstaller : MonoInstaller
    {
        [Header("Audio")]
        [Tooltip("Реєстр звуків. Якщо порожньо — завантажується з Resources/MoyvaAudioRegistry.")]
        [SerializeField] private AudioRegistrySO _audioRegistry;

        [Tooltip("Per-scene overrides для звуків. Якщо порожньо — завантажується з Resources/MoyvaSceneAudioOverrides.")]
        [SerializeField] private SceneAudioOverridesSO _sceneOverrides;

        [Tooltip("Профілі музики сцен. Може бути порожнім — тоді музика не налаштовується автоматично.")]
        [SerializeField] private SceneMusicProfileSO[] _musicProfiles;

        public override void InstallBindings()
        {
            SharedInstaller.Install(Container);

            AudioInstaller.Install(Container, _audioRegistry, _musicProfiles, _sceneOverrides);

            Kruty1918.Moyva.GameAudio.Runtime.GameAudioInstaller.InstallShared(Container);

            SaveSystemInstaller.Install(Container);

            MultiplayerInstaller.Install(Container);
        }
    }
}
