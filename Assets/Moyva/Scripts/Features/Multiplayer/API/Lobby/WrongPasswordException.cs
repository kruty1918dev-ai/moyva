using System;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// Base class for typed multiplayer domain failures.
    /// Carries a short machine-readable <see cref="ErrorCode"/> that maps to
    /// <see cref="Kruty1918.Moyva.Shared.Common.DomainErrorCode"/> values.
    /// Defined locally to avoid a circular assembly dependency with Shared.
    /// </summary>
    public abstract class MultiplayerDomainException : Exception
    {
        public string ErrorCode { get; }

        protected MultiplayerDomainException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode ?? string.Empty;
        }
    }

    /// <summary>
    /// Викидається, коли наданий пароль не збігається з паролем кімнати.
    /// Код помилки: <c>MP-WRONG-PW</c>.
    /// </summary>
    public sealed class WrongPasswordException : MultiplayerDomainException
    {
        public WrongPasswordException() : base("MP-WRONG-PW", "Невірний пароль кімнати.") { }
        public WrongPasswordException(string message) : base("MP-WRONG-PW", message) { }
    }

    /// <summary>
    /// Викидається, коли кімната переповнена і нові гравці не можуть приєднатися.
    /// Код помилки: <c>MP-ROOM-FULL</c>.
    /// </summary>
    public sealed class RoomFullException : MultiplayerDomainException
    {
        public RoomFullException() : base("MP-ROOM-FULL", "Кімната переповнена.") { }
        public RoomFullException(string message) : base("MP-ROOM-FULL", message) { }
    }

    /// <summary>
    /// Викидається, коли гравець не має дозволу входити у кімнату.
    /// Код помилки: <c>MP-ACCESS-DENIED</c>.
    /// </summary>
    public sealed class RoomAccessDeniedException : MultiplayerDomainException
    {
        public string Reason { get; }

        public RoomAccessDeniedException(string reason = null)
            : base("MP-ACCESS-DENIED", string.IsNullOrWhiteSpace(reason) ? "Доступ до кімнати заборонено." : reason)
        {
            Reason = reason ?? string.Empty;
        }
    }

    /// <summary>
    /// Викидається, коли сесія застаріла або видалена хостом.
    /// Код помилки: <c>MP-SESSION-EXPIRED</c>.
    /// </summary>
    public sealed class SessionExpiredException : MultiplayerDomainException
    {
        public SessionExpiredException() : base("MP-SESSION-EXPIRED", "Ігрова сесія більше не існує.") { }
        public SessionExpiredException(string message) : base("MP-SESSION-EXPIRED", message) { }
    }

    /// <summary>
    /// Викидається при невиправній помилці мережевого транспорту.
    /// Код помилки: <c>MP-TRANSPORT-ERR</c>.
    /// </summary>
    public sealed class NetworkTransportException : MultiplayerDomainException
    {
        public NetworkTransportException(string message)
            : base("MP-TRANSPORT-ERR", message) { }

        public NetworkTransportException(string message, Exception inner)
            : base("MP-TRANSPORT-ERR", message)
        {
            Data["innerMessage"] = inner?.Message;
        }
    }
}

