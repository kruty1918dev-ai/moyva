using System;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Attributes;
using GiantGrey.TileWorldCreator.Components;
using GiantGrey.TileWorldCreator.Utilities;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    [Serializable]
    [BuildLayer("Moyva Height Aware Tiles", "Tiles.twc")]
    public sealed class MoyvaTerrainHeightAwareTilesBuildLayer : TilesBuildLayer
    {
        protected override void InstantiateTile(BuildLayer.TileData tileData, int clusterKey, int tileLayerIndex)
        {
            GameObject cluster = FindCluster(clusterKey);
            TilePreset preset = ResolvePreset(tileData, tileLayerIndex);
            if (preset == null)
                return;

            GameObject prefab = preset.GetTile(tileData.tileType, out tileData.xRotationOffset, out tileData.yRotationOffset);
            if (prefab == null)
                return;

            GameObject tile = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.Euler(tileData.xRotationOffset, tileData.yRotation + tileData.yRotationOffset, 0f));
            ApplyMaterial(tile, preset);
            ApplyScale(tile, tileData);
            tile.transform.SetParent(cluster.transform, false);
            tile.transform.localPosition = ResolveLocalPosition(tileData, tileLayerIndex, cluster);
        }

        private TilePreset ResolvePreset(BuildLayer.TileData tileData, int tileLayerIndex)
        {
            int x = Mathf.FloorToInt(tileData.tilePosition.x * 1000f);
            int y = Mathf.FloorToInt(tileData.tilePosition.y * 1000f);
            uint seed = Unity.Mathematics.math.hash(new Unity.Mathematics.int3(
                x,
                y,
                configuration.useGlobalRandomSeed ? configuration.globalRandomSeed : (int)configuration.currentRandomSeed));
            if (seed == 0)
                seed = 1;

            var random = new Unity.Mathematics.Random(seed);
            return GetRandomTilePreset(tileData, tileLayerIndex, ref random);
        }

        private static void ApplyMaterial(GameObject tile, TilePreset preset)
        {
            Material material = preset.GetMaterialOverride();
            if (material == null)
                return;

            var renderer = tile.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.material = material;
        }

        private void ApplyScale(GameObject tile, BuildLayer.TileData tileData)
        {
            Vector3 scale = tile.transform.localScale;
            float cellScale = scaleTileToCellSize ? configuration.cellSize : 1f;
            tile.transform.localScale = new Vector3(
                scale.x * scaleOffset.x * cellScale,
                scale.y * scaleOffset.y * cellScale,
                scale.z * scaleOffset.z * cellScale);

            if (!useDualGrid && TileConfigurations.NRMGRD_minusXScale_configurations.Contains(tileData.configuration))
                tile.transform.localScale = new Vector3(-tile.transform.localScale.x, tile.transform.localScale.y, tile.transform.localScale.z);
        }

        private Vector3 ResolveLocalPosition(BuildLayer.TileData tileData, int tileLayerIndex, GameObject cluster)
        {
            float layerHeight = currentBlueprintLayer != null ? currentBlueprintLayer.defaultLayerHeight : 0f;
            float tileLayerOffset = tileLayers != null && tileLayerIndex >= 0 && tileLayerIndex < tileLayers.Count && tileLayers[tileLayerIndex] != null
                ? tileLayers[tileLayerIndex].heightOffset
                : 0f;
            float terrainHeight = ResolveTerrainHeight(tileData, cluster);
            return new Vector3(
                tileData.tilePosition.x * configuration.cellSize,
                layerHeight + layerYOffset + tileLayerOffset + terrainHeight,
                tileData.tilePosition.y * configuration.cellSize);
        }

        private float ResolveTerrainHeight(BuildLayer.TileData tileData, GameObject cluster)
        {
            var context = cluster != null ? cluster.GetComponentInParent<MoyvaTerrainHeightContext>() : null;
            if (context != null && context.TryGetTileHeight(tileData.tilePosition, useDualGrid, tileData.contributingCells, out float height))
                return height;

            return 0f;
        }
    }
}
