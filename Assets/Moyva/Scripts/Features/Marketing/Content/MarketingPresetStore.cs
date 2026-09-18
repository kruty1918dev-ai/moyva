using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Marketing.Output;
using Kruty1918.Moyva.Marketing.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Content
{
    /// <summary>
    /// Load → Validate → Resolve → Freeze for marketing presets.
    /// Reads JSON under Assets/Moyva/Presets/Marketing/ directly (editor tool —
    /// no AssetDatabase at runtime; files are plain JSON on disk).
    /// </summary>
    public sealed class MarketingPresetStore
    {
        public static string PresetsRoot =>
            Path.Combine(MarketingOutputLayout.ProjectRoot(), "Assets", "Moyva", "Presets", "Marketing");

        public static string RecipesDir => Path.Combine(PresetsRoot, "recipes");
        public static string PlatformsDir => Path.Combine(PresetsRoot, "platforms");
        public static string MetadataDir => Path.Combine(PresetsRoot, "metadata");
        public static string ClaimsFile => Path.Combine(PresetsRoot, "claims.json");
        public static string BrandingFile => Path.Combine(PresetsRoot, "branding.json");

        public static string IndexCacheFile =>
            Path.Combine(MarketingOutputLayout.ProjectRoot(), "MarketingOutput", ".cache", "content-index.json");

        public List<MarketingCaptureRecipe> Recipes { get; } = new List<MarketingCaptureRecipe>();
        public List<PlatformCaptureProfile> Platforms { get; } = new List<PlatformCaptureProfile>();
        public List<MarketingContentMetadata> Metadata { get; } = new List<MarketingContentMetadata>();
        public MarketingClaimCatalogData Claims { get; private set; } = new MarketingClaimCatalogData();
        public MarketingBranding Branding { get; private set; } = new MarketingBranding();

        public void LoadAll()
        {
            Recipes.Clear();
            Platforms.Clear();
            Metadata.Clear();
            LoadInto(RecipesDir, Recipes);
            LoadInto(PlatformsDir, Platforms);
            if (Directory.Exists(MetadataDir))
            {
                foreach (var file in Directory.GetFiles(MetadataDir, "*.json", SearchOption.AllDirectories))
                {
                    var wrapper = TryRead<MarketingMetadataFile>(file);
                    if (wrapper?.overrides != null)
                        Metadata.AddRange(wrapper.overrides);
                }
            }
            if (File.Exists(ClaimsFile))
                Claims = TryRead<MarketingClaimCatalogData>(ClaimsFile) ?? new MarketingClaimCatalogData();
            if (File.Exists(BrandingFile))
                Branding = TryRead<MarketingBranding>(BrandingFile) ?? new MarketingBranding();
        }

        public MarketingCaptureRecipe FindRecipe(string id)
            => Recipes.Find(r => string.Equals(r.id, id, StringComparison.OrdinalIgnoreCase));

        public PlatformCaptureProfile FindPlatform(string id)
            => Platforms.Find(p => string.Equals(p.id, id, StringComparison.OrdinalIgnoreCase))
               ?? Platforms.Find(p => p.id == "generic");

        public ContentIndexSnapshot LoadIndexSnapshot()
        {
            if (!File.Exists(IndexCacheFile)) return null;
            return TryRead<ContentIndexSnapshot>(IndexCacheFile);
        }

        public void SaveIndexSnapshot(ContentIndexSnapshot snapshot)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(IndexCacheFile));
            File.WriteAllText(IndexCacheFile, JsonConvert.SerializeObject(snapshot, Formatting.Indented));
        }

        private static void LoadInto<T>(string dir, List<T> target) where T : class
        {
            if (!Directory.Exists(dir)) return;
            foreach (var file in Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories))
            {
                var item = TryRead<T>(file);
                if (item != null) target.Add(item);
            }
            target.Sort((a, b) => string.Compare(IdOf(a), IdOf(b), StringComparison.Ordinal));
        }

        /// <summary>Metadata file wrapper: {"overrides":[...]}.</summary>
        [Serializable]
        public sealed class MarketingMetadataFile
        {
            public string schema = "moyva.marketing-metadata";
            public int version = 1;
            public List<MarketingContentMetadata> overrides = new List<MarketingContentMetadata>();
        }

        private static string IdOf(object item) => item switch
        {
            MarketingCaptureRecipe r => r.id,
            PlatformCaptureProfile p => p.id,
            MarketingContentMetadata m => m.contentId,
            _ => string.Empty,
        };

        public static string SerializeJson(object dto)
            => JsonConvert.SerializeObject(dto, Formatting.Indented);

        public static T DeserializeJson<T>(string json)
            => JsonConvert.DeserializeObject<T>(json);

        private static T TryRead<T>(string file) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(file));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MarketingStudio] Failed to parse preset '{file}': {e.Message}");
                return null;
            }
        }
    }

    /// <summary>Branding configuration (logo sprite, end-card line, colors).</summary>
    [Serializable]
    public sealed class MarketingBranding
    {
        public string schema = "moyva.marketing-branding";
        public int version = 1;
        public string logoSpritePath = string.Empty;   // e.g. Assets/Moyva/Art/UI/logo.png
        public string gameTitle = "MOYVA";
        public string endCardLine = string.Empty;      // e.g. "Coming Soon" — empty = none
        public string fontAssetPath = string.Empty;    // optional .ttf for overlay text
        public Color textColor = Color.white;
        public Color shadowColor = new Color(0f, 0f, 0f, 0.75f);
    }
}
