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

        [Header("Terrain Viewshed")]
        [Tooltip("Maximum ray samples per target tile. 1 = center only, 5 = center + corners, 9 = center + corners + edge midpoints.")]
        [Range(1, 9)] public int TerrainRaySamplesPerTile = 5;
        [Tooltip("Minimum visibility coefficient required for a tile to count as visible in the boolean fog map.")]
        [Range(0.01f, 1f)] public float TerrainVisibilityThreshold = 0.5f;
        [Tooltip("Multiplier applied to partially visible targets. Use values below 1 to make detection harder through partial cover.")]
        [Range(0f, 1f)] public float PartialVisibilityDetectionMultiplier = 1f;
        [Tooltip("Distance in tiles between height samples along each ray. Lower values are more precise but costlier.")]
        [Range(0.25f, 1f)] public float TerrainRayStepTiles = 0.5f;
        [Tooltip("Additional eye height above the observer terrain height.")]
        [Min(0f)] public float ObserverEyeHeightOffset = 0.35f;
        [Tooltip("Sample height above the target tile terrain. Higher values make figures on edges easier to see.")]
        [Min(0f)] public float TargetSampleHeightOffset = 0.1f;
        [Tooltip("When distance reaches this fraction of the current search radius, far targets use fewer ray samples.")]
        [Range(0.1f, 1f)] public float TerrainFarSampleDistanceRatio = 0.65f;
        [Tooltip("Maximum cached terrain visibility ray results. Cache is cleared when the heightmap changes or settings change.")]
        [Min(0)] public int TerrainVisibilityCacheCapacity = 24576;

        [Header("Terrain Edge Vision")]
        [Tooltip("Makes cliffs and steep terrain edges create a short blind zone behind the edge unless the observer stands close to it.")]
        public bool EnableTerrainEdgeLineOfSight = true;
        [Tooltip("Minimum height drop between neighbouring tiles that is treated as a terrain edge.")]
        [Min(0.001f)] public float TerrainEdgeHeightThreshold = 0.12f;
        [Tooltip("How many tiles from a steep edge still count as standing at the edge.")]
        [Min(0)] public int TerrainEdgePeekDistanceTiles = 1;
        [Tooltip("How many low-ground tiles immediately behind a steep edge are hidden from an observer who is not near the edge.")]
        [Min(0)] public int TerrainEdgeBlindZoneTiles = 2;
        [Tooltip("How much the blind zone grows as the observer stands farther from the edge.")]
        [Min(0f)] public float TerrainEdgeBlindZoneDistanceScale = 0.35f;
        [Tooltip("Maximum blind-zone depth caused by distance from the edge.")]
        [Min(0)] public int TerrainEdgeMaxBlindZoneTiles = 4;
        [Tooltip("Reduces uphill penalties when the target is standing on the upper edge facing the observer.")]
        [Range(0f, 1f)] public float TerrainEdgeUphillPeekStrength = 0.65f;

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
        [Tooltip("Extra fog overlay contour past the map border, measured in map cells. This expands only the overlay geometry and does not change fog cell size or fog texture resolution.")]
        [Min(0f)] public float FogMapEdgeOverhangCells = 0.5f;
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

        [Header("3D Fog Plane")]
        [Tooltip("Small gap above the highest generated terrain point where the top fog plane is placed.")]
        [Min(0f)] public float Fog3DTopClearance = 0.08f;

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
            ElevationStep = Mathf.Max(0.01f, ElevationStep);
            ObserverHeightBonusPerStep = Mathf.Max(0, ObserverHeightBonusPerStep);
            DownhillVisionBonusPerStep = Mathf.Max(0, DownhillVisionBonusPerStep);
            UphillVisionPenaltyPerStep = Mathf.Max(0, UphillVisionPenaltyPerStep);
            MaxObserverHeightBonus = Mathf.Max(0, MaxObserverHeightBonus);
            MaxDownhillVisionBonus = Mathf.Max(0, MaxDownhillVisionBonus);
            MaxUphillVisionPenalty = Mathf.Max(0, MaxUphillVisionPenalty);
            OcclusionSlopeBias = Mathf.Max(0f, OcclusionSlopeBias);
            TerrainRaySamplesPerTile = Mathf.Clamp(TerrainRaySamplesPerTile, 1, 9);
            TerrainVisibilityThreshold = Mathf.Clamp(TerrainVisibilityThreshold, 0.01f, 1f);
            PartialVisibilityDetectionMultiplier = Mathf.Clamp01(PartialVisibilityDetectionMultiplier);
            TerrainRayStepTiles = Mathf.Clamp(TerrainRayStepTiles, 0.25f, 1f);
            ObserverEyeHeightOffset = Mathf.Max(0f, ObserverEyeHeightOffset);
            TargetSampleHeightOffset = Mathf.Max(0f, TargetSampleHeightOffset);
            TerrainFarSampleDistanceRatio = Mathf.Clamp(TerrainFarSampleDistanceRatio, 0.1f, 1f);
            TerrainVisibilityCacheCapacity = Mathf.Max(0, TerrainVisibilityCacheCapacity);
            TerrainEdgeHeightThreshold = Mathf.Max(0.001f, TerrainEdgeHeightThreshold);
            TerrainEdgePeekDistanceTiles = Mathf.Max(0, TerrainEdgePeekDistanceTiles);
            TerrainEdgeBlindZoneTiles = Mathf.Max(0, TerrainEdgeBlindZoneTiles);
            TerrainEdgeBlindZoneDistanceScale = Mathf.Max(0f, TerrainEdgeBlindZoneDistanceScale);
            TerrainEdgeMaxBlindZoneTiles = Mathf.Max(TerrainEdgeBlindZoneTiles, TerrainEdgeMaxBlindZoneTiles);
            TerrainEdgeUphillPeekStrength = Mathf.Clamp01(TerrainEdgeUphillPeekStrength);
            FogTileSpritePixelSize = ClampSpritePixelSize(FogTileSpritePixelSize);
            FogTileSizeInCells = ClampTileSizeInCells(FogTileSizeInCells);
            FogTileSeamOverlapPixels = Mathf.Max(0f, FogTileSeamOverlapPixels);
            FogMapEdgePaddingPixels = Mathf.Max(0f, FogMapEdgePaddingPixels);
            FogMapEdgeOverhangCells = Mathf.Max(0f, FogMapEdgeOverhangCells);
            FogIconSpritePixelSize = ClampSpritePixelSize(FogIconSpritePixelSize);
            Fog3DTopClearance = Mathf.Max(0f, Fog3DTopClearance);
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
