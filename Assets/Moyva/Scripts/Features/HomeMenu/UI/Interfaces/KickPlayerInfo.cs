namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// UI-модель учасника для панелі виключення гравців.
    /// </summary>
    public struct KickPlayerInfo
    {
        /// <summary>Стабільний ідентифікатор гравця.</summary>
        public string PlayerId;

        /// <summary>Ім'я гравця для відображення.</summary>
        public string DisplayName;

        /// <summary>True, якщо цей гравець є хостом.</summary>
        public bool IsHost;

        /// <summary>True, якщо це локальний гравець поточного клієнта.</summary>
        public bool IsLocalPlayer;

        /// <summary>True, якщо поточний користувач може виключити цього гравця.</summary>
        public bool CanKick;

        /// <summary>Додатковий статусний текст у UI.</summary>
        public string StatusLabel;
    }
}