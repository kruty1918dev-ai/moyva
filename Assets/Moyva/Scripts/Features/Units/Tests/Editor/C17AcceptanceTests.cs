using System.Collections.Generic;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Tests.Editor
{
    /// <summary>
    /// C17: one queue entry yields exactly one ready unit. Turn and realtime
    /// progression, ready-state handoff, cancel, capacity, source-building loss
    /// and save/restore are exercised against the canonical state machine.
    /// "Ready" is not "deployed" — deployment is C18's responsibility.
    /// </summary>
    public sealed class C17AcceptanceTests
    {
        private const string Owner = "owner-a";
        private static readonly Vector2Int Position = new Vector2Int(3, 7);

        private UnitRecruitmentQueueStateMachine _machine;

        [SetUp]
        public void SetUp() => _machine = new UnitRecruitmentQueueStateMachine();

        private UnitRecruitmentQueueItemSnapshot Enqueue(
            int trainingTurns = 3, long turn = 1, string unitTypeId = "worker")
            => _machine.EnqueueValidated(Owner, Position, "barracks", unitTypeId, trainingTurns, turn);

        [Test]
        public void HeadProgressesOncePerOwnerTurn()
        {
            var item = Enqueue(trainingTurns: 2, turn: 1);

            Assert.IsTrue(_machine.AdvanceOwnerTurn(Owner, 2));
            var queue = _machine.GetQueue(Owner, Position);
            Assert.AreEqual(1, queue[0].CompletedTurns, "First turn completes one training turn.");
            Assert.AreEqual(UnitRecruitmentQueueStatus.Training, queue[0].Status);

            Assert.IsTrue(_machine.AdvanceOwnerTurn(Owner, 3));
            Assert.IsTrue(_machine.TryPeekReady(Owner, Position, out var ready));
            Assert.AreEqual(item.QueueId, ready.QueueId);
            Assert.AreEqual(UnitRecruitmentQueueStatus.Ready, ready.Status);
        }

        [Test]
        public void SameTurnSecondAdvance_DoesNotProgress()
        {
            Enqueue(trainingTurns: 2, turn: 1);
            Assert.IsTrue(_machine.AdvanceOwnerTurn(Owner, 2));
            Assert.IsFalse(_machine.AdvanceOwnerTurn(Owner, 2),
                "LastProgressGlobalTurn guard: one owner turn = one progress step.");
            Assert.AreEqual(1, _machine.GetQueue(Owner, Position)[0].CompletedTurns);
        }

        [Test]
        public void EnqueueTurn_DoesNotProgressSameTurn()
        {
            Enqueue(trainingTurns: 1, turn: 5);
            Assert.IsFalse(_machine.AdvanceOwnerTurn(Owner, 5),
                "EnqueuedGlobalTurn >= globalTurn blocks progress on the enlist turn.");
        }

        [Test]
        public void OnlyHeadTrains_LaterEntriesWait()
        {
            var first = Enqueue(trainingTurns: 2, turn: 1);
            var second = Enqueue(trainingTurns: 2, turn: 1);

            _machine.AdvanceOwnerTurn(Owner, 2);
            var queue = _machine.GetQueue(Owner, Position);
            Assert.AreEqual(1, queue[0].CompletedTurns);
            Assert.AreEqual(0, queue[1].CompletedTurns, "Serial training: the tail waits for the head.");
            Assert.AreEqual(UnitRecruitmentQueueStatus.Waiting, queue[1].Status);

            _machine.AdvanceOwnerTurn(Owner, 3);
            _machine.AdvanceOwnerTurn(Owner, 4);
            _machine.AdvanceOwnerTurn(Owner, 5);
            Assert.IsTrue(_machine.TryGetReady(Owner, Position, first.QueueId, out _));
            Assert.IsTrue(_machine.TryGetReady(Owner, Position, second.QueueId, out _),
                "After the head finishes, the next entry becomes the training head.");
        }

        [Test]
        public void OneEntry_TakesExactlyOnce()
        {
            var item = Enqueue(trainingTurns: 1, turn: 1);
            _machine.AdvanceOwnerTurn(Owner, 2);

            Assert.IsTrue(_machine.TryTakeReady(Owner, Position, item.QueueId, out var taken));
            Assert.AreEqual(item.QueueId, taken.QueueId);
            Assert.IsFalse(_machine.TryTakeReady(Owner, Position, item.QueueId, out _),
                "Taken entry is consumed — it cannot produce a second unit.");
            Assert.IsFalse(_machine.TryPeekReady(Owner, Position, out _));
        }

        [Test]
        public void ReadyEntry_CannotBeTaken_WithWrongQueueId()
        {
            var item = Enqueue(trainingTurns: 1, turn: 1);
            _machine.AdvanceOwnerTurn(Owner, 2);
            Assert.IsFalse(_machine.TryTakeReady(Owner, Position, item.QueueId + 99, out _));
            Assert.IsTrue(_machine.TryPeekReady(Owner, Position, out _), "Entry survives a rejected take.");
        }

        [Test]
        public void Cancel_OnlyRemovesUnfinished()
        {
            var unfinished = Enqueue(trainingTurns: 5, turn: 1);
            var doneSoon = Enqueue(trainingTurns: 1, turn: 1);
            _machine.AdvanceOwnerTurn(Owner, 2); // head (unfinished) still training; doneSoon waits

            Assert.IsFalse(_machine.TryCancel(Owner, Position, doneSoon.QueueId + 777, out _),
                "Unknown queue id cannot cancel.");

            // Finish the head, then verify a ready entry cannot be cancelled.
            _machine.AdvanceOwnerTurn(Owner, 3);
            _machine.AdvanceOwnerTurn(Owner, 4);
            _machine.AdvanceOwnerTurn(Owner, 5);
            _machine.AdvanceOwnerTurn(Owner, 6);
            Assert.IsTrue(_machine.TryGetReady(Owner, Position, unfinished.QueueId, out _));
            Assert.IsFalse(_machine.TryCancel(Owner, Position, unfinished.QueueId, out _),
                "Ready entries leave the queue only via deployment, not cancel.");
        }

        [Test]
        public void Capacity_IsPerOwnerPerBuilding()
        {
            Assert.IsTrue(_machine.CanEnqueue(Owner, Position, 1, out _));
            Enqueue(trainingTurns: 5, turn: 1);
            Assert.IsFalse(_machine.CanEnqueue(Owner, Position, 1, out var reason));
            Assert.IsNotEmpty(reason);
            Assert.IsTrue(_machine.CanEnqueue("owner-b", Position, 1, out _),
                "Another owner's queue at the same position is independent.");
            Assert.IsTrue(_machine.CanEnqueue(Owner, new Vector2Int(9, 9), 1, out _),
                "A different building position has its own queue.");
        }

        [Test]
        public void SourceBuildingLost_ClearsItsQueueOnly()
        {
            Enqueue(trainingTurns: 3, turn: 1);
            var otherPosition = new Vector2Int(20, 20);
            _machine.EnqueueValidated(Owner, otherPosition, "barracks", "worker", 3, 1);

            Assert.IsTrue(_machine.RemoveBuildingQueues(Position));
            Assert.AreEqual(0, _machine.GetQueue(Owner, Position).Count);
            Assert.AreEqual(1, _machine.GetQueue(Owner, otherPosition).Count,
                "Queues of surviving buildings are untouched.");
            Assert.IsFalse(_machine.RemoveBuildingQueues(Position), "Second removal is a no-op.");
        }

        [Test]
        public void Restore_RoundTrips_AndKeepsQueueIdsUnique()
        {
            var a = Enqueue(trainingTurns: 3, turn: 1);
            var b = Enqueue(trainingTurns: 1, turn: 2);
            _machine.AdvanceOwnerTurn(Owner, 2);
            var snapshot = _machine.CaptureAll();

            var restored = new UnitRecruitmentQueueStateMachine();
            restored.RestoreAll(snapshot);

            var queue = restored.GetQueue(Owner, Position);
            Assert.AreEqual(2, queue.Count);
            Assert.AreEqual(a.QueueId, queue[0].QueueId);
            Assert.AreEqual(b.QueueId, queue[1].QueueId);

            var next = restored.EnqueueValidated(Owner, Position, "barracks", "worker", 1, 9);
            Assert.Greater(next.QueueId, b.QueueId, "Restore advances _nextQueueId — ids never repeat.");
        }

        [Test]
        public void Restore_RejectsDuplicateQueueIds()
        {
            var item = Enqueue(trainingTurns: 2, turn: 1);
            var snapshot = new List<UnitRecruitmentQueueItemSnapshot> { item, item };
            Assert.Throws<System.InvalidOperationException>(() => _machine.RestoreAll(snapshot));
        }

        [Test]
        public void RealtimeAdvance_CompletesBySeconds()
        {
            Enqueue(trainingTurns: 2, turn: 1);
            var changed = _machine.AdvanceRealtime(30f, 10f); // 2 turns * 10s = 20s needed
            Assert.IsNotEmpty(changed);
            Assert.IsTrue(_machine.TryPeekReady(Owner, Position, out _));
        }

        [Test]
        public void RealtimeAdvance_HeadConsumesBudgetFirst()
        {
            Enqueue(trainingTurns: 1, turn: 1);
            Enqueue(trainingTurns: 1, turn: 1);
            _machine.AdvanceRealtime(10f, 10f); // exactly one entry's worth
            var queue = _machine.GetQueue(Owner, Position);
            Assert.AreEqual(UnitRecruitmentQueueStatus.Ready, queue[0].Status);
            Assert.AreEqual(UnitRecruitmentQueueStatus.Training, queue[1].Status,
                "The completed head hands the training slot to the next entry.");
            Assert.AreEqual(0, queue[1].CompletedTurns,
                "Leftover seconds flow to the next entry only after the head finishes.");
        }
    }
}
