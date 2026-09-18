using System;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using ReactUnity.UGUI;
using ReactUnity.UGUI.Behaviours;
using UnityEngine;

namespace UnityHTML.Runtime
{
    public sealed class UnityHtmlMotionBridge : IUnityHtmlMotion
    {
        private const float DefaultDuration = 0.16f;
        private const float DefaultDistance = 20f;

        private readonly Dictionary<string, ActiveMotion> _active = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _declared = new(StringComparer.Ordinal);
        private readonly HashSet<string> _seen = new(StringComparer.Ordinal);
        private readonly List<string> _stale = new(8);
        private readonly List<ReactElement> _elements = new(96);
        private RectTransform _root;

        public void Play(string targetId, string preset, float duration, float delay)
        {
            if (!Application.isPlaying || !TryFindTarget(targetId, out RectTransform target))
                return;

            Play(targetId, target, preset, duration, delay, DefaultDistance, Ease.OutCubic);
        }

        public void Stop(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId) || !_active.Remove(targetId, out ActiveMotion motion))
                return;

            motion.Tween?.Kill(false);
            motion.Restore();
        }

        internal void Attach(RectTransform root)
        {
            _root = root;
        }

        internal void PrepareForDocumentUpdate()
        {
            StopAll();
        }

        internal void ApplyDeclaredMotions()
        {
            if (!Application.isPlaying || _root == null)
                return;

            _seen.Clear();
            CollectElements();
            for (int index = 0; index < _elements.Count; index++)
            {
                UGUIComponent component = _elements[index] != null ? _elements[index].Component : null;
                if (component == null
                    || string.IsNullOrWhiteSpace(component.Id)
                    || !TryData(component, "motion", out string preset))
                {
                    continue;
                }

                string id = component.Id;
                string durationText = Data(component, "motion-duration");
                string delayText = Data(component, "motion-delay");
                string distanceText = Data(component, "motion-distance");
                string easeText = Data(component, "motion-ease");
                string signature = $"{component.RectTransform.GetInstanceID()}|{preset}|{durationText}|{delayText}|{distanceText}|{easeText}";
                _seen.Add(id);

                if (_declared.TryGetValue(id, out string previous) && previous == signature)
                    continue;

                _declared[id] = signature;
                try
                {
                    Play(
                        id,
                        component.RectTransform,
                        preset,
                        Number(durationText, DefaultDuration),
                        Number(delayText, 0f),
                        Number(distanceText, DefaultDistance),
                        ResolveEase(easeText));
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[UnityHTML Motion] Skipped '{id}' motion '{preset}': {exception.GetBaseException().Message}");
                }
            }

            _stale.Clear();
            foreach (string id in _declared.Keys)
            {
                if (!_seen.Contains(id))
                    _stale.Add(id);
            }
            for (int index = 0; index < _stale.Count; index++)
                _declared.Remove(_stale[index]);
        }

        internal void Detach()
        {
            StopAll();
            _declared.Clear();
            _root = null;
        }

        private void Play(
            string id,
            RectTransform target,
            string preset,
            float duration,
            float delay,
            float distance,
            Ease ease)
        {
            if (target == null || string.IsNullOrWhiteSpace(id))
                return;

            Stop(id);
            duration = Mathf.Clamp(duration, 0.04f, 2f);
            delay = Mathf.Clamp(delay, 0f, 2f);
            distance = Mathf.Clamp(Mathf.Abs(distance), 0f, 160f);

            string normalizedPreset = (preset ?? string.Empty).Trim().ToLowerInvariant();
            CanvasGroup group = RequiresAlpha(normalizedPreset) ? TryEnsureCanvasGroup(target) : null;
            var motion = new ActiveMotion(target, group);
            Sequence sequence = DOTween.Sequence()
                .SetUpdate(true)
                .SetRecyclable(true)
                .SetLink(target.gameObject, LinkBehaviour.KillOnDestroy)
                .SetTarget(target);

            switch (normalizedPreset)
            {
                case "fade":
                    if (group == null)
                    {
                        sequence.Kill(false);
                        return;
                    }
                    group.alpha = 0f;
                    sequence.Append(Fade(group, motion.Alpha, duration, ease));
                    break;
                case "slide-left":
                    target.anchoredPosition = motion.Position + Vector2.right * distance;
                    AppendMoveAndFade(sequence, motion, duration, ease);
                    break;
                case "slide-right":
                    target.anchoredPosition = motion.Position + Vector2.left * distance;
                    AppendMoveAndFade(sequence, motion, duration, ease);
                    break;
                case "slide-up":
                    target.anchoredPosition = motion.Position + Vector2.down * distance;
                    AppendMoveAndFade(sequence, motion, duration, ease);
                    break;
                case "slide-down":
                    target.anchoredPosition = motion.Position + Vector2.up * distance;
                    AppendMoveAndFade(sequence, motion, duration, ease);
                    break;
                case "scale":
                    target.localScale = motion.Scale * 0.94f;
                    sequence.Append(Scale(target, motion.Scale, duration, ease));
                    if (group != null)
                    {
                        group.alpha = 0f;
                        sequence.Join(Fade(group, motion.Alpha, duration * 0.8f, Ease.OutQuad));
                    }
                    break;
                case "pulse":
                    sequence.Append(Scale(target, motion.Scale * 1.035f, duration * 0.45f, Ease.OutQuad))
                        .Append(Scale(target, motion.Scale, duration * 0.55f, Ease.InOutQuad));
                    break;
                default:
                    motion.Restore();
                    sequence.Kill(false);
                    return;
            }

            if (delay > 0f)
                sequence.SetDelay(delay);

            motion.Tween = sequence;
            _active[id] = motion;
            sequence.OnKill(() =>
            {
                if (_active.TryGetValue(id, out ActiveMotion current) && ReferenceEquals(current, motion))
                    _active.Remove(id);
            });
        }

        private static void AppendMoveAndFade(Sequence sequence, ActiveMotion motion, float duration, Ease ease)
        {
            sequence.Append(Move(motion.Target, motion.Position, duration, ease));
            if (motion.Group == null)
                return;

            motion.Group.alpha = 0f;
            sequence.Join(Fade(motion.Group, motion.Alpha, duration * 0.8f, Ease.OutQuad));
        }

        private static Tweener Move(RectTransform target, Vector2 end, float duration, Ease ease)
            => DOTween.To(() => target.anchoredPosition, value => target.anchoredPosition = value, end, duration).SetEase(ease);

        private static Tweener Scale(RectTransform target, Vector3 end, float duration, Ease ease)
            => DOTween.To(() => target.localScale, value => target.localScale = value, end, duration).SetEase(ease);

        private static Tweener Fade(CanvasGroup group, float end, float duration, Ease ease)
            => DOTween.To(() => group.alpha, value => group.alpha = value, end, duration).SetEase(ease);

        private static bool RequiresAlpha(string preset)
        {
            return preset == "fade"
                || preset == "slide-left"
                || preset == "slide-right"
                || preset == "slide-up"
                || preset == "slide-down"
                || preset == "scale";
        }

        private static CanvasGroup TryEnsureCanvasGroup(RectTransform target)
        {
            if (target == null)
                return null;

            try
            {
                CanvasGroup group = target.GetComponent<CanvasGroup>();
                return group != null ? group : target.gameObject.AddComponent<CanvasGroup>();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[UnityHTML Motion] CanvasGroup unavailable on '{target.name}': {exception.GetBaseException().Message}");
                return null;
            }
        }

        private bool TryFindTarget(string id, out RectTransform target)
        {
            target = null;
            if (_root == null || string.IsNullOrWhiteSpace(id))
                return false;

            CollectElements();
            for (int index = 0; index < _elements.Count; index++)
            {
                UGUIComponent component = _elements[index] != null ? _elements[index].Component : null;
                if (component != null && component.Id == id)
                {
                    target = component.RectTransform;
                    return target != null;
                }
            }
            return false;
        }

        private void CollectElements()
        {
            _elements.Clear();
            _root.GetComponentsInChildren(true, _elements);
        }

        private void StopAll()
        {
            if (_active.Count == 0)
                return;

            _stale.Clear();
            _stale.AddRange(_active.Keys);
            for (int index = 0; index < _stale.Count; index++)
                Stop(_stale[index]);
        }

        private static bool TryData(UGUIComponent component, string key, out string value)
        {
            value = Data(component, key);
            return !string.IsNullOrWhiteSpace(value);
        }

        private static string Data(UGUIComponent component, string key)
            => component.Data.TryGetValue(key, out object value) ? value?.ToString() ?? string.Empty : string.Empty;

        private static float Number(string value, float fallback)
            => float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : fallback;

        private static Ease ResolveEase(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "linear" => Ease.Linear,
                "out-quad" => Ease.OutQuad,
                "in-out-quad" => Ease.InOutQuad,
                "out-back" => Ease.OutBack,
                _ => Ease.OutCubic,
            };
        }

        private sealed class ActiveMotion
        {
            public ActiveMotion(RectTransform target, CanvasGroup group)
            {
                Target = target;
                Group = group;
                Position = target.anchoredPosition;
                Scale = target.localScale;
                Alpha = group != null ? group.alpha : 1f;
            }

            public RectTransform Target { get; }
            public CanvasGroup Group { get; }
            public Vector2 Position { get; }
            public Vector3 Scale { get; }
            public float Alpha { get; }
            public Tween Tween { get; set; }

            public void Restore()
            {
                if (Target != null)
                {
                    Target.anchoredPosition = Position;
                    Target.localScale = Scale;
                }
                if (Group != null)
                    Group.alpha = Alpha;
            }
        }
    }
}
