namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Легка UI-модель користувача в лобі.
    /// </summary>
    public struct LobbyUserInfo
    {
        /// <summary>Ім'я користувача для UI.</summary>
        public string UserName;

        /// <summary>Числовий ідентифікатор користувача.</summary>
        public int UserId;

        /// <summary>True, якщо цей користувач є хостом лобі.</summary>
        public bool IsHost;

        /// <summary>True, якщо цей запис описує локального гравця.</summary>
        public bool IsLocal;
    }
}