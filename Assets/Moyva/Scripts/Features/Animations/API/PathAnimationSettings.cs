using System;
using UnityEngine;

namespace Kruty1918.Moyva.Animations.API
{
    /// <summary>Налаштування анімації проходження юніта шляхом: швидкість, прискорення, повороти, bob та колбеки.</summary>
    [Serializable]
    public struct PathAnimationSettings
    {
        /// <summary>Час руху між двома сусідніми тайлами (крейсерська швидкість).</summary>
        public float MoveDurationPerTile; // Час руху між двома сусідніми тайлами (крейсерська швидкість)
        /// <summary>Legacy: ігнорується continuous traversal — шлях іде без зупинок.</summary>
        public float DelayOnTile;         // Legacy: ігнорується continuous traversal — шлях йде без зупинок
        /// <summary>Прискорення в од/с²; 0 використовує типовий профіль.</summary>
        public float Acceleration;        // Од/с² — 0 використовує типовий профіль
        /// <summary>Сповільнення в од/с²; 0 використовує типовий профіль.</summary>
        public float Deceleration;        // Од/с² — 0 використовує типовий профіль
        /// <summary>Швидкість повороту до напрямку руху, град/с.</summary>
        public float TurnSpeedDegPerSec;  // Швидкість повороту до напрямку руху
        /// <summary>Чи повертатися в напрямку руху (лише 3D world plane).</summary>
        public bool FaceTravelDirection;  // Повертатися в напрямку руху (лише 3D world plane)
        /// <summary>Вертикальна амплітуда bob у метрах (secondary motion).</summary>
        public float BobAmplitude;        // Вертикальний bob у метрах (secondary motion)
        /// <summary>Цикли bob на одиницю пройденої відстані.</summary>
        public float BobFrequency;        // Цикли bob на одиницю пройденої відстані
        /// <summary>Колбек завершення кроку на тайл.</summary>
        public Action<Vector2Int> OnStepCompleted;
        /// <summary>Предикат дозволу кроку на вказаний тайл.</summary>
        public Func<Vector2Int, bool> CanPerformStep;
        /// <summary>Резолвер світової позиції тайла.</summary>
        public Func<Vector2Int, Vector3> ResolveWorldPosition;
        /// <summary>Предикат дозволу скасування зі snap до позиції.</summary>
        public Func<bool> AllowCancelSnap;
        /// <summary>Усталені налаштування анімації шляху.</summary>
        public static PathAnimationSettings Default => new PathAnimationSettings
        {
            MoveDurationPerTile = 0.3f,
            DelayOnTile = 0f,
            Acceleration = 24f,
            Deceleration = 30f,
            TurnSpeedDegPerSec = 540f,
            FaceTravelDirection = true,
            BobAmplitude = 0.03f,
            BobFrequency = 0.9f
        };
    }
}
