using System;
using Kruty1918.Moyva.Animations.API;
using UnityEngine;

namespace Kruty1918.Moyva.Animations.Runtime.Motion
{
    /// <summary>
    /// Канонічний presentation-motion для сутності. Замінює розкидані tween-виклики:
    /// експоненційне слідкування, eased-переміщення (з опційною дугою підйому),
    /// easing повороту yaw, адитивні punch-імпульси, easing масштабу та повністю
    /// кастомні евалюатори. Канали взаємовиключні — запуск нового руху базової
    /// позиції скасовує попередній, тому ретаргетинг ніколи не створює дублікатів
    /// писачів. Чиста презентація: скасування, перемотка чи snap ніколи не впливають
    /// на авторитетний gameplay-стан.
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

        /// <summary>Чи анімується зараз хоча б один канал.</summary>
        public bool IsAnimating => _custom != null || HasAnySlot();

        /// <summary>Чи зайнятий канал базової позиції follow- або eased-рухом.</summary>
        public bool HasBasePositionMotion => _slots[SlotFollow] != null || _slots[SlotEase] != null;

        /// <summary>
        /// Повертає наявний компонент <see cref="EntityMotion"/> на цілі або додає новий.
        /// </summary>
        public static EntityMotion AttachOrUpdate(GameObject target)
        {
            if (target == null)
                return null;
            if (!target.TryGetComponent(out EntityMotion motion))
                motion = target.AddComponent<EntityMotion>();
            return motion;
        }

        // ── Base position: exponential follow ────────────────────────────────

        /// <summary>Безперервно підтягує позицію до <paramref name="target"/>; ретаргетиться на місці.</summary>
        public void FollowTo(Vector3 target, float sharpness)
        {
            if (sharpness <= 0f)
            {
                SnapTo(target);
                return;
            }
            CancelSlot(SlotEase);
            var follow = _slots[SlotFollow] as FollowMotion ?? new FollowMotion();
            _slots[SlotFollow] = follow;
            follow.Target = target;
            follow.Sharpness = sharpness;
            Enable();
        }

        /// <summary>Зупиняє канал follow на поточній позиції.</summary>
        public void StopFollow() => CancelSlot(SlotFollow);

        // ── Base position: eased move ────────────────────────────────────────

        /// <summary>Плавно переводить позицію з поточної до <paramref name="target"/> за <paramref name="duration"/>.</summary>
        public void EaseTo(Vector3 target, float duration, MotionEaseKind ease, Action onComplete = null)
            => EaseToWithLift(target, duration, ease, 0f, onComplete);

        /// <summary>Eased-переміщення з малою параболічною дугою підйому — для переїзду юніта.</summary>
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

        /// <summary>Плавно повертає yaw з поточного значення до <paramref name="targetYawDeg"/>.</summary>
        public void RotateYawTo(float targetYawDeg, float duration, MotionEaseKind ease)
            => RotateYawFromTo(transform.eulerAngles.y, targetYawDeg, duration, ease);

        /// <summary>Плавно повертає yaw між явними кутами; обирає найкоротшу дугу.</summary>
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

        /// <summary>Плавно змінює localScale з поточного значення до <paramref name="targetScale"/>.</summary>
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
        /// Позиційний імпульс "туди-назад" (віддача влучання, ривок атаки, тремтіння блоку).
        /// Накладається адитивно поверх будь-якого руху базової позиції.
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
        /// Єдиний загальний канал: <paramref name="apply"/> отримує eased t у [0,1]
        /// і може керувати будь-якими властивостями трансформа (нахил+занурення смерті,
        /// стискання гарнізону). Одночасно активний лише один custom-рух — запуск
        /// нового скасовує старий.
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

        /// <summary>Скасовує всі позиційні канали та миттєво ставить <paramref name="position"/>.</summary>
        public void SnapTo(Vector3 position)
        {
            CancelSlot(SlotFollow);
            CancelSlot(SlotEase);
            CancelSlot(SlotPunch);
            transform.position = position;
        }

        /// <summary>Повне скасування + snap — для серверних корекцій, restore, resync.</summary>
        public void SnapPose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            CancelAll();
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = scale;
        }

        /// <summary>Скасовує всі канали без виклику completion-колбеків.</summary>
        public void CancelAll()
        {
            for (int i = 0; i < SlotCount; i++)
                CancelSlot(i);
            CancelCustom();
        }

        /// <summary>Кадровий драйвер. Викликається з Update; public, щоб EditMode-тести могли його прокачувати.</summary>
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
            _custom?.OnCancelled(this);
            _custom = null;
        }

        // ── Motion primitives ────────────────────────────────────────────────

        /// <summary>
        /// Базовий примітив активного руху в каналі <see cref="EntityMotion"/>.
        /// </summary>
        private abstract class ActiveMotion
        {
            /// <summary>Колбек, що викликається після завершення руху.</summary>
            public Action OnComplete;
            /// <summary>Повертає true після завершення (completion-колбек уже викликано).</summary>
            public abstract bool Tick(EntityMotion owner, float deltaTime);
            /// <summary>Реакція на скасування каналу; за замовчуванням нічого не робить.</summary>
            public virtual void OnCancelled(EntityMotion owner) { }

            /// <summary>Завершує рух і викликає відкладений completion-колбек.</summary>
            protected void Complete()
            {
                Action callback = OnComplete;
                OnComplete = null;
                callback?.Invoke();
            }
        }

        /// <summary>
        /// Експоненційне слідкування за цільовою позицією з заданою різкістю.
        /// </summary>
        private sealed class FollowMotion : ActiveMotion
        {
            /// <summary>Цільова позиція слідкування.</summary>
            public Vector3 Target;
            /// <summary>Різкість експоненційного наближення.</summary>
            public float Sharpness;

            /// <summary>Крокує позицію до цілі; повертає true при досягненні.</summary>
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

        /// <summary>
        /// Eased-переміщення між двома позиціями з опційною параболічною дугою підйому.
        /// </summary>
        private sealed class PositionEaseMotion : ActiveMotion
        {
            private Vector3 _from;
            private Vector3 _to;
            private float _lift;
            private float _duration;
            private float _elapsed;
            private MotionEaseKind _ease;

            /// <summary>Перезапускає рух із новими параметрами.</summary>
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

            /// <summary>Інтерполює позицію; повертає true по завершенні.</summary>
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

        /// <summary>
        /// Eased-обертання yaw між двома кутами по найкоротшій дузі.
        /// </summary>
        private sealed class YawEaseMotion : ActiveMotion
        {
            private float _fromDeg;
            private float _toDeg;
            private float _duration;
            private float _elapsed;
            private MotionEaseKind _ease;

            /// <summary>Перезапускає обертання з новими параметрами.</summary>
            public void Restart(float fromDeg, float toDeg, float duration, MotionEaseKind ease)
            {
                _fromDeg = fromDeg;
                _toDeg = toDeg;
                _duration = duration;
                _ease = ease;
                _elapsed = 0f;
            }

            /// <summary>Інтерполює кут yaw; повертає true по завершенні.</summary>
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

        /// <summary>
        /// Eased-зміна localScale між двома значеннями.
        /// </summary>
        private sealed class ScaleEaseMotion : ActiveMotion
        {
            private Vector3 _from;
            private Vector3 _to;
            private float _duration;
            private float _elapsed;
            private MotionEaseKind _ease;

            /// <summary>Перезапускає зміну масштабу з новими параметрами.</summary>
            public void Restart(Vector3 from, Vector3 to, float duration, MotionEaseKind ease, Action onComplete)
            {
                _from = from;
                _to = to;
                _duration = duration;
                _ease = ease;
                _elapsed = 0f;
                OnComplete = onComplete;
            }

            /// <summary>Інтерполює масштаб; повертає true по завершенні.</summary>
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

        /// <summary>
        /// Адитивний позиційний імпульс "туди-назад" із гарантованим нульовим залишком.
        /// </summary>
        private sealed class PunchMotion : ActiveMotion
        {
            private Vector3 _offset;
            private float _duration;
            private float _elapsed;
            private float _applied;
            private MotionEaseKind _ease;

            /// <summary>Перезапускає імпульс з новими параметрами.</summary>
            public void Restart(Vector3 offset, float duration, MotionEaseKind ease, Action onComplete)
            {
                _offset = offset;
                _duration = duration;
                _ease = ease;
                _elapsed = 0f;
                _applied = 0f;
                OnComplete = onComplete;
            }

            /// <summary>Накладає дельту імпульсу; повертає true по завершенні.</summary>
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

            /// <summary>Знімає ще-застосований залишок, щоб скасування не лишало дрейфу.</summary>
            public override void OnCancelled(EntityMotion owner)
            {
                // Remove the still-applied residual so cancellation never leaves drift.
                owner.transform.position -= _offset * _applied;
                _offset = Vector3.zero;
                _applied = 0f;
            }
        }

        /// <summary>
        /// Канал повністю кастомного руху з довільним евалюатором за eased t.
        /// </summary>
        private sealed class CustomMotion : ActiveMotion
        {
            /// <summary>Тривалість руху в секундах.</summary>
            public float Duration;
            /// <summary>Накопичений час руху.</summary>
            public float Elapsed;
            /// <summary>Крива easing для цього руху.</summary>
            public MotionEaseKind Ease;
            /// <summary>Евалюатор, що застосовує eased t до трансформа.</summary>
            public Action<float> Apply;

            /// <summary>Викликає евалюатор; повертає true по завершенні.</summary>
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
