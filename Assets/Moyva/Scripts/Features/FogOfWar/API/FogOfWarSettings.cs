using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public enum FogVolumeUpdateMode
    {
        DebouncePerFrame = 0,
        Interval = 1,
        Immediate = 2,
    }

    public enum FogVolumeTilePresetSlot
    {
        Top = 0,
        Middle = 1,
        Bottom = 2,
    }

    public enum FogVolumeHeightSource
    {
        TerrainLevelMapThenHeightMap = 0,
        HeightMapThenTerrainLevelMap = 1,
        Flat = 2,
    }

    [Serializable]
    public sealed class FogVolumeTilePresetVariant
    {
        [Required]
        [AssetsOnly]
        [ValidateInput(nameof(HasDualGridPrefab), "TilePreset має містити dual-grid prefabs.")]
        public TilePreset Preset;

        public FogVolumeTilePresetSlot Slot = FogVolumeTilePresetSlot.Top;

        [Range(0f, 1f)]
        public float Weight = 1f;

        [MinValue(0f)]
        public float TileHeight;

        public bool IsConfigured => Preset != null;

        public float NormalizedWeight => Mathf.Clamp01(Weight);

        private bool HasDualGridPrefab(TilePreset preset)
        {
            return preset == null || FogOfWarSettings.HasUsableDualGridPreset(preset);
        }
    }

    [Serializable]
    public sealed class FogVolumeStateTileSettings
    {
        [HorizontalGroup("State", Width = 80)]
        public bool Enabled = true;

        [HorizontalGroup("State")]
        [Required]
        [ValidateInput(nameof(HasLayerName), "Layer name cannot be empty.")]
        public string LayerName = "Fog_State";

        [TableList(AlwaysExpanded = true, DrawScrollView = false)]
        [ValidateInput(nameof(HasRequiredPreset), "Enabled fog state needs at least one dual-grid TilePreset.")]
        public List<FogVolumeTilePresetVariant> TileVariants = new List<FogVolumeTilePresetVariant>
        {
            new FogVolumeTilePresetVariant()
        };

        [FoldoutGroup("Build Layer")]
        public float LayerYOffset;

        [FoldoutGroup("Build Layer")]
        public Vector3 ScaleOffset = Vector3.one;

        [FoldoutGroup("Build Layer")]
        public ShadowCastingMode ShadowCastingMode = ShadowCastingMode.Off;

        [FoldoutGroup("Build Layer")]
        public LayerMask ObjectLayer = 0;

        [FoldoutGroup("Build Layer")]
        public RenderingLayerMask RenderingLayer = 1;

        [FoldoutGroup("Build Layer")]
        public Configuration.ColliderType ColliderType = Configuration.ColliderType.none;

        [FoldoutGroup("Build Layer")]
        [MinValue(0f)]
        public float TileColliderHeight;

        [FoldoutGroup("Build Layer")]
        [MinValue(0f)]
        public float TileColliderExtrusionHeight;

        [FoldoutGroup("Build Layer")]
        public bool InvertCollisionWalls;

        public void EnsureDefaults(string fallbackLayerName)
        {
            if (string.IsNullOrWhiteSpace(LayerName))
                LayerName = fallbackLayerName;

            TileVariants ??= new List<FogVolumeTilePresetVariant>();
            if (TileVariants.Count == 0)
                TileVariants.Add(new FogVolumeTilePresetVariant());

            ScaleOffset = new Vector3(
                Mathf.Max(0.001f, ScaleOffset.x),
                Mathf.Max(0.001f, ScaleOffset.y),
                Mathf.Max(0.001f, ScaleOffset.z));
            TileColliderHeight = Mathf.Max(0f, TileColliderHeight);
            TileColliderExtrusionHeight = Mathf.Max(0f, TileColliderExtrusionHeight);
        }

        private bool HasLayerName(string layerName)
            => !Enabled || !string.IsNullOrWhiteSpace(layerName);

        private bool HasRequiredPreset(List<FogVolumeTilePresetVariant> variants)
        {
            if (!Enabled)
                return true;

            if (variants == null)
                return false;

            for (int i = 0; i < variants.Count; i++)
            {
                var variant = variants[i];
                if (variant != null
                    && variant.Preset != null
                    && FogOfWarSettings.HasUsableDualGridPreset(variant.Preset))
                    return true;
            }

            return false;
        }
    }

    [Serializable]
    public sealed class FogVolumeTileSettings
    {
        [BoxGroup("Runtime")]
        public FogVolumeUpdateMode UpdateMode = FogVolumeUpdateMode.DebouncePerFrame;

        [BoxGroup("Runtime")]
        [ShowIf(nameof(UpdateMode), FogVolumeUpdateMode.Interval)]
        [MinValue(0.02f)]
        public float RebuildIntervalSeconds = 0.1f;

        [BoxGroup("Runtime")]
        [MinValue(0f)]
        public float TopClearance = 0.08f;

        [BoxGroup("Runtime")]
        public bool UseWorldCellSize = true;

        [BoxGroup("Runtime")]
        [HideIf(nameof(UseWorldCellSize))]
        [MinValue(0.001f)]
        public float CellSizeOverride = 1f;

        [BoxGroup("Generated World Heights")]
        public FogVolumeHeightSource HeightSource = FogVolumeHeightSource.TerrainLevelMapThenHeightMap;

        [BoxGroup("Generated World Heights")]
        [Tooltip("World units per TerrainLevelMap step when the generated world provides integer terrain levels.")]
        [MinValue(0.001f)]
        public float TerrainLevelHeightStep = 1f;

        [BoxGroup("Generated World Heights")]
        [Tooltip("Height values closer than this are built on the same TWC layer. Keeps runtime layer count stable for noisy HeightMap data.")]
        [MinValue(0.001f)]
        public float HeightLayerSnap = 0.01f;

        [BoxGroup("Preview")]
        [Tooltip("Editor preview uses the current scene TWC grid when available. It is cleared automatically at runtime and rebuilt from generated world data.")]
        public bool ClearPreviewOnRuntimeStart = true;

        [BoxGroup("Preview")]
        [MinValue(1)]
        public int PreviewFallbackWidth = 16;

        [BoxGroup("Preview")]
        [MinValue(1)]
        public int PreviewFallbackHeight = 16;

        [BoxGroup("Unexplored Fog")]
        [InlineProperty]
        [HideLabel]
        public FogVolumeStateTileSettings Unexplored = new FogVolumeStateTileSettings
        {
            LayerName = "Fog_Unexplored",
            LayerYOffset = 0f,
        };

        [BoxGroup("Explored Fog")]
        [InlineProperty]
        [HideLabel]
        public FogVolumeStateTileSettings Explored = new FogVolumeStateTileSettings
        {
            LayerName = "Fog_Explored",
            LayerYOffset = 0.02f,
        };

        public void EnsureDefaults()
        {
            RebuildIntervalSeconds = Mathf.Max(0.02f, RebuildIntervalSeconds);
            TopClearance = Mathf.Max(0f, TopClearance);
            CellSizeOverride = Mathf.Max(0.001f, CellSizeOverride);
            TerrainLevelHeightStep = Mathf.Max(0.001f, TerrainLevelHeightStep);
            HeightLayerSnap = Mathf.Max(0.001f, HeightLayerSnap);
            PreviewFallbackWidth = Mathf.Max(1, PreviewFallbackWidth);
            PreviewFallbackHeight = Mathf.Max(1, PreviewFallbackHeight);
            Unexplored ??= new FogVolumeStateTileSettings();
            Explored ??= new FogVolumeStateTileSettings();
            Unexplored.EnsureDefaults("Fog_Unexplored");
            Explored.EnsureDefaults("Fog_Explored");
        }
    }

    [CreateAssetMenu(menuName = "Moyva/FogOfWarSettings", fileName = "FogOfWarSettings")]
    public class FogOfWarSettings : ScriptableObject
    {
        [TitleGroup("Vision")]
        [MinValue(1)]
        public int DefaultVisionRange = 5;

        [TitleGroup("Vision")]
        [MinValue(1)]
        public int MinVisionRange = 1;

        [TitleGroup("Vision")]
        [MinValue(1)]
        public int MaxVisionRange = 12;

        [TitleGroup("Terrain LOS")]
        [MinValue(0.01f)]
        public float ElevationStep = 0.15f;

        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int ObserverHeightBonusPerStep = 1;

        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int DownhillVisionBonusPerStep = 1;

        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int UphillVisionPenaltyPerStep = 1;

        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int MaxObserverHeightBonus = 4;

        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int MaxDownhillVisionBonus = 2;

        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int MaxUphillVisionPenalty = 6;

        [TitleGroup("Terrain LOS")]
        [MinValue(0f)]
        public float OcclusionSlopeBias = 0.02f;

        [TitleGroup("Terrain LOS/Raycast")]
        [Range(1, 9)]
        public int TerrainRaySamplesPerTile = 5;

        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0.01f, 1f)]
        public float TerrainVisibilityThreshold = 0.5f;

        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0f, 1f)]
        public float PartialVisibilityDetectionMultiplier = 1f;

        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0.25f, 1f)]
        public float TerrainRayStepTiles = 0.5f;

        [TitleGroup("Terrain LOS/Raycast")]
        [MinValue(0f)]
        public float ObserverEyeHeightOffset = 0.35f;

        [TitleGroup("Terrain LOS/Raycast")]
        [MinValue(0f)]
        public float TargetSampleHeightOffset = 0.1f;

        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0.1f, 1f)]
        public float TerrainFarSampleDistanceRatio = 0.65f;

        [TitleGroup("Terrain LOS/Raycast")]
        [MinValue(0)]
        public int TerrainVisibilityCacheCapacity = 24576;

        [TitleGroup("Terrain LOS/Edges")]
        public bool EnableTerrainEdgeLineOfSight = true;

        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0.001f)]
        public float TerrainEdgeHeightThreshold = 0.12f;

        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0)]
        public int TerrainEdgePeekDistanceTiles = 1;

        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0)]
        public int TerrainEdgeBlindZoneTiles = 2;

        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0f)]
        public float TerrainEdgeBlindZoneDistanceScale = 0.35f;

        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0)]
        public int TerrainEdgeMaxBlindZoneTiles = 4;

        [TitleGroup("Terrain LOS/Edges")]
        [Range(0f, 1f)]
        public float TerrainEdgeUphillPeekStrength = 0.65f;

        [TitleGroup("TWC Volume")]
        [InlineProperty]
        [HideLabel]
        public FogVolumeTileSettings Volume = new FogVolumeTileSettings();

        [TitleGroup("Renderer Culling")]
        [Tooltip("Disables world renderers that are fully covered by unexplored fog.")]
        public bool EnableRendererCulling = true;

        [TitleGroup("Renderer Culling")]
        [Tooltip("If enabled, renderer culling works only when UnexploredAlpha is close to opaque (>= 0.99).")]
        public bool RequireOpaqueUnexploredForCulling = true;

        [TitleGroup("Renderer Culling")]
        [Tooltip("Layers affected by fog renderer culling.")]
        public LayerMask RendererCullingLayerMask = ~0;

        [TitleGroup("Renderer Culling")]
        [Tooltip("Maximum tracked renderers evaluated per frame. Lower values spread work across more frames.")]
        [MinValue(1)]
        public int RendererCullingMaxRenderersPerFrame = 384;

        [TitleGroup("Renderer Culling")]
        [Tooltip("How often the culling service searches for newly spawned world renderers.")]
        [MinValue(0.05f)]
        public float RendererCullingDiscoveryInterval = 0.75f;

        [TitleGroup("Renderer Culling")]
        [Tooltip("Small bounds padding in map cells to avoid edge flicker when sprites move between cells.")]
        [MinValue(0f)]
        public float RendererCullingBoundsPaddingCells = 0f;

        [TitleGroup("Renderer Culling")]
        [Tooltip("Alpha for fully unexplored fog; still used by renderer culling safety checks.")]
        [Range(0f, 1f)]
        public float UnexploredAlpha = 1f;

        [TitleGroup("Renderer Culling")]
        [Tooltip("Alpha for explored fog; kept for preset migration and tuning notes.")]
        [Range(0f, 1f)]
        public float ExploredAlpha = 0.5f;

        [TitleGroup("Startup Reveal")]
        [Tooltip("If no bootstrap startup reveal arrives for a fresh non-load world, FogOfWarService opens a random visible start area so the map never starts fully black.")]
        public bool EnableStartupFallbackReveal = true;

        [TitleGroup("Startup Reveal")]
        [Tooltip("Radius used by FogOfWarService when bootstrap did not provide a startup reveal.")]
        [MinValue(1)]
        public int StartupFallbackRevealRadius = 15;

        [TitleGroup("Startup Reveal")]
        [Tooltip("Minimum margin from map edge for the fallback random start point.")]
        [MinValue(0)]
        public int StartupFallbackMinMarginFromBorder = 5;

        [TitleGroup("Startup Reveal")]
        [Tooltip("Additional map-relative margin for the fallback random start point.")]
        [Range(0f, 0.45f)]
        public float StartupFallbackRelativeMarginFactor = 0.1667f;

        [TitleGroup("Startup Reveal")]
        [Tooltip("Shape used by FogOfWarService fallback startup reveal.")]
        public FogRevealShape StartupFallbackRevealShape = FogRevealShape.PixelCircle;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Color UnexploredColor = new Color(0f, 0f, 0f, 1f);

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Color ExploredColor = new Color(0f, 0f, 0f, 0.5f);

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Sprite FogTileSprite;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2Int FogTileSpritePixelSize = new Vector2Int(16, 16);

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2 FogTileSizeInCells = Vector2.one;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogTileSeamOverlapPixels = 1f;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogMapEdgePaddingPixels = 2f;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogMapEdgeOverhangCells = 0.5f;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogTileTiling = 1f;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Sprite[] FogIconSprites;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2Int FogIconSpritePixelSize = new Vector2Int(16, 16);

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2Int FogIconGridSize = new Vector2Int(10, 10);

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogIconScale = 0.5f;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public bool CenterIcon = true;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public bool FullyTransparentWhenVisible = true;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float Fog3DTopClearance = 0.08f;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public bool EnableShaderFogCulling = false;

        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float ShaderFogCullThreshold = 0.01f;

        public static bool HasUsableDualGridPreset(TilePreset preset)
        {
            return preset != null
                && (preset.DUALGRD_cornerTile != null
                    || preset.DUALGRD_invertedCornerTile != null
                    || preset.DUALGRD_edgeTile != null
                    || preset.DUALGRD_fillTile != null
                    || preset.DUALGRD_doubleInteriorCornerTile != null);
        }

        private void OnValidate()
        {
            DefaultVisionRange = Mathf.Max(1, DefaultVisionRange);
            MinVisionRange = Mathf.Max(1, MinVisionRange);
            MaxVisionRange = Mathf.Max(MinVisionRange, MaxVisionRange);
            DefaultVisionRange = Mathf.Clamp(DefaultVisionRange, MinVisionRange, MaxVisionRange);
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
            Volume ??= new FogVolumeTileSettings();
            Volume.EnsureDefaults();
            FogTileSpritePixelSize = ClampSpritePixelSize(FogTileSpritePixelSize);
            FogTileSizeInCells = ClampTileSizeInCells(FogTileSizeInCells);
            FogTileSeamOverlapPixels = Mathf.Max(0f, FogTileSeamOverlapPixels);
            FogMapEdgePaddingPixels = Mathf.Max(0f, FogMapEdgePaddingPixels);
            FogMapEdgeOverhangCells = Mathf.Max(0f, FogMapEdgeOverhangCells);
            FogTileTiling = Mathf.Max(1f, FogTileTiling);
            FogIconSpritePixelSize = ClampSpritePixelSize(FogIconSpritePixelSize);
            FogIconScale = Mathf.Max(0.1f, FogIconScale);
            Fog3DTopClearance = Mathf.Max(0f, Fog3DTopClearance);
            StartupFallbackRevealRadius = Mathf.Max(1, StartupFallbackRevealRadius);
            StartupFallbackMinMarginFromBorder = Mathf.Max(0, StartupFallbackMinMarginFromBorder);
            StartupFallbackRelativeMarginFactor = Mathf.Clamp(StartupFallbackRelativeMarginFactor, 0f, 0.45f);
            RendererCullingMaxRenderersPerFrame = Mathf.Max(1, RendererCullingMaxRenderersPerFrame);
            RendererCullingDiscoveryInterval = Mathf.Max(0.05f, RendererCullingDiscoveryInterval);
            RendererCullingBoundsPaddingCells = Mathf.Max(0f, RendererCullingBoundsPaddingCells);
            UnexploredAlpha = Mathf.Clamp01(UnexploredAlpha);
            ExploredAlpha = Mathf.Clamp01(ExploredAlpha);
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
