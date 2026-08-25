using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Wraps a primary and a fallback <see cref="INetworkProvider"/>.
    /// When the primary provider fails (returns a failed <see cref="SessionResult"/> or throws),
    /// the wrapper automatically promotes the fallback and retries the same operation.
    /// Subsequent calls go directly to the fallback until <see cref="Reset"/> is called.
    /// </summary>
    public sealed class FallbackNetworkProvider :
        INetworkProvider,
        INetworkPeerIdentityConfigurator,
        IDisposable
    {
        private readonly INetworkProvider _primary;
        private readonly INetworkProvider _fallback;

        // Observers subscribed to this provider's Messages
        private readonly List<IObserver<NetworkMessage>> _observers = new List<IObserver<NetworkMessage>>();

        // Active subscription that forwards messages from whichever provider is live
        private IDisposable _activeSub;
        private bool _usingFallback;

        public event Action<string> PeerConnected;
        public event Action<string> PeerDisconnected;

        public IObservable<NetworkMessage> Messages => new MessageObservable(_observers);

        public FallbackNetworkProvider(
            INetworkProvider primary,
            INetworkProvider fallback)
        {
            _primary = primary ?? throw new ArgumentNullException(nameof(primary));
            _fallback = fallback ?? throw new ArgumentNullException(nameof(fallback));

            // Forward peer events from both providers (the active one fires them)
            _primary.PeerConnected += id => PeerConnected?.Invoke(id);
            _primary.PeerDisconnected += id => PeerDisconnected?.Invoke(id);
            _fallback.PeerConnected += id => PeerConnected?.Invoke(id);
            _fallback.PeerDisconnected += id => PeerDisconnected?.Invoke(id);

            // Start forwarding messages from the primary
            SubscribeTo(_primary);
        }

        // ── Session lifecycle ──────────────────────────────────────────────────────

        public Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default)
            => ExecuteWithFallback(p => p.HostSessionAsync(sessionId, ct), sessionId, ct);

        public Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default)
            => ExecuteWithoutFallback(p => p.JoinSessionAsync(sessionId, ct), sessionId);

        public Task LeaveSessionAsync(CancellationToken ct = default)
            => Active.LeaveSessionAsync(ct);

        public Task SendMessageAsync(string targetPeerId, byte[] payload, CancellationToken ct = default)
            => Active.SendMessageAsync(targetPeerId, payload, ct);

        public void SetLocalPeerId(string playerId)
        {
            if (_primary is INetworkPeerIdentityConfigurator primary)
                primary.SetLocalPeerId(playerId);
            if (_fallback is INetworkPeerIdentityConfigurator fallback)
                fallback.SetLocalPeerId(playerId);
        }

        // ── Fallback logic ─────────────────────────────────────────────────────────

        private INetworkProvider Active => _usingFallback ? _fallback : _primary;

        private async Task<SessionResult> ExecuteWithoutFallback(
            Func<INetworkProvider, Task<SessionResult>> action,
            string sessionId)
        {
            var provider = Active;
            try
            {
                var result = await action(provider);
                if (result == null)
                    return SessionResult.Fail("JoinSession returned null result.");

                return result;
            }
            catch (Exception e)
            {
                return SessionResult.Fail(e.Message);
            }
        }

        private async Task<SessionResult> ExecuteWithFallback(
            Func<INetworkProvider, Task<SessionResult>> action,
            string sessionId,
            CancellationToken ct)
        {
            if (_usingFallback)
                return await action(_fallback);

            // Try primary first
            SessionResult result;
            try
            {
                result = await action(_primary);
            }
            catch (Exception)
            {
                return await ActivateFallbackAndRetry(action, ct);
            }

            if (!result.Success)
            {
                return await ActivateFallbackAndRetry(action, ct);
            }

            return result;
        }

        private async Task<SessionResult> ActivateFallbackAndRetry(
            Func<INetworkProvider, Task<SessionResult>> action,
            CancellationToken ct)
        {
            _usingFallback = true;
            SubscribeTo(_fallback);

            try
            {
                return await action(_fallback);
            }
            catch (Exception e)
            {
                return SessionResult.Fail($"Both primary and fallback providers failed. Last error: {e.Message}");
            }
        }

        /// <summary>
        /// Resets the provider back to primary. Call after primary is believed to be healthy again.
        /// </summary>
        public void Reset()
        {
            _usingFallback = false;
            SubscribeTo(_primary);
        }

        public void Dispose()
        {
            _activeSub?.Dispose();

            if (_primary is IDisposable primaryDisposable)
            {
                try { primaryDisposable.Dispose(); } catch { }
            }

            if (_fallback is IDisposable fallbackDisposable)
            {
                try { fallbackDisposable.Dispose(); } catch { }
            }
        }

        // ── Message forwarding ─────────────────────────────────────────────────────

        private void SubscribeTo(INetworkProvider provider)
        {
            _activeSub?.Dispose();
            _activeSub = provider.Messages.Subscribe(new ForwardObserver(_observers));
        }

        private sealed class ForwardObserver : IObserver<NetworkMessage>
        {
            private readonly List<IObserver<NetworkMessage>> _targets;
            public ForwardObserver(List<IObserver<NetworkMessage>> targets) => _targets = targets;
            public void OnNext(NetworkMessage m)
            {
                // Snapshot list to guard against modification during iteration
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
