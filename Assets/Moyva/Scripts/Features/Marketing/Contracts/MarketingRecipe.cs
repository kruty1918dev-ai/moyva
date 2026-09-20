using System;

namespace Kruty1918.Moyva.Marketing.Contracts
{
    public enum MarketingContentType
    {
        Trailer = 0,
        Teaser = 1,
        GameplayTrailer = 2,
        Screenshots = 3,
        SocialVideo = 4,
        HeroStills = 5,
        StoreAssets = 6,
        SocialImage = 7,
        MenuBackground = 8,
    }

    public enum MarketingOutputKind
    {
        Image = 0,
        ImageSequence = 1,
        Video = 2,
        VideoWithAudio = 3,
        VideoMuted = 4,
    }

    public enum MarketingQualityTier
    {
        Preview = 0,
        Production = 1,
        Master = 2,
    }

    public enum MarketingUiVisibility
    {
        Hidden = 0,
        Full = 1,
        Minimal = 2,
    }

    public enum MarketingCameraStyle
    {
        Cinematic = 0,
        Gameplay = 1,
        Mixed = 2,
    }

    public enum MarketingLightingStyle
    {
        Unchanged = 0,
        Dawn = 1,
        Morning = 2,
        Day = 3,
        GoldenHour = 4,
        Dusk = 5,
        Overcast = 6,
    }

    /// <summary>
    /// Deterministic seeds for a run. 0 means "auto" (generated at run start
    /// and recorded into the manifest so the run stays reproducible).
    /// </summary>
    [Serializable]
    public sealed class MarketingSeeds
    {
        public int worldSeed;
        public int scenarioSeed;
        public int cinematicSeed;

        public static MarketingSeeds Auto => new MarketingSeeds();

        public MarketingSeeds ResolveAuto(int fallbackBase)
        {
            var rng = new Random(fallbackBase);
            return new MarketingSeeds
            {
                worldSeed = worldSeed != 0 ? worldSeed : rng.Next(1, int.MaxValue),
                scenarioSeed = scenarioSeed != 0 ? scenarioSeed : rng.Next(1, int.MaxValue),
                cinematicSeed = cinematicSeed != 0 ? cinematicSeed : rng.Next(1, int.MaxValue),
            };
        }
    }

    /// <summary>
    /// Data-driven capture recipe. Authored as JSON under
    /// Assets/Moyva/Presets/Marketing/recipes/. Resolved and frozen before a run.
    /// </summary>
    [Serializable]
    public sealed class MarketingCaptureRecipe
    {
        public string schema = "moyva.marketing-recipe";
        public int version = 1;
        public string id = "recipe";
        public string displayName = "Recipe";

        public MarketingContentType contentType = MarketingContentType.Screenshots;
        public string platformProfileId = "generic";
        public MarketingOutputKind outputKind = MarketingOutputKind.Image;
        public MarketingQualityTier quality = MarketingQualityTier.Production;

        public int resolutionWidth;    // 0 → take from platform profile
        public int resolutionHeight;   // 0 → take from platform profile
        public int frameRate = 30;
        public float durationSec = 30f;
        public int shotCount = 8;

        public MarketingUiVisibility uiVisibility = MarketingUiVisibility.Hidden;
        public bool includeLogo;
        public bool includeMarketingText;
        public bool includeSubtitles;
        public bool includeSfx = true;
        public bool includeMusic = true;
        public MarketingCameraStyle cameraStyle = MarketingCameraStyle.Cinematic;
        public string scenarioStrategy = "auto";
        public MarketingLightingStyle lightingStyle = MarketingLightingStyle.Unchanged;
        public string language = "en";

        /// <summary>Allow staging extra real units via IUnitFactory. Store/gameplay
        /// compliance profiles may forbid this.</summary>
        public bool allowStagingSpawns = true;

        public string musicKey = string.Empty;     // empty → auto-pick a Music-bus loop
        public string ambienceKey = string.Empty;  // empty → auto-pick ambience if any
        public string logoSpritePath = string.Empty; // empty → branding.json default

        public MarketingSeeds seeds = new MarketingSeeds();

        public string outputFolder = string.Empty; // empty → MarketingOutput/<run>
        public string namingPrefix = string.Empty; // empty → recipe id

        public bool emitCleanMaster = true;   // always keep an overlay-free master
        public bool emitTextVariant;          // secondary pass with text on

        public string Prefix => string.IsNullOrWhiteSpace(namingPrefix) ? id : namingPrefix;
    }
}
