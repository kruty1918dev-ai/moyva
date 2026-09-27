using Kruty1918.Moyva.Construction.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Tests.Editor
{
    /// <summary>
    /// C23: the paid placement becomes operational only after real work — no
    /// production/recruitment before that, no skipped delivery, no double
    /// progress. The state machine is the authority: registration is
    /// idempotent, progress is owner-scoped and capped, and the operational
    /// transition publishes exactly once.
    /// </summary>
    public sealed class C23AcceptanceTests
    {
        private static readonly Vector2Int Pos = new Vector2Int(4, 4);
        private const string Building = "lumber-camp";
        private const string Owner = "p1";

        private ConstructionLifecycleStateMachine _machine;

        [SetUp]
        public void SetUp() => _machine = new ConstructionLifecycleStateMachine();

        private bool Register(int buildTurns = 2, long placedTurn = 1,
            Vector2Int? relocation = null, string buildingId = Building,
            string ownerId = Owner, Vector2Int? pos = null)
            => _machine.RegisterPlacement(
                pos ?? Pos, buildingId, ownerId, buildTurns, placedTurn,
                relocation, out _);

        [Test]
        public void ZeroBuildTurns_OperationalImmediately()
        {
            _machine.RegisterPlacement(Pos, Building, Owner, 0, 1, null,
                out var transition);
            Assert.IsTrue(transition.IsValid);
            Assert.IsTrue(_machine.IsOperational(Pos));
        }

        [Test]
        public void BuildTurns1_OperationalOnFirstOwnerTurn()
        {
            Register(buildTurns: 1, placedTurn: 1);
            Assert.IsFalse(_machine.IsOperational(Pos),
                "Placed is not operational — work must land first.");

            var transitions = _machine.AdvanceOwnerTurn(Owner, 2);
            Assert.AreEqual(1, transitions.Count);
            Assert.IsTrue(_machine.IsOperational(Pos));

            Assert.AreEqual(0, _machine.AdvanceOwnerTurn(Owner, 3).Count,
                "Operational publishes exactly once.");
        }

        [Test]
        public void BuildTurns2_NeedsTwoOwnerTurns()
        {
            Register(buildTurns: 2, placedTurn: 1);
            Assert.AreEqual(0, _machine.AdvanceOwnerTurn(Owner, 2).Count);
            Assert.IsFalse(_machine.IsOperational(Pos));
            Assert.AreEqual(1, _machine.AdvanceOwnerTurn(Owner, 3).Count);
            Assert.IsTrue(_machine.IsOperational(Pos));
        }

        [Test]
        public void PlacedTurn_CannotCompleteSameTurn()
        {
            Register(buildTurns: 1, placedTurn: 5);
            Assert.AreEqual(0, _machine.AdvanceOwnerTurn(Owner, 5).Count,
                "PlacedTurn >= globalTurn blocks instant completion.");
            Assert.AreEqual(1, _machine.AdvanceOwnerTurn(Owner, 6).Count);
        }

        [Test]
        public void OtherOwnersTurn_DoesNotProgress()
        {
            Register(buildTurns: 1, placedTurn: 1);
            Assert.AreEqual(0, _machine.AdvanceOwnerTurn("p2", 2).Count);
            Assert.IsFalse(_machine.IsOperational(Pos));
            Assert.AreEqual(1, _machine.AdvanceOwnerTurn(Owner, 3).Count);
        }

        [Test]
        public void DuplicatePlacement_DoesNotResetProgress()
        {
            Register(buildTurns: 2, placedTurn: 1);
            _machine.AdvanceOwnerTurn(Owner, 2); // completed = 1
            Assert.IsFalse(Register(buildTurns: 2, placedTurn: 99),
                "Duplicate notification is a no-op, not a new registration.");
            Assert.IsTrue(_machine.TryGetProgress(Pos, out int done, out int req));
            Assert.AreEqual(1, done, "Re-registering must not wipe progress.");
            Assert.AreEqual(2, req);
            Assert.AreEqual(1, _machine.AdvanceOwnerTurn(Owner, 3).Count,
                "One more turn completes — progress was kept.");
        }

        [Test]
        public void Relocation_PreservesProgress()
        {
            Register(buildTurns: 2, placedTurn: 1);
            _machine.AdvanceOwnerTurn(Owner, 2);
            var dst = new Vector2Int(9, 9);
            _machine.RegisterPlacement(dst, Building, Owner, 2, 1, Pos, out _);
            Assert.IsFalse(_machine.TryGetProgress(Pos, out _, out _),
                "Relocation moves the tracked state off the source cell.");
            Assert.IsTrue(_machine.TryGetProgress(dst, out int done, out _));
            Assert.AreEqual(1, done, "Moved structure keeps its completed work.");
            Assert.AreEqual(1, _machine.AdvanceOwnerTurn(Owner, 3).Count);
            Assert.IsTrue(_machine.IsOperational(dst));
        }

        [Test]
        public void CompletedRelocation_ReannouncesOperational()
        {
            Register(buildTurns: 1, placedTurn: 1);
            _machine.AdvanceOwnerTurn(Owner, 2);
            var dst = new Vector2Int(9, 9);
            _machine.RegisterPlacement(dst, Building, Owner, 1, 1, Pos,
                out var transition);
            Assert.IsTrue(transition.IsValid,
                "A completed building moved announces operational at the new origin.");
            Assert.IsTrue(_machine.IsOperational(dst));
        }

        [Test]
        public void Remove_ClearsAllState()
        {
            Register(buildTurns: 1, placedTurn: 1);
            Assert.IsTrue(_machine.Remove(Pos));
            Assert.AreEqual(0, _machine.Count);
            Assert.IsFalse(_machine.TryGetProgress(Pos, out _, out _));
            Assert.IsFalse(_machine.TryGetSavedState(Pos, out _));
            Assert.IsFalse(_machine.Remove(Pos));
        }

        [Test]
        public void RealtimeWork_RequiresFullRequired()
        {
            Register(buildTurns: 2, placedTurn: 1);
            var first = _machine.AdvanceRealtime(0.9f, (o, p) => 1f);
            Assert.AreEqual(0, first.Count, "0.9/2 work completes nothing.");
            Assert.IsFalse(_machine.IsOperational(Pos));

            var second = _machine.AdvanceRealtime(0.9f, (o, p) => 1f);
            Assert.AreEqual(0, second.Count, "1.8/2 still not operational.");
            var third = _machine.AdvanceRealtime(0.9f, (o, p) => 1f);
            Assert.AreEqual(1, third.Count, "Partial work accumulates across ticks.");
        }

        [Test]
        public void RealtimeWork_ZeroWorkerSpeed_StillProgressesAtFloor()
        {
            // Service clamps speed to >= 0.05 and gives empty settlements 0.25 —
            // the machine itself honors whatever speed the resolver returns.
            Register(buildTurns: 1, placedTurn: 1);
            Assert.AreEqual(0, _machine.AdvanceRealtime(1f, (o, p) => 0.1f).Count);
            Assert.AreEqual(1, _machine.AdvanceRealtime(9f, (o, p) => 0.1f).Count,
                "work*speed = 0.9 crosses the required 1.0.");
        }

        [Test]
        public void CaptureRestore_RoundTrips_WithoutDoublePublish()
        {
            Register(buildTurns: 3, placedTurn: 1);
            _machine.AdvanceOwnerTurn(Owner, 2); // completed = 1
            var saved = _machine.CaptureSorted();

            var restored = new ConstructionLifecycleStateMachine();
            restored.Restore(saved, _ => Building);
            Assert.IsTrue(restored.TryGetProgress(Pos, out int done, out int req));
            Assert.AreEqual(1, done);
            Assert.AreEqual(3, req);

            // A completed entry restored mid-build must publish operational
            // once when the restore contains a finished state.
            var alreadyDone = new ConstructionLifecycleStateMachine();
            var finished = new ConstructionLifecycleStateMachine();
            finished.RegisterPlacement(Pos, Building, Owner, 1, 1, null, out _);
            finished.AdvanceOwnerTurn(Owner, 2);
            var finishSave = finished.CaptureSorted();
            var transitions = alreadyDone.Restore(finishSave, _ => Building);
            Assert.AreEqual(1, transitions.Count);
            Assert.IsTrue(alreadyDone.IsOperational(Pos));
        }

        [Test]
        public void TryRestoreOperational_ReportsOnce()
        {
            Register(buildTurns: 5, placedTurn: 1);
            Assert.IsTrue(_machine.TryRestoreOperational(Pos, out var transition));
            Assert.IsTrue(transition.IsValid);
            Assert.IsTrue(_machine.IsOperational(Pos));
            Assert.IsFalse(_machine.TryRestoreOperational(Pos, out _),
                "Trusted restore is not re-announced.");
            Assert.IsFalse(_machine.TryRestoreOperational(new Vector2Int(9, 9), out _),
                "Unknown position is a no-op.");
        }

        [Test]
        public void AdvanceOwnerTurn_IsDeterministicAcrossPositions()
        {
            var p2 = new Vector2Int(2, 2);
            var p1 = new Vector2Int(8, 8);
            Register(1, 1, pos: p1);
            Register(1, 1, pos: p2);
            var transitions = _machine.AdvanceOwnerTurn(Owner, 2);
            Assert.AreEqual(2, transitions.Count);
            Assert.AreEqual(p2, transitions[0].Position,
                "Sorted positions produce a stable processing order.");
            Assert.AreEqual(p1, transitions[1].Position);
        }
    }
}
