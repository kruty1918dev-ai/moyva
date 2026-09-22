using System.Collections.Generic;
using System.Linq;
using Kruty1918.Audio;
using Kruty1918.JsonConfig;
using Zenject;

namespace Kruty1918.Moyva.Audio.Runtime
{
    /// <summary>
    /// Moyva composition adapter for the reusable audio package: loads the
    /// JSON-backed audio catalog / scene overrides / music profiles and wires
    /// service lifecycle (Initialize/Tick/Dispose) into Zenject.
    /// </summary>
    public static class AudioInstaller
    {
        private const string DefaultRegistryResourcePath = "MoyvaAudioRegistry";

        public static void Install(DiContainer container, AudioRegistrySO registry = null,
            IEnumerable<SceneMusicProfileSO> musicProfiles = null,
            SceneAudioOverridesSO sceneOverrides = null)
        {
            if (!container.HasBinding<AudioRegistrySO>())
            {
                // Serialized plain-class config fields deserialize as non-null but empty,
                // which would shadow the JSON source of truth — prefer JSON when present.
                AudioRegistrySO jsonRegistry = JsonConfigRuntime.GetLegacyResource<AudioRegistrySO>(DefaultRegistryResourcePath);
                registry = jsonRegistry ?? registry;
                if (registry != null)
                    container.BindInstance(registry).AsSingle();
            }

            if (!container.HasBinding<SceneAudioOverridesSO>())
            {
                SceneAudioOverridesSO jsonOverrides = JsonConfigRuntime.GetLegacyResource<SceneAudioOverridesSO>("MoyvaSceneAudioOverrides");
                sceneOverrides = jsonOverrides ?? sceneOverrides;
                if (sceneOverrides != null)
                    container.BindInstance(sceneOverrides).AsSingle();
            }

            if (!container.HasBinding<IAudioCatalog>())
                container.Bind<IAudioCatalog>()
                    .FromMethod(ctx => ctx.Container.TryResolve<AudioRegistrySO>())
                    .AsSingle();

            if (!container.HasBinding<IAudioSceneOverrides>())
                container.Bind<IAudioSceneOverrides>()
                    .FromMethod(ctx => ctx.Container.TryResolve<SceneAudioOverridesSO>())
                    .AsSingle();

            if (!container.HasBinding<IAudioService>())
            {
                container.BindInterfacesAndSelfTo<AudioService>()
                    .AsSingle()
                    .OnInstantiated<AudioService>((_, service) => service.Initialize())
                    .NonLazy();
                container.Bind<ITickable>()
                    .To<AudioServiceTickable>()
                    .AsSingle()
                    .NonLazy();
            }

            MusicInstaller.Install(container, musicProfiles);
        }

        private sealed class AudioServiceTickable : ITickable
        {
            private readonly AudioService _service;

            public AudioServiceTickable(AudioService service)
            {
                _service = service;
            }

            public void Tick() => _service.Tick();
        }
    }

    public static class MusicInstaller
    {
        /// <summary>
        /// Реєструє IMusicService. Виклик з AudioInstaller або окремого installer.
        /// profiles — список профілів, знайдених у Resources або прив'язаних вручну.
        /// </summary>
        public static void Install(DiContainer container, IEnumerable<SceneMusicProfileSO> profiles = null)
        {
            if (container.HasBinding<IMusicService>()) return;

            var list = new List<SceneMusicProfileSO>(profiles ?? System.Array.Empty<SceneMusicProfileSO>());
            // Serialized plain-class entries can be empty stubs (no clips); drop them so
            // they don't shadow the JSON source of truth.
            list.RemoveAll(p => p == null
                || (p.DefaultMusic?.Clip == null && p.EpicMusic?.Clip == null));
            if (list.Count == 0)
            {
                var found = JsonConfigRuntime.GetAllLegacyResources<SceneMusicProfileSO>("MusicProfiles");
                list.AddRange(found);
            }

            container.BindInstance(list).AsSingle();
            container.BindInstance(list.Cast<IMusicSceneProfile>().ToList()).AsSingle();
            container.BindInterfacesAndSelfTo<MusicService>()
                .AsSingle()
                .OnInstantiated<MusicService>((_, service) => service.Initialize())
                .NonLazy();
        }
    }
}
