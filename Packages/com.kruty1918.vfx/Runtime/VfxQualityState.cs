using UnityEngine;

namespace Kruty1918.Vfx
{
    /// <summary>Знімок якісного стану VFX: ліміти активних ефектів, масштаби кількості, дистанції відсікання.</summary>
    public readonly struct VfxQualityState
    {
        /// <summary>Створює стан якості з явними лімітами.</summary>
        public VfxQualityState(
            int maxActive,
            float countScale,
            float cullDistance,
            float lowPriorityMaxOrthoSize,
            int maxSpawnsPerFrame)
        {
            MaxActive = Mathf.Max(1, maxActive);
            CountScale = Mathf.Max(0.05f, countScale);
            CullDistance = Mathf.Max(0f, cullDistance);
            LowPriorityMaxOrthoSize = Mathf.Max(0f, lowPriorityMaxOrthoSize);
            MaxSpawnsPerFrame = Mathf.Max(1, maxSpawnsPerFrame);
        }

        /// <summary>Максимум активних ефектів.</summary>
        public int MaxActive { get; }
        /// <summary>Масштаб кількості частинок.</summary>
        public float CountScale { get; }
        /// <summary>Дистанція відсікання ефектів.</summary>
        public float CullDistance { get; }
        /// <summary>Максимальний орто-розмір для низькопріоритетних ефектів.</summary>
        public float LowPriorityMaxOrthoSize { get; }
        /// <summary>Максимум спавнів за кадр.</summary>
        public int MaxSpawnsPerFrame { get; }

    }
}
