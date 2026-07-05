using UnityEngine;

#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Відповідає за анімацію натиску UI-кнопок Construction модуля.
    /// За наявності DOTween виконує punch-анімацію масштабу.
    /// </summary>
    public static class ConstructionButtonPressAnimator
    {
        public static void AnimatePress(
            Transform target,
            float scaleMultiplier,
            float duration,
            int vibrato,
            float elasticity)
        {
            if (target == null)
                return;

#if DOTWEEN_ENABLED
            var clampedMultiplier = Mathf.Max(1f, scaleMultiplier);
            var punch = new Vector3(clampedMultiplier - 1f, clampedMultiplier - 1f, 0f);

            target.DOKill();
            target.DOPunchScale(punch, Mathf.Max(0.01f, duration), Mathf.Max(1, vibrato), Mathf.Clamp01(elasticity));
#endif
        }
    }
}