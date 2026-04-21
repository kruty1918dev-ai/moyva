using System;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Сервіс керування гучністю та налаштуваннями звуку головного меню.
    /// Значення зберігаються між сесіями через <c>IConfigService</c> + <c>PlayerPrefs</c>.
    /// Діапазон усіх гучностей — [0..1] (лінійний), внутрішньо конвертується у dB для AudioMixer.
    /// </summary>
    public interface IAudioSettingsService
    {
        /// <summary>Подія: змінилась гучність каналу (канал, нове значення у [0..1]).</summary>
        event Action<AudioChannel, float> VolumeChanged;

        /// <summary>Подія: змінився стан приглушення майстер-каналу.</summary>
        event Action<bool> MuteChanged;

        /// <summary>Повертає лінійне значення гучності каналу у діапазоні [0..1].</summary>
        float GetVolume(AudioChannel channel);

        /// <summary>
        /// Встановлює нову гучність каналу. Значення буде обмежене у [0..1].
        /// Виконує <see cref="VolumeChanged"/> і синхронізацію з AudioMixer.
        /// </summary>
        void SetVolume(AudioChannel channel, float value);

        /// <summary>Повертає стан приглушення майстер-каналу.</summary>
        bool IsMuted { get; }

        /// <summary>Встановлює приглушення майстер-каналу.</summary>
        void SetMuted(bool muted);

        /// <summary>Зберігає поточні значення у перманентне сховище.</summary>
        void Save();

        /// <summary>Завантажує значення з перманентного сховища й застосовує до AudioMixer.</summary>
        void Load();

        /// <summary>Скидає всі значення до дефолтних і зберігає.</summary>
        void ResetToDefaults();
    }
}
