using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    [Serializable]
    public sealed class InitialResourceEntry
    {
        [ResourceIdAttribute]
        [SerializeField]
        public string ResourceId = string.Empty;

        [Min(0f)]
        [SerializeField]
        public float Amount = 10f;

        public InitialResourceEntry() { }

        public InitialResourceEntry(string resourceId, float amount)
        {
            ResourceId = resourceId;
            Amount = amount;
        }
    }

    [Serializable]
    public sealed class BootstrapGameSettings
    {
        [Serializable]
        public sealed class WorldRevealFadeSettings
        {
            [Tooltip("Увімкнути плавне проявлення світу після сигналу готовності карти.")]
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

        [Serializable]
        public sealed class SandboxProgressSettings
        {
            [Tooltip("Realtime seconds that represent one gameplay progress cycle in sandbox mode.")]
            [Min(0.1f)]
            [SerializeField]
            public float RoundSeconds = 10f;

            [Tooltip("Initial sandbox speed multiplier.")]
            [Range(0.1f, 8f)]
            [SerializeField]
            public float InitialSpeed = 1f;
        }

        [Header("Initial Resources")]
        [Tooltip("Список ресурсів, які гравець отримує на старт нової гри.")]
        [SerializeField]
        // Завдання: це serialized джерело для вкладки Economy Designer -> "Стартова економіка".
        // Bootstrap читає саме цей список, щоб стартові ресурси з редактора потрапляли в новий світ.
        public List<InitialResourceEntry> InitialResources = new()
        {
            new InitialResourceEntry("steak-food-resources", 140f),
            new InitialResourceEntry("walnut-wood-materials-resources", 180f),
            new InitialResourceEntry("hardwood-materials-resources", 120f),
            new InitialResourceEntry("stone-materials-resources", 100f),
            new InitialResourceEntry("iron-ore-materials-resources", 20f),
            new InitialResourceEntry("iron-ingot-materials-resources", 20f),
            new InitialResourceEntry("gold-coins-materials-resources", 50f),
        };

        [Header("World Reveal")]
        [Tooltip("Налаштування плавного проявлення світу після генерації/завантаження.")]
        [SerializeField]
        public WorldRevealFadeSettings WorldRevealFade = new();

        [Header("Sandbox Progress")]
        [Tooltip("Realtime progress settings used when gameplay runs without turn ownership.")]
        [SerializeField]
        public SandboxProgressSettings SandboxProgress = new();

        public BootstrapGameSettings() { }
    }
}
