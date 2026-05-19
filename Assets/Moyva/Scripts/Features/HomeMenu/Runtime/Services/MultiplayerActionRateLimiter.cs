using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    /// <summary>
    /// Допоміжний обмежувач частоти дій у multiplayer-flow.
    /// Залежності: використовується сервісами HomeMenu для захисту від занадто частих повторних натискань.
    /// </summary>
    internal sealed class MultiplayerActionRateLimiter
    {
        /// <summary>Час останнього дозволеного виклику для кожного ключа дії.</summary>
        private readonly Dictionary<string, DateTime> _lastAt = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Перевірити, чи можна виконати дію зараз з огляду на мінімальний інтервал.
        /// </summary>
        /// <param name="key">Ключ дії, за яким ведеться throttling.</param>
        /// <param name="minInterval">Мінімальний дозволений інтервал між двома викликами.</param>
        /// <returns>True, якщо дію дозволено виконати зараз.</returns>
        public bool Allow(string key, TimeSpan minInterval)
        {
            // 1: Порожній ключ не throttling-имо, щоб не блокувати загальні або службові виклики.
            if (string.IsNullOrWhiteSpace(key))
                return true;

            // 2: Беремо поточний UTC-час для стабільного порівняння без залежності від локальної часової зони.
            var now = DateTime.UtcNow;

            // 3: Якщо ключ уже виконувався нещодавно і інтервал ще не минув, забороняємо повтор.
            if (_lastAt.TryGetValue(key, out var last) && now - last < minInterval)
                return false;

            // 4: Фіксуємо поточний момент як останній дозволений запуск для цього ключа.
            _lastAt[key] = now;

            // 5: Дозволяємо виконання, бо або це перший виклик, або інтервал уже минув.
            return true;
        }
    }
}
