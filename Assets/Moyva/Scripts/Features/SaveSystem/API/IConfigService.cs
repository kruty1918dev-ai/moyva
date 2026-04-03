using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Сервіс для збереження глобального конфігу (налаштування, локалізація, тощо).
    /// На відміну від SaveService (слоти), ConfigService має один файл config.mvs.
    /// </summary>
    public interface IConfigService : IInitializable, IDisposable
    {
        /// <summary>Зберегти конфіг з модулями. Виконується атомарно.</summary>
        void SaveConfig(List<ISaveModule> modules);

        /// <summary>Завантажити конфіг у модулі.</summary>
        void LoadConfig(List<ISaveModule> modules);

        /// <summary>Перевірити, чи існує config.mvs.</summary>
        bool HasConfig();

        /// <summary>Видалити config.mvs.</summary>
        void DeleteConfig();

        /// <summary>Отримати інформацію про config файл (розмір, дата).</summary>
        SaveSlotInfo GetConfigInfo();
    }
}
