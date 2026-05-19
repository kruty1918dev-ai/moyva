using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Контракт сервісу, який координує безпечний вихід із застосунку.
    /// Залежності: використовується навігацією меню, confirmation flow і стартовими сценаріями.
    /// </summary>
    public interface IApplicationQuitHandler
    {
        /// <summary>
        /// Асинхронно завершує застосунок, якщо переданий предикат підтверджує можливість виходу.
        /// </summary>
        /// <param name="match">Асинхронна умова, яка повертає true, якщо вихід дозволений.</param>
        /// <param name="ct">Токен скасування операції.</param>
        Task QuitApplicationIfAsync(Func<Task<bool>> match, CancellationToken ct = default);
    }
}