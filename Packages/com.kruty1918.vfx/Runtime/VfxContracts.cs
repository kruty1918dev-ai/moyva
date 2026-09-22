using System;
using UnityEngine;

namespace Kruty1918.Vfx
{
    /// <summary>
    /// Параметри одного спавну ефекту. Заповнює GameplayVfxService з доменної події.
    /// </summary>
    public struct VfxSpawnRequest
    {
        /// <summary>Світова позиція спавну.</summary>
        public Vector3 Position;
        /// <summary>Обертання ефекту при спавні.</summary>
        public Quaternion Rotation;
        /// <summary>Масштаб ефекту.</summary>
        public float Scale;
        /// <summary>Тінт ефекту.</summary>
        public Color Tint;
        /// <summary>Чи застосовувати тінт.</summary>
        public bool HasTint;
        /// <summary>Множник кількості частинок від поточного quality-профілю (0..1+).</summary>
        public float CountScale;
        /// <summary>Ключ для cooldownPerKey троттлингу (наприклад unitId). Null = без ключа.</summary>
        public string CooldownKey;
        /// <summary>Тривалість override. &lt;=0 — з правила/префабу.</summary>
        public float DurationOverride;
    }

    /// <summary>
    /// Test seam між координацією подій і pooled Unity-спавном.
    /// </summary>
    public interface IVfxSpawner
    {
        /// <summary>Намагається заспавнити ефект за правилом і запитом.</summary>
        bool TrySpawn(VfxEffectRule rule, in VfxSpawnRequest request);
    }

    /// <summary>
    /// Публічна точка для tooling/тестів: програвання ефекту за id події.
    /// Gameplay-код не викликає це напряму — усе йде через доменні сигнали.
    /// </summary>
    public interface IVfxService
    {
        /// <summary>Відтворює ефект за назвою події у світовій позиції.</summary>
        bool Play(string eventName, Vector3 worldPosition);
        /// <summary>Відтворює ефект із контекстом і тінтом фракції.</summary>
        bool Play(string eventName, Vector3 worldPosition, string context, Color? factionTint);
    }

    /// <summary>
    /// Правило: подія (+опційний контекст) → pooled префаб-ефект.
    /// Контекст: "unit:&lt;unitTypeId&gt;", "tile:&lt;tileTypeId&gt;" —
    /// спочатку шукається eventName+context, інакше базове eventName.
    /// </summary>
    [Serializable]
    public sealed class VfxEffectRule
    {
        /// <summary>Назва події, на яку реагує правило.</summary>
        public string eventName;
        /// <summary>Контекстний фільтр правила (порожній — будь-який).</summary>
        public string context = string.Empty;
        /// <summary>Префаб ефекту для спавну.</summary>
        public GameObject prefab;

        [Tooltip("0 = low (ambient/dust), 1 = normal, 2 = critical (kill/capture).")]
        [Range(0, 2)] public int priority = 1;

        [Tooltip("Одночасних інстансів цього ефекту. 0 = лише глобальний бюджет.")]
        [Min(0)] public int maxConcurrent;

        [Tooltip("Кількість інстансів для прогріву пулу на Initialize.")]
        [Min(0)] public int prewarm;

        /// <summary>Тривалість ефекту в секундах.</summary>
        [Tooltip("Тривалість до auto-return. <=0 — взяти duration із VfxEffect на префабі.")]
        public float duration;

        [Tooltip("Множник world-scale ефекту.")]
        [Min(0.01f)] public float scale = 1f;

        [Tooltip("Мінімальний інтервал між спавнами з тим самим dedupeKey (наприклад unitId). 0 = без троттлингу.")]
        [Min(0f)] public float cooldownPerKey;

        /// <summary>Чи тінтити ефект кольором фракції.</summary>
        [Tooltip("Підфарбувати tint-системи ефекту кольором фракції власника.")]
        public bool factionTint;

        [Tooltip("Не спавнити, якщо подія далі за цю дистанцію від камери. 0 = без обмеження.")]
        [Min(0f)] public float cullDistance;

        [Tooltip("Не спавнити low-priority ефект, якщо orthographicSize камери більший (далекий zoom). 0 = вимкнено.")]
        [Min(0f)] public float maxOrthoSize;
    }

    /// <summary>Глобальний бюджет активних ефектів за профілем якості.</summary>
    [Serializable]
    public sealed class VfxBudgetSettings
    {
        [Min(1)] public int maxActivePerformance = 24;
        [Min(1)] public int maxActiveBalanced = 48;
        [Min(1)] public int maxActiveQuality = 96;

        [Tooltip("Максимум нових спавнів за один кадр (решта drop).")]
        [Min(1)] public int maxSpawnsPerFrame = 6;

        [Tooltip("Дистанція від камери, за якою не-критичні ефекти не спавняться. 0 = вимкнено.")]
        [Min(0f)] public float defaultCullDistance = 90f;

        [Tooltip("orthographicSize камери, за яким low-priority ефекти повністю вимикаються. 0 = вимкнено.")]
        [Min(0f)] public float lowPriorityMaxOrthoSize = 0f;
    }
}
