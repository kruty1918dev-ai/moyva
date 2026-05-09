using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface IHomeMultiplayerService
    {
        Task JoinToLobbyAsync(string lobbyId, CancellationToken ct = default);
        Task CreateLobbyAsync(CancellationToken ct = default);
        Task LeaveLobbyAsync(CancellationToken ct = default);
        Task StartGameAsync(CancellationToken ct = default);
        Task<LobbyModelUIWrapper[]> GetAvailableLobbiesAsync(CancellationToken ct = default);
    }
}