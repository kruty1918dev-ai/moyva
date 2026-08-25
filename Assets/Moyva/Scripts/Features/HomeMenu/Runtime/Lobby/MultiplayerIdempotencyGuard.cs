using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    /// <summary>
    /// Захист від паралельного повторного входу в одну й ту саму multiplayer-операцію.
    /// Залежності: використовується HomeMenu multiplayer-сервісами для in-flight контролю.
    /// </summary>
    internal sealed class MultiplayerIdempotencyGuard
    {
        /// <summary>Набір ключів операцій, які зараз уже виконуються.</summary>
        private readonly HashSet<string> _inFlight = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Спробувати зареєструвати початок операції.
        /// </summary>
        /// <param name="key">Ключ операції.</param>
        /// <returns>True, якщо операція ще не виконувалася паралельно і тепер зареєстрована.</returns>
        public bool TryEnter(string key)
        {
            // 1: Порожній ключ вважаємо некоректним і не допускаємо до in-flight реєстру.
            if (string.IsNullOrWhiteSpace(key))
                return false;

            // 2: Add поверне false, якщо така операція вже позначена як активна.
            return _inFlight.Add(key);
        }

        /// <summary>
        /// Позначити завершення операції й прибрати її з in-flight набору.
        /// </summary>
        /// <param name="key">Ключ завершеної операції.</param>
        public void Exit(string key)
        {
            // 1: Порожній ключ ігноруємо, щоб не пошкодити стан guard'а.
            if (string.IsNullOrWhiteSpace(key))
                return;

            // 2: Прибираємо ключ, відкриваючи можливість повторного запуску цієї операції.
            _inFlight.Remove(key);
        }
    }
}
