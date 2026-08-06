using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Explicitly marks this object hierarchy as a valid fog surface.
    /// Use on generated terrain or transparent water when automatic
    /// name/shader classification is insufficient.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FogSurfaceContributor
        : MonoBehaviour
    {
    }
}
