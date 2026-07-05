using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IWallPathfinder
    {
        IReadOnlyList<Vector2Int> BuildPath(Vector2Int startPosition, Vector2Int endPosition);
    }
}
