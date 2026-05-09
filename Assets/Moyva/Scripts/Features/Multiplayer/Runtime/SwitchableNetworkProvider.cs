using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Wrapper that stays DI-bound and can switch the underlying INetworkProvider at runtime.
    /// Preserves subscriber lists and forwards events/messages.
    /// </summary>
    public sealed class SwitchableNetworkProvider : INetworkProvider, IDisposable
    {
        private readonly MultiplayerConfig _config;
        private readonly IMultiplayerLogger _logger;

        private INetworkProvider _inner;
        private readonly List<IObserver<NetworkMessage>> _observers = new List<IObserver<NetworkMessage>>();
        private IDisposable _activeSub;
        private readonly SemaphoreSlim _switchLock = new SemaphoreSlim(1, 1);

        public event Action<string> PeerConnected;
        public event Action<string> PeerDisconnected;

        public IObservable<NetworkMessage> Messages => new MessageObservable(_observers);

        public NetworkProviderType CurrentType { get; private set; }

        public SwitchableNetworkProvider(MultiplayerConfig config, IMultiplayerLogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Create initial provider based on config
            _inner = NetworkProviderFactory.CreateByType(_config.ProviderType, _config, _logger);
            CurrentType = _config.ProviderType;
            HookInner(_inner);
        }

        private void HookInner(INetworkProvider prov)
        {
            prov.PeerConnected += OnPeerConnectedInternal;
            prov.PeerDisconnected += OnPeerDisconnectedInternal;
            _activeSub = prov.Messages.Subscribe(new ForwardObserver(_observers));
        }

        private void UnhookInner(INetworkProvider prov)
        {
            try
            {
                prov.PeerConnected -= OnPeerConnectedInternal;
                prov.PeerDisconnected -= OnPeerDisconnectedInternal;
            }
            catch { }
            _activeSub?.Dispose();
            _activeSub = null;
        }

        private void OnPeerConnectedInternal(string id) => PeerConnected?.Invoke(id);
        private void OnPeerDisconnectedInternal(string id) => PeerDisconnected?.Invoke(id);

        public async Task SwitchToAsync(NetworkProviderType type, CancellationToken ct = default)
        {
            await _switchLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (type == CurrentType) return;
                // Gracefully leave existing session
                try { await _inner.LeaveSessionAsync(ct).ConfigureAwait(false); }
                catch (Exception e) { _logger.Warn($"[Switchable] Leave failed: {e.Message}"); }

                DisposeProvider(_inner);
                UnhookInner(_inner);
                var next = NetworkProviderFactory.CreateByType(type, _config, _logger);
                _inner = next;
                CurrentType = type;
                HookInner(_inner);
            }
            finally { _switchLock.Release(); }
        }

        // Delegate methods
        public Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default)
            => _inner.HostSessionAsync(sessionId, ct);

        public Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default)
            => _inner.JoinSessionAsync(sessionId, ct);

        public Task LeaveSessionAsync(CancellationToken ct = default)
            => _inner.LeaveSessionAsync(ct);

        public Task SendMessageAsync(string targetPeerId, byte[] payload, CancellationToken ct = default)
            => _inner.SendMessageAsync(targetPeerId, payload, ct);

        public void Dispose()
        {
            UnhookInner(_inner);
            DisposeProvider(_inner);
            _switchLock.Dispose();
        }

        private static void DisposeProvider(INetworkProvider provider)
        {
            if (provider is IDisposable disposable)
            {
                try { disposable.Dispose(); }
                catch { }
            }
        }

        private sealed class ForwardObserver : IObserver<NetworkMessage>
        {
            private readonly List<IObserver<NetworkMessage>> _targets;
            public ForwardObserver(List<IObserver<NetworkMessage>> targets) => _targets = targets;
            public void OnNext(NetworkMessage m)
            {
                var snap = _targets.ToArray();
                foreach (var t in snap) t.OnNext(m);
            }
            public void OnError(Exception e)
            {
                var snap = _targets.ToArray();
                foreach (var t in snap) t.OnError(e);
            }
            public void OnCompleted() { }
        }

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
