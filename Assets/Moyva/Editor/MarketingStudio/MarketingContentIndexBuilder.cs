using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Kruty1918.Moyva.Marketing.Content;
using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Marketing.Output;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.EditorTools
{
    /// <summary>
    /// Builds MarketingContentIndex by scanning the project's canonical JSON
    /// presets (units/buildings/tiles/map-objects/audio registry) and probing
    /// prefabs via AssetDatabase. Applies manual metadata overrides. Writes a
    /// snapshot the runtime consumes; fingerprint detects content changes.
    /// </summary>
    public static class MarketingContentIndexBuilder
    {
        [Serializable] private sealed class AssetRef { public string editorPath; }
        [Serializable] private sealed class UnitJson
        {
            public string id; public string role; public string typeId;
            public AssetRef prefab; public string[] tags;
        }
        [Serializable] private sealed class BuildingIdentity
        {
            public string id; public string displayName; public string category;
            public string role; public string[] tags;
        }
        [Serializable] private sealed class BuildingPresentation { public AssetRef prefab; }
        [Serializable] private sealed class BuildingJson
        {
            public string id; public BuildingIdentity identity;
            public BuildingPresentation presentation;
        }
        [Serializable] private sealed class TileJson { public string id; public string displayName; }
        [Serializable] private sealed class AudioSoundJson
        {
            public string key; public string bus; public AssetRef clip; public bool loop;
        }
        [Serializable] private sealed class AudioRegistryJson
        {
            public List<AudioSoundJson> sounds;
        }

        private static readonly Regex EditorPathRx =
            new Regex("\"editorPath\"\\s*:\\s*\"([^\"]+\\.prefab)\"", RegexOptions.Compiled);

        public static ContentIndexSnapshot Build(bool saveSnapshot = true)
        {
            var store = new MarketingPresetStore();
            store.LoadAll();
            var metadata = new Dictionary<string, MarketingContentMetadata>(StringComparer.Ordinal);
            foreach (var m in store.Metadata)
                if (!string.IsNullOrEmpty(m.contentId))
                    metadata[m.contentId] = m;

            var snapshot = new ContentIndexSnapshot
            {
                builtAtUtc = DateTime.UtcNow.ToString("o"),
                gameProcessSha = GitSha("origin/game-process"),
            };
            var fingerprintParts = new List<string>();

            ScanUnits(store, snapshot, fingerprintParts);
            ScanBuildings(store, snapshot, fingerprintParts);
            ScanTiles(snapshot, fingerprintParts);
            ScanMapObjects(snapshot, fingerprintParts);
            ScanAudio(snapshot, fingerprintParts);

            // Apply metadata overrides
            foreach (var e in snapshot.entries)
            {
                if (!metadata.TryGetValue(e.id, out var m)) continue;
                if (m.valueTier.HasValue) e.marketingValue = m.valueTier.Value;
                if (m.sizeClass.HasValue) e.sizeClass = m.sizeClass.Value;
                if (m.selectionOverride.HasValue) e.selectionOverride = m.selectionOverride.Value;
                if (m.marketingDisabled.HasValue) e.marketingDisabled = m.marketingDisabled.Value;
                if (!string.IsNullOrEmpty(m.biome)) e.biome = m.biome;
                if (m.suitsWide.HasValue) e.suitsWide = m.suitsWide.Value;
                if (m.suitsMedium.HasValue) e.suitsMedium = m.suitsMedium.Value;
                if (m.suitsClose.HasValue) e.suitsClose = m.suitsClose.Value;
            }

            snapshot.fingerprint = ContentFingerprint.Compute(fingerprintParts);
            if (saveSnapshot)
                store.SaveIndexSnapshot(snapshot);
            return snapshot;
        }

        /// <summary>True when the stored snapshot is missing or stale —
        /// triggers automatic rebuild after game-process syncs.</summary>
        public static bool NeedsRebuild()
        {
            var store = new MarketingPresetStore();
            var existing = store.LoadIndexSnapshot();
            if (existing == null) return true;
            var fresh = Build(false);
            return !string.Equals(existing.fingerprint, fresh.fingerprint, StringComparison.Ordinal);
        }

        private static void ScanUnits(MarketingPresetStore store, ContentIndexSnapshot snap, List<string> fp)
        {
            string dir = Path.Combine(MarketingPresetStore.PresetsRoot, "..", "Units");
            dir = Path.GetFullPath(dir);
            if (!Directory.Exists(dir)) return;
            foreach (var file in Directory.GetFiles(dir, "*.json"))
            {
                var unit = Read<UnitJson>(file);
                if (unit == null || string.IsNullOrEmpty(unit.id)) continue;
                var e = new ContentIndexEntry
                {
                    id = unit.id,
                    category = MarketingContentCategory.Unit,
                    role = unit.role ?? string.Empty,
                    tags = unit.tags ?? Array.Empty<string>(),
                };
                ProbePrefab(unit.prefab?.editorPath, e);
                e.marketingValue = MarketingValueTier.Supporting;
                if (e.hasAnimator && e.animationClips.Length > 0) e.suitsClose = true;
                fp.Add($"unit:{unit.id}:{e.assetGuid}:{e.editorPath}");
                snap.entries.Add(e);
            }
        }

        private static void ScanBuildings(MarketingPresetStore store, ContentIndexSnapshot snap, List<string> fp)
        {
            string dir = Path.GetFullPath(Path.Combine(MarketingPresetStore.PresetsRoot, "..", "Buildings"));
            if (!Directory.Exists(dir)) return;
            foreach (var file in Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly))
            {
                var b = Read<BuildingJson>(file);
                if (b == null) continue;
                string id = !string.IsNullOrEmpty(b.identity?.id) ? b.identity.id : b.id;
                if (string.IsNullOrEmpty(id)) continue;
                var e = new ContentIndexEntry
                {
                    id = id,
                    category = MarketingContentCategory.Building,
                    role = b.identity?.role ?? b.identity?.category ?? string.Empty,
                    tags = b.identity?.tags ?? Array.Empty<string>(),
                };
                ProbePrefab(b.presentation?.prefab?.editorPath, e);
                if ((b.identity?.role ?? "").Contains("SettlementCenter"))
                    e.marketingValue = MarketingValueTier.Hero;
                e.suitsClose = true;
                fp.Add($"building:{id}:{e.assetGuid}:{e.editorPath}");
                snap.entries.Add(e);
            }
        }

        private static void ScanTiles(ContentIndexSnapshot snap, List<string> fp)
        {
            string dir = Path.GetFullPath(Path.Combine(MarketingPresetStore.PresetsRoot, "..", "Tiles"));
            if (!Directory.Exists(dir)) return;
            foreach (var file in Directory.GetFiles(dir, "*.json"))
            {
                var t = Read<TileJson>(file);
                if (t == null || string.IsNullOrEmpty(t.id)) continue;
                snap.entries.Add(new ContentIndexEntry
                {
                    id = t.id,
                    category = MarketingContentCategory.Terrain,
                    role = "terrain",
                    marketingValue = MarketingValueTier.Background,
                });
                fp.Add($"tile:{t.id}:{AssetDatabase.AssetPathToGUID(ToAssetPath(file))}");
            }
        }

        private static void ScanMapObjects(ContentIndexSnapshot snap, List<string> fp)
        {
            string dir = Path.GetFullPath(Path.Combine(
                MarketingPresetStore.PresetsRoot, "..", "Generator", "map-object-registry"));
            if (!Directory.Exists(dir)) return;
            foreach (var file in Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories))
            {
                string json = File.ReadAllText(file);
                fp.Add($"mapobj:{Path.GetFileName(file)}:{json.Length}:{File.GetLastWriteTimeUtc(file).Ticks}");
                foreach (Match m in EditorPathRx.Matches(json))
                {
                    string path = m.Groups[1].Value;
                    string id = Path.GetFileNameWithoutExtension(path);
                    if (snap.entries.Exists(e => e.editorPath == path)) continue;
                    var e = new ContentIndexEntry
                    {
                        id = $"env-{id}",
                        category = MarketingContentCategory.Environment,
                        role = "mapobject",
                        marketingValue = MarketingValueTier.Background,
                    };
                    ProbePrefab(path, e);
                    snap.entries.Add(e);
                }
            }
        }

        private static void ScanAudio(ContentIndexSnapshot snap, List<string> fp)
        {
            string reg = Path.Combine(MarketingOutputLayout.ProjectRoot(),
                "Assets", "Moyva", "Resources", "MoyvaConfigGenerated",
                "moyva-audio-registry--moyvaaudioregistry.json");
            if (!File.Exists(reg)) return;
            var data = Read<AudioRegistryJson>(reg);
            fp.Add($"audio:{File.GetLastWriteTimeUtc(reg).Ticks}");
            if (data?.sounds == null) return;
            foreach (var s in data.sounds)
            {
                if (string.IsNullOrEmpty(s?.key)) continue;
                string bus = s.bus ?? "Sfx";
                if (bus.Equals("Music", StringComparison.OrdinalIgnoreCase))
                    snap.musicKeys.Add(s.key);
                else if (bus.Equals("Ambience", StringComparison.OrdinalIgnoreCase))
                    snap.ambienceKeys.Add(s.key);
                else
                    snap.sfxKeys.Add(s.key);
                fp.Add($"sound:{s.key}:{s.clip?.editorPath}");
            }
            snap.musicKeys.Sort(StringComparer.Ordinal);
            snap.ambienceKeys.Sort(StringComparer.Ordinal);
            snap.sfxKeys.Sort(StringComparer.Ordinal);
        }

        private static void ProbePrefab(string editorPath, ContentIndexEntry e)
        {
            if (string.IsNullOrEmpty(editorPath)) return;
            e.editorPath = editorPath;
            e.assetGuid = AssetDatabase.AssetPathToGUID(editorPath);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(editorPath);
            if (go == null)
            {
                e.missingMaterial = true;
                return;
            }
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            e.rendererCount = renderers.Length;
            foreach (var r in renderers)
            {
                if (r.sharedMaterial == null) { e.missingMaterial = true; break; }
            }
            var animator = go.GetComponentInChildren<Animator>(true);
            e.hasAnimator = animator != null && animator.runtimeAnimatorController != null;
            if (e.hasAnimator)
            {
                var clips = animator.runtimeAnimatorController.animationClips;
                var names = new List<string>();
                foreach (var c in clips) if (c != null) names.Add(c.name);
                e.animationClips = names.ToArray();
            }
            // Bounds
            if (renderers.Length > 0)
            {
                var b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds);
                e.boundsRadius = Mathf.Max(0.1f, b.extents.magnitude);
                e.sizeClass = e.boundsRadius < 1.5f ? MarketingSizeClass.Small
                    : e.boundsRadius < 4f ? MarketingSizeClass.Medium
                    : MarketingSizeClass.Large;
            }
            string n = go.name.ToLowerInvariant();
            e.placeholderSuspect = n.Contains("placeholder") || n.Contains("temp")
                || n.Contains("debug") || n.Contains("test");
        }

        private static T Read<T>(string file) where T : class
        {
            try { return JsonConvert.DeserializeObject<T>(File.ReadAllText(file)); }
            catch (Exception e)
            {
                Debug.LogWarning($"[MarketingStudio] Index scan failed for '{file}': {e.Message}");
                return null;
            }
        }

        private static string ToAssetPath(string absolute)
        {
            string norm = absolute.Replace('\\', '/');
            int i = norm.IndexOf("Assets/", StringComparison.Ordinal);
            return i >= 0 ? norm.Substring(i) : norm;
        }

        private static string GitSha(string rev)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("git", $"rev-parse {rev}")
                {
                    WorkingDirectory = MarketingOutputLayout.ProjectRoot(),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                var p = System.Diagnostics.Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit(3000);
                return p.ExitCode == 0 ? output : string.Empty;
            }
            catch { return string.Empty; }
        }
    }
}
