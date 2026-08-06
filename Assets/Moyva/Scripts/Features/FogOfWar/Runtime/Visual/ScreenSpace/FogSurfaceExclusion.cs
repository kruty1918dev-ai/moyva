using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Explicitly prevents this object hierarchy from participating in
    /// FogSurfaceDepth. Intended for construction grids, previews,
    /// highlights and other world overlays.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FogSurfaceExclusion
        : MonoBehaviour
    {
    }
}
