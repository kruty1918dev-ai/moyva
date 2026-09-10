using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Startup
{
    public sealed class MultiplayerStartupBarrierTests
    {
        [Test]
        public async Task FasterClientWaitsUntilHostFinishesAndRetriesLostReply()
        {
            using var host = new Peer(true);
            using var client = new Peer(false);
            int replies = 0;
            client.Commands.Send = (_, type, bytes) => host.Commands.Receive("client", type, bytes);
            host.Commands.Send = (_, type, bytes) => {
                if (++replies > 1) client.Commands.Receive("host", type, bytes);
            };
            client.BuildWorld();
            await client.Barrier.WaitForLocalWorldAsync(CancellationToken.None);
            using var timeout = new CancellationTokenSource(3000);
            var wait = client.Barrier.WaitForHostAsync(timeout.Token);
            Assert.That(wait.IsCompleted, Is.False);
            Assert.Throws<InvalidOperationException>(() => host.Barrier.MarkHostReady());
            host.BuildWorld();
            await host.Barrier.WaitForLocalWorldAsync(timeout.Token);
            Assert.That(wait.IsCompleted, Is.False, "World data alone must not release the client before the host transition finishes.");
            host.Barrier.MarkHostReady();
            await wait;
            Assert.That(replies, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public async Task OldReplyAndOtherPeerCannotReleaseNewStartup()
        {
            using var client = new Peer(false);
            using var cancelOld = new CancellationTokenSource();
            var oldWait = client.Barrier.WaitForHostAsync(cancelOld.Token);
            var oldReply = (byte[])client.Commands.Last.Clone(); oldReply[0] = 2;
            cancelOld.Cancel();
            await ExpectCancelled(oldWait);
            client.Barrier.BeginStartup();
            using var timeout = new CancellationTokenSource(3000);
            var wait = client.Barrier.WaitForHostAsync(timeout.Token);
            client.Commands.Receive("host", GameCommandType.MatchStartSync, oldReply);
            Assert.That(wait.IsCompleted, Is.False);
            var reply = (byte[])client.Commands.Last.Clone(); reply[0] = 2;
            client.Commands.Receive("other-peer", GameCommandType.MatchStartSync, reply);
            Assert.That(wait.IsCompleted, Is.False);
            client.Commands.Receive("host", GameCommandType.MatchStartSync, reply);
            await wait;
        }

        [Test]
        public async Task PreviousWorldDoesNotCountAsReadyForNewStartup()
        {
            using var host = new Peer(true);
            host.BuildWorld();
            host.Barrier.BeginStartup();
            using var cancellation = new CancellationTokenSource();
            var wait = host.Barrier.WaitForLocalWorldAsync(cancellation.Token);
            Assert.That(wait.IsCompleted, Is.False);
            cancellation.Cancel();
            await ExpectCancelled(wait);
        }

        [Test]
        public async Task SlowClientRequestsPositionsLostBeforeItsWorldWasGenerated()
        {
            using var client = new Peer(false);
            client.BuildWorld(spawns: false);
            int requests = 0;
            client.Commands.Send = (peerId, type, payload) => {
                Assert.That(peerId, Is.EqualTo("host"));
                Assert.That(type, Is.EqualTo(GameCommandType.StartingPositions));
                Assert.That(payload, Is.Empty);
                requests++;
                client.StoreSpawns();
            };
            using var timeout = new CancellationTokenSource(3000);
            await client.Barrier.WaitForLocalWorldAsync(timeout.Token);
            Assert.That(requests, Is.EqualTo(1));
        }

        [Test]
        public async Task HostDisconnectFailsWaitingWithoutReleasingClient()
        {
            using var client = new Peer(false);
            using var timeout = new CancellationTokenSource(3000);
            var wait = client.Barrier.WaitForHostAsync(timeout.Token);
            client.Network.Disconnect("host");
            try { await wait; Assert.Fail("Host disconnect must fail the barrier."); }
            catch (InvalidOperationException e) { Assert.That(e.Message, Does.Contain("disconnected")); }
        }

        [Test]
        public async Task DisposingBarrierCancelsPendingWorldWait()
        {
            using var client = new Peer(false);
            using var timeout = new CancellationTokenSource(3000);
            var wait = client.Barrier.WaitForLocalWorldAsync(timeout.Token);
            client.Barrier.Dispose();
            await ExpectCancelled(wait);
        }

        private static async Task ExpectCancelled(Task task)
        {
            try { await task; Assert.Fail("Expected cancellation."); }
            catch (OperationCanceledException) { }
        }

        private sealed class Peer : IDisposable
        {
            public readonly Commands Commands = new Commands();
            public readonly Network Network = new Network();
            public readonly WorldGenerationSignalState World = new WorldGenerationSignalState();
            public readonly MultiplayerStartupBarrier Barrier;
            readonly Session session;
            public Peer(bool host)
            {
                session = new Session(host);
                Barrier = new MultiplayerStartupBarrier(Commands, Network, session, World);
                Barrier.Initialize();
                Barrier.BeginStartup();
            }
            public void BuildWorld(bool spawns = true)
            {
                var sequence = World.BeginWorldSnapshotCycle("test-world");
                World.StoreWorldGeneratedData(new WorldGeneratedDataSignal {
                    StartupSequence = sequence, StartupSessionId = "test-world", Width = 8, Height = 8 });
                if (spawns) StoreSpawns();
            }
            public void StoreSpawns()
            {
                World.TryGetCurrentWorldIdentity(out var sequence, out var id);
                Assert.That(World.TryStoreWorldSpawnPositions(new WorldSpawnPositionsSignal {
                    StartupSequence = sequence, StartupSessionId = id,
                    Assignments = new[] { new SpawnPositionAssignment { ParticipantId = session.LocalPlayerId } }
                }, out _), Is.True);
            }
            public void Dispose() => Barrier.Dispose();
        }
        private sealed class Commands : IGameCommandSyncService
        {
            readonly Dictionary<GameCommandType, Action<string, byte[]>> handlers = new Dictionary<GameCommandType, Action<string, byte[]>>();
            public Action<string, GameCommandType, byte[]> Send;
            public byte[] Last;
            public void SendCommand(GameCommandType type, byte[] payload) => SendCommandToPeer("*", type, payload);
            public void SendCommandToPeer(string peer, GameCommandType type, byte[] payload) { Last = payload; Send?.Invoke(peer, type, payload); }
            public void RegisterHandler(GameCommandType type, Action<string, byte[]> handler) => handlers[type] = handler;
            public void Receive(string sender, GameCommandType type, byte[] payload) { if (handlers.TryGetValue(type, out var handler)) handler?.Invoke(sender, payload); }
        }
        private sealed class Session : ISessionManager
        {
            public Session(bool host) { IsLocalPlayerHost = host; }
            public bool IsLocalPlayerHost { get; }
            public string LocalPlayerId => IsLocalPlayerHost ? "host" : "client";
            public IReadOnlyList<Participant> Participants { get; } = new[] {
                new Participant(new ParticipantIdentity("host", "Host"), true),
                new Participant(new ParticipantIdentity("client", "Client"), false) };
            public Task<bool> CreateOrJoinSessionAsync(SessionConnectOptions options, CancellationToken ct = default) => throw new NotSupportedException();
            public Task LeaveSessionAsync(CancellationToken ct = default) => Task.CompletedTask;
        }
        private sealed class Network : INetworkProvider
        {
            public IObservable<NetworkMessage> Messages => null;
            public event Action<string> PeerConnected { add { } remove { } }
            public event Action<string> PeerDisconnected;
            public void Disconnect(string id) => PeerDisconnected?.Invoke(id);
            public Task<SessionResult> HostSessionAsync(string id, CancellationToken ct = default) => throw new NotSupportedException();
            public Task<SessionResult> JoinSessionAsync(string id, CancellationToken ct = default) => throw new NotSupportedException();
            public Task LeaveSessionAsync(CancellationToken ct = default) => Task.CompletedTask;
            public Task SendMessageAsync(string id, byte[] payload, CancellationToken ct = default) => Task.CompletedTask;
        }
    }
}