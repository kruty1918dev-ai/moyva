using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphLogicalTileMapDiagnostics
    {
        public static void EmitAndCompare(
            string source,
            GraphAsset graph,
            int seed,
            GraphLogicalTileMap map,
            Object context = null)
        {
            GraphLogicalTileMapServiceFactory.CreateDiagnostics()
                .EmitAndCompare(source, graph, seed, map, context);
        }
    }
}
