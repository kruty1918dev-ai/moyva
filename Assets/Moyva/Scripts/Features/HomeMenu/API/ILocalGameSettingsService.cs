using System;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Контракт локальних налаштувань гравця, що зберігаються на пристрої.
    /// Залежності: використовується GameSettingsPanelService, LocalGameSettingsService та стартом multiplayer/gameplay.
    /// </summary>
    public interface ILocalGameSettingsService
    {
        /// <summary>Повний знімок локальних налаштувань.</summary>
        LocalGameSettings Settings { get; }

        /// <summary>Ім'я гравця.</summary>
        string PlayerName { get; }

        /// <summary>Гучність master-каналу.</summary>
        float MasterVolume { get; }

        /// <summary>Гучність музики.</summary>
        float MusicVolume { get; }

        /// <summary>Гучність звукових ефектів.</summary>
        float SfxVolume { get; }

        /// <summary>Гучність UI-звуків.</summary>
        float UiVolume { get; }

        /// <summary>Прапорець повного вимкнення звуку.</summary>
        bool IsMuted { get; }

        /// <summary>Подія, що сповіщає про зміну локальних налаштувань.</summary>
        event Action<LocalGameSettings> OnSettingsChanged;

        /// <summary>Змінити ім'я гравця.</summary>
        void SetPlayerName(string playerName);

        /// <summary>Змінити master-гучність.</summary>
        void SetMasterVolume(float volume);

        /// <summary>Змінити гучність музики.</summary>
        void SetMusicVolume(float volume);

        /// <summary>Змінити гучність SFX.</summary>
        void SetSfxVolume(float volume);

        /// <summary>Змінити гучність UI.</summary>
        void SetUiVolume(float volume);

        /// <summary>Увімкнути або вимкнути mute.</summary>
        void SetMuted(bool isMuted);

        /// <summary>Видалити всі локальні збереження гри.</summary>
        void DeleteAllSaves();
    }
}
