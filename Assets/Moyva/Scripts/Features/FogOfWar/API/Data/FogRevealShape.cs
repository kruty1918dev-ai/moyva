namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Описує форму reveal-області, яку використовують gameplay fog state і preview/runtime visual updaters.
    /// </summary>
    public enum FogRevealShape
    {
        /// <summary>
        /// Коло в дискретній сітці з м'якою діагональною аппроксимацією.
        /// </summary>
        [UnityEngine.InspectorName("Піксельне коло")]
        PixelCircle = 0,

        /// <summary>
        /// Ромб за Manhattan distance.
        /// </summary>
        [UnityEngine.InspectorName("Ромб")]
        Diamond = 1,

        /// <summary>
        /// Квадрат за Chebyshev distance.
        /// </summary>
        [UnityEngine.InspectorName("Квадрат")]
        Square = 2,
    }
}
