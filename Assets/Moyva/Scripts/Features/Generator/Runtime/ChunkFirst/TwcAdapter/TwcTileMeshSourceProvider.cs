using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class TwcTileMeshSourceProvider : IResolvedTileMeshSource
    {
        private readonly ITileWorldCreatorBuildEnvironment _environment;
        private readonly Dictionary<string, TilesBuildLayer> _buildLayerByGuid = new Dictionary<string, TilesBuildLayer>(System.StringComparer.Ordinal);

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

            return buildLayer.useDualGrid
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
            var tileData = new BuildLayer.TileData
            {
                configuration = configuration,
                tilePosition = composition.Cell
            };
            var tileType = buildLayer.GetTileType(configuration, tileData, out int yRotation);
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

            int configuration = BuildConfiguration(topLeft, topRight, bottomLeft, bottomRight);
            var tileData = new BuildLayer.TileData
            {
                configuration = configuration,
                tilePosition = new Vector2(composition.Cell.x + offset.x, composition.Cell.y + offset.y)
            };
            var tileType = buildLayer.GetTileType(configuration, tileData, out int yRotation);
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

            MeshFilter meshFilter = prefab.GetComponentInChildren<MeshFilter>(true);
            MeshRenderer renderer = meshFilter != null
                ? meshFilter.GetComponent<MeshRenderer>() ?? meshFilter.GetComponentInParent<MeshRenderer>(true)
                : null;
            if (meshFilter == null || meshFilter.sharedMesh == null || renderer == null)
                return false;

            float cellSize = ResolveCellSize();
            Vector3 scale = prefab.transform.localScale;
            if (buildLayer.scaleTileToCellSize)
                scale *= cellSize;

            scale = new Vector3(
                scale.x * buildLayer.scaleOffset.x * scaleSign.x,
                scale.y * buildLayer.scaleOffset.y * scaleSign.y,
                scale.z * buildLayer.scaleOffset.z * scaleSign.z);

            Vector3 position = new Vector3(
                tilePosition.x * cellSize,
                sample.Height + buildLayer.layerYOffset,
                tilePosition.y * cellSize);
            Quaternion rotation = Quaternion.Euler(xRotationOffset, yRotation + yRotationOffset, 0f);
            Matrix4x4 rootMatrix = Matrix4x4.TRS(position, rotation, scale);
            Matrix4x4 childMatrix = prefab.transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
            Material[] materials = ResolveMaterials(renderer, preset);

            var meshSource = new TileMeshSource(
                meshFilter.sharedMesh,
                materials,
                rootMatrix * childMatrix);
            if (!meshSource.IsValid)
                return false;

            results.Add(meshSource);
            return true;
        }

        private static Material[] ResolveMaterials(MeshRenderer renderer, TilePreset preset)
        {
            var overrideMaterial = preset.GetMaterialOverride();
            if (overrideMaterial == null)
                return renderer.sharedMaterials;

            int count = Mathf.Max(1, renderer.sharedMaterials?.Length ?? 0);
            var materials = new Material[count];
            for (int i = 0; i < count; i++)
                materials[i] = overrideMaterial;
            return materials;
        }

        private static int BuildNormalConfiguration(ResolvedTileComposition composition)
        {
            return BuildConfiguration(
                composition.NorthWestMatches,
                composition.NorthMatches,
                composition.NorthEastMatches,
                composition.WestMatches,
                true,
                composition.EastMatches,
                composition.SouthWestMatches,
                composition.SouthMatches,
                composition.SouthEastMatches);
        }

        private static int BuildConfiguration(params bool[] bits)
        {
            int configuration = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    configuration += 1 << i;
            }

            return configuration;
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
    }
}
