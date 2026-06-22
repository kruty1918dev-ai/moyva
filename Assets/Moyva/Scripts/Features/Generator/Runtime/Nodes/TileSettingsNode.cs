using System;
using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Attributes;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum TilePresetSlot
    {
        Top,
        Middle,
        Bottom
    }

    [Serializable]
    public sealed class TilePresetVariant
    {
        [TilePresetPopup]
        public TilePreset Preset;

        public TilePresetSlot Slot = TilePresetSlot.Top;

        [Range(0f, 1f)]
        public float Weight = 1f;

        [Min(0f)]
        public float TileHeight;

        public bool IsConfigured => Preset != null;

        public float NormalizedWeight => Mathf.Clamp01(Weight);
    }

    /// <summary>
    /// Immutable runtime snapshot emitted by <see cref="TileSettingsNode"/> for graph diagnostics.
    /// The compiler reads the node itself so that Unity object references remain intact.
    /// </summary>
    [Serializable]
    public sealed class GraphTileSettings
    {
        public string NodeId;
        public string TilePresetName;
        public string TileId;
        public TilePresetSlot Slot;
        public float Weight;
        public float TileHeight;
        public bool UseDualGrid;
        public bool ScaleTileToCellSize;
        public bool GenerateFlatSurface;

        public override string ToString()
        {
            if (GenerateFlatSurface)
                return $"Flat Surface ({NodeId})";

            return string.IsNullOrWhiteSpace(TilePresetName)
                ? $"Tile Settings: <missing preset> ({NodeId})"
                : $"Tile Settings: {TilePresetName} [{Slot}] weight={Weight:0.###}";
        }
    }

    [NodeInfo(
        "Tile Settings",
        "Tiles",
        "Налаштування tileset/build-шару всередині graph layer. Один Tile Settings node описує build-параметри шару та список TilePreset варіантів для weighted random вибору; старе окреме build-layer вікно більше не є основним джерелом налаштувань.")]
    public sealed class TileSettingsNode : NodeBase, IPreviewableNode
    {
        [Header("Tile Preset Variants")]
        [SerializeField]
        [Tooltip("Список tileset/preset варіантів для цього шару. TWC вибирає варіант випадково за Weight у межах одного Slot.")]
        private List<TilePresetVariant> _tileVariants = new List<TilePresetVariant>
        {
            new TilePresetVariant()
        };

        [HideInInspector, SerializeField]
        private TilePreset _tilePreset;

        [HideInInspector, SerializeField]
        private TilePresetSlot _slot = TilePresetSlot.Top;

        [HideInInspector, SerializeField]
        private float _weight = 1f;

        [HideInInspector, SerializeField]
        private float _tileHeight;

        [Header("Build Layer")]
        [SerializeField]
        [Tooltip("Якщо увімкнено, TWC використовує Dual Grid. Для dual TilePreset це увімкнеться автоматично під час застосування.")]
        private bool _useDualGrid = true;

        [SerializeField]
        [Tooltip("Масштабувати prefab-тайли до розміру клітинки TWC.")]
        private bool _scaleTileToCellSize = true;

        [SerializeField]
        [Tooltip("Y offset усього build-шару відносно BlueprintLayer.defaultLayerHeight.")]
        private float _layerYOffset;

        [SerializeField]
        [Tooltip("Scale offset для TWC TilesBuildLayer.")]
        private Vector3 _scaleOffset = Vector3.one;

        [Header("Flat Surface")]
        [SerializeField]
        [Tooltip("Замість TilePreset prefabs створити одну пласку grid-поверхню для цього шару.")]
        private bool _generateFlatSurface;

        [SerializeField]
        [Tooltip("Матеріал для Flat Surface режиму.")]
        private Material _flatSurfaceMaterial;

        [Header("Tile Layer")]
        [SerializeField]
        [Tooltip("Height offset першого tile layer у TWC build-шарі.")]
        private float _tileLayerHeightOffset;

        [SerializeField]
        [Tooltip("Ігнорувати fill tiles у першому tile layer.")]
        private bool _ignoreFillTiles;

        [Header("Mesh / Collider")]
        [SerializeField]
        private bool _meshGenerationOverride;

        [SerializeField]
        private bool _mergeTiles;

        [SerializeField]
        private ShadowCastingMode _shadowCastingMode = ShadowCastingMode.On;

        [SerializeField]
        private LayerMask _objectLayer;

        [SerializeField]
        private RenderingLayerMask _renderingLayer = default;

        [SerializeField]
        private Configuration.ColliderType _colliderType = Configuration.ColliderType.none;

        [SerializeField]
        [Min(0f)]
        private float _tileColliderHeight;

        [SerializeField]
        [Min(0f)]
        private float _tileColliderExtrusionHeight;

        [SerializeField]
        private bool _invertCollisionWalls;

        [NonSerialized] private bool[,] _lastMask;

        public IReadOnlyList<TilePresetVariant> TileVariants => GetConfiguredPresetVariants();
        public TilePreset TilePreset => ResolvePrimaryVariant()?.Preset ?? _tilePreset;
        public TilePresetSlot Slot => ResolvePrimaryVariant()?.Slot ?? _slot;
        public float Weight => ResolvePrimaryVariant()?.NormalizedWeight ?? Mathf.Clamp01(_weight);
        public float PrimaryTileHeight => Mathf.Max(0f, ResolvePrimaryVariant()?.TileHeight ?? _tileHeight);
        public float LayerYOffset => _layerYOffset;
        public float TileLayerHeightOffset => _tileLayerHeightOffset;
        public bool UseDualGrid => _useDualGrid || GetConfiguredPresetVariants().Any(variant => ShouldUseDualGrid(variant.Preset, false));
        public bool ScaleTileToCellSize => _scaleTileToCellSize || UseDualGrid;
        public bool GenerateFlatSurface => _generateFlatSurface;
        public bool HasRenderableTileOutput => _generateFlatSurface || GetConfiguredPresetVariants().Any(variant => variant.Preset != null);
        public int ConfiguredVariantCount => GetConfiguredPresetVariants().Count;
        public string TileId
        {
            get
            {
                var preset = ResolvePrimaryVariant()?.Preset ?? _tilePreset;
                return !string.IsNullOrWhiteSpace(preset?.tileId) ? preset.tileId.Trim() : null;
            }
        }

        public override string Title
        {
            get
            {
                if (_generateFlatSurface)
                    return "Tile Settings (Flat Surface)";

                var variants = GetConfiguredPresetVariants();
                if (variants.Count > 1)
                    return $"Tile Settings ({variants.Count} variants)";

                var preset = variants.Count == 1 ? variants[0].Preset : _tilePreset;
                return preset != null
                    ? $"Tile Settings ({preset.name})"
                    : "Tile Settings";
            }
        }

        public override string Category => "Tiles";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask"),
            PortDefinition.Output<GraphTileSettings>("Settings")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            _lastMask = inputs != null && inputs.Length > 0 ? inputs[0] as bool[,] : null;
            return NodeOutput.Success(_lastMask, CreateSnapshot());
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            if (_lastMask == null)
                return null;

            var texture = new Texture2D(
                Mathf.Max(1, _lastMask.GetLength(0)),
                Mathf.Max(1, _lastMask.GetLength(1)),
                TextureFormat.RGBA32,
                false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "Tile Settings Preview"
            };

            var active = HasRenderableTileOutput
                ? Color.white
                : (_generateFlatSurface ? new Color(0.7f, 0.7f, 0.7f, 1f) : Color.yellow);
            var inactive = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                    texture.SetPixel(x, y, _lastMask[x, y] ? active : inactive);
            }

            texture.Apply(false, false);
            return texture;
        }

        public GraphTileSettings CreateSnapshot()
        {
            return new GraphTileSettings
            {
                NodeId = NodeId,
                TilePresetName = TilePreset != null ? TilePreset.name : null,
                TileId = TileId,
                Slot = Slot,
                Weight = Weight,
                TileHeight = PrimaryTileHeight,
                UseDualGrid = UseDualGrid,
                ScaleTileToCellSize = ScaleTileToCellSize,
                GenerateFlatSurface = _generateFlatSurface
            };
        }

        public static List<TileSettingsNode> GetNodesForLayer(GraphAsset graph, string layerId)
        {
            if (graph == null)
                return new List<TileSettingsNode>();

            graph.EnsureLayerGraphStates();
            return graph.GetNodesForLayer(layerId)
                .OfType<TileSettingsNode>()
                .ToList();
        }

        public static bool HasRenderableTiles(GraphAsset graph, string layerId)
        {
            var nodes = GetNodesForLayer(graph, layerId);
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null && nodes[i].HasRenderableTileOutput)
                    return true;
            }

            return false;
        }

        public static string ResolveFirstTileId(GraphAsset graph, GeneratorLayerDefinition layer)
        {
            var nodes = GetNodesForLayer(graph, layer?.Id);
            for (int i = 0; i < nodes.Count; i++)
            {
                string tileId = nodes[i]?.TileId;
                if (!string.IsNullOrWhiteSpace(tileId))
                    return tileId;
            }

            return layer?.Id;
        }

        public static void ApplyNodesToBuildLayer(
            TilesBuildLayer buildLayer,
            IReadOnlyList<TileSettingsNode> nodes,
            Configuration configuration,
            BlueprintLayer blueprintLayer,
            GeneratorLayerDefinition layerDefinition)
        {
            if (buildLayer == null)
                return;

            nodes ??= Array.Empty<TileSettingsNode>();
            var primary = nodes.FirstOrDefault(node => node != null);

            buildLayer.configuration = configuration;
            buildLayer.layerName = layerDefinition?.Name ?? buildLayer.layerName;
            buildLayer.isEnabled = layerDefinition?.Enabled ?? buildLayer.isEnabled;
            buildLayer.currentBlueprintLayer = blueprintLayer;
            if (blueprintLayer != null)
                buildLayer.SetBlueprintLayer(blueprintLayer);

            if (primary != null)
                primary.ApplyGeneralSettings(buildLayer);
            else
                ApplyLegacyLayerSettings(buildLayer, layerDefinition);

            ApplyPresetSelections(buildLayer, nodes);
            EnsurePrimaryTileLayer(buildLayer, primary);
        }

        private void ApplyGeneralSettings(TilesBuildLayer buildLayer)
        {
            buildLayer.useDualGrid = UseDualGrid;
            buildLayer.scaleTileToCellSize = ScaleTileToCellSize;
            buildLayer.layerYOffset = _layerYOffset;
            buildLayer.scaleOffset = _scaleOffset;
            buildLayer.generateFlatSurface = _generateFlatSurface;
            buildLayer.flatSurfaceMaterial = _flatSurfaceMaterial;
            buildLayer.meshGenerationOverride = _meshGenerationOverride;
            buildLayer.mergeTiles = _mergeTiles;
            buildLayer.shadowCastingMode = _shadowCastingMode;
            buildLayer.objectLayer = _objectLayer;
            buildLayer.renderingLayer = _renderingLayer;
            buildLayer.colliderType = _colliderType;
            buildLayer.tileColliderHeight = Mathf.Max(0f, _tileColliderHeight);
            buildLayer.tileColliderExtrusionHeight = Mathf.Max(0f, _tileColliderExtrusionHeight);
            buildLayer.invertCollisionWalls = _invertCollisionWalls;
        }

        private static void ApplyLegacyLayerSettings(TilesBuildLayer buildLayer, GeneratorLayerDefinition layerDefinition)
        {
            if (layerDefinition == null)
                return;

            buildLayer.generateFlatSurface = layerDefinition.GenerateFlatSurface;
            buildLayer.flatSurfaceMaterial = layerDefinition.FlatSurfaceMaterial;
        }

        private static void ApplyPresetSelections(TilesBuildLayer buildLayer, IReadOnlyList<TileSettingsNode> nodes)
        {
            buildLayer.tilePresetsTop ??= new List<TilesBuildLayer.TilePresetSelection>();
            buildLayer.tilePresetsMiddle ??= new List<TilesBuildLayer.TilePresetSelection>();
            buildLayer.tilePresetsBottom ??= new List<TilesBuildLayer.TilePresetSelection>();

            buildLayer.tilePresetsTop.Clear();
            buildLayer.tilePresetsMiddle.Clear();
            buildLayer.tilePresetsBottom.Clear();

            if (nodes == null)
                return;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null || node._generateFlatSurface)
                    continue;

                var variants = node.GetConfiguredPresetVariants();
                for (int variantIndex = 0; variantIndex < variants.Count; variantIndex++)
                {
                    var variant = variants[variantIndex];
                    if (variant == null || variant.Preset == null)
                        continue;

                    var selection = new TilesBuildLayer.TilePresetSelection
                    {
                        preset = variant.Preset,
                        weight = variant.NormalizedWeight,
                        tileHeight = Mathf.Max(0f, variant.TileHeight)
                    };

                    switch (variant.Slot)
                    {
                        case TilePresetSlot.Middle:
                            buildLayer.tilePresetsMiddle.Add(selection);
                            break;
                        case TilePresetSlot.Bottom:
                            buildLayer.tilePresetsBottom.Add(selection);
                            break;
                        default:
                            buildLayer.tilePresetsTop.Add(selection);
                            break;
                    }
                }
            }
        }

        private static void EnsurePrimaryTileLayer(TilesBuildLayer buildLayer, TileSettingsNode primary)
        {
            buildLayer.tileLayers ??= new List<TilesBuildLayer.TileLayers>();
            if (buildLayer.tileLayers.Count == 0)
                buildLayer.tileLayers.Add(new TilesBuildLayer.TileLayers());

            var first = buildLayer.tileLayers[0] ?? new TilesBuildLayer.TileLayers();
            first.name = string.IsNullOrWhiteSpace(first.name) ? "Main" : first.name;
            if (primary != null)
            {
                first.heightOffset = primary._tileLayerHeightOffset;
                first.ignoreFillTiles = primary._ignoreFillTiles;
            }

            first.layerOverrides ??= new List<TilesBuildLayer.TilePresetOverride>();
            buildLayer.tileLayers[0] = first;
        }

        private TilePresetVariant ResolvePrimaryVariant()
        {
            var variants = GetConfiguredPresetVariants();
            return variants.Count > 0 ? variants[0] : null;
        }

        private List<TilePresetVariant> GetConfiguredPresetVariants()
        {
            var variants = new List<TilePresetVariant>();
            if (_tileVariants != null)
            {
                for (int i = 0; i < _tileVariants.Count; i++)
                {
                    var variant = _tileVariants[i];
                    if (variant != null && variant.Preset != null)
                        variants.Add(variant);
                }
            }

            // Backwards compatibility for graphs saved with the old one-preset TileSettingsNode.
            if (variants.Count == 0 && _tilePreset != null)
            {
                variants.Add(new TilePresetVariant
                {
                    Preset = _tilePreset,
                    Slot = _slot,
                    Weight = Mathf.Clamp01(_weight),
                    TileHeight = Mathf.Max(0f, _tileHeight)
                });
            }

            return variants;
        }

        private static bool ShouldUseDualGrid(TilePreset preset, bool requested)
        {
            return requested || preset != null && preset.gridtype == TilePreset.GridType.dual;
        }
    }
}