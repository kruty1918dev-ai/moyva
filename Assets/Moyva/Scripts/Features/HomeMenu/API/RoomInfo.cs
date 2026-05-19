using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// DTO кімнати для списку лобі в HomeMenu.
    /// Залежності: наповнюється lobby/network шаром і читається JoinRoom/Lobby UI.
    /// </summary>
    public struct RoomInfo
    {
        /// <summary>Відображувана назва кімнати.</summary>
        public string RoomName;

        /// <summary>Join code для входу в кімнату.</summary>
        public string JoinCode;

        /// <summary>Глобальний ідентифікатор lobby.</summary>
        public string LobbyId;

        /// <summary>Ім'я хоста кімнати.</summary>
        public string HostDisplayName;

        /// <summary>Тип мережевого провайдера кімнати.</summary>
        public NetworkProviderType ProviderType;

        /// <summary>Поточна кількість гравців у кімнаті.</summary>
        public int CurrentPlayers;

        /// <summary>Максимальна кількість гравців у кімнаті.</summary>
        public int MaxPlayers;

        /// <summary>True, якщо кімната захищена паролем.</summary>
        public bool HasPassword;

        /// <summary>True, якщо кімната позначена як приватна (не видна у списку).</summary>
        public bool IsPrivate;

        /// <summary>Бітові прапорці можливостей кімнати.</summary>
        public RoomCapabilityFlags CapabilityFlags;

        /// <summary>True, якщо join code існує і не порожній.</summary>
        public bool HasJoinCode => !string.IsNullOrWhiteSpace(JoinCode);

        /// <summary>True, якщо lobby id існує і не порожній.</summary>
        public bool HasLobbyId => !string.IsNullOrWhiteSpace(LobbyId);

        /// <summary>
        /// Повертає найкращий доступний ключ для входу (join code або lobby id).
        /// </summary>
        public string DisplayKey
        {
            get
            {
                // 1: Пріоритетно показуємо join code як головний user-facing ідентифікатор.
                if (HasJoinCode) return JoinCode.Trim();
                // 2: Якщо join code відсутній — fallback на lobby id.
                if (HasLobbyId) return LobbyId.Trim();
                // 3: Якщо жодного ключа немає — повертаємо порожній рядок.
                return string.Empty;
            }
        }

        /// <summary>
        /// Форматує ідентифікатор кімнати для UI з підписом у випадку lobby id.
        /// </summary>
        public string DisplayIdentifier
        {
            get
            {
                // 1: Найкоротший варіант для UI — join code без префікса.
                if (HasJoinCode) return JoinCode.Trim();
                // 2: Lobby id віддаємо з явною міткою, щоб не плутати з join code.
                if (HasLobbyId) return $"LobbyId: {LobbyId.Trim()}";
                // 3: Повідомляємо користувачу, що ключ недоступний.
                return "Код недоступний";
            }
        }

        /// <summary>True, якщо кімната має хоча б один валідний ідентифікатор для входу.</summary>
        public bool IsJoinable => HasJoinCode || HasLobbyId;

        /// <summary>
        /// Повертає пріоритетний display name: хост, потім назва кімнати, потім fallback.
        /// </summary>
        public string HostOrRoomDisplayName
        {
            get
            {
                // 1: Якщо є ім'я хоста, використовуємо його як головний підпис.
                if (!string.IsNullOrWhiteSpace(HostDisplayName)) return HostDisplayName.Trim();
                // 2: Інакше fallback на назву кімнати.
                if (!string.IsNullOrWhiteSpace(RoomName)) return RoomName.Trim();
                // 3: Останній fallback для порожніх даних.
                return "Room";
            }
        }

        /// <summary>
        /// Людинозрозуміла мітка мережевого провайдера.
        /// </summary>
        public string ProviderLabel
        {
            get
            {
                // 1: Мапимо внутрішній enum провайдера в коротку UI-мітку.
                switch (ProviderType)
                {
                    case NetworkProviderType.Lan:
                        return "Local";
                    case NetworkProviderType.Relay:
                        return "Global";
                    case NetworkProviderType.WebSocket:
                        return "Global";
                    case NetworkProviderType.Offline:
                        return "Offline";
                    default:
                        // 2: Для невідомих/нових типів використовуємо ToString як безпечний fallback.
                        return ProviderType.ToString();
                }
            }
        }
    }
}
