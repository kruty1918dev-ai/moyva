using System;
using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal readonly struct TileLayerSample
    {
        public TileLayerSample(
            string layerId,
            string layerName,
            string blueprintLayerGuid,
            string buildLayerGuid,
            string tileId,
            string presetId,
            LayerKind layerKind,
            int sortingOrder,
            int layerOrder,
            int terrainPriority,
            float height,
            float surfaceHeight,
            string sourceLayerId,
            TileGeometryMode tileGeometryMode = TileGeometryMode.SolidTerrain,
            AuthoredClosurePolicy authoredClosurePolicy = AuthoredClosurePolicy.PreserveAuthored)
        {
            LayerId = layerId;
            LayerName = layerName;
            BlueprintLayerGuid = blueprintLayerGuid;
            BuildLayerGuid = buildLayerGuid;
            TileId = tileId;
            PresetId = presetId;
            LayerKind = layerKind;
            SortingOrder = sortingOrder;
            LayerOrder = layerOrder;
            TerrainPriority = terrainPriority;
            Height = height;
            SurfaceHeight = surfaceHeight;
            SourceLayerId = sourceLayerId;
            TileGeometryMode = tileGeometryMode;
            AuthoredClosurePolicy = authoredClosurePolicy;
        }

        public string LayerId { get; }
        public string LayerName { get; }
        public string BlueprintLayerGuid { get; }
        public string BuildLayerGuid { get; }
        public string TileId { get; }
        public string PresetId { get; }
        public LayerKind LayerKind { get; }
        public int SortingOrder { get; }
        public int LayerOrder { get; }
        public int TerrainPriority { get; }
        public float Height { get; }
        public float SurfaceHeight { get; }
        public string SourceLayerId { get; }
        public TileGeometryMode TileGeometryMode { get; }
        public AuthoredClosurePolicy AuthoredClosurePolicy { get; }

        /// <summary>Copies the sample with a new base height and surface height.</summary>
        public TileLayerSample WithHeights(float height, float surfaceHeight)
        {
            return new TileLayerSample(
                LayerId,
                LayerName,
                BlueprintLayerGuid,
                BuildLayerGuid,
                TileId,
                PresetId,
                LayerKind,
                SortingOrder,
                LayerOrder,
                TerrainPriority,
                height,
                surfaceHeight,
                SourceLayerId,
                TileGeometryMode,
                AuthoredClosurePolicy);
        }

        /// <summary>Copies the sample with the relief surface applied as both heights.</summary>
        public TileLayerSample WithSurfaceHeight(float surfaceHeight)
            => WithHeights(surfaceHeight, surfaceHeight);

        public string StableTieBreakKey
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BuildLayerGuid))
                    return BuildLayerGuid;
                if (!string.IsNullOrWhiteSpace(BlueprintLayerGuid))
                    return BlueprintLayerGuid;
                return LayerId ?? string.Empty;
            }
        }

        public bool IsTerrainLike =>
            LayerKind == LayerKind.BaseTerrain
            || LayerKind == LayerKind.OverlayTerrain
            || LayerKind == LayerKind.Road
            || LayerKind == LayerKind.Shore
            || LayerKind == LayerKind.Cliff;
        public int LayerKindRank => GetKindRank(LayerKind);

        public int CompareTo(TileLayerSample other)
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

            result = LayerOrder.CompareTo(other.LayerOrder);
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
            TileLayerSample current,
            TileLayerSample candidate)
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
            TileLayerSample sample)
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
