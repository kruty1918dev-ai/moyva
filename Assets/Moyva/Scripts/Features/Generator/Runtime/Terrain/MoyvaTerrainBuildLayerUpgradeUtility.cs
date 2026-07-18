using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public static class MoyvaTerrainBuildLayerUpgradeUtility
    {
        public static MoyvaTerrainHeightAwareTilesBuildLayer EnsureHeightAware(
            TileWorldCreatorManager manager,
            Configuration configuration,
            TilesBuildLayer current,
            string layerName)
        {
            if (current is MoyvaTerrainHeightAwareTilesBuildLayer heightAware)
                return heightAware;

            var replacement = CreateLayer(manager, layerName);

            CopyIdentity(current, replacement, layerName);
            ReplaceInConfiguration(configuration, current, replacement);
            Debug.Log($"[MoyvaTWCChunks] Upgraded TWC build layer '{layerName}' to height-aware merged terrain layer. preservedGuid='{replacement.guid}'.");
            return replacement;
        }

        public static MoyvaTerrainHeightAwareTilesBuildLayer CreateHeightAware(TileWorldCreatorManager manager, string layerName)
        {
            return CreateLayer(manager, layerName);
        }

        private static MoyvaTerrainHeightAwareTilesBuildLayer CreateLayer(
            TileWorldCreatorManager manager,
            string layerName)
        {
            if (CanUseManagerAssetApi(manager))
                return manager.AddNewBuildLayer<MoyvaTerrainHeightAwareTilesBuildLayer>(layerName);

            var layer = ScriptableObject.CreateInstance<MoyvaTerrainHeightAwareTilesBuildLayer>();
            layer.layerName = layerName;
            layer.hideFlags = HideFlags.HideAndDontSave;

            Configuration configuration = manager != null ? manager.configuration : null;
            if (configuration != null)
            {
                GraphCompilerLayerAssetUtility.EnsureBuildRootFolder(configuration);
                if (!configuration.buildLayerFolders[0].buildLayers.Contains(layer))
                    configuration.buildLayerFolders[0].buildLayers.Add(layer);
            }

            return layer;
        }

        private static bool CanUseManagerAssetApi(TileWorldCreatorManager manager)
        {
#if UNITY_EDITOR
            return manager != null
                   && GraphCompilerLayerAssetUtility.IsPersistentAsset(manager.configuration);
#else
            return manager != null;
#endif
        }

        private static void CopyIdentity(TilesBuildLayer source, MoyvaTerrainHeightAwareTilesBuildLayer target, string layerName)
        {
            if (target == null)
                return;

            target.layerName = source != null && !string.IsNullOrWhiteSpace(source.layerName) ? source.layerName : layerName;
            target.isEnabled = source?.isEnabled ?? true;
            target.guid = source != null && !string.IsNullOrWhiteSpace(source.guid) ? source.guid : target.guid;
            target.hierarchyLayerID = source != null && !string.IsNullOrWhiteSpace(source.hierarchyLayerID) ? source.hierarchyLayerID : target.hierarchyLayerID;
            target.assignedBlueprintLayerGuid = source?.assignedBlueprintLayerGuid;
            target.currentBlueprintLayer = source?.currentBlueprintLayer;
            target.foldoutState = source?.foldoutState ?? target.foldoutState;
            target.useMultiLayers = source?.useMultiLayers ?? target.useMultiLayers;
            MoyvaTerrainBuildLayerCopyUtility.Copy(source, target);
        }

        private static void ReplaceInConfiguration(Configuration configuration, TilesBuildLayer source, MoyvaTerrainHeightAwareTilesBuildLayer replacement)
        {
            if (configuration?.buildLayerFolders == null || source == null || replacement == null)
                return;

            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                folder.buildLayers.Remove(replacement);
                int sourceIndex = folder.buildLayers.IndexOf(source);
                if (sourceIndex >= 0)
                    folder.buildLayers[sourceIndex] = replacement;
            }
        }
    }
}
