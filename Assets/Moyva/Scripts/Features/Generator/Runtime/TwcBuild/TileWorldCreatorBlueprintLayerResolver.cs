using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorBlueprintLayerResolver
    {
        bool TryResolve(Configuration configuration, TileWorldCreatorIdMappingSO.LayerMapping mapping, out string layerGuid);
    }

    internal sealed class TileWorldCreatorBlueprintLayerResolver : ITileWorldCreatorBlueprintLayerResolver
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private readonly HashSet<string> _loggedMissingLayers = new();

        public bool TryResolve(Configuration configuration, TileWorldCreatorIdMappingSO.LayerMapping mapping, out string layerGuid)
        {
            layerGuid = null;
            if (configuration == null || mapping == null)
                return false;

            if (!string.IsNullOrWhiteSpace(mapping.BlueprintLayerGuid)
                && configuration.GetBlueprintLayerByGuid(mapping.BlueprintLayerGuid) != null)
            {
                layerGuid = mapping.BlueprintLayerGuid;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(mapping.BlueprintLayerName))
            {
                string resolvedGuid = configuration.GetBlueprintLayerGuid(mapping.BlueprintLayerName);
                if (!string.IsNullOrWhiteSpace(resolvedGuid))
                {
                    layerGuid = resolvedGuid;
                    return true;
                }
            }

            string logKey = $"{mapping.IdPattern}:{mapping.BlueprintLayerGuid}:{mapping.BlueprintLayerName}";
            if (_loggedMissingLayers.Add(logKey))
                Debug.LogWarning($"{LogTag} Cannot resolve TWC blueprint layer for ID pattern '{mapping.IdPattern}'.");

            return false;
        }
    }
}
