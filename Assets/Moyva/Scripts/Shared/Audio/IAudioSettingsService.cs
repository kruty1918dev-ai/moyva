using System;

namespace Kruty1918.Moyva.Shared.Audio
{
    public interface IAudioSettingsService
    {
        float MasterVolume { get; }
        float MusicVolume { get; }
        float SfxVolume { get; }

        event Action<float, float, float> OnVolumesChanged;

        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
    }
}