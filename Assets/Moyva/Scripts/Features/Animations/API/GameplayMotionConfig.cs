using System;
using Kruty1918.JsonConfig;
using UnityEngine;

namespace Kruty1918.Moyva.Animations.API
{
    /// <summary>Централізований тюнінг gameplay-рухів. Модель JSON-пресета "gameplay-motion" (schema moyva.gameplay-motion). Перевизначення за класом юніта живуть у PathAnimationSettings; ці профілі — спільні усталені значення та канонічний словник переходів будівель/юнітів.</summary>
    [Serializable]
    public sealed class GameplayMotionConfig : JsonConfigObject
    {
        /// <summary>Профіль безперервного переміщення юнітів.</summary>
        public UnitLocomotionMotionProfile unitLocomotion = new UnitLocomotionMotionProfile();
        /// <summary>Профіль не-локомоційних переходів юнітів.</summary>
        public UnitTransitionMotionProfile unitTransitions = new UnitTransitionMotionProfile();
        /// <summary>Профіль рухів будівель (preview/placed).</summary>
        public BuildingMotionProfile building = new BuildingMotionProfile();
        /// <summary>Профіль доступності та якісного масштабування рухів.</summary>
        public MotionAccessibilityProfile accessibility = new MotionAccessibilityProfile();
    }

    /// <summary>Усталені параметри безперервної локомоції юнітів (прискорення/поворот/bob).</summary>
    [Serializable]
    public sealed class UnitLocomotionMotionProfile
    {
        /// <summary>Чи повертатися в напрямку руху.</summary>
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

    /// <summary>Не-локомоційні переходи юніта: розгортання, гарнізон, влучання, смерть, ривок атаки.</summary>
    [Serializable]
    public sealed class UnitTransitionMotionProfile
    {
        /// <summary>Висота підйому під час розгортання.</summary>
        [Min(0f)] public float deployDuration = 0.45f;
        public float deployRiseHeight = 0.25f;
        /// <summary>Крива easing розгортання.</summary>
        [Range(0.1f, 1f)] public float deployScaleStart = 0.85f;
        public MotionEaseKind deployEase = MotionEaseKind.OutCubic;

        /// <summary>Глибина занурення під час смерті.</summary>
        [Min(0f)] public float deathDuration = 0.6f;
        public float deathSinkDepth = 0.3f;
        /// <summary>Кут нахилу під час смерті, градусів.</summary>
        public float deathTiltDeg = 12f;
        /// <summary>Крива easing смерті.</summary>
        public MotionEaseKind deathEase = MotionEaseKind.InQuad;

        /// <summary>Дистанція відкату при влучанні.</summary>
        [Min(0f)] public float hitRecoilDuration = 0.14f;
        public float hitRecoilDistance = 0.12f;
        /// <summary>Крива easing відкату при влучанні.</summary>
        public MotionEaseKind hitRecoilEase = MotionEaseKind.OutQuad;

        /// <summary>Дистанція ривка атаки.</summary>
        [Min(0f)] public float attackLungeDuration = 0.18f;
        public float attackLungeDistance = 0.18f;
        /// <summary>Крива easing ривка атаки.</summary>
        public MotionEaseKind attackLungeEase = MotionEaseKind.OutQuad;

        /// <summary>Крива easing входу/виходу гарнізону.</summary>
        [Min(0f)] public float garrisonEnterDuration = 0.35f;
        [Min(0f)] public float garrisonExitDuration = 0.4f;
        [Range(0.05f, 1f)] public float garrisonShrinkScale = 0.4f;
        public MotionEaseKind garrisonEase = MotionEaseKind.InOutQuad;
    }

    /// <summary>Словник рухів для preview та розміщених будівель.</summary>
    [Serializable]
    public sealed class BuildingMotionProfile
    {
        [Min(0.01f)] public float previewMoveSharpness = 18f;
        [Min(0.01f)] public float previewDragSharpness = 28f;
        [Min(0.01f)] public float previewSnapSharpness = 14f;
        [Min(0.01f)] public float placedSnapSharpness = 20f;

        /// <summary>Крива easing повороту preview.</summary>
        [Min(0f)] public float previewRotateDuration = 0.15f;
        public MotionEaseKind previewRotateEase = MotionEaseKind.OutCubic;

        /// <summary>Крива easing появи будівлі при розміщенні.</summary>
        [Tooltip("Placement emerge: how far below final Y the building starts, in world units.")]
        [Min(0f)] public float emergeDepth = 0.35f;
        [Min(0f)] public float emergeDuration = 0.5f;
        public MotionEaseKind emergeEase = MotionEaseKind.OutCubic;
        [Range(0.5f, 1f)] public float emergeScaleYStart = 0.92f;

        /// <summary>Глибина занурення під час знесення.</summary>
        [Min(0f)] public float demolishDuration = 0.55f;
        public float demolishSinkDepth = 0.35f;
        /// <summary>Амплітуда тремтіння під час знесення.</summary>
        public float demolishShakeAmplitude = 0.05f;
        /// <summary>Крива easing знесення.</summary>
        public MotionEaseKind demolishEase = MotionEaseKind.InQuad;

        /// <summary>Висота підйому при переході в робочий стан.</summary>
        [Min(0f)] public float operationalDuration = 0.45f;
        [Range(0.5f, 1f)] public float operationalStartScale = 0.96f;
        public float operationalRiseHeight = 0.12f;
        /// <summary>Крива easing переходу в робочий стан.</summary>
        public MotionEaseKind operationalEase = MotionEaseKind.OutCubic;

        /// <summary>Висота дуги підйому при переміщенні будівлі.</summary>
        [Tooltip("Relocation travel speed in world units/sec; lift is a small arc on top.")]
        [Min(0.01f)] public float relocationSpeed = 8f;
        public float relocationLift = 0.3f;
        /// <summary>Крива easing переміщення будівлі.</summary>
        public MotionEaseKind relocationEase = MotionEaseKind.InOutQuad;

        /// <summary>Амплітуда тремтіння при заблокованій дії.</summary>
        public float blockedShakeAmplitude = 0.06f;
        [Min(0f)] public float blockedShakeDuration = 0.18f;
    }

    /// <summary>Масштабування якості/доступності. Reduced Motion скорочує переходи та прибирає вторинний рух (bob, overshoot, тремтіння); рівні якості масштабують вторинну амплітуду на слабких пристроях.</summary>
    [Serializable]
    public sealed class MotionAccessibilityProfile
    {
        [Range(0f, 1f)] public float reducedMotionDurationScale = 0.35f;
        [Range(0f, 1f)] public float secondaryMotionPerformanceScale = 0.3f;
        [Range(0f, 1f)] public float secondaryMotionBalancedScale = 1f;
        [Range(0f, 1f)] public float secondaryMotionQualityScale = 1f;
    }
}
