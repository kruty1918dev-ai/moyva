using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Runtime;
using System.Net;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Unity.Collections;
using Unity.Networking.Transport;
using UtpDataStreamReader = Unity.Collections.DataStreamReader;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// LAN network provider skeleton.
    ///
    /// Real implementation uses Netcode for GameObjects (NGO) CustomMessagingManager
    /// or a UDP transport. This provider uses Unity Transport primitives and assumes
    /// the necessary Unity Transport / Netcode packages are installed in the project.
    /// </summary>
    public sealed class LanNetworkProvider :
        INetworkProvider,
        INetworkPeerIdentityConfigurator
    {
        private readonly MultiplayerConfig _config;
        private readonly MultiplayerTransportPump _transportPump;
        private string _configuredLocalPeerId;

        // Observers subscribed to this provider's Messages
        private readonly List<IObserver<NetworkMessage>> _observers = new List<IObserver<NetworkMessage>>();

        public event Action<string> PeerConnected;
        public event Action<string> PeerDisconnected;

        public IObservable<NetworkMessage> Messages => new MessageObservable(_observers);

        public LanNetworkProvider(MultiplayerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _transportPump = new MultiplayerTransportPump();
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

        public void SetLocalPeerId(string playerId)
        {
            _configuredLocalPeerId = string.IsNullOrWhiteSpace(playerId)
                ? null
                : playerId.Trim();
        }

    // LAN transport implementation using Unity Transport (NetworkDriver).
                private const byte FrameHello = 1;
                private const byte FrameIdentity = 2;
                private const byte FrameUserData = 3;
                private const byte FrameBye = 4;

                private const int MaxFrameBodyBytes = 60 * 1024;
                private const int HandshakeTimeoutMs = 10_000;

                private NetworkDriver _driver;
                private NetworkConnection _serverConnection;
                private NativeList<NetworkConnection> _serverConnections;
                private readonly Dictionary<int, string> _connectionPlayerIds = new Dictionary<int, string>();
                private bool _isHost;
                private string _localPeerId;
                private string _hostPeerId;
                private bool _hostHelloReceived;

                private async Task<SessionResult> HostViaLanAsync(string sessionId, CancellationToken ct)
                {
                    try
                    {
                        _localPeerId = ResolveLocalPeerId();

                        await ShutdownTransportAsync();

                        var netSettings = new NetworkSettings();
                        _driver = NetworkDriver.Create(netSettings);
                        _serverConnections = new NativeList<NetworkConnection>(Math.Max(4, 4), Allocator.Persistent);

                        var endpoint = NetworkEndpoint.AnyIpv4.WithPort((ushort)LanLobbyService.DefaultPort);
                        if (_driver.Bind(endpoint) != 0)
                            return SessionResult.Fail("LAN host bind failed.");

                        if (_driver.Listen() != 0)
                            return SessionResult.Fail("LAN host listen failed.");

                        _isHost = true;
                        StartPumpLoop(ct);
                        PeerConnected?.Invoke(_localPeerId);

                        var ip = GetLocalIPAddress() ?? "127.0.0.1";
                        var joinCode = $"lan:{ip}:{LanLobbyService.DefaultPort}";
                        return SessionResult.Ok(joinCode);
                    }
                    catch (Exception e)
                    {
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

                        _localPeerId = ResolveLocalPeerId();

                        await ShutdownTransportAsync();

                        var netSettings = new NetworkSettings();
                        _driver = NetworkDriver.Create(netSettings);

                        var ep = default(NetworkEndpoint);
                        if (!NetworkEndpoint.TryParse(ip, port, out ep))
                        {
                            // Fallback: try parse via DNS
                            ep = NetworkEndpoint.AnyIpv4.WithPort(port);
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
                        return SessionResult.Ok(joinCode);
                    }
                    catch (OperationCanceledException)
                    {
                        return SessionResult.Fail("Join cancelled.");
                    }
                    catch (Exception e)
                    {
                        return SessionResult.Fail(e.Message);
                    }
                }

                private void StartPumpLoop(CancellationToken externalCt)
                {
                    _transportPump.Start(
                        externalCt,
                        () => _driver.IsCreated,
                        PumpTransportOnce);
                }

                private void PumpTransportOnce()
                {
                    _driver.ScheduleUpdate().Complete();
                    if (_isHost)
                        PumpHost();
                    else
                        PumpClient();
                }

                private void PumpHost()
                {
                    MultiplayerFrameCodec.PumpHostConnections(
                        ref _driver,
                        ref _serverConnections,
                        _connectionPlayerIds,
                        HandleFrame,
                        PeerDisconnected);
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
                                SendFrame(_serverConnection, BuildHelloFrame(_localPeerId));
                                SendFrame(_serverConnection, BuildIdentityFrame(_localPeerId));
                                break;
                            case NetworkEvent.Type.Data:
                                HandleFrame(_serverConnection, stream, isHostSide: false);
                                break;
                            case NetworkEvent.Type.Disconnect:
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
                        return;
                    }

                    switch (type)
                    {
                        case FrameHello:    HandleHello(source, body, isHostSide); break;
                        case FrameIdentity: HandleIdentity(source, body, isHostSide); break;
                        case FrameUserData: HandleUserData(source, body, isHostSide); break;
                        case FrameBye:      break;
                        default:
                            break;
                    }
                }

                private void HandleHello(NetworkConnection source, byte[] body, bool isHostSide)
                {
                    if (body == null || body.Length < 4)
                    {
                        return;
                    }

                    uint version = BitConverter.ToUInt32(body, 0);
                    if (version != 1)
                    {
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
                    if (!MultiplayerFrameCodec.TryParseUserData(
                            body,
                            out string target,
                            out string senderId,
                            out byte[] payload))
                    {
                        return;
                    }

                    if (isHostSide)
                    {
                        int connectionKey = source.GetHashCode();
                        if (!_connectionPlayerIds.TryGetValue(
                                connectionKey,
                                out string authoritativeSenderId)
                            || string.IsNullOrWhiteSpace(
                                authoritativeSenderId))
                        {
                            return;
                        }

                        if (!string.Equals(
                                senderId,
                                authoritativeSenderId,
                                StringComparison.Ordinal))
                        {
                        }

                        senderId = authoritativeSenderId;
                    }

                    DispatchUserMessage(senderId, payload);
                    if (!isHostSide) return;

                    byte[] wireFrame =
                        MultiplayerFrameCodec.BuildUserDataFrame(
                            FrameUserData,
                            senderId,
                            target,
                            payload);
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
                    if (!_driver.IsCreated)
                    {
                        return Task.CompletedTask;
                    }

                    var safePayload = payload ?? Array.Empty<byte>();
                    var frame =
                        MultiplayerFrameCodec.BuildUserDataFrame(
                            FrameUserData,
                            _localPeerId,
                            targetPeerId,
                            safePayload);

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
                    if (frame.Length > MaxFrameBodyBytes + 3) { return; }

                    if (_driver.BeginSend(connection, out var writer) != 0)
                    {
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
                    return MultiplayerFrameCodec.Wrap(
                        FrameHello,
                        body);
                }

                private static string BuildLocalPeerId()
                {
                    var machineName = string.IsNullOrWhiteSpace(Environment.MachineName) ? "local" : Environment.MachineName;
                    return MultiplayerClientScope.IsDefault ? machineName : $"{machineName}-{MultiplayerClientScope.ScopeId}";
                }

                private string ResolveLocalPeerId()
                    => string.IsNullOrWhiteSpace(_configuredLocalPeerId)
                        ? BuildLocalPeerId()
                        : _configuredLocalPeerId;

                private static byte[] BuildIdentityFrame(string peerId)
                {
                    return MultiplayerFrameCodec.Wrap(
                        FrameIdentity,
                        System.Text.Encoding.UTF8.GetBytes(
                            peerId ?? string.Empty));
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
                        catch (Exception) { }
                    }
                }

                private async Task ShutdownTransportAsync()
                {
                    await _transportPump.StopAsync();

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
