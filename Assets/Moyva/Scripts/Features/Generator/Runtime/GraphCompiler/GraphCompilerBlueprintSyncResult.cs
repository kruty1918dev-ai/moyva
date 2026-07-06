using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public sealed class GraphCompilerBlueprintSyncResult
    {
        public readonly List<CompiledLayerMap> CompiledLayers = new();
        public readonly List<GeneratorLayerDefinition> OrderedLayers = new();
        public readonly Dictionary<string, string> BlueprintGuidByGraphLayerId = new();
        public readonly Dictionary<string, BlueprintLayer> BlueprintByGraphLayerId = new();
        public readonly HashSet<string> UsedLayerGuids = new();
        public readonly List<BlueprintLayer> ExistingLayers = new();
    }
}
