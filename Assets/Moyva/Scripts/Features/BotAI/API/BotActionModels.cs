using System;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.API
{
    public enum BotActionKind
    {
        None = 0,
        DeployReadyUnit = 10,
        Attack = 20,
        Move = 30,
        Build = 40,
        Recruit = 50,
        Garrison = 60,
        Ungarrison = 61,
        ScoutMove = 70,
        Hold = 80,
    }

    public readonly struct BotActionScore : IEquatable<BotActionScore>
    {
        public BotActionScore(int total, string explanation)
        {
            Total = total;
            Explanation = string.IsNullOrWhiteSpace(explanation) ? string.Empty : explanation.Trim();
        }

        public int Total { get; }
        public string Explanation { get; }

        public bool Equals(BotActionScore other)
            => Total == other.Total && string.Equals(Explanation, other.Explanation, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => obj is BotActionScore other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Total * 397) ^ StringComparer.Ordinal.GetHashCode(Explanation ?? string.Empty);
            }
        }
    }

    public readonly struct BotActionCandidate : IEquatable<BotActionCandidate>
    {
        public BotActionCandidate(
            string candidateId,
            BotActionKind kind,
            BotStrategicPosture posture,
            BotActionScore score,
            string actorId = null,
            string targetId = null,
            Vector2Int? targetCell = null,
            string definitionId = null,
            string reason = null)
        {
            CandidateId = Normalize(candidateId) ?? $"{(int)kind}:{posture}";
            Kind = kind;
            Posture = posture;
            Score = score;
            ActorId = Normalize(actorId);
            TargetId = Normalize(targetId);
            TargetCell = targetCell;
            DefinitionId = Normalize(definitionId);
            Reason = Normalize(reason) ?? string.Empty;
        }

        public string CandidateId { get; }
        public BotActionKind Kind { get; }
        public BotStrategicPosture Posture { get; }
        public BotActionScore Score { get; }
        public string ActorId { get; }
        public string TargetId { get; }
        public Vector2Int? TargetCell { get; }
        public string DefinitionId { get; }
        public string Reason { get; }

        public bool Equals(BotActionCandidate other)
            => string.Equals(CandidateId, other.CandidateId, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => obj is BotActionCandidate other && Equals(other);

        public override int GetHashCode()
            => StringComparer.Ordinal.GetHashCode(CandidateId ?? string.Empty);

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public readonly struct BotStrategicContext
    {
        public BotStrategicContext(
            string ownerId,
            long globalTurn,
            BotStrategicPosture posture,
            int postureScore,
            string reason)
        {
            OwnerId = string.IsNullOrWhiteSpace(ownerId) ? string.Empty : ownerId.Trim();
            GlobalTurn = globalTurn;
            Posture = posture;
            PostureScore = postureScore;
            Reason = string.IsNullOrWhiteSpace(reason) ? string.Empty : reason.Trim();
        }

        public string OwnerId { get; }
        public long GlobalTurn { get; }
        public BotStrategicPosture Posture { get; }
        public int PostureScore { get; }
        public string Reason { get; }
    }
}
