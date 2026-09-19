using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    /// <summary>
    /// Solo sessions register a single local host participant, so the starting
    /// position broadcast must stay silent — there is no remote peer and the
    /// relay transport is offline. Regression: solo launch used to log
    /// "Command StartingPositions to '*' failed: Relay session is not connected."
    /// </summary>
    [TestFixture]
    public class StartingPositionSyncServiceTests
    {
        private DiContainer _container;
        private SignalBus _signals;
        private FakeCommands _commands;
        private FakeNetwork _network;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            global::Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<WorldSpawnPositionsSignal>().OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();
            _commands = new FakeCommands();
            _network = new FakeNetwork();
        }

        [Test]
        public void SoloSession_DoesNotBroadcastStartingPositions()
        {
            var session = new FakeSession(isHost: true, participantCount: 1);
            var service = CreateService(session);

            FireSpawnPositions();

            Assert.That(_commands.SendCount, Is.EqualTo(0),
                "Solo session must not push StartingPositions into a disconnected relay.");
            service.Dispose();
        }

        [Test]
        public void MultiplayerHost_BroadcastsStartingPositions()
        {
            var session = new FakeSession(isHost: true, participantCount: 2);
            var service = CreateService(session);

            FireSpawnPositions();

            Assert.That(_commands.SendCount, Is.EqualTo(1));
            Assert.That(_commands.LastType, Is.EqualTo(GameCommandType.StartingPositions));
            service.Dispose();
        }

        [Test]
        public void MultiplayerClient_DoesNotBroadcastStartingPositions()
        {
            var session = new FakeSession(isHost: false, participantCount: 2);
            var service = CreateService(session);

            FireSpawnPositions();

            Assert.That(_commands.SendCount, Is.EqualTo(0));
            service.Dispose();
        }

        private StartingPositionSyncService CreateService(FakeSession session)
        {
            _container.Bind<ISessionManager>().FromInstance(session);
            var service = new StartingPositionSyncService(_signals, _network, _commands);
            _container.Inject(service);
            service.Initialize();
            return service;
        }

        private void FireSpawnPositions()
        {
            _signals.Fire(new WorldSpawnPositionsSignal
            {
                StartupSequence = 1,
                StartupSessionId = "test",
                Assignments = new[]
                {
                    new SpawnPositionAssignment
                    {
                        SlotIndex = 0,
                        ParticipantId = "host",
                        Position = new Vector2Int(4, 4),
                    },
                },
            });
        }

        private sealed class FakeSession : ISessionManager
        {
            public FakeSession(bool isHost, int participantCount)
            {
                IsLocalPlayerHost = isHost;
                var participants = new List<Participant>();
                for (int i = 0; i < participantCount; i++)
                    participants.Add(new Participant(
                        new ParticipantIdentity(i == 0 ? "host" : $"p{i}", $"P{i}"),
                        isHost: i == 0));
                Participants = participants;
            }

            public bool IsLocalPlayerHost { get; }
            public string LocalPlayerId => IsLocalPlayerHost ? "host" : "p1";
            public IReadOnlyList<Participant> Participants { get; }

            public Task<bool> CreateOrJoinSessionAsync(SessionConnectOptions options, CancellationToken ct = default)
                => Task.FromResult(true);
            public Task LeaveSessionAsync(CancellationToken ct = default) => Task.CompletedTask;
        }

        private sealed class FakeCommands : IGameCommandSyncService
        {
            public int SendCount;
            public GameCommandType LastType;

            public void SendCommand(GameCommandType type, byte[] payload)
            {
                SendCount++;
                LastType = type;
            }

            public void SendCommandToPeer(string peerId, GameCommandType type, byte[] payload)
            {
                SendCount++;
                LastType = type;
            }

            public void RegisterHandler(GameCommandType type, Action<string, byte[]> handler) { }
        }

        private sealed class FakeNetwork : INetworkProvider
        {
            public IObservable<NetworkMessage> Messages => null;
            public event Action<string> PeerConnected { add { } remove { } }
            public event Action<string> PeerDisconnected { add { } remove { } }

            public void Disconnect(string id) { }
            public Task<SessionResult> HostSessionAsync(string id, CancellationToken ct = default)
                => Task.FromResult(SessionResult.Ok(id));
            public Task<SessionResult> JoinSessionAsync(string id, CancellationToken ct = default)
                => Task.FromResult(SessionResult.Ok(id));
            public Task LeaveSessionAsync(CancellationToken ct = default) => Task.CompletedTask;
            public Task SendMessageAsync(string id, byte[] payload, CancellationToken ct = default)
                => Task.CompletedTask;
        }
    }
}
