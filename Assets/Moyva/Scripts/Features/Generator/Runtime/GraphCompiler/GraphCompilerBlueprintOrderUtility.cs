using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphCompilerBlueprintOrderUtility
    {
        public static void Reorder(
            Configuration config,
            List<GeneratorLayerDefinition> orderedLayers,
            Dictionary<string, BlueprintLayer> blueprintByGraphLayerId)
        {
            if (config?.blueprintLayerFolders == null || config.blueprintLayerFolders.Count == 0)
                return;

            var root = config.blueprintLayerFolders[0];
            if (root == null)
                return;

            root.blueprintLayers ??= new List<BlueprintLayer>();
            var orderedBlueprints = new List<BlueprintLayer>();
            foreach (var layerDef in orderedLayers)
            {
                if (layerDef != null
                    && blueprintByGraphLayerId.TryGetValue(layerDef.Id, out var blueprint)
                    && blueprint != null
                    && !orderedBlueprints.Contains(blueprint))
                    orderedBlueprints.Add(blueprint);
            }

            var graphBlueprints = new HashSet<BlueprintLayer>(orderedBlueprints);
            var remainder = new List<BlueprintLayer>();
            foreach (var existing in root.blueprintLayers)
            {
                if (existing != null && !graphBlueprints.Contains(existing))
                    remainder.Add(existing);
            }

            foreach (var folder in config.blueprintLayerFolders)
                folder?.blueprintLayers?.RemoveAll(layer => layer != null && graphBlueprints.Contains(layer));

            foreach (var existing in remainder)
            {
                if (!orderedBlueprints.Contains(existing))
                    orderedBlueprints.Add(existing);
            }

            root.blueprintLayers = orderedBlueprints;
        }
    }
}
