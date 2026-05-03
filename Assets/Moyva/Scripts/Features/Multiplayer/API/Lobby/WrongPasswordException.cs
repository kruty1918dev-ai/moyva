using System;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// Викидається, коли наданий пароль не збігається з паролем кімнати.
    /// </summary>
    public sealed class WrongPasswordException : Exception
    {
        public WrongPasswordException() : base("Невірний пароль кімнати.") { }
        public WrongPasswordException(string message) : base(message) { }
    }
}
