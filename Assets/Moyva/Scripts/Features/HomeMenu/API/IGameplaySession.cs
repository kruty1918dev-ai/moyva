using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.WorldCreation.API;

namespace Kruty1918.Moyva.HomeMenu.API
{

    /// <summary>
    /// Контейнер стану ігрової сесії, заповнюється до завантаження сцени геймплея.
    /// Геймплейний шар має споживати лише цей інтерфейс — без посилань на конкретні мережеві сервіси.
    /// </summary>
    public interface IGameplaySession
    {
        /// <summary>True, якщо локальний гравець є хостом поточної сесії.</summary>
        bool IsHost { get; }

        /// <summary>Поточний мережевий режим (Relay/Lan/Offline тощо).</summary>
        NetworkProviderType Mode { get; }

        /// <summary>Налаштування світу, обрані хостом і розіслані всім гравцям.</summary>
        WorldSettingsDto WorldSettings { get; }

        /// <summary>Список учасників сесії.</summary>
        IReadOnlyList<GameplayPlayer> Players { get; }

        /// <summary>Локальний гравець.</summary>
        GameplayPlayer LocalPlayer { get; }

        /// <summary>Хост сесії.</summary>
        GameplayPlayer Host { get; }

        /// <summary>Заповнюється сервісом перед StartGame; не використовується геймплейним шаром.</summary>
        void Apply(NetworkProviderType mode, WorldSettingsDto worldSettings, IReadOnlyList<GameplayPlayer> players, string localPlayerId);

        /// <summary>Очистити стан (наприклад, при поверненні в меню).</summary>
        void Clear();
    }
}
