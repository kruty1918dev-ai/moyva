using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// C25: settlement capture must reject invalid requests without any
    /// mutation and, on success, flip ownership exactly once across the
    /// position/building/owner registries with a single capture signal.
    /// </summary>
    [TestFixture]
    public class C25AcceptanceTests
    {
        private const string OldOwner = "p1";
        private const string NewOwner = "p2";
        private const string SettlementId = "s1";
        private static readonly Vector2Int TownHall = new(10, 10);
        private static readonly Vector2Int Barracks = new(11, 10);
        private static readonly Vector2Int Farm = new(10, 11);

        private EconomySettlementRegistryService _registry;
        private FakeConstructionTransfer _construction;
        private SettlementCaptureService _service;
        private SignalBus _signals;
        private List<SettlementCapturedSignal> _captured;
        private List<FactionEliminatedSignal> _eliminated;

        [SetUp]
        public void SetUp()
        {
            _registry = new EconomySettlementRegistryService();
            _construction = new FakeConstructionTransfer();
            _captured = new List<SettlementCapturedSignal>();
            _eliminated = new List<FactionEliminatedSignal>();

            var container = new DiContainer();
            SignalBusInstaller.Install(container);
            container.DeclareSignal<SettlementCapturedSignal>().OptionalSubscriber();
            container.DeclareSignal<FactionEliminatedSignal>().OptionalSubscriber();
            _signals = container.Resolve<SignalBus>();
            _signals.Subscribe<SettlementCapturedSignal>(s => _captured.Add(s));
            _signals.Subscribe<FactionEliminatedSignal>(s => _eliminated.Add(s));

            _service = new SettlementCaptureService(_registry, _construction, _signals);
        }

        [Test]
        public void Capture_SelfTransfer_RejectedWithoutMutation()
        {
            RegisterSettlement();

            var result = _service.CaptureSettlement(SettlementId, OldOwner, OldOwner);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(0, _construction.Calls.Count, "Self-capture must not touch buildings.");
            Assert.AreEqual(OldOwner, _registry.GetSettlement(SettlementId).OwnerId);
            Assert.AreEqual(0, _captured.Count);
        }

        [Test]
        public void Capture_InactiveSettlement_RejectedWithoutMutation()
        {
            var state = RegisterSettlement();
            state.IsActive = false;

            var result = _service.CaptureSettlement(SettlementId, OldOwner, NewOwner);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(0, _construction.Calls.Count, "Inactive settlement must not churn buildings.");
            Assert.AreEqual(OldOwner, state.OwnerId);
        }

        [Test]
        public void Capture_WrongPreviousOwner_RejectedWithoutMutation()
        {
            RegisterSettlement();

            var result = _service.CaptureSettlement(SettlementId, "someone-else", NewOwner);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(0, _construction.Calls.Count);
            Assert.AreEqual(OldOwner, _registry.GetSettlement(SettlementId).OwnerId);
        }

        [Test]
        public void Capture_MissingSettlement_Rejected()
        {
            var result = _service.CaptureSettlement("ghost", OldOwner, NewOwner);

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains("not found", result.Reason);
        }

        [Test]
        public void Capture_AtPosition_UnmappedPosition_Rejected()
        {
            var result = _service.CaptureSettlementAtPosition(new Vector2Int(99, 99), OldOwner, NewOwner);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(0, _construction.Calls.Count);
        }

        [Test]
        public void Capture_BuildingTransferFails_RollsBackTransferred()
        {
            RegisterSettlement();
            _construction.FailOn.Add(Barracks);

            var result = _service.CaptureSettlement(SettlementId, OldOwner, NewOwner);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(OldOwner, _registry.GetSettlement(SettlementId).OwnerId,
                "Settlement owner must survive a failed building transfer.");
            // TownHall was transferred to the new owner first, then rolled back
            // when Barracks failed; Farm was never attempted.
            Assert.AreEqual(3, _construction.Calls.Count);
            Assert.AreEqual((TownHall, OldOwner, NewOwner), _construction.Calls[0]);
            Assert.AreEqual((Barracks, OldOwner, NewOwner), _construction.Calls[1]);
            Assert.AreEqual((TownHall, NewOwner, OldOwner), _construction.Calls[2],
                "Rolled-back building must be transferred back to the previous owner.");
            Assert.AreEqual(OldOwner, _construction.Owners[TownHall]);
            Assert.AreEqual(OldOwner, _construction.Owners[Farm]);
            Assert.AreEqual(OldOwner, _construction.Owners[Barracks]);
            Assert.AreEqual(0, _captured.Count, "Rejected capture must not signal.");
        }

        [Test]
        public void Capture_Success_TransfersOnce_SignalsOnce()
        {
            RegisterSettlement();

            var result = _service.CaptureSettlement(SettlementId, OldOwner, NewOwner, "test");

            Assert.IsTrue(result.Succeeded, result.Reason);
            Assert.AreEqual(NewOwner, _registry.GetSettlement(SettlementId).OwnerId);
            foreach (var pos in new[] { TownHall, Barracks, Farm })
            {
                Assert.AreEqual(NewOwner, _construction.Owners[pos], $"Building at {pos} transferred once.");
                Assert.IsTrue(_registry.TryGetBuildingAtPosition(pos, out _, out string ownerAt));
                Assert.AreEqual(NewOwner, ownerAt, $"Position registry must mirror new owner at {pos}.");
            }
            Assert.AreEqual(1, _captured.Count, "Exactly one capture signal.");
            Assert.AreEqual(TownHall, _captured[0].CenterPosition,
                "Center must be the registered town hall, not an arbitrary mapped position.");
            Assert.AreEqual(OldOwner, _captured[0].PreviousOwnerId);
            Assert.AreEqual(NewOwner, _captured[0].NewOwnerId);
        }

        [Test]
        public void Capture_LastSettlement_FiresEliminated()
        {
            RegisterSettlement();

            _service.CaptureSettlement(SettlementId, OldOwner, NewOwner);

            Assert.AreEqual(1, _eliminated.Count);
            Assert.AreEqual(OldOwner, _eliminated[0].FactionId);
        }

        [Test]
        public void Capture_OtherSettlementRemains_NoElimination()
        {
            RegisterSettlement();
            var second = new EconomySettlementState
            {
                SettlementId = "s2",
                OwnerId = OldOwner,
                IsActive = true,
            };
            _registry.RegisterSettlement(second, new Vector2Int(50, 50));

            _service.CaptureSettlement(SettlementId, OldOwner, NewOwner);

            Assert.AreEqual(0, _eliminated.Count, "Owner still holding a settlement is not eliminated.");
        }

        [Test]
        public void Capture_OldOwnerLosesPositionQueries()
        {
            RegisterSettlement();

            _service.CaptureSettlement(SettlementId, OldOwner, NewOwner);

            Assert.IsFalse(_registry.TryFindNearestSettlement(TownHall, OldOwner, out _),
                "Old owner must not resolve the captured settlement as theirs.");
            Assert.IsTrue(_registry.TryFindNearestSettlement(TownHall, NewOwner, out var nearest));
            Assert.AreEqual(SettlementId, nearest.SettlementId);
        }

        private EconomySettlementState RegisterSettlement()
        {
            var state = new EconomySettlementState
            {
                SettlementId = SettlementId,
                OwnerId = OldOwner,
                IsActive = true,
            };
            state.Buildings.Add(new EconomyBuildingState { GridPosition = TownHall, BuildingId = "townhall" });
            state.Buildings.Add(new EconomyBuildingState { GridPosition = Barracks, BuildingId = "barracks" });
            state.Buildings.Add(new EconomyBuildingState { GridPosition = Farm, BuildingId = "farm" });
            _registry.RegisterSettlement(state, TownHall);
            _registry.RegisterBuildingPosition(TownHall, SettlementId, "townhall", OldOwner);
            _registry.RegisterBuildingPosition(Barracks, SettlementId, "barracks", OldOwner);
            _registry.RegisterBuildingPosition(Farm, SettlementId, "farm", OldOwner);
            _construction.Owners[TownHall] = OldOwner;
            _construction.Owners[Barracks] = OldOwner;
            _construction.Owners[Farm] = OldOwner;
            return state;
        }

        private sealed class FakeConstructionTransfer : IConstructionOwnershipTransfer
        {
            public readonly Dictionary<Vector2Int, string> Owners = new Dictionary<Vector2Int, string>();
            public readonly HashSet<Vector2Int> FailOn = new HashSet<Vector2Int>();
            public readonly List<(Vector2Int pos, string prev, string next)> Calls =
                new List<(Vector2Int, string, string)>();

            public bool TryTransferPlacedBuildingOwner(Vector2Int position, string previousOwnerId,
                string nextOwnerId, out string reason)
            {
                Calls.Add((position, previousOwnerId, nextOwnerId));
                if (FailOn.Contains(position))
                {
                    reason = "transfer failed";
                    return false;
                }
                if (!Owners.TryGetValue(position, out string current) || current != previousOwnerId)
                {
                    reason = "owner mismatch";
                    return false;
                }
                Owners[position] = nextOwnerId;
                reason = null;
                return true;
            }
        }
    }
}
