using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Grid.API
{
    public static class MovementProfileIds
    {
        public const string GroundDefault = "ground-default";
    }

    public sealed class TileVisualVariantSnapshot
    {
        public TileVisualVariantSnapshot(
            TilePreset preset,
            TileVisualSlot slot,
            float weight,
            float tileHeight)
        {
            Preset = preset;
            Slot = slot;
            Weight = weight;
            TileHeight = tileHeight;
        }

        public TilePreset Preset { get; }
        public TileVisualSlot Slot { get; }
        public float Weight { get; }
        public float TileHeight { get; }
    }

    /// <summary>
    /// Frozen runtime-вигляд секції visual з tile-type JSON.
    /// Це не authoring-модель: значення сюди потрапляють після Load/Resolve.
    /// </summary>
    public sealed class TileVisualSnapshot
    {
        public TileVisualSnapshot(TileVisualConfig source)
        {
            source ??= new TileVisualConfig();
            GridMode = source.GridMode;
            RepresentativePrefab = source.RepresentativePrefab;
            ScaleToCellSize = source.ScaleToCellSize;
            LayerYOffset = source.LayerYOffset;
            ScaleOffset = source.ScaleOffset;
            SurfaceOffset = source.SurfaceOffset;
            FlatSurfaceMaterial = source.FlatSurfaceMaterial;
            TileLayerHeightOffset = source.TileLayerHeightOffset;
            IgnoreFillTiles = source.IgnoreFillTiles;
            MeshGenerationOverride = source.MeshGenerationOverride;
            MergeTiles = source.MergeTiles;
            ShadowCastingMode = source.ShadowCastingMode;
            ObjectLayer = source.ObjectLayer;
            RenderingLayer = source.RenderingLayer;
            ColliderType = source.ColliderType;
            TileColliderHeight = source.TileColliderHeight;
            TileColliderExtrusionHeight = source.TileColliderExtrusionHeight;
            InvertCollisionWalls = source.InvertCollisionWalls;

            var variants = new List<TileVisualVariantSnapshot>();
            if (source.Variants != null)
            {
                for (int i = 0; i < source.Variants.Count; i++)
                {
                    TileVisualVariantConfig variant = source.Variants[i];
                    if (variant == null)
                        continue;

                    variants.Add(new TileVisualVariantSnapshot(
                        variant.Preset,
                        variant.Slot,
                        variant.Weight,
                        variant.TileHeight));
                }
            }

            Variants = variants.AsReadOnly();
        }

        public TileGridMode GridMode { get; }
        public IReadOnlyList<TileVisualVariantSnapshot> Variants { get; }
        public GameObject RepresentativePrefab { get; }
        public bool ScaleToCellSize { get; }
        public float LayerYOffset { get; }
        public Vector3 ScaleOffset { get; }
        public float SurfaceOffset { get; }
        public Material FlatSurfaceMaterial { get; }
        public float TileLayerHeightOffset { get; }
        public bool IgnoreFillTiles { get; }
        public bool MeshGenerationOverride { get; }
        public bool MergeTiles { get; }
        public ShadowCastingMode ShadowCastingMode { get; }
        public LayerMask ObjectLayer { get; }
        public RenderingLayerMask RenderingLayer { get; }
        public Configuration.ColliderType ColliderType { get; }
        public float TileColliderHeight { get; }
        public float TileColliderExtrusionHeight { get; }
        public bool InvertCollisionWalls { get; }
    }

    /// <summary>
    /// Семантичний тип terrain tile, яким користуються рух, будівництво, fog і generator.
    /// </summary>
    public sealed class TileTypeSnapshot
    {
        private readonly HashSet<string> _tags;

        public TileTypeSnapshot(
            string id,
            string displayName,
            string traversalClassId,
            IEnumerable<string> aliases,
            IEnumerable<string> tags,
            TileVisualConfig visual)
        {
            Id = id ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName.Trim();
            TraversalClassId = traversalClassId?.Trim() ?? string.Empty;
            Aliases = Normalize(aliases).AsReadOnly();
            var normalizedTags = Normalize(tags);
            Tags = normalizedTags.AsReadOnly();
            _tags = new HashSet<string>(normalizedTags, StringComparer.OrdinalIgnoreCase);
            Visual = new TileVisualSnapshot(visual);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string TraversalClassId { get; }
        public IReadOnlyList<string> Aliases { get; }
        public IReadOnlyList<string> Tags { get; }
        public TileVisualSnapshot Visual { get; }

        public bool HasTag(string tag)
            => !string.IsNullOrWhiteSpace(tag) && _tags.Contains(tag.Trim());

        private static List<string> Normalize(IEnumerable<string> values)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (values == null)
                return result;

            foreach (string value in values)
            {
                string normalized = value?.Trim();
                if (!string.IsNullOrWhiteSpace(normalized) && seen.Add(normalized))
                    result.Add(normalized);
            }

            return result;
        }
    }

    public sealed class MovementRuleSnapshot
    {
        public MovementRuleSnapshot(bool passable, float staminaCost)
        {
            Passable = passable;
            StaminaCost = staminaCost;
        }

        public bool Passable { get; }
        public float StaminaCost { get; }
    }

    /// <summary>
    /// Frozen правила руху для одного класу сутностей.
    /// Пріоритет резолву: tile override -> traversal class -> fallback.
    /// </summary>
    public sealed class MovementProfileSnapshot
    {
        private readonly IReadOnlyDictionary<string, MovementRuleSnapshot> _classRules;
        private readonly IReadOnlyDictionary<string, MovementRuleSnapshot> _tileOverrides;

        public MovementProfileSnapshot(
            string id,
            MovementRuleSnapshot fallback,
            IReadOnlyDictionary<string, MovementRuleSnapshot> classRules,
            IReadOnlyDictionary<string, MovementRuleSnapshot> tileOverrides)
        {
            Id = id ?? string.Empty;
            Fallback = fallback ?? new MovementRuleSnapshot(false, 0f);
            _classRules = classRules ?? new Dictionary<string, MovementRuleSnapshot>();
            _tileOverrides = tileOverrides ?? new Dictionary<string, MovementRuleSnapshot>();
        }

        public string Id { get; }
        public MovementRuleSnapshot Fallback { get; }

        public bool TryGetTileOverride(string tileTypeId, out MovementRuleSnapshot rule)
            => _tileOverrides.TryGetValue(tileTypeId ?? string.Empty, out rule);

        public bool TryGetClassRule(string classId, out MovementRuleSnapshot rule)
            => _classRules.TryGetValue(classId ?? string.Empty, out rule);
    }

    /// <summary>
    /// Публічна read-only межа для terrain типів. Вона потрібна між фічами,
    /// бо різні системи читають той самий semantic tileId.
    /// </summary>
    public interface ITileTypeRepository
    {
        int Revision { get; }
        IReadOnlyCollection<TileTypeSnapshot> All { get; }
        bool TryGet(string tileTypeId, out TileTypeSnapshot tileType);
        bool TryResolveId(string tileTypeIdOrAlias, out string canonicalTileTypeId);
    }

    /// <summary>
    /// Read-only межа карти terrain. Запис terrain ID залишається тільки в IGridService.
    /// </summary>
    public interface ITileMapQuery
    {
        bool ContainsCell(Vector2Int position);
        bool TryGetTileTypeId(Vector2Int position, out string tileTypeId);
    }

    /// <summary>
    /// Єдина runtime-точка відповіді на питання, чи профіль може пройти конкретний tileId і за яку stamina cost.
    /// </summary>
    public interface ITraversalCostResolver
    {
        bool TryResolve(
            string movementProfileId,
            string tileTypeId,
            out float staminaCost,
            out string reason);
    }
}
