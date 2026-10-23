using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>
    /// Resolves the grid position the startup auto-frame should focus on. The
    /// implementation knows which spawn assignment belongs to the local player;
    /// without one the framer falls back to the first assignment. Optional —
    /// single-player compositions may omit the binding.
    /// </summary>
    public interface ICameraStartupFocusResolver
    {
        bool TryResolveStartupFocusGridPosition(out Vector2Int position);
    }
}
