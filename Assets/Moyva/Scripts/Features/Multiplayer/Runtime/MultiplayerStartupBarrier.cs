using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    // Project lifetime: the handshake survives unloading the menu scene.
    public sealed class MultiplayerStartupBarrier : IMultiplayerStartupBarrier, IInitializable, IDisposable
    {
        private const byte Request = 1;
        private const byte Ready = 2;
        private readonly IGameCommandSyncService _commands;
        private readonly INetworkProvider _network;
        private readonly ISessionManager _session;
        private readonly IWorldGenerationSignalState _world;
        private readonly Stopwatch _elapsed = new Stopwatch();
        private byte[] _request;
        private string _hostId;
        private long _previousWorldSequence;
        private int _attempt;
        private bool _localReady;
        private bool _hostConfirmed;
        private bool _hostDisconnected;
        private bool _disposed;
        private bool _isHost;

        public bool IsHostReady { get; private set; }
        public bool IsReadyToPlay => !_disposed && !_hostDisconnected && _localReady &&
            (_isHost ? IsHostReady : _hostConfirmed);

        public MultiplayerStartupBarrier(IGameCommandSyncService commands, INetworkProvider network,
            ISessionManager session, IWorldGenerationSignalState world)
        {
            _commands = commands;
            _network = network;
            _session = session;
            _world = world;
        }

        public void Initialize()
        {
            _commands.RegisterHandler(GameCommandType.MatchStartSync, OnHandshake);
            _network.PeerDisconnected += OnPeerDisconnected;
        }

        public void BeginStartup()
        {
            _attempt++;
            _localReady = false;
            _hostConfirmed = false;
            _hostDisconnected = false;
            IsHostReady = false;
            _isHost = _session.IsLocalPlayerHost;
            _hostId = null;
            foreach (var participant in _session.Participants)
                if (participant?.IsHost == true)
                    _hostId = participant.Identity.PlayerId;
            _world.TryGetCurrentWorldIdentity(out _previousWorldSequence, out _);
            // Echoing a fresh nonce prevents delayed replies from releasing a new load.
            _request = new byte[17];
            _request[0] = Request;
            Buffer.BlockCopy(Guid.NewGuid().ToByteArray(), 0, _request, 1, 16);
            _elapsed.Restart();
        }

        public async Task WaitForLocalWorldAsync(CancellationToken ct)
        {
            int attempt = _attempt;
            long worldSequence = 0;
            double nextRequest = 0;
            while (true)
            {
                CheckWait(attempt, ct);
                if (worldSequence == 0 && _world.TryGetCurrentWorldIdentity(out long current, out _) &&
                    current > _previousWorldSequence && _world.TryGetWorldGeneratedData(out var generated))
                    worldSequence = generated.StartupSequence;

                if (worldSequence > 0 && _world.TryGetWorldSpawnPositions(out var spawns) &&
                    spawns.StartupSequence == worldSequence && HasLocalAssignment(spawns))
                {
                    _localReady = true;
                    return;
                }

                if (worldSequence > 0 && !_isHost && _elapsed.Elapsed.TotalSeconds >= nextRequest)
                {
                    // A slower client may have missed spawns before its world cycle began.
                    _commands.SendCommandToPeer(_hostId, GameCommandType.StartingPositions, Array.Empty<byte>());
                    nextRequest = _elapsed.Elapsed.TotalSeconds + 1;
                }
                await Task.Delay(100, ct);
            }
        }

        public async Task WaitForHostAsync(CancellationToken ct)
        {
            int attempt = _attempt;
            while (!_hostConfirmed)
            {
                CheckWait(attempt, ct);
                _commands.SendCommandToPeer(_hostId, GameCommandType.MatchStartSync, _request);
                await Task.Delay(250, ct);
            }
            CheckWait(attempt, ct);
        }

        public void MarkHostReady()
        {
            if (!_localReady || !_isHost || !_session.IsLocalPlayerHost)
                throw new InvalidOperationException("The host world must be ready before releasing clients.");
            IsHostReady = true;
        }

        private bool HasLocalAssignment(WorldSpawnPositionsSignal signal)
        {
            if (signal.Assignments != null)
                foreach (var assignment in signal.Assignments)
                    if (string.Equals(assignment.ParticipantId, _session.LocalPlayerId, StringComparison.Ordinal))
                        return true;
            return false;
        }

        private void OnHandshake(string senderId, byte[] payload)
        {
            if (_disposed || payload == null || payload.Length != 17)
                return;
            if (payload[0] == Request && IsHostReady && _isHost && _session.IsLocalPlayerHost)
            {
                foreach (var participant in _session.Participants)
                {
                    if (participant?.Identity?.PlayerId != senderId || participant.IsHost)
                        continue;
                    var response = (byte[])payload.Clone();
                    response[0] = Ready;
                    _commands.SendCommandToPeer(senderId, GameCommandType.MatchStartSync, response);
                    return;
                }
            }
            if (payload[0] != Ready || _isHost || _request == null || senderId != _hostId)
                return;
            for (int i = 1; i < payload.Length; i++)
                if (payload[i] != _request[i])
                    return;
            _hostConfirmed = true;
        }

        private void OnPeerDisconnected(string peerId)
        {
            if (!_isHost && peerId == _hostId)
                _hostDisconnected = true;
        }

        private void CheckWait(int attempt, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (_disposed || attempt != _attempt)
                throw new OperationCanceledException("Gameplay loading was cancelled.");
            if (_hostDisconnected)
                throw new InvalidOperationException("The host disconnected while loading the world.");
            if (_elapsed.Elapsed.TotalSeconds >= 180)
                throw new TimeoutException("The multiplayer world did not become ready within three minutes.");
        }

        public void Dispose()
        {
            _disposed = true;
            _network.PeerDisconnected -= OnPeerDisconnected;
            _commands.RegisterHandler(GameCommandType.MatchStartSync, null);
        }
    }
}
