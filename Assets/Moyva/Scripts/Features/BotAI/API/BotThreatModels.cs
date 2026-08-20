using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.API
{
    public readonly struct BotThreatSnapshot
    {
        public BotThreatSnapshot(
            string enemyUnitId,
            Vector2Int enemyPosition,
            int attackRange,
            int estimatedDamage,
            int distanceToCastle,
            bool canAttackCastleAreaNow,
            bool canThreatenCastleNextTurn,
            int threatScore)
        {
            EnemyUnitId = string.IsNullOrWhiteSpace(enemyUnitId) ? string.Empty : enemyUnitId.Trim();
            EnemyPosition = enemyPosition;
            AttackRange = Math.Max(0, attackRange);
            EstimatedDamage = Math.Max(0, estimatedDamage);
            DistanceToCastle = Math.Max(0, distanceToCastle);
            CanAttackCastleAreaNow = canAttackCastleAreaNow;
            CanThreatenCastleNextTurn = canThreatenCastleNextTurn;
            ThreatScore = threatScore;
        }

        public string EnemyUnitId { get; }
        public Vector2Int EnemyPosition { get; }
        public int AttackRange { get; }
        public int EstimatedDamage { get; }
        public int DistanceToCastle { get; }
        public bool CanAttackCastleAreaNow { get; }
        public bool CanThreatenCastleNextTurn { get; }
        public int ThreatScore { get; }
    }

    public sealed class BotDefenseContext
    {
        public BotDefenseContext(
            bool hasCastle,
            Vector2Int castlePosition,
            int totalThreatScore,
            IReadOnlyList<BotThreatSnapshot> threats)
        {
            HasCastle = hasCastle;
            CastlePosition = castlePosition;
            TotalThreatScore = Math.Max(0, totalThreatScore);
            Threats = threats ?? Array.Empty<BotThreatSnapshot>();
        }

        public bool HasCastle { get; }
        public Vector2Int CastlePosition { get; }
        public int TotalThreatScore { get; }
        public IReadOnlyList<BotThreatSnapshot> Threats { get; }
        public bool HasVisibleThreats => Threats.Count > 0;
    }
}
