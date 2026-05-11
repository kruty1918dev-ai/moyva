// WebSocketNetworkProvider — connects to a custom WebSocket signalling server.
//
// Protocol (text-framed with UTF-8 JSON control messages, binary for data):
//   Control messages (Text frames):
//     → HOST:<sessionId>:<peerId>            — claim host for a room
//     → JOIN:<sessionId>:<peerId>            — join existing room
//     ← OK:<sessionId>                       — server acknowledges
//     ← ERR:<reason>                         — server rejects
//     ← PEER_CONNECTED:<peerId>              — another peer joined
//     ← PEER_DISCONNECTED:<peerId>           — a peer left
//   Data messages (Binary frames):
//     → [16-byte senderId (ASCII, zero/space-padded)] [payload bytes]
//     ← same framing from server → client
//
// The server-side relay implementation is intentionally outside this file.
// Any WebSocket server that speaks this protocol will work.

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// WebSocket backend — connects to a custom signalling/relay server.
    /// Uses <c>System.Net.WebSockets.ClientWebSocket</c> (available on all Unity
    /// standalone, mobile, and server platforms; NOT supported on WebGL — use a
    /// JS-bridge plugin there).
    /// </summary>
    public sealed class WebSocketNetworkProvider : INetworkProvider
    {
        /// <summary>Sender-ID field width in data frames (bytes).</summary>
        private const int SenderIdWidth = 16;

        private readonly WebSocketProviderSettings _settings;
        private readonly IMultiplayerLogger _logger;
        private readonly IMultiplayerQosMonitorService _qosMonitor;
        private readonly List<IObserver<NetworkMessage>> _observers = new List<IObserver<NetworkMessage>>();

        private ClientWebSocket _socket;
        private CancellationTokenSource _receiveCts;
        private string _localPeerId;
        private string _currentSessionId;
        private int _reconnectCount;

        public event Action<string> PeerConnected;
        public event Action<string> PeerDisconnected;
        public IObservable<NetworkMessage> Messages => new MessageObservable(_observers);

        public WebSocketNetworkProvider(WebSocketProviderSettings settings, IMultiplayerLogger logger, IMultiplayerQosMonitorService qosMonitor = null)
        {
            _settings = settings ?? WebSocketProviderSettings.Default();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _qosMonitor = qosMonitor;
        }

        // ── Session lifecycle ──────────────────────────────────────────────────────

        public async Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default)
        {
            _localPeerId = GeneratePeerId();
            _currentSessionId = sessionId;
            return await ConnectAndHandshakeAsync($"HOST:{sessionId}:{_localPeerId}", sessionId, ct);
        }

        public async Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default)
        {
            _localPeerId = GeneratePeerId();
            _currentSessionId = sessionId;
            return await ConnectAndHandshakeAsync($"JOIN:{sessionId}:{_localPeerId}", sessionId, ct);
        }

        public async Task LeaveSessionAsync(CancellationToken ct = default)
        {
            _receiveCts?.Cancel();
            if (_socket?.State == WebSocketState.Open)
            {
                try
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "leave", CancellationToken.None);
                }
                catch (Exception e)
                {
                    _logger.Warn($"[WebSocket] Close error: {e.Message}");
                }
            }
            _socket?.Dispose();
            _socket = null;
            _currentSessionId = null;
        }

        public async Task SendMessageAsync(string targetPeerId, byte[] payload, CancellationToken ct = default)
        {
            if (_socket?.State != WebSocketState.Open)
            {
                _logger.Warn("[WebSocket] SendMessage: socket not open.");
                return;
            }

            try
            {
                // Binary frame: [SenderIdWidth bytes for senderId][payload]
                var frame = BuildDataFrame(_localPeerId ?? "unknown", payload);
                await _socket.SendAsync(new ArraySegment<byte>(frame), WebSocketMessageType.Binary, true, ct);
            }
            catch (Exception e)
            {
                _qosMonitor?.RecordPacketDropped("websocket-send-failed");
                _logger.Error($"[WebSocket] SendMessage failed: {e.Message}");
            }
        }

        // ── Connection + handshake ─────────────────────────────────────────────────

        private async Task<SessionResult> ConnectAndHandshakeAsync(
            string handshakeCommand, string sessionId, CancellationToken ct)
        {
            _reconnectCount = 0;
            try
            {
                await OpenSocketAsync(ct);
            }
            catch (Exception e)
            {
                _logger.Error($"[WebSocket] Connect failed: {e.Message}");
                return SessionResult.Fail(e.Message);
            }

            // Send handshake command
            try
            {
                var cmdBytes = Encoding.UTF8.GetBytes(handshakeCommand);
                await _socket.SendAsync(new ArraySegment<byte>(cmdBytes), WebSocketMessageType.Text, true, ct);
            }
            catch (Exception e)
            {
                _logger.Error($"[WebSocket] Handshake send failed: {e.Message}");
                return SessionResult.Fail(e.Message);
            }

            // Wait for OK or ERR response (with a short timeout)
            var ackResult = await WaitForAckAsync(ct);
            if (!ackResult.Success)
                return ackResult;

            // Start background receive loop
            _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _ = ReceiveLoopAsync(_receiveCts.Token);

            _logger.Info($"[WebSocket] Connected to {_settings.ServerUrl}:{_settings.Port}, session={sessionId}");
            return SessionResult.Ok(sessionId);
        }

        private async Task OpenSocketAsync(CancellationToken ct)
        {
            _socket?.Dispose();
            _socket = new ClientWebSocket();

            if (!string.IsNullOrEmpty(_settings.AuthToken))
                _socket.Options.SetRequestHeader("Authorization", $"Bearer {_settings.AuthToken}");

            var uri = BuildUri();
            await _socket.ConnectAsync(uri, ct);
        }

        private Uri BuildUri()
        {
            // Use Uri parsing to check whether the URL already contains a port.
            // Append the configured port only when no port is present in the URL.
            string url = _settings.ServerUrl.TrimEnd('/');

            // Temporarily substitute ws/wss scheme with http/https so Uri can parse it.
            string parseUrl = url.Replace("wss://", "https://").Replace("ws://", "http://");
            if (Uri.TryCreate(parseUrl, UriKind.Absolute, out var parsed) &&
                (parsed.IsDefaultPort || parsed.Port < 0) &&
                _settings.Port > 0)
            {
                url = $"{url}:{_settings.Port}";
            }

            return new Uri(url);
        }

        private async Task<SessionResult> WaitForAckAsync(CancellationToken ct)
        {
            var buffer = new byte[256];
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);
            try
            {
                var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), linked.Token);
                if (result.MessageType != WebSocketMessageType.Text)
                    return SessionResult.Fail("Unexpected binary frame during handshake.");

                var response = Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (response.StartsWith("OK:"))
                    return SessionResult.Ok(response.Substring(3));
                if (response.StartsWith("ERR:"))
                    return SessionResult.Fail(response.Substring(4));

                return SessionResult.Fail($"Unexpected server response: {response}");
            }
            catch (OperationCanceledException)
            {
                return SessionResult.Fail("Server did not respond to handshake within 10 seconds.");
            }
        }

        // ── Receive loop ───────────────────────────────────────────────────────────

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            var buffer = new byte[65536];
            while (!ct.IsCancellationRequested && _socket?.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.Info("[WebSocket] Server closed connection.");
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        HandleControlFrame(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary && result.Count > SenderIdWidth)
                    {
                        HandleDataFrame(buffer, result.Count);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (WebSocketException wse)
                {
                    _qosMonitor?.RecordPacketDropped("websocket-receive-error");
                    _logger.Error($"[WebSocket] Receive error: {wse.Message}");
                    if (!ct.IsCancellationRequested && _reconnectCount < _settings.ReconnectAttempts)
                    {
                        if (await TryReconnectAsync(ct))
                            continue;
                    }
                    break;
                }
            }
        }

        private void HandleControlFrame(string text)
        {
            if (text.StartsWith("PEER_CONNECTED:"))
            {
                var peerId = text.Substring("PEER_CONNECTED:".Length);
                _logger.Info($"[WebSocket] Peer connected: {peerId}");
                PeerConnected?.Invoke(peerId);
            }
            else if (text.StartsWith("PEER_DISCONNECTED:"))
            {
                var peerId = text.Substring("PEER_DISCONNECTED:".Length);
                _logger.Info($"[WebSocket] Peer disconnected: {peerId}");
                PeerDisconnected?.Invoke(peerId);
            }
        }

        private void HandleDataFrame(byte[] buffer, int count)
        {
            string senderId = Encoding.ASCII.GetString(buffer, 0, SenderIdWidth).TrimEnd('\0', ' ');
            int payloadLength = count - SenderIdWidth;
            var payload = new byte[payloadLength];
            Array.Copy(buffer, SenderIdWidth, payload, 0, payloadLength);

            var msg = new NetworkMessage(senderId, payload);
            foreach (var obs in _observers)
                obs.OnNext(msg);
        }

        // ── Reconnect ──────────────────────────────────────────────────────────────

        private async Task<bool> TryReconnectAsync(CancellationToken ct)
        {
            _reconnectCount++;
            _qosMonitor?.RecordReconnect("websocket", _reconnectCount);
            _logger.Warn($"[WebSocket] Reconnect attempt {_reconnectCount}/{_settings.ReconnectAttempts}...");

            await Task.Delay(TimeSpan.FromSeconds(_settings.ReconnectDelaySeconds), ct);

            try
            {
                await OpenSocketAsync(ct);
                var cmd = $"JOIN:{_currentSessionId}:{_localPeerId}";
                var cmdBytes = Encoding.UTF8.GetBytes(cmd);
                await _socket.SendAsync(new ArraySegment<byte>(cmdBytes), WebSocketMessageType.Text, true, ct);
                var ack = await WaitForAckAsync(ct);
                if (ack.Success)
                {
                    _logger.Info($"[WebSocket] Reconnected successfully.");
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"[WebSocket] Reconnect failed: {e.Message}");
            }
            return false;
        }

        // ── Utilities ──────────────────────────────────────────────────────────────

        private static byte[] BuildDataFrame(string senderId, byte[] payload)
        {
            var frame = new byte[SenderIdWidth + (payload?.Length ?? 0)];
            var idBytes = Encoding.ASCII.GetBytes(senderId);
            int idLen = Math.Min(idBytes.Length, SenderIdWidth);
            Array.Copy(idBytes, 0, frame, 0, idLen);
            if (payload != null)
                Array.Copy(payload, 0, frame, SenderIdWidth, payload.Length);
            return frame;
        }

        private static string GeneratePeerId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 12);
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
