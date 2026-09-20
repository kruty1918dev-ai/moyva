using System;
using Kruty1918.Moyva.Jsonization;
using UnityEngine;

namespace Kruty1918.Moyva.Animations.API
{
    /// <summary>
    /// Centralized gameplay motion tuning. JSON preset model: "gameplay-motion"
    /// (schema moyva.gameplay-motion). Per-unit-class overrides live in
    /// <see cref="PathAnimationSettings"/>; these profiles are the shared
    /// defaults and the canonical vocabulary for building/unit transitions.
    /// </summary>
    [Serializable]
    public sealed class GameplayMotionConfig : MoyvaJsonConfigObject
    {
        public UnitLocomotionMotionProfile unitLocomotion = new UnitLocomotionMotionProfile();
        public UnitTransitionMotionProfile unitTransitions = new UnitTransitionMotionProfile();
        public BuildingMotionProfile building = new BuildingMotionProfile();
        public MotionAccessibilityProfile accessibility = new MotionAccessibilityProfile();
    }

    /// <summary>Continuous locomotion defaults for units (accel/decel/turn/bob).</summary>
    [Serializable]
    public sealed class UnitLocomotionMotionProfile
    {
        [Min(0.01f)] public float acceleration = 24f;
        [Min(0.01f)] public float deceleration = 30f;
        [Min(0f)] public float turnSpeedDegPerSec = 540f;
        public bool faceTravelDirection = true;
        [Min(0f)] public float bobAmplitude = 0.03f;
        [Tooltip("Bob cycles per world unit travelled — speed-scaled footfall rhythm.")]
        [Min(0f)] public float bobFrequency = 0.9f;
        [Tooltip("Heading anticipation: fraction of a tile before a corner where facing starts to blend toward the next segment.")]
        [Range(0f, 1f)] public float cornerAnticipation = 0.45f;
    }

    /// <summary>Non-locomotion unit transitions: deploy, garrison, hit, death, attack lunge.</summary>
    [Serializable]
    public sealed class UnitTransitionMotionProfile
    {
        [Min(0f)] public float deployDuration = 0.45f;
        public float deployRiseHeight = 0.25f;
        [Range(0.1f, 1f)] public float deployScaleStart = 0.85f;
        public MotionEaseKind deployEase = MotionEaseKind.OutCubic;

        [Min(0f)] public float deathDuration = 0.6f;
        public float deathSinkDepth = 0.3f;
        public float deathTiltDeg = 12f;
        public MotionEaseKind deathEase = MotionEaseKind.InQuad;

        [Min(0f)] public float hitRecoilDuration = 0.14f;
        public float hitRecoilDistance = 0.12f;
        public MotionEaseKind hitRecoilEase = MotionEaseKind.OutQuad;

        [Min(0f)] public float attackLungeDuration = 0.18f;
        public float attackLungeDistance = 0.18f;
        public MotionEaseKind attackLungeEase = MotionEaseKind.OutQuad;

        [Min(0f)] public float garrisonEnterDuration = 0.35f;
        [Min(0f)] public float garrisonExitDuration = 0.4f;
        [Range(0.05f, 1f)] public float garrisonShrinkScale = 0.4f;
        public MotionEaseKind garrisonEase = MotionEaseKind.InOutQuad;
    }

    /// <summary>Preview/placed building motion vocabulary.</summary>
    [Serializable]
    public sealed class BuildingMotionProfile
    {
        [Min(0.01f)] public float previewMoveSharpness = 18f;
        [Min(0.01f)] public float previewDragSharpness = 28f;
        [Min(0.01f)] public float previewSnapSharpness = 14f;
        [Min(0.01f)] public float placedSnapSharpness = 20f;

        [Min(0f)] public float previewRotateDuration = 0.15f;
        public MotionEaseKind previewRotateEase = MotionEaseKind.OutCubic;

        [Tooltip("Placement emerge: how far below final Y the building starts, in world units.")]
        [Min(0f)] public float emergeDepth = 0.35f;
        [Min(0f)] public float emergeDuration = 0.5f;
        public MotionEaseKind emergeEase = MotionEaseKind.OutCubic;
        [Range(0.5f, 1f)] public float emergeScaleYStart = 0.92f;

        [Min(0f)] public float demolishDuration = 0.55f;
        public float demolishSinkDepth = 0.35f;
        public float demolishShakeAmplitude = 0.05f;
        public MotionEaseKind demolishEase = MotionEaseKind.InQuad;

        [Min(0f)] public float operationalDuration = 0.45f;
        [Range(0.5f, 1f)] public float operationalStartScale = 0.96f;
        public float operationalRiseHeight = 0.12f;
        public MotionEaseKind operationalEase = MotionEaseKind.OutCubic;

        [Tooltip("Relocation travel speed in world units/sec; lift is a small arc on top.")]
        [Min(0.01f)] public float relocationSpeed = 8f;
        public float relocationLift = 0.3f;
        public MotionEaseKind relocationEase = MotionEaseKind.InOutQuad;

        public float blockedShakeAmplitude = 0.06f;
        [Min(0f)] public float blockedShakeDuration = 0.18f;
    }

    /// <summary>
    /// Quality/accessibility scaling. Reduced Motion shortens transitions and
    /// removes secondary motion (bob, overshoot, shake); quality tiers scale
    /// secondary amplitude on low-end devices.
    /// </summary>
    [Serializable]
    public sealed class MotionAccessibilityProfile
    {
        [Range(0f, 1f)] public float reducedMotionDurationScale = 0.35f;
        [Range(0f, 1f)] public float secondaryMotionPerformanceScale = 0.3f;
        [Range(0f, 1f)] public float secondaryMotionBalancedScale = 1f;
        [Range(0f, 1f)] public float secondaryMotionQualityScale = 1f;
    }
}
