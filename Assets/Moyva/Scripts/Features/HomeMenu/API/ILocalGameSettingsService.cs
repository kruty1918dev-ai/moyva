using System;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface ILocalGameSettingsService
    {
        LocalGameSettings Settings { get; }
        string PlayerName { get; }
        float MasterVolume { get; }
        float MusicVolume { get; }
        float SfxVolume { get; }
        float UiVolume { get; }
        bool IsMuted { get; }

        event Action<LocalGameSettings> OnSettingsChanged;

        void SetPlayerName(string playerName);
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
        void SetUiVolume(float volume);
        void SetMuted(bool isMuted);
        void DeleteAllSaves();
    }
}
