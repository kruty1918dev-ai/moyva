using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Контракт запуску ігрової сцени з HomeMenu.
    /// Залежності: реалізується HomeMenuGameStarter і використовується сервісами старту/continue/lobby.
    /// </summary>
    public interface IHomeMenuGameStarter
    {
        /// <summary>Запустити перехід із меню до ігрової сцени.</summary>
        /// <param name="ct">Токен скасування запуску.</param>
        Task StartGameAsync(CancellationToken ct = default);
    }
}