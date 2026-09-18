// RelayNetworkProvider — Unity Gaming Services Relay backend (Unity 6).
//
// SETUP:
//   1. Install packages:  com.unity.services.relay, com.unity.transport, com.unity.services.authentication, com.unity.services.core
//   2. Add scripting define:  MOYVA_UGS_RELAY  (Project Settings -> Player)
//   3. Unity Dashboard: enable Authentication + Relay.
//
// Protocol (length-prefixed binary frames):
//   [byte type][ushort bodyLength][body ... ]
//   type:
//     1 Hello      - body: uint32 protocolVersion + utf8 playerId
//     2 Identity   - body: utf8 playerId (sender announces id)
//     3 UserData   - body: ushort targetLen + targetUtf8 + ushort senderLen + senderUtf8 + payload
//     4 Bye        - body: (empty)
//
// PeerConnected event fires with the real PlayerId only after the Identity frame
// is received. Without MOYVA_UGS_RELAY the provider compiles as a graceful stub.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;
#if MOYVA_UGS_RELAY
using Unity.Collections;
using Unity.Networking.Transport;
using UtpDataStreamReader = Unity.Collections.DataStreamReader;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
#endif

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Unity Relay backend — cloud NAT traversal via UGS Relay + Unity Transport.
    /// </summary>
    public sealed class RelayNetworkProvider :
        INetworkProvider,
        INetworkPeerIdentityConfigurator,
        IDisposable
    {
        public static bool IsRuntimeAvailable
        {
            get
            {
#if MOYVA_UGS_RELAY
                return true;
#else
                return false;
#endif
            }
        }

        public static bool TryValidateReflectionBindings(out string error)
        {
            return RelayReflectionCache.TryValidate(out error);
        }

        // v2: frames run through ReliableTransportChannel (sequence header),
        // so peers on protocol v1 are rejected cleanly at handshake.
        public const uint ProtocolVersion = 2;
        private const byte FrameHello = 1;
        private const byte FrameIdentity = 2;
        private const byte FrameUserData = 3;
        private const byte FrameBye = 4;

        private const int MaxFrameBodyBytes = 60 * 1024;
        private const int HandshakeTimeoutMs = 15_000;
        private readonly RelayProviderSettings _settings;
        private readonly MultiplayerTransportPump _transportPump;
        private readonly List<IObserver<NetworkMessage>> _observers = new List<IObserver<NetworkMessage>>();
        private string _configuredLocalPeerId;

    #pragma warning disable CS0067
        public event Action<string> PeerConnected;
        public event Action<string> PeerDisconnected;
    #pragma warning restore CS0067
        public IObservable<NetworkMessage> Messages => new MessageObservable(_observers);

        public RelayNetworkProvider(RelayProviderSettings settings)
        {
            _settings = settings ?? RelayProviderSettings.Default();
            _transportPump = new MultiplayerTransportPump();
        }

        public void SetLocalPeerId(string playerId)
        {
            _configuredLocalPeerId = string.IsNullOrWhiteSpace(playerId)
                ? null
                : playerId.Trim();
        }

        public async Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default)
        {
#if MOYVA_UGS_RELAY
            return await HostViaRelayAsync(ct);
#else
            return await Task.FromResult(UgsNotAvailable());
#endif
        }

        public async Task<SessionResult> JoinSessionAsync(string joinCode, CancellationToken ct = default)
        {
#if MOYVA_UGS_RELAY
            return await JoinViaRelayAsync(joinCode, ct);
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

#if MOYVA_UGS_RELAY
        private const string RelayConnectionType = "dtls";

        private NetworkDriver _driver;
        private NetworkConnection _serverConnection;
        private NativeList<NetworkConnection> _serverConnections;
        private readonly Dictionary<int, string> _connectionPlayerIds = new Dictionary<int, string>();
        private volatile bool _hostHelloReceived;
        private volatile bool _isHost;
        private string _localPeerId;
        private volatile string _hostPeerId;
        private ReliableTransportChannel _reliableChannel;
        private readonly Dictionary<NetworkConnection, string> _helloPeerIds = new();
        private readonly List<NetworkConnection> _expiredHandshakes = new();
        private volatile string _transportError;
        private volatile bool _transportActive;
        private volatile int _relayStatus;

        // Outbound user messages are enqueued by any thread and drained on the
        // transport pump thread, which exclusively owns the driver.
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

        private async Task<SessionResult> HostViaRelayAsync(CancellationToken ct)
        {
            try
            {
                await EnsureRelayReadyAsync();
                _localPeerId = ResolveLocalPeerId(
                    AuthenticationService.Instance.PlayerId);

                await ShutdownTransportAsync();
                _hostPeerId = _localPeerId;

                var relayService = ResolveRelayServiceInstance();
                var allocation = await CreateAllocationAsync(
                    relayService,
                    _settings.MaxConnections,
                    string.IsNullOrEmpty(_settings.Region) ? null : _settings.Region);

                var allocationId = GetPropertyValue<Guid>(allocation, "AllocationId");
                var relayServerData = BuildRelayServerData(allocation, RelayConnectionType, isHostAllocation: true);
                _driver = CreateRelayDriver(ref relayServerData);
                _reliableChannel = new ReliableTransportChannel(_driver);
                _serverConnections = new NativeList<NetworkConnection>(
                    Math.Max(_settings.MaxConnections, 4), Allocator.Persistent);

                if (_driver.Bind(NetworkEndpoint.AnyIpv4) != 0)
                    return await FailAndShutdownAsync("Relay host bind failed.");

                if (_driver.Listen() != 0)
                    return await FailAndShutdownAsync("Relay host listen failed.");

                _isHost = true;
                Application.runInBackground = true;
                _transportActive = true;
                StartPumpLoop(ct);
                var deadline = DateTime.UtcNow.AddMilliseconds(HandshakeTimeoutMs);
                while (_relayStatus != (int)RelayConnectionStatus.Established)
                {
                    ct.ThrowIfCancellationRequested();
                    if (_relayStatus == (int)RelayConnectionStatus.AllocationInvalid)
                        return await FailAndShutdownAsync("Relay host allocation expired or was rejected before binding.");
                    if (DateTime.UtcNow >= deadline)
                        return await FailAndShutdownAsync("Relay host binding timeout. Could not reach the Relay server.");
                    await Task.Delay(50, ct);
                }
                var joinCode = await GetJoinCodeAsync(relayService, allocationId);
                if (!RelayJoinCodeUtility.IsValid(joinCode))
                    return await FailAndShutdownAsync("Relay returned an invalid join code.");
                RaisePeerConnected(_localPeerId);
                return SessionResult.Ok(joinCode);
            }
            catch (Exception e)
            {
                await SafeShutdownAfterFailureAsync();
                return SessionResult.Fail(e.Message);
            }
        }

        private async Task<SessionResult> JoinViaRelayAsync(string joinCode, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(joinCode))
                    return SessionResult.Fail("Relay join code is empty.");

                var normalizedJoinCode = joinCode.Trim();
                if (!RelayJoinCodeUtility.IsValid(normalizedJoinCode))
                    return SessionResult.Fail($"Relay join code '{normalizedJoinCode}' is invalid. Expected 6-12 chars from '6789BCDFGHJKLMNPQRTW'.");

                await EnsureRelayReadyAsync();
                _localPeerId = ResolveLocalPeerId(
                    AuthenticationService.Instance.PlayerId);

                await ShutdownTransportAsync();

                var relayService = ResolveRelayServiceInstance();
                var joinAllocation = await JoinAllocationAsync(relayService, normalizedJoinCode);

                var relayServerData = BuildRelayServerData(joinAllocation, RelayConnectionType, isHostAllocation: false);
                _driver = CreateRelayDriver(ref relayServerData);
                _reliableChannel = new ReliableTransportChannel(_driver);

                if (_driver.Bind(NetworkEndpoint.AnyIpv4) != 0)
                    return await FailAndShutdownAsync("Relay client bind failed.");

                _serverConnection = _driver.Connect();
                if (!_serverConnection.IsCreated)
                    return await FailAndShutdownAsync("Relay client connect request failed.");

                _isHost = false;
                Application.runInBackground = true;
                _transportActive = true;
                StartPumpLoop(ct);

                var deadline = DateTime.UtcNow.AddMilliseconds(HandshakeTimeoutMs);
                while (!_hostHelloReceived)
                {
                    if (ct.IsCancellationRequested) return await FailAndShutdownAsync("Join cancelled.");
                    if (_relayStatus == (int)RelayConnectionStatus.AllocationInvalid)
                        return await FailAndShutdownAsync("Relay allocation expired or was rejected. Ask the host to recreate the room.");
                    if (!string.IsNullOrEmpty(_transportError)) return await FailAndShutdownAsync(_transportError);
                    if (DateTime.UtcNow > deadline) return await FailAndShutdownAsync("Relay handshake timeout.");
                    await Task.Delay(50, ct);
                }
                return SessionResult.Ok(normalizedJoinCode);
            }
            catch (OperationCanceledException)
            {
                await SafeShutdownAfterFailureAsync();
                return SessionResult.Fail("Join cancelled.");
            }
            catch (Exception e)
            {
                await SafeShutdownAfterFailureAsync();
                return SessionResult.Fail(e.Message);
            }
        }

        private async Task LeaveRelayAsync(CancellationToken ct)
        {
            var localId = _localPeerId;
            await ShutdownTransportAsync();
            if (!string.IsNullOrEmpty(localId))
                RaisePeerDisconnected(localId);
        }

        private async Task<SessionResult> FailAndShutdownAsync(string message)
        {
            await SafeShutdownAfterFailureAsync();
            return SessionResult.Fail(message);
        }

        private async Task SafeShutdownAfterFailureAsync()
        {
            try
            {
                await ShutdownTransportAsync();
            }
            catch (Exception)
            {
                CleanupTransportImmediate();
            }
        }

        private Task SendViaRelayAsync(string targetPeerId, byte[] payload, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return Task.FromCanceled(ct);
            if (!_transportActive)
                return Task.FromException(
                    new InvalidOperationException("Relay session is not connected."));

            _outbound.Enqueue(
                new OutboundMessage(
                    targetPeerId,
                    payload ?? Array.Empty<byte>()));
            return Task.CompletedTask;
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
                    Debug.LogWarning(
                        $"[Relay Transport] Peer '{targetPeerId}' is not connected; message dropped.");
                }

                PostUserMessage(_localPeerId, safePayload);
            }
            else
            {
                SendFrame(_serverConnection, frame);
            }
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
                        Debug.LogWarning(
                            "[Relay Transport] Dropping a message; the send retry queue is full.");
                    break;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        $"[Relay Transport] Send failed: {exception.Message}");
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

        private async Task EnsureRelayReadyAsync()
        {
            await MultiplayerAuthenticationGate.EnsureReadyAsync();
        }

        private static object ResolveRelayServiceInstance()
        {
            return RelayReflectionCache.GetRelayServiceInstance();
        }

        private static async Task<object> CreateAllocationAsync(object relayService, int maxConnections, string region)
        {
            return await InvokeRelayMethodAsync(relayService, "CreateAllocationAsync", maxConnections, region);
        }

        private static async Task<string> GetJoinCodeAsync(object relayService, Guid allocationId)
        {
            var result = await InvokeRelayMethodAsync(relayService, "GetJoinCodeAsync", allocationId);
            return result as string ?? throw new InvalidOperationException("Relay GetJoinCodeAsync returned null.");
        }

        private static async Task<object> JoinAllocationAsync(object relayService, string joinCode)
        {
            // SDK 1.1+: JoinAllocationAsync(string joinCode) — try direct string overload first.
            // SDK 1.0.x: JoinAllocationAsync(JoinAllocationArgs args) — fall back if string overload not found.
            try
            {
                return await InvokeRelayMethodAsync(relayService, "JoinAllocationAsync", joinCode);
            }
            catch (MissingMethodException)
            {
                var joinArgs = CreateJoinAllocationArgs(joinCode);
                return await InvokeRelayMethodAsync(relayService, "JoinAllocationAsync", joinArgs);
            }
        }

        private static object CreateJoinAllocationArgs(string joinCode)
        {
            var argsType =
                Type.GetType("Unity.Services.Relay.Models.JoinAllocationArgs, Unity.Services.Relay")
                ?? Type.GetType("Unity.Services.Relay.Models.JoinAllocationArgs, Unity.Services.Multiplayer");

            if (argsType == null)
                throw new InvalidOperationException(
                    "JoinAllocationArgs type not found. Cannot join Relay allocation.");

            var instance = Activator.CreateInstance(argsType);
            var joinCodeProp = argsType.GetProperty("JoinCode",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (joinCodeProp == null)
                throw new InvalidOperationException("JoinAllocationArgs.JoinCode property not found.");

            joinCodeProp.SetValue(instance, joinCode);
            return instance;
        }

        private static async Task<object> InvokeRelayMethodAsync(object relayService, string methodName, params object[] args)
        {
            if (relayService == null)
                throw new ArgumentNullException(nameof(relayService));

            var method = RelayReflectionCache.ResolveRelayMethod(methodName, args);

            var invoked = method.Invoke(relayService, args);
            if (invoked is not Task task)
                throw new InvalidOperationException($"{methodName} did not return Task.");

            await task.ConfigureAwait(false);

            var taskType = task.GetType();
            if (taskType.IsGenericType)
                return taskType.GetProperty("Result")?.GetValue(task);

            return null;
        }

        private static NetworkDriver CreateRelayDriver(ref RelayServerData relayServerData)
        {
            var settings = new NetworkSettings();
            try
            {
                settings.WithRelayParameters(ref relayServerData);
                ReliableTransportChannel.Configure(ref settings);
                return NetworkDriver.Create(settings);
            }
            finally { settings.Dispose(); }
        }

        private static RelayServerData BuildRelayServerData(object allocation, string connectionType, bool isHostAllocation)
        {
            var endpoints = GetPropertyValue<System.Collections.IEnumerable>(allocation, "ServerEndpoints");
            object selectedEndpoint = null;
            foreach (var endpoint in endpoints)
            {
                var endpointType = GetPropertyValue<string>(endpoint, "ConnectionType");
                if (string.Equals(endpointType, connectionType, StringComparison.OrdinalIgnoreCase))
                {
                    selectedEndpoint = endpoint;
                    break;
                }
            }

            if (selectedEndpoint == null)
                throw new InvalidOperationException($"Relay allocation does not contain a '{connectionType}' endpoint.");

            var host = GetPropertyValue<string>(selectedEndpoint, "Host");
            var port = Convert.ToUInt16(GetPropertyValue<int>(selectedEndpoint, "Port"));
            var secure = GetPropertyValue<bool>(selectedEndpoint, "Secure");

            var allocationIdBytes = GetPropertyValue<byte[]>(allocation, "AllocationIdBytes");
            var connectionData = GetPropertyValue<byte[]>(allocation, "ConnectionData");
            var key = GetPropertyValue<byte[]>(allocation, "Key");
            var hostConnectionData = isHostAllocation
                ? connectionData
                : GetPropertyValue<byte[]>(allocation, "HostConnectionData");

            return new RelayServerData(
                host,
                port,
                allocationIdBytes,
                connectionData,
                hostConnectionData,
                key,
                secure);
        }

        private static T GetPropertyValue<T>(object source, string propertyName)
        {
            return RelayReflectionCache.ReadProperty<T>(source, propertyName);
        }

        private void StartPumpLoop(CancellationToken externalCt)
        {
            _transportPump.Start(
                // The connect token owns setup only. The established transport
                // belongs to the session and stops explicitly on leave/dispose.
                CancellationToken.None,
                () => _driver.IsCreated,
                PumpTransportOnce);
        }

        private void PumpTransportOnce()
        {
            _driver.ScheduleUpdate().Complete();
            _relayStatus = (int)_driver.GetRelayConnectionStatus();
            DrainOutbound();
            if (_isHost)
                PumpHost();
            else
                PumpClient();
            _reliableChannel?.Flush(_driver, FailTransportPeer);
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
                        _transportError ??= "Relay host disconnected during the session.";
                        if (!string.IsNullOrEmpty(_hostPeerId))
                            RaisePeerDisconnected(_hostPeerId);
                        break;
                }
            }
        }

        private const int MaximumIdentityBytes = 256;

        private void HandleFrame(NetworkConnection source, UtpDataStreamReader stream, bool isHostSide)
        {
            if (_reliableChannel == null || _driver.GetConnectionState(source) != NetworkConnection.State.Connected)
                return;
            if (!_reliableChannel.TryReadFrame(source, ref stream, out var channelError))
            {
                if (channelError != null)
                    FailTransportPeer(source, channelError);
                return;
            }
            if (!TryReadFrame(stream, out byte type, out byte[] body))
            {
                FailTransportPeer(source, "Invalid Relay message frame.");
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
                FailTransportPeer(source, "Invalid Relay handshake identity.");
                return;
            }

            uint version = BitConverter.ToUInt32(body, 0);
            if (version != ProtocolVersion)
            {
                FailTransportPeer(source, "Relay game versions are incompatible.");
                return;
            }

            string peerId = body.Length > 4 ? Encoding.UTF8.GetString(body, 4, body.Length - 4) : string.Empty;
            if (string.IsNullOrWhiteSpace(peerId) || string.Equals(peerId, _localPeerId, StringComparison.Ordinal))
            {
                FailTransportPeer(source, "Relay peer identity conflicts with the local player.");
                return;
            }
            if (_helloPeerIds.TryGetValue(source, out var previousIdentity))
            {
                if (!string.Equals(previousIdentity, peerId, StringComparison.Ordinal))
                    FailTransportPeer(source, "Relay peer tried to change its identity.");
                return;
            }
            _helloPeerIds.Add(source, peerId);
            if (isHostSide)
            {
                SendControlFrame(source, BuildHelloFrame(_localPeerId));
                SendControlFrame(source, BuildIdentityFrame(_localPeerId));
            }
            else if (!string.IsNullOrEmpty(peerId))
            {
                _hostPeerId = peerId;
            }
        }

        private void HandleIdentity(NetworkConnection source, byte[] body, bool isHostSide)
        {
            string peerId = body != null && body.Length > 0 ? Encoding.UTF8.GetString(body) : string.Empty;
            if (string.IsNullOrWhiteSpace(peerId) || body.Length > MaximumIdentityBytes ||
                !_helloPeerIds.TryGetValue(source, out var helloIdentity) ||
                !string.Equals(helloIdentity, peerId, StringComparison.Ordinal))
            {
                FailTransportPeer(source, "Relay identity does not match the handshake.");
                return;
            }

            int key = source.GetHashCode();
            if (isHostSide)
            {
                if (_connectionPlayerIds.TryGetValue(key, out var existingIdentity))
                {
                    if (!string.Equals(existingIdentity, peerId, StringComparison.Ordinal))
                        FailTransportPeer(source, "Relay peer tried to change its identity.");
                    return;
                }
                if (TryFindConnectionByPlayerId(peerId, out var existingConnection) && existingConnection != source)
                {
                    FailTransportPeer(source, "Another connected Relay player already uses this identity.");
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
                FailTransportPeer(source, "Relay gameplay message arrived before the host handshake.");
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

        private string ResolveLocalPeerId(string authenticatedPlayerId)
        {
            if (!string.IsNullOrWhiteSpace(authenticatedPlayerId))
                return authenticatedPlayerId.Trim();
            if (!string.IsNullOrWhiteSpace(_configuredLocalPeerId))
                return _configuredLocalPeerId;
            return $"local-{Guid.NewGuid():N}";
        }

        private void SendFrame(NetworkConnection connection, byte[] frame)
        {
            if (_reliableChannel == null)
                throw new InvalidOperationException("Relay transport is not initialized.");
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
            Debug.LogWarning($"[Relay Transport] {reason}");
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
            var idBytes = Encoding.UTF8.GetBytes(peerId ?? string.Empty);
            var body = new byte[4 + idBytes.Length];
            Buffer.BlockCopy(BitConverter.GetBytes(ProtocolVersion), 0, body, 0, 4);
            if (idBytes.Length > 0)
                Buffer.BlockCopy(idBytes, 0, body, 4, idBytes.Length);
            return MultiplayerFrameCodec.Wrap(
                FrameHello,
                body);
        }

        private static byte[] BuildIdentityFrame(string peerId)
        {
            return MultiplayerFrameCodec.Wrap(
                FrameIdentity,
                Encoding.UTF8.GetBytes(
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

                try
                {
                    _driver.ScheduleUpdate().Complete();
                }
                catch (Exception)
                {
                }

                _driver.Dispose();
            }

            if (_serverConnections.IsCreated)
                _serverConnections.Dispose();

            _driver = default;
            _serverConnection = default;
            _connectionPlayerIds.Clear();
            _reliableChannel = null;
            _helloPeerIds.Clear();
            _expiredHandshakes.Clear();
            _transportError = null;
            _hostHelloReceived = false;
            _hostPeerId = null;
            _isHost = false;
        }

        private void CleanupTransportImmediate()
        {
            _transportActive = false;
            _transportPump.Dispose();
            while (_outbound.TryDequeue(out _)) { }
            _sendRetry.Clear();

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

            _driver = default;
            _serverConnection = default;
            _connectionPlayerIds.Clear();
            _reliableChannel = null;
            _helloPeerIds.Clear();
            _expiredHandshakes.Clear();
            _transportError = null;
            _hostHelloReceived = false;
            _hostPeerId = null;
            _isHost = false;
        }

#endif

        public void Dispose()
        {
    #if MOYVA_UGS_RELAY
            CleanupTransportImmediate();
    #endif
        }

        private SessionResult UgsNotAvailable()
        {
            const string msg = "Unity Relay SDK not installed. Add com.unity.services.relay + com.unity.transport " +
                               "and enable MOYVA_UGS_RELAY scripting define.";
            return SessionResult.Fail(msg);
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
