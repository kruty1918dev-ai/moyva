using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>CameraAttentionRequest — struct: камери уваги запиту.</summary>
    public struct CameraAttentionRequest
    {
        /// <summary>світу позицію — Vector3.</summary>
        public Vector3 WorldPosition;
        /// <summary>Чи світу позицію — HasWorldPosition.</summary>
        public bool HasWorldPosition;

        /// <summary>Важливість запиту уваги.</summary>
        public float Importance;

        /// <summary>Suggest фокус — bool.</summary>
        public bool SuggestFocus;

        /// <summary>Suggested імпульсу — CameraImpulseProfile?.</summary>
        public CameraImpulseProfile? SuggestedImpulse;

        /// <summary>категорії — CameraImpulseCategory.</summary>
        public CameraImpulseCategory Category;
    }

    /// <summary>ICameraAttentionService — interface: I камери уваги сервісу.</summary>
    public interface ICameraAttentionService
    {
        /// <summary>Надсилає Submit.</summary>
        void Submit(CameraAttentionRequest request);
    }
}
