using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ResolvedTileCompositionResolver : IResolvedTileCompositionResolver
    {
        private const string TerrainWinnerReason =
            "Main terrain won by LayerKind/TerrainPriority/CompositionRuleTable/SortingOrder fallback.";

        private readonly ICompositionRuleTable _rules;

        public ResolvedTileCompositionResolver(ICompositionRuleTable rules = null)
        {
            _rules = rules ?? new DefaultCompositionRuleTable();
        }

        public ResolvedTileComposition Resolve(Vector2Int cell, TileNeighborhood neighborhood)
        {
            if (neighborhood.Center == null || neighborhood.Center.IsEmpty)
                return new ResolvedTileComposition(cell, default, default, false, false, "empty stack");

            bool hasMain = TryResolveMainTerrain(neighborhood, out var main);
            bool hasOverlay = TryResolveOverlay(neighborhood, out var overlay);
            string reason = hasMain ? TerrainWinnerReason : "no terrain-like layer in stack";

            return new ResolvedTileComposition(
                cell,
                main,
                overlay,
                hasMain,
                hasOverlay,
                reason,
                MatchesMain(main, neighborhood.North),
                MatchesMain(main, neighborhood.East),
                MatchesMain(main, neighborhood.South),
                MatchesMain(main, neighborhood.West),
                MatchesMain(main, neighborhood.NorthEast),
                MatchesMain(main, neighborhood.SouthEast),
                MatchesMain(main, neighborhood.SouthWest),
                MatchesMain(main, neighborhood.NorthWest));
        }

        private bool TryResolveMainTerrain(TileNeighborhood neighborhood, out GraphTileLayerSample sample)
            => TryResolveMainTerrain(neighborhood.Center, neighborhood, out sample);

        private bool TryResolveMainTerrain(TileStackCell cell, TileNeighborhood neighborhood, out GraphTileLayerSample sample)
        {
            sample = default;
            bool hasSample = false;
            if (cell == null)
                return false;

            for (int i = 0; i < cell.Samples.Count; i++)
            {
                var candidate = cell.Samples[i];
                if (!candidate.IsTerrainLike || candidate.LayerKind == LayerKind.OverlayTerrain)
                    continue;

                if (!hasSample || Compare(sample, candidate, neighborhood) <= 0)
                {
                    sample = candidate;
                    hasSample = true;
                }
            }

            return hasSample;
        }

        private bool MatchesMain(GraphTileLayerSample main, TileStackCell cell)
        {
            if (!TryResolveMainTerrain(cell, new TileNeighborhood(cell, null, null, null, null, null, null, null, null), out var other))
                return false;

            return SameTerrainIdentity(main, other);
        }

        private static bool SameTerrainIdentity(GraphTileLayerSample a, GraphTileLayerSample b)
        {
            if (!string.IsNullOrWhiteSpace(a.BuildLayerGuid) && !string.IsNullOrWhiteSpace(b.BuildLayerGuid))
                return string.Equals(a.BuildLayerGuid, b.BuildLayerGuid, System.StringComparison.Ordinal);
            if (!string.IsNullOrWhiteSpace(a.BlueprintLayerGuid) && !string.IsNullOrWhiteSpace(b.BlueprintLayerGuid))
                return string.Equals(a.BlueprintLayerGuid, b.BlueprintLayerGuid, System.StringComparison.Ordinal);
            if (!string.IsNullOrWhiteSpace(a.GraphLayerId) && !string.IsNullOrWhiteSpace(b.GraphLayerId))
                return string.Equals(a.GraphLayerId, b.GraphLayerId, System.StringComparison.Ordinal);

            return string.Equals(a.TileId, b.TileId, System.StringComparison.Ordinal);
        }

        private bool TryResolveOverlay(TileNeighborhood neighborhood, out GraphTileLayerSample sample)
        {
            sample = default;
            bool hasSample = false;
            TileStackCell cell = neighborhood.Center;

            for (int i = 0; i < cell.Samples.Count; i++)
            {
                var candidate = cell.Samples[i];
                if (candidate.LayerKind != LayerKind.OverlayTerrain && candidate.LayerKind != LayerKind.Decoration)
                    continue;

                if (!hasSample || Compare(sample, candidate, neighborhood) <= 0)
                {
                    sample = candidate;
                    hasSample = true;
                }
            }

            return hasSample;
        }

        private int Compare(GraphTileLayerSample current, GraphTileLayerSample candidate, TileNeighborhood neighborhood)
        {
            int result = current.LayerKindRank.CompareTo(candidate.LayerKindRank);
            if (result != 0)
                return result;

            result = current.TerrainPriority.CompareTo(candidate.TerrainPriority);
            if (result != 0)
                return result;

            if (_rules.TryCompare(current, candidate, neighborhood, out result, out _) && result != 0)
                return result;

            result = current.SortingOrder.CompareTo(candidate.SortingOrder);
            if (result != 0)
                return result;

            result = current.GraphLayerOrder.CompareTo(candidate.GraphLayerOrder);
            if (result != 0)
                return result;

            return string.Compare(current.StableTieBreakKey, candidate.StableTieBreakKey, System.StringComparison.Ordinal);
        }
    }
}
