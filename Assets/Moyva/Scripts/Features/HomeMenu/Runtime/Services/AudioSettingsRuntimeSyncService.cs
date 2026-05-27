using System;
using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.Audio.Runtime;
using Kruty1918.Moyva.HomeMenu.API;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    internal sealed class AudioSettingsRuntimeSyncService : IInitializable, IDisposable
    {
        private ILocalGameSettingsService _settingsService;
        private IAudioService _audioService;
        private IMusicService _musicService;

        [Inject]
        private void Construct(
            [InjectOptional] ILocalGameSettingsService settingsService,
            [InjectOptional] IAudioService audioService,
            [InjectOptional] IMusicService musicService)
        {
            _settingsService = settingsService;
            _audioService = audioService;
            _musicService = musicService;
        }

        public void Initialize()
        {
            if (_settingsService == null)
                return;

            _settingsService.OnSettingsChanged -= OnSettingsChanged;
            _settingsService.OnSettingsChanged += OnSettingsChanged;
            Apply(_settingsService.Settings);
        }

        public void Dispose()
        {
            if (_settingsService != null)
                _settingsService.OnSettingsChanged -= OnSettingsChanged;
        }

        private void OnSettingsChanged(LocalGameSettings settings)
        {
            Apply(settings);
        }

        private void Apply(LocalGameSettings settings)
        {
            float effectiveMusicVolume = settings.IsMuted ? 0f : settings.MusicVolume;
            float effectiveSfxVolume = settings.IsMuted ? 0f : settings.SfxVolume;
            float effectiveUiVolume = settings.IsMuted ? 0f : settings.UiVolume;

            _audioService?.SetBusVolume(AudioBus.Music, effectiveMusicVolume);
            _audioService?.SetBusVolume(AudioBus.Sfx, effectiveSfxVolume);
            _audioService?.SetBusVolume(AudioBus.Ui, effectiveUiVolume);

            if (_musicService == null)
                return;

            _musicService.SetDefaultVolume(effectiveMusicVolume);
            _musicService.SetEpicVolume(effectiveMusicVolume);
        }
    }
}