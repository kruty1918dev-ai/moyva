using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    [CreateAssetMenu(menuName = "Moyva/FogOfWarSettings", fileName = "FogOfWarSettings")]
    public class FogOfWarSettings : ScriptableObject
    {
        [Header("Vision Range")]
        public int DefaultVisionRange = 5;
        public int MinVisionRange = 1;
        public int MaxVisionRange = 12;

        [Header("Height Vision")]
        [Min(0.01f)] public float ElevationStep = 0.15f;
        [Min(0)] public int ObserverHeightBonusPerStep = 1;
        [Min(0)] public int DownhillVisionBonusPerStep = 1;
        [Min(0)] public int UphillVisionPenaltyPerStep = 1;
        [Min(0)] public int MaxObserverHeightBonus = 4;
        [Min(0)] public int MaxDownhillVisionBonus = 2;
        [Min(0)] public int MaxUphillVisionPenalty = 6;
        [Min(0f)] public float OcclusionSlopeBias = 0.02f;

        [Header("Fog Colors")]
        public Color UnexploredColor = new Color(0f, 0f, 0f, 1f);
        public Color ExploredColor   = new Color(0f, 0f, 0f, 0.5f);

        [Header("Fog Tile Sprite")]
        public Sprite FogTileSprite;
        [Tooltip("Sprite sample size in pixels, measured from the sprite rect origin inside the texture atlas.")]
        public Vector2Int FogTileSpritePixelSize = new Vector2Int(16, 16);
        [Tooltip("Visual footprint of one fog sprite tile in map cells. The fog grid does not change: every map cell still draws one sprite, but values above 1 let that sprite overlap neighboring cells.")]
        public Vector2 FogTileSizeInCells = Vector2.one;
        [Tooltip("Extra overlap on each fog tile edge in sprite pixels. Helps hide zoom/filtering seams without increasing opacity on overlaps.")]
        [Min(0f)] public float FogTileSeamOverlapPixels = 1f;
        [Tooltip("Extra fog overlay geometry past the map border, measured in sprite pixels. This hides zoom/rasterization gaps on the outer map edge while fog UVs stay aligned to the real map.")]
        [Min(0f)] public float FogMapEdgePaddingPixels = 2f;
        [Tooltip("Scale for tiling the fog cell texture. Higher = more tiles per cell")]
        [Min(1f)] public float FogTileTiling = 1f;

        [Header("Fog Icons")]
        [Tooltip("Array of icon sprites to cycle through fog cells in regular pattern")]
        public Sprite[] FogIconSprites;
        [Tooltip("Icon sprite sample size in pixels, measured from the icon sprite rect origin inside the texture atlas.")]
        public Vector2Int FogIconSpritePixelSize = new Vector2Int(16, 16);
        [Tooltip("Independent icon grid over map (X columns, Y rows). Icons are placed sequentially by this grid, not by tile cells")]
        public Vector2Int FogIconGridSize = new Vector2Int(10, 10);
        [Tooltip("Icon scale relative to cell size")]
        [Min(0.1f)] public float FogIconScale = 0.5f;
        [Tooltip("If enabled, place icon at cell center; if disabled, distribute across cell")]
        public bool CenterIcon = true;

        [Header("Transparency Blending")]
        [Tooltip("Alpha for fully unexplored fog")]
        [Range(0f, 1f)] public float UnexploredAlpha = 1f;
        [Tooltip("Alpha for explored fog")]
        [Range(0f, 1f)] public float ExploredAlpha = 0.5f;
        [Tooltip("Visible areas are fully transparent (alpha=0)")]
        public bool FullyTransparentWhenVisible = true;

        [Header("Visibility Culling")]
        [Tooltip("Disables world renderers that are fully covered by unexplored fog.")]
        public bool EnableRendererCulling = true;
        [Tooltip("If enabled, renderer culling works only when UnexploredAlpha is close to opaque (>= 0.99).")]
        public bool RequireOpaqueUnexploredForCulling = true;
        [Tooltip("Layers affected by fog renderer culling.")]
        public LayerMask RendererCullingLayerMask = ~0;
        [Tooltip("Lets custom 2D shaders clip pixels hidden by fully unexplored fog.")]
        public bool EnableShaderFogCulling = true;
        [Tooltip("Maximum tracked renderers evaluated per frame. Lower values spread work across more frames.")]
        [Min(1)] public int RendererCullingMaxRenderersPerFrame = 384;
        [Tooltip("How often the culling service searches for newly spawned world renderers.")]
        [Min(0.05f)] public float RendererCullingDiscoveryInterval = 0.75f;
        [Tooltip("Small bounds padding in map cells to avoid edge flicker when sprites move between cells.")]
        [Min(0f)] public float RendererCullingBoundsPaddingCells = 0f;
        [Tooltip("Fog texture value below this threshold is treated as fully hidden by shaders.")]
        [Range(0f, 0.25f)] public float ShaderFogCullThreshold = 0.01f;

        private void OnValidate()
        {
            FogTileSpritePixelSize = ClampSpritePixelSize(FogTileSpritePixelSize);
            FogTileSizeInCells = ClampTileSizeInCells(FogTileSizeInCells);
            FogTileSeamOverlapPixels = Mathf.Max(0f, FogTileSeamOverlapPixels);
            FogMapEdgePaddingPixels = Mathf.Max(0f, FogMapEdgePaddingPixels);
            FogIconSpritePixelSize = ClampSpritePixelSize(FogIconSpritePixelSize);
            RendererCullingMaxRenderersPerFrame = Mathf.Max(1, RendererCullingMaxRenderersPerFrame);
            RendererCullingDiscoveryInterval = Mathf.Max(0.05f, RendererCullingDiscoveryInterval);
            RendererCullingBoundsPaddingCells = Mathf.Max(0f, RendererCullingBoundsPaddingCells);
            ShaderFogCullThreshold = Mathf.Clamp(ShaderFogCullThreshold, 0f, 0.25f);
        }

        private static Vector2Int ClampSpritePixelSize(Vector2Int size)
        {
            return new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
        }

        private static Vector2 ClampTileSizeInCells(Vector2 size)
        {
            return new Vector2(Mathf.Max(0.001f, size.x), Mathf.Max(0.001f, size.y));
        }
    }
}
