namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Утиліта нормалізації цілі входу до кімнати.
    /// Залежності: використовується join-flow сервісами для уніфікованого визначення JoinCode/LobbyId.
    /// </summary>
    public static class JoinRoomResolver
    {
        /// <summary>
        /// Розпізнає тип цілі входу з ручного текстового вводу.
        /// </summary>
        /// <param name="input">Рядок, введений користувачем.</param>
        /// <returns>Нормалізована ціль входу.</returns>
        public static JoinRoomTarget FromManualInput(string input)
        {
            // 1: Нормалізуємо пробіли по краях для стабільного парсингу.
            var value = input?.Trim();

            // 2: Порожній ввід означає відсутність валідної цілі.
            if (string.IsNullOrWhiteSpace(value))
                return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);

            // 3: LAN-префікс вважаємо явним join code.
            if (value.StartsWith("lan:", System.StringComparison.OrdinalIgnoreCase))
                return new JoinRoomTarget(JoinRoomTargetKind.JoinCode, value);

            // Relay join codes are 6-12 chars; longer manual input is treated as lobby id.
            // 4: Довгі значення частіше відповідають lobby id.
            if (value.Length > 12)
                return new JoinRoomTarget(JoinRoomTargetKind.LobbyId, value);

            // 5: Інакше інтерпретуємо значення як join code.
            return new JoinRoomTarget(JoinRoomTargetKind.JoinCode, value);
        }

        /// <summary>
        /// Будує ціль входу на основі елемента кімнати зі списку.
        /// </summary>
        /// <param name="room">Модель кімнати.</param>
        /// <returns>Нормалізована ціль входу.</returns>
        public static JoinRoomTarget FromRoom(RoomInfo room)
        {
            // 1: Пріоритетно використовуємо join code, якщо він присутній.
            if (room.HasJoinCode)
                return new JoinRoomTarget(JoinRoomTargetKind.JoinCode, room.JoinCode.Trim());

            // 2: Якщо join code відсутній — fallback на lobby id.
            if (room.HasLobbyId)
                return new JoinRoomTarget(JoinRoomTargetKind.LobbyId, room.LobbyId.Trim());

            // 3: Якщо дані кімнати не містять валідного ідентифікатора — повертаємо None.
            return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);
        }
    }
}