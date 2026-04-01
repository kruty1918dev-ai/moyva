using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Надсилається VisibilityService коли тайл змінює стан видимості:
    /// з туману (лічильник 0 → 1) або назад у туман (лічильник 1 → 0).
    /// </summary>
    public struct OnVisibilityChangedSignal
    {
        public Vector2Int Position;

        /// <summary>
        /// true  — тайл тепер видимий (лічильник став 1)
        /// false — тайл знову в тумані (лічильник став 0)
        /// </summary>
        public bool IsVisible;
    }
}
