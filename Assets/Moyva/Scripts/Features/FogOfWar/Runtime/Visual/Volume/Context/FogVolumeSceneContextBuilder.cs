using System.Collections.Generic;
using System.Diagnostics;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{

    /// <summary>
    /// Збирає <see cref="FogWorldVisualContext"/> з наявного TWC world source у сцені.
    /// Якщо source відсутній, переходить на безпечний preview fallback.
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    internal sealed class FogVolumeSceneContextBuilder : IFogVolumeSceneContextBuilder
    {
        /// <summary>
        /// Формує visual context на основі існуючої scene grid та generated world height data.
        /// </summary>
        /// <param name="host">Host-компонент, для якого будується контекст.</param>
        /// <returns>Готовий visual context для preview або runtime build.</returns>
        public FogWorldVisualContext BuildContext(IFogVolumeSceneContextHost host)
        {
            if (host == null)
                return CreateFallbackContext(null, null);

            var sourceManager = FindSceneSourceTileWorldCreatorManager(host.TileWorldCreatorManager);
            var sourceConfiguration = sourceManager != null ? sourceManager.configuration : null;
            if (sourceConfiguration == null)
                return CreateFallbackContext(host.Settings, host.transform);

            int width = Mathf.Max(1, sourceConfiguration.width);
            int height = Mathf.Max(1, sourceConfiguration.height);
            float cellSize = Mathf.Max(0.001f, sourceConfiguration.cellSize);
            Bounds bounds = CreateGridBounds(sourceManager.transform, width, height, cellSize);
            float[,] heightMap = BuildHeightMap(sourceConfiguration, width, height);

            return new FogWorldVisualContext(
                width,
                height,
                GridTopology.Orthogonal,
                GridProjectionMode.Orthographic3D,
                GridRenderMode.Mesh3D,
                GridNeighborhoodMode.Moore8,
                cellSize,
                true,
                bounds,
                heightMap,
                null);
        }

        private static FogWorldVisualContext CreateFallbackContext(FogOfWarSettings settings, Transform hostTransform)
        {
            int width = Mathf.Max(1, settings != null ? settings.Volume.PreviewFallbackWidth : 16);
            int height = Mathf.Max(1, settings != null ? settings.Volume.PreviewFallbackHeight : 16);
            Bounds bounds = CreateGridBounds(hostTransform, width, height, 1f);
            return new FogWorldVisualContext(
                width,
                height,
                GridTopology.Orthogonal,
                GridProjectionMode.Orthographic3D,
                GridRenderMode.Mesh3D,
                GridNeighborhoodMode.Moore8,
                1f,
                true,
                bounds,
                null,
                null);
        }

        private static TileWorldCreatorManager FindSceneSourceTileWorldCreatorManager(TileWorldCreatorManager ownManager)
        {
            var managers = Object.FindObjectsByType<TileWorldCreatorManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < managers.Length; i++)
            {
                var manager = managers[i];
                if (manager != null && manager != ownManager && manager.configuration != null)
                    return manager;
            }

            return null;
        }

        private static float[,] BuildHeightMap(Configuration configuration, int width, int height)
        {
            var heightMap = new float[width, height];
            if (configuration?.blueprintLayerFolders == null)
                return heightMap;

            for (int folderIndex = 0; folderIndex < configuration.blueprintLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.blueprintLayerFolders[folderIndex];
                if (folder?.blueprintLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.blueprintLayers.Count; layerIndex++)
                {
                    var layer = folder.blueprintLayers[layerIndex];
                    if (layer == null)
                        continue;

                    float layerHeight = layer.defaultLayerHeight + ResolveBuildLayerYOffset(configuration, layer.guid);
                    var positions = layer.GetAllCellPositions(new HashSet<Vector2>());
                    foreach (var position in positions)
                    {
                        int x = Mathf.RoundToInt(position.x);
                        int y = Mathf.RoundToInt(position.y);
                        if (x >= 0 && x < width && y >= 0 && y < height)
                            heightMap[x, y] = Mathf.Max(heightMap[x, y], layerHeight);
                    }
                }
            }

            return heightMap;
        }

        private static float ResolveBuildLayerYOffset(Configuration configuration, string blueprintGuid)
        {
            if (configuration?.buildLayerFolders == null || string.IsNullOrEmpty(blueprintGuid))
                return 0f;

            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                {
                    if (folder.buildLayers[layerIndex] is TilesBuildLayer buildLayer
                        && buildLayer.assignedBlueprintLayerGuid == blueprintGuid)
                    {
                        return buildLayer.layerYOffset;
                    }
                }
            }

            return 0f;
        }

        private static Bounds CreateGridBounds(Transform root, int width, int height, float cellSize)
        {
            float halfCell = cellSize * 0.5f;
            Vector3 localMin = new Vector3(-halfCell, 0f, -halfCell);
            Vector3 localMax = new Vector3(
                (width - 1) * cellSize + halfCell,
                1f,
                (height - 1) * cellSize + halfCell);

            Vector3 worldMin = root != null ? root.TransformPoint(localMin) : localMin;
            Vector3 worldMax = worldMin;
            Encapsulate(root != null ? root.TransformPoint(new Vector3(localMax.x, localMin.y, localMin.z)) : new Vector3(localMax.x, localMin.y, localMin.z), ref worldMin, ref worldMax);
            Encapsulate(root != null ? root.TransformPoint(new Vector3(localMin.x, localMax.y, localMin.z)) : new Vector3(localMin.x, localMax.y, localMin.z), ref worldMin, ref worldMax);
            Encapsulate(root != null ? root.TransformPoint(new Vector3(localMin.x, localMin.y, localMax.z)) : new Vector3(localMin.x, localMin.y, localMax.z), ref worldMin, ref worldMax);
            Encapsulate(root != null ? root.TransformPoint(localMax) : localMax, ref worldMin, ref worldMax);
            return new Bounds((worldMin + worldMax) * 0.5f, worldMax - worldMin);
        }

        private static void Encapsulate(Vector3 point, ref Vector3 min, ref Vector3 max)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
