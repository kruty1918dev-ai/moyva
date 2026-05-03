using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface IJoinRoomPanelService
    {
        string LastJoinPanelName { get; }
        NetworkProviderType LastJoinProviderType { get; }

        Task<bool> PrepareForOpenAsync(CancellationToken ct = default);
        Task<bool> RefreshRoomListAsync(CancellationToken ct = default);
    }
}