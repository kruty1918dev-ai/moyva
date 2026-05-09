using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    public interface IMultiplayerModeSelector
    {
        NetworkProviderType CurrentMode { get; }
        NetworkProviderType EffectiveMode { get; }

        event Action<NetworkProviderType> OnModeChanged;

        Task SetModeAsync(NetworkProviderType mode, CancellationToken ct = default);
    }
}