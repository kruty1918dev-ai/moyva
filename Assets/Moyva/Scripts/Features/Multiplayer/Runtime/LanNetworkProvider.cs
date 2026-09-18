using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Runtime;
using System.Net;
using System.Net.Sockets;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Unity.Collections;
using Unity.Networking.Transport;
using UtpDataStreamReader = Unity.Collections.DataStreamReader;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// LAN transport with reliable, ordered delivery over Unity Transport.
    /// </summary>
    public sealed class LanNetworkProvider :
        INetworkProvider,
        INetworkPeerIdentityConfigurator,
        IDisposable
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
            return LanLobbyService.GetPreferredLocalIPAddress();
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
                private const int HandshakeTimeoutMs = 30_000;
                private const uint ProtocolVersion = 2;
                private const int MaximumIdentityBytes = 256;
                private const int HostPortSearchCount = 32;

                private NetworkDriver _driver;
                private NetworkConnection _serverConnection;
                private NativeList<NetworkConnection> _serverConnections;
                private readonly Dictionary<int, string> _connectionPlayerIds = new Dictionary<int, string>();
                private volatile bool _isHost;
                private string _localPeerId;
                private volatile string _hostPeerId;
                private volatile bool _hostHelloReceived;
                private ReliableTransportChannel _reliableChannel;
                private readonly Dictionary<NetworkConnection, string> _helloPeerIds = new();
                private readonly List<NetworkConnection> _expiredHandshakes = new();
                private volatile string _transportError;
                private volatile bool _transportActive;

                // Outbound user messages are enqueued by any thread and drained
                // on the transport pump thread, which exclusively owns the driver.
                private const int MaxOutboundPerTick = 32;
                private const int MaxRetryQueue = 256;
                private readonly System.Collections.Concurrent.ConcurrentQueue<OutboundMessage> _outbound =
                    new System.Collections.Concurrent.ConcurrentQueue<OutboundMessage>();
                private readonly Queue<OutboundMessage> _sendRetry = new Queue<OutboundMessage>();

                private readonly struct OutboundMessage
                {
                    public readonly string TargetPeerId;
                    public readonly byte[] Payload;

                    public OutboundMessage(string targetPeerId, byte[] payload)
                    {
                        TargetPeerId = targetPeerId;
                        Payload = payload;
                    }
                }

                private async Task<SessionResult> HostViaLanAsync(string sessionId, CancellationToken ct)
                {
                    try
                    {
                        _localPeerId = ResolveLocalPeerId();

                        await ShutdownTransportAsync();

                        if (!TryStartHostDriver(out var boundPort, out var error))
                            return SessionResult.Fail(error);

                        _isHost = true;
                        _transportActive = true;
                        StartPumpLoop();
                        RaisePeerConnected(_localPeerId);

                        var ip = GetLocalIPAddress() ?? "127.0.0.1";
                        var joinCode = $"lan:{ip}:{boundPort}";
                        return SessionResult.Ok(joinCode);
                    }
                    catch (Exception e)
                    {
                        return SessionResult.Fail(e.Message);
                    }
                }

                private bool TryStartHostDriver(out ushort boundPort, out string error)
                {
                    boundPort = 0;
                    error = null;

                    for (var i = 0; i < HostPortSearchCount; i++)
                    {
                        var candidatePort = LanLobbyService.DefaultPort + i;
                        if (candidatePort > ushort.MaxValue)
                            break;

                        if (!IsUdpPortAvailable(candidatePort))
                            continue;

                        var netSettings = new NetworkSettings();
                        ReliableTransportChannel.Configure(ref netSettings);
                        var candidateDriver = NetworkDriver.Create(netSettings);
                        netSettings.Dispose();
                        var candidateChannel = new ReliableTransportChannel(candidateDriver);
                        var candidateConnections = new NativeList<NetworkConnection>(4, Allocator.Persistent);
                        var endpoint = NetworkEndpoint.AnyIpv4.WithPort((ushort)candidatePort);

                        if (candidateDriver.Bind(endpoint) != 0)
                        {
                            DisposeCandidate(candidateDriver, candidateConnections);
                            continue;
                        }

                        if (candidateDriver.Listen() != 0)
                        {
                            DisposeCandidate(candidateDriver, candidateConnections);
                            continue;
                        }

                        _driver = candidateDriver;
                        _reliableChannel = candidateChannel;
                        _serverConnections = candidateConnections;
                        boundPort = (ushort)candidatePort;
                        return true;
                    }

                    error = $"LAN host bind failed. No free UDP port found in range {LanLobbyService.DefaultPort}-{LanLobbyService.DefaultPort + HostPortSearchCount - 1}.";
                    return false;
                }

                private static bool IsUdpPortAvailable(int port)
                {
                    try
                    {
                        using (var client = new UdpClient(AddressFamily.InterNetwork))
                        {
                            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
                            return true;
                        }
                    }
                    catch (SocketException)
                    {
                        return false;
                    }
                    catch (ObjectDisposedException)
                    {
                        return false;
                    }
                }

                private static void DisposeCandidate(NetworkDriver driver, NativeList<NetworkConnection> connections)
                {
                    try
                    {
                        if (driver.IsCreated)
                            driver.Dispose();
                    }
                    catch { }

                    try
                    {
                        if (connections.IsCreated)
                            connections.Dispose();
                    }
                    catch { }
                }

                private async Task<SessionResult> JoinViaLanAsync(string joinCode, CancellationToken ct)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(joinCode) || !joinCode.StartsWith("lan:", StringComparison.OrdinalIgnoreCase))
                            return SessionResult.Fail("Invalid LAN join code.");

                        var parts = joinCode.Split(':');
                        if (parts.Length < 3) return SessionResult.Fail("Invalid LAN join code format.");
                        var ip = parts[1];
                        if (!ushort.TryParse(parts[2], out var port)) return SessionResult.Fail("Invalid LAN port.");

                        _localPeerId = ResolveLocalPeerId();

                        await ShutdownTransportAsync();

                        var netSettings = new NetworkSettings();
                        ReliableTransportChannel.Configure(ref netSettings);
                        _driver = NetworkDriver.Create(netSettings);
                        netSettings.Dispose();
                        _reliableChannel = new ReliableTransportChannel(_driver);

                        var ep = default(NetworkEndpoint);
                        if (!NetworkEndpoint.TryParse(ip, port, out ep))
                        {
                            return SessionResult.Fail($"Invalid LAN host address '{ip}'.");
                        }

                        _serverConnection = _driver.Connect(ep);
                        _isHost = false;
                        _transportActive = true;
                        StartPumpLoop();

                        var deadline = DateTime.UtcNow.AddMilliseconds(HandshakeTimeoutMs);
                        while (!_hostHelloReceived)
                        {
                            if (ct.IsCancellationRequested) return SessionResult.Fail("Join cancelled.");
                            if (!string.IsNullOrEmpty(_transportError)) return SessionResult.Fail(_transportError);
                            if (DateTime.UtcNow > deadline) return SessionResult.Fail("LAN handshake timed out after 30 seconds. Check that both players use the same game version.");
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
                    finally
                    {
                        if (!_hostHelloReceived)
                            await ShutdownTransportAsync();
                    }
                }

                private void StartPumpLoop()
                {
                    _transportPump.Start(
                        CancellationToken.None,
                        () => _driver.IsCreated,
                        PumpTransportOnce);
                }

                private void PumpTransportOnce()
                {
                    _driver.ScheduleUpdate().Complete();
                    DrainOutbound();
                    if (_isHost)
                        PumpHost();
                    else
                        PumpClient();
                    _reliableChannel?.Flush(_driver, FailTransportPeer);
                }

                private void DrainOutbound()
                {
                    var budget = MaxOutboundPerTick;
                    while (budget-- > 0 && TryDequeueOutbound(out var message))
                    {
                        try
                        {
                            DeliverOutbound(message.TargetPeerId, message.Payload);
                        }
                        catch (TransportSendQueueFullException)
                        {
                            // Back-pressure: retry next tick ahead of new traffic.
                            if (_sendRetry.Count < MaxRetryQueue)
                                _sendRetry.Enqueue(message);
                            else
                                UnityEngine.Debug.LogWarning(
                                    "[LAN Transport] Dropping a message; the send retry queue is full.");
                            break;
                        }
                        catch (Exception exception)
                        {
                            UnityEngine.Debug.LogWarning(
                                $"[LAN Transport] Send failed: {exception.Message}");
                        }
                    }
                }

                private bool TryDequeueOutbound(out OutboundMessage message)
                {
                    if (_sendRetry.Count > 0)
                    {
                        message = _sendRetry.Dequeue();
                        return true;
                    }

                    return _outbound.TryDequeue(out message);
                }

                private void DeliverOutbound(string targetPeerId, byte[] safePayload)
                {
                    var frame = MultiplayerFrameCodec.BuildUserDataFrame(
                        FrameUserData,
                        _localPeerId,
                        targetPeerId,
                        safePayload);

                    if (_isHost)
                    {
                        if (string.IsNullOrWhiteSpace(targetPeerId) || targetPeerId == "*")
                        {
                            var recipientCount = 0;
                            for (var i = 0; i < _serverConnections.Length; i++)
                                if (IsAuthenticatedConnection(_serverConnections[i]))
                                    recipientCount++;
                            _reliableChannel.EnsureCapacity(frame.Length, recipientCount);
                            for (int i = 0; i < _serverConnections.Length; i++)
                            {
                                var c = _serverConnections[i];
                                if (IsAuthenticatedConnection(c))
                                    SendFrame(c, frame);
                            }
                        }
                        else if (TryFindConnectionByPlayerId(targetPeerId, out var target))
                        {
                            SendFrame(target, frame);
                        }
                        else if (!string.Equals(targetPeerId, _localPeerId, StringComparison.Ordinal))
                        {
                            UnityEngine.Debug.LogWarning(
                                $"[LAN Transport] Peer '{targetPeerId}' is not connected; message dropped.");
                        }

                        PostUserMessage(_localPeerId, safePayload);
                    }
                    else
                    {
                        SendFrame(_serverConnection, frame);
                    }
                }

                private void PumpHost()
                {
                    MultiplayerFrameCodec.PumpHostConnections(
                        ref _driver,
                        ref _serverConnections,
                        _connectionPlayerIds,
                        HandleFrame,
                        RaisePeerDisconnected);
                    _expiredHandshakes.Clear();
                    foreach (var pair in _helloPeerIds)
                        if (_driver.GetConnectionState(pair.Key) == NetworkConnection.State.Disconnected)
                            _expiredHandshakes.Add(pair.Key);
                    foreach (var connection in _expiredHandshakes)
                    {
                        _helloPeerIds.Remove(connection);
                        _reliableChannel?.Forget(connection);
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
                                SendControlFrame(_serverConnection, BuildHelloFrame(_localPeerId));
                                SendControlFrame(_serverConnection, BuildIdentityFrame(_localPeerId));
                                break;
                            case NetworkEvent.Type.Data:
                                HandleFrame(_serverConnection, stream, isHostSide: false);
                                break;
                            case NetworkEvent.Type.Disconnect:
                                _reliableChannel?.Forget(_serverConnection);
                                _helloPeerIds.Remove(_serverConnection);
                                _serverConnection = default;
                                _transportActive = false;
                                _transportError ??= "LAN host disconnected during the connection.";
                                if (!string.IsNullOrEmpty(_hostPeerId))
                                    RaisePeerDisconnected(_hostPeerId);
                                break;
                        }
                    }
                }

                private void HandleFrame(NetworkConnection source, UtpDataStreamReader stream, bool isHostSide)
                {
                    if (_reliableChannel == null || _driver.GetConnectionState(source) != NetworkConnection.State.Connected)
                        return;
                    if (!_reliableChannel.TryReadFrame(source, ref stream, out var error))
                    {
                        if (error != null)
                            FailTransportPeer(source, error);
                        return;
                    }
                    if (!TryReadFrame(stream, out byte type, out byte[] body))
                    {
                        FailTransportPeer(source, "Invalid LAN message frame.");
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
                    if (body == null || body.Length <= 4 || body.Length > MaximumIdentityBytes + 4)
                    {
                        FailTransportPeer(source, "Invalid LAN handshake identity.");
                        return;
                    }

                    uint version = BitConverter.ToUInt32(body, 0);
                    if (version != ProtocolVersion)
                    {
                        FailTransportPeer(source, "LAN game versions are incompatible.");
                        return;
                    }

                    string peerId = body.Length > 4 ? System.Text.Encoding.UTF8.GetString(body, 4, body.Length - 4) : string.Empty;
                    if (string.IsNullOrWhiteSpace(peerId) || string.Equals(peerId, _localPeerId, StringComparison.Ordinal))
                    {
                        FailTransportPeer(source, "LAN peer identity conflicts with the local player.");
                        return;
                    }
                    if (_helloPeerIds.TryGetValue(source, out var previousIdentity))
                    {
                        if (!string.Equals(previousIdentity, peerId, StringComparison.Ordinal))
                            FailTransportPeer(source, "LAN peer tried to change its identity.");
                        return;
                    }
                    _helloPeerIds.Add(source, peerId);
                    if (isHostSide)
                    {
                        SendControlFrame(source, BuildHelloFrame(_localPeerId));
                        SendControlFrame(source, BuildIdentityFrame(_localPeerId));
                    }
                }

                private void HandleIdentity(NetworkConnection source, byte[] body, bool isHostSide)
                {
                    string peerId = body != null && body.Length > 0 ? System.Text.Encoding.UTF8.GetString(body) : string.Empty;
                    if (string.IsNullOrWhiteSpace(peerId) || body.Length > MaximumIdentityBytes ||
                        !_helloPeerIds.TryGetValue(source, out var helloIdentity) ||
                        !string.Equals(helloIdentity, peerId, StringComparison.Ordinal))
                    {
                        FailTransportPeer(source, "LAN identity does not match the handshake.");
                        return;
                    }

                    int key = source.GetHashCode();
                    if (isHostSide)
                    {
                        if (_connectionPlayerIds.TryGetValue(key, out var existingIdentity))
                        {
                            if (!string.Equals(existingIdentity, peerId, StringComparison.Ordinal))
                                FailTransportPeer(source, "LAN peer tried to change its identity.");
                            return;
                        }
                        if (TryFindConnectionByPlayerId(peerId, out var existingConnection) && existingConnection != source)
                        {
                            FailTransportPeer(source, "Another connected LAN player already uses this identity.");
                            return;
                        }
                        _connectionPlayerIds[key] = peerId;
                        RaisePeerConnected(peerId);
                    }
                    else
                    {
                        if (_hostHelloReceived)
                            return;
                        _hostPeerId = peerId;
                        _hostHelloReceived = true;
                        RaisePeerConnected(peerId);
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

                        senderId = authoritativeSenderId;
                    }
                    else if (!_hostHelloReceived)
                    {
                        FailTransportPeer(source, "LAN gameplay message arrived before the host handshake.");
                        return;
                    }

                    PostUserMessage(senderId, payload);
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
                            if (!IsAuthenticatedConnection(c) || c == source) continue;
                            SendControlFrame(c, wireFrame);
                        }
                    }
                    else if (target != _localPeerId && TryFindConnectionByPlayerId(target, out var dest))
                    {
                        SendControlFrame(dest, wireFrame);
                    }
                }

                private Task SendViaLanAsync(string targetPeerId, byte[] payload, CancellationToken ct)
                {
                    if (ct.IsCancellationRequested)
                        return Task.FromCanceled(ct);
                    if (!_transportActive)
                        return Task.FromException(
                            new InvalidOperationException("LAN session is not connected."));

                    _outbound.Enqueue(
                        new OutboundMessage(
                            targetPeerId,
                            payload ?? Array.Empty<byte>()));
                    return Task.CompletedTask;
                }

                private void SendFrame(NetworkConnection connection, byte[] frame)
                {
                    if (_reliableChannel == null)
                        throw new InvalidOperationException("LAN transport is not initialized.");
                    _reliableChannel.Send(_driver, connection, frame);
                }

                private void SendControlFrame(NetworkConnection connection, byte[] frame)
                {
                    try { SendFrame(connection, frame); }
                    catch (Exception exception) { FailTransportPeer(connection, exception.Message); }
                }

                private bool IsAuthenticatedConnection(NetworkConnection connection)
                    => connection.IsCreated && _driver.GetConnectionState(connection) == NetworkConnection.State.Connected &&
                       _connectionPlayerIds.ContainsKey(connection.GetHashCode());

                private void FailTransportPeer(NetworkConnection connection, string reason)
                {
                    UnityEngine.Debug.LogWarning($"[LAN Transport] {reason}");
                    _reliableChannel?.Forget(connection);
                    _helloPeerIds.Remove(connection);
                    if (_driver.IsCreated && connection.IsCreated)
                        _driver.Disconnect(connection);
                    if (!_isHost)
                    {
                        _transportError = reason;
                        _transportActive = false;
                        _serverConnection = default;
                        if (!string.IsNullOrEmpty(_hostPeerId))
                            RaisePeerDisconnected(_hostPeerId);
                    }
                    else if (_connectionPlayerIds.TryGetValue(connection.GetHashCode(), out var peerId))
                    {
                        _connectionPlayerIds.Remove(connection.GetHashCode());
                        RaisePeerDisconnected(peerId);
                    }
                }

                private static bool TryReadFrame(UtpDataStreamReader stream, out byte type, out byte[] body)
                {
                    type = 0; body = null;
                    if (stream.Length - stream.GetBytesRead() < 3) return false;

                    type = stream.ReadByte();
                    ushort len = stream.ReadUShort();
                    if (len > MaxFrameBodyBytes) return false;
                    if (stream.Length - stream.GetBytesRead() != len) return false;

                    body = new byte[len];
                    if (len > 0)
                    {
                        var buffer = new NativeArray<byte>(len, Allocator.TempJob);
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
                    Buffer.BlockCopy(BitConverter.GetBytes(ProtocolVersion), 0, body, 0, 4);
                    if (idBytes.Length > 0) Buffer.BlockCopy(idBytes, 0, body, 4, idBytes.Length);
                    return MultiplayerFrameCodec.Wrap(
                        FrameHello,
                        body);
                }

                private static string BuildLocalPeerId()
                {
                    return Kruty1918.Moyva.Multiplayer.Lobbies.LanLobbyService.BuildLocalHostId();
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
                                if (IsAuthenticatedConnection(c) && c.GetHashCode() == kv.Key)
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

                private void RaisePeerConnected(string peerId)
                {
                    MultiplayerThreadContext.Post(() => PeerConnected?.Invoke(peerId));
                }

                private void RaisePeerDisconnected(string peerId)
                {
                    MultiplayerThreadContext.Post(() => PeerDisconnected?.Invoke(peerId));
                }

                // Messages always reach observers on Unity's main thread; the
                // pump thread never runs gameplay or UI callbacks.
                private void PostUserMessage(string senderId, byte[] payload)
                {
                    var msg = new NetworkMessage(senderId ?? string.Empty, payload ?? Array.Empty<byte>());
                    MultiplayerThreadContext.Post(() => DispatchUserMessage(msg));
                }

                private void DispatchUserMessage(NetworkMessage message)
                {
                    for (int i = _observers.Count - 1; i >= 0; i--)
                    {
                        try { _observers[i].OnNext(message); }
                        catch (Exception) { }
                    }
                }

                private async Task ShutdownTransportAsync()
                {
                    _transportActive = false;
                    await _transportPump.StopAsync();
                    while (_outbound.TryDequeue(out _)) { }
                    _sendRetry.Clear();

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

                    _driver = default;
                    _serverConnection = default;
                    _reliableChannel?.Clear();
                    _reliableChannel = null;
                    _helloPeerIds.Clear();
                    _expiredHandshakes.Clear();
                    _transportError = null;
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

        // Dispose is the teardown path (provider switch / app quit): the pump
        // is stopped with a bounded wait before the native driver is released,
        // so no tick can touch a deallocated driver.
        public void Dispose()
        {
            _transportActive = false;
            _transportPump.Stop();

            try
            {
                if (_driver.IsCreated)
                    _driver.Dispose();
            }
            catch { }

            try
            {
                if (_serverConnections.IsCreated)
                    _serverConnections.Dispose();
            }
            catch { }

            while (_outbound.TryDequeue(out _)) { }
            _sendRetry.Clear();
            _serverConnection = default;
            _driver = default;
            _reliableChannel = null;
            _helloPeerIds.Clear();
            _expiredHandshakes.Clear();
            _connectionPlayerIds.Clear();
            _transportError = null;
            _hostHelloReceived = false;
            _hostPeerId = null;
            _isHost = false;
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
