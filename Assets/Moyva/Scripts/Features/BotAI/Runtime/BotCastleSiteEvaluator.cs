using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotCastleSiteEvaluator : IBotCastleSiteEvaluator
    {
        private readonly IBotTerrainKnowledge _terrain;

        [Inject]
        public BotCastleSiteEvaluator(
            [InjectOptional] IBotTerrainKnowledge terrain = null)
        {
            _terrain = terrain;
        }

        public BotSiteEvaluation Evaluate(
            BotWorldSnapshot snapshot,
            string castleBuildingId,
            Vector2Int cell,
            bool placementAllowed)
        {
            var factors = new List<BotSiteScoreFactor>();

            if (!placementAllowed)
            {
                factors.Add(Factor(
                    "placement",
                    "Правила будівництва",
                    0,
                    1,
                    -100000,
                    "Canonical placement query забороняє будівництво на цій клітинці."));

                return new BotSiteEvaluation(
                    cell,
                    false,
                    -100000,
                    "Клітинка відкинута правилами будівництва.",
                    factors);
            }

            Add(factors, "placement", "Правила будівництва", 1f, 250, 250,
                "Клітинка пройшла canonical IConstructionPlacementQuery.");

            Vector2Int start = snapshot?.StartPosition ?? cell;
            int startDistance = Manhattan(start, cell);
            int proximity = Mathf.Clamp(260 - startDistance * 18, -220, 260);
            Add(factors, "start-distance", "Відстань від старту", startDistance, -18, proximity,
                $"Манхеттенська відстань від стартової позиції: {startDistance}.");

            if (_terrain == null || !_terrain.IsReady ||
                !_terrain.TryGetCell(cell, out BotTerrainCellSnapshot center))
            {
                Add(factors, "terrain-unavailable", "Дані рельєфу", 0, 0, -40,
                    "Повний terrain snapshot ще не готовий; використовується безпечний штраф за невизначеність.");

                int fallback = Sum(factors);
                return new BotSiteEvaluation(
                    cell,
                    true,
                    fallback,
                    "Місце допустиме, але повні дані рельєфу ще не доступні.",
                    factors);
            }

            int elevation = center.TerrainLevel * 36 + Mathf.RoundToInt(center.Height * 8f);
            Add(factors, "elevation", "Висота позиції",
                center.TerrainLevel + center.Height, 36, elevation,
                $"TerrainLevel={center.TerrainLevel}, Height={center.Height:0.##}. Вища позиція покращує оборону й огляд.");

            IReadOnlyList<Vector2Int> near = _terrain.GetNeighbors(cell, 1);
            int lower = 0;
            int higher = 0;
            int blocked = 0;
            int open = 0;

            for (int i = 0; i < near.Count; i++)
            {
                if (!_terrain.TryGetCell(near[i], out BotTerrainCellSnapshot n))
                    continue;

                if (n.TerrainLevel < center.TerrainLevel || n.Height + 0.25f < center.Height)
                    lower++;
                else if (n.TerrainLevel > center.TerrainLevel || n.Height > center.Height + 0.25f)
                    higher++;

                if (IsNaturalBarrier(n))
                    blocked++;
                else
                    open++;
            }

            int dominance = lower * 32 - higher * 42;
            Add(factors, "high-ground", "Домінування висоти", lower - higher, 32, dominance,
                $"Нижчих сусідніх клітин: {lower}; вищих: {higher}. Вищі сусіди створюють ризик для лучників противника.");

            int barrierContribution = blocked <= 3
                ? blocked * 45
                : 135 - (blocked - 3) * 35;
            Add(factors, "natural-barriers", "Природні бар'єри", blocked, 45, barrierContribution,
                $"Поруч {blocked} водних/гірських/скельних бар'єрів. Невелика кількість звужує підходи, надмірна — заважає розвитку.");

            int localOpenContribution = Mathf.Clamp((open - 3) * 22, -100, 110);
            Add(factors, "local-expansion", "Локальне місце для забудови", open, 22, localOpenContribution,
                $"Вільних сусідніх напрямків: {open}. Замку потрібна оборона без блокування майбутнього міста.");

            IReadOnlyList<Vector2Int> cityRadius = _terrain.GetNeighbors(cell, 3);
            int buildableLike = 0;
            int waterLike = 0;
            int higherThreatCells = 0;
            for (int i = 0; i < cityRadius.Count; i++)
            {
                if (!_terrain.TryGetCell(cityRadius[i], out BotTerrainCellSnapshot n))
                    continue;

                if (IsWater(n))
                    waterLike++;

                if (!IsNaturalBarrier(n) && !n.HasObject)
                    buildableLike++;

                int dist = Manhattan(cell, n.Cell);
                if (dist <= 3 &&
                    (n.TerrainLevel > center.TerrainLevel ||
                     n.Height > center.Height + 0.65f))
                {
                    higherThreatCells++;
                }
            }

            int cityContribution = Mathf.Clamp(buildableLike * 7, 0, 280);
            Add(factors, "city-capacity", "Потенціал міста", buildableLike, 7, cityContribution,
                $"У радіусі 3 знайдено {buildableLike} відкритих клітин без природного бар'єра/об'єкта.");

            int archerExposure = -Mathf.Clamp(higherThreatCells * 28, 0, 280);
            Add(factors, "archer-exposure", "Ризик вогню з висоти", higherThreatCells, -28, archerExposure,
                $"У радіусі 3 є {higherThreatCells} позицій вище замку, потенційно небезпечних для дальнього бою.");

            int cavalryLanes = CountFlatApproachLanes(cell, center);
            int cavalryPenalty = -cavalryLanes * 24;
            Add(factors, "cavalry-lanes", "Прямі підходи кавалерії", cavalryLanes, -24, cavalryPenalty,
                $"Виявлено {cavalryLanes} довгих відносно рівних відкритих напрямків підходу.");

            int edgeDistance = DistanceToEdge(cell);
            int edgeContribution = edgeDistance switch
            {
                <= 0 => -180,
                1 => -120,
                2 => -55,
                >= 5 => 45,
                _ => 0,
            };
            Add(factors, "map-edge", "Відстань від краю мапи", edgeDistance, 1, edgeContribution,
                $"Відстань до найближчого краю карти: {edgeDistance}. Надто близький край обмежує розвиток.");

            int visibleThreatDistance = ClosestVisibleThreatDistance(snapshot, cell);
            int threatContribution = visibleThreatDistance switch
            {
                < 0 => 30,
                <= 3 => -320,
                <= 5 => -220,
                <= 8 => -110,
                <= 12 => -35,
                _ => 45,
            };
            Add(factors, "visible-threat-distance", "Відстань до видимої загрози",
                visibleThreatDistance, 1, threatContribution,
                visibleThreatDistance < 0
                    ? "Видимих ворожих цілей немає."
                    : $"Найближча видима ворожа ціль на відстані {visibleThreatDistance}.");

            int waterBalance = waterLike <= 8 ? waterLike * 4 : 32 - (waterLike - 8) * 5;
            Add(factors, "water-balance", "Вода поблизу", waterLike, 4, waterBalance,
                $"У радіусі 3 визначено {waterLike} водних клітин: це може захищати фланг, але не повинно душити забудову.");

            int total = Sum(factors);
            string summary =
                $"Оцінка {total}. Висота={center.TerrainLevel}/{center.Height:0.##}, " +
                $"відкритий простір={buildableLike}, природні бар'єри={blocked}, " +
                $"ризик вищих позицій={higherThreatCells}, кавалерійські коридори={cavalryLanes}.";

            return new BotSiteEvaluation(cell, true, total, summary, factors);
        }

        private int CountFlatApproachLanes(
            Vector2Int origin,
            BotTerrainCellSnapshot center)
        {
            if (_terrain == null || !_terrain.IsReady)
                return 0;

            Vector2Int[] directions =
            {
                Vector2Int.right,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.down,
            };

            int lanes = 0;
            for (int d = 0; d < directions.Length; d++)
            {
                int openRun = 0;
                for (int step = 1; step <= 4; step++)
                {
                    Vector2Int cell = origin + directions[d] * step;
                    if (!_terrain.TryGetCell(cell, out BotTerrainCellSnapshot sample))
                        break;
                    if (IsNaturalBarrier(sample) || sample.HasObject)
                        break;

                    float heightDelta = Mathf.Abs(sample.Height - center.Height);
                    int levelDelta = Math.Abs(sample.TerrainLevel - center.TerrainLevel);
                    if (heightDelta > 0.75f || levelDelta > 1)
                        break;

                    openRun++;
                }

                if (openRun >= 3)
                    lanes++;
            }

            return lanes;
        }

        private int DistanceToEdge(Vector2Int cell)
        {
            if (_terrain == null || !_terrain.IsReady)
                return 0;

            return Math.Max(
                0,
                Math.Min(
                    Math.Min(cell.x, cell.y),
                    Math.Min(_terrain.Width - 1 - cell.x, _terrain.Height - 1 - cell.y)));
        }

        private static int ClosestVisibleThreatDistance(
            BotWorldSnapshot snapshot,
            Vector2Int cell)
        {
            if (snapshot == null)
                return -1;

            int best = int.MaxValue;

            for (int i = 0; i < snapshot.VisibleEnemyUnits.Count; i++)
                best = Math.Min(best, Manhattan(cell, snapshot.VisibleEnemyUnits[i].Position));

            for (int i = 0; i < snapshot.VisibleEnemyBuildings.Count; i++)
                best = Math.Min(best, Manhattan(cell, snapshot.VisibleEnemyBuildings[i].Position));

            return best == int.MaxValue ? -1 : best;
        }

        private static bool IsNaturalBarrier(BotTerrainCellSnapshot cell)
            => IsWater(cell) ||
               ContainsAny(cell.TileId, "mountain", "cliff", "rock") ||
               ContainsAny(cell.ObjectId, "mountain", "cliff", "rock");

        private static bool IsWater(BotTerrainCellSnapshot cell)
            => ContainsAny(cell.TileId, "water", "river", "lake", "sea", "ocean", "swamp") ||
               ContainsAny(cell.ObjectId, "water", "river", "lake", "sea", "ocean");

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            for (int i = 0; i < tokens.Length; i++)
            {
                if (value.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static int Manhattan(Vector2Int a, Vector2Int b)
            => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        private static void Add(
            List<BotSiteScoreFactor> factors,
            string key,
            string label,
            float raw,
            int weight,
            int contribution,
            string detail)
            => factors.Add(Factor(key, label, raw, weight, contribution, detail));

        private static BotSiteScoreFactor Factor(
            string key,
            string label,
            float raw,
            int weight,
            int contribution,
            string detail)
            => new(key, label, raw, weight, contribution, detail);

        private static int Sum(IReadOnlyList<BotSiteScoreFactor> factors)
        {
            long total = 0;
            for (int i = 0; i < factors.Count; i++)
                total += factors[i].Contribution;

            return total > int.MaxValue
                ? int.MaxValue
                : total < int.MinValue
                    ? int.MinValue
                    : (int)total;
        }
    }
}
