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
        [Tooltip("Радіус ядра стартової зони в тайлах.\nУ ядрі тайли найчастіше відкриваються повністю (без туману).")]
        [Min(1)]
        public int innerRadius = 8;

        [Tooltip("Якщо увімкнено, ядро підтримується повністю видимим (без туману)\nчерез службовий стартовий огляд.\nЯкщо вимкнено, ядро буде лише розвіданим і з часом стане сірим без юнітів.")]
        public bool keepCoreFullyVisible = true;

        [Tooltip("Радіус службового стартового огляду.\n0 або менше = використовувати innerRadius.")]
        [Min(0)]
        public int coreVisibleRadiusOverride = 0;

        [Tooltip("Зовнішній радіус стартової зони в тайлах.\nМіж innerRadius і outerRadius формується напіврозвідана сіра периферія.")]
        [Min(1)]
        public int outerRadius = 15;

        [Tooltip("Додатковий буфер за outerRadius, де алгоритм ще може обробляти тайли.\nДозволяє зробити край зони менш різким і більш природним.")]
        [Min(0f)]
        public float outerPadding = 4f;

        [Header("Noise")]
        [Tooltip("Масштаб Perlin-шуму для форми плям розвідки.\nМенше значення = більші плавні плями, більше значення = дрібніша і хаотичніша текстура.")]
        [Min(0.01f)]
        public float noiseScale = 0.35f;

        [Tooltip("Діапазон випадкового зсуву шуму по X/Y на кожен старт гри.\nЧим більше значення, тим менш повторюваний візерунок розкриття туману.")]
        [Min(0f)]
        public float noiseOffsetRange = 200f;

        [Header("Reveal Probability")]
        [Tooltip("Початкова ймовірність розкриття на межі переходу з ядра до периферії.\nКонтролює, наскільки щільно видно тайли одразу за innerRadius.")]
        [Range(0f, 1f)]
        public float outerStartReveal = 0.65f;

        [Tooltip("Кінцева ймовірність розкриття на дальньому краю периферії (біля outerRadius).\nМенші значення дають густіший сірий/нерозвіданий край.")]
        [Range(0f, 1f)]
        public float outerEndReveal = 0.08f;

        [Tooltip("Мінімальний множник впливу шуму в периферійній зоні.\nПідвищення значення робить периферію більш відкритою навіть у 'темних' ділянках шуму.")]
        [Range(0f, 2f)]
        public float outerNoiseMinFactor = 0.35f;

        [Tooltip("Сила внеску шуму в периферії.\nБільші значення підсилюють контраст: поруч будуть і відкриті, і сірі ділянки.")]
        [Range(0f, 2f)]
        public float outerNoiseFactor = 0.85f;

        [Header("Camera")]
        [Tooltip("Позиція Z для різкого перенесення камери в стартову точку.\nДля 2D зазвичай -10, щоб камера залишалась на правильній глибині.")]
        public float cameraZ = -10f;
    }
}
