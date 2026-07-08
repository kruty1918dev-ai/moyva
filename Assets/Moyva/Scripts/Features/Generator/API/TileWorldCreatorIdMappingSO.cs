using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(fileName = "TileWorldCreatorIdMapping", menuName = "Moyva/Generator/TileWorldCreator ID Mapping")]
    public sealed class TileWorldCreatorIdMappingSO : ScriptableObject
    {
        [SerializeField] private LayerMapping[] _terrainLayers = Array.Empty<LayerMapping>();
        [SerializeField] private LayerMapping[] _objectLayers = Array.Empty<LayerMapping>();
        [SerializeField] private LayerMapping[] _buildingLayers = Array.Empty<LayerMapping>();

        public IReadOnlyList<LayerMapping> TerrainLayers => _terrainLayers;
        public IReadOnlyList<LayerMapping> ObjectLayers => _objectLayers;
        public IReadOnlyList<LayerMapping> BuildingLayers => _buildingLayers;

        public bool TryResolveTerrainLayer(string id, out LayerMapping mapping)
            => TryResolveLayer(_terrainLayers, id, out mapping);

        public bool TryResolveObjectLayer(string id, out LayerMapping mapping)
            => TryResolveLayer(_objectLayers, id, out mapping);

        public bool TryResolveBuildingLayer(string id, out LayerMapping mapping)
            => TryResolveLayer(_buildingLayers, id, out mapping);

        private static bool TryResolveLayer(IReadOnlyList<LayerMapping> layers, string id, out LayerMapping mapping)
        {
            mapping = null;
            if (layers == null || string.IsNullOrWhiteSpace(id))
                return false;

            for (int i = 0; i < layers.Count; i++)
            {
                var candidate = layers[i];
                if (candidate == null || !candidate.Matches(id))
                    continue;

                mapping = candidate;
                return true;
            }

            return false;
        }

        [Serializable]
        public sealed class LayerMapping
        {
            [SerializeField] private string _idPattern;
            [SerializeField] private string _blueprintLayerGuid;
            [SerializeField] private string _blueprintLayerName;
            [SerializeField] private TilePreset _tilePreset;
            [SerializeField] private bool _useDualGrid = true;
            [SerializeField] private bool _scaleTileToCellSize = true;
            [SerializeField] private GameObject _registryVisualPrefab;
            [SerializeField] private float _movementCost = 1f;

            public string IdPattern => _idPattern;
            public string BlueprintLayerGuid => _blueprintLayerGuid;
            public string BlueprintLayerName => _blueprintLayerName;
            public TilePreset TilePreset => _tilePreset;
            public bool UseDualGrid => _useDualGrid;
            public bool ScaleTileToCellSize => _scaleTileToCellSize;
            public GameObject RegistryVisualPrefab => _registryVisualPrefab;
            public float MovementCost => Mathf.Max(0f, _movementCost);
            public bool HasExactId => !string.IsNullOrWhiteSpace(_idPattern) && !_idPattern.Contains("*");

            public bool Matches(string id)
            {
                if (string.IsNullOrWhiteSpace(_idPattern) || string.IsNullOrWhiteSpace(id))
                    return false;

                if (!_idPattern.Contains("*"))
                    return string.Equals(_idPattern, id, StringComparison.Ordinal);

                return MatchesWildcard(_idPattern, id);
            }

            private static bool MatchesWildcard(string pattern, string value)
            {
                if (pattern == "*")
                    return true;

                string[] parts = pattern.Split('*');
                int searchIndex = 0;

                if (!pattern.StartsWith("*", StringComparison.Ordinal)
                    && !value.StartsWith(parts[0], StringComparison.Ordinal))
                    return false;

                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    if (string.IsNullOrEmpty(part))
                        continue;

                    int foundIndex = value.IndexOf(part, searchIndex, StringComparison.Ordinal);
                    if (foundIndex < 0)
                        return false;

                    searchIndex = foundIndex + part.Length;
                }

                string lastPart = parts.Length > 0 ? parts[^1] : string.Empty;
                return pattern.EndsWith("*", StringComparison.Ordinal)
                    || string.IsNullOrEmpty(lastPart)
                    || value.EndsWith(lastPart, StringComparison.Ordinal);
            }
        }
    }

    public enum TileWorldCreatorTerrainBuildMode
    {
        // Historical serialized value. Despite the old name, this now routes to the
        // chunk-first composite terrain builder. Do not change the numeric value.
        MergedChunksWithPrecomputedHeights = 0,
        LegacyPostBuildHeightProjection = 1,
        ChunkFirstCompositeMesh = 2
    }

    [Serializable]
    public sealed class TileWorldCreatorBuildOptions
    {
        [SerializeField] private bool _replaceMappedTerrainVisuals = true;
        [SerializeField] private bool _sendObjectIdsToTileWorldCreator;
        [SerializeField] private bool _replaceMappedObjectVisuals;
        [SerializeField] private bool _sendBuildingIdsToTileWorldCreator;
        [SerializeField] private bool _replaceMappedBuildingVisuals;
        [SerializeField] private bool _suppressMoyvaLayerDataWhenTerrainMapped = true;
        [SerializeField] private bool _resetConfigurationBeforeBuild = true;
        [SerializeField] private bool _syncConfigurationSize = true;
        [SerializeField] private bool _useWorldSeed = true;
        [SerializeField] private float _configurationCellSizeOverride;

        [Header("Terrain shaping")]
        [Tooltip("Підіймати тайли по інтегральному TerrainLevelMap після GenerateCompleteMap.")]
        [SerializeField] private bool _applyIntegerTerrainHeights = true;
        [Tooltip("Основний режим будує TWC merged chunks і підставляє висоту до merge. Legacy лишає старий post-build projector.")]
        [SerializeField] private TileWorldCreatorTerrainBuildMode _terrainBuildMode = TileWorldCreatorTerrainBuildMode.MergedChunksWithPrecomputedHeights;
        [Tooltip("Висота однієї одиниці level (Y units), завжди використовується як цілий множник.")]
        [SerializeField] private int _terrainHeightStep = 1;
        [Tooltip("Скільки секунд відстежувати нові tile transforms після GenerateCompleteMap (TWC спавнить тайли через корутини).")]
        [SerializeField] private float _terrainHeightTrackingSeconds = 60f;
        [Tooltip("Нормалізувати рівні графа у невелику integer шкалу для TWC visuals: вода, берег, суша, пагорби.")]
        [SerializeField] private bool _normalizeTerrainLevelsForTileWorldCreator = true;
        [Tooltip("Integer-рівень води. Можна поставити -1, якщо water mesh візуально вище піску.")]
        [SerializeField] private int _waterTerrainLevel;
        [Tooltip("Integer-рівень піску/берега біля води.")]
        [SerializeField] private int _shoreTerrainLevel = 1;
        [Tooltip("Базовий integer-рівень рівнинної суші.")]
        [SerializeField] private int _landTerrainLevel = 1;
        [Tooltip("Integer-рівень явних hill/forest/stone ділянок.")]
        [SerializeField] private int _hillTerrainLevel = 3;
        [Tooltip("Максимальний integer-рівень для TWC visuals.")]
        [SerializeField] private int _maxTerrainLevel = 5;

        [Header("Terrain side walls")]
        [Tooltip("Згенерувати вертикальні стінки між TWC cells з різними integer terrain levels.")]
        [SerializeField] private bool _generateTerrainSideWalls = true;
        [Tooltip("Матеріал вертикальних стінок. Якщо порожній — буде створено runtime material з кольором нижче.")]
        [SerializeField] private Material _terrainSideWallMaterial;
        [Tooltip("Колір runtime material для вертикальних стінок, коли матеріал не заданий явно.")]
        [SerializeField] private Color _terrainSideWallColor = new Color(0.36f, 0.34f, 0.3f, 1f);
        [Tooltip("Також будувати стінки по зовнішньому краю карти. Зазвичай вимкнено, щоб не створювати великі вертикальні плити по периметру.")]
        [SerializeField] private bool _generateTerrainSideWallsAtMapBorder;

        [Header("Runtime performance")]
        [Tooltip("Після стабільного height projection зливати TWC tile meshes у cluster meshes, щоб прибрати тисячі runtime renderers.")]
        [SerializeField] private bool _combineTerrainMeshesAfterHeightProjection = true;
        [Tooltip("Скільки TWC clusters оптимізувати за кадр. Менше значення дає менший фриз, більше — швидше прибирає лаги після генерації.")]
        [SerializeField] private int _terrainMeshCombineClustersPerFrame = 4;
        [Tooltip("Вимикати source GameObjects після combine. Якщо вимкнено — вимикаються лише source MeshRenderers, а colliders/об'єкти лишаються активними.")]
        [SerializeField] private bool _terrainMeshCombineDeactivateSourceObjects;

        [Header("Coastline")]
        [Tooltip("Розширити піщану смугу на 1 клітинку по обидва боки від води, щоб закрити прогалину.")]
        [SerializeField] private bool _expandSandShoreBand = true;
        [Tooltip("ID тайлу, який буде підставлено для shore-band. Має матчити мапінг (sand* → Sand).")]
        [SerializeField] private string _shoreBandTileId = "sand-shore-band";

        public bool ReplaceMappedTerrainVisuals => _replaceMappedTerrainVisuals;
        public bool SendObjectIdsToTileWorldCreator => _sendObjectIdsToTileWorldCreator;
        public bool ReplaceMappedObjectVisuals => _replaceMappedObjectVisuals;
        public bool SendBuildingIdsToTileWorldCreator => _sendBuildingIdsToTileWorldCreator;
        public bool ReplaceMappedBuildingVisuals => _replaceMappedBuildingVisuals;
        public bool SuppressMoyvaLayerDataWhenTerrainMapped => _suppressMoyvaLayerDataWhenTerrainMapped;
        public bool ResetConfigurationBeforeBuild => _resetConfigurationBeforeBuild;
        public bool SyncConfigurationSize => _syncConfigurationSize;
        public bool UseWorldSeed => _useWorldSeed;
        public float ConfigurationCellSizeOverride => _configurationCellSizeOverride;

        public bool ApplyIntegerTerrainHeights => _applyIntegerTerrainHeights;
        public TileWorldCreatorTerrainBuildMode TerrainBuildMode => _terrainBuildMode;
        public int TerrainHeightStep => Mathf.Max(1, _terrainHeightStep);
        public float TerrainHeightTrackingSeconds => Mathf.Max(0f, _terrainHeightTrackingSeconds);
        public bool NormalizeTerrainLevelsForTileWorldCreator => _normalizeTerrainLevelsForTileWorldCreator;
        public int WaterTerrainLevel => _waterTerrainLevel;
        public int ShoreTerrainLevel => _shoreTerrainLevel;
        public int LandTerrainLevel => _landTerrainLevel;
        public int HillTerrainLevel => _hillTerrainLevel;
        public int MaxTerrainLevel => Mathf.Max(_landTerrainLevel, _hillTerrainLevel, _maxTerrainLevel);
        public bool GenerateTerrainSideWalls => _generateTerrainSideWalls;
        public Material TerrainSideWallMaterial => _terrainSideWallMaterial;
        public Color TerrainSideWallColor => _terrainSideWallColor.a > 0.001f ? _terrainSideWallColor : new Color(_terrainSideWallColor.r, _terrainSideWallColor.g, _terrainSideWallColor.b, 1f);
        public bool GenerateTerrainSideWallsAtMapBorder => _generateTerrainSideWallsAtMapBorder;
        public bool CombineTerrainMeshesAfterHeightProjection => _combineTerrainMeshesAfterHeightProjection;
        public int TerrainMeshCombineClustersPerFrame => Mathf.Clamp(_terrainMeshCombineClustersPerFrame, 1, 64);
        public bool TerrainMeshCombineDeactivateSourceObjects => _terrainMeshCombineDeactivateSourceObjects;
        public bool ExpandSandShoreBand => _expandSandShoreBand;
        public string ShoreBandTileId => string.IsNullOrWhiteSpace(_shoreBandTileId) ? "sand-shore-band" : _shoreBandTileId;
    }
}
