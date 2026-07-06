using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTerrainSideWallMaterialService
    {
        Material Resolve(TileWorldCreatorTerrainSideWallState state, Material materialOverride, Color wallColor);
    }
}
