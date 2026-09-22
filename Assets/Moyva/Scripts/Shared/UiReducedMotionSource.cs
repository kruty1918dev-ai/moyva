using Kruty1918.Moyva.Shared.Controls;
using Kruty1918.UiFoundation;

namespace Kruty1918.Moyva.Shared
{
    /// <summary>Bridges the package-side reduced-motion contract to the
    /// persisted player control settings.</summary>
    internal sealed class UiReducedMotionSource : IUiReducedMotionSource
    {
        private readonly IPlayerControlSettingsService _settings;

        public UiReducedMotionSource(IPlayerControlSettingsService settings)
            => _settings = settings;

        public bool ReduceMotion => _settings.Settings.ReduceMotion;

        public void SetReduceMotion(bool value) => _settings.SetReduceMotion(value);
    }
}
