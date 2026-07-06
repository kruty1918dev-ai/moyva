using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphLogicalTileMapMetricsService
    {
        Dictionary<string, int> CountValues(string[,] grid, out int emptyCount, out int occupiedCount);
        ulong ComputeHash(string[,] grid);
        int CountRegions(string[,] grid, Func<string, bool> include);
        int CountEdgeTransitions(string[,] grid);
    }
}
