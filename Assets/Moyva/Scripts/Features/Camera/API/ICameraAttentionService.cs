using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>
    /// A "something worth noticing happened" hint from gameplay systems.
    /// The camera decides what to do with it based on player settings —
    /// submitters never move the camera directly.
    /// </summary>
    public struct CameraAttentionRequest
    {
        /// <summary>World position of the event; only used when HasWorldPosition is set.</summary>
        public Vector3 WorldPosition;
        public bool HasWorldPosition;

        /// <summary>0..1 — gates optional automatic focus responses.</summary>
        public float Importance;

        /// <summary>Allow the camera to focus the event when Automatic Camera Focus is enabled.</summary>
        public bool SuggestFocus;

        /// <summary>Optional impulse to accompany the event.</summary>
        public CameraImpulseProfile? SuggestedImpulse;

        public CameraImpulseCategory Category;
    }

    /// <summary>
    /// Single entry point for gameplay systems that want camera attention.
    /// Never takes control away: focus suggestions only run when the player
    /// opted in via Automatic Camera Focus, and every transition remains
    /// interruptible by any input.
    /// </summary>
    public interface ICameraAttentionService
    {
        void Submit(CameraAttentionRequest request);
    }
}
