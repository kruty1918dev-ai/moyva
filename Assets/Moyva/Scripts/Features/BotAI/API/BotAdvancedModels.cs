using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.API
{
    public enum BotUnitTacticalRole
    {
        Unknown = 0,
        Frontline = 10,
        Ranged = 20,
        FastScout = 30,
        Siege = 40,
        Support = 50,
        Worker = 60,
    }

    public enum BotGoalKind
    {
        None = 0,
        StabilizeOpening = 10,
        DevelopEconomy = 20,
        BuildArmy = 30,
        DefendBase = 40,
        PressureEnemy = 50,
        SearchEnemy = 60,
        SiegeObjective = 70,
        Recover = 80,
    }

    public readonly struct BotGoalSnapshot
    {
        public BotGoalSnapshot(
            string ownerId,
            BotGoalKind kind,
            long createdTurn,
            long minimumHoldUntilTurn,
            int priority,
            string targetId,
            Vector2Int? targetCell,
            string reason)
        {
            OwnerId = Normalize(ownerId);
            Kind = kind;
            CreatedTurn = Math.Max(1L, createdTurn);
            MinimumHoldUntilTurn = Math.Max(CreatedTurn, minimumHoldUntilTurn);
            Priority = priority;
            TargetId = Normalize(targetId);
            TargetCell = targetCell;
            Reason = Normalize(reason);
        }

        public string OwnerId { get; }
        public BotGoalKind Kind { get; }
        public long CreatedTurn { get; }
        public long MinimumHoldUntilTurn { get; }
        public int Priority { get; }
        public string TargetId { get; }
        public Vector2Int? TargetCell { get; }
        public string Reason { get; }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public readonly struct BotInfluenceScore
    {
        public BotInfluenceScore(int threat, int opportunity, int support, int formation)
        {
            Threat = Math.Max(0, threat);
            Opportunity = Math.Max(0, opportunity);
            Support = Math.Max(0, support);
            Formation = formation;
        }

        public int Threat { get; }
        public int Opportunity { get; }
        public int Support { get; }
        public int Formation { get; }
        public int NetUtility => Opportunity + Support + Formation - Threat;
    }

    public readonly struct BotDecisionTraceEntry
    {
        public BotDecisionTraceEntry(
            long globalTurn,
            BotStrategicPosture posture,
            string candidateId,
            BotActionKind kind,
            int score,
            string explanation)
        {
            GlobalTurn = globalTurn;
            Posture = posture;
            CandidateId = string.IsNullOrWhiteSpace(candidateId) ? string.Empty : candidateId.Trim();
            Kind = kind;
            Score = score;
            Explanation = string.IsNullOrWhiteSpace(explanation) ? string.Empty : explanation.Trim();
        }

        public long GlobalTurn { get; }
        public BotStrategicPosture Posture { get; }
        public string CandidateId { get; }
        public BotActionKind Kind { get; }
        public int Score { get; }
        public string Explanation { get; }
    }
}
