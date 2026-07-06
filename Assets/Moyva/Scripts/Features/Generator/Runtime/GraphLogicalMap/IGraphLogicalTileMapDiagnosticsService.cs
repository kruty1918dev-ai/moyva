using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphLogicalTileMapDiagnosticsService
    {
        void EmitAndCompare(string source, GraphAsset graph, int seed, GraphLogicalTileMap map, Object context = null);
    }
}
