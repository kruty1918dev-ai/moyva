using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>CameraImpulseProfile — enum: камери імпульсу профілю.</summary>
    public enum CameraImpulseProfile
    {
        /// <summary>Варіант Tiny.</summary>
        Tiny,
        /// <summary>Варіант Light.</summary>
        Light,
        /// <summary>Варіант Heavy.</summary>
        Heavy,
        /// <summary>Варіант Strategic.</summary>
        Strategic,
    }

    /// <summary>CameraImpulsePriority — enum: камери імпульсу пріоритету.</summary>
    public enum CameraImpulsePriority
    {
        /// <summary>Варіант Low.</summary>
        Low,
        /// <summary>Варіант Medium.</summary>
        Medium,
        /// <summary>Варіант High.</summary>
        High,
        /// <summary>Варіант Critical.</summary>
        Critical,
    }

    /// <summary>CameraImpulseCategory — enum: камери імпульсу категорії.</summary>
    public enum CameraImpulseCategory
    {
        /// <summary>Варіант Generic.</summary>
        Generic,
        /// <summary>Варіант CombatImpact.</summary>
        CombatImpact,
        /// <summary>Варіант SiegeImpact.</summary>
        SiegeImpact,
        /// <summary>Варіант Demolition.</summary>
        Demolition,
        /// <summary>Варіант ConstructionSettle.</summary>
        ConstructionSettle,
        /// <summary>Варіант Capture.</summary>
        Capture,
        /// <summary>Варіант GameEvent.</summary>
        GameEvent,
    }

    /// <summary>CameraImpulseRequest — struct: камери імпульсу запиту.</summary>
    public struct CameraImpulseRequest
    {
        /// <summary>позицію амплітуди — float.</summary>
        public float PositionAmplitude;

        /// <summary>поворот амплітуди — float.</summary>
        public float RotationAmplitude;

        /// <summary>тривалості — float.</summary>
        public float Duration;

        /// <summary>частоти — float.</summary>
        public float Frequency;

        /// <summary>світу позицію — Vector3.</summary>
        public Vector3 WorldPosition;

        /// <summary>Чи світу позицію — HasWorldPosition.</summary>
        public bool HasWorldPosition;

        /// <summary>затухання радіуса — float.</summary>
        public float FalloffRadius;

        /// <summary>викиду напрямку — Vector3.</summary>
        public Vector3 KickDirection;

        /// <summary>пріоритету — CameraImpulsePriority.</summary>
        public CameraImpulsePriority Priority;
        /// <summary>категорії — CameraImpulseCategory.</summary>
        public CameraImpulseCategory Category;
    }

    /// <summary>CameraImpulseProfiles — class: камери імпульсу профілів.</summary>
    public static class CameraImpulseProfiles
    {
        /// <summary>Створює Create.</summary>
        public static CameraImpulseRequest Create(CameraImpulseProfile profile)
        {
            switch (profile)
            {
                case CameraImpulseProfile.Tiny:
                    return new CameraImpulseRequest
                    {
                        PositionAmplitude = 0.06f,
                        RotationAmplitude = 0.08f,
                        Duration = 0.22f,
                        Frequency = 16f,
                        Priority = CameraImpulsePriority.Low,
                    };
                case CameraImpulseProfile.Light:
                    return new CameraImpulseRequest
                    {
                        PositionAmplitude = 0.14f,
                        RotationAmplitude = 0.25f,
                        Duration = 0.3f,
                        Frequency = 13f,
                        Priority = CameraImpulsePriority.Medium,
                    };
                case CameraImpulseProfile.Heavy:
                    return new CameraImpulseRequest
                    {
                        PositionAmplitude = 0.32f,
                        RotationAmplitude = 0.6f,
                        Duration = 0.42f,
                        Frequency = 10f,
                        Priority = CameraImpulsePriority.High,
                    };
                case CameraImpulseProfile.Strategic:
                    return new CameraImpulseRequest
                    {
                        PositionAmplitude = 0.22f,
                        RotationAmplitude = 0.35f,
                        Duration = 0.55f,
                        Frequency = 7f,
                        Priority = CameraImpulsePriority.High,
                    };
                default:
                    return new CameraImpulseRequest
                    {
                        PositionAmplitude = 0.1f,
                        RotationAmplitude = 0.15f,
                        Duration = 0.25f,
                        Frequency = 14f,
                        Priority = CameraImpulsePriority.Low,
                    };
            }
        }

        /// <summary>Виконує At.</summary>
        public static CameraImpulseRequest At(CameraImpulseProfile profile, Vector3 worldPosition)
        {
            var request = Create(profile);
            request.WorldPosition = worldPosition;
            request.HasWorldPosition = true;
            return request;
        }
    }

    /// <summary>ICameraFeedbackService — interface: I камери відгуку сервісу.</summary>
    public interface ICameraFeedbackService
    {
        /// <summary>Чи імпульсу активної — IsImpulseActive.</summary>
        bool IsImpulseActive { get; }

        /// <summary>Запитує імпульсу.</summary>
        void RequestImpulse(CameraImpulseRequest request);
        /// <summary>Запитує імпульсу.</summary>
        void RequestImpulse(CameraImpulseProfile profile, Vector3 worldPosition);

        /// <summary>Скасовує All Impulses.</summary>
        void CancelAllImpulses();
    }
}
