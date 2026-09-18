using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Marketing.Contracts
{
    public enum MarketingContentCategory
    {
        Unit = 0,
        Building = 1,
        Environment = 2,
        Terrain = 3,
        Effect = 4,
        Landmark = 5,
        Audio = 6,
    }

    public enum MarketingValueTier
    {
        Background = 0,
        Supporting = 1,
        Hero = 2,
    }

    public enum MarketingSizeClass
    {
        Small = 0,
        Medium = 1,
        Large = 2,
    }

    public enum SelectionOverride
    {
        None = 0,
        Pin = 1,     // never replace
        Ban = 2,     // never use
        Prefer = 3,  // raise weight
        Avoid = 4,   // lower weight
    }

    [Serializable]
    public sealed class ContentIndexEntry
    {
        public string id = string.Empty;
        public MarketingContentCategory category;
        public string editorPath = string.Empty;
        public string assetGuid = string.Empty;
        public string role = string.Empty;
        public string[] tags = Array.Empty<string>();

        public MarketingSizeClass sizeClass = MarketingSizeClass.Medium;
        public bool hasAnimator;
        public string[] animationClips = Array.Empty<string>();
        public int rendererCount;
        public bool missingMaterial;
        public bool placeholderSuspect;
        public float boundsRadius;

        public MarketingValueTier marketingValue = MarketingValueTier.Supporting;
        public SelectionOverride selectionOverride = SelectionOverride.None;
        public bool marketingDisabled;
        public string biome = string.Empty;

        /// <summary>Suitability masks per framing class (wide/medium/close).</summary>
        public bool suitsWide = true;
        public bool suitsMedium = true;
        public bool suitsClose;
    }

    /// <summary>Manual overrides authored in Presets/Marketing/metadata/*.json.
    /// Keyed by content id — third-party prefabs are never modified.</summary>
    [Serializable]
    public sealed class MarketingContentMetadata
    {
        public string contentId = string.Empty;
        public MarketingValueTier? valueTier;
        public MarketingSizeClass? sizeClass;
        public SelectionOverride? selectionOverride;
        public bool? marketingDisabled;
        public string biome;
        public bool? suitsWide;
        public bool? suitsMedium;
        public bool? suitsClose;
        public float scoreBias;
    }

    [Serializable]
    public sealed class ContentIndexSnapshot
    {
        public string schema = "moyva.marketing-content-index";
        public int version = 1;
        public string builtAtUtc = string.Empty;
        public string fingerprint = string.Empty;
        public string gameProcessSha = string.Empty;
        public List<ContentIndexEntry> entries = new List<ContentIndexEntry>();
        public List<string> musicKeys = new List<string>();
        public List<string> ambienceKeys = new List<string>();
        public List<string> sfxKeys = new List<string>();
    }
}
