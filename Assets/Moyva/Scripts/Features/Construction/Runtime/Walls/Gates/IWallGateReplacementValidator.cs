using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IWallGateReplacementValidator
    {
        bool CanReplaceWallWithGate(Vector2Int position, string gateBuildingId, out string replacedWallId);
    }
}
