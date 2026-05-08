using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    public enum NetworkMode { Solo, Lan, Global }

    /// <summary>
    /// High-level controller that orchestrates runtime switching between Solo / LAN / Global modes.
    /// UI should call SetModeAsync to change mode. Auto-switching on internet availability is enabled.
    /// </summary>
    public sealed class NetworkModeController
    {
        private readonly SwitchableNetworkProvider _network;
        private readonly SwitchableLobbyService _lobby;
        private readonly IMultiplayerState _state;
        private NetworkMode _current = NetworkMode.Solo;
        private readonly CancellationTokenSource _probeCts = new CancellationTokenSource();

        public NetworkModeController(SwitchableNetworkProvider network, SwitchableLobbyService lobby, IMultiplayerState state)
        {
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _lobby = lobby ?? throw new ArgumentNullException(nameof(lobby));
            _state = state ?? throw new ArgumentNullException(nameof(state));

            // Start background probe to auto-switch to Global if internet appears and no session active
            _ = ProbeLoopAsync(_probeCts.Token);
        }

        public NetworkMode GetCurrentMode() => _current;

        public async Task SetModeAsync(NetworkMode mode, CancellationToken ct = default)
        {
            if (mode == _current) return;

            switch (mode)
            {
                case NetworkMode.Solo:
                    await _network.SwitchToAsync(NetworkProviderType.Offline, ct);
                    await _lobby.SwitchToAsync(NetworkProviderType.Offline, ct);
                    break;
                case NetworkMode.Lan:
                    await _network.SwitchToAsync(NetworkProviderType.Lan, ct);
                    await _lobby.SwitchToAsync(NetworkProviderType.Lan, ct);
                    break;
                case NetworkMode.Global:
                    await _network.SwitchToAsync(NetworkProviderType.Relay, ct);
                    await _lobby.SwitchToAsync(NetworkProviderType.Relay, ct);
                    break;
            }

            _current = mode;
            Debug.Log($"[NetworkModeController] Mode switched to {_current}");
        }

        private async Task ProbeLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    bool ready = false;
                    try { await _state.WaitUntilReadyAsync(new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token); ready = true; }
                    catch { ready = false; }

                    if (ready && _current != NetworkMode.Global)
                    {
                        // Auto-switch only when no active session (SessionManager usage not injected here)
                        await SetModeAsync(NetworkMode.Global, ct);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception e) { Debug.LogWarning($"[NetworkModeController] ProbeLoop error: {e.Message}"); }

                try { await Task.Delay(TimeSpan.FromSeconds(5), ct); } catch (OperationCanceledException) { break; }
            }
        }
    }
}
