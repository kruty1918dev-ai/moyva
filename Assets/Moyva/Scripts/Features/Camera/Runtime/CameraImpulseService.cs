using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Shared.Controls;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>CameraImpulseService — class: камери імпульсу сервісу.</summary>
    internal sealed class CameraImpulseService : ICameraFeedbackService, ILateTickable, IDisposable
    {
        private const float KickAttackFraction = 0.06f;

        /// <summary>ActiveImpulse — struct: активної імпульсу.</summary>
        private struct ActiveImpulse
        {
            /// <summary>запиту — CameraImpulseRequest.</summary>
            public CameraImpulseRequest Request;
            /// <summary>Час, що минув з початку імпульсу.</summary>
            public float Elapsed;
            /// <summary>Зерно генератора для детермінованої тряски.</summary>
            public float Seed;
        }

        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;
        private readonly IPlayerControlSettingsService _controlSettings;
        private readonly List<ActiveImpulse> _active = new List<ActiveImpulse>(8);

        private Vector3 _positionOffset;
        private Quaternion _appliedRotationOffset = Quaternion.identity;

        /// <summary>Виконує CameraImpulseService.</summary>
        public CameraImpulseService(
            UnityEngine.Camera camera,
            CameraSettingsSO settings,
            [InjectOptional] IPlayerControlSettingsService controlSettings = null)
        {
            _camera = camera;
            _settings = settings;
            _controlSettings = controlSettings;
        }

        /// <summary>
        /// World-space positional offset composed by CameraMovement this frame.
        /// </summary>
        internal Vector3 PositionOffset => _positionOffset;

        /// <summary>Чи імпульсу активної — IsImpulseActive.</summary>
        public bool IsImpulseActive => _active.Count > 0;

        /// <summary>Запитує імпульсу.</summary>
        public void RequestImpulse(CameraImpulseRequest request)
        {
            if (_camera == null)
                return;

            request.PositionAmplitude = Mathf.Max(0f, request.PositionAmplitude);
            request.RotationAmplitude = Mathf.Max(0f, request.RotationAmplitude);
            request.Duration = Mathf.Max(0.05f, request.Duration);
            request.Frequency = Mathf.Clamp(request.Frequency <= 0f ? 12f : request.Frequency, 1f, 60f);
            if (request.KickDirection.sqrMagnitude > 0.0001f)
                request.KickDirection = request.KickDirection.normalized;

            if (request.PositionAmplitude <= 0.0001f && request.RotationAmplitude <= 0.0001f)
                return;

            var impulseSettings = _settings.ResolveImpulse();
            if (_active.Count >= impulseSettings.maxConcurrent)
            {
                int evictIndex = -1;
                for (int i = 0; i < _active.Count; i++)
                {
                    if (evictIndex < 0
                        || _active[i].Request.Priority < _active[evictIndex].Request.Priority
                        || (_active[i].Request.Priority == _active[evictIndex].Request.Priority
                            && _active[i].Elapsed > _active[evictIndex].Elapsed))
                    {
                        evictIndex = i;
                    }
                }

                if (evictIndex >= 0 && request.Priority <= _active[evictIndex].Request.Priority)
                    return; // lower-or-equal priority: queue is full, drop the new impulse

                _active.RemoveAt(evictIndex);
            }

            _active.Add(new ActiveImpulse
            {
                Request = request,
                Elapsed = 0f,
                Seed = Mathf.Repeat(UnityEngine.Random.value * 1000f, 1000f),
            });
        }

        /// <summary>Запитує імпульсу.</summary>
        public void RequestImpulse(CameraImpulseProfile profile, Vector3 worldPosition)
            => RequestImpulse(CameraImpulseProfiles.At(profile, worldPosition));

        /// <summary>Скасовує All Impulses.</summary>
        public void CancelAllImpulses()
        {
            _active.Clear();
            _positionOffset = Vector3.zero;
            RemoveRotationOffset();
        }

        /// <summary>Виконує LateTick.</summary>
        public void LateTick()
        {
            if (_camera == null)
                return;

            RemoveRotationOffset();

            if (_active.Count == 0)
            {
                _positionOffset = Vector3.zero;
                return;
            }

            var impulseSettings = _settings.ResolveImpulse();
            float playerScale = ResolvePlayerScale(impulseSettings);
            float zoomScale = ResolveZoomScale(impulseSettings);
            Vector3 cameraPosition = _camera.transform.position;
            Transform cameraTransform = _camera.transform;

            Vector3 position = Vector3.zero;
            Vector3 euler = Vector3.zero;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var impulse = _active[i];
                impulse.Elapsed += Time.unscaledDeltaTime;
                if (impulse.Elapsed >= impulse.Request.Duration)
                {
                    _active.RemoveAt(i);
                    continue;
                }
                _active[i] = impulse;

                var request = impulse.Request;
                float envelope = CameraViewMath.ImpulseEnvelope(impulse.Elapsed, request.Duration);
                float kick = CameraViewMath.ImpulseKickEnvelope(impulse.Elapsed, request.Duration);
                float scale = playerScale * zoomScale;

                if (request.HasWorldPosition)
                {
                    float radius = request.FalloffRadius > 0f
                        ? request.FalloffRadius
                        : impulseSettings.defaultFalloffRadius;
                    scale *= CameraViewMath.EvaluateDistanceFalloff(
                        Vector3.Distance(cameraPosition, request.WorldPosition), radius);
                }

                if (scale <= 0.0001f)
                    continue;

                float t = impulse.Elapsed * request.Frequency;
                float jx = Mathf.PerlinNoise(impulse.Seed, t) * 2f - 1f;
                float jy = Mathf.PerlinNoise(impulse.Seed + 57.31f, t) * 2f - 1f;
                float jz = Mathf.PerlinNoise(impulse.Seed + 113.7f, t) * 2f - 1f;

                Vector3 jitter = (cameraTransform.right * jx + cameraTransform.up * jy)
                    * (request.PositionAmplitude * envelope * scale);
                Vector3 kickOffset = request.KickDirection
                    * (request.PositionAmplitude * kick * scale * 0.8f);
                position += jitter + kickOffset;

                euler.x += jy * request.RotationAmplitude * envelope * scale;
                euler.y += jx * request.RotationAmplitude * envelope * scale * 0.5f;
                euler.z += jz * request.RotationAmplitude * envelope * scale;
            }

            _positionOffset = Vector3.ClampMagnitude(position, impulseSettings.maxPositionOffset);
            euler = Vector3.ClampMagnitude(euler, impulseSettings.maxRotationDegrees);

            if (euler.sqrMagnitude > 0.000001f)
            {
                _appliedRotationOffset = Quaternion.Euler(euler);
                cameraTransform.rotation *= _appliedRotationOffset;
            }
        }

        /// <summary>Звільняє ресурси та відписує від подій.</summary>
        public void Dispose()
        {
            _active.Clear();
            _positionOffset = Vector3.zero;
            RemoveRotationOffset();
        }

        private void RemoveRotationOffset()
        {
            if (_appliedRotationOffset == Quaternion.identity || _camera == null)
            {
                _appliedRotationOffset = Quaternion.identity;
                return;
            }

            _camera.transform.rotation *= Quaternion.Inverse(_appliedRotationOffset);
            _appliedRotationOffset = Quaternion.identity;
        }

        private float ResolvePlayerScale(CameraImpulseSettings impulseSettings)
        {
            if (_controlSettings == null)
                return 1f;

            var data = _controlSettings.Settings;
            if (!data.CameraEffects)
                return 0f;

            float scale = Mathf.Clamp01(data.CameraShakeIntensity);
            if (data.ReduceCameraMotion)
                scale *= impulseSettings.reduceMotionScale;
            if (Application.isMobilePlatform)
                scale *= impulseSettings.mobileScale;
            return scale;
        }

        private float ResolveZoomScale(CameraImpulseSettings impulseSettings)
        {
            float zoom = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;
            float normalized = CameraViewMath.NormalizeZoom(
                zoom, _settings.ResolveMinZoom(), _settings.ResolveMaxZoom());
            return Mathf.Lerp(1f, impulseSettings.farZoomScale, normalized);
        }
    }
}
