using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// DTO returned when a session is hosted or joined.
    /// </summary>
    public sealed class SessionResult
    {
        public bool Success { get; }
        public string SessionId { get; }
        public string ErrorMessage { get; }

        public SessionResult(bool success, string sessionId, string errorMessage = null)
        {
            Success = success;
            SessionId = sessionId;
            ErrorMessage = errorMessage;
        }

        public static SessionResult Ok(string sessionId) => new SessionResult(true, sessionId);
        public static SessionResult Fail(string error) => new SessionResult(false, null, error);
    }

    /// <summary>
    /// Message envelope exchanged between peers.
    /// </summary>
    public sealed class NetworkMessage
    {
        public string SenderId { get; }
        public byte[] Payload { get; }

        public NetworkMessage(string senderId, byte[] payload)
        {
            SenderId = senderId;
            Payload = payload;
        }
    }

    /// <summary>
    /// Abstraction over networking backends (Relay, WebSocket, Offline).
    /// </summary>
    public interface INetworkProvider
    {
        /// <summary>Observable stream of incoming messages.</summary>
        IObservable<NetworkMessage> Messages { get; }

        /// <summary>Raised when a peer connects.</summary>
        event Action<string> PeerConnected;

        /// <summary>Raised when a peer disconnects.</summary>
        event Action<string> PeerDisconnected;

        Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default);
        Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default);
        Task LeaveSessionAsync(CancellationToken ct = default);
        Task SendMessageAsync(string targetPeerId, byte[] payload, CancellationToken ct = default);
    }
}
