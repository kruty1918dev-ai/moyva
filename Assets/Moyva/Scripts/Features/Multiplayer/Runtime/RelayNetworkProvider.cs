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
    public sealed class RelayNetworkProvider : INetworkProvider, IDisposable
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

        public const uint ProtocolVersion = 1;
        private const byte FrameHello = 1;
        private const byte FrameIdentity = 2;
        private const byte FrameUserData = 3;
        private const byte FrameBye = 4;

        private const int MaxFrameBodyBytes = 60 * 1024;
        private const int HandshakeTimeoutMs = 15_000;
        private const int PumpDelayMs = 16;

        private readonly RelayProviderSettings _settings;
        private readonly IMultiplayerLogger _logger;
        private readonly List<IObserver<NetworkMessage>> _observers = new List<IObserver<NetworkMessage>>();

    #pragma warning disable CS0067
        public event Action<string> PeerConnected;
        public event Action<string> PeerDisconnected;
    #pragma warning restore CS0067
        public IObservable<NetworkMessage> Messages => new MessageObservable(_observers);

        public RelayNetworkProvider(RelayProviderSettings settings, IMultiplayerLogger logger)
        {
            _settings = settings ?? RelayProviderSettings.Default();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        private bool _hostHelloReceived;
        private bool _isHost;
        private string _localPeerId;
        private string _hostPeerId;
        private CancellationTokenSource _pumpCts;
        private Task _pumpTask;

        private async Task<SessionResult> HostViaRelayAsync(CancellationToken ct)
        {
            try
            {
                await EnsureRelayReadyAsync();
                _localPeerId = AuthenticationService.Instance.PlayerId ?? $"local-{Guid.NewGuid():N}";
                _hostPeerId = _localPeerId;

                await ShutdownTransportAsync();

                var relayService = ResolveRelayServiceInstance();
                var allocation = await CreateAllocationAsync(
                    relayService,
                    _settings.MaxConnections,
                    string.IsNullOrEmpty(_settings.Region) ? null : _settings.Region);

                var allocationId = GetPropertyValue<Guid>(allocation, "AllocationId");
                var joinCode = await GetJoinCodeAsync(relayService, allocationId);
                if (!RelayJoinCodeUtility.IsValid(joinCode))
                    return await FailAndShutdownAsync($"Relay GetJoinCodeAsync returned invalid join code '{joinCode ?? string.Empty}'.");

                _logger.Info($"[Relay] Hosted allocation. Join code: {joinCode}");

                var relayServerData = BuildRelayServerData(allocation, RelayConnectionType, isHostAllocation: true);
                var netSettings = new NetworkSettings();
                netSettings.WithRelayParameters(ref relayServerData);

                _driver = NetworkDriver.Create(netSettings);
                _serverConnections = new NativeList<NetworkConnection>(
                    Math.Max(_settings.MaxConnections, 4), Allocator.Persistent);

                if (_driver.Bind(NetworkEndpoint.AnyIpv4) != 0)
                    return await FailAndShutdownAsync("Relay host bind failed.");

                if (_driver.Listen() != 0)
                    return await FailAndShutdownAsync("Relay host listen failed.");

                _isHost = true;
                StartPumpLoop(ct);
                PeerConnected?.Invoke(_localPeerId);
                return SessionResult.Ok(joinCode);
            }
            catch (Exception e)
            {
                await SafeShutdownAfterFailureAsync();
                _logger.Error($"[Relay] HostAsync failed: {e.Message}");
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
                _localPeerId = AuthenticationService.Instance.PlayerId ?? $"local-{Guid.NewGuid():N}";

                await ShutdownTransportAsync();

                var relayService = ResolveRelayServiceInstance();
                var joinAllocation = await JoinAllocationAsync(relayService, normalizedJoinCode);

                var relayServerData = BuildRelayServerData(joinAllocation, RelayConnectionType, isHostAllocation: false);
                var netSettings = new NetworkSettings();
                netSettings.WithRelayParameters(ref relayServerData);

                _driver = NetworkDriver.Create(netSettings);

                if (_driver.Bind(NetworkEndpoint.AnyIpv4) != 0)
                    return await FailAndShutdownAsync("Relay client bind failed.");

                _serverConnection = _driver.Connect();
                if (!_serverConnection.IsCreated)
                    return await FailAndShutdownAsync("Relay client connect request failed.");

                _isHost = false;
                StartPumpLoop(ct);

                var deadline = DateTime.UtcNow.AddMilliseconds(HandshakeTimeoutMs);
                while (!_hostHelloReceived)
                {
                    if (ct.IsCancellationRequested) return await FailAndShutdownAsync("Join cancelled.");
                    if (DateTime.UtcNow > deadline) return await FailAndShutdownAsync("Relay handshake timeout.");
                    await Task.Delay(50, ct);
                }

                _logger.Info($"[Relay] Joined allocation. Host={_hostPeerId}");
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
                _logger.Error($"[Relay] JoinAsync failed: {e.Message}");
                return SessionResult.Fail(e.Message);
            }
        }

        private async Task LeaveRelayAsync(CancellationToken ct)
        {
            var localId = _localPeerId;
            await ShutdownTransportAsync();
            if (!string.IsNullOrEmpty(localId))
                PeerDisconnected?.Invoke(localId);
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
            catch (Exception shutdownError)
            {
                _logger.Warn($"[Relay] Shutdown after failure also failed: {shutdownError.Message}");
                CleanupTransportImmediate();
            }
        }

        private Task SendViaRelayAsync(string targetPeerId, byte[] payload, CancellationToken ct)
        {
            if (!_driver.IsCreated)
            {
                _logger.Warn("[Relay] SendMessage ignored: driver is not created.");
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

        private async Task EnsureRelayReadyAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
                _logger.Info("[Relay] Unity Services initialized.");
            }

            MultiplayerClientScope.ApplyAuthenticationProfileIfNeeded(_logger);

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                _logger.Info("[Relay] Signed in anonymously.");
            }
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

        private static RelayServerData BuildRelayServerData(object allocation, string connectionType, bool isHostAllocation)
        {
            var endpoints = GetPropertyValue<System.Collections.IEnumerable>(allocation, "ServerEndpoints");
            object selectedEndpoint = null;
            object firstEndpoint = null;
            foreach (var endpoint in endpoints)
            {
                firstEndpoint ??= endpoint;
                var endpointType = GetPropertyValue<string>(endpoint, "ConnectionType");
                if (string.Equals(endpointType, connectionType, StringComparison.OrdinalIgnoreCase))
                {
                    selectedEndpoint = endpoint;
                    break;
                }
            }

            selectedEndpoint ??= firstEndpoint;
            if (selectedEndpoint == null)
                throw new InvalidOperationException("Relay allocation does not contain server endpoints.");

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
                    _logger.Warn($"[Relay] Pump error: {e.Message}");
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
                        _logger.Info("[Relay] Client connected to host; sending Hello+Identity.");
                        SendFrame(_serverConnection, BuildHelloFrame(_localPeerId));
                        SendFrame(_serverConnection, BuildIdentityFrame(_localPeerId));
                        break;
                    case NetworkEvent.Type.Data:
                        HandleFrame(_serverConnection, stream, isHostSide: false);
                        break;
                    case NetworkEvent.Type.Disconnect:
                        _logger.Warn("[Relay] Client disconnected from host.");
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
                _logger.Warn("[Relay] Failed to read frame.");
                return;
            }

            switch (type)
            {
                case FrameHello:    HandleHello(source, body, isHostSide); break;
                case FrameIdentity: HandleIdentity(source, body, isHostSide); break;
                case FrameUserData: HandleUserData(source, body, isHostSide); break;
                case FrameBye:      break;
                default:
                    _logger.Warn($"[Relay] Unknown frame type {type}.");
                    break;
            }
        }

        private void HandleHello(NetworkConnection source, byte[] body, bool isHostSide)
        {
            if (body == null || body.Length < 4)
            {
                _logger.Warn("[Relay] Invalid Hello frame.");
                return;
            }

            uint version = BitConverter.ToUInt32(body, 0);
            if (version != ProtocolVersion)
            {
                _logger.Warn($"[Relay] Protocol mismatch: remote={version}, local={ProtocolVersion}. Dropping.");
                _driver.Disconnect(source);
                return;
            }

            string peerId = body.Length > 4 ? Encoding.UTF8.GetString(body, 4, body.Length - 4) : string.Empty;
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
            string peerId = body != null && body.Length > 0 ? Encoding.UTF8.GetString(body) : string.Empty;
            if (string.IsNullOrEmpty(peerId))
            {
                _logger.Warn("[Relay] Empty Identity frame, ignoring.");
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
                _logger.Warn("[Relay] Malformed UserData frame.");
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

        private void SendFrame(NetworkConnection connection, byte[] frame)
        {
            if (!_driver.IsCreated || !connection.IsCreated || frame == null) return;
            if (frame.Length > MaxFrameBodyBytes + 3)
            {
                _logger.Warn($"[Relay] Frame too large ({frame.Length}B), dropping.");
                return;
            }

            if (_driver.BeginSend(connection, out var writer) != 0)
            {
                _logger.Warn("[Relay] BeginSend failed.");
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
            var idBytes = Encoding.UTF8.GetBytes(peerId ?? string.Empty);
            var body = new byte[4 + idBytes.Length];
            Buffer.BlockCopy(BitConverter.GetBytes(ProtocolVersion), 0, body, 0, 4);
            if (idBytes.Length > 0)
                Buffer.BlockCopy(idBytes, 0, body, 4, idBytes.Length);
            return WrapFrame(FrameHello, body);
        }

        private static byte[] BuildIdentityFrame(string peerId)
        {
            return WrapFrame(FrameIdentity, Encoding.UTF8.GetBytes(peerId ?? string.Empty));
        }

        private static byte[] BuildUserDataFrame(string senderId, string targetPeerId, byte[] payload)
        {
            var targetBytes = Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(targetPeerId) ? "*" : targetPeerId);
            var senderBytes = Encoding.UTF8.GetBytes(senderId ?? string.Empty);
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
            target = Encoding.UTF8.GetString(body, offset, tLen); offset += tLen;

            if (offset + 2 > body.Length) return false;
            ushort sLen = ReadUShort(body, ref offset);
            if (offset + sLen > body.Length) return false;
            senderId = Encoding.UTF8.GetString(body, offset, sLen); offset += sLen;

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
                catch (Exception e) { _logger.Warn($"[Relay] Observer error: {e.Message}"); }
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
                catch (Exception e)
                {
                    _logger.Warn($"[Relay] Final transport update failed during shutdown: {e.Message}");
                }

                _driver.Dispose();
            }

            if (_serverConnections.IsCreated)
                _serverConnections.Dispose();

            _driver = default;
            _serverConnection = default;
            _connectionPlayerIds.Clear();
            _hostHelloReceived = false;
            _hostPeerId = null;
            _isHost = false;
        }

        private void CleanupTransportImmediate()
        {
            try
            {
                _pumpCts?.Cancel();
            }
            catch { }

            try
            {
                _pumpCts?.Dispose();
            }
            catch { }

            _pumpCts = null;
            _pumpTask = null;

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
            _logger.Warn($"[Relay] {msg}");
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
