using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotAdvancedTacticalPlannerTests
    {
        [Test]
        public void Advanced_Chebyshev_IsEightNeighborMetric()
        {
            Assert.That(BotAdvancedHeuristics.Chebyshev(new Vector2Int(0,0), new Vector2Int(4,2)), Is.EqualTo(4));
            Assert.That(BotAdvancedHeuristics.Chebyshev(new Vector2Int(3,-2), new Vector2Int(-1,5)), Is.EqualTo(7));
        }

        [Test]
        public void Advanced_SectorKey_IsDeterministic()
        {
            int first=BotAdvancedHeuristics.StableSectorKey(new Vector2Int(13,-7));
            for(int i=0;i<100;i++) Assert.That(BotAdvancedHeuristics.StableSectorKey(new Vector2Int(13,-7)),Is.EqualTo(first));
        }

        [Test]
        public void Advanced_GoalStore_EmergencyOverridesHeldGoal()
        {
            var store=new BotGoalStore(BotPlanningProfile.Normal());
            BotWorldSnapshot opening=Snapshot("bot",1,0,0);
            BotGoalSnapshot first=store.Observe(opening,new BotStrategicContext("bot",1,BotStrategicPosture.Opening,900,"opening"));
            Assert.That(first.Kind,Is.EqualTo(BotGoalKind.StabilizeOpening));

            BotWorldSnapshot emergency=Snapshot("bot",2,1,1);
            BotGoalSnapshot next=store.Observe(emergency,new BotStrategicContext("bot",2,BotStrategicPosture.EmergencyDefense,1000,"threat"));
            Assert.That(next.Kind,Is.EqualTo(BotGoalKind.DefendBase));
        }

        [Test]
        public void Advanced_GoalStore_HysteresisKeepsNonEmergencyGoal()
        {
            var profile=BotPlanningProfile.Normal();
            var store=new BotGoalStore(profile);
            BotGoalSnapshot first=store.Observe(Snapshot("bot",10,1,0),new BotStrategicContext("bot",10,BotStrategicPosture.ArmyBuildUp,600,"army"));
            BotGoalSnapshot second=store.Observe(Snapshot("bot",11,4,0),new BotStrategicContext("bot",11,BotStrategicPosture.Search,500,"search"));
            Assert.That(second.Kind,Is.EqualTo(first.Kind));
            Assert.That(second.CreatedTurn,Is.EqualTo(first.CreatedTurn));
        }

        [Test]
        public void Advanced_GoalStore_CaptureIsOwnerStable()
        {
            var store=new BotGoalStore(BotPlanningProfile.Normal());
            store.Observe(Snapshot("z",1,0,0),new BotStrategicContext("z",1,BotStrategicPosture.Opening,1,""));
            store.Observe(Snapshot("a",1,0,0),new BotStrategicContext("a",1,BotStrategicPosture.Opening,1,""));
            IReadOnlyList<BotGoalSnapshot> captured=store.Capture();
            Assert.That(captured.Count,Is.EqualTo(2));
            Assert.That(captured[0].OwnerId,Is.EqualTo("a"));
            Assert.That(captured[1].OwnerId,Is.EqualTo("z"));
        }

        [Test]
        public void Advanced_GoalStore_RestoreRoundTrip()
        {
            var source=new BotGoalStore(BotPlanningProfile.Normal());
            source.Observe(Snapshot("bot",3,1,1),new BotStrategicContext("bot",3,BotStrategicPosture.Pressure,600,"contact"));
            var restored=new BotGoalStore(BotPlanningProfile.Normal());
            restored.Restore(source.Capture());
            Assert.That(restored.TryGet("bot",out BotGoalSnapshot goal),Is.True);
            Assert.That(goal.Kind,Is.EqualTo(BotGoalKind.PressureEnemy));
        }

        [Test]
        public void Advanced_DecisionTrace_IsBoundedAndOrdered()
        {
            var trace=new BotDecisionTraceStore();
            var candidates=new List<BotActionCandidate>();
            for(int i=0;i<20;i++) candidates.Add(new BotActionCandidate($"c{i:D2}",BotActionKind.Move,BotStrategicPosture.Pressure,new BotActionScore(100-i,"x")));
            trace.Record("bot",9,new BotStrategicContext("bot",9,BotStrategicPosture.Pressure,1,""),candidates);
            IReadOnlyList<BotDecisionTraceEntry> entries=trace.GetLast("bot");
            Assert.That(entries.Count,Is.EqualTo(8));
            Assert.That(entries[0].CandidateId,Is.EqualTo("c00"));
            Assert.That(entries[7].CandidateId,Is.EqualTo("c07"));
        }

        [Test]
        public void Advanced_PlanningProfile_TacticalBoundsAreSafe()
        {
            BotPlanningProfile p=BotPlanningProfile.Normal();
            Assert.That(p.RetreatHealthPercent,Is.InRange(1,100));
            Assert.That(p.MaxTacticalTilesPerUnit,Is.GreaterThanOrEqualTo(8));
            Assert.That(p.MaxScoutsPerTurn,Is.GreaterThanOrEqualTo(1));
            Assert.That(p.GoalHysteresisTurns,Is.GreaterThanOrEqualTo(0));
        }

        [TestCase(-12,-12)] [TestCase(-7,5)] [TestCase(0,0)] [TestCase(1,1)] [TestCase(11,19)]
        public void Advanced_SectorKey_RepeatedCoordinatesNeverDrift(int x,int y)
        {
            int expected=BotAdvancedHeuristics.StableSectorKey(new Vector2Int(x,y));
            Assert.That(BotAdvancedHeuristics.StableSectorKey(new Vector2Int(x,y)),Is.EqualTo(expected));
        }

        private static BotWorldSnapshot Snapshot(string owner,long turn,int buildings,int enemies)
        {
            var ownBuildings=new List<BotBuildingSnapshot>();
            for(int i=0;i<buildings;i++) ownBuildings.Add(new BotBuildingSnapshot($"b{i}",owner,new Vector2Int(i,0)));
            var visible=new List<BotUnitSnapshot>();
            for(int i=0;i<enemies;i++) visible.Add(new BotUnitSnapshot($"e{i}","enemy","warrior",new Vector2Int(3+i,3),1f));
            return new BotWorldSnapshot(owner,1,turn,TurnPhase.AwaitingInput,0,Vector2Int.zero,
                Array.Empty<BotUnitSnapshot>(),visible,ownBuildings,
                Array.Empty<Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot>(),
                Array.Empty<BotKnownEntityMemory>());
        }
    }
}
