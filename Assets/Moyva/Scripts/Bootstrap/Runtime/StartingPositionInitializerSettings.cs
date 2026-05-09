using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    [Serializable]
    public sealed class StartingFogScalePoint
    {
        [Tooltip("Менша сторона мапи у тайлах, для якої задано це значення.")]
        [Min(16)] public int MapSideTiles = 64;

        [Tooltip("Радіус стартового розкриття туману для цього розміру мапи.")]
        [Min(1)] public int RevealedRadius = 15;

        [Tooltip("Радіус постійно видимого ядра для цього розміру. 0 = використати RevealedRadius.")]
        [Min(0)] public int CoreVisibleRadius = 0;
    }

    [Serializable]
    public sealed class StartingPositionInitializerSettings
    {
        [Header("Стартова позиція")]
        [Tooltip("Мінімальний відступ стартової точки від краю мапи в тайлах.\nБільше значення зменшує шанс старту біля краю і робить старт безпечнішим.")]
        [Min(0)]
        public int minMarginFromBorder = 5;

        [Tooltip("Додатковий відступ від краю як частка від меншої сторони мапи.\nНаприклад 0.1667 ≈ 1/6 мапи. Підсумковий відступ = max(minMarginFromBorder, відносний відступ).")]
        [Range(0f, 0.45f)]
        public float relativeMarginFactor = 0.1667f;

        [Header("Висота стартового тайла")]
        [Tooltip("Мінімальна висота тайла, який може бути обраний як стартовий. Використовується HeightMap зі згенерованого світу, щоб старт не потрапляв у воду.")]
        [Range(0f, 1f)]
        public float startMinHeight = 0.35f;

        [Tooltip("Максимальна висота тайла, який може бути обраний як стартовий. Дозволяє відсікти надто високі гори або інші небажані зони.")]
        [Range(0f, 1f)]
        public float startMaxHeight = 1f;

        [Tooltip("Якщо увімкнено, старт не буде обрано без валідної HeightMap. Якщо вимкнено, за відсутності HeightMap використовується звичайний випадковий fallback.")]
        public bool requireHeightMapForStart = false;

        [Header("Форма розкриття туману")]
        [Tooltip("Радіус стартового кола розсіювання туману в тайлах. Усі тайли всередині кола відкриваються рівномірно, без випадкового шуму.")]
        [Min(1)]
        public int revealedCircleRadius = 15;

        [Tooltip("Форма області, яку туман відкриває на старті нового світу або при ремонті пошкодженого сейву.")]
        public FogRevealShape revealShape = FogRevealShape.PixelCircle;

        [Tooltip("Мінімальна кількість розвіданих тайлів у завантаженому сейві. Якщо менше — туман вважається пошкодженим і стартова область відновлюється.")]
        [Min(1)] public int minimumExploredTilesBeforeRepair = 4;

        [Tooltip("Якщо увімкнено, стартовий туман масштабується за таблицею залежно від меншої сторони мапи.")]
        public bool useMapSizeScaledFog = true;

        [Tooltip("Опорні точки масштабування туману. Між найближчими точками значення інтерполюється лінійно.")]
        public List<StartingFogScalePoint> fogScaleByMapSize = new()
        {
            new StartingFogScalePoint { MapSideTiles = 32, RevealedRadius = 9, CoreVisibleRadius = 7 },
            new StartingFogScalePoint { MapSideTiles = 64, RevealedRadius = 15, CoreVisibleRadius = 12 },
            new StartingFogScalePoint { MapSideTiles = 128, RevealedRadius = 24, CoreVisibleRadius = 18 },
        };

        [Tooltip("Якщо увімкнено, ядро підтримується повністю видимим (без туману)\nчерез службовий стартовий огляд.\nЯкщо вимкнено, ядро буде лише розвіданим і з часом стане сірим без юнітів.")]
        public bool keepCoreFullyVisible = true;

        [Tooltip("Радіус службового стартового огляду.\n0 або менше = використовувати revealedCircleRadius.")]
        [Min(0)]
        public int coreVisibleRadiusOverride = 0;

        [Header("Стартові позиції мультиплеєра")]
        [Tooltip("Скільки стартових позицій резервувати, коли світ генерує хост. Якщо учасників у сесії більше, використовується фактична кількість учасників.")]
        [Min(1)]
        public int multiplayerStartSlots = 4;

        [Tooltip("Мінімальна відстань між стартовими позиціями гравців у тайлах за A* шляхом. Якщо A* не прив'язаний у сцені, використовується fallback за евклідовою відстанню.")]
        [Min(1)]
        public int minAStarDistanceBetweenPlayers = 15;

        [Tooltip("Кількість спроб знайти валідну стартову позицію перед fallback. Більше значення корисне для малих або складних мап.")]
        [Min(1)]
        public int startCandidateAttempts = 256;

        [Header("Камера")]
        [Tooltip("Позиція Z для різкого перенесення камери в стартову точку.\nДля 2D зазвичай -10, щоб камера залишалась на правильній глибині.")]
        public float cameraZ = -10f;

        public int ResolveRevealedRadius(int width, int height)
            => ResolveFogRadius(width, height, useCoreRadius: false);

        public FogRevealShape ResolveRevealShape()
            => revealShape;

        public int ResolveCoreVisibleRadius(int width, int height)
        {
            if (coreVisibleRadiusOverride > 0)
                return coreVisibleRadiusOverride;

            return ResolveFogRadius(width, height, useCoreRadius: true);
        }

        private int ResolveFogRadius(int width, int height, bool useCoreRadius)
        {
            if (!useMapSizeScaledFog || fogScaleByMapSize == null || fogScaleByMapSize.Count == 0)
                return Mathf.Max(1, revealedCircleRadius);

            int side = Mathf.Max(1, Mathf.Min(width, height));
            StartingFogScalePoint lower = null;
            StartingFogScalePoint upper = null;

            for (int i = 0; i < fogScaleByMapSize.Count; i++)
            {
                var point = fogScaleByMapSize[i];
                if (point == null || point.MapSideTiles <= 0)
                    continue;

                if (point.MapSideTiles <= side && (lower == null || point.MapSideTiles > lower.MapSideTiles))
                    lower = point;

                if (point.MapSideTiles >= side && (upper == null || point.MapSideTiles < upper.MapSideTiles))
                    upper = point;
            }

            lower ??= upper;
            upper ??= lower;

            if (lower == null)
                return Mathf.Max(1, revealedCircleRadius);

            if (lower == upper || lower.MapSideTiles == upper.MapSideTiles)
                return ResolvePointRadius(lower, useCoreRadius);

            float t = Mathf.InverseLerp(lower.MapSideTiles, upper.MapSideTiles, side);
            float radius = Mathf.Lerp(ResolvePointRadius(lower, useCoreRadius), ResolvePointRadius(upper, useCoreRadius), t);
            return Mathf.Max(1, Mathf.RoundToInt(radius));
        }

        private static int ResolvePointRadius(StartingFogScalePoint point, bool useCoreRadius)
        {
            if (point == null)
                return 1;

            if (!useCoreRadius)
                return Mathf.Max(1, point.RevealedRadius);

            return point.CoreVisibleRadius > 0
                ? point.CoreVisibleRadius
                : Mathf.Max(1, point.RevealedRadius);
        }
    }
}
