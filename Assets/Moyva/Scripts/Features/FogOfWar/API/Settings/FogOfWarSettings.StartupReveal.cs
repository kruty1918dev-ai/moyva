using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public partial class FogOfWarSettings
    {
        /// <summary>
        /// Чи слід FogOfWarService відкривати fallback стартову область, якщо bootstrap reveal не надійшов.
        /// </summary>
        [TitleGroup("Startup Reveal")]
        [Tooltip("If no bootstrap startup reveal arrives for a fresh non-load world, FogOfWarService opens a random visible start area so the map never starts fully black.")]
        public bool EnableStartupFallbackReveal = true;

        /// <summary>
        /// Радіус fallback стартового reveal, який service застосує у крайньому випадку.
        /// </summary>
        [TitleGroup("Startup Reveal")]
        [Tooltip("Radius used by FogOfWarService when bootstrap did not provide a startup reveal.")]
        [MinValue(1)]
        public int StartupFallbackRevealRadius = 15;

        /// <summary>
        /// Мінімальний відступ fallback стартової точки від краю карти.
        /// </summary>
        [TitleGroup("Startup Reveal")]
        [Tooltip("Minimum margin from map edge for the fallback random start point.")]
        [MinValue(0)]
        public int StartupFallbackMinMarginFromBorder = 5;

        /// <summary>
        /// Додатковий відносний margin для випадкової fallback стартової точки.
        /// </summary>
        [TitleGroup("Startup Reveal")]
        [Tooltip("Additional map-relative margin for the fallback random start point.")]
        [Range(0f, 0.45f)]
        public float StartupFallbackRelativeMarginFactor = 0.1667f;

        /// <summary>
        /// Форма fallback стартового reveal.
        /// </summary>
        [TitleGroup("Startup Reveal")]
        [Tooltip("Shape used by FogOfWarService fallback startup reveal.")]
        public FogRevealShape StartupFallbackRevealShape = FogRevealShape.PixelCircle;
    }
}
