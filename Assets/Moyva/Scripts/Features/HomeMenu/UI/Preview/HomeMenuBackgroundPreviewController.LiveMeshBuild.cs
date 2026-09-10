using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Clouds.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public sealed partial class HomeMenuBackgroundPreviewController
    {
        private bool TryBuildLiveMeshPreview(
            MenuWorldPreviewData previewData,
            TileRegistrySO tileRegistry,
            MoyvaProjectSettingsSO projectSettings,
            out string failureReason)
        {
            failureReason = string.Empty;
            DestroyLiveMeshPreview();

            if (previewData?.BiomeMap == null || tileRegistry?.Definitions == null || projectSettings == null)
            {
                failureReason = "The generated terrain, tile registry, or project settings are missing.";
                return false;
            }

            var tileMeshCache = BuildTileLiveMeshCache(tileRegistry, _graphAsset);
            if (tileMeshCache.Count == 0)
            {
                failureReason = "The tile registry contains no usable mesh prefabs.";
                return false;
            }

            var objectMeshCache = projectSettings.HomeMenuPreviewIncludeObjects
                ? BuildObjectLiveMeshCache(_mapObjectRegistry)
                : new Dictionary<string, LivePreviewPrefabMesh>(StringComparer.OrdinalIgnoreCase);
            var buildingMeshCache = projectSettings.HomeMenuPreviewIncludeBuildings
                ? BuildBuildingLiveMeshCache(_buildingRegistry)
                : new Dictionary<string, LivePreviewPrefabMesh>(StringComparer.OrdinalIgnoreCase);

            var projection = GridProjectionFactory.Create(projectSettings);
            int previewLayer = Mathf.Clamp(projectSettings.HomeMenuPreviewLayer, 0, 31);
            int tileStride = ResolveLivePreviewTileStride(previewData, projectSettings);

            _livePreviewRoot = new GameObject("HomeMenuLiveMeshWorld")
            {
                hideFlags = HideFlags.DontSave
            };
            _livePreviewRoot.layer = previewLayer;

            var terrainSurfaceY = new Dictionary<int, float>();
            var builder = new LivePreviewMeshBuilder(_livePreviewRoot.transform, previewLayer, projectSettings, _livePreviewMeshes);

            try
            {
                int terrainCount = AddLiveTerrainMeshes(previewData, tileMeshCache, projection, projectSettings, builder, terrainSurfaceY, tileStride);
                if (terrainCount == 0)
                {
                    failureReason = "No generated terrain IDs match usable tile mesh prefabs.";
                    DestroyLiveMeshPreview();
                    return false;
                }

                int objectCount = AddLiveOverlayMeshes(previewData.ObjectMap, previewData, objectMeshCache, tileMeshCache, projection,
                    projectSettings, builder, terrainSurfaceY, tileStride);
                int buildingCount = _simulationSettings != null && _simulationSettings.enabled ? 0 :
                    AddLiveOverlayMeshes(previewData.BuildingMap, previewData, buildingMeshCache, tileMeshCache, projection,
                        projectSettings, builder, terrainSurfaceY, tileStride);
                int meshObjectCount = builder.Flush();
                if (meshObjectCount == 0)
                {
                    failureReason = "The preview meshes contain no usable material draws.";
                    DestroyLiveMeshPreview();
                    return false;
                }

                if (!ConfigureLivePreviewCamera(builder.WorldBounds, projectSettings, previewLayer, out failureReason))
                {
                    DestroyLiveMeshPreview();
                    return false;
                }

                ConfigureLivePreviewLight(projectSettings, previewLayer);
                return true;
            }
            catch (Exception exception)
            {
                failureReason = $"Live preview initialization failed: {exception.GetType().Name}: {exception.Message}";
                DestroyLiveMeshPreview();
                return false;
            }
        }

        private int AddLiveTerrainMeshes(
            MenuWorldPreviewData previewData,
            Dictionary<string, LivePreviewPrefabMesh> meshCache,
            IGridProjection projection,
            MoyvaProjectSettingsSO projectSettings,
            LivePreviewMeshBuilder builder,
            Dictionary<int, float> terrainSurfaceY,
            int tileStride)
        {
            int width = Mathf.Min(previewData.Width, previewData.BiomeMap.GetLength(0));
            int height = Mathf.Min(previewData.Height, previewData.BiomeMap.GetLength(1));
            int rendered = 0;

            for (int y = 0; y < height; y += tileStride)
            {
                for (int x = 0; x < width; x += tileStride)
                {
                    string id = NormalizePreviewId(previewData.BiomeMap[x, y]);
                    if (string.IsNullOrEmpty(id) || !meshCache.TryGetValue(id, out var prefabMesh))
                        continue;

                    float elevation = ResolvePreviewHeight(previewData, x, y, projectSettings);
                    Vector3 worldPosition = projection.GridToWorld(new Vector2Int(x, y), elevation, 0f);
                    builder.AddPrefab(prefabMesh, Matrix4x4.Translate(worldPosition));
                    terrainSurfaceY[ToPreviewIndex(x, y, previewData.Width)] = worldPosition.y + prefabMesh.Bounds.max.y;
                    rendered++;
                }
            }

            return rendered;
        }

        private int AddLiveOverlayMeshes(
            string[,] map,
            MenuWorldPreviewData previewData,
            Dictionary<string, LivePreviewPrefabMesh> meshCache,
            Dictionary<string, LivePreviewPrefabMesh> tileMeshCache,
            IGridProjection projection,
            MoyvaProjectSettingsSO projectSettings,
            LivePreviewMeshBuilder builder,
            Dictionary<int, float> terrainSurfaceY,
            int tileStride)
        {
            if (map == null || meshCache == null || meshCache.Count == 0)
                return 0;

            int width = Mathf.Min(previewData.Width, map.GetLength(0));
            int height = Mathf.Min(previewData.Height, map.GetLength(1));
            int rendered = 0;

            for (int y = 0; y < height; y += tileStride)
            {
                for (int x = 0; x < width; x += tileStride)
                {
                    string id = NormalizePreviewId(map[x, y]);
                    if (string.IsNullOrEmpty(id) || !meshCache.TryGetValue(id, out var prefabMesh))
                        continue;

                    float elevation = ResolvePreviewHeight(previewData, x, y, projectSettings);
                    Vector3 worldPosition = projection.GridToWorld(new Vector2Int(x, y), elevation, 0f);
                    float surfaceY = ResolveLiveTerrainSurfaceY(previewData, tileMeshCache, projection, projectSettings, terrainSurfaceY, x, y, elevation);
                    worldPosition.y = surfaceY - prefabMesh.Bounds.min.y + GridSurfacePlacementUtility.DefaultSurfaceClearance;
                    builder.AddPrefab(prefabMesh, Matrix4x4.Translate(worldPosition));
                    rendered++;
                }
            }

            return rendered;
        }

        private float ResolveLiveTerrainSurfaceY(
            MenuWorldPreviewData previewData,
            Dictionary<string, LivePreviewPrefabMesh> tileMeshCache,
            IGridProjection projection,
            MoyvaProjectSettingsSO projectSettings,
            Dictionary<int, float> terrainSurfaceY,
            int x,
            int y,
            float elevation)
        {
            int index = ToPreviewIndex(x, y, previewData.Width);
            if (terrainSurfaceY.TryGetValue(index, out float cachedSurfaceY))
                return cachedSurfaceY;

            Vector3 worldPosition = projection.GridToWorld(new Vector2Int(x, y), elevation, 0f);
            string tileId = previewData.BiomeMap != null
                && x >= 0 && y >= 0
                && x < previewData.BiomeMap.GetLength(0)
                && y < previewData.BiomeMap.GetLength(1)
                    ? NormalizePreviewId(previewData.BiomeMap[x, y])
                    : string.Empty;

            if (!string.IsNullOrEmpty(tileId) && tileMeshCache.TryGetValue(tileId, out var tileMesh))
                return worldPosition.y + tileMesh.Bounds.max.y;

            return worldPosition.y + GridSurfacePlacementUtility.DefaultSurfaceClearance;
        }

    }
}
