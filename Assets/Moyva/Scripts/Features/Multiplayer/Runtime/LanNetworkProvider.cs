using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Config;
using System.Net;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Unity.Collections;
using Unity.Networking.Transport;
using UtpDataStreamReader = Unity.Networking.Transport.DataStreamReader;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// LAN network provider skeleton.
    ///
    /// Real implementation uses Netcode for GameObjects (NGO) CustomMessagingManager
    /// or a UDP transport. This provider uses Unity Transport primitives and assumes
    /// the necessary Unity Transport / Netcode packages are installed in the project.
    /// </summary>
    public sealed class LanNetworkProvider : INetworkProvider
    {
        private readonly MultiplayerConfig _config;
        private readonly IMultiplayerLogger _logger;

        // Observers subscribed to this provider's Messages
        private readonly List<IObserver<NetworkMessage>> _observers = new List<IObserver<NetworkMessage>>();

        public event Action<string> PeerConnected;
        public event Action<string> PeerDisconnected;

        public IObservable<NetworkMessage> Messages => new MessageObservable(_observers);

        public LanNetworkProvider(MultiplayerConfig config, IMultiplayerLogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                foreach (var ni in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (ni.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ni.ToString();
                }
            }
            catch { }
            return null;
        }

        public Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default)
        {
            return HostViaLanAsync(sessionId, ct);
        }

        public Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default)
        {
            return JoinViaLanAsync(sessionId, ct);
        }

        public Task LeaveSessionAsync(CancellationToken ct = default)
        {
            return ShutdownTransportAsync();
        }

        public Task SendMessageAsync(string targetPeerId, byte[] payload, CancellationToken ct = default)
        {
            return SendViaLanAsync(targetPeerId, payload, ct);
        }

    // LAN transport implementation using Unity Transport (NetworkDriver).
                private const byte FrameHello = 1;
                private const byte FrameIdentity = 2;
                private const byte FrameUserData = 3;
                private const byte FrameBye = 4;

                private const int MaxFrameBodyBytes = 60 * 1024;
                private const int HandshakeTimeoutMs = 10_000;
                private const int PumpDelayMs = 16;

                private NetworkDriver _driver;
                private NetworkConnection _serverConnection;
                private NativeList<NetworkConnection> _serverConnections;
                private readonly Dictionary<int, string> _connectionPlayerIds = new Dictionary<int, string>();
                private bool _isHost;
                private string _localPeerId;
                private string _hostPeerId;
                private bool _hostHelloReceived;
                private CancellationTokenSource _pumpCts;
                private Task _pumpTask;

                private async Task<SessionResult> HostViaLanAsync(string sessionId, CancellationToken ct)
                {
                    try
                    {
                        _localPeerId = Environment.MachineName ?? $"host-{Guid.NewGuid():N}";

                        await ShutdownTransportAsync();

                        var netSettings = new NetworkSettings();
                        _driver = NetworkDriver.Create(netSettings);
                        _serverConnections = new NativeList<NetworkConnection>(Math.Max(4, 4), Allocator.Persistent);

                        var endpoint = NetworkEndPoint.AnyIpv4.WithPort((ushort)LanLobbyService.DefaultPort);
                        if (_driver.Bind(endpoint) != 0)
                            return SessionResult.Fail("LAN host bind failed.");

                        if (_driver.Listen() != 0)
                            return SessionResult.Fail("LAN host listen failed.");

                        _isHost = true;
                        StartPumpLoop(ct);
                        PeerConnected?.Invoke(_localPeerId);

                        var ip = GetLocalIPAddress() ?? "127.0.0.1";
                        var joinCode = $"lan:{ip}:{LanLobbyService.DefaultPort}";
                        _logger.Info($"[Lan] Hosted on {ip}:{LanLobbyService.DefaultPort}");
                        return SessionResult.Ok(joinCode);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"[Lan] Host failed: {e.Message}");
                        return SessionResult.Fail(e.Message);
                    }
                }

                private async Task<SessionResult> JoinViaLanAsync(string joinCode, CancellationToken ct)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(joinCode) || !joinCode.StartsWith("lan:"))
                            return SessionResult.Fail("Invalid LAN join code.");

                        var parts = joinCode.Split(':');
                        if (parts.Length < 3) return SessionResult.Fail("Invalid LAN join code format.");
                        var ip = parts[1];
                        if (!ushort.TryParse(parts[2], out var port)) return SessionResult.Fail("Invalid LAN port.");

                        await ShutdownTransportAsync();

                        var netSettings = new NetworkSettings();
                        _driver = NetworkDriver.Create(netSettings);

                        var ep = default(NetworkEndPoint);
                        if (!NetworkEndPoint.TryParse(ip, port, out ep))
                        {
                            // Fallback: try parse via DNS
                            ep = NetworkEndPoint.AnyIpv4.WithPort(port);
                        }

                        _serverConnection = _driver.Connect(ep);
                        _isHost = false;
                        StartPumpLoop(ct);

                        var deadline = DateTime.UtcNow.AddMilliseconds(HandshakeTimeoutMs);
                        while (!_hostHelloReceived)
                        {
                            if (ct.IsCancellationRequested) return SessionResult.Fail("Join cancelled.");
                            if (DateTime.UtcNow > deadline) return SessionResult.Fail("LAN handshake timeout.");
                            await Task.Delay(50, ct);
                        }

                        _logger.Info($"[Lan] Joined host={_hostPeerId}");
                        return SessionResult.Ok(joinCode);
                    }
                    catch (OperationCanceledException)
                    {
                        return SessionResult.Fail("Join cancelled.");
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"[Lan] Join failed: {e.Message}");
                        return SessionResult.Fail(e.Message);
                    }
                }

                private void StartPumpLoop(CancellationToken externalCt)
                {
                    _pumpCts?.Cancel();
                    _pumpCts?.Dispose();
                    _pumpCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
                    _pumpTask = PumpLoopAsync(_pumpCts.Token);
                }

                private async Task PumpLoopAsync(CancellationToken ct)
                {
                    while (!ct.IsCancellationRequested && _driver.IsCreated)
                    {
                        try
                        {
                            _driver.ScheduleUpdate().Complete();

                            if (_isHost)
                                PumpHost();
                            else
                                PumpClient();
                        }
                        catch (Exception e)
                        {
                            _logger.Warn($"[Lan] Pump error: {e.Message}");
                        }

                        try { await Task.Delay(PumpDelayMs, ct); }
                        catch (OperationCanceledException) { return; }
                    }
                }

                private void PumpHost()
                {
                    NetworkConnection incoming;
                    while ((incoming = _driver.Accept()) != default)
                    {
                        _serverConnections.Add(incoming);
                    }

                    for (int i = 0; i < _serverConnections.Length; i++)
                    {
                        var connection = _serverConnections[i];
                        if (!connection.IsCreated)
                        {
                            _serverConnections.RemoveAtSwapBack(i--);
                            continue;
                        }

                        NetworkEvent.Type eventType;
                        while ((eventType = _driver.PopEventForConnection(connection, out var stream)) != NetworkEvent.Type.Empty)
                        {
                            switch (eventType)
                            {
                                case NetworkEvent.Type.Data:
                                    HandleFrame(connection, stream, isHostSide: true);
                                    break;
                                case NetworkEvent.Type.Disconnect:
                                    var key = connection.GetHashCode();
                                    if (_connectionPlayerIds.TryGetValue(key, out var pid))
                                    {
                                        _connectionPlayerIds.Remove(key);
                                        PeerDisconnected?.Invoke(pid);
                                    }
                                    _serverConnections[i] = default;
                                    break;
                            }
                        }
                    }
                }

                private void PumpClient()
                {
                    if (!_serverConnection.IsCreated) return;

                    NetworkEvent.Type eventType;
                    while ((eventType = _serverConnection.PopEvent(_driver, out var stream)) != NetworkEvent.Type.Empty)
                    {
                        switch (eventType)
                        {
                            case NetworkEvent.Type.Connect:
                                _logger.Info("[Lan] Connected to host; sending Hello+Identity.");
                                SendFrame(_serverConnection, BuildHelloFrame(_localPeerId));
                                SendFrame(_serverConnection, BuildIdentityFrame(_localPeerId));
                                break;
                            case NetworkEvent.Type.Data:
                                HandleFrame(_serverConnection, stream, isHostSide: false);
                                break;
                            case NetworkEvent.Type.Disconnect:
                                _logger.Warn("[Lan] Disconnected from host.");
                                _serverConnection = default;
                                if (!string.IsNullOrEmpty(_hostPeerId))
                                    PeerDisconnected?.Invoke(_hostPeerId);
                                break;
                        }
                    }
                }

                private void HandleFrame(NetworkConnection source, UtpDataStreamReader stream, bool isHostSide)
                {
                    if (!TryReadFrame(stream, out byte type, out byte[] body))
                    {
                        _logger.Warn("[Lan] Failed to read frame.");
                        return;
                    }

                    switch (type)
                    {
                        case FrameHello:    HandleHello(source, body, isHostSide); break;
                        case FrameIdentity: HandleIdentity(source, body, isHostSide); break;
                        case FrameUserData: HandleUserData(source, body, isHostSide); break;
                        case FrameBye:      break;
                        default:
                            _logger.Warn($"[Lan] Unknown frame type {type}.");
                            break;
                    }
                }

                private void HandleHello(NetworkConnection source, byte[] body, bool isHostSide)
                {
                    if (body == null || body.Length < 4)
                    {
                        _logger.Warn("[Lan] Invalid Hello frame.");
                        return;
                    }

                    uint version = BitConverter.ToUInt32(body, 0);
                    if (version != 1)
                    {
                        _logger.Warn($"[Lan] Protocol mismatch: remote={version}, local=1. Dropping.");
                        _driver.Disconnect(source);
                        return;
                    }

                    string peerId = body.Length > 4 ? System.Text.Encoding.UTF8.GetString(body, 4, body.Length - 4) : string.Empty;
                    if (isHostSide)
                    {
                        SendFrame(source, BuildHelloFrame(_localPeerId));
                        SendFrame(source, BuildIdentityFrame(_localPeerId));
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(peerId))
                            _hostPeerId = peerId;
                    }
                }

                private void HandleIdentity(NetworkConnection source, byte[] body, bool isHostSide)
                {
                    string peerId = body != null && body.Length > 0 ? System.Text.Encoding.UTF8.GetString(body) : string.Empty;
                    if (string.IsNullOrEmpty(peerId))
                    {
                        _logger.Warn("[Lan] Empty Identity frame, ignoring.");
                        return;
                    }

                    int key = source.GetHashCode();
                    if (isHostSide)
                    {
                        _connectionPlayerIds[key] = peerId;
                        PeerConnected?.Invoke(peerId);
                    }
                    else
                    {
                        _hostPeerId = peerId;
                        _hostHelloReceived = true;
                        PeerConnected?.Invoke(peerId);
                    }
                }

                private void HandleUserData(NetworkConnection source, byte[] body, bool isHostSide)
                {
                    if (!TryParseUserData(body, out string target, out string senderId, out byte[] payload))
                    {
                        _logger.Warn("[Lan] Malformed UserData frame.");
                        return;
                    }

                    DispatchUserMessage(senderId, payload);
                    if (!isHostSide) return;

                    byte[] wireFrame = BuildUserDataFrame(senderId, target, payload);
                    if (string.IsNullOrWhiteSpace(target) || target == "*")
                    {
                        for (int i = 0; i < _serverConnections.Length; i++)
                        {
                            var c = _serverConnections[i];
                            if (!c.IsCreated || c == source) continue;
                            SendFrame(c, wireFrame);
                        }
                    }
                    else if (target != _localPeerId && TryFindConnectionByPlayerId(target, out var dest))
                    {
                        SendFrame(dest, wireFrame);
                    }
                }

                private Task SendViaLanAsync(string targetPeerId, byte[] payload, CancellationToken ct)
                {
                    if (_driver == null || !_driver.IsCreated)
                    {
                        _logger.Warn("[Lan] SendMessage ignored: driver is not created.");
                        return Task.CompletedTask;
                    }

                    var safePayload = payload ?? Array.Empty<byte>();
                    var frame = BuildUserDataFrame(_localPeerId, targetPeerId, safePayload);

                    if (_isHost)
                    {
                        if (string.IsNullOrWhiteSpace(targetPeerId) || targetPeerId == "*")
                        {
                            for (int i = 0; i < _serverConnections.Length; i++)
                            {
                                var c = _serverConnections[i];
                                if (c.IsCreated)
                                    SendFrame(c, frame);
                            }
                        }
                        else if (TryFindConnectionByPlayerId(targetPeerId, out var target))
                        {
                            SendFrame(target, frame);
                        }

                        DispatchUserMessage(_localPeerId, safePayload);
                    }
                    else
                    {
                        SendFrame(_serverConnection, frame);
                    }

                    return Task.CompletedTask;
                }

                private void SendFrame(NetworkConnection connection, byte[] frame)
                {
                    if (!_driver.IsCreated || !connection.IsCreated || frame == null) return;
                    if (frame.Length > MaxFrameBodyBytes + 3) { _logger.Warn($"[Lan] Frame too large ({frame.Length}B), dropping."); return; }

                    if (_driver.BeginSend(connection, out var writer) != 0)
                    {
                        _logger.Warn("[Lan] BeginSend failed.");
                        return;
                    }

                    var buffer = new NativeArray<byte>(frame, Allocator.Temp);
                    writer.WriteBytes(buffer);
                    buffer.Dispose();
                    _driver.EndSend(writer);
                }

                private static bool TryReadFrame(UtpDataStreamReader stream, out byte type, out byte[] body)
                {
                    type = 0; body = null;
                    if (stream.Length < 3) return false;

                    type = stream.ReadByte();
                    ushort len = stream.ReadUShort();
                    if (len > MaxFrameBodyBytes) return false;
                    if (stream.Length - stream.GetBytesRead() < len) return false;

                    body = new byte[len];
                    if (len > 0)
                    {
                        var buffer = new NativeArray<byte>(len, Allocator.Temp);
                        stream.ReadBytes(buffer);
                        buffer.CopyTo(body);
                        buffer.Dispose();
                    }
                    return true;
                }

                private static byte[] BuildHelloFrame(string peerId)
                {
                    var idBytes = System.Text.Encoding.UTF8.GetBytes(peerId ?? string.Empty);
                    var body = new byte[4 + idBytes.Length];
                    Buffer.BlockCopy(BitConverter.GetBytes((uint)1), 0, body, 0, 4);
                    if (idBytes.Length > 0) Buffer.BlockCopy(idBytes, 0, body, 4, idBytes.Length);
                    return WrapFrame(FrameHello, body);
                }

                private static byte[] BuildIdentityFrame(string peerId)
                {
                    return WrapFrame(FrameIdentity, System.Text.Encoding.UTF8.GetBytes(peerId ?? string.Empty));
                }

                private static byte[] BuildUserDataFrame(string senderId, string targetPeerId, byte[] payload)
                {
                    var targetBytes = System.Text.Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(targetPeerId) ? "*" : targetPeerId);
                    var senderBytes = System.Text.Encoding.UTF8.GetBytes(senderId ?? string.Empty);
                    var p = payload ?? Array.Empty<byte>();

                    var body = new byte[2 + targetBytes.Length + 2 + senderBytes.Length + p.Length];
                    int offset = 0;
                    WriteUShort(body, ref offset, (ushort)targetBytes.Length);
                    Buffer.BlockCopy(targetBytes, 0, body, offset, targetBytes.Length); offset += targetBytes.Length;
                    WriteUShort(body, ref offset, (ushort)senderBytes.Length);
                    Buffer.BlockCopy(senderBytes, 0, body, offset, senderBytes.Length); offset += senderBytes.Length;
                    if (p.Length > 0) Buffer.BlockCopy(p, 0, body, offset, p.Length);
                    return WrapFrame(FrameUserData, body);
                }

                private static bool TryParseUserData(byte[] body, out string target, out string senderId, out byte[] payload)
                {
                    target = null; senderId = null; payload = null;
                    if (body == null || body.Length < 4) return false;

                    int offset = 0;
                    ushort tLen = ReadUShort(body, ref offset);
                    if (offset + tLen > body.Length) return false;
                    target = System.Text.Encoding.UTF8.GetString(body, offset, tLen); offset += tLen;

                    if (offset + 2 > body.Length) return false;
                    ushort sLen = ReadUShort(body, ref offset);
                    if (offset + sLen > body.Length) return false;
                    senderId = System.Text.Encoding.UTF8.GetString(body, offset, sLen); offset += sLen;

                    int payloadLen = body.Length - offset;
                    payload = new byte[payloadLen];
                    if (payloadLen > 0) Buffer.BlockCopy(body, offset, payload, 0, payloadLen);
                    return true;
                }

                private static byte[] WrapFrame(byte type, byte[] body)
                {
                    if (body == null) body = Array.Empty<byte>();
                    var frame = new byte[3 + body.Length];
                    frame[0] = type;
                    frame[1] = (byte)(body.Length & 0xFF);
                    frame[2] = (byte)((body.Length >> 8) & 0xFF);
                    if (body.Length > 0) Buffer.BlockCopy(body, 0, frame, 3, body.Length);
                    return frame;
                }

                private static void WriteUShort(byte[] buf, ref int offset, ushort value)
                {
                    buf[offset++] = (byte)(value & 0xFF);
                    buf[offset++] = (byte)((value >> 8) & 0xFF);
                }

                private static ushort ReadUShort(byte[] buf, ref int offset)
                {
                    ushort v = (ushort)(buf[offset] | (buf[offset + 1] << 8));
                    offset += 2;
                    return v;
                }

                private bool TryFindConnectionByPlayerId(string playerId, out NetworkConnection connection)
                {
                    foreach (var kv in _connectionPlayerIds)
                    {
                        if (kv.Value == playerId)
                        {
                            for (int i = 0; i < _serverConnections.Length; i++)
                            {
                                var c = _serverConnections[i];
                                if (c.IsCreated && c.GetHashCode() == kv.Key)
                                {
                                    connection = c;
                                    return true;
                                }
                            }
                        }
                    }
                    connection = default;
                    return false;
                }

                private void DispatchUserMessage(string senderId, byte[] payload)
                {
                    var msg = new NetworkMessage(senderId ?? string.Empty, payload ?? Array.Empty<byte>());
                    for (int i = _observers.Count - 1; i >= 0; i--)
                    {
                        try { _observers[i].OnNext(msg); }
                        catch (Exception e) { _logger.Warn($"[Lan] Observer error: {e.Message}"); }
                    }
                }

                private async Task ShutdownTransportAsync()
                {
                    if (_pumpCts != null)
                    {
                        _pumpCts.Cancel();
                        try { if (_pumpTask != null) await _pumpTask; }
                        catch (OperationCanceledException) { /* expected */ }
                        _pumpTask = null;
                        _pumpCts.Dispose();
                        _pumpCts = null;
                    }

                    try
                    {
                        if (_driver.IsCreated)
                        {
                            if (_isHost && _serverConnections.IsCreated)
                            {
                                for (int i = 0; i < _serverConnections.Length; i++)
                                    if (_serverConnections[i].IsCreated)
                                        _driver.Disconnect(_serverConnections[i]);
                            }
                            else if (_serverConnection.IsCreated)
                            {
                                _driver.Disconnect(_serverConnection);
                            }

                            _driver.ScheduleUpdate().Complete();
                            _driver.Dispose();
                        }
                    }
                    catch { }

                    try { if (_serverConnections.IsCreated) _serverConnections.Dispose(); } catch { }

                    _serverConnection = default;
                    _connectionPlayerIds.Clear();
                    _hostHelloReceived = false;
                    _hostPeerId = null;
                    _isHost = false;
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
