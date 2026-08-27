using Kruty1918.Moyva.Shared.Common;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    /// <summary>
    /// Нормалізована модель помилки мультиплеєра для показу користувачу.
    /// Залежності: будується з <see cref="DomainError"/> і передається в InfoPanel або інший UI.
    /// </summary>
    internal readonly struct MultiplayerUserFacingError
    {
        /// <summary>Стабільний користувацький код помилки.</summary>
        public readonly string ErrorCode;

        /// <summary>Основне повідомлення для користувача.</summary>
        public readonly string UserMessage;

        /// <summary>Коротка порада, що робити далі.</summary>
        public readonly string ActionHint;

        /// <summary>Трасувальний ідентифікатор для підтримки або логів.</summary>
        public readonly string TraceId;

        /// <summary>Створити користувацьке представлення multiplayer-помилки.</summary>
        public MultiplayerUserFacingError(string errorCode, string userMessage, string actionHint, string traceId)
        {
            // 1: Нормалізуємо код, щоб UI завжди мав стабільне значення навіть при порожньому input.
            ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? "MP-UNKNOWN" : errorCode;

            // 2: Захищаємося від null, щоб подальше форматування повідомлень було безпечним.
            UserMessage = userMessage ?? string.Empty;

            // 3: Аналогічно нормалізуємо підказку для користувача.
            ActionHint = actionHint ?? string.Empty;

            // 4: Зберігаємо trace id або порожній рядок, якщо він відсутній.
            TraceId = traceId ?? string.Empty;
        }

        /// <summary>
        /// Побудувати користувацьку помилку на основі доменного коду помилки.
        /// </summary>
        public static MultiplayerUserFacingError FromDomainError(DomainError error, string traceId)
        {
            // 1: Мапимо технічний доменний код у зрозумілий для користувача текст і стабільний зовнішній код.
            switch (error.Code)
            {
                case DomainErrorCode.WrongPassword:
                    return new MultiplayerUserFacingError("MP-JOIN-401", "Wrong room password.", "Check the password and try again.", traceId);
                case DomainErrorCode.NotFound:
                    return new MultiplayerUserFacingError("MP-JOIN-404", "Room was not found or is already closed.", "Refresh the list and choose another room.", traceId);
                case DomainErrorCode.Timeout:
                    return new MultiplayerUserFacingError("MP-NET-408", "Operation timed out.", "Check your network and try again.", traceId);
                case DomainErrorCode.Network:
                    return new MultiplayerUserFacingError("MP-NET-503", "Network operation failed.", "Try again in a few seconds.", traceId);
                case DomainErrorCode.Validation:
                    return new MultiplayerUserFacingError("MP-REQ-422", string.IsNullOrWhiteSpace(error.Message) ? "Invalid request." : error.Message, "Check the input parameters.", traceId);
                default:
                    return new MultiplayerUserFacingError("MP-UNKNOWN", string.IsNullOrWhiteSpace(error.Message) ? "Unknown multiplayer error." : error.Message, "Try again or re-enter the menu.", traceId);
            }
        }

        /// <summary>
        /// Зібрати фінальний текст для відображення у UI.
        /// </summary>
        public string BuildDisplayMessage()
        {
            // 1: Додаємо trace id лише тоді, коли він реально є, щоб не засмічувати повідомлення.
            var tracePart = string.IsNullOrWhiteSpace(TraceId) ? string.Empty : $"\nTraceId: {TraceId}";

            // 2: Додаємо action hint тільки для сценаріїв, де є корисна порада користувачу.
            var hintPart = string.IsNullOrWhiteSpace(ActionHint) ? string.Empty : $"\nHint: {ActionHint}";

            // 3: Повертаємо один форматований рядок/блок для прямого показу в InfoPanel.
            return $"[{ErrorCode}] {UserMessage}{hintPart}{tracePart}";
        }
    }
}
