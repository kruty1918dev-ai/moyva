using System;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal readonly struct GraphTileLayerSample
    {
        public GraphTileLayerSample(
            string graphLayerId,
            string graphLayerName,
            string blueprintLayerGuid,
            string buildLayerGuid,
            string tileId,
            string presetId,
            LayerKind layerKind,
            int sortingOrder,
            int graphLayerOrder,
            int terrainPriority,
            float height,
            float surfaceHeight,
            string sourceNodeId,
            TileGeometryMode tileGeometryMode = TileGeometryMode.SolidTerrain,
            AuthoredClosurePolicy authoredClosurePolicy = AuthoredClosurePolicy.PreserveAuthored)
        {
            GraphLayerId = graphLayerId;
            GraphLayerName = graphLayerName;
            BlueprintLayerGuid = blueprintLayerGuid;
            BuildLayerGuid = buildLayerGuid;
            TileId = tileId;
            PresetId = presetId;
            LayerKind = layerKind;
            SortingOrder = sortingOrder;
            GraphLayerOrder = graphLayerOrder;
            TerrainPriority = terrainPriority;
            Height = height;
            SurfaceHeight = surfaceHeight;
            SourceNodeId = sourceNodeId;
            TileGeometryMode = tileGeometryMode;
            AuthoredClosurePolicy = authoredClosurePolicy;
        }

        public string GraphLayerId { get; }
        public string GraphLayerName { get; }
        public string BlueprintLayerGuid { get; }
        public string BuildLayerGuid { get; }
        public string TileId { get; }
        public string PresetId { get; }
        public LayerKind LayerKind { get; }
        public int SortingOrder { get; }
        public int GraphLayerOrder { get; }
        public int TerrainPriority { get; }
        public float Height { get; }
        public float SurfaceHeight { get; }
        public string SourceNodeId { get; }
        public TileGeometryMode TileGeometryMode { get; }
        public AuthoredClosurePolicy AuthoredClosurePolicy { get; }

        public string StableTieBreakKey
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BuildLayerGuid))
                    return BuildLayerGuid;
                if (!string.IsNullOrWhiteSpace(BlueprintLayerGuid))
                    return BlueprintLayerGuid;
                return GraphLayerId ?? string.Empty;
            }
        }

        public bool IsTerrainLike =>
            LayerKind == LayerKind.BaseTerrain
            || LayerKind == LayerKind.OverlayTerrain
            || LayerKind == LayerKind.Road
            || LayerKind == LayerKind.Shore
            || LayerKind == LayerKind.Cliff;
        public int LayerKindRank => GetKindRank(LayerKind);

        public int CompareTo(GraphTileLayerSample other)
        {
            int result = CompareVisualElevation(this, other);
            if (result != 0)
                return result;

            result = LayerKindRank.CompareTo(other.LayerKindRank);
            if (result != 0)
                return result;

            result = TerrainPriority.CompareTo(other.TerrainPriority);
            if (result != 0)
                return result;

            result = SortingOrder.CompareTo(other.SortingOrder);
            if (result != 0)
                return result;

            result = GraphLayerOrder.CompareTo(other.GraphLayerOrder);
            if (result != 0)
                return result;

            return string.Compare(StableTieBreakKey, other.StableTieBreakKey, StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares the authored surface that is actually rendered. This is shared
        /// by the resolved visual composition and the compatibility projection so
        /// terrain queries can never publish a lower, hidden layer at an overlap.
        /// </summary>
        public static int CompareVisualElevation(
            GraphTileLayerSample current,
            GraphTileLayerSample candidate)
        {
            const float epsilon = 0.0001f;

            // SurfaceHeight is the authoritative rendered top. Height is only
            // the layer/root base and may legitimately sit above the mesh when
            // an authored prefab has a negative top offset. Ranking by max(base,
            // surface) would make prefab pivots change the overlap winner.
            float currentTop = ResolveVisualSurface(current);
            float candidateTop = ResolveVisualSurface(candidate);
            float topDelta = currentTop - candidateTop;
            if (Math.Abs(topDelta) > epsilon)
                return topDelta < 0f ? -1 : 1;

            return 0;
        }

        private static float ResolveVisualSurface(
            GraphTileLayerSample sample)
        {
            if (!float.IsNaN(sample.SurfaceHeight)
                && !float.IsInfinity(sample.SurfaceHeight))
            {
                return sample.SurfaceHeight;
            }

            return !float.IsNaN(sample.Height)
                   && !float.IsInfinity(sample.Height)
                ? sample.Height
                : float.NegativeInfinity;
        }

        private static int GetKindRank(LayerKind kind)
        {
            return kind switch
            {
                LayerKind.BaseTerrain => 800,
                LayerKind.Shore => 700,
                LayerKind.Road => 650,
                LayerKind.Cliff => 600,
                LayerKind.OverlayTerrain => 500,
                LayerKind.Building => 300,
                LayerKind.ObjectSpawn => 200,
                LayerKind.Decoration => 100,
                _ => 0
            };
        }
    }
}
