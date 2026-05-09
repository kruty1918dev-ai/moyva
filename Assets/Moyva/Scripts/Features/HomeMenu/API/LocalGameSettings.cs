using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Локальні налаштування гри, що зберігаються на пристрої гравця.
    /// Включає ім'я гравця, рівень загального звуку, музику, SFX, UI-звуки та прапорець mute.
    /// </summary>
    public struct LocalGameSettings
    {
        public string PlayerName;
        public float MasterVolume;
        public float MusicVolume;
        public float SfxVolume;
        public float UiVolume;
        public bool IsMuted;

        /// <summary>Сумісність зі старим API: зберігає попередні значення Music/Sfx/Ui за замовчуванням.</summary>
        public LocalGameSettings(string playerName, float masterVolume)
            : this(playerName, masterVolume, 0.7f, 0.9f, 0.9f, false)
        {
        }

        public LocalGameSettings(string playerName, float master, float music, float sfx, float ui, bool isMuted)
        {
            PlayerName = string.IsNullOrWhiteSpace(playerName) ? string.Empty : playerName.Trim();
            MasterVolume = Mathf.Clamp01(master);
            MusicVolume = Mathf.Clamp01(music);
            SfxVolume = Mathf.Clamp01(sfx);
            UiVolume = Mathf.Clamp01(ui);
            IsMuted = isMuted;
        }

        public LocalGameSettings WithPlayerName(string playerName) =>
            new LocalGameSettings(playerName, MasterVolume, MusicVolume, SfxVolume, UiVolume, IsMuted);
        public LocalGameSettings WithMaster(float v) =>
            new LocalGameSettings(PlayerName, v, MusicVolume, SfxVolume, UiVolume, IsMuted);
        public LocalGameSettings WithMusic(float v) =>
            new LocalGameSettings(PlayerName, MasterVolume, v, SfxVolume, UiVolume, IsMuted);
        public LocalGameSettings WithSfx(float v) =>
            new LocalGameSettings(PlayerName, MasterVolume, MusicVolume, v, UiVolume, IsMuted);
        public LocalGameSettings WithUi(float v) =>
            new LocalGameSettings(PlayerName, MasterVolume, MusicVolume, SfxVolume, v, IsMuted);
        public LocalGameSettings WithMuted(bool muted) =>
            new LocalGameSettings(PlayerName, MasterVolume, MusicVolume, SfxVolume, UiVolume, muted);
    }
}
