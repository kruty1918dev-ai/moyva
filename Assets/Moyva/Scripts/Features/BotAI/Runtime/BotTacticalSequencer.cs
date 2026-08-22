using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotTacticalSequencer : IBotTacticalSequencer
    {
        private readonly IUnitMovementQuery _movement;
        private readonly IUnitCombatService _combat;
        private readonly IUnitClassConfig _configs;
        private readonly IBotInfluenceMapService _influence;
        private readonly IBotUnitRoleResolver _roles;
        private readonly BotPlanningProfile _profile;

        [Inject]
        public BotTacticalSequencer(
            [InjectOptional] IUnitMovementQuery movement = null,
            [InjectOptional] IUnitCombatService combat = null,
            [InjectOptional] IUnitClassConfig configs = null,
            [InjectOptional] IBotInfluenceMapService influence = null,
            [InjectOptional] IBotUnitRoleResolver roles = null,
            [InjectOptional] BotPlanningProfile profile = null)
        {
            _movement = movement;
            _combat = combat;
            _configs = configs;
            _influence = influence;
            _roles = roles;
            _profile = profile ?? BotPlanningProfile.Normal();
        }

        public IReadOnlyList<BotActionCandidate> Generate(BotWorldSnapshot snapshot, BotStrategicContext strategy)
        {
            if (snapshot == null || _movement == null || snapshot.OwnUnits.Count == 0)
                return Array.Empty<BotActionCandidate>();

            var result = new List<BotActionCandidate>();
            for (int unitIndex = 0; unitIndex < snapshot.OwnUnits.Count; unitIndex++)
            {
                BotUnitSnapshot unit = snapshot.OwnUnits[unitIndex];
                if (string.IsNullOrWhiteSpace(unit.UnitId))
                    continue;

                UnitClassConfig config = BotTacticalAnalysis.SafeGetConfig(_configs, unit.TypeId);
                BotUnitTacticalRole role = _roles?.Resolve(config) ?? BotAdvancedHeuristics.InferRole(config);
                bool lowHealth = IsLowHealth(unit.UnitId);

                IReadOnlyList<UnitMovementTileSnapshot> tiles = _movement.GetMovementTiles(unit.UnitId);
                if (tiles == null || tiles.Count == 0)
                    continue;

                if (lowHealth && TryChooseRetreat(snapshot, unit, tiles, out Vector2Int retreat, out int retreatScore))
                {
                    result.Add(new BotActionCandidate(
                        $"tactical:retreat:{unit.UnitId}:{retreat.x},{retreat.y}",
                        BotActionKind.Move,
                        strategy.Posture,
                        new BotActionScore(retreatScore, "Low-HP retreat toward lower visible threat."),
                        actorId: unit.UnitId,
                        targetCell: retreat,
                        reason: "tactical-retreat"));
                    continue;
                }

                if (snapshot.VisibleEnemyUnits.Count == 0 || role == BotUnitTacticalRole.Worker)
                    continue;

                int attackRange = Math.Max(1, config?.AttackRange ?? 1);
                if (TryChooseMoveToAttack(snapshot, unit, tiles, attackRange, role, out BotUnitSnapshot enemy, out Vector2Int target, out int score))
                {
                    result.Add(new BotActionCandidate(
                        $"tactical:move-to-attack:{unit.UnitId}:{enemy.UnitId}:{target.x},{target.y}",
                        BotActionKind.Move,
                        strategy.Posture,
                        new BotActionScore(score, "Canonical move enters attack radius; attack is replanned after movement."),
                        actorId: unit.UnitId,
                        targetId: enemy.UnitId,
                        targetCell: target,
                        reason: "move-to-attack-intent"));
                }
            }

            result.Sort(CompareCandidate);
            return result;
        }

        private bool IsLowHealth(string unitId)
        {
            if (_combat == null || !_combat.TryGetHealth(unitId, out UnitHealthSnapshot health) || health.MaxHp <= 0)
                return false;
            return health.CurrentHp * 100 <= health.MaxHp * _profile.RetreatHealthPercent;
        }

        private bool TryChooseMoveToAttack(
            BotWorldSnapshot snapshot,
            BotUnitSnapshot unit,
            IReadOnlyList<UnitMovementTileSnapshot> tiles,
            int attackRange,
            BotUnitTacticalRole role,
            out BotUnitSnapshot selectedEnemy,
            out Vector2Int selectedTile,
            out int selectedScore)
        {
            selectedEnemy = default;
            selectedTile = default;
            selectedScore = int.MinValue;
            int evaluated = 0;

            for (int enemyIndex = 0; enemyIndex < snapshot.VisibleEnemyUnits.Count; enemyIndex++)
            {
                BotUnitSnapshot enemy = snapshot.VisibleEnemyUnits[enemyIndex];
                if (string.IsNullOrWhiteSpace(enemy.UnitId))
                    continue;

                // If already legal, immediate combat planner should win; do not waste movement.
                if (_combat != null && _combat.CanAttack(unit.UnitId, enemy.UnitId, out _))
                    continue;

                for (int tileIndex = 0; tileIndex < tiles.Count && evaluated < _profile.MaxTacticalTilesPerUnit; tileIndex++)
                {
                    UnitMovementTileSnapshot tile = tiles[tileIndex];
                    if (!tile.IsReachable || tile.Position == unit.Position)
                        continue;
                    evaluated++;

                    int enemyDistance = BotAdvancedHeuristics.Chebyshev(tile.Position, enemy.Position);
                    if (enemyDistance > attackRange)
                        continue;

                    BotInfluenceScore influence = _influence?.Evaluate(snapshot, tile.Position) ?? default;
                    int score = _profile.MoveToAttackWeight;
                    score += Math.Max(0, 400 - enemyDistance * 50);
                    score += Math.Max(-900, Math.Min(900, influence.NetUtility));
                    score -= Mathf.RoundToInt(Mathf.Max(0f, tile.Cost) * 8f);
                    if (role == BotUnitTacticalRole.Ranged && enemyDistance == attackRange)
                        score += 260;
                    if (role == BotUnitTacticalRole.Frontline)
                        score += 80;

                    if (score > selectedScore
                        || score == selectedScore && CompareTarget(enemy, tile.Position, selectedEnemy, selectedTile) < 0)
                    {
                        selectedEnemy = enemy;
                        selectedTile = tile.Position;
                        selectedScore = score;
                    }
                }
            }
            return selectedScore > int.MinValue;
        }

        private bool TryChooseRetreat(
            BotWorldSnapshot snapshot,
            BotUnitSnapshot unit,
            IReadOnlyList<UnitMovementTileSnapshot> tiles,
            out Vector2Int selected,
            out int selectedScore)
        {
            selected = default;
            selectedScore = int.MinValue;
            for (int index = 0; index < tiles.Count && index < _profile.MaxTacticalTilesPerUnit; index++)
            {
                UnitMovementTileSnapshot tile = tiles[index];
                if (!tile.IsReachable || tile.Position == unit.Position)
                    continue;

                BotInfluenceScore influence = _influence?.Evaluate(snapshot, tile.Position) ?? default;
                int nearestEnemy = 99;
                for (int e = 0; e < snapshot.VisibleEnemyUnits.Count; e++)
                    nearestEnemy = Math.Min(nearestEnemy, BotAdvancedHeuristics.Chebyshev(tile.Position, snapshot.VisibleEnemyUnits[e].Position));

                int score = _profile.RetreatWeight + nearestEnemy * 90 - influence.Threat * 2 + influence.Support;
                score -= Mathf.RoundToInt(Mathf.Max(0f, tile.Cost) * 6f);
                if (score > selectedScore || score == selectedScore && BotDeterministicGeometry.ComparePosition(tile.Position, selected) < 0)
                {
                    selected = tile.Position;
                    selectedScore = score;
                }
            }
            return selectedScore > int.MinValue;
        }

        private static int CompareTarget(BotUnitSnapshot leftEnemy, Vector2Int leftTile, BotUnitSnapshot rightEnemy, Vector2Int rightTile)
        {
            int enemy = string.CompareOrdinal(leftEnemy.UnitId, rightEnemy.UnitId);
            return enemy != 0 ? enemy : BotDeterministicGeometry.ComparePosition(leftTile, rightTile);
        }

        private static int CompareCandidate(BotActionCandidate left, BotActionCandidate right)
        {
            int score = right.Score.Total.CompareTo(left.Score.Total);
            return score != 0 ? score : string.CompareOrdinal(left.CandidateId, right.CandidateId);
        }
    }
}
