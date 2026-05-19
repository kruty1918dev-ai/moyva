using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Високорівневий контракт multiplayer-операцій із домашнього меню.
    /// Залежності: інкапсулює lobby/network логіку для UI-панелей HomeMenu.
    /// </summary>
    public interface IHomeMultiplayerService
    {
        /// <summary>Приєднатися до лобі за його ідентифікатором.</summary>
        Task JoinToLobbyAsync(string lobbyId, CancellationToken ct = default);

        /// <summary>Створити нове лобі.</summary>
        Task CreateLobbyAsync(CancellationToken ct = default);

        /// <summary>Покинути поточне лобі.</summary>
        Task LeaveLobbyAsync(CancellationToken ct = default);

        /// <summary>Запустити гру з поточного лобі.</summary>
        Task StartGameAsync(CancellationToken ct = default);

        /// <summary>Отримати список доступних лобі для відображення у меню.</summary>
        Task<LobbyModelUIWrapper[]> GetAvailableLobbiesAsync(CancellationToken ct = default);
    }
}