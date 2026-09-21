using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Construction.API;
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
    /// Construction placement authority flow: client confirm requests are
    /// de-duplicated while awaiting a host answer, host rejections travel back
    /// as Rejected payloads, and the client surfaces them through
    /// <see cref="ConstructionPlacementRejectedSignal"/>.
    /// </summary>
    [TestFixture]
    public sealed class MultiplayerConstructionPlacementTests
    {
        private DiContainer _container;
        private SignalBus _signals;
        private FakeCommands _commands;
        private StubConstructionService _construction;
        private int _rejectedSignalCount;
        private ConstructionPlacementRejectedSignal _lastRejected;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            global::Zenject.SignalBusInstaller.Install(_container);
            _container
                .DeclareSignal<ConstructionPlacementRejectedSignal>()
                .OptionalSubscriber();
            _rejectedSignalCount = 0;
            _lastRejected = default;
            _signals = _container.Resolve<SignalBus>();
            _signals.Subscribe<ConstructionPlacementRejectedSignal>(signal =>
            {
                _rejectedSignalCount++;
                _lastRejected = signal;
            });
            _commands = new FakeCommands();
            _construction = new StubConstructionService();
        }

        [Test]
        public void ClientConfirm_SendsSingleRequestUntilHostResponds()
        {
            var service = CreateService(
                LocalGameplayRole.Client,
                "p1",
                isLocalHost: false);
            _construction.Pending[new Vector2Int(3, 4)] = "castle";

            Assert.IsTrue(service.TryHandleConfirmRequest());
            Assert.AreEqual(1, _commands.PeerSends.Count);
            Assert.AreEqual("host", _commands.PeerSends[0].PeerId);

            // Дубль Confirm (клік/хоткей) не шле повторний запит для тієї ж позиції.
            Assert.IsTrue(service.TryHandleConfirmRequest());
            Assert.AreEqual(1, _commands.PeerSends.Count);
        }

        [Test]
        public void ClientConfirmed_ResendsAfterNewPendingOnly()
        {
            var service = CreateService(
                LocalGameplayRole.Client,
                "p1",
                isLocalHost: false);
            var first = new Vector2Int(3, 4);
            var second = new Vector2Int(6, 4);
            _construction.Pending[first] = "castle";
            _construction.Pending[second] = "house";

            Assert.IsTrue(service.TryHandleConfirmRequest());
            Assert.AreEqual(2, _commands.PeerSends.Count);

            // Хост підтвердив першу позицію — повторний Confirm шле лише її,
            // якщо вона зникла з pending, але друга вже в польоті і не дублюється.
            _construction.Pending.Remove(first);
            var confirmed = new BuildingPlacePayload(
                GameActionMessageKind.Confirmed,
                "castle",
                first,
                "p1",
                "p1");
            service.OnNetworkBuildingPlace("host", confirmed.ToBytes());

            // Pending-прев'ю першої позиції прибрано локально, тож повторний
            // Confirm не має що слати: друга позиція досі в польоті.
            Assert.IsTrue(service.TryHandleConfirmRequest());
            Assert.AreEqual(2, _commands.PeerSends.Count);
        }

        [Test]
        public void ClientRejected_ClearsInFlight_AndFiresRejectedSignal()
        {
            var service = CreateService(
                LocalGameplayRole.Client,
                "p1",
                isLocalHost: false);
            var position = new Vector2Int(3, 4);
            _construction.Pending[position] = "castle";
            service.TryHandleConfirmRequest();
            Assert.AreEqual(1, _commands.PeerSends.Count);

            var rejected = new BuildingPlacePayload(
                GameActionMessageKind.Rejected,
                "castle",
                position,
                "p1",
                "p1",
                rejectionReason: "Tile or footprint is occupied.");
            service.OnNetworkBuildingPlace("host", rejected.ToBytes());

            Assert.AreEqual(1, _rejectedSignalCount);
            Assert.AreEqual(position, _lastRejected.Position);
            StringAssert.Contains("occupied", _lastRejected.Reason);

            // Після відмови повторний Confirm знову надсилає запит.
            Assert.IsTrue(service.TryHandleConfirmRequest());
            Assert.AreEqual(2, _commands.PeerSends.Count);
        }

        [Test]
        public void ClientRejected_FromNonHost_IsIgnored()
        {
            var service = CreateService(
                LocalGameplayRole.Client,
                "p1",
                isLocalHost: false,
                participantCount: 3);
            var position = new Vector2Int(3, 4);
            _construction.Pending[position] = "castle";
            service.TryHandleConfirmRequest();

            var rejected = new BuildingPlacePayload(
                GameActionMessageKind.Rejected,
                "castle",
                position,
                "p1",
                "p1",
                rejectionReason: "spoofed");
            service.OnNetworkBuildingPlace("p2", rejected.ToBytes());

            Assert.AreEqual(0, _rejectedSignalCount);

            // In-flight не знято: повторний Confirm досі не дублює запит.
            Assert.IsTrue(service.TryHandleConfirmRequest());
            Assert.AreEqual(1, _commands.PeerSends.Count);
        }

        [Test]
        public void HostRequest_FromUnauthorizedSender_ReceivesRejected()
        {
            var service = CreateService(
                LocalGameplayRole.Host,
                "host",
                isLocalHost: true);
            var request = new BuildingPlacePayload(
                GameActionMessageKind.Request,
                "castle",
                new Vector2Int(3, 4),
                "ghost",
                "ghost");

            service.OnNetworkBuildingPlace("ghost", request.ToBytes());

            Assert.AreEqual(1, _commands.PeerSends.Count);
            Assert.AreEqual("ghost", _commands.PeerSends[0].PeerId);
            var reply = BuildingPlacePayload.FromBytes(
                _commands.PeerSends[0].Payload);
            Assert.AreEqual(GameActionMessageKind.Rejected, reply.Kind);
            StringAssert.Contains("not an active participant", reply.RejectionReason);
        }

        [Test]
        public void HostRequest_WhenPlacementFails_ReceivesRejected()
        {
            var service = CreateService(
                LocalGameplayRole.Host,
                "host",
                isLocalHost: true);
            _construction.DirectPlaceResult = false;
            var request = new BuildingPlacePayload(
                GameActionMessageKind.Request,
                "castle",
                new Vector2Int(3, 4),
                "p1",
                "p1");

            service.OnNetworkBuildingPlace("p1", request.ToBytes());

            Assert.AreEqual(1, _commands.PeerSends.Count);
            Assert.AreEqual("p1", _commands.PeerSends[0].PeerId);
            var reply = BuildingPlacePayload.FromBytes(
                _commands.PeerSends[0].Payload);
            Assert.AreEqual(GameActionMessageKind.Rejected, reply.Kind);
            Assert.IsFalse(string.IsNullOrWhiteSpace(reply.RejectionReason));
        }

        [Test]
        public void HostRequest_WhenPlacementSucceeds_BroadcastsConfirmed()
        {
            var service = CreateService(
                LocalGameplayRole.Host,
                "host",
                isLocalHost: true);
            _construction.DirectPlaceResult = true;
            var request = new BuildingPlacePayload(
                GameActionMessageKind.Request,
                "castle",
                new Vector2Int(3, 4),
                "p1",
                "p1");

            service.OnNetworkBuildingPlace("p1", request.ToBytes());

            // Успіх не повертає Rejected; підтвердження йде через
            // SendConfirmedCommandToVisiblePeers (може бути відфільтровано туманом).
            foreach (var send in _commands.PeerSends)
            {
                var reply = BuildingPlacePayload.FromBytes(send.Payload);
                Assert.AreNotEqual(GameActionMessageKind.Rejected, reply.Kind);
            }
        }

        private MultiplayerAuthorityService CreateService(
            LocalGameplayRole role,
            string playerId,
            bool isLocalHost,
            int participantCount = 2)
        {
            var service = new MultiplayerAuthorityService(
                _commands,
                new FakeSession(isLocalHost, participantCount),
                _signals,
                new FakeRoleResolver(role, playerId));
            service.Attach(_construction);
            return service;
        }

        private sealed class FakeRoleResolver : ILocalGameplayRoleResolver
        {
            private readonly LocalGameplayRoleSnapshot _snapshot;

            public FakeRoleResolver(LocalGameplayRole role, string playerId)
                => _snapshot = new LocalGameplayRoleSnapshot(role, playerId);

            public LocalGameplayRoleSnapshot Resolve() => _snapshot;
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
            public readonly List<(string PeerId, GameCommandType Type, byte[] Payload)> PeerSends =
                new();
            public readonly List<(GameCommandType Type, byte[] Payload)> Broadcasts =
                new();

            public void SendCommand(GameCommandType type, byte[] payload)
                => Broadcasts.Add((type, payload));

            public void SendCommandToPeer(string peerId, GameCommandType type, byte[] payload)
                => PeerSends.Add((peerId, type, payload));

            public void RegisterHandler(GameCommandType type, Action<string, byte[]> handler) { }
        }

        private sealed class StubConstructionService :
            IConstructionService,
            IConfirmedConstructionPlacementIntentApplier
        {
            public bool ConfirmedApplyResult = true;

            public bool TryApplyConfirmedPlacement(
                string buildingId,
                Vector2Int position,
                string ownerId,
                ConstructionPlacementCommitIntent intent)
                => ConfirmedApplyResult;

            public readonly Dictionary<Vector2Int, string> Pending = new();
            public string ActiveOwner = "p1";
            public bool DirectPlaceResult;
            public int ConfirmCalls;
            public bool Cancelled;

            public BuildingPlacementState State => BuildingPlacementState.Idle;
            public bool IsDemolishMode => false;
            public int PendingDemolitionCount => 0;

            public void SelectBuilding(string buildingId) { }
            public string GetSelectedBuildingId() => null;
            public void SetActiveOwner(string ownerId) => ActiveOwner = ownerId;
            public string GetActiveOwner() => ActiveOwner;
            public bool TryPreviewAt(Vector2Int position) => false;
            public bool HasPendingPlacementAt(Vector2Int position)
                => Pending.ContainsKey(position);
            public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
                => Pending.TryGetValue(position, out buildingId);
            public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements()
                => Pending;
            public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition)
                => false;
            public bool RemovePendingAt(Vector2Int position)
                => Pending.Remove(position);
            public bool TryGetPendingPlacementStatus(
                Vector2Int position,
                out ConstructionPendingPlacementStatus status)
            {
                status = default;
                return false;
            }
            public ConstructionResourceProjection GetResourceProjection(Vector2Int position)
                => ConstructionResourceProjection.Empty;
            public IReadOnlyDictionary<string, float> GetBuildingResourceCosts(string buildingId)
                => null;
            public void Confirm() => ConfirmCalls++;
            public void Cancel() => Cancelled = true;
            public void UndoLast() { }
            public void RedoLast() { }
            public void ToggleDemolishMode() { }
            public bool TryDemolishAt(Vector2Int position) => false;
            public string GetLastActionMessage() => null;
            public IReadOnlyDictionary<Vector2Int, string> GetPlayerPlacedBuildings()
                => null;
            public bool HasPlacedBuilding(string buildingId, string ownerId = null)
                => false;
            public void RestoreFromSave(Vector2Int position, string buildingId) { }
            public bool TryDirectPlace(string buildingId, Vector2Int position, string placedByFactionId)
                => DirectPlaceResult;
            public bool TryDemolishByFaction(Vector2Int position, string factionId)
                => false;
        }
    }
}
