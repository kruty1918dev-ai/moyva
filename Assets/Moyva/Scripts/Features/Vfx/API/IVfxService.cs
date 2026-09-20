using UnityEngine;

namespace Kruty1918.Moyva.Vfx.API
{
    /// <summary>
    /// Параметри одного спавну ефекту. Заповнює GameplayVfxService з доменної події.
    /// </summary>
    public struct VfxSpawnRequest
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Scale;
        public Color Tint;
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
        bool TrySpawn(VfxEffectRule rule, in VfxSpawnRequest request);
    }

    /// <summary>
    /// Публічна точка для tooling/тестів: програвання ефекту за id події.
    /// Gameplay-код не викликає це напряму — усе йде через доменні сигнали.
    /// </summary>
    public interface IVfxService
    {
        bool Play(string eventName, Vector3 worldPosition);
        bool Play(string eventName, Vector3 worldPosition, string context, Color? factionTint);
    }
}
