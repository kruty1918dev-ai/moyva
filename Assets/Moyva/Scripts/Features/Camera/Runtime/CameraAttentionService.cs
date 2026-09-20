using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Shared.Controls;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>
    /// Single entry point for gameplay systems that want camera attention.
    /// Translates requests into optional focus suggestions and impulse accents;
    /// never takes control away — focus suggestions require the player's
    /// Automatic Camera Focus setting and remain interruptible.
    /// </summary>
    internal sealed class CameraAttentionService : ICameraAttentionService
    {
        private readonly ICameraFocusService _focus;
        private readonly ICameraFeedbackService _feedback;
        private readonly IPlayerControlSettingsService _controlSettings;

        public CameraAttentionService(
            [InjectOptional] ICameraFocusService focus = null,
            [InjectOptional] ICameraFeedbackService feedback = null,
            [InjectOptional] IPlayerControlSettingsService controlSettings = null)
        {
            _focus = focus;
            _feedback = feedback;
            _controlSettings = controlSettings;
        }

        public void Submit(CameraAttentionRequest request)
        {
            if (request.SuggestedImpulse.HasValue && _feedback != null)
            {
                var impulse = request.HasWorldPosition
                    ? CameraImpulseProfiles.At(request.SuggestedImpulse.Value, request.WorldPosition)
                    : CameraImpulseProfiles.Create(request.SuggestedImpulse.Value);
                impulse.Category = request.Category;
                _feedback.RequestImpulse(impulse);
            }

            if (!request.SuggestFocus || !request.HasWorldPosition || _focus == null)
                return;

            bool autoFocusEnabled = _controlSettings != null && _controlSettings.Settings.AutomaticCameraFocus;
            if (!autoFocusEnabled)
                return;

            _focus.FocusWorldPoint(request.WorldPosition, new CameraFocusRequest { KeepCurrentZoom = true });
        }
    }
}
