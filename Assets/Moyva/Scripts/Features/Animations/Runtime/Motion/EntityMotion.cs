using System;
using Kruty1918.Moyva.Animations.API;
using UnityEngine;

namespace Kruty1918.Moyva.Animations.Runtime.Motion
{
    /// <summary>
    /// Canonical per-entity presentation motion. Replaces scattered tween calls:
    /// exponential follow, eased moves (with optional lift arc), yaw easing,
    /// additive punch impulses, scale easing and fully custom evaluators.
    /// Channels are mutually owned — starting a base-position motion cancels the
    /// previous one, so retargeting never produces duplicate writers.
    /// Pure presentation: cancelling, fast-forwarding or snapping must never
    /// affect authoritative gameplay state.
    /// </summary>
    public sealed class EntityMotion : MonoBehaviour
    {
        private const int SlotCount = 5;
        private const int SlotFollow = 0;
        private const int SlotEase = 1;
        private const int SlotRotation = 2;
        private const int SlotScale = 3;
        private const int SlotPunch = 4;

        private readonly ActiveMotion[] _slots = new ActiveMotion[SlotCount];
        private ActiveMotion _custom;

        /// <summary>Any channel currently animating.</summary>
        public bool IsAnimating => _custom != null || HasAnySlot();

        /// <summary>Base-position channel is owned by follow or an eased move.</summary>
        public bool HasBasePositionMotion => _slots[SlotFollow] != null || _slots[SlotEase] != null;

        public static EntityMotion AttachOrUpdate(GameObject target)
        {
            if (target == null)
                return null;
            if (!target.TryGetComponent(out EntityMotion motion))
                motion = target.AddComponent<EntityMotion>();
            return motion;
        }

        // ── Base position: exponential follow ────────────────────────────────

        /// <summary>Continuously eases position toward <paramref name="target"/>; retargets in place.</summary>
        public void FollowTo(Vector3 target, float sharpness)
        {
            if (sharpness <= 0f)
            {
                SnapTo(target);
                return;
            }
            CancelSlot(SlotEase);
            var follow = _slots[SlotFollow] as FollowMotion;
            if (follow == null)
            {
                follow = new FollowMotion();
                _slots[SlotFollow] = follow;
            }
            follow.Target = target;
            follow.Sharpness = sharpness;
            Enable();
        }

        /// <summary>Stops the follow channel at the current position.</summary>
        public void StopFollow() => CancelSlot(SlotFollow);

        // ── Base position: eased move ────────────────────────────────────────

        /// <summary>Eases from the current position to <paramref name="target"/> over <paramref name="duration"/>.</summary>
        public void EaseTo(Vector3 target, float duration, MotionEaseKind ease, Action onComplete = null)
            => EaseToWithLift(target, duration, ease, 0f, onComplete);

        /// <summary>Eased move with a small parabolic lift — used for relocation travel.</summary>
        public void EaseToWithLift(
            Vector3 target,
            float duration,
            MotionEaseKind ease,
            float liftHeight,
            Action onComplete = null)
        {
            CancelSlot(SlotFollow);
            if (duration <= 0f)
            {
                CancelSlot(SlotEase);
                transform.position = target;
                onComplete?.Invoke();
                return;
            }
            var motion = _slots[SlotEase] as PositionEaseMotion ?? new PositionEaseMotion();
            motion.Restart(transform.position, target, duration, ease, liftHeight, onComplete);
            _slots[SlotEase] = motion;
            Enable();
        }

        // ── Rotation ─────────────────────────────────────────────────────────

        /// <summary>Eases yaw from the current value toward <paramref name="targetYawDeg"/>.</summary>
        public void RotateYawTo(float targetYawDeg, float duration, MotionEaseKind ease)
            => RotateYawFromTo(transform.eulerAngles.y, targetYawDeg, duration, ease);

        /// <summary>Eases yaw between explicit angles; takes the shortest arc.</summary>
        public void RotateYawFromTo(float fromYawDeg, float toYawDeg, float duration, MotionEaseKind ease)
        {
            if (duration <= 0f)
            {
                CancelSlot(SlotRotation);
                transform.rotation = Quaternion.Euler(0f, toYawDeg, 0f);
                return;
            }
            var motion = _slots[SlotRotation] as YawEaseMotion ?? new YawEaseMotion();
            motion.Restart(fromYawDeg, toYawDeg, duration, ease);
            _slots[SlotRotation] = motion;
            transform.rotation = Quaternion.Euler(0f, fromYawDeg, 0f);
            Enable();
        }

        // ── Scale ────────────────────────────────────────────────────────────

        /// <summary>Eases localScale from the current value toward <paramref name="targetScale"/>.</summary>
        public void ScaleTo(Vector3 targetScale, float duration, MotionEaseKind ease, Action onComplete = null)
        {
            if (duration <= 0f)
            {
                CancelSlot(SlotScale);
                transform.localScale = targetScale;
                onComplete?.Invoke();
                return;
            }
            var motion = _slots[SlotScale] as ScaleEaseMotion ?? new ScaleEaseMotion();
            motion.Restart(transform.localScale, targetScale, duration, ease, onComplete);
            _slots[SlotScale] = motion;
            Enable();
        }

        // ── Additive punch ───────────────────────────────────────────────────

        /// <summary>
        /// Out-and-back positional impulse (hit recoil, attack lunge, blocked shake).
        /// Composes additively on top of any base-position motion.
        /// </summary>
        public void PunchPosition(Vector3 worldOffset, float duration, MotionEaseKind ease, Action onComplete = null)
        {
            if (duration <= 0f)
            {
                onComplete?.Invoke();
                return;
            }
            var motion = _slots[SlotPunch] as PunchMotion ?? new PunchMotion();
            motion.Restart(worldOffset, duration, ease, onComplete);
            _slots[SlotPunch] = motion;
            Enable();
        }

        // ── Fully custom evaluator ───────────────────────────────────────────

        /// <summary>
        /// Single generic channel: <paramref name="apply"/> receives eased t in [0,1]
        /// and may drive any transform properties (death tilt+sink+fade, garrison shrink).
        /// Only one custom motion at a time — starting a new one cancels the old.
        /// </summary>
        public void PlayCustom(float duration, MotionEaseKind ease, Action<float> apply, Action onComplete = null)
        {
            CancelCustom();
            if (duration <= 0f || apply == null)
            {
                apply?.Invoke(1f);
                onComplete?.Invoke();
                return;
            }
            _custom = new CustomMotion
            {
                Duration = duration,
                Ease = ease,
                Apply = apply,
                OnComplete = onComplete
            };
            Enable();
        }

        // ── Authority snaps / cancellation ───────────────────────────────────

        /// <summary>Cancels all position channels and snaps to <paramref name="position"/>.</summary>
        public void SnapTo(Vector3 position)
        {
            CancelSlot(SlotFollow);
            CancelSlot(SlotEase);
            CancelSlot(SlotPunch);
            transform.position = position;
        }

        /// <summary>Full cancel + snap — used for server corrections, restore, resync.</summary>
        public void SnapPose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            CancelAll();
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = scale;
        }

        /// <summary>Cancels every channel without invoking completion callbacks.</summary>
        public void CancelAll()
        {
            for (int i = 0; i < SlotCount; i++)
                CancelSlot(i);
            CancelCustom();
        }

        /// <summary>Frame driver. Called by Update; public so EditMode tests can pump it.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            for (int i = 0; i < SlotCount; i++)
            {
                ActiveMotion motion = _slots[i];
                if (motion != null && motion.Tick(this, deltaTime))
                    _slots[i] = null;
            }

            if (_custom != null && _custom.Tick(this, deltaTime))
                _custom = null;

            if (!IsAnimating)
                enabled = false;
        }

        private void Update() => Tick(Time.deltaTime);

        private void OnDisable() => CancelAll();

        private void Enable()
        {
            if (!enabled)
                enabled = true;
        }

        private bool HasAnySlot()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (_slots[i] != null)
                    return true;
            }
            return false;
        }

        private void CancelSlot(int slot)
        {
            ActiveMotion motion = _slots[slot];
            if (motion == null)
                return;
            _slots[slot] = null;
            motion.OnCancelled(this);
        }

        private void CancelCustom()
        {
            _custom?.OnCancelled();
            _custom = null;
        }

        // ── Motion primitives ────────────────────────────────────────────────

        private abstract class ActiveMotion
        {
            public Action OnComplete;
            /// <summary>Returns true when finished (completion callback already fired).</summary>
            public abstract bool Tick(EntityMotion owner, float deltaTime);
            public virtual void OnCancelled(EntityMotion owner) { }

            protected void Complete()
            {
                Action callback = OnComplete;
                OnComplete = null;
                callback?.Invoke();
            }
        }

        private sealed class FollowMotion : ActiveMotion
        {
            public Vector3 Target;
            public float Sharpness;

            public override bool Tick(EntityMotion owner, float deltaTime)
            {
                Transform t = owner.transform;
                float k = 1f - Mathf.Exp(-Sharpness * deltaTime);
                Vector3 next = Vector3.Lerp(t.position, Target, k);
                t.position = next;
                if ((Target - next).sqrMagnitude <= 0.0001f)
                {
                    t.position = Target;
                    Complete();
                    return true;
                }
                return false;
            }
        }

        private sealed class PositionEaseMotion : ActiveMotion
        {
            private Vector3 _from;
            private Vector3 _to;
            private float _lift;
            private float _duration;
            private float _elapsed;
            private MotionEaseKind _ease;

            public void Restart(Vector3 from, Vector3 to, float duration, MotionEaseKind ease, float lift, Action onComplete)
            {
                _from = from;
                _to = to;
                _duration = duration;
                _ease = ease;
                _lift = lift;
                _elapsed = 0f;
                OnComplete = onComplete;
            }

            public override bool Tick(EntityMotion owner, float deltaTime)
            {
                _elapsed += deltaTime;
                float t = Mathf.Clamp01(_elapsed / _duration);
                float e = MotionEaseEvaluator.Evaluate(_ease, t);
                Vector3 pos = Vector3.LerpUnclamped(_from, _to, e);
                if (_lift != 0f)
                    pos.y += _lift * 4f * t * (1f - t);
                owner.transform.position = pos;
                if (t >= 1f)
                {
                    owner.transform.position = _to;
                    Complete();
                    return true;
                }
                return false;
            }
        }

        private sealed class YawEaseMotion : ActiveMotion
        {
            private float _fromDeg;
            private float _toDeg;
            private float _duration;
            private float _elapsed;
            private MotionEaseKind _ease;

            public void Restart(float fromDeg, float toDeg, float duration, MotionEaseKind ease)
            {
                _fromDeg = fromDeg;
                _toDeg = toDeg;
                _duration = duration;
                _ease = ease;
                _elapsed = 0f;
            }

            public override bool Tick(EntityMotion owner, float deltaTime)
            {
                _elapsed += deltaTime;
                float t = Mathf.Clamp01(_elapsed / _duration);
                float e = MotionEaseEvaluator.Evaluate(_ease, t);
                float yaw = Mathf.LerpAngle(_fromDeg, _toDeg, e);
                owner.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                if (t >= 1f)
                {
                    owner.transform.rotation = Quaternion.Euler(0f, _toDeg, 0f);
                    return true;
                }
                return false;
            }
        }

        private sealed class ScaleEaseMotion : ActiveMotion
        {
            private Vector3 _from;
            private Vector3 _to;
            private float _duration;
            private float _elapsed;
            private MotionEaseKind _ease;

            public void Restart(Vector3 from, Vector3 to, float duration, MotionEaseKind ease, Action onComplete)
            {
                _from = from;
                _to = to;
                _duration = duration;
                _ease = ease;
                _elapsed = 0f;
                OnComplete = onComplete;
            }

            public override bool Tick(EntityMotion owner, float deltaTime)
            {
                _elapsed += deltaTime;
                float t = Mathf.Clamp01(_elapsed / _duration);
                owner.transform.localScale = Vector3.LerpUnclamped(_from, _to, MotionEaseEvaluator.Evaluate(_ease, t));
                if (t >= 1f)
                {
                    owner.transform.localScale = _to;
                    Complete();
                    return true;
                }
                return false;
            }
        }

        private sealed class PunchMotion : ActiveMotion
        {
            private Vector3 _offset;
            private float _duration;
            private float _elapsed;
            private float _applied;
            private MotionEaseKind _ease;

            public void Restart(Vector3 offset, float duration, MotionEaseKind ease, Action onComplete)
            {
                _offset = offset;
                _duration = duration;
                _ease = ease;
                _elapsed = 0f;
                _applied = 0f;
                OnComplete = onComplete;
            }

            public override bool Tick(EntityMotion owner, float deltaTime)
            {
                _elapsed += deltaTime;
                float t = Mathf.Clamp01(_elapsed / _duration);
                // Out-and-back: full offset at t=0.5, back to zero at t=1.
                float e = MotionEaseEvaluator.Evaluate(_ease, 1f - Mathf.Abs(2f * t - 1f));
                float delta = e - _applied;
                _applied = e;
                owner.transform.position += _offset * delta;
                if (t >= 1f)
                {
                    // Guarantee zero residual offset at the end.
                    owner.transform.position -= _offset * _applied;
                    Complete();
                    return true;
                }
                return false;
            }

            public override void OnCancelled(EntityMotion owner)
            {
                // Remove the still-applied residual so cancellation never leaves drift.
                owner.transform.position -= _offset * _applied;
                _offset = Vector3.zero;
                _applied = 0f;
            }
        }

        private sealed class CustomMotion : ActiveMotion
        {
            public float Duration;
            public float Elapsed;
            public MotionEaseKind Ease;
            public Action<float> Apply;

            public override bool Tick(EntityMotion owner, float deltaTime)
            {
                Elapsed += deltaTime;
                float t = Mathf.Clamp01(Elapsed / Duration);
                Apply?.Invoke(MotionEaseEvaluator.Evaluate(Ease, t));
                if (t >= 1f)
                {
                    Complete();
                    return true;
                }
                return false;
            }
        }
    }
}
