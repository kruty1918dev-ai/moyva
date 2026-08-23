using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotStrategicPlanner : IBotStrategicPlanner, IBotStrategicStateStore
    {
        internal const int EmergencyDefenseRadius = 6;
        internal const int HysteresisMargin = 75;
        internal const int OpeningInfrastructureScore = 500;
        internal const int EnemyContactScore = 400;

        private readonly Dictionary<string, BotStrategicContext> _lastByOwner = new(StringComparer.Ordinal);
        private readonly IBuildingRegistry _buildings;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly BotPlanningProfile _profile;

        [Inject]
        public BotStrategicPlanner(
            [InjectOptional] IBuildingRegistry buildings = null,
            [InjectOptional] IUnitClassConfig unitConfigs = null,
            [InjectOptional] BotPlanningProfile profile = null)
        {
            _buildings = buildings;
            _unitConfigs = unitConfigs;
            _profile = profile ?? BotPlanningProfile.Normal();
        }

        public BotStrategicContext Plan(BotWorldSnapshot snapshot)
        {
            if (snapshot == null)
                return new BotStrategicContext(string.Empty, 0, BotStrategicPosture.Recovery, 0, "Snapshot unavailable.");

            var scored = new List<ScoredPosture>
            {
                ScoreEmergency(snapshot),
                ScoreOpening(snapshot),
                ScoreArmyBuildUp(snapshot),
                ScorePressure(snapshot),
                ScoreSiege(snapshot),
                ScoreSearch(snapshot),
                ScoreRecovery(snapshot),
            };

            scored.Sort(CompareScore);
            ScoredPosture best = scored[0];

            if (_lastByOwner.TryGetValue(snapshot.OwnerId, out BotStrategicContext previous)
                && previous.GlobalTurn < snapshot.GlobalTurn)
            {
                if (previous.PostureScore > 0 && best.Score - previous.PostureScore < HysteresisMargin)
                    best = new ScoredPosture(
                        previous.Posture,
                        previous.PostureScore,
                        previous.Reason + " Hysteresis retained previous posture.");
            }

            var context = new BotStrategicContext(
                snapshot.OwnerId,
                snapshot.GlobalTurn,
                best.Posture,
                best.Score,
                best.Reason);
            _lastByOwner[snapshot.OwnerId] = context;
            return context;
        }

        public IReadOnlyList<BotStrategicStateSnapshot> CaptureStrategicState()
        {
            var owners = new List<string>(_lastByOwner.Keys);
            owners.Sort(StringComparer.Ordinal);

            var result = new List<BotStrategicStateSnapshot>(owners.Count);
            for (int index = 0; index < owners.Count; index++)
            {
                BotStrategicContext context = _lastByOwner[owners[index]];
                result.Add(new BotStrategicStateSnapshot(
                    context.OwnerId,
                    context.GlobalTurn,
                    context.Posture,
                    context.PostureScore,
                    context.Reason));
            }

            return result;
        }

        public void RestoreStrategicState(IReadOnlyList<BotStrategicStateSnapshot> states)
        {
            _lastByOwner.Clear();
            if (states == null)
                return;

            for (int index = 0; index < states.Count; index++)
            {
                BotStrategicStateSnapshot state = states[index];
                if (string.IsNullOrWhiteSpace(state.OwnerId))
                    continue;

                _lastByOwner[state.OwnerId] = new BotStrategicContext(
                    state.OwnerId,
                    state.GlobalTurn,
                    state.Posture,
                    state.PostureScore,
                    state.Reason);
            }
        }

        private ScoredPosture ScoreEmergency(BotWorldSnapshot snapshot)
        {
            BotDefenseContext defense = BotTacticalAnalysis.BuildDefenseContext(
                snapshot,
                _buildings,
                _unitConfigs,
                _profile);

            if (defense.HasCastle && defense.HasVisibleThreats)
            {
                BotThreatSnapshot primary = defense.Threats[0];
                bool credible = primary.CanAttackCastleAreaNow
                    || primary.CanThreatenCastleNextTurn
                    || defense.TotalThreatScore >= _profile.EmergencyThreatThreshold;
                if (credible)
                {
                    int score = 1000 + Math.Min(500, defense.TotalThreatScore / 100);
                    return new ScoredPosture(
                        BotStrategicPosture.EmergencyDefense,
                        score,
                        $"Visible hostile threatens data-driven Castle at {defense.CastlePosition}; threat={defense.TotalThreatScore}.");
                }

                return new ScoredPosture(
                    BotStrategicPosture.EmergencyDefense,
                    0,
                    "Visible hostiles do not currently threaten the Castle envelope.");
            }

            // Compatibility fallback for unit tests/minimal scenes that have no
            // IBuildingRegistry binding yet. Runtime DI should normally resolve
            // the real Castle above; this fallback never inspects hidden units.
            for (int index = 0; index < snapshot.VisibleEnemyUnits.Count; index++)
            {
                BotUnitSnapshot enemy = snapshot.VisibleEnemyUnits[index];
                if (ManhattanDistance(enemy.Position, snapshot.StartPosition) <= EmergencyDefenseRadius)
                {
                    return new ScoredPosture(
                        BotStrategicPosture.EmergencyDefense,
                        1000,
                        "Visible hostile near fallback start-position defense anchor.");
                }
            }

            return new ScoredPosture(BotStrategicPosture.EmergencyDefense, 0, "No immediate base threat.");
        }

        private static ScoredPosture ScoreOpening(BotWorldSnapshot snapshot)
        {
            if (snapshot.OwnBuildings.Count == 0)
                return new ScoredPosture(BotStrategicPosture.Opening, 900, "No owned buildings in snapshot.");

            if (snapshot.OwnUnits.Count == 0 && snapshot.ReadyRecruitmentItems.Count == 0)
                return new ScoredPosture(BotStrategicPosture.Opening, OpeningInfrastructureScore, "No deployed or ready army.");

            return new ScoredPosture(BotStrategicPosture.Opening, 100, "Found initial owned infrastructure.");
        }

        private static ScoredPosture ScoreArmyBuildUp(BotWorldSnapshot snapshot)
        {
            if (snapshot.OwnBuildings.Count == 0)
                return new ScoredPosture(BotStrategicPosture.ArmyBuildUp, 0, "No infrastructure for army buildup.");

            int score = snapshot.OwnUnits.Count < 3 ? 520 : 200;
            if (snapshot.ReadyRecruitmentItems.Count > 0)
                score += 160;

            return new ScoredPosture(BotStrategicPosture.ArmyBuildUp, score, "Army below desired baseline.");
        }

        private static ScoredPosture ScorePressure(BotWorldSnapshot snapshot)
        {
            if (snapshot.VisibleEnemyUnits.Count <= 0)
                return new ScoredPosture(BotStrategicPosture.Pressure, 0, "No visible enemy pressure target.");

            int score = EnemyContactScore + Math.Min(200, snapshot.OwnUnits.Count * 50);
            return new ScoredPosture(BotStrategicPosture.Pressure, score, "Visible enemy contact exists.");
        }

        private static ScoredPosture ScoreSiege(BotWorldSnapshot snapshot)
        {
            if (snapshot.OwnUnits.Count == 0)
                return new ScoredPosture(BotStrategicPosture.Siege, 0, "No army for siege.");

            if (HasKnownObjective(snapshot))
                return new ScoredPosture(BotStrategicPosture.Siege, 620, "Known enemy objective.");

            return new ScoredPosture(BotStrategicPosture.Siege, 0, "No known enemy objective.");
        }

        private static ScoredPosture ScoreSearch(BotWorldSnapshot snapshot)
        {
            if (snapshot.VisibleEnemyUnits.Count > 0)
                return new ScoredPosture(BotStrategicPosture.Search, 0, "Visible contact suppresses search.");

            int score = snapshot.Memory.Count == 0 ? 480 : 260;
            return new ScoredPosture(BotStrategicPosture.Search, score, "No visible contact.");
        }

        private static ScoredPosture ScoreRecovery(BotWorldSnapshot snapshot)
        {
            if (snapshot.OwnBuildings.Count == 0 && snapshot.OwnUnits.Count == 0)
                return new ScoredPosture(BotStrategicPosture.Recovery, 700, "No field army or infrastructure.");

            return new ScoredPosture(BotStrategicPosture.Recovery, 0, "No critical loss detected.");
        }

        private static int CompareScore(ScoredPosture left, ScoredPosture right)
        {
            int score = right.Score.CompareTo(left.Score);
            return score != 0 ? score : left.Posture.CompareTo(right.Posture);
        }

        private static int ManhattanDistance(Vector2Int left, Vector2Int right)
            => Mathf.Abs(left.x - right.x) + Mathf.Abs(left.y - right.y);

        private static bool HasKnownObjective(BotWorldSnapshot snapshot)
        {
            for (int index = 0; index < snapshot.VisibleEnemyBuildings.Count; index++)
            {
                if (IsLikelyCastle(snapshot.VisibleEnemyBuildings[index].BuildingId))
                    return true;
            }

            for (int index = 0; index < snapshot.Memory.Count; index++)
            {
                BotKnownEntityMemory memory = snapshot.Memory[index];
                if (memory.Kind == BotKnownEntityKind.Objective || IsLikelyCastle(memory.TypeId))
                    return true;
            }

            return false;
        }

        // P10 owns the full data-driven enemy-objective migration. Keep this
        // compatibility heuristic confined to objective discovery; P09A's own
        // Castle defense path is already capability-driven.
        private static bool IsLikelyCastle(string id)
            => !string.IsNullOrWhiteSpace(id)
                && id.IndexOf("castle", StringComparison.OrdinalIgnoreCase) >= 0;

        private readonly struct ScoredPosture
        {
            public ScoredPosture(BotStrategicPosture posture, int score, string reason)
            {
                Posture = posture;
                Score = score;
                Reason = reason ?? string.Empty;
            }

            public BotStrategicPosture Posture { get; }
            public int Score { get; }
            public string Reason { get; }

            public ScoredPosture WithReason(string reason)
                => new(Posture, Score, reason);
        }
    }
}
