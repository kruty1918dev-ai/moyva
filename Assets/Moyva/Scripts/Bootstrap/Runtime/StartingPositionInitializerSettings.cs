using System;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    [Serializable]
    public sealed class StartingPositionInitializerSettings
    {
        [Header("Start Position")]
        [Tooltip("Мінімальний відступ стартової точки від краю мапи в тайлах.\nБільше значення зменшує шанс старту біля краю і робить старт безпечнішим.")]
        [Min(0)]
        public int minMarginFromBorder = 5;

        [Tooltip("Додатковий відступ від краю як частка від меншої сторони мапи.\nНаприклад 0.1667 ≈ 1/6 мапи. Підсумковий відступ = max(minMarginFromBorder, відносний відступ).")]
        [Range(0f, 0.45f)]
        public float relativeMarginFactor = 0.1667f;

        [Header("Fog Reveal Shape")]
        [Tooltip("Радіус піксельного кола стартової розвіданої зони в тайлах.\nАлгоритм бере випадкову стартову точку і відкриває всі тайли всередині кола.")]
        [Min(1)]
        public int revealedCircleRadius = 15;

        [Tooltip("Якщо увімкнено, ядро підтримується повністю видимим (без туману)\nчерез службовий стартовий огляд.\nЯкщо вимкнено, ядро буде лише розвіданим і з часом стане сірим без юнітів.")]
        public bool keepCoreFullyVisible = true;

        [Tooltip("Радіус службового стартового огляду.\n0 або менше = використовувати revealedCircleRadius.")]
        [Min(0)]
        public int coreVisibleRadiusOverride = 0;

        [Header("Camera")]
        [Tooltip("Позиція Z для різкого перенесення камери в стартову точку.\nДля 2D зазвичай -10, щоб камера залишалась на правильній глибині.")]
        public float cameraZ = -10f;
    }
}
