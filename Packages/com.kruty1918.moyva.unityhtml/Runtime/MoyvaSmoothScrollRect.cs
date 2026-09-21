using System.Reflection;
using ReactUnity.UGUI.Behaviours;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityHTML.Runtime
{
    /// <summary>
    /// SmoothScrollRect with the in-flight accumulation fixed. Upstream computes
    /// each scroll event's target from the mid-animation position, so a continuous
    /// wheel/touchpad stream collapses to roughly a single step. Here an in-flight
    /// animation is first completed (position snaps to its target) so the next
    /// delta accumulates on the real position.
    /// </summary>
    public class MoyvaSmoothScrollRect : SmoothScrollRect
    {
        private static readonly FieldInfo SmoothCoroutineField = typeof(SmoothScrollRect)
            .GetField("SmoothCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo TargetPositionField = typeof(SmoothScrollRect)
            .GetField("targetPosition", BindingFlags.Instance | BindingFlags.NonPublic);

        public override void OnScroll(PointerEventData data)
        {
            // Complete any in-flight animation before upstream applies the new delta,
            // so the delta accumulates on the animation target instead of the
            // mid-flight position.
            var coroutine = SmoothCoroutineField?.GetValue(this) as Coroutine;
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                SmoothCoroutineField.SetValue(this, null);
                if (TargetPositionField != null)
                    normalizedPosition = (Vector2)TargetPositionField.GetValue(this);
            }

            base.OnScroll(data);
        }
    }
}
