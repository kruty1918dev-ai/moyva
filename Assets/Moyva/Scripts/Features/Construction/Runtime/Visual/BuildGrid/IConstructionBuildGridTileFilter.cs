using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBuildGridTileFilter
    {
        bool ShouldRender(Vector2Int position);
    }
}
