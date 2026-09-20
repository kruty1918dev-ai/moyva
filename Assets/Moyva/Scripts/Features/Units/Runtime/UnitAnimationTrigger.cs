using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Runtime
{
    /// <summary>
    /// Stateless animator bridge: resolves the configured clip for a unit type
    /// and fires its Animator trigger. Shared by combat, movement and lifecycle
    /// presentation so every caller uses the same lookup rules.
    /// </summary>
    internal static class UnitAnimationTrigger
    {
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
