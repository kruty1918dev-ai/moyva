using System;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    [Serializable]
    public sealed class HomeMenuRevealFadeSettings
    {
        [Tooltip("Увімкнути плавне проявлення головного меню після входу в сцену.")]
        [SerializeField]
        public bool Enabled = true;

        [Tooltip("Тривалість fade з чорного в прозорий (сек).")]
        [Min(0f)]
        [SerializeField]
        public float DurationSeconds = 0.8f;

        [Tooltip("Додаткова затримка перед стартом fade (сек).")]
        [Min(0f)]
        [SerializeField]
        public float StartDelaySeconds = 0.05f;

        [Tooltip("Стартова непрозорість чорного екрану.")]
        [Range(0f, 1f)]
        [SerializeField]
        public float StartAlpha = 1f;
    }
}