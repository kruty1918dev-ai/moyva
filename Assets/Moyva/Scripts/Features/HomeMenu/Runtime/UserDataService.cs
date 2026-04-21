using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Реалізація <see cref="IUserDataService"/>, що видаляє всі слоти збереження
    /// (перебором у межах <see cref="MaxSlot"/>) та глобальний конфіг.
    /// </summary>
    internal sealed class UserDataService : IUserDataService
    {
        /// <summary>Максимальний розмір пулу слотів. Відповідає документованому діапазону SaveService.</summary>
        public const int MaxSlot = 99;

        private readonly ISaveService _saveService;
        private readonly IConfigService _configService;

        public event Action UserDataDeleted;

        public UserDataService(ISaveService saveService, IConfigService configService)
        {
            _saveService   = saveService   ?? throw new ArgumentNullException(nameof(saveService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        /// <inheritdoc/>
        public bool HasAnyUserData()
        {
            if (_configService.HasConfig()) return true;
            for (int slot = 0; slot <= MaxSlot; slot++)
                if (_saveService.HasSave(slot)) return true;
            return false;
        }

        /// <inheritdoc/>
        public int DeleteAllUserData()
        {
            int deleted = 0;

            for (int slot = 0; slot <= MaxSlot; slot++)
            {
                if (!_saveService.HasSave(slot)) continue;
                try
                {
                    _saveService.Delete(slot);
                    deleted++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UserDataService] Не вдалося видалити слот {slot}: {e.Message}");
                }
            }

            if (_configService.HasConfig())
            {
                try
                {
                    _configService.DeleteConfig();
                    deleted++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UserDataService] Не вдалося видалити конфіг: {e.Message}");
                }
            }

            // Також очищуємо налаштування аудіо у PlayerPrefs, бо це частина "користувацьких даних".
            foreach (AudioChannel c in Enum.GetValues(typeof(AudioChannel)))
                PlayerPrefs.DeleteKey($"Moyva.Audio.{c}");
            PlayerPrefs.DeleteKey("Moyva.Audio.Muted");
            PlayerPrefs.Save();

            UserDataDeleted?.Invoke();
            return deleted;
        }
    }
}
