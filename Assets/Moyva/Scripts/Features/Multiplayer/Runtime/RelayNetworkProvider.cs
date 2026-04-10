// RelayNetworkProvider — Unity Gaming Services Relay backend.
//
// SETUP INSTRUCTIONS:
//   1. Install the Relay SDK:  Window → Package Manager → Unity Gaming Services → Relay
//   2. Add the scripting define MOYVA_UGS_RELAY:
//      Edit → Project Settings → Player → Scripting Define Symbols
//   3. Authenticate with UGS before calling HostSessionAsync / JoinSessionAsync:
//        await UnityServices.InitializeAsync();
//        await AuthenticationService.Instance.SignInAnonymouslyAsync();
//
// Without MOYVA_UGS_RELAY the provider compiles cleanly but returns a graceful failure,
// so FallbackNetworkProvider will automatically promote the fallback (WebSocket / Offline).

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Unity Relay backend — cloud NAT traversal, no open ports required.
    /// Requires the Unity Gaming Services Relay SDK and the <c>MOYVA_UGS_RELAY</c>
    /// scripting define symbol. Falls back gracefully when the SDK is absent.
    /// </summary>
    public sealed class RelayNetworkProvider : INetworkProvider
    {
        private readonly RelayProviderSettings _settings;
        private readonly IMultiplayerLogger _logger;
        private readonly List<IObserver<NetworkMessage>> _observers = new List<IObserver<NetworkMessage>>();

        public event Action<string> PeerConnected;
        public event Action<string> PeerDisconnected;
        public IObservable<NetworkMessage> Messages => new MessageObservable(_observers);

        public RelayNetworkProvider(RelayProviderSettings settings, IMultiplayerLogger logger)
        {
            _settings = settings ?? RelayProviderSettings.Default();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── Session lifecycle ──────────────────────────────────────────────────────

        public async Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default)
        {
#if MOYVA_UGS_RELAY
            return await HostViaRelayAsync(sessionId, ct);
#else
            return await Task.FromResult(UgsNotAvailable());
#endif
        }

        public async Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default)
        {
#if MOYVA_UGS_RELAY
            return await JoinViaRelayAsync(sessionId, ct);
#else
            return await Task.FromResult(UgsNotAvailable());
#endif
        }

        public Task LeaveSessionAsync(CancellationToken ct = default)
        {
#if MOYVA_UGS_RELAY
            return LeaveRelayAsync(ct);
#else
            return Task.CompletedTask;
#endif
        }

        public Task SendMessageAsync(string targetPeerId, byte[] payload, CancellationToken ct = default)
        {
#if MOYVA_UGS_RELAY
            return SendViaRelayAsync(targetPeerId, payload, ct);
#else
            return Task.CompletedTask;
#endif
        }

        // ── UGS Relay implementation (compiled only with MOYVA_UGS_RELAY) ─────────

#if MOYVA_UGS_RELAY
        // The UGS Relay SDK types used below:
        //   Unity.Services.Relay.RelayService               — main entry point
        //   Unity.Services.Relay.Models.Allocation          — host-side allocation
        //   Unity.Services.Relay.Models.JoinAllocation      — client-side allocation
        //   Unity.Networking.Transport.RelayServerData      — transport data
        //
        // All await calls are async-safe for Unity's main thread with UniTask or
        // standard SynchronizationContext.

        private object _allocation;   // Unity.Services.Relay.Models.Allocation
        private string _joinCode;

        private async Task<SessionResult> HostViaRelayAsync(string sessionId, CancellationToken ct)
        {
            try
            {
                // 1. Create a Relay allocation
                var allocation = await Unity.Services.Relay.RelayService.Instance
                    .CreateAllocationAsync(_settings.MaxConnections, _settings.Region);

                // 2. Get the join code that other players will use
                _joinCode = await Unity.Services.Relay.RelayService.Instance
                    .GetJoinCodeAsync(allocation.AllocationId);

                _allocation = allocation;
                _logger.Info($"[Relay] Hosted allocation. Join code: {_joinCode}");

                // 3. Configure Unity Transport with Relay data
                //    (done externally via NetworkManager / Unity Transport package)
                //    var relayServerData = new RelayServerData(allocation, "dtls");
                //    NetworkManager.Singleton.GetComponent<UnityTransport>()
                //        .SetRelayServerData(relayServerData);
                //    NetworkManager.Singleton.StartHost();

                // Return the join code as SessionId so clients can use it to join
                return SessionResult.Ok(_joinCode);
            }
            catch (Exception e)
            {
                _logger.Error($"[Relay] HostAsync failed: {e.Message}");
                return SessionResult.Fail(e.Message);
            }
        }

        private async Task<SessionResult> JoinViaRelayAsync(string joinCode, CancellationToken ct)
        {
            try
            {
                // 1. Join allocation via join code
                var joinAllocation = await Unity.Services.Relay.RelayService.Instance
                    .JoinAllocationAsync(joinCode);

                _logger.Info($"[Relay] Joined allocation for join code: {joinCode}");

                // 2. Configure Unity Transport with Relay data
                //    var relayServerData = new RelayServerData(joinAllocation, "dtls");
                //    NetworkManager.Singleton.GetComponent<UnityTransport>()
                //        .SetRelayServerData(relayServerData);
                //    NetworkManager.Singleton.StartClient();

                return SessionResult.Ok(joinCode);
            }
            catch (Exception e)
            {
                _logger.Error($"[Relay] JoinAsync failed: {e.Message}");
                return SessionResult.Fail(e.Message);
            }
        }

        private Task LeaveRelayAsync(CancellationToken ct)
        {
            _allocation = null;
            _joinCode = null;
            // NetworkManager.Singleton.Shutdown();
            return Task.CompletedTask;
        }

        private Task SendViaRelayAsync(string targetPeerId, byte[] payload, CancellationToken ct)
        {
            // Send via Unity Transport / Netcode for GameObjects RPC / custom message system.
            // The exact implementation depends on the Netcode layer used (NGO vs. raw transport).
            // Example with Netcode for GameObjects:
            //   NetworkManager.Singleton.CustomMessagingManager
            //       .SendNamedMessage("game_msg", clientId, new FastBufferWriter(...));
            _logger.Trace($"[Relay] SendMessage to {targetPeerId}, {payload?.Length ?? 0} bytes");
            return Task.CompletedTask;
        }

        private void OnRelayMessageReceived(string senderId, byte[] payload)
        {
            var msg = new NetworkMessage(senderId, payload);
            foreach (var obs in _observers)
                obs.OnNext(msg);
        }
#endif

        // ── Helpers ───────────────────────────────────────────────────────────────

        private SessionResult UgsNotAvailable()
        {
            const string msg =
                "Unity Gaming Services Relay SDK is not installed. " +
                "Install com.unity.services.relay and add the MOYVA_UGS_RELAY scripting define.";
            _logger.Warn($"[Relay] {msg}");
            return SessionResult.Fail(msg);
        }

        // ── Observable helpers (mirrors OfflineNetworkProvider pattern) ───────────

        private sealed class MessageObservable : IObservable<NetworkMessage>
        {
            private readonly List<IObserver<NetworkMessage>> _observers;
            public MessageObservable(List<IObserver<NetworkMessage>> observers) => _observers = observers;
            public IDisposable Subscribe(IObserver<NetworkMessage> observer)
            {
                _observers.Add(observer);
                return new Unsubscriber(_observers, observer);
            }
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<NetworkMessage>> _observers;
            private readonly IObserver<NetworkMessage> _observer;
            public Unsubscriber(List<IObserver<NetworkMessage>> observers, IObserver<NetworkMessage> observer)
            { _observers = observers; _observer = observer; }
            public void Dispose() => _observers.Remove(_observer);
        }
    }
}
