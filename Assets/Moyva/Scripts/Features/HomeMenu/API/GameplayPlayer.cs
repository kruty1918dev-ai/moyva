namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Учасник ігрової сесії — мінімальний знімок гравця для шару геймплея.
    /// Залежності: формується menu/multiplayer сервісами та споживається gameplay startup pipeline.
    /// </summary>
    public readonly struct GameplayPlayer
    {
        /// <summary>Стабільний ідентифікатор гравця.</summary>
        public string PlayerId { get; }

        /// <summary>Display-ім'я гравця для UI.</summary>
        public string DisplayName { get; }

        /// <summary>True, якщо гравець є хостом сесії.</summary>
        public bool IsHost { get; }

        /// <summary>True, якщо гравець локальний для поточного клієнта.</summary>
        public bool IsLocal { get; }

        /// <summary>
        /// Створює immutable DTO гравця.
        /// </summary>
        /// <param name="playerId">Унікальний ідентифікатор гравця.</param>
        /// <param name="displayName">Ім'я, видиме в UI.</param>
        /// <param name="isHost">Прапорець хоста.</param>
        /// <param name="isLocal">Прапорець локального гравця.</param>
        public GameplayPlayer(string playerId, string displayName, bool isHost, bool isLocal)
        {
            PlayerId = playerId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            IsHost = isHost;
            IsLocal = isLocal;
        }
    }
}
