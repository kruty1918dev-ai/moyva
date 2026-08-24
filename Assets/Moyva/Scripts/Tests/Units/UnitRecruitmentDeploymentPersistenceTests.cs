using System.Collections.Generic;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Units
{
    public sealed class UnitRecruitmentDeploymentPersistenceTests
    {
        [Test]
        public void CaptureRestore_PreservesPaidQueueIdentityProgressAndOrder()
        {
            var queue = new UnitRecruitmentQueueStateMachine();
            var pos = new Vector2Int(4, 5);
            queue.EnqueueValidated("p1", pos, "barrack", "warrior", 2, 10);
            queue.EnqueueValidated("p1", pos, "barrack", "archer", 3, 10);
            queue.AdvanceOwnerTurn("p1", 11);

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> saved = queue.CaptureAll();
            var restored = new UnitRecruitmentQueueStateMachine();
            restored.RestoreAll(saved);
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> result = restored.GetQueue("p1", pos);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(saved[0].QueueId, result[0].QueueId);
            Assert.AreEqual("barrack", result[0].RecruitingBuildingId);
            Assert.AreEqual(1, result[0].CompletedTurns);
            Assert.AreEqual(11, result[0].LastProgressGlobalTurn);
            Assert.AreEqual("archer", result[1].UnitTypeId);
        }

        [Test]
        public void Restore_RecomputesNextQueueIdWithoutCollision()
        {
            var restored = new UnitRecruitmentQueueStateMachine();
            restored.RestoreAll(new[]
            {
                new UnitRecruitmentQueueItemSnapshot(
                    41, "p1", Vector2Int.zero, "barrack", "warrior",
                    0, 2, 5, 5, UnitRecruitmentQueueStatus.Training),
            });

            UnitRecruitmentQueueItemSnapshot next = restored.EnqueueValidated(
                "p1", Vector2Int.zero, "barrack", "archer", 1, 6);
            Assert.AreEqual(42, next.QueueId);
        }

        [Test]
        public void CompletionAfterSpawn_RemovesReadyHeadByExpectedId()
        {
            var queue = new UnitRecruitmentQueueStateMachine();
            var pos = new Vector2Int(3, 7);
            UnitRecruitmentQueueItemSnapshot created = queue.EnqueueValidated(
                "p1", pos, "barrack", "warrior", 1, 1);
            queue.AdvanceOwnerTurn("p1", 2);

            Assert.IsTrue(queue.TryTakeReady("p1", pos, created.QueueId, out var completed));
            Assert.AreEqual(created.QueueId, completed.QueueId);
            Assert.AreEqual(2, completed.LastProgressGlobalTurn);
            Assert.AreEqual(0, queue.GetQueue("p1", pos).Count);
            Assert.IsFalse(queue.TryTakeReady("p1", pos, created.QueueId, out _));
        }

        [Test]
        public void ReadyHeads_AreReturnedInDeterministicPositionOrder()
        {
            var queue = new UnitRecruitmentQueueStateMachine();
            queue.EnqueueValidated("p1", new Vector2Int(8, 2), "barrack", "warrior", 1, 1);
            queue.EnqueueValidated("p1", new Vector2Int(1, 9), "barrack", "archer", 1, 1);
            queue.AdvanceOwnerTurn("p1", 2);

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> ready = queue.GetReadyHeads("p1");
            Assert.AreEqual(new Vector2Int(1, 9), ready[0].RecruitingBuildingPosition);
            Assert.AreEqual(new Vector2Int(8, 2), ready[1].RecruitingBuildingPosition);
        }

        [Test]
        public void SpawnCandidates_AreStableUniqueAndExcludeBuildingCell()
        {
            var center = new Vector2Int(10, 10);
            List<Vector2Int> first = UnitRecruitmentDeploymentService.BuildSpawnCandidates(center, 2);
            List<Vector2Int> second = UnitRecruitmentDeploymentService.BuildSpawnCandidates(center, 2);
            CollectionAssert.AreEqual(first, second);
            Assert.AreEqual(24, first.Count);
            Assert.IsFalse(first.Contains(center));
            Assert.AreEqual(first.Count, new HashSet<Vector2Int>(first).Count);
        }

        [Test]
        public void StableDeploymentId_IsDerivedFromQueueIdentity()
        {
            Assert.AreEqual(
                "recruit_0000000012_warrior",
                UnitRecruitmentDeploymentService.BuildRecruitmentUnitId(12, "warrior"));
        }

        [Test]
        public void Snapshot_DerivesReadyStateFromProgressInsteadOfSerializedFlag()
        {
            var item = new UnitRecruitmentQueueItemSnapshot(
                1, "p1", Vector2Int.zero, "barrack", "warrior",
                1, 2, 4, 4, UnitRecruitmentQueueStatus.Ready);
            Assert.IsFalse(item.IsReady);
            Assert.AreEqual(1, item.RemainingTurns);
        }
    }
}
