using Newtonsoft.Json;
using System;
using System.IO;
using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Marketing.Output;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// Owns the run folder, writes files + manifest, cleans the temp dir on
    /// success. Non-destructive: masters are written before overlays, never
    /// overwritten.
    /// </summary>
    public sealed class MarketingOutputManager
    {
        private readonly MarketingOutputLayout _layout;
        private readonly MarketingManifest _manifest;

        public MarketingOutputLayout Layout => _layout;
        public MarketingManifest Manifest => _manifest;

        public MarketingOutputManager(MarketingOutputLayout layout, MarketingCaptureRecipe recipe,
            MarketingSeeds seeds, string gameProcessSha, string toolSha)
        {
            _layout = layout;
            _manifest = new MarketingManifest
            {
                runId = $"{DateTime.UtcNow:yyyy-MM-dd_HHmmss}_{MarketingOutputLayout.Sanitize(recipe.id)}",
                timestampUtc = DateTime.UtcNow.ToString("o"),
                recipeId = recipe.id,
                platformProfileId = recipe.platformProfileId,
                resolutionWidth = recipe.resolutionWidth,
                resolutionHeight = recipe.resolutionHeight,
                frameRate = recipe.frameRate,
                durationSec = recipe.durationSec,
                worldSeed = seeds.worldSeed,
                scenarioSeed = seeds.scenarioSeed,
                cinematicSeed = seeds.cinematicSeed,
                sourceGameProcessSha = gameProcessSha ?? string.Empty,
                toolBranchSha = toolSha ?? string.Empty,
                language = recipe.language,
            };
            _layout.CreateRunFolder(recipe.id);
        }

        public string WritePng(Texture2D tex, string platformFolder, string fileName)
        {
            string dir = _layout.PlatformFolder(platformFolder);
            string path = Path.Combine(dir, fileName);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            _manifest.filesGenerated.Add(Rel(path));
            return path;
        }

        public string WriteMasterPng(Texture2D tex, string fileName)
        {
            string path = Path.Combine(_layout.RunFolder, "Master", fileName);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            _manifest.filesGenerated.Add(Rel(path));
            return path;
        }

        public string WriteMetadataJson(object dto, string fileName)
        {
            string path = _layout.MetadataFile(fileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(dto, Formatting.Indented));
            _manifest.filesGenerated.Add(Rel(path));
            return path;
        }

        public void WriteManifest()
        {
            File.WriteAllText(_layout.ManifestFile, JsonConvert.SerializeObject(_manifest, Formatting.Indented));
        }

        public void CleanupTemp()
        {
            try
            {
                if (Directory.Exists(_layout.TempFolder))
                    Directory.Delete(_layout.TempFolder, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MarketingStudio] Temp cleanup failed: {e.Message}");
            }
        }

        private string Rel(string absolute)
        {
            string root = _layout.RunFolder ?? string.Empty;
            return absolute.StartsWith(root) ? absolute.Substring(root.Length).TrimStart('\\', '/') : absolute;
        }
    }
}
