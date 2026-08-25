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
//     → [magic:4] [version:1] [sender UTF-8 length:2 LE]
//       [full senderId UTF-8 bytes] [payload bytes]
//     ← same framing from server → client
//   Receivers also accept the legacy 16-byte ASCII sender prefix so rolling
//   upgrades do not corrupt or truncate already in-flight messages.
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
    public sealed class WebSocketNetworkProvider :
        INetworkProvider,
        INetworkPeerIdentityConfigurator
    {
        private const int LegacySenderIdWidth = 16;
        private const int DataFrameHeaderSize = 7;
        private const byte DataFrameVersion = 1;
        private const int MaxSenderIdByteCount = 4096;
        private static readonly byte[] DataFrameMagic =
        {
            0xF3,
            (byte)'M',
            (byte)'Y',
            (byte)'V',
        };
        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true);

        private readonly WebSocketProviderSettings _settings;
        private readonly IMultiplayerQosMonitorService _qosMonitor;
        private readonly List<IObserver<NetworkMessage>> _observers = new List<IObserver<NetworkMessage>>();

        private ClientWebSocket _socket;
        private CancellationTokenSource _receiveCts;
        private string _localPeerId;
        private string _configuredLocalPeerId;
        private string _currentSessionId;
        private int _reconnectCount;

        public event Action<string> PeerConnected;
        public event Action<string> PeerDisconnected;
        public IObservable<NetworkMessage> Messages => new MessageObservable(_observers);

        public WebSocketNetworkProvider(WebSocketProviderSettings settings, IMultiplayerQosMonitorService qosMonitor = null)
        {
            _settings = settings ?? WebSocketProviderSettings.Default();
            _qosMonitor = qosMonitor;
        }

        // ── Session lifecycle ──────────────────────────────────────────────────────

        public async Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default)
        {
            _localPeerId = ResolveLocalPeerId();
            _currentSessionId = sessionId;
            return await ConnectAndHandshakeAsync($"HOST:{sessionId}:{_localPeerId}", sessionId, ct);
        }

        public async Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default)
        {
            _localPeerId = ResolveLocalPeerId();
            _currentSessionId = sessionId;
            return await ConnectAndHandshakeAsync($"JOIN:{sessionId}:{_localPeerId}", sessionId, ct);
        }

        public void SetLocalPeerId(string playerId)
        {
            _configuredLocalPeerId = string.IsNullOrWhiteSpace(playerId)
                ? null
                : playerId.Trim();
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
                catch (Exception)
                {
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
                return;
            }

            try
            {
                // Binary frame keeps the complete UTF-8 identity.
                var frame = BuildDataFrame(_localPeerId ?? "unknown", payload);
                await _socket.SendAsync(new ArraySegment<byte>(frame), WebSocketMessageType.Binary, true, ct);
            }
            catch (Exception)
            {
                _qosMonitor?.RecordPacketDropped("websocket-send-failed");
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
                return SessionResult.Fail(e.Message);
            }

            // Wait for OK or ERR response (with a short timeout)
            var ackResult = await WaitForAckAsync(ct);
            if (!ackResult.Success)
                return ackResult;

            // Start background receive loop
            _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _ = ReceiveLoopAsync(_receiveCts.Token);
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
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        HandleControlFrame(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary
                             && result.Count > 0)
                    {
                        HandleDataFrame(buffer, result.Count);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (WebSocketException)
                {
                    _qosMonitor?.RecordPacketDropped("websocket-receive-error");
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
                PeerConnected?.Invoke(peerId);
            }
            else if (text.StartsWith("PEER_DISCONNECTED:"))
            {
                var peerId = text.Substring("PEER_DISCONNECTED:".Length);
                PeerDisconnected?.Invoke(peerId);
            }
        }

        private void HandleDataFrame(byte[] buffer, int count)
        {
            if (!TryParseDataFrame(
                    buffer,
                    count,
                    out string senderId,
                    out byte[] payload))
            {
                _qosMonitor?.RecordPacketDropped(
                    "websocket-malformed-data-frame");
                return;
            }

            var msg = new NetworkMessage(senderId, payload);
            foreach (var obs in _observers)
                obs.OnNext(msg);
        }

        // ── Reconnect ──────────────────────────────────────────────────────────────

        private async Task<bool> TryReconnectAsync(CancellationToken ct)
        {
            _reconnectCount++;
            _qosMonitor?.RecordReconnect("websocket", _reconnectCount);

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
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        // ── Utilities ──────────────────────────────────────────────────────────────

        internal static byte[] BuildDataFrame(
            string senderId,
            byte[] payload)
        {
            if (string.IsNullOrWhiteSpace(senderId))
            {
                throw new ArgumentException(
                    "Sender id cannot be empty.",
                    nameof(senderId));
            }

            byte[] idBytes = StrictUtf8.GetBytes(senderId);
            if (idBytes.Length == 0
                || idBytes.Length > MaxSenderIdByteCount
                || idBytes.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(senderId),
                    $"Sender id must contain between 1 and {MaxSenderIdByteCount} UTF-8 bytes.");
            }

            int payloadLength = payload?.Length ?? 0;
            var frame =
                new byte[
                    DataFrameHeaderSize
                    + idBytes.Length
                    + payloadLength];
            Array.Copy(
                DataFrameMagic,
                0,
                frame,
                0,
                DataFrameMagic.Length);
            frame[4] = DataFrameVersion;
            frame[5] = (byte)(idBytes.Length & 0xFF);
            frame[6] = (byte)((idBytes.Length >> 8) & 0xFF);
            Array.Copy(
                idBytes,
                0,
                frame,
                DataFrameHeaderSize,
                idBytes.Length);
            if (payloadLength > 0)
            {
                Array.Copy(
                    payload,
                    0,
                    frame,
                    DataFrameHeaderSize + idBytes.Length,
                    payloadLength);
            }

            return frame;
        }

        internal static bool TryParseDataFrame(
            byte[] buffer,
            int count,
            out string senderId,
            out byte[] payload)
        {
            senderId = null;
            payload = null;
            if (buffer == null || count < 0 || count > buffer.Length)
                return false;

            if (HasDataFrameMagic(buffer, count))
            {
                if (count < DataFrameHeaderSize
                    || buffer[4] != DataFrameVersion)
                {
                    return false;
                }

                int senderByteCount =
                    buffer[5] | (buffer[6] << 8);
                if (senderByteCount <= 0
                    || senderByteCount > MaxSenderIdByteCount
                    || senderByteCount
                        > count - DataFrameHeaderSize)
                {
                    return false;
                }

                try
                {
                    senderId = StrictUtf8.GetString(
                        buffer,
                        DataFrameHeaderSize,
                        senderByteCount);
                }
                catch (DecoderFallbackException)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(senderId))
                    return false;

                int payloadOffset =
                    DataFrameHeaderSize + senderByteCount;
                int payloadLength = count - payloadOffset;
                payload = new byte[payloadLength];
                if (payloadLength > 0)
                {
                    Array.Copy(
                        buffer,
                        payloadOffset,
                        payload,
                        0,
                        payloadLength);
                }

                return true;
            }

            if (count < LegacySenderIdWidth)
                return false;

            senderId = Encoding.ASCII
                .GetString(
                    buffer,
                    0,
                    LegacySenderIdWidth)
                .TrimEnd('\0', ' ');
            if (string.IsNullOrWhiteSpace(senderId))
                return false;

            int legacyPayloadLength =
                count - LegacySenderIdWidth;
            payload = new byte[legacyPayloadLength];
            if (legacyPayloadLength > 0)
            {
                Array.Copy(
                    buffer,
                    LegacySenderIdWidth,
                    payload,
                    0,
                    legacyPayloadLength);
            }

            return true;
        }

        private static bool HasDataFrameMagic(
            byte[] buffer,
            int count)
        {
            if (count < DataFrameMagic.Length)
                return false;

            for (int index = 0;
                 index < DataFrameMagic.Length;
                 index++)
            {
                if (buffer[index] != DataFrameMagic[index])
                    return false;
            }

            return true;
        }

        private static string GeneratePeerId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 12);
        }

        private string ResolveLocalPeerId()
            => string.IsNullOrWhiteSpace(_configuredLocalPeerId)
                ? GeneratePeerId()
                : _configuredLocalPeerId;

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
