namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Стан списку кімнат на панелі Join Room.
    /// </summary>
    public enum RoomListStatus
    {
        /// <summary>Список актуальний, службових повідомлень немає.</summary>
        Ready,

        /// <summary>Триває оновлення/завантаження списку кімнат.</summary>
        Loading,

        /// <summary>Запит успішний, але кімнати відсутні.</summary>
        Empty,

        /// <summary>Оновлення списку завершилося помилкою.</summary>
        Error,

        /// <summary>Триває приєднання до кімнати; список тимчасово неінтерактивний.</summary>
        Joining
    }

    /// <summary>
    /// Необов'язковий boundary: view, що вміє показувати стан списку кімнат усередині панелі.
    /// </summary>
    public interface IRoomListStatusView
    {
        void SetRoomListStatus(RoomListStatus status, string message);
    }

    /// <summary>
    /// Зведений стан панелі лобі для презентації (без впливу на канонічний стан).
    /// </summary>
    public struct LobbyStatusInfo
    {
        /// <summary>Локальний гравець є хостом лобі.</summary>
        public bool IsHost;

        /// <summary>Хост може керувати учасниками (kick тощо) — бекенд підтримує.</summary>
        public bool CanManagePlayers;

        /// <summary>Чи можна натиснути Start Game.</summary>
        public bool CanStart;

        /// <summary>Причина, чому старт недоступний (або підказка готовності).</summary>
        public string StartReason;

        /// <summary>Назва кімнати.</summary>
        public string RoomName;

        /// <summary>Мітка мережі (LAN / Relay).</summary>
        public string NetworkLabel;

        /// <summary>Мітка приватності (Public / Private).</summary>
        public string PrivacyLabel;

        /// <summary>Кількість підключених гравців.</summary>
        public int PlayerCount;

        /// <summary>Місткість кімнати.</summary>
        public int MaxPlayers;

        /// <summary>Короткий опис світу, що буде згенерований.</summary>
        public string WorldSummary;
    }

    /// <summary>
    /// Необов'язковий boundary: view, що вміє показувати розширений стан лобі.
    /// </summary>
    public interface ILobbyStatusView
    {
        void SetLobbyStatus(LobbyStatusInfo status);
    }
}
