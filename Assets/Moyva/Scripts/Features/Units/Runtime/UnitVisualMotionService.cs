using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Animations.Runtime.Motion;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    /// <summary>
    /// Presentation-only unit lifecycle transitions: recruitment deployment
    /// emerge, garrison enter/exit and death follow-through. Authoritative
    /// state is already committed by the domain services before these run —
    /// every transition can be cancelled, fast-forwarded or skipped without
    /// gameplay consequences.
    /// </summary>
    internal sealed class UnitVisualMotionService : IInitializable, IDisposable
    {
        private readonly SignalBus _signals;
        private readonly IUnitService _units;
        private readonly IUnitClassConfig _configs;
        private readonly IGameplayMotionSettingsProvider _motion;
        private readonly IUnitWorldPositionResolver _worldPositionResolver;
        private readonly HashSet<GameObject> _dying = new();

        public UnitVisualMotionService(
            SignalBus signals,
            IUnitService units,
            IUnitClassConfig configs,
            [InjectOptional] IGameplayMotionSettingsProvider motion = null,
            [InjectOptional] IUnitWorldPositionResolver worldPositionResolver = null)
        {
            _signals = signals;
            _units = units;
            _configs = configs;
            _motion = motion;
            _worldPositionResolver = worldPositionResolver;
        }

        public void Initialize()
        {
            _signals.Subscribe<UnitRecruitmentDeployedSignal>(HandleDeployed);
        }

        public void Dispose()
        {
            _signals.TryUnsubscribe<UnitRecruitmentDeployedSignal>(HandleDeployed);
            _dying.Clear();
        }

        // ── Recruitment deployment ───────────────────────────────────────────

        private void HandleDeployed(UnitRecruitmentDeployedSignal signal)
        {
            GameObject unitObject = _units.GetUnitObject(signal.UnitId);
            if (unitObject == null || !unitObject.activeSelf)
                return;

            UnitTransitionMotionProfile p = Transitions;
            float duration = ScaleDuration(p.deployDuration);
            if (duration <= 0f)
                return;

            Vector3 finalPosition = unitObject.transform.position;
            Vector3 finalScale = unitObject.transform.localScale;
            float rise = ScaleSecondary(p.deployRiseHeight);
            float scaleStart = Mathf.Lerp(1f, p.deployScaleStart, SecondaryScale);

            EntityMotion motion = EntityMotion.AttachOrUpdate(unitObject);
            if (motion == null)
                return;

            unitObject.transform.position = finalPosition + Vector3.down * rise;
            unitObject.transform.localScale = finalScale * scaleStart;
            motion.PlayCustom(duration, p.deployEase, t =>
            {
                unitObject.transform.position = finalPosition + Vector3.down * (rise * (1f - t));
                unitObject.transform.localScale = Vector3.LerpUnclamped(finalScale * scaleStart, finalScale, t);
            });
        }

        // ── Garrison ─────────────────────────────────────────────────────────

        /// <summary>
        /// Visual enter: unit slides toward the building and shrinks away.
        /// Returns true when the presentation owns the deactivation.
        /// </summary>
        public bool TryPlayGarrisonEnter(GameObject unitObject, Vector2Int buildingPosition)
        {
            if (unitObject == null || !unitObject.activeSelf)
                return false;
            if (_worldPositionResolver == null || !_worldPositionResolver.Uses3DWorldPlane)
                return false;

            UnitTransitionMotionProfile p = Transitions;
            float duration = ScaleDuration(p.garrisonEnterDuration);
            if (duration <= 0f)
                return false;

            EntityMotion motion = EntityMotion.AttachOrUpdate(unitObject);
            if (motion == null)
                return false;

            Vector3 startPosition = unitObject.transform.position;
            Vector3 startScale = unitObject.transform.localScale;
            Vector3 buildingWorld = _worldPositionResolver.ResolveWorldPosition(buildingPosition);
            Vector3 endPosition = new Vector3(buildingWorld.x, startPosition.y, buildingWorld.z);
            Vector3 endScale = startScale * Mathf.Lerp(1f, p.garrisonShrinkScale, SecondaryScale + 0.5f);

            motion.PlayCustom(duration, p.garrisonEase, t =>
            {
                unitObject.transform.position = Vector3.LerpUnclamped(startPosition, endPosition, t);
                unitObject.transform.localScale = Vector3.LerpUnclamped(startScale, endScale, t);
            }, () =>
            {
                unitObject.transform.localScale = startScale;
                if (unitObject != null)
                    unitObject.SetActive(false);
            });
            return true;
        }

        /// <summary>
        /// Visual exit: unit emerges at the building edge, grows back and slides
        /// to the target tile. <paramref name="finalizePose"/> applies canonical
        /// presentation transform once the motion lands.
        /// Returns true when the presentation owns the positioning.
        /// </summary>
        public bool TryPlayGarrisonExit(
            GameObject unitObject,
            Vector2Int buildingPosition,
            Vector2Int targetPosition,
            Action<GameObject> finalizePose)
        {
            if (unitObject == null || !unitObject.activeSelf)
                return false;
            if (_worldPositionResolver == null || !_worldPositionResolver.Uses3DWorldPlane)
                return false;

            UnitTransitionMotionProfile p = Transitions;
            float duration = ScaleDuration(p.garrisonExitDuration);
            if (duration <= 0f)
                return false;

            EntityMotion motion = EntityMotion.AttachOrUpdate(unitObject);
            if (motion == null)
                return false;

            Vector3 buildingWorld = _worldPositionResolver.ResolveWorldPosition(buildingPosition);
            Vector3 targetWorld = _worldPositionResolver.ResolveWorldPosition(targetPosition);
            Vector3 startPosition = new Vector3(buildingWorld.x, targetWorld.y, buildingWorld.z);
            Vector3 endScale = unitObject.transform.localScale;
            Vector3 smallScale = endScale * Mathf.Lerp(1f, p.garrisonShrinkScale, SecondaryScale + 0.5f);

            unitObject.transform.position = startPosition;
            unitObject.transform.localScale = smallScale;

            motion.PlayCustom(duration, p.garrisonEase, t =>
            {
                unitObject.transform.position = Vector3.LerpUnclamped(startPosition, targetWorld, t);
                unitObject.transform.localScale = Vector3.LerpUnclamped(smallScale, endScale, t);
            }, () =>
            {
                unitObject.transform.position = targetWorld;
                unitObject.transform.localScale = endScale;
                finalizePose?.Invoke(unitObject);
            });
            return true;
        }

        // ── Death ────────────────────────────────────────────────────────────

        /// <summary>
        /// Death follow-through: recoil, tilt, sink — then the object is
        /// destroyed. Gameplay never waits on this; the unit is already gone
        /// from registries before this runs.
        /// Returns true when the presentation owns the destruction.
        /// </summary>
        public bool TryPlayDeath(GameObject unitObject, string unitTypeId)
        {
            if (unitObject == null || !unitObject.activeSelf || _dying.Contains(unitObject))
                return false;

            UnitTransitionMotionProfile p = Transitions;
            float duration = ScaleDuration(p.deathDuration);
            if (duration <= 0f)
                return false;

            EntityMotion motion = EntityMotion.AttachOrUpdate(unitObject);
            if (motion == null)
                return false;

            _dying.Add(unitObject);
            motion.CancelAll();

            UnitClassConfig config = string.IsNullOrWhiteSpace(unitTypeId)
                ? null
                : _configs?.GetConfig(unitTypeId);
            UnitAnimationTrigger.Play(unitObject, config, AnimationType.Die);

            Vector3 startPosition = unitObject.transform.position;
            Quaternion startRotation = unitObject.transform.rotation;
            Vector3 startScale = unitObject.transform.localScale;
            float sink = Mathf.Max(0f, p.deathSinkDepth);
            float tilt = p.deathTiltDeg * Mathf.Max(0.4f, SecondaryScale);
            Vector3 tiltAxis = unitObject.transform.right.sqrMagnitude > 0.001f
                ? unitObject.transform.right
                : Vector3.right;

            motion.PlayCustom(duration, p.deathEase, t =>
            {
                unitObject.transform.position = startPosition + Vector3.down * (sink * t);
                unitObject.transform.rotation = startRotation * Quaternion.AngleAxis(tilt * t, tiltAxis);
                unitObject.transform.localScale = startScale * (1f - 0.12f * t);
            }, () =>
            {
                _dying.Remove(unitObject);
                if (unitObject != null)
                    UnityEngine.Object.Destroy(unitObject);
            });
            return true;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private UnitTransitionMotionProfile Transitions
            => _motion?.UnitTransitions ?? new UnitTransitionMotionProfile();

        private float SecondaryScale
            => _motion?.SecondaryMotionScale ?? 1f;

        private float ScaleDuration(float seconds)
            => _motion?.ScaleDuration(seconds) ?? seconds;

        private float ScaleSecondary(float amplitude)
            => _motion?.ScaleSecondaryAmplitude(amplitude) ?? amplitude;
    }
}
