using System.Diagnostics;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// DTO лобі для UI-рівня HomeMenu.
    /// Залежності: формується lobby сервісами й використовується LobbyPanelViewController та суміжним UI.
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public struct LobbyModelUIWrapper
    {
        /// <summary>Назва лобі для відображення в списку/заголовку.</summary>
        public string LobbyDisplayName;

        /// <summary>Стабільний ідентифікатор лобі.</summary>
        public string LobbyId { get; }

        /// <summary>Нікнейм хоста лобі.</summary>
        public string HostNickname { get; }

        /// <summary>Поточний склад учасників лобі.</summary>
        public PlayerUIWrapper[] CurrentPlayers { get; }

        /// <summary>Максимальна кількість учасників лобі.</summary>
        public int MaxPlayers { get; }

        /// <summary>Хеш пароля лобі (якщо лобі захищене).</summary>
        public string HashPassword;

        /// <summary>True, якщо для лобі задано пароль.</summary>
        public bool HasPassword => !string.IsNullOrWhiteSpace(HashPassword);

        /// <summary>True, якщо лобі вже заповнене.</summary>
        public bool IsFull => CurrentPlayers.Length >= MaxPlayers;

        /// <summary>
        /// Створити UI-модель лобі.
        /// </summary>
        public LobbyModelUIWrapper(
            string lobbyId,
            string hostNickname,
            PlayerUIWrapper[] currentPlayers,
            int maxPlayers,
            string hashPassword,
            string lobbyDisplayName)
        {
            LobbyId = lobbyId;
            HostNickname = hostNickname;
            CurrentPlayers = currentPlayers;
            MaxPlayers = maxPlayers;
            HashPassword = hashPassword;
            LobbyDisplayName = lobbyDisplayName ?? $"{hostNickname}'s Lobby";
        }

        /// <summary>
        /// Повертає рядок для DebuggerDisplay.
        /// </summary>
        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}