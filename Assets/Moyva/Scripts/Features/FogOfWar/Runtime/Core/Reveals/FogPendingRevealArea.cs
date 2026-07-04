using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Відкладена reveal-операція, яку можна застосувати після ініціалізації або resize fog map.
    /// </summary>
    internal readonly struct FogPendingRevealArea
    {
        /// <summary>
        /// Створює опис відкладеної reveal-операції.
        /// </summary>
        /// <param name="center">Центр області.</param>
        /// <param name="radius">Радіус області.</param>
        /// <param name="shape">Форма reveal.</param>
        /// <param name="keepVisible">Чи має область стати постійно visible.</param>
        /// <param name="visibleAreaId">Необов'язковий id постійної visible області.</param>
        public FogPendingRevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId)
        {
            Center = center;
            Radius = radius;
            Shape = shape;
            KeepVisible = keepVisible;
            VisibleAreaId = visibleAreaId;
        }

        /// <summary>
        /// Центр reveal області.
        /// </summary>
        public Vector2Int Center { get; }

        /// <summary>
        /// Радіус reveal області.
        /// </summary>
        public int Radius { get; }

        /// <summary>
        /// Форма reveal області.
        /// </summary>
        public FogRevealShape Shape { get; }

        /// <summary>
        /// Чи повинна область лишатися visible після застосування.
        /// </summary>
        public bool KeepVisible { get; }

        /// <summary>
        /// Необов'язковий id постійної visible області.
        /// </summary>
        public string VisibleAreaId { get; }
    }
}
