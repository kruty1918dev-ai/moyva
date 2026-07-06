using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphCompilerTileBuildLayerSyncService
    {
        void Sync(GraphAsset graph, Configuration config, TileWorldCreatorManager manager,
            GraphCompilerBlueprintSyncResult blueprintSync, ISet<string> skippedLayerIds);
    }

    internal sealed class GraphCompilerTileBuildLayerSyncService : IGraphCompilerTileBuildLayerSyncService
    {
        private readonly IGraphCompilerTileBuildLayerLookup _lookup;

        public GraphCompilerTileBuildLayerSyncService(IGraphCompilerTileBuildLayerLookup lookup)
        {
            _lookup = lookup;
        }

        public void Sync(GraphAsset graph, Configuration config, TileWorldCreatorManager manager,
            GraphCompilerBlueprintSyncResult blueprintSync, ISet<string> skippedLayerIds)
        {
            if (graph == null || config == null || manager == null || blueprintSync == null)
                return;

            GraphCompilerLayerAssetUtility.EnsureBuildRootFolder(config);
            var folder = config.buildLayerFolders[0];
            folder.buildLayers ??= new List<BuildLayer>();
            var orderedBuildLayers = new List<BuildLayer>();

            foreach (var layerDef in blueprintSync.OrderedLayers)
                SyncLayer(graph, config, manager, blueprintSync, skippedLayerIds, folder, orderedBuildLayers, layerDef);

            PreserveGeneratedObjectLayers(folder, orderedBuildLayers);
            RemoveStaleGraphTileBuildLayers(folder, orderedBuildLayers);
            folder.buildLayers = orderedBuildLayers;
        }

        private void SyncLayer(GraphAsset graph, Configuration config, TileWorldCreatorManager manager,
            GraphCompilerBlueprintSyncResult blueprintSync, ISet<string> skippedLayerIds, BuildLayerFolder folder,
            List<BuildLayer> orderedBuildLayers, GeneratorLayerDefinition layerDef)
        {
            if (layerDef == null || skippedLayerIds != null && skippedLayerIds.Contains(layerDef.Id))
                return;

            var tileNodes = TileSettingsNode.GetNodesForLayer(graph, layerDef.Id);
            if (!tileNodes.Any(node => node != null && node.HasRenderableTileOutput))
            {
                layerDef.BuildLayerKey = string.Empty;
                return;
            }

            blueprintSync.BlueprintByGraphLayerId.TryGetValue(layerDef.Id, out var blueprint);
            string blueprintGuid = blueprint?.guid ?? layerDef.BlueprintLayerGuid;
            var buildLayer = _lookup.Find(config, layerDef.BuildLayerKey, blueprintGuid)
                             ?? FindByName(folder, layerDef.Name, orderedBuildLayers);

            buildLayer = buildLayer == null
                ? MoyvaTerrainBuildLayerUpgradeUtility.CreateHeightAware(manager, layerDef.Name)
                : MoyvaTerrainBuildLayerUpgradeUtility.EnsureHeightAware(manager, config, buildLayer, layerDef.Name);
            TileSettingsNode.ApplyNodesToBuildLayer(buildLayer, tileNodes, config, blueprint, layerDef);

            if (!string.IsNullOrWhiteSpace(buildLayer.guid))
                layerDef.BuildLayerKey = buildLayer.guid;
            if (!orderedBuildLayers.Contains(buildLayer))
                orderedBuildLayers.Add(buildLayer);
        }

        private static TilesBuildLayer FindByName(BuildLayerFolder folder, string layerName, List<BuildLayer> orderedBuildLayers)
        {
            return folder.buildLayers
                .OfType<TilesBuildLayer>()
                .FirstOrDefault(layer => layer != null && layer.layerName == layerName && !orderedBuildLayers.Contains(layer));
        }

        private static void PreserveGeneratedObjectLayers(BuildLayerFolder folder, List<BuildLayer> orderedBuildLayers)
        {
            foreach (var objectLayer in folder.buildLayers.Where(TWCObjectPlacementAdapter.IsGeneratedObjectLayer).ToList())
            {
                if (objectLayer != null && !orderedBuildLayers.Contains(objectLayer))
                    orderedBuildLayers.Add(objectLayer);
            }
        }

        private static void RemoveStaleGraphTileBuildLayers(BuildLayerFolder folder, List<BuildLayer> orderedBuildLayers)
        {
            foreach (var stale in folder.buildLayers.ToList())
            {
                if (stale == null || orderedBuildLayers.Contains(stale) || TWCObjectPlacementAdapter.IsGeneratedObjectLayer(stale))
                    continue;

                folder.buildLayers.Remove(stale);
#if UNITY_EDITOR
                if (UnityEditor.AssetDatabase.Contains(stale))
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(stale);
                else
                    Object.DestroyImmediate(stale);
#else
                Object.Destroy(stale);
#endif
            }
        }
    }
}
