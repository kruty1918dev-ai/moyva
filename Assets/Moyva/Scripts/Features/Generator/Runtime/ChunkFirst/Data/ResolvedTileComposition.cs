using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal readonly struct ResolvedTileComposition
    {
        public ResolvedTileComposition(
            Vector2Int cell,
            GraphTileLayerSample mainTerrain,
            GraphTileLayerSample overlay,
            bool hasMainTerrain,
            bool hasOverlay,
            string reason,
            bool northMatches = false,
            bool eastMatches = false,
            bool southMatches = false,
            bool westMatches = false,
            bool northEastMatches = false,
            bool southEastMatches = false,
            bool southWestMatches = false,
            bool northWestMatches = false,
            float supportHeight = float.NaN,
            float northSurfaceHeight = float.NaN,
            float eastSurfaceHeight = float.NaN,
            float southSurfaceHeight = float.NaN,
            float westSurfaceHeight = float.NaN,
            float northEastSurfaceHeight = float.NaN,
            float southEastSurfaceHeight = float.NaN,
            float southWestSurfaceHeight = float.NaN,
            float northWestSurfaceHeight = float.NaN)
        {
            Cell = cell;
            MainTerrain = mainTerrain;
            Overlay = overlay;
            HasMainTerrain = hasMainTerrain;
            HasOverlay = hasOverlay;
            Reason = reason;
            NorthMatches = northMatches;
            EastMatches = eastMatches;
            SouthMatches = southMatches;
            WestMatches = westMatches;
            NorthEastMatches = northEastMatches;
            SouthEastMatches = southEastMatches;
            SouthWestMatches = southWestMatches;
            NorthWestMatches = northWestMatches;
            SupportHeight = supportHeight;
            NorthSurfaceHeight = northSurfaceHeight;
            EastSurfaceHeight = eastSurfaceHeight;
            SouthSurfaceHeight = southSurfaceHeight;
            WestSurfaceHeight = westSurfaceHeight;
            NorthEastSurfaceHeight = northEastSurfaceHeight;
            SouthEastSurfaceHeight = southEastSurfaceHeight;
            SouthWestSurfaceHeight = southWestSurfaceHeight;
            NorthWestSurfaceHeight = northWestSurfaceHeight;
        }

        public Vector2Int Cell { get; }
        public GraphTileLayerSample MainTerrain { get; }
        public GraphTileLayerSample Overlay { get; }
        public bool HasMainTerrain { get; }
        public bool HasOverlay { get; }
        public string Reason { get; }
        public bool NorthMatches { get; }
        public bool EastMatches { get; }
        public bool SouthMatches { get; }
        public bool WestMatches { get; }
        public bool NorthEastMatches { get; }
        public bool SouthEastMatches { get; }
        public bool SouthWestMatches { get; }
        public bool NorthWestMatches { get; }
        public float SupportHeight { get; }
        public bool HasSupportHeight => !float.IsNaN(SupportHeight);
        public float NorthSurfaceHeight { get; }
        public float EastSurfaceHeight { get; }
        public float SouthSurfaceHeight { get; }
        public float WestSurfaceHeight { get; }
        public float NorthEastSurfaceHeight { get; }
        public float SouthEastSurfaceHeight { get; }
        public float SouthWestSurfaceHeight { get; }
        public float NorthWestSurfaceHeight { get; }
    }
}
