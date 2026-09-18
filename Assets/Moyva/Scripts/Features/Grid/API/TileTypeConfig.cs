using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Jsonization;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Grid.API
{
    public enum TileGridMode
    {
        Normal = 0,
        Dual = 1,
        Flat = 2,
    }

    public enum TileVisualSlot
    {
        Top = 0,
        Middle = 1,
        Bottom = 2,
    }

    [Serializable]
    public sealed class TileVisualVariantConfig
    {
        public TilePreset Preset;
        public TileVisualSlot Slot = TileVisualSlot.Top;
        [Range(0f, 1f)] public float Weight = 1f;
        [Min(0f)] public float TileHeight;
    }

    [Serializable]
    public sealed class TileVisualConfig
    {
        public TileGridMode GridMode = TileGridMode.Dual;
        public List<TileVisualVariantConfig> Variants = new();
        public GameObject RepresentativePrefab;
        public bool ScaleToCellSize = true;
        public float LayerYOffset;
        public Vector3 ScaleOffset = Vector3.one;
        public float SurfaceOffset;

        public Material FlatSurfaceMaterial;
        public float TileLayerHeightOffset;
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
    /// Canonical JSON-authored definition of one semantic terrain tile.
    /// </summary>
    [Serializable]
    public sealed class TileTypeConfig : MoyvaJsonConfigObject
    {
        public string DisplayName;
        public List<string> Aliases = new();
        public List<string> Tags = new();
        public string TraversalClassId;
        public TileVisualConfig Visual = new();
    }

    [Serializable]
    public sealed class MovementFallbackConfig
    {
        public bool Passable;
        [Min(0f)] public float StaminaCost = 1f;
    }

    [Serializable]
    public sealed class MovementClassRuleConfig
    {
        public string ClassId;
        public bool Passable = true;
        [Min(0f)] public float StaminaCost = 1f;
    }

    [Serializable]
    public sealed class MovementTileOverrideConfig
    {
        public string TileTypeId;
        public bool Passable = true;
        [Min(0f)] public float StaminaCost = 1f;
    }

    /// <summary>
    /// Reusable terrain traversal rules referenced by movable entity configs.
    /// </summary>
    [Serializable]
    public sealed class MovementProfileConfig : MoyvaJsonConfigObject
    {
        public MovementFallbackConfig Fallback = new();
        public List<MovementClassRuleConfig> ClassRules = new();
        public List<MovementTileOverrideConfig> TileOverrides = new();
    }
}
