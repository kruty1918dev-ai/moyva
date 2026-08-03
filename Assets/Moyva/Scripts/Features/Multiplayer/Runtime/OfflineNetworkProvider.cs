using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Offline/local implementation of INetworkProvider.
    /// No real networking. Immediate success responses.
    /// Enables SessionManager to be fully tested without any network stack.
    /// </summary>
    public sealed class OfflineNetworkProvider :
        INetworkProvider,
        INetworkPeerIdentityConfigurator
    {
        private readonly List<IObserver<NetworkMessage>> _messageObservers = new List<IObserver<NetworkMessage>>();
        private string _localPeerId = "local";

        public IObservable<NetworkMessage> Messages => new MessageObservable(_messageObservers);

        public event Action<string> PeerConnected;
        public event Action<string> PeerDisconnected;

        public Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default)
        {
            PeerConnected?.Invoke(_localPeerId);
            return Task.FromResult(SessionResult.Ok(sessionId));
        }

        public Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default)
        {
            PeerConnected?.Invoke(_localPeerId);
            return Task.FromResult(SessionResult.Ok(sessionId));
        }

        public Task LeaveSessionAsync(CancellationToken ct = default)
        {
            PeerDisconnected?.Invoke(_localPeerId);
            return Task.CompletedTask;
        }

        public Task SendMessageAsync(string targetPeerId, byte[] payload, CancellationToken ct = default)
        {
            // In offline mode, loopback: deliver to all observers as if from "local"
            var msg = new NetworkMessage(_localPeerId, payload);
            foreach (var obs in _messageObservers)
                obs.OnNext(msg);
            return Task.CompletedTask;
        }

        public void SetLocalPeerId(string playerId)
        {
            _localPeerId = string.IsNullOrWhiteSpace(playerId)
                ? "local"
                : playerId.Trim();
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
            {
                _observers = observers;
                _observer = observer;
            }
            public void Dispose() => _observers.Remove(_observer);
        }
    }
}
