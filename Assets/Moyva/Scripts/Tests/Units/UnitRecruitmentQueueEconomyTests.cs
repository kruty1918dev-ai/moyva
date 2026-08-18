using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Units
{
    public sealed class UnitRecruitmentQueueEconomyTests
    {
        [Test]
        public void Queue_IsFifo_AndOnlyHeadAdvances()
        {
            var queue = new UnitRecruitmentQueueStateMachine();
            var pos = new Vector2Int(4, 7);
            Assert.IsTrue(queue.CanEnqueue("p1", pos, 3, out _));
            queue.EnqueueValidated("p1", pos, "warrior", 1, 10);
            queue.EnqueueValidated("p1", pos, "archer", 1, 10);

            Assert.IsTrue(queue.AdvanceOwnerTurn("p1", 11));
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> snapshot =
                queue.GetQueue("p1", pos);

            Assert.IsTrue(snapshot[0].IsReady);
            Assert.AreEqual(0, snapshot[1].CompletedTurns);
            Assert.IsTrue(queue.TryPeekReady("p1", pos, out var ready));
            Assert.AreEqual("warrior", ready.UnitTypeId);

            Assert.IsFalse(queue.AdvanceOwnerTurn("p1", 12),
                "A ready head must block the next queued unit until P08 deploys it.");
            Assert.AreEqual(0, queue.GetQueue("p1", pos)[1].CompletedTurns);
        }

        [Test]
        public void Queue_DoesNotProgressOnEnqueueTurnOrOtherOwnerTurn()
        {
            var queue = new UnitRecruitmentQueueStateMachine();
            var pos = new Vector2Int(1, 2);
            queue.EnqueueValidated("p1", pos, "spearman", 2, 5);

            Assert.IsFalse(queue.AdvanceOwnerTurn("p1", 5));
            Assert.IsFalse(queue.AdvanceOwnerTurn("p2", 6));
            Assert.AreEqual(0, queue.GetQueue("p1", pos)[0].CompletedTurns);

            Assert.IsTrue(queue.AdvanceOwnerTurn("p1", 6));
            Assert.AreEqual(1, queue.GetQueue("p1", pos)[0].CompletedTurns);
            Assert.IsTrue(queue.AdvanceOwnerTurn("p1", 7));
            Assert.IsTrue(queue.GetQueue("p1", pos)[0].IsReady);
        }

        [Test]
        public void Queue_DuplicateTurnStartDoesNotDoubleProgress()
        {
            var queue = new UnitRecruitmentQueueStateMachine();
            var pos = new Vector2Int(8, 8);
            queue.EnqueueValidated("p1", pos, "spearman", 2, 4);

            Assert.IsTrue(queue.AdvanceOwnerTurn("p1", 5));
            Assert.IsFalse(queue.AdvanceOwnerTurn("p1", 5));
            Assert.AreEqual(1, queue.GetQueue("p1", pos)[0].CompletedTurns);
        }

        [Test]
        public void Queue_DemolishedBuildingDropsAllOwnerQueuesAtPosition()
        {
            var queue = new UnitRecruitmentQueueStateMachine();
            var pos = new Vector2Int(9, 2);
            queue.EnqueueValidated("p1", pos, "warrior", 1, 1);
            queue.EnqueueValidated("p2", pos, "archer", 2, 1);

            Assert.IsTrue(queue.RemoveBuildingQueues(pos));
            Assert.AreEqual(0, queue.GetQueue("p1", pos).Count);
            Assert.AreEqual(0, queue.GetQueue("p2", pos).Count);
            Assert.IsFalse(queue.RemoveBuildingQueues(pos));
        }

        [Test]
        public void Queue_EnforcesCapacityPerOwnerAndBuilding()
        {
            var queue = new UnitRecruitmentQueueStateMachine();
            var pos = new Vector2Int(2, 3);
            queue.EnqueueValidated("p1", pos, "warrior", 1, 1);
            queue.EnqueueValidated("p1", pos, "archer", 2, 1);

            Assert.IsFalse(queue.CanEnqueue("p1", pos, 2, out string reason));
            StringAssert.Contains("full", reason);
            Assert.IsTrue(queue.CanEnqueue("p2", pos, 2, out _));
            Assert.IsTrue(queue.CanEnqueue("p1", new Vector2Int(3, 3), 2, out _));
        }

        [Test]
        public void Queue_AssignsStableIncreasingIds()
        {
            var queue = new UnitRecruitmentQueueStateMachine();
            var first = queue.EnqueueValidated("p1", Vector2Int.zero, "warrior", 1, 1);
            var second = queue.EnqueueValidated("p1", Vector2Int.zero, "archer", 1, 1);
            Assert.Greater(second.QueueId, first.QueueId);
        }

        [Test]
        public void CostMap_AggregatesDuplicateResourcesAndIgnoresInvalidEntries()
        {
            var costs = new List<BuildingResourceAmount>
            {
                new BuildingResourceAmount { ResourceId = "wood", Amount = 3 },
                new BuildingResourceAmount { ResourceId = " wood ", Amount = 2 },
                new BuildingResourceAmount { ResourceId = "food", Amount = 4 },
                new BuildingResourceAmount { ResourceId = "", Amount = 9 },
                null,
            };

            Dictionary<string, float> map = UnitRecruitmentService.BuildCostMap(costs);
            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(5f, map["wood"]);
            Assert.AreEqual(4f, map["food"]);
        }

        [Test]
        public void Snapshot_ClampsInvalidProgressInputs()
        {
            var item = new UnitRecruitmentQueueItemSnapshot(
                1, "p1", Vector2Int.zero, "warrior", -5, 0, 0,
                UnitRecruitmentQueueStatus.Training);
            Assert.AreEqual(0, item.CompletedTurns);
            Assert.AreEqual(1, item.TrainingTurns);
            Assert.AreEqual(1, item.EnqueuedGlobalTurn);
            Assert.AreEqual(1, item.RemainingTurns);
        }
    }
}
