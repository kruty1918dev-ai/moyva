using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Runtime
{
    /// <summary>Тригер анімацій юніта: програє кліпи за подіями стану.</summary>
    internal static class UnitAnimationTrigger
    {
        /// <summary>Програє анімацію на вказаному об'єкті юніта.</summary>
        public static void Play(
            GameObject unitObject,
            UnitClassConfig config,
            AnimationType type)
        {
            if (unitObject == null || config == null)
                return;

            UnitAnimationClip clip = config.GetAnimation(type);
            if (clip == null || string.IsNullOrWhiteSpace(clip.AnimatorParameterName))
                return;

            Animator animator = unitObject.GetComponentInChildren<Animator>(true);
            if (animator != null)
                animator.SetTrigger(clip.AnimatorParameterName);
        }
    }
}
