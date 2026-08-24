using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotStrategicPlanner :
        IBotStrategicPlanner,
        IBotStrategicStateStore
    {
        internal const int EmergencyDefenseRadius = 6;
        internal const int HysteresisMargin = 75;
        internal const int EnemyContactScore = 400;

        private readonly Dictionary<
            string,
            BotStrategicContext> _lastByOwner =
                new(StringComparer.Ordinal);

        private readonly IBuildingRegistry _buildings;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly BotPlanningProfile _profile;
        private readonly IBotStallTracker _stall;

        [Inject]
        public BotStrategicPlanner(
            [InjectOptional] IBuildingRegistry buildings = null,
            [InjectOptional] IUnitClassConfig unitConfigs = null,
            [InjectOptional] BotPlanningProfile profile = null,
            [InjectOptional] IBotStallTracker stall = null)
        {
            _buildings = buildings;
            _unitConfigs = unitConfigs;
            _profile =
                profile ??
                BotPlanningProfile.Normal();

            _stall = stall;
        }

        public BotStrategicContext Plan(
            BotWorldSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return new BotStrategicContext(
                    string.Empty,
                    0,
                    BotStrategicPosture.Recovery,
                    0,
                    "Snapshot unavailable.");
            }

            BotStallStatus stall =
                _stall?.GetStatus(snapshot.OwnerId) ??
                default;

            var scored = new List<ScoredPosture>
            {
                ScoreEmergency(snapshot),
                ScoreOpening(snapshot),
                ScoreEconomy(snapshot),
                ScoreArmyBuildUp(snapshot),
                ScorePressure(snapshot),
                ScoreSiege(snapshot),
                ScoreSearch(snapshot),
                ScoreRecovery(snapshot, stall),
            };

            scored.Sort(CompareScore);
            ScoredPosture best = scored[0];

            // Hysteresis compares CURRENT utilities. The old implementation
            // compared the challenger against a stale previous-turn score,
            // allowing Opening=900 to survive forever after the Castle existed.
            if (!stall.IsStalled &&
                _lastByOwner.TryGetValue(
                    snapshot.OwnerId,
                    out BotStrategicContext previous) &&
                previous.GlobalTurn < snapshot.GlobalTurn &&
                previous.Posture != best.Posture &&
                TryFindCurrentScore(
                    scored,
                    previous.Posture,
                    out ScoredPosture previousNow) &&
                previousNow.Score > 0 &&
                best.Score - previousNow.Score <
                    HysteresisMargin)
            {
                best = new ScoredPosture(
                    previous.Posture,
                    previousNow.Score,
                    $"Hysteresis retained {previous.Posture}: " +
                    $"currentPreviousUtility={previousNow.Score}, " +
                    $"challenger={best.Posture}/{best.Score}, " +
                    $"margin={HysteresisMargin}. " +
                    $"Current reason: {previousNow.Reason}");
            }

            var context =
                new BotStrategicContext(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    best.Posture,
                    best.Score,
                    best.Reason);

            _lastByOwner[snapshot.OwnerId] =
                context;

            return context;
        }

        public IReadOnlyList<BotStrategicStateSnapshot>
            CaptureStrategicState()
        {
            var owners =
                new List<string>(
                    _lastByOwner.Keys);

            owners.Sort(StringComparer.Ordinal);

            var result =
                new List<BotStrategicStateSnapshot>(
                    owners.Count);

            for (int index = 0;
                 index < owners.Count;
                 index++)
            {
                BotStrategicContext context =
                    _lastByOwner[owners[index]];

                result.Add(
                    new BotStrategicStateSnapshot(
                        context.OwnerId,
                        context.GlobalTurn,
                        context.Posture,
                        context.PostureScore,
                        context.Reason));
            }

            return result;
        }

        public void RestoreStrategicState(
            IReadOnlyList<BotStrategicStateSnapshot> states)
        {
            _lastByOwner.Clear();

            if (states == null)
                return;

            for (int index = 0;
                 index < states.Count;
                 index++)
            {
                BotStrategicStateSnapshot state =
                    states[index];

                if (string.IsNullOrWhiteSpace(
                        state.OwnerId))
                {
                    continue;
                }

                _lastByOwner[state.OwnerId] =
                    new BotStrategicContext(
                        state.OwnerId,
                        state.GlobalTurn,
                        state.Posture,
                        state.PostureScore,
                        state.Reason);
            }
        }

        private ScoredPosture ScoreEmergency(
            BotWorldSnapshot snapshot)
        {
            BotDefenseContext defense =
                BotTacticalAnalysis.BuildDefenseContext(
                    snapshot,
                    _buildings,
                    _unitConfigs,
                    _profile);

            if (defense.HasCastle &&
                defense.HasVisibleThreats)
            {
                BotThreatSnapshot primary =
                    defense.Threats[0];

                bool credible =
                    primary.CanAttackCastleAreaNow ||
                    primary.CanThreatenCastleNextTurn ||
                    defense.TotalThreatScore >=
                        _profile.EmergencyThreatThreshold;

                if (credible)
                {
                    int score =
                        1600 +
                        Math.Min(
                            600,
                            defense.TotalThreatScore / 100);

                    return new ScoredPosture(
                        BotStrategicPosture.EmergencyDefense,
                        score,
                        $"Visible hostile threatens Castle at " +
                        $"{defense.CastlePosition}; " +
                        $"threat={defense.TotalThreatScore}.");
                }

                return new ScoredPosture(
                    BotStrategicPosture.EmergencyDefense,
                    0,
                    "Visible hostiles do not currently threaten " +
                    "the Castle envelope.");
            }

            // Minimal-scene fallback; only visible enemies are inspected.
            for (int index = 0;
                 index < snapshot.VisibleEnemyUnits.Count;
                 index++)
            {
                BotUnitSnapshot enemy =
                    snapshot.VisibleEnemyUnits[index];

                if (ManhattanDistance(
                        enemy.Position,
                        snapshot.StartPosition) <=
                    EmergencyDefenseRadius)
                {
                    return new ScoredPosture(
                        BotStrategicPosture.EmergencyDefense,
                        1500,
                        "Visible hostile near fallback start-position " +
                        "defense anchor.");
                }
            }

            return new ScoredPosture(
                BotStrategicPosture.EmergencyDefense,
                0,
                "No immediate base threat.");
        }

        private ScoredPosture ScoreOpening(
            BotWorldSnapshot snapshot)
        {
            if (!HasOwnedCastle(snapshot))
            {
                return new ScoredPosture(
                    BotStrategicPosture.Opening,
                    900,
                    "No owned data-driven Castle is visible in snapshot.");
            }

            // Critical invariant: once Castle exists, Opening loses.
            return new ScoredPosture(
                BotStrategicPosture.Opening,
                80,
                $"Castle established; OwnBuildings={snapshot.OwnBuildings.Count}. " +
                "Opening bootstrap is complete.");
        }

        private ScoredPosture ScoreEconomy(
            BotWorldSnapshot snapshot)
        {
            if (!HasOwnedCastle(snapshot))
            {
                return new ScoredPosture(
                    BotStrategicPosture.Economy,
                    0,
                    "Economy stage waits for the capital.");
            }

            if (_buildings == null)
            {
                return new ScoredPosture(
                    BotStrategicPosture.Economy,
                    180,
                    "Building capabilities are unavailable; economy gaps are not inferred.");
            }

            bool warehouse =
                HasOwnedCapability(
                    snapshot,
                    BuildingDefinitionCapabilities.IsWarehouse);

            bool recruitment =
                HasRecruitmentInfrastructure(snapshot);

            int industrialKinds =
                CountIndustrialKinds(snapshot);

            int score = 260;
            var gaps = new List<string>();

            if (!warehouse)
            {
                score += 280;
                gaps.Add("storage");
            }

            if (industrialKinds < 2)
            {
                score +=
                    (2 - industrialKinds) * 170;

                gaps.Add(
                    $"industrial-diversity={industrialKinds}/2");
            }

            if (!recruitment)
            {
                // Economy still values recruitment infrastructure, but
                // ArmyBuildUp will dominate once recruiting is actually viable.
                score += 90;
                gaps.Add("recruitment-infrastructure");
            }

            if (gaps.Count == 0)
                score = 180;

            return new ScoredPosture(
                BotStrategicPosture.Economy,
                score,
                gaps.Count == 0
                    ? "Economic core is sufficiently diversified."
                    : "Economic/resource-stage gaps: " +
                      string.Join(", ", gaps) + ".");
        }

        private ScoredPosture ScoreArmyBuildUp(
            BotWorldSnapshot snapshot)
        {
            if (!HasOwnedCastle(snapshot))
            {
                return new ScoredPosture(
                    BotStrategicPosture.ArmyBuildUp,
                    0,
                    "No capital for army buildup.");
            }

            bool recruitment =
                HasRecruitmentInfrastructure(snapshot);

            if (!recruitment)
            {
                return new ScoredPosture(
                    BotStrategicPosture.ArmyBuildUp,
                    300,
                    "Army is desirable, but recruitment infrastructure " +
                    "is not established yet.");
            }

            int score =
                snapshot.OwnUnits.Count < 3
                    ? 700
                    : 260;

            if (snapshot.ReadyRecruitmentItems.Count > 0)
                score += 240;

            return new ScoredPosture(
                BotStrategicPosture.ArmyBuildUp,
                score,
                $"recruitment=true; units={snapshot.OwnUnits.Count}; " +
                $"ready={snapshot.ReadyRecruitmentItems.Count}.");
        }

        private static ScoredPosture ScorePressure(
            BotWorldSnapshot snapshot)
        {
            if (snapshot.VisibleEnemyUnits.Count <= 0)
            {
                return new ScoredPosture(
                    BotStrategicPosture.Pressure,
                    0,
                    "No visible enemy pressure target.");
            }

            int score =
                EnemyContactScore +
                Math.Min(
                    240,
                    snapshot.OwnUnits.Count * 60);

            return new ScoredPosture(
                BotStrategicPosture.Pressure,
                score,
                "Visible enemy contact exists.");
        }

        private static ScoredPosture ScoreSiege(
            BotWorldSnapshot snapshot)
        {
            if (snapshot.OwnUnits.Count == 0)
            {
                return new ScoredPosture(
                    BotStrategicPosture.Siege,
                    0,
                    "No army for siege.");
            }

            if (HasKnownObjective(snapshot))
            {
                return new ScoredPosture(
                    BotStrategicPosture.Siege,
                    620,
                    "Known enemy objective.");
            }

            return new ScoredPosture(
                BotStrategicPosture.Siege,
                0,
                "No known enemy objective.");
        }

        private static ScoredPosture ScoreSearch(
            BotWorldSnapshot snapshot)
        {
            if (snapshot.VisibleEnemyUnits.Count > 0)
            {
                return new ScoredPosture(
                    BotStrategicPosture.Search,
                    0,
                    "Visible contact suppresses search.");
            }

            if (snapshot.OwnUnits.Count == 0)
            {
                return new ScoredPosture(
                    BotStrategicPosture.Search,
                    90,
                    "No deployed unit can scout yet.");
            }

            int score =
                snapshot.Memory.Count == 0
                    ? 500
                    : 300;

            return new ScoredPosture(
                BotStrategicPosture.Search,
                score,
                "No visible contact; scouting has utility.");
        }

        private static ScoredPosture ScoreRecovery(
            BotWorldSnapshot snapshot,
            BotStallStatus stall)
        {
            if (stall.IsStalled)
            {
                return new ScoredPosture(
                    BotStrategicPosture.Recovery,
                    1150,
                    stall.StallReason);
            }

            if (snapshot.OwnBuildings.Count == 0 &&
                snapshot.OwnUnits.Count == 0)
            {
                return new ScoredPosture(
                    BotStrategicPosture.Recovery,
                    700,
                    "No field army or infrastructure.");
            }

            return new ScoredPosture(
                BotStrategicPosture.Recovery,
                0,
                "No critical loss or stall detected.");
        }

        private bool HasOwnedCastle(
            BotWorldSnapshot snapshot)
        {
            if (snapshot == null)
                return false;

            if (_buildings == null)
                return snapshot.OwnBuildings.Count > 0;

            return HasOwnedCapability(
                snapshot,
                BuildingDefinitionCapabilities.IsCastle);
        }

        private bool HasRecruitmentInfrastructure(
            BotWorldSnapshot snapshot)
            => HasOwnedCapability(
                snapshot,
                definition =>
                    BuildingDefinitionCapabilities
                        .HasEnabledModule<
                            UnitRecruitmentBuildingModule>(
                            definition));

        private bool HasOwnedCapability(
            BotWorldSnapshot snapshot,
            Func<BuildingDefinition, bool> predicate)
        {
            if (_buildings == null ||
                snapshot == null ||
                predicate == null)
            {
                // Snapshot buildings still count as infrastructure in
                // minimal tests, but capability-specific scoring is disabled.
                return false;
            }

            for (int i = 0;
                 i < snapshot.OwnBuildings.Count;
                 i++)
            {
                BuildingDefinition definition =
                    _buildings.GetById(
                        snapshot.OwnBuildings[i].BuildingId);

                if (definition != null &&
                    predicate(definition))
                {
                    return true;
                }
            }

            return false;
        }

        private int CountIndustrialKinds(
            BotWorldSnapshot snapshot)
        {
            if (_buildings == null ||
                snapshot == null)
            {
                return 0;
            }

            var ids =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0;
                 i < snapshot.OwnBuildings.Count;
                 i++)
            {
                BuildingDefinition definition =
                    _buildings.GetById(
                        snapshot.OwnBuildings[i].BuildingId);

                string resourceId =
                    BuildingDefinitionCapabilities
                        .GetIndustrialResourceId(
                            definition);

                if (!string.IsNullOrWhiteSpace(
                        resourceId))
                {
                    ids.Add(resourceId);
                }
            }

            return ids.Count;
        }

        private static bool TryFindCurrentScore(
            IReadOnlyList<ScoredPosture> scored,
            BotStrategicPosture posture,
            out ScoredPosture result)
        {
            for (int i = 0; i < scored.Count; i++)
            {
                if (scored[i].Posture == posture)
                {
                    result = scored[i];
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static int CompareScore(
            ScoredPosture left,
            ScoredPosture right)
        {
            int score =
                right.Score.CompareTo(left.Score);

            return score != 0
                ? score
                : left.Posture.CompareTo(
                    right.Posture);
        }

        private static int ManhattanDistance(
            Vector2Int left,
            Vector2Int right)
            => Mathf.Abs(left.x - right.x) +
               Mathf.Abs(left.y - right.y);

        private static bool HasKnownObjective(
            BotWorldSnapshot snapshot)
        {
            for (int index = 0;
                 index < snapshot.VisibleEnemyBuildings.Count;
                 index++)
            {
                if (IsLikelyCastle(
                        snapshot
                            .VisibleEnemyBuildings[index]
                            .BuildingId))
                {
                    return true;
                }
            }

            for (int index = 0;
                 index < snapshot.Memory.Count;
                 index++)
            {
                BotKnownEntityMemory memory =
                    snapshot.Memory[index];

                if (memory.Kind ==
                        BotKnownEntityKind.Objective ||
                    IsLikelyCastle(memory.TypeId))
                {
                    return true;
                }
            }

            return false;
        }

        // Compatibility heuristic remains confined to enemy objective memory.
        private static bool IsLikelyCastle(
            string id)
            => !string.IsNullOrWhiteSpace(id) &&
               id.IndexOf(
                   "castle",
                   StringComparison.OrdinalIgnoreCase) >= 0;

        private readonly struct ScoredPosture
        {
            public ScoredPosture(
                BotStrategicPosture posture,
                int score,
                string reason)
            {
                Posture = posture;
                Score = score;
                Reason =
                    reason ??
                    string.Empty;
            }

            public BotStrategicPosture Posture { get; }
            public int Score { get; }
            public string Reason { get; }
        }
    }
}
