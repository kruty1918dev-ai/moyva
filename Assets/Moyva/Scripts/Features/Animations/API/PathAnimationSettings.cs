using System;
using UnityEngine;

namespace Kruty1918.Moyva.Animations.API
{
    [Serializable]
    public struct PathAnimationSettings
    {
        public float MoveDurationPerTile; // Час руху між двома сусідніми тайлами (крейсерська швидкість)
        public float DelayOnTile;         // Legacy: ігнорується continuous traversal — шлях йде без зупинок
        public float Acceleration;        // Од/с² — 0 використовує типовий профіль
        public float Deceleration;        // Од/с² — 0 використовує типовий профіль
        public float TurnSpeedDegPerSec;  // Швидкість повороту до напрямку руху
        public bool FaceTravelDirection;  // Повертатися в напрямку руху (лише 3D world plane)
        public float BobAmplitude;        // Вертикальний bob у метрах (secondary motion)
        public float BobFrequency;        // Цикли bob на одиницю пройденої відстані
        public Action<Vector2Int> OnStepCompleted;
        public Func<Vector2Int, bool> CanPerformStep;
        public Func<Vector2Int, Vector3> ResolveWorldPosition;
        /// <summary>
        /// Cancellation policy: may the presentation snap the transform back to
        /// the last authoritative tile? False when another lifecycle transition
        /// (e.g. death) owns the transform. Null = always snap.
        /// </summary>
        public Func<bool> AllowCancelSnap;
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
