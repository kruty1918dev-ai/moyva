using System;

namespace Kruty1918.Moyva.Marketing.Contracts
{
    /// <summary>
    /// Editable platform requirements. Authored as JSON under
    /// Assets/Moyva/Presets/Marketing/platforms/. Nothing here is hard-coded
    /// into the pipeline — recipes reference profiles by id.
    /// </summary>
    [Serializable]
    public sealed class PlatformCaptureProfile
    {
        public string schema = "moyva.marketing-platform";
        public int version = 1;
        public string id = "generic";
        public string displayName = "Generic";

        public float aspectRatio = 16f / 9f;   // w/h
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public int minResolutionWidth;
        public int minResolutionHeight;

        /// <summary>Fraction of each edge that text/logo must stay inside
        /// (0 = no inset, 0.1 = 10% safe margin).</summary>
        public float safeAreaMargin = 0.05f;

        public bool textAllowed = true;
        public bool gameplayOnly;
        public bool logoAllowed = true;
        public bool uiRecommended;
        public bool preRenderedAllowed = true;

        public string imageFormat = "png";     // png | jpg
        public string videoFormat = "mp4";     // mp4 | webm
        public float preferredDurationSec;
        public float maxDurationSec;

        public string notes = string.Empty;

        public bool MatchesAspect(float aspect, float tolerance = 0.02f)
            => Math.Abs(aspectRatio - aspect) <= tolerance;
    }
}
