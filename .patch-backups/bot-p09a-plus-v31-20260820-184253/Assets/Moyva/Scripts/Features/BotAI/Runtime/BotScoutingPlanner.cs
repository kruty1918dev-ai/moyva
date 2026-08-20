using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotScoutingPlanner : IBotScoutingPlanner
    {
        private readonly IUnitMovementQuery _movement;
        private readonly IFogOfWarServiceRegistry _fogRegistry;
        private readonly IGridService _grid;
        private readonly IUnitClassConfig _configs;
        private readonly IBotUnitRoleResolver _roles;
        private readonly IBotInfluenceMapService _influence;
        private readonly BotPlanningProfile _profile;

        [Inject]
        public BotScoutingPlanner(
            [InjectOptional] IUnitMovementQuery movement = null,
            [InjectOptional] IFogOfWarServiceRegistry fogRegistry = null,
            [InjectOptional] IGridService grid = null,
            [InjectOptional] IUnitClassConfig configs = null,
            [InjectOptional] IBotUnitRoleResolver roles = null,
            [InjectOptional] IBotInfluenceMapService influence = null,
            [InjectOptional] BotPlanningProfile profile = null)
        {
            _movement = movement;
            _fogRegistry = fogRegistry;
            _grid = grid;
            _configs = configs;
            _roles = roles;
            _influence = influence;
            _profile = profile ?? BotPlanningProfile.Normal();
        }

        public IReadOnlyList<BotActionCandidate> Generate(BotWorldSnapshot snapshot, BotStrategicContext strategy)
        {
            if (snapshot == null || _movement == null || _grid == null || snapshot.OwnUnits.Count == 0)
                return Array.Empty<BotActionCandidate>();
            if (snapshot.VisibleEnemyUnits.Count > 0 && strategy.Posture != BotStrategicPosture.Search)
                return Array.Empty<BotActionCandidate>();
            if (_fogRegistry == null || !_fogRegistry.TryGetFor(snapshot.OwnerId, out IFogOfWarService fog) || fog == null)
                return Array.Empty<BotActionCandidate>();

            var units = new List<BotUnitSnapshot>(snapshot.OwnUnits);
            units.Sort(CompareScoutPreference);
            var reservedCells = new HashSet<Vector2Int>();
            var reservedSectors = new HashSet<int>();
            var result = new List<BotActionCandidate>();

            int scoutCount = 0;
            for (int u = 0; u < units.Count && scoutCount < _profile.MaxScoutsPerTurn; u++)
            {
                BotUnitSnapshot unit = units[u];
                UnitClassConfig config = BotTacticalAnalysis.SafeGetConfig(_configs, unit.TypeId);
                BotUnitTacticalRole role = _roles?.Resolve(config) ?? BotAdvancedHeuristics.InferRole(config);
                if (role == BotUnitTacticalRole.Worker || role == BotUnitTacticalRole.Siege)
                    continue;

                IReadOnlyList<UnitMovementTileSnapshot> tiles = _movement.GetMovementTiles(unit.UnitId);
                if (tiles == null) continue;

                Vector2Int best = default;
                int bestScore = int.MinValue;
                for (int i = 0; i < tiles.Count && i < _profile.MaxTacticalTilesPerUnit; i++)
                {
                    UnitMovementTileSnapshot tile = tiles[i];
                    if (!tile.IsReachable || tile.Position == unit.Position || reservedCells.Contains(tile.Position))
                        continue;
                    int sector = BotAdvancedHeuristics.StableSectorKey(tile.Position);
                    if (reservedSectors.Contains(sector))
                        continue;

                    int frontier = CountFrontier(fog, tile.Position, 2);
                    if (frontier <= 0)
                        continue;
                    BotInfluenceScore influence = _influence?.Evaluate(snapshot, tile.Position) ?? default;
                    int score = 450 + frontier * _profile.ScoutFrontierWeight - influence.Threat * _profile.ScoutThreatPenaltyPercent / 100;
                    score -= Mathf.RoundToInt(Mathf.Max(0f, tile.Cost) * 5f);
                    if (role == BotUnitTacticalRole.FastScout) score += 250;
                    if (score > bestScore || score == bestScore && BotTurnExecutor.ComparePosition(tile.Position, best) < 0)
                    {
                        best = tile.Position;
                        bestScore = score;
                    }
                }

                if (bestScore == int.MinValue)
                    continue;
                reservedCells.Add(best);
                reservedSectors.Add(BotAdvancedHeuristics.StableSectorKey(best));
                result.Add(new BotActionCandidate(
                    $"scout:frontier:{unit.UnitId}:{best.x},{best.y}",
                    BotActionKind.ScoutMove,
                    BotStrategicPosture.Search,
                    new BotActionScore(bestScore, "Fog-frontier reveal with local sector reservation."),
                    actorId: unit.UnitId,
                    targetCell: best,
                    reason: "frontier-scout"));
                scoutCount++;
            }

            result.Sort((a,b) => b.Score.Total != a.Score.Total ? b.Score.Total.CompareTo(a.Score.Total) : string.CompareOrdinal(a.CandidateId,b.CandidateId));
            return result;
        }

        private int CountFrontier(IFogStateReader fog, Vector2Int center, int radius)
        {
            int count = 0;
            for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                Vector2Int p = center + new Vector2Int(dx, dy);
                if (!_grid.ContainsCell(p)) continue;
                if (!fog.IsExplored(p)) count++;
            }
            return count;
        }

        private int CompareScoutPreference(BotUnitSnapshot left, BotUnitSnapshot right)
        {
            UnitClassConfig lc = BotTacticalAnalysis.SafeGetConfig(_configs, left.TypeId);
            UnitClassConfig rc = BotTacticalAnalysis.SafeGetConfig(_configs, right.TypeId);
            BotUnitTacticalRole lr = _roles?.Resolve(lc) ?? BotAdvancedHeuristics.InferRole(lc);
            BotUnitTacticalRole rr = _roles?.Resolve(rc) ?? BotAdvancedHeuristics.InferRole(rc);
            int lp = lr == BotUnitTacticalRole.FastScout ? 0 : lr == BotUnitTacticalRole.Ranged ? 1 : 2;
            int rp = rr == BotUnitTacticalRole.FastScout ? 0 : rr == BotUnitTacticalRole.Ranged ? 1 : 2;
            int role = lp.CompareTo(rp);
            return role != 0 ? role : string.CompareOrdinal(left.UnitId, right.UnitId);
        }
    }
}
