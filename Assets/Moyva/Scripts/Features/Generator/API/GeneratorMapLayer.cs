using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// Defines whether chunk-first generation creates vertical closure geometry
    /// for a recipe layer. Explicit so runtime never infers water behaviour
    /// from asset names or materials.
    /// </summary>
    public enum TileGeometryMode
    {
        SolidTerrain = 0,
        SurfaceOnly = 1
    }

    /// <summary>
    /// Controls how authored tile meshes may be changed while producing the
    /// runtime terrain mesh.
    /// </summary>
    public enum AuthoredClosurePolicy
    {
        PreserveAuthored = 0,
        GeneratedClosure = 1,
        TopOnlyTemplate = 2
    }

    /// <summary>Which output channel a recipe layer feeds.</summary>
    public enum LayerOutputKind
    {
        Tiles,
        Objects,
        Masks,
        InternalData,
        Other
    }

    public enum TilePresetSlot
    {
        Top,
        Middle,
        Bottom
    }

    /// <summary>Weighted TilePreset candidate for a recipe layer slot.</summary>
    [System.Serializable]
    public sealed class TilePresetVariant
    {
        public TilePreset Preset;
        public TilePresetSlot Slot = TilePresetSlot.Top;
        [Range(0f, 1f)] public float Weight = 1f;
        [Min(0f)] public float TileHeight;

        public bool IsConfigured => Preset != null;
        public float NormalizedWeight => Mathf.Clamp01(Weight);
    }

    /// <summary>
    /// Build-layer overrides applied to the TWC TilesBuildLayer. Values coming
    /// from <see cref="TileTypeConfig.Visual"/> take precedence when a tile type
    /// is assigned; these fields remain the fallback for preset-only layers.
    /// </summary>
    [System.Serializable]
    public sealed class GeneratorTileBuildSettings
    {
        [Tooltip("If enabled, TWC uses Dual Grid. Enabled automatically for dual presets.")]
        public bool UseDualGrid;
        [Tooltip("Scale prefab tiles to the TWC cell size.")]
        public bool ScaleTileToCellSize = true;
        [Tooltip("Y offset of the whole build layer relative to BlueprintLayer.defaultLayerHeight.")]
        public float LayerYOffset;
        public Vector3 ScaleOffset = Vector3.one;
        [Tooltip("Height offset of the first tile layer inside the TWC build layer.")]
        public float TileLayerHeightOffset;
        [Tooltip("Ignore fill tiles in the first tile layer.")]
        public bool IgnoreFillTiles;
        public bool MeshGenerationOverride;
        public bool MergeTiles;
        public ShadowCastingMode ShadowCastingMode = ShadowCastingMode.On;
        public LayerMask ObjectLayer;
        public RenderingLayerMask RenderingLayer;
        public Configuration.ColliderType ColliderType = Configuration.ColliderType.none;
        [Min(0f)] public float TileColliderHeight;
        [Min(0f)] public float TileColliderExtrusionHeight;
        public bool InvertCollisionWalls;
    }

    /// <summary>
    /// One ordered generator layer inside a <see cref="GeneratorMapRecipe"/>.
    /// Each layer compiles to a TileWorldCreator blueprint layer; its
    /// <see cref="Steps"/> are a linear mask recipe evaluated top-down.
    /// </summary>
    [System.Serializable]
    public sealed class GeneratorMapLayer
    {
        [Tooltip("Stable id used by mask steps referencing this layer and by object placements.")]
        public string Id = System.Guid.NewGuid().ToString();
        public string Name = "Layer";
        public Color Color = Color.white;
        public int SortingOrder;
        public bool Enabled = true;
        public float DefaultHeight;
        public bool UseZeroLayerPadding;
        [Min(0)] public int ExtraWidthCells;
        [Min(0)] public int ExtraLengthCells;
        public bool GenerateFlatSurface;
        public Material FlatSurfaceMaterial;
        public TileGeometryMode TileGeometryMode = TileGeometryMode.SolidTerrain;
        public AuthoredClosurePolicy AuthoredClosurePolicy = AuthoredClosurePolicy.PreserveAuthored;

        [Tooltip("Persistent link to the BlueprintLayer in the companion TWC Configuration.")]
        public string BlueprintLayerGuid;

        [Tooltip("Which output channel this layer feeds.")]
        public LayerOutputKind OutputKind = LayerOutputKind.Tiles;

        [Tooltip("Canonical moyva.tile-type JSON referenced by this layer.")]
        public TileTypeConfig TileType;

        [Tooltip("Weighted preset variants; overridden by TileType.Visual.Variants when set.")]
        public List<TilePresetVariant> TileVariants = new();

        [Tooltip("Build-layer overrides used when TileType has no Visual config.")]
        public GeneratorTileBuildSettings TileBuild = new();

        [Tooltip("Ordered mask steps evaluated top-down to produce this layer's mask.")]
        public List<GeneratorMaskStep> Steps = new();

        /// <summary>
        /// Effective preset variants: <see cref="TileTypeConfig.Visual"/> variants win
        /// when configured; otherwise the layer's own <see cref="TileVariants"/>.
        /// </summary>
        public List<TilePresetVariant> ResolveTileVariants()
        {
            var variants = new List<TilePresetVariant>();
            var visualVariants = TileType?.Visual?.Variants;
            if (visualVariants != null && visualVariants.Count > 0)
            {
                foreach (var source in visualVariants)
                {
                    if (source?.Preset == null)
                        continue;
                    variants.Add(new TilePresetVariant
                    {
                        Preset = source.Preset,
                        Slot = source.Slot switch
                        {
                            TileVisualSlot.Middle => TilePresetSlot.Middle,
                            TileVisualSlot.Bottom => TilePresetSlot.Bottom,
                            _ => TilePresetSlot.Top
                        },
                        Weight = source.Weight,
                        TileHeight = source.TileHeight
                    });
                }
                return variants;
            }

            if (TileVariants != null)
            {
                foreach (var variant in TileVariants)
                {
                    if (variant != null && variant.Preset != null)
                        variants.Add(variant);
                }
            }
            return variants;
        }

        /// <summary>Canonical tile id for this layer (tile-type id or preset tile id).</summary>
        public string ResolveTileId()
        {
            if (!string.IsNullOrWhiteSpace(TileType?.JsonId))
                return TileType.JsonId.Trim();
            var preset = ResolveTileVariants().Find(v => v?.Preset != null)?.Preset;
            return !string.IsNullOrWhiteSpace(preset?.tileId)
                ? preset.tileId.Trim()
                : null;
        }

        /// <summary>True when this layer produces renderable tile output.</summary>
        public bool HasRenderableTileOutput()
        {
            if (OutputKind != LayerOutputKind.Tiles)
                return false;
            if (ResolveGenerateFlatSurface() || TileType != null)
                return true;
            return ResolveTileVariants().Exists(v => v?.Preset != null);
        }

        public bool ResolveGenerateFlatSurface()
        {
            var visual = TileType?.Visual;
            return visual != null
                ? visual.GridMode == TileGridMode.Flat
                : GenerateFlatSurface;
        }

        public bool ResolveUseDualGrid()
        {
            var visual = TileType?.Visual;
            if (visual != null)
                return visual.GridMode == TileGridMode.Dual;
            var variants = ResolveTileVariants();
            if (variants.Exists(v => v.Preset != null && v.Preset.gridtype == TilePreset.GridType.dual))
                return true;
            if (TileBuild == null || !TileBuild.UseDualGrid)
                return false;
            return variants.Count == 0 || variants.Exists(v => HasAnyDualGridPrefab(v.Preset));
        }

        private static bool HasAnyDualGridPrefab(TilePreset preset)
        {
            return preset != null
                && (preset.DUALGRD_cornerTile != null
                    || preset.DUALGRD_invertedCornerTile != null
                    || preset.DUALGRD_edgeTile != null
                    || preset.DUALGRD_fillTile != null
                    || preset.DUALGRD_doubleInteriorCornerTile != null);
        }
    }
}
