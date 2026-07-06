using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphCompilerLayerAssetUtility
    {
        public static void EnsureBlueprintRootFolder(Configuration config)
        {
            config.blueprintLayerFolders ??= new List<BlueprintLayerFolder>();
            if (config.blueprintLayerFolders.Count == 0)
                config.blueprintLayerFolders.Add(new BlueprintLayerFolder("Root"));
        }

        public static void EnsureBuildRootFolder(Configuration config)
        {
            config.buildLayerFolders ??= new List<BuildLayerFolder>();
            if (config.buildLayerFolders.Count == 0)
                config.buildLayerFolders.Add(new BuildLayerFolder("Root"));
        }

        public static void PrepareLayerAsset(Configuration config, ScriptableObject layer, string layerName)
        {
            layer.name = layerName;
            if (layer is BuildLayer buildLayer)
                buildLayer.layerName = layerName;
            if (layer is BlueprintLayer blueprintLayer)
                blueprintLayer.layerName = layerName;

            layer.hideFlags = IsPersistentAsset(config) ? HideFlags.HideInHierarchy : HideFlags.HideAndDontSave;
#if UNITY_EDITOR
            if (IsPersistentAsset(config))
                UnityEditor.AssetDatabase.AddObjectToAsset(layer, config);
#endif
        }

        public static int CountBuildLayers(Configuration config)
        {
            if (config?.buildLayerFolders == null)
                return 0;

            int count = 0;
            foreach (var folder in config.buildLayerFolders)
                count += folder?.buildLayers?.Count ?? 0;
            return count;
        }

        public static bool IsPersistentAsset(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            return obj != null && UnityEditor.AssetDatabase.Contains(obj);
#else
            return false;
#endif
        }
    }
}
