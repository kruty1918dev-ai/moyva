using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingObserverServer : IDisposable
    {
        private readonly object _gate = new object();
        private readonly List<ClientState> _clients = new List<ClientState>();
        private readonly Queue<AgentDecisionEvent> _history = new Queue<AgentDecisionEvent>();
        private readonly int _historyCapacity;
        private readonly int _queueCapacity;
        private readonly int _maxMessageBytes;
        private readonly string _endpoint;
        private readonly CancellationTokenSource _shutdown = new CancellationTokenSource();
        private Thread _acceptThread;
        private Socket _unixListener;
        private NamedPipeServerStream _pendingPipe;
        private bool _started;
        private long _lastSequence;
        private long _resyncCount;
        private string _unixPath;

        public string SessionId { get; }
        public string ResolvedEndpoint { get; private set; }
        public long LastSequence { get { lock (_gate) return _lastSequence; } }
        public long ResyncCount { get { lock (_gate) return _resyncCount; } }

        public TrainingObserverServer(string sessionId, string endpoint, int queueCapacity, int maxMessageBytes)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("Observer session id is required.", nameof(sessionId));
            SessionId = sessionId;
            _endpoint = endpoint;
            _queueCapacity = Math.Max(8, queueCapacity);
            _maxMessageBytes = Math.Max(TrainingObserverProtocol.MinimumMessageBytes, maxMessageBytes);
            _historyCapacity = Math.Max(64, _queueCapacity * 4);
        }

        public void Start()
        {
            lock (_gate)
            {
                if (_started) return;
                _started = true;
            }
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            string pipeName = SanitizeName(_endpoint, "moyva-training-observer");
            ResolvedEndpoint = @"\\.\pipe\" + pipeName;
            _acceptThread = new Thread(() => AcceptNamedPipes(pipeName)) { IsBackground = true, Name = "MoyvaTrainingObserverAccept" };
#else
            string name = SanitizeName(_endpoint, "moyva-training-observer");
            _unixPath = Path.Combine(Path.GetTempPath(), name + ".sock");
            if (_unixPath.Length > 96) _unixPath = Path.Combine(Path.GetTempPath(), name.Substring(0, Math.Min(32, name.Length)) + ".sock");
            TryDeleteUnixSocket(_unixPath);
            _unixListener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            _unixListener.Bind(new UnixDomainSocketEndPoint(_unixPath));
            _unixListener.Listen(16);
            ResolvedEndpoint = _unixPath;
            _acceptThread = new Thread(AcceptUnixSockets) { IsBackground = true, Name = "MoyvaTrainingObserverAccept" };
#endif
            _acceptThread.Start();
        }

        public InMemoryClient CreateInMemoryClient()
        {
            return InMemoryClient.Create(this);
        }

        public void PublishDecision(AgentDecisionEvent source)
        {
            if (source == null) return;
            var entry = CloneDecision(source);
            lock (_gate)
            {
                _lastSequence = Math.Max(_lastSequence, entry.sequence);
                while (_history.Count >= _historyCapacity) _history.Dequeue();
                _history.Enqueue(entry);
                foreach (var client in _clients.ToArray())
                {
                    if (!client.IsSubscribed(entry.arenaId)) continue;
                    if (!string.IsNullOrEmpty(client.SelectedAgent) && !string.Equals(client.SelectedAgent, entry.agentId, StringComparison.Ordinal)) continue;
                    if (client.NeedsSnapshot(entry.arenaId)) continue;
                    if (client.LastEpisodeByArena.TryGetValue(entry.arenaId, out long episode) && episode != entry.episodeId)
                    {
                        MarkClientResync(client, entry.arenaId, entry.episodeId, entry.sequence, "episode-reset");
                        continue;
                    }
                    var envelope = TrainingObserverProtocol.Create(TrainingObserverProtocol.AgentDecision, SessionId,
                        entry.arenaId, entry.episodeId, entry.sequence, entry);
                    if (!client.Outgoing.TryEnqueue(envelope))
                        MarkClientResync(client, entry.arenaId, entry.episodeId, entry.sequence, "outgoing-queue-overflow");
                    else client.LastSequence = Math.Max(client.LastSequence, entry.sequence);
                }
            }
        }

        public void MarkEpisodeReset(int arenaId, long episodeId, long sequence)
        {
            lock (_gate)
            {
                foreach (var client in _clients.ToArray())
                    if (client.IsSubscribed(arenaId)) MarkClientResync(client, arenaId, episodeId, sequence, "episode-reset");
            }
        }

        public int[] CollectSnapshotTargets(IReadOnlyList<int> availableArenaIds, bool includePeriodicVisuals)
        {
            if (availableArenaIds == null) return Array.Empty<int>();
            lock (_gate)
            {
                var available = new HashSet<int>(availableArenaIds);
                var result = new HashSet<int>();
                foreach (var client in _clients)
                {
                    if (client.PendingSnapshots.Remove(-1))
                        foreach (int id in available) if (client.IsSubscribed(id)) client.PendingSnapshots.Add(id);
                    foreach (int id in client.PendingSnapshots) if (available.Contains(id)) result.Add(id);
                    if (!includePeriodicVisuals) continue;
                    foreach (int id in available) if (client.IsSubscribed(id) && client.WantsVisuals(id)) result.Add(id);
                }
                return result.ToArray();
            }
        }

        public void PublishSnapshot(ArenaSnapshot source, bool periodic)
        {
            if (source == null) return;
            lock (_gate)
            {
                _lastSequence = Math.Max(_lastSequence, source.sequence);
                foreach (var client in _clients.ToArray())
                {
                    if (!client.IsSubscribed(source.arenaId)) continue;
                    bool forced = client.PendingSnapshots.Contains(source.arenaId);
                    if (!forced && !(periodic && client.WantsVisuals(source.arenaId))) continue;
                    ArenaSnapshot snapshot = CloneSnapshot(source, client.WantsVisuals(source.arenaId));
                    var envelope = TrainingObserverProtocol.Create(TrainingObserverProtocol.Snapshot, SessionId,
                        snapshot.arenaId, snapshot.episodeId, snapshot.sequence, snapshot);
                    if (!client.Outgoing.TryEnqueue(envelope))
                    {
                        MarkClientResync(client, snapshot.arenaId, snapshot.episodeId, snapshot.sequence, "outgoing-queue-overflow");
                        continue;
                    }
                    client.PendingSnapshots.Remove(snapshot.arenaId);
                    client.LastEpisodeByArena[snapshot.arenaId] = snapshot.episodeId;
                    client.LastSequence = Math.Max(client.LastSequence, snapshot.sequence);
                }
            }
        }

        private void HandleCommand(ClientState client, ObserverCommand command)
        {
            if (command == null) return;
            lock (_gate)
            {
                if (!string.IsNullOrEmpty(command.sessionId) && !string.Equals(command.sessionId, SessionId, StringComparison.Ordinal))
                {
                    client.Outgoing.TryEnqueue(Status(TrainingObserverProtocol.Rejected, command.arenaId, 0, command.lastSequence, "session-id-mismatch"));
                    return;
                }
                switch (command.kind)
                {
                    case ObserverCommandKind.Subscribe:
                        bool gap = command.lastSequence > 0 && !CanResumeAfter(command.lastSequence);
                        client.Subscriptions[command.arenaId] = command.visuals;
                        client.PendingSnapshots.Add(command.arenaId);
                        if (gap) MarkClientResync(client, command.arenaId, 0, command.lastSequence, "sequence-gap");
                        else client.Outgoing.TryEnqueue(Status(TrainingObserverProtocol.Subscribed, command.arenaId, 0, command.lastSequence, "full-snapshot-required"));
                        break;
                    case ObserverCommandKind.Unsubscribe:
                        if (command.arenaId < 0)
                        {
                            client.Subscriptions.Clear(); client.PendingSnapshots.Clear(); client.LastEpisodeByArena.Clear();
                        }
                        else
                        {
                            client.Subscriptions.Remove(command.arenaId); client.PendingSnapshots.Remove(command.arenaId); client.LastEpisodeByArena.Remove(command.arenaId);
                        }
                        client.Outgoing.TryEnqueue(Status(TrainingObserverProtocol.Unsubscribed, command.arenaId, 0, command.lastSequence, "ok"));
                        break;
                    case ObserverCommandKind.RequestFullSnapshot:
                        if (client.IsSubscribed(command.arenaId) || command.arenaId < 0) client.PendingSnapshots.Add(command.arenaId);
                        else client.PendingSnapshots.Add(command.arenaId);
                        break;
                    case ObserverCommandKind.SelectAgent:
                        client.SelectedAgent = string.IsNullOrWhiteSpace(command.agentId) ? null : command.agentId;
                        client.Outgoing.TryEnqueue(Status(TrainingObserverProtocol.AgentSelected, command.arenaId, 0, command.lastSequence, client.SelectedAgent ?? "all"));
                        break;
                }
            }
        }

        private bool CanResumeAfter(long lastSequence)
        {
            if (lastSequence >= _lastSequence) return true;
            if (_history.Count == 0) return false;
            return _history.Peek().sequence <= lastSequence + 1;
        }

        private void MarkClientResync(ClientState client, int arenaId, long episodeId, long sequence, string reason)
        {
            _resyncCount++;
            if (arenaId < 0)
            {
                foreach (int id in client.Subscriptions.Keys) client.PendingSnapshots.Add(id);
                if (client.Subscriptions.ContainsKey(-1)) client.PendingSnapshots.Add(-1);
            }
            else client.PendingSnapshots.Add(arenaId);
            client.Outgoing.Clear();
            client.Outgoing.TryEnqueue(Status(TrainingObserverProtocol.ResyncRequired, arenaId, episodeId, sequence, reason));
        }

        private TrainingObserverEnvelope Status(string type, int arenaId, long episodeId, long sequence, string reason)
            => TrainingObserverProtocol.Create(type, SessionId, arenaId, episodeId, sequence, new ObserverProtocolStatus { reason = reason });

        private bool ProcessInbound(ClientState client, byte[] body, out bool terminalReject)
        {
            terminalReject = false;
            if (!TrainingObserverProtocol.TryDecodeMessage(body, _maxMessageBytes, out var envelope, out string reason))
            {
                client.Outgoing.TryEnqueue(Status(TrainingObserverProtocol.Rejected, -1, 0, 0, reason));
                if (reason != null && reason.StartsWith("Unsupported observer protocol version", StringComparison.Ordinal)) terminalReject = true;
                return false;
            }
            if (!string.IsNullOrEmpty(envelope.sessionId) && !string.Equals(envelope.sessionId, SessionId, StringComparison.Ordinal))
            {
                client.Outgoing.TryEnqueue(Status(TrainingObserverProtocol.Rejected, envelope.arenaId, envelope.episodeId, envelope.sequence, "session-id-mismatch"));
                terminalReject = true;
                return false;
            }
            if (!TrainingObserverProtocol.TryParseCommand(envelope, out var command, out reason))
            {
                client.Outgoing.TryEnqueue(Status(TrainingObserverProtocol.Rejected, envelope.arenaId, envelope.episodeId, envelope.sequence, reason));
                return false;
            }
            HandleCommand(client, command);
            return true;
        }

        private void AcceptUnixSockets()
        {
            while (!_shutdown.IsCancellationRequested)
            {
                try
                {
                    Socket socket = _unixListener.Accept();
                    AddNetworkClient(new NetworkStream(socket, true));
                }
                catch (ObjectDisposedException) { break; }
                catch (SocketException) { if (_shutdown.IsCancellationRequested) break; }
            }
        }

        private void AcceptNamedPipes(string pipeName)
        {
            while (!_shutdown.IsCancellationRequested)
            {
                NamedPipeServerStream pipe = null;
                try
                {
                    pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    lock (_gate) _pendingPipe = pipe;
                    pipe.WaitForConnection();
                    lock (_gate) if (ReferenceEquals(_pendingPipe, pipe)) _pendingPipe = null;
                    if (_shutdown.IsCancellationRequested) { pipe.Dispose(); break; }
                    AddNetworkClient(pipe);
                }
                catch (ObjectDisposedException) { pipe?.Dispose(); break; }
                catch (IOException) { pipe?.Dispose(); if (_shutdown.IsCancellationRequested) break; }
            }
        }

        private void AddNetworkClient(Stream stream)
        {
            var client = new ClientState(_queueCapacity, stream);
            lock (_gate) _clients.Add(client);
            client.Reader = new Thread(() => ReaderLoop(client)) { IsBackground = true, Name = "MoyvaObserverReader" };
            client.Writer = new Thread(() => WriterLoop(client)) { IsBackground = true, Name = "MoyvaObserverWriter" };
            client.Writer.Start(); client.Reader.Start();
        }

        private void ReaderLoop(ClientState client)
        {
            try
            {
                while (!_shutdown.IsCancellationRequested && !client.Closed)
                {
                    if (!TryReadBody(client.Stream, out byte[] body, out string readReason, out bool oversized)) break;
                    if (oversized)
                    {
                        WriteDirect(client, Status(TrainingObserverProtocol.Rejected, -1, 0, 0, readReason));
                        break;
                    }
                    bool terminal;
                    ProcessInbound(client, body, out terminal);
                    if (terminal)
                    {
                        if (client.Outgoing.TryTake(out var rejection)) WriteDirect(client, rejection);
                        break;
                    }
                }
            }
            catch { }
            finally { Disconnect(client); }
        }

        private void WriterLoop(ClientState client)
        {
            try
            {
                while (!_shutdown.IsCancellationRequested && !client.Closed)
                {
                    if (!client.Outgoing.WaitTake(_shutdown.Token, out var envelope)) continue;
                    WriteDirect(client, envelope);
                }
            }
            catch { }
            finally { Disconnect(client); }
        }

        private bool TryReadBody(Stream stream, out byte[] body, out string reason, out bool oversized)
        {
            body = null; reason = null; oversized = false;
            var header = new byte[4]; if (!ReadExact(stream, header, 4)) return false;
            int length = header[0] | (header[1] << 8) | (header[2] << 16) | (header[3] << 24);
            if (length < 1) { reason = "Empty observer message."; return false; }
            if (length > _maxMessageBytes) { reason = "Observer message exceeds configured maximum."; oversized = true; return true; }
            body = new byte[length]; return ReadExact(stream, body, length);
        }

        private static bool ReadExact(Stream stream, byte[] buffer, int count)
        {
            int offset = 0;
            while (offset < count)
            {
                int read = stream.Read(buffer, offset, count - offset);
                if (read <= 0) return false; offset += read;
            }
            return true;
        }

        private void WriteDirect(ClientState client, TrainingObserverEnvelope envelope)
        {
            if (client.Stream == null || client.Closed) return;
            byte[] frame = TrainingObserverProtocol.EncodeFrame(envelope, _maxMessageBytes);
            lock (client.StreamWriteGate)
            {
                client.Stream.Write(frame, 0, frame.Length);
                client.Stream.Flush();
            }
        }

        private void Disconnect(ClientState client)
        {
            if (client == null) return;
            lock (_gate)
            {
                if (client.Closed) return;
                client.Closed = true;
                _clients.Remove(client);
            }
            client.Outgoing.Dispose();
            try { client.Stream?.Dispose(); } catch { }
        }

        public void Dispose()
        {
            if (_shutdown.IsCancellationRequested) return;
            _shutdown.Cancel();
            try { _unixListener?.Dispose(); } catch { }
            ClientState[] clients;
            lock (_gate)
            {
                try { _pendingPipe?.Dispose(); } catch { }
                _pendingPipe = null;
                clients = _clients.ToArray();
                foreach (var client in clients)
                {
                    client.Closed = true;
                    client.Outgoing.Dispose();
                    try { client.Stream?.Dispose(); } catch { }
                }
                _clients.Clear();
            }
            if (_acceptThread != null && _acceptThread.IsAlive) _acceptThread.Join(1000);
            foreach (var client in clients)
            {
                if (client.Reader != null && client.Reader.IsAlive) client.Reader.Join(500);
                if (client.Writer != null && client.Writer.IsAlive) client.Writer.Join(500);
            }
            TryDeleteUnixSocket(_unixPath);
            _shutdown.Dispose();
        }

        private static AgentDecisionEvent CloneDecision(AgentDecisionEvent e) => new AgentDecisionEvent
        {
            version = e.version, sessionId = e.sessionId, arenaId = e.arenaId, episodeId = e.episodeId, sequence = e.sequence,
            agentId = e.agentId, scenarioId = e.scenarioId, scenarioStep = e.scenarioStep,
            scenarioProgress = e.scenarioProgress, candidateCount = e.candidateCount,
            availableIntents = e.availableIntents == null ? Array.Empty<string>() : (string[])e.availableIntents.Clone(),
            availableActions = e.availableActions == null ? Array.Empty<string>() : (string[])e.availableActions.Clone(),
            chosenSlot = e.chosenSlot, chosenIntent = e.chosenIntent, actorId = e.actorId,
            actionId = e.actionId, targetId = e.targetId, targetX = e.targetX, targetY = e.targetY,
            result = e.result, rejectionReason = e.rejectionReason, rewardDelta = e.rewardDelta
        };

        private static ArenaSnapshot CloneSnapshot(ArenaSnapshot s, bool visuals) => new ArenaSnapshot
        {
            version = s.version, sessionId = s.sessionId, arenaId = s.arenaId, episodeId = s.episodeId, sequence = s.sequence,
            width = s.width, height = s.height, scenarioId = s.scenarioId, scenarioStep = s.scenarioStep,
            isComplete = visuals && s.isComplete, status = visuals ? s.status : "metadata-only subscription; visuals=false",
            cellLayers = visuals && s.cellLayers != null ? (string[])s.cellLayers.Clone() : Array.Empty<string>(),
            heights = visuals && s.heights != null ? (float[])s.heights.Clone() : Array.Empty<float>(),
            surfaces = visuals && s.surfaces != null ? (string[])s.surfaces.Clone() : Array.Empty<string>(),
            staticObjects = visuals && s.staticObjects != null ? (string[])s.staticObjects.Clone() : Array.Empty<string>(),
            projection = visuals && s.projection != null ? (float[])s.projection.Clone() : Array.Empty<float>()
        };

        private static string SanitizeName(string raw, string fallback)
        {
            string source = string.IsNullOrWhiteSpace(raw) ? fallback : Path.GetFileNameWithoutExtension(raw.Trim());
            var chars = source.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.').Take(48).ToArray();
            return chars.Length == 0 ? fallback : new string(chars);
        }

        private static void TryDeleteUnixSocket(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        public sealed class InMemoryClient : IDisposable
        {
            private readonly TrainingObserverServer _server;
            private ClientState _state;
            private InMemoryClient(TrainingObserverServer server, ClientState state)
            {
                _server = server;
                _state = state;
            }

            internal static InMemoryClient Create(TrainingObserverServer server)
            {
                var state = new ClientState(server._queueCapacity, null);
                lock (server._gate) server._clients.Add(state);
                return new InMemoryClient(server, state);
            }
            public int QueuedCount => _state == null ? 0 : _state.Outgoing.Count;
            public void Send(ObserverCommand command) { if (_state != null) _server.HandleCommand(_state, command); }
            public bool ProcessRaw(byte[] body) { if (_state == null) return false; return _server.ProcessInbound(_state, body, out _); }
            public TrainingObserverEnvelope[] Drain()
            {
                if (_state == null) return Array.Empty<TrainingObserverEnvelope>();
                return _state.Outgoing.Drain();
            }
            public void Dispose() { if (_state != null) { _server.Disconnect(_state); _state = null; } }
        }

        private sealed class ClientState
        {
            public readonly Dictionary<int, bool> Subscriptions = new Dictionary<int, bool>();
            public readonly HashSet<int> PendingSnapshots = new HashSet<int>();
            public readonly Dictionary<int, long> LastEpisodeByArena = new Dictionary<int, long>();
            public readonly OutgoingQueue Outgoing;
            public readonly Stream Stream;
            public readonly object StreamWriteGate = new object();
            public Thread Reader, Writer;
            public string SelectedAgent;
            public long LastSequence;
            public bool Closed;
            public ClientState(int capacity, Stream stream) { Outgoing = new OutgoingQueue(capacity); Stream = stream; }
            public bool IsSubscribed(int arenaId) => Subscriptions.ContainsKey(arenaId) || Subscriptions.ContainsKey(-1);
            public bool WantsVisuals(int arenaId)
            {
                bool exact = Subscriptions.TryGetValue(arenaId, out bool a) && a;
                bool all = Subscriptions.TryGetValue(-1, out bool b) && b;
                return exact || all;
            }
            public bool NeedsSnapshot(int arenaId) => PendingSnapshots.Contains(arenaId) || PendingSnapshots.Contains(-1);
        }

        private sealed class OutgoingQueue : IDisposable
        {
            private readonly object _gate = new object();
            private readonly Queue<TrainingObserverEnvelope> _queue = new Queue<TrainingObserverEnvelope>();
            private readonly int _capacity;
            private readonly AutoResetEvent _available = new AutoResetEvent(false);
            private bool _disposed;
            public OutgoingQueue(int capacity) { _capacity = Math.Max(1, capacity); }
            public int Count { get { lock (_gate) return _queue.Count; } }
            public bool TryEnqueue(TrainingObserverEnvelope item)
            {
                lock (_gate)
                {
                    if (_disposed || _queue.Count >= _capacity) return false;
                    _queue.Enqueue(item); _available.Set(); return true;
                }
            }
            public bool TryTake(out TrainingObserverEnvelope item)
            {
                lock (_gate)
                {
                    if (_queue.Count == 0) { item = null; return false; }
                    item = _queue.Dequeue(); return true;
                }
            }
            public bool WaitTake(CancellationToken token, out TrainingObserverEnvelope item)
            {
                if (TryTake(out item)) return true;
                WaitHandle.WaitAny(new[] { _available, token.WaitHandle }, 250);
                return TryTake(out item);
            }
            public void Clear() { lock (_gate) _queue.Clear(); }
            public TrainingObserverEnvelope[] Drain()
            {
                lock (_gate) { var result = _queue.ToArray(); _queue.Clear(); return result; }
            }
            public void Dispose()
            {
                lock (_gate) { if (_disposed) return; _disposed = true; _queue.Clear(); }
                _available.Set();
            }
        }
    }
}
