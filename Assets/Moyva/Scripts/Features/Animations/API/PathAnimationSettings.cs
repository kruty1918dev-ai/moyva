using System;
using UnityEngine;

namespace Kruty1918.Moyva.Animations.API
{
    [Serializable]
    public struct PathAnimationSettings
    {
        public float MoveDurationPerTile; // Час руху між двома сусідніми тайлами
        public float DelayOnTile;         // Затримка перед тим, як зробити наступний крок
        public Action<Vector2Int> OnStepCompleted;
        public Func<Vector2Int, bool> CanPerformStep;
        public static PathAnimationSettings Default => new PathAnimationSettings
        {
            MoveDurationPerTile = 0.3f,
            DelayOnTile = 0.05f
        };
    }
}