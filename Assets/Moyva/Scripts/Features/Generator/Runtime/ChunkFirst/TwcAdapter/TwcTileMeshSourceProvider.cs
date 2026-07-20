using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class TwcTileMeshSourceProvider : IResolvedTileMeshSource
    {
        private const string HeightDiagnosticsTag = "[MoyvaTileHeightDiag]";

        private readonly ITileWorldCreatorBuildEnvironment _environment;
        private readonly Dictionary<string, TilesBuildLayer> _buildLayerByGuid = new Dictionary<string, TilesBuildLayer>(System.StringComparer.Ordinal);
        private readonly Dictionary<GameObject, PrefabMeshTemplate> _meshTemplateByPrefab = new Dictionary<GameObject, PrefabMeshTemplate>();
        private readonly HashSet<string> _heightDiagnosticKeys = new HashSet<string>(System.StringComparer.Ordinal);

        public TwcTileMeshSourceProvider(ITileWorldCreatorBuildEnvironment environment)
        {
            _environment = environment;
        }

        public int CollectMeshSources(ResolvedTileComposition composition, List<TileMeshSource> results)
        {
            if (!composition.HasMainTerrain || results == null)
                return 0;

            var sample = composition.MainTerrain;
            TilesBuildLayer buildLayer = ResolveBuildLayer(sample);
            TilePreset preset = ResolvePreset(buildLayer, sample, composition.Cell, GlobalSeed.Current);
            if (buildLayer == null || preset == null)
                return 0;

            return preset.gridtype == TilePreset.GridType.dual
                ? CollectDualGridSources(composition, buildLayer, preset, results)
                : CollectNormalGridSource(composition, buildLayer, preset, results);
        }

        private int CollectNormalGridSource(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            List<TileMeshSource> results)
        {
            int configuration = BuildNormalConfiguration(composition);
            var tileType = ResolveTileType(preset.gridtype, configuration, out int yRotation);
            if (tileType == TilePreset.TileType.none)
                tileType = TilePreset.TileType.NRMGRD_fill;

            var scaleSign = TileConfigurations.NRMGRD_minusXScale_configurations.Contains(configuration)
                ? new Vector3(-1f, 1f, 1f)
                : Vector3.one;
            return TryAddMeshSource(
                composition,
                buildLayer,
                preset,
                tileType,
                new Vector2(composition.Cell.x, composition.Cell.y),
                yRotation,
                scaleSign,
                results)
                ? 1
                : 0;
        }

        private int CollectDualGridSources(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            List<TileMeshSource> results)
        {
            int before = results.Count;
            // TWC dual grid creates four half-offset fragments around a source cell.
            // We keep that shape selection, but assign each fragment to one source cell
            // so neighboring chunks do not duplicate border geometry.
            TryAddDualGridSource(
                composition,
                buildLayer,
                preset,
                new Vector2(-0.5f, -0.5f),
                composition.WestMatches,
                true,
                composition.SouthWestMatches,
                composition.SouthMatches,
                ShouldCurrentOwnDualFragment(composition.WestMatches, composition.SouthMatches, composition.SouthWestMatches),
                results);
            TryAddDualGridSource(
                composition,
                buildLayer,
                preset,
                new Vector2(0.5f, -0.5f),
                true,
                composition.EastMatches,
                composition.SouthMatches,
                composition.SouthEastMatches,
                ShouldCurrentOwnDualFragment(false, composition.SouthMatches, composition.SouthEastMatches),
                results);
            TryAddDualGridSource(
                composition,
                buildLayer,
                preset,
                new Vector2(-0.5f, 0.5f),
                composition.NorthWestMatches,
                composition.NorthMatches,
                composition.WestMatches,
                true,
                !composition.WestMatches,
                results);
            TryAddDualGridSource(
                composition,
                buildLayer,
                preset,
                new Vector2(0.5f, 0.5f),
                composition.NorthMatches,
                composition.NorthEastMatches,
                true,
                composition.EastMatches,
                true,
                results);
            return results.Count - before;
        }

        private void TryAddDualGridSource(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            Vector2 offset,
            bool topLeft,
            bool topRight,
            bool bottomLeft,
            bool bottomRight,
            bool ownedByCurrentCell,
            List<TileMeshSource> results)
        {
            if (!ownedByCurrentCell)
                return;

            int configuration = BuildDualConfiguration(topLeft, topRight, bottomLeft, bottomRight);
            var tileData = new BuildLayer.TileData
            {
                configuration = configuration,
                tilePosition = new Vector2(composition.Cell.x + offset.x, composition.Cell.y + offset.y)
            };
            var tileType = ResolveTileType(preset.gridtype, configuration, out int yRotation);
            if (tileType == TilePreset.TileType.none)
                return;

            TryAddMeshSource(
                composition,
                buildLayer,
                preset,
                tileType,
                tileData.tilePosition,
                yRotation,
                Vector3.one,
                results);
        }

        private bool TryAddMeshSource(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            TilePreset.TileType tileType,
            Vector2 tilePosition,
            int yRotation,
            Vector3 scaleSign,
            List<TileMeshSource> results)
        {
            var sample = composition.MainTerrain;
            GameObject prefab = preset.GetTile(tileType, out float xRotationOffset, out float yRotationOffset);
            if (prefab == null)
                return false;

            if (!TryGetMeshTemplate(prefab, out PrefabMeshTemplate template))
                return false;

            float cellSize = ResolveCellSize();
            Vector3 scale = template.PrefabScale;
            if (buildLayer.scaleTileToCellSize)
                scale *= cellSize;

            scale = new Vector3(
                scale.x * buildLayer.scaleOffset.x * scaleSign.x,
                scale.y * buildLayer.scaleOffset.y * scaleSign.y,
                scale.z * buildLayer.scaleOffset.z * scaleSign.z);

            Quaternion rotation = Quaternion.Euler(xRotationOffset, yRotation + yRotationOffset, 0f);
            float fallbackPlacementHeight = ResolvePlacementHeight(sample, buildLayer);
            float prefabTopOffset = ResolveTransformedBoundsTop(
                template.Mesh.bounds,
                Matrix4x4.TRS(Vector3.zero, rotation, scale) * template.ChildMatrix);
            float placementHeight = ResolveSurfaceAlignedPlacementHeight(
                sample.SurfaceHeight,
                fallbackPlacementHeight,
                prefabTopOffset);
            Vector3 position = new Vector3(
                tilePosition.x * cellSize,
                placementHeight,
                tilePosition.y * cellSize);
            Matrix4x4 rootMatrix = Matrix4x4.TRS(position, rotation, scale);
            Material[] materials = template.ResolveMaterials(preset.GetMaterialOverride());

            LogHeightPlacementOnce(
                composition,
                buildLayer,
                preset,
                tileType,
                prefab,
                fallbackPlacementHeight,
                prefabTopOffset,
                placementHeight);

            // Water stays a flat surface. Only solid terrain receives vertical
            // geometry down to the global world floor.
            float visibleBottomY = ShouldUseVerticalFill(
                sample,
                prefab,
                materials)
                ? 0f
                : float.NaN;
            var meshSource = new TileMeshSource(
                template.Mesh,
                materials,
                rootMatrix * template.ChildMatrix,
                visibleBottomY);
            if (!meshSource.IsValid)
                return false;

            results.Add(meshSource);
            return true;
        }

        private static bool ShouldUseVerticalFill(
            GraphTileLayerSample sample,
            GameObject prefab,
            Material[] materials)
        {
            // Graph metadata is the strongest signal: a water layer must never
            // become a solid column.
            if (IsWaterLike(sample.GraphLayerName)
                || IsWaterLike(sample.TileId)
                || IsWaterLike(sample.PresetId))
            {
                return false;
            }

            int waterMaterialCount = 0;
            int solidMaterialCount = 0;
            if (materials != null)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    if (material == null)
                        continue;

                    bool waterMaterial = IsWaterLike(material.name)
                        || IsWaterLike(material.shader != null
                            ? material.shader.name
                            : null);
                    if (waterMaterial)
                        waterMaterialCount++;
                    else
                        solidMaterialCount++;
                }
            }

            // Pure water meshes are excluded. Mixed shoreline meshes retain
            // vertical filling for their solid terrain part.
            if (waterMaterialCount > 0 && solidMaterialCount == 0)
                return false;

            // Fallback for water prefabs without assigned materials.
            if (waterMaterialCount == 0
                && solidMaterialCount == 0
                && IsWaterLike(prefab != null ? prefab.name : null))
            {
                return false;
            }

            return true;
        }

        private static bool IsWaterLike(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string normalized = value.Trim().ToLowerInvariant();
            return normalized.Contains("water")
                || normalized.Contains("ocean")
                || normalized.Contains("river")
                || normalized.Contains("lake")
                || normalized.Contains("liquid")
                || normalized.Contains("wasser")
                || normalized.Contains("вода")
                || normalized.Contains("водн")
                || normalized.Contains("озеро")
                || normalized.Contains("річк")
                || normalized.Contains("море")
                || normalized.Contains("океан");
        }

        private static float ResolvePlacementHeight(
            GraphTileLayerSample sample,
            TilesBuildLayer buildLayer)
        {
            float height = sample.Height;
            if (buildLayer == null)
                return height;

            height += buildLayer.layerYOffset;
            if (buildLayer.tileLayers != null
                && buildLayer.tileLayers.Count > 0
                && buildLayer.tileLayers[0] != null)
            {
                height += buildLayer.tileLayers[0].heightOffset;
            }

            return height;
        }

        internal static float ResolveSurfaceAlignedPlacementHeight(
            float expectedSurfaceHeight,
            float fallbackPlacementHeight,
            float prefabTopOffset)
        {
            if (!IsFinite(expectedSurfaceHeight) || !IsFinite(prefabTopOffset))
                return fallbackPlacementHeight;

            return expectedSurfaceHeight - prefabTopOffset;
        }

        internal static float ResolveTransformedBoundsTop(Bounds bounds, Matrix4x4 matrix)
        {
            Vector3 center = matrix.MultiplyPoint3x4(bounds.center);
            Vector3 extents = bounds.extents;
            Vector3 axisX = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
            float extentY = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
            return center.y + extentY;
        }

        private void LogHeightPlacementOnce(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            TilePreset.TileType tileType,
            GameObject prefab,
            float fallbackPlacementHeight,
            float prefabTopOffset,
            float placementHeight)
        {
            GraphTileLayerSample sample = composition.MainTerrain;
            string key =
                $"{sample.GraphLayerId}|{preset.GetInstanceID()}|{tileType}|{prefab.GetInstanceID()}";
            if (!_heightDiagnosticKeys.Add(key))
                return;

            float tileLayerOffset = buildLayer.tileLayers != null
                                    && buildLayer.tileLayers.Count > 0
                                    && buildLayer.tileLayers[0] != null
                ? buildLayer.tileLayers[0].heightOffset
                : 0f;
            float actualSurfaceHeight = placementHeight + prefabTopOffset;
            string mode = IsFinite(sample.SurfaceHeight) && IsFinite(prefabTopOffset)
                ? "surface-aligned"
                : "fallback";

            Debug.Log(
                $"{HeightDiagnosticsTag} Placement mode={mode} layer='{sample.GraphLayerName}' " +
                $"layerId='{sample.GraphLayerId}' cell={composition.Cell} tileType={tileType} " +
                $"preset='{preset.name}' prefab='{prefab.name}' layerHeight={sample.Height:0.###} " +
                $"expectedSurface={sample.SurfaceHeight:0.###} buildYOffset={buildLayer.layerYOffset:0.###} " +
                $"tileLayerOffset={tileLayerOffset:0.###} fallbackRootY={fallbackPlacementHeight:0.###} " +
                $"prefabTopOffset={prefabTopOffset:0.###} correctedRootY={placementHeight:0.###} " +
                $"actualSurface={actualSurfaceHeight:0.###} " +
                $"surfaceDelta={(actualSurfaceHeight - sample.SurfaceHeight):0.#####}");
        }

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);

        private bool TryGetMeshTemplate(GameObject prefab, out PrefabMeshTemplate template)
        {
            if (_meshTemplateByPrefab.TryGetValue(prefab, out template) && template.Mesh != null)
                return true;

            MeshFilter meshFilter = prefab.GetComponentInChildren<MeshFilter>(true);
            MeshRenderer renderer = meshFilter != null
                ? meshFilter.GetComponent<MeshRenderer>() ?? meshFilter.GetComponentInParent<MeshRenderer>(true)
                : null;
            if (meshFilter == null || meshFilter.sharedMesh == null || renderer == null)
            {
                template = null;
                return false;
            }

            template = new PrefabMeshTemplate(
                meshFilter.sharedMesh,
                prefab.transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix,
                prefab.transform.localScale,
                renderer.sharedMaterials);
            _meshTemplateByPrefab[prefab] = template;
            return true;
        }

        internal static int BuildNormalConfiguration(ResolvedTileComposition composition)
        {
            return (composition.NorthWestMatches ? 1 << 0 : 0)
                   | (composition.NorthMatches ? 1 << 1 : 0)
                   | (composition.NorthEastMatches ? 1 << 2 : 0)
                   | (composition.WestMatches ? 1 << 3 : 0)
                   | 1 << 4
                   | (composition.EastMatches ? 1 << 5 : 0)
                   | (composition.SouthWestMatches ? 1 << 6 : 0)
                   | (composition.SouthMatches ? 1 << 7 : 0)
                   | (composition.SouthEastMatches ? 1 << 8 : 0);
        }

        internal static int BuildDualConfiguration(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
            => (topLeft ? 1 << 0 : 0)
               | (topRight ? 1 << 1 : 0)
               | (bottomLeft ? 1 << 2 : 0)
               | (bottomRight ? 1 << 3 : 0);

        internal static TilePreset.TileType ResolveTileType(
            TilePreset.GridType gridType,
            int configuration,
            out int rotation)
        {
            rotation = ResolveRotation(gridType, configuration);

            return gridType == TilePreset.GridType.dual
                ? ResolveDualTileType(configuration)
                : ResolveNormalTileType(configuration);
        }

        private static TilePreset.TileType ResolveNormalTileType(int configuration)
        {
            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_cornerWay_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_cornerWay;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_cornerFill_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_cornerFill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_edgeWay_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_edgeWay;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_edgeFill_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_edgeFill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_fill_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_fill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_single_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_single;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_threeWay_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_threeWay;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_threeWayFill_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_threeWayFill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_threeCorner_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_threeCorner;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_deadEndWay_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_deadEnd;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_fourWay_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_fourWay;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_edgeCornerFill_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_edgeCornerFill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_doubleCorner_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_doubleCorner;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_interiorCorner_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_interiorCorner;
            }

            return TilePreset.TileType.none;
        }

        private static TilePreset.TileType ResolveDualTileType(int configuration)
        {
            if (ContainsConfiguration(
                    TileConfigurations.DUALGRD_corner_configurations,
                    configuration))
            {
                return TilePreset.TileType.DUALGRD_corner;
            }

            if (ContainsConfiguration(
                    TileConfigurations.DUALGRD_edge_configurations,
                    configuration))
            {
                return TilePreset.TileType.DUALGRD_edge;
            }

            if (ContainsConfiguration(
                    TileConfigurations.DUALGRD_fill_configurations,
                    configuration))
            {
                return TilePreset.TileType.DUALGRD_fill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.DUALGRD_interiorCorner_configurations,
                    configuration))
            {
                return TilePreset.TileType.DUALGRD_interiorCorner;
            }

            if (ContainsConfiguration(
                    TileConfigurations.DUALGRD_doubleInteriorCorner_configurations,
                    configuration))
            {
                return TilePreset.TileType.DUALGRD_doubleInteriorCorner;
            }

            return TilePreset.TileType.none;
        }

        private static int ResolveRotation(
            TilePreset.GridType gridType,
            int configuration)
        {
            if (gridType == TilePreset.GridType.dual)
            {
                if (ContainsConfiguration(TileConfigurations.rotation90Configurations, configuration))
                    return 90;
                if (ContainsConfiguration(TileConfigurations.rotation180Configurations, configuration))
                    return 180;
                if (ContainsConfiguration(TileConfigurations.rotation270Configurations, configuration))
                    return 270;

                return 0;
            }

            if (ContainsConfiguration(TileConfigurations.NRMGRD_rotation90_configurations, configuration))
                return 90;
            if (ContainsConfiguration(TileConfigurations.NRMGRD_rotation180_configurations, configuration))
                return 180;
            if (ContainsConfiguration(TileConfigurations.NRMGRD_rotation270_configurations, configuration))
                return 270;

            return 0;
        }

        private static bool ContainsConfiguration(
            IEnumerable<int> configurations,
            int configuration)
        {
            if (configurations == null)
                return false;

            foreach (int candidate in configurations)
            {
                if (candidate == configuration)
                    return true;
            }

            return false;
        }

        private static bool ShouldCurrentOwnDualFragment(bool west, bool south, bool southWest)
            => !west && !south && !southWest;

        private TilesBuildLayer ResolveBuildLayer(GraphTileLayerSample sample)
        {
            if (!string.IsNullOrWhiteSpace(sample.BuildLayerGuid)
                && _buildLayerByGuid.TryGetValue(sample.BuildLayerGuid, out var cached))
            {
                return cached;
            }

            var configuration = _environment?.Manager?.configuration;
            if (configuration?.buildLayerFolders == null)
                return null;

            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                {
                    if (folder.buildLayers[layerIndex] is not TilesBuildLayer layer)
                        continue;

                    bool matchesBuild = !string.IsNullOrWhiteSpace(sample.BuildLayerGuid)
                        && string.Equals(layer.guid, sample.BuildLayerGuid, System.StringComparison.Ordinal);
                    bool matchesBlueprint = !string.IsNullOrWhiteSpace(sample.BlueprintLayerGuid)
                        && (string.Equals(layer.assignedBlueprintLayerGuid, sample.BlueprintLayerGuid, System.StringComparison.Ordinal)
                            || string.Equals(layer.currentBlueprintLayer?.guid, sample.BlueprintLayerGuid, System.StringComparison.Ordinal));

                    if (!matchesBuild && !matchesBlueprint)
                        continue;

                    if (!string.IsNullOrWhiteSpace(layer.guid))
                        _buildLayerByGuid[layer.guid] = layer;
                    return layer;
                }
            }

            return null;
        }

        private static TilePreset ResolvePreset(TilesBuildLayer buildLayer, GraphTileLayerSample sample, Vector2Int cell, int seed)
        {
            if (buildLayer == null)
                return null;

            TilePreset preset = FindPreset(buildLayer.tilePresetsTop, sample.PresetId)
                                ?? FindPreset(buildLayer.tilePresetsMiddle, sample.PresetId)
                                ?? FindPreset(buildLayer.tilePresetsBottom, sample.PresetId);
            if (preset != null)
                return preset;

            return ChooseWeightedPreset(buildLayer.tilePresetsTop, sample, cell, seed, 0)
                   ?? ChooseWeightedPreset(buildLayer.tilePresetsMiddle, sample, cell, seed, 1)
                   ?? ChooseWeightedPreset(buildLayer.tilePresetsBottom, sample, cell, seed, 2);
        }

        private static TilePreset FindPreset(List<TilesBuildLayer.TilePresetSelection> selections, string presetId)
        {
            if (selections == null || string.IsNullOrWhiteSpace(presetId))
                return null;

            for (int i = 0; i < selections.Count; i++)
            {
                var preset = selections[i]?.preset;
                if (preset == null)
                    continue;

                if (string.Equals(preset.name, presetId, System.StringComparison.Ordinal)
                    || string.Equals(preset.tileId, presetId, System.StringComparison.Ordinal))
                    return preset;
            }

            return null;
        }

        private static TilePreset ChooseWeightedPreset(
            List<TilesBuildLayer.TilePresetSelection> selections,
            GraphTileLayerSample sample,
            Vector2Int cell,
            int seed,
            int tileLayerIndex)
        {
            if (selections == null)
                return null;

            float totalWeight = 0f;
            for (int i = 0; i < selections.Count; i++)
            {
                if (selections[i]?.preset != null)
                    totalWeight += Mathf.Max(0f, selections[i].weight);
            }

            if (totalWeight <= 0.0001f)
                return FirstPreset(selections);

            uint hash = ChunkFirstStableHash.TileVariant(
                seed,
                cell,
                sample.GraphLayerId,
                !string.IsNullOrWhiteSpace(sample.PresetId) ? sample.PresetId : sample.TileId,
                tileLayerIndex,
                "tile-preset");
            float roll = (hash / (float)uint.MaxValue) * totalWeight;
            float cumulative = 0f;
            for (int i = 0; i < selections.Count; i++)
            {
                var selection = selections[i];
                if (selection?.preset == null)
                    continue;

                cumulative += Mathf.Max(0f, selection.weight);
                if (roll <= cumulative)
                    return selection.preset;
            }

            return FirstPreset(selections);
        }

        private static TilePreset FirstPreset(List<TilesBuildLayer.TilePresetSelection> selections)
        {
            if (selections == null)
                return null;

            for (int i = 0; i < selections.Count; i++)
            {
                if (selections[i]?.preset != null)
                    return selections[i].preset;
            }

            return null;
        }

        private float ResolveCellSize()
        {
            var configuration = _environment?.Manager?.configuration;
            return configuration != null && configuration.cellSize > 0.0001f ? configuration.cellSize : 1f;
        }

        private sealed class PrefabMeshTemplate
        {
            private readonly Material[] _baseMaterials;
            private readonly Dictionary<Material, Material[]> _overrideMaterials = new Dictionary<Material, Material[]>();

            public PrefabMeshTemplate(
                Mesh mesh,
                Matrix4x4 childMatrix,
                Vector3 prefabScale,
                Material[] baseMaterials)
            {
                Mesh = mesh;
                ChildMatrix = childMatrix;
                PrefabScale = prefabScale;
                _baseMaterials = baseMaterials;
            }

            public Mesh Mesh { get; }
            public Matrix4x4 ChildMatrix { get; }
            public Vector3 PrefabScale { get; }

            public Material[] ResolveMaterials(Material overrideMaterial)
            {
                if (overrideMaterial == null)
                    return _baseMaterials;

                if (_overrideMaterials.TryGetValue(overrideMaterial, out Material[] materials))
                    return materials;

                int count = Mathf.Max(1, _baseMaterials?.Length ?? 0);
                materials = new Material[count];
                for (int i = 0; i < count; i++)
                    materials[i] = overrideMaterial;

                _overrideMaterials[overrideMaterial] = materials;
                return materials;
            }
        }
    }
}
