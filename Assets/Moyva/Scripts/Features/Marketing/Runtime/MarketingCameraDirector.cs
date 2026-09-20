using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// Drives the single capture camera through ShotPlans. Rig archetypes are
    /// parameterized behaviours applied to one camera — no per-shot objects.
    /// Motion envelopes are deliberately restrained (no zoom spam, no random
    /// per-second reframing).
    /// </summary>
    public sealed class MarketingCameraDirector
    {
        private readonly Camera _camera;
        private float _aspect = 16f / 9f;
        private ShotPlan _shot;
        private Vector3 _subjectPos;
        private float _subjectRadius = 2f;
        private float _azimuthDeg;
        private float _pitchDeg = 35f;
        private float _distance = 12f;
        private float _height;
        private Vector3 _basePos;

        public MarketingCameraDirector(Camera camera)
        {
            _camera = camera;
        }

        public void SetAspect(float aspect)
        {
            _aspect = Mathf.Max(0.3f, aspect);
        }

        public void ApplyShot(ShotPlan shot, float fallbackAzimuthDeg)
        {
            _shot = shot;
            _subjectPos = new Vector3(shot.subject.worldX, shot.subject.worldY, shot.subject.worldZ);
            _subjectRadius = Mathf.Max(0.5f, shot.subject.approximateRadius);

            var rng = new Planning.MarketingRng(shot.seed);
            _azimuthDeg = fallbackAzimuthDeg + rng.Range(-25f, 25f);

            float scale = FramingScale(shot.rig);
            // Distance to fit subject within fov; vertical framing tightens for
            // portrait aspects so the subject survives the narrow crop.
            float fov = _camera != null ? _camera.fieldOfView : 50f;
            float fit = _subjectRadius * scale / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            float aspectComp = _aspect < 1f ? Mathf.Lerp(1f, 1.6f, 1f - _aspect) : 1f;
            _distance = Mathf.Max(2f, fit * aspectComp);
            _pitchDeg = PitchFor(shot.rig);
            _height = HeightFor(shot.rig, _subjectRadius);
        }

        /// <summary>Position camera for shot progress t ∈ [0,1].</summary>
        public void Evaluate(float t)
        {
            if (_camera == null || _shot == null) return;
            float tt = Mathf.Clamp01(t);
            float motion = Mathf.Clamp01(_shot.motionSpeed <= 0f ? 0.5f : _shot.motionSpeed);

            float azimuth = _azimuthDeg;
            float pitch = _pitchDeg;
            float distance = _distance;
            float height = _height;
            Vector3 lateral = Vector3.zero;

            switch (_shot.motion)
            {
                case ShotMotion.SlowPushIn:
                    distance *= Mathf.Lerp(1.18f, 1f, tt) * (1f - 0.10f * motion * tt);
                    break;
                case ShotMotion.SlowPullOut:
                    distance *= Mathf.Lerp(1f, 1.2f, tt);
                    break;
                case ShotMotion.LateralTrack:
                {
                    Vector3 fwd = Quaternion.Euler(pitch, azimuth, 0f) * Vector3.forward;
                    Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
                    lateral = right * Mathf.Lerp(-1f, 1f, tt) * _subjectRadius * 0.8f * motion;
                    break;
                }
                case ShotMotion.Orbit:
                    azimuth += Mathf.Lerp(-9f, 9f, tt) * (0.4f + 0.6f * motion);
                    break;
                case ShotMotion.FullOrbit:
                    azimuth += 360f * tt; // full circle → seamless video loop
                    break;
                case ShotMotion.Crane:
                    height += Mathf.Lerp(0f, _subjectRadius * 0.9f, tt) * motion;
                    pitch = Mathf.Lerp(pitch, Mathf.Min(pitch + 8f, 80f), tt * motion);
                    break;
            }

            Quaternion rot = Quaternion.Euler(pitch, azimuth, 0f);
            Vector3 offset = rot * (Vector3.back * distance) + Vector3.up * height + lateral;
            Vector3 pos = _subjectPos + offset;
            _basePos = pos;

            _camera.transform.position = pos;
            _camera.transform.rotation = Quaternion.LookRotation(_subjectPos - pos, Vector3.up);
        }

        /// <summary>Normalized viewport position of the subject center (for
        /// golden-frame metrics / crop focus).</summary>
        public Vector2 SubjectViewportPoint()
        {
            if (_camera == null) return new Vector2(0.5f, 0.5f);
            Vector3 vp = _camera.WorldToViewportPoint(_subjectPos);
            return new Vector2(Mathf.Clamp01(vp.x), Mathf.Clamp01(vp.y));
        }

        public Vector3 SubjectPosition => _subjectPos;
        public Vector3 CameraPosition => _camera != null ? _camera.transform.position : Vector3.zero;

        private float FramingScale(CameraRigType rig) => rig switch
        {
            CameraRigType.WorldReveal => 3.5f,
            CameraRigType.TacticalOverview => 3f,
            CameraRigType.TopDown => 1.9f,
            CameraRigType.SettlementWide => 3.2f,
            CameraRigType.BattleWide => 3.5f,
            CameraRigType.SettlementMedium => 2.2f,
            CameraRigType.BattleMedium => 2.2f,
            CameraRigType.ArmyTrack => 2.6f,
            CameraRigType.ArmySideTrack => 2.6f,
            CameraRigType.BuildingHero => 1.6f,
            CameraRigType.UnitHero => 1.4f,
            CameraRigType.BattleClose => 1.5f,
            CameraRigType.LowAngle => 2.0f,
            CameraRigType.Aftermath => 3.0f,
            CameraRigType.Follow => 2.4f,
            CameraRigType.Dolly => 2.4f,
            _ => 2.5f,
        };

        private float PitchFor(CameraRigType rig) => rig switch
        {
            CameraRigType.TopDown => 88f,
            CameraRigType.TacticalOverview => 55f,
            CameraRigType.WorldReveal => 30f,
            CameraRigType.LowAngle => 6f,
            CameraRigType.UnitHero => 14f,
            CameraRigType.BuildingHero => 18f,
            CameraRigType.Aftermath => 25f,
            _ => 32f,
        };

        private float HeightFor(CameraRigType rig, float radius) => rig switch
        {
            CameraRigType.LowAngle => radius * 0.4f,
            CameraRigType.UnitHero => radius * 0.5f,
            CameraRigType.BuildingHero => radius * 0.5f,
            _ => 0f,
        };
    }
}
