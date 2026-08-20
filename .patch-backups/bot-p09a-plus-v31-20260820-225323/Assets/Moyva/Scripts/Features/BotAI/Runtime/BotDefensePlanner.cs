using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotDefensePlanner : IBotDefensePlanner
    {
        private readonly IBuildingRegistry _buildings;
        private readonly IUnitMovementQuery _movementQuery;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly BotPlanningProfile _profile;

        [Inject]
        public BotDefensePlanner(
            [InjectOptional] IBuildingRegistry buildings = null,
            [InjectOptional] IUnitMovementQuery movementQuery = null,
            [InjectOptional] IUnitClassConfig unitConfigs = null,
            [InjectOptional] BotPlanningProfile profile = null)
        {
            _buildings = buildings;
            _movementQuery = movementQuery;
            _unitConfigs = unitConfigs;
            _profile = profile ?? BotPlanningProfile.Normal();
        }

        public BotDefenseContext Analyze(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
            => BotTacticalAnalysis.BuildDefenseContext(
                snapshot,
                _buildings,
                _unitConfigs,
                _profile);

        public IReadOnlyList<string> GetProtectedHomeGuardUnitIds(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            if (snapshot == null
                || _profile.MinimumHomeDefenders <= 0
                || strategy.Posture == BotStrategicPosture.EmergencyDefense
                || !BotTacticalAnalysis.TryResolveOwnCastle(
                    snapshot,
                    _buildings,
                    out BotBuildingSnapshot castle))
            {
                return Array.Empty<string>();
            }

            List<DefenderUnit> military = CollectMilitaryUnits(snapshot, castle.Position);
            if (military.Count <= _profile.MinimumHomeDefenders)
                return Array.Empty<string>();

            military.Sort(CompareDefender);
            int count = Math.Min(_profile.MinimumHomeDefenders, military.Count - 1);
            if (count <= 0)
                return Array.Empty<string>();

            var result = new string[count];
            for (int index = 0; index < count; index++)
                result[index] = military[index].Unit.UnitId;
            return result;
        }

        public IReadOnlyList<BotActionCandidate> Generate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            if (snapshot == null
                || _movementQuery == null
                || snapshot.OwnUnits.Count == 0)
            {
                return Array.Empty<BotActionCandidate>();
            }

            BotDefenseContext context = Analyze(snapshot, strategy);
            if (!context.HasCastle || !context.HasVisibleThreats)
                return Array.Empty<BotActionCandidate>();

            bool emergency =
                strategy.Posture == BotStrategicPosture.EmergencyDefense
                || context.TotalThreatScore >= _profile.EmergencyThreatThreshold;
            if (!emergency)
                return Array.Empty<BotActionCandidate>();

            List<DefenderUnit> defenders = CollectMilitaryUnits(
                snapshot,
                context.CastlePosition);
            defenders.Sort(CompareDefender);

            int defenderLimit = Math.Min(
                _profile.MaxDefendersToEvaluate,
                defenders.Count);
            if (defenderLimit <= 0)
                return Array.Empty<BotActionCandidate>();

            var candidates = new List<BotActionCandidate>(defenderLimit);
            var reserved = new HashSet<Vector2Int>();
            BotThreatSnapshot primaryThreat = context.Threats[0];

            for (int defenderIndex = 0;
                 defenderIndex < defenderLimit;
                 defenderIndex++)
            {
                DefenderUnit defender = defenders[defenderIndex];
                if (!TryChooseIntercept(
                        defender,
                        primaryThreat,
                        context,
                        reserved,
                        out Vector2Int target,
                        out int score))
                {
                    continue;
                }

                reserved.Add(target);
                candidates.Add(new BotActionCandidate(
                    $"defense:intercept:{defender.Unit.UnitId}:{target.x},{target.y}",
                    BotActionKind.Move,
                    BotStrategicPosture.EmergencyDefense,
                    new BotActionScore(
                        score,
                        "Emergency interception from canonical reachable tiles."),
                    actorId: defender.Unit.UnitId,
                    targetCell: target,
                    reason: "emergency-intercept"));
            }

            candidates.Sort(CompareCandidate);
            return candidates;
        }

        private bool TryChooseIntercept(
            DefenderUnit defender,
            BotThreatSnapshot threat,
            BotDefenseContext context,
            HashSet<Vector2Int> reserved,
            out Vector2Int target,
            out int score)
        {
            target = default;
            score = int.MinValue;

            IReadOnlyList<UnitMovementTileSnapshot> tiles =
                _movementQuery.GetMovementTiles(defender.Unit.UnitId);
            if (tiles == null || tiles.Count == 0)
                return false;

            int currentEnemyDistance =
                BotTacticalAnalysis.Chebyshev(
                    defender.Unit.Position,
                    threat.EnemyPosition);
            int currentCastleDistance =
                BotTacticalAnalysis.Chebyshev(
                    defender.Unit.Position,
                    context.CastlePosition);
            int ownAttackRange = Math.Max(
                1,
                defender.Config?.AttackRange ?? 1);

            for (int index = 0; index < tiles.Count; index++)
            {
                UnitMovementTileSnapshot tile = tiles[index];
                if (!tile.IsReachable
                    || tile.Position == defender.Unit.Position
                    || reserved.Contains(tile.Position))
                {
                    continue;
                }

                int enemyDistance =
                    BotTacticalAnalysis.Chebyshev(
                        tile.Position,
                        threat.EnemyPosition);
                int castleDistance =
                    BotTacticalAnalysis.Chebyshev(
                        tile.Position,
                        context.CastlePosition);

                int candidateScore = 2600;
                candidateScore += Math.Min(3500, threat.ThreatScore / 20);
                candidateScore += (currentEnemyDistance - enemyDistance) * 220;
                candidateScore += (currentCastleDistance - castleDistance) * 80;

                if (castleDistance <= _profile.HomeDefenseRadius)
                    candidateScore += 500;
                else
                    candidateScore -=
                        (castleDistance - _profile.HomeDefenseRadius) * 250;

                if (enemyDistance <= ownAttackRange)
                    candidateScore += 450;

                int corridorLength = enemyDistance + castleDistance;
                if (corridorLength <= threat.DistanceToCastle + 2)
                    candidateScore += 260;

                candidateScore -= Mathf.RoundToInt(
                    Mathf.Max(0f, tile.Cost) * 8f);

                if (candidateScore > score
                    || (candidateScore == score
                        && BotTurnExecutor.ComparePosition(
                            tile.Position,
                            target) < 0))
                {
                    target = tile.Position;
                    score = candidateScore;
                }
            }

            return score > int.MinValue;
        }

        private List<DefenderUnit> CollectMilitaryUnits(
            BotWorldSnapshot snapshot,
            Vector2Int castlePosition)
        {
            var result = new List<DefenderUnit>();
            for (int index = 0; index < snapshot.OwnUnits.Count; index++)
            {
                BotUnitSnapshot unit = snapshot.OwnUnits[index];
                if (string.IsNullOrWhiteSpace(unit.UnitId)
                    || !string.Equals(
                        unit.OwnerId,
                        snapshot.OwnerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                UnitClassConfig config =
                    BotTacticalAnalysis.SafeGetConfig(
                        _unitConfigs,
                        unit.TypeId);

                if (config != null && config.Role != UnitRole.Military)
                    continue;

                result.Add(new DefenderUnit(
                    unit,
                    config,
                    BotTacticalAnalysis.Chebyshev(
                        unit.Position,
                        castlePosition)));
            }

            return result;
        }

        private static int CompareDefender(
            DefenderUnit left,
            DefenderUnit right)
        {
            int distance =
                left.CastleDistance.CompareTo(right.CastleDistance);
            if (distance != 0)
                return distance;

            int hp =
                (right.Config?.HitPoints ?? 0)
                .CompareTo(left.Config?.HitPoints ?? 0);
            if (hp != 0)
                return hp;

            int damage =
                BotTacticalAnalysis.EstimateRawThreatDamage(right.Config)
                .CompareTo(
                    BotTacticalAnalysis.EstimateRawThreatDamage(left.Config));
            return damage != 0
                ? damage
                : string.CompareOrdinal(
                    left.Unit.UnitId,
                    right.Unit.UnitId);
        }

        private static int CompareCandidate(
            BotActionCandidate left,
            BotActionCandidate right)
        {
            int score =
                right.Score.Total.CompareTo(left.Score.Total);
            return score != 0
                ? score
                : string.CompareOrdinal(
                    left.CandidateId,
                    right.CandidateId);
        }

        private readonly struct DefenderUnit
        {
            public DefenderUnit(
                BotUnitSnapshot unit,
                UnitClassConfig config,
                int castleDistance)
            {
                Unit = unit;
                Config = config;
                CastleDistance = castleDistance;
            }

            public BotUnitSnapshot Unit { get; }
            public UnitClassConfig Config { get; }
            public int CastleDistance { get; }
        }
    }
}
