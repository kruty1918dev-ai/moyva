namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Полегшене представлення гравця для UI лобі та панелей HomeMenu.
    /// Залежності: використовується LobbyModelUIWrapper і view-компонентами списку гравців.
    /// </summary>
    public struct PlayerUIWrapper
    {
        /// <summary>Унікальний ідентифікатор гравця.</summary>
        public string PlayerId { get; }

        /// <summary>Нікнейм, який показується у UI.</summary>
        public string Nickname { get; }

        /// <summary>Створити DTO гравця для UI.</summary>
        public PlayerUIWrapper(string playerId, string nickname)
        {
            PlayerId = playerId;
            Nickname = nickname;
        }
    }
}