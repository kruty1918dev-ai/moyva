using System;
using System.Security.Cryptography;
using System.Text;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// Утиліта для хешування паролів кімнат (SHA-256, hex lowercase).
    /// Сирий пароль ніколи не зберігається і не передається по мережі.
    /// </summary>
    public static class LobbyPasswordHasher
    {
        private const string Salt = "moyva-lobby-v1";

        /// <summary>
        /// Повертає SHA-256 hex від рядка <paramref name="password"/> з префіксом-сіллю.
        /// Повертає <see cref="string.Empty"/> для null/whitespace.
        /// </summary>
        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(Salt + ":" + password);
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(bytes);

            var sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// Звіряє сирий <paramref name="password"/> з раніше обчисленим <paramref name="storedHash"/>.
        /// Якщо <paramref name="storedHash"/> порожній — кімната вважається без пароля (повертає true для будь-якого).
        /// </summary>
        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return true;

            var candidate = Hash(password);
            return string.Equals(candidate, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
