using Kruty1918.Moyva.Shared.Common;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    /// <summary>
    /// Набір статичних перевірок готовності multiplayer-залежностей до виконання сценаріїв меню.
    /// Залежності: повертає <see cref="Result"/>, який далі споживають join/start-game сервіси.
    /// </summary>
    internal static class MultiplayerPreflightChecks
    {
        /// <summary>
        /// Перевірити, що join-flow має всі критичні залежності.
        /// </summary>
        public static Result ValidateForJoin(bool hasLobbyService, bool hasNetworkProvider, bool hasModeSelector)
        {
            // 1: Без lobby service неможливо знайти кімнату або виконати join на рівні лобі.
            if (!hasLobbyService)
                return Result.Fail(DomainErrorCode.NotFound, "Lobby service недоступний.");

            // 2: Без network provider неможливо встановити транспортне підключення після входу до лобі.
            if (!hasNetworkProvider)
                return Result.Fail(DomainErrorCode.NotFound, "Network provider недоступний.");

            // 3: Без mode selector не можна коректно визначити режим роботи multiplayer-стеку.
            if (!hasModeSelector)
                return Result.Fail(DomainErrorCode.Validation, "Mode selector недоступний.");

            // 4: Усі необхідні залежності доступні, join-flow можна продовжувати.
            return Result.Success();
        }

        /// <summary>
        /// Перевірити, що сесія готова до старту гри або синхронізації команд.
        /// </summary>
        public static Result ValidateSessionReadiness(bool hasLobbyService, bool hasGameStarter, bool hasCommandSync)
        {
            // 1: Без lobby service немає надійного джерела стану кімнати та складу гравців.
            if (!hasLobbyService)
                return Result.Fail(DomainErrorCode.NotFound, "Lobby service недоступний.");

            // 2: Без game starter неможливо перейти з HomeMenu до gameplay-сцени.
            if (!hasGameStarter)
                return Result.Fail(DomainErrorCode.NotFound, "Game starter недоступний.");

            // 3: Без command sync неможливо координувати мультиплеєрний старт сесії.
            if (!hasCommandSync)
                return Result.Fail(DomainErrorCode.NotFound, "Command sync service недоступний.");

            // 4: Сесія має всі залежності для подальших multiplayer-операцій.
            return Result.Success();
        }
    }
}
