using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// Stages real gameplay situations through canonical APIs (IUnitFactory).
    /// Staging accelerates setup — it never fabricates impossible state:
    /// spawned units are real units registered in game state.
    /// </summary>
    public sealed class MarketingScenarioDirector
    {
        public sealed class StageResult
        {
            public bool ok;
            public string scenario = string.Empty;
            public List<string> spawnedUnitIds = new List<string>();
            public string message = string.Empty;
        }

        private readonly MarketingGameplayContext _ctx;

        public MarketingScenarioDirector(MarketingGameplayContext ctx)
        {
            _ctx = ctx;
        }

        /// <summary>Apply the recipe's scenario strategy. "auto" stages a
        /// populated settlement/battle when the world is empty and staging is
        /// allowed; otherwise it observes the naturally spawned world.</summary>
        public StageResult Apply(MarketingCaptureRecipe recipe, WorldSubjects world,
            ContentIndexSnapshot index, int seed)
        {
            var result = new StageResult();
            string strategy = string.IsNullOrEmpty(recipe.scenarioStrategy) ? "auto" : recipe.scenarioStrategy;

            bool worldEmpty = world.buildings.Count == 0 && world.units.Count == 0;
            bool wantsStaging = recipe.contentType == MarketingContentType.Trailer
                || recipe.contentType == MarketingContentType.GameplayTrailer
                || recipe.contentType == MarketingContentType.Teaser
                || recipe.contentType == MarketingContentType.SocialVideo
                || recipe.contentType == MarketingContentType.HeroStills
                || recipe.contentType == MarketingContentType.SocialImage
                || worldEmpty;

            if (strategy == "observe" || (!wantsStaging && strategy == "auto"))
            {
                result.ok = true;
                result.scenario = "observe";
                return result;
            }

            if (strategy == "auto" || strategy == "battle" || strategy == "settlement")
            {
                if (!recipe.allowStagingSpawns)
                {
                    result.ok = true;
                    result.scenario = "observe(no-staging)";
                    result.message = "Staging disabled by recipe/compliance.";
                    return result;
                }
                return StageWorld(world, index, seed);
            }

            result.ok = true;
            result.scenario = strategy;
            return result;
        }

        /// <summary>Stage a real playable situation: a small settlement
        /// (canonical IConstructionService.TryDirectPlace) plus two squads
        /// (canonical IUnitFactory). Every spawned object is genuine game
        /// state — no fabricated props.</summary>
        private StageResult StageWorld(WorldSubjects world, ContentIndexSnapshot index, int seed)
        {
            var result = new StageResult { scenario = "staged" };
            var factory = _ctx?.UnitFactory;
            var construction = _ctx?.Construction;
            var projection = _ctx?.GridProjection;
            var grid = _ctx?.Grid;
            if (projection == null || grid == null || (factory == null && construction == null))
            {
                result.ok = false;
                result.message = "UnitFactory/Construction/Grid not available.";
                return result;
            }

            Vector3 anchor = world.buildings.Count > 0
                ? new Vector3(world.buildings[0].worldX, world.buildings[0].worldY, world.buildings[0].worldZ)
                : new Vector3(world.worldCenterX, world.worldCenterY, world.worldCenterZ);
            Vector2Int anchorCell = projection.WorldToGrid(anchor);
            var rng = new Planning.MarketingRng(seed);

            int placedBuildings = 0;
            if (construction != null && world.buildings.Count < 3 && index != null)
                placedBuildings = StageSettlement(construction, grid, anchorCell, index, rng);

            int spawned = 0;
            if (factory != null && world.units.Count < 6)
            {
                string[] squadA = { "spearman", "spearman", "archer" };
                string[] squadB = { "spearman", "archer", "light-cavalry" };
                spawned += SpawnSquad(factory, grid, anchorCell, squadA, new Vector2Int(-4, -3), rng, result);
                spawned += SpawnSquad(factory, grid, anchorCell, squadB, new Vector2Int(4, 3), rng, result);
            }

            result.ok = placedBuildings > 0 || spawned > 0
                || world.buildings.Count > 0 || world.units.Count > 0;
            result.message = $"placed {placedBuildings} buildings, spawned {spawned} units (canonical APIs).";
            return result;
        }

        /// <summary>Place a compact settlement: the SettlementCenter (or top
        /// value) building at the anchor, then a ring of supporting buildings.
        /// Building ids come from the content index — newly merged buildings
        /// are picked up automatically.</summary>
        private int StageSettlement(
            Construction.API.IConstructionService construction,
            IGridService grid, Vector2Int anchor,
            ContentIndexSnapshot index, Planning.MarketingRng rng)
        {
            const string owner = "player_0"; // default local owner id
            var buildings = new List<ContentIndexEntry>();
            foreach (var e in index.entries)
            {
                if (e.category != MarketingContentCategory.Building) continue;
                if (e.marketingDisabled || e.selectionOverride == SelectionOverride.Ban) continue;
                if (e.missingMaterial || e.rendererCount == 0) continue;
                buildings.Add(e);
            }
            if (buildings.Count == 0) return 0;

            buildings.Sort((a, b) => b.marketingValue.CompareTo(a.marketingValue));
            int placed = 0;
            int slot = 0;
            foreach (var b in buildings)
            {
                if (placed >= 5) break;
                // Ring offsets around the anchor.
                var cell = anchor + RingOffset(slot++);
                for (int retry = 0; retry < 6 && !grid.ContainsCell(cell); retry++)
                    cell = anchor + RingOffset(slot++);
                if (!grid.ContainsCell(cell)) continue;
                try
                {
                    if (construction.TryDirectPlace(b.id, cell, owner))
                        placed++;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[MarketingStudio] Staging building '{b.id}' failed: {e.Message}");
                }
            }
            return placed;
        }

        private static Vector2Int RingOffset(int i)
        {
            // Small spiral around the anchor — deterministic, compact.
            var ring = new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(2, 0), new Vector2Int(-2, 0),
                new Vector2Int(0, 2), new Vector2Int(0, -2),
                new Vector2Int(2, 2), new Vector2Int(-2, -2),
                new Vector2Int(2, -2), new Vector2Int(-2, 2),
                new Vector2Int(4, 0), new Vector2Int(-4, 0),
                new Vector2Int(0, 4), new Vector2Int(0, -4),
            };
            return ring[Mathf.Abs(i) % ring.Length];
        }

        private int SpawnSquad(
            IUnitFactory factory,
            IGridService grid,
            Vector2Int anchor, string[] typeIds, Vector2Int offset,
            Planning.MarketingRng rng, StageResult result)
        {
            int count = 0;
            for (int i = 0; i < typeIds.Length; i++)
            {
                var cell = anchor + offset + new Vector2Int(i - 1, rng.Next(-1, 2));
                if (!grid.ContainsCell(cell)) continue;
                if (grid.TryGetTileData(cell, out string tileId) && IsBlocked(tileId))
                    continue;
                try
                {
                    string id = factory.CreateUnit(typeIds[i], cell, null);
                    if (!string.IsNullOrEmpty(id))
                    {
                        result.spawnedUnitIds.Add(id);
                        count++;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[MarketingStudio] Staging spawn '{typeIds[i]}' failed: {e.Message}");
                }
            }
            return count;
        }

        private static bool IsBlocked(string tileId)
        {
            if (string.IsNullOrEmpty(tileId)) return false;
            string t = tileId.ToLowerInvariant();
            return t.Contains("water") || t.Contains("ocean") || t.Contains("mount");
        }
    }
}
