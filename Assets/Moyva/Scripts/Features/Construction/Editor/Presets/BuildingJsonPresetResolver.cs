using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kruty1918.Moyva.Editor.Shared;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor.Presets
{
    internal static class BuildingJsonPresetResolver
    {
        public static ResolvedBuildingPreset Resolve(BuildingPresetSpec spec, BuildingPresetPackSpec pack)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            var result = new ResolvedBuildingPreset { Spec = spec };
            HashSet<string> resources = new HashSet<string>(EconomyResourceEditorShared.LoadResourceIds(), StringComparer.OrdinalIgnoreCase);
            foreach (BuildingPresetCostSpec cost in spec.Costs)
            {
                if (cost.Amount <= 0) continue;
                string resourceId = ResolveResourceId(cost.Resource, pack);
                EnsureResource(resources, resourceId, $"{spec.Id}: construction cost '{cost.Resource}'");
                result.Costs.Add(new ResolvedCost { ResourceId = resourceId, Amount = cost.Amount });
            }
            foreach (BuildingPresetModuleSpec module in spec.ActiveModules)
            {
                if (string.Equals(module.Type, "ProductionBuildingModule", StringComparison.Ordinal))
                {
                    string resourceId = GetDataString(module, "resourceId");
                    EnsureResource(resources, resourceId, $"{spec.Id}: ProductionBuildingModule.resourceId");
                }
            }
            result.Icon = ResolveIcon(spec.Icon, spec.Id);
            result.Prefab = ResolvePrefab(spec.Prefab, spec.Id);
            return result;
        }

        private static string ResolveResourceId(string aliasOrId, BuildingPresetPackSpec pack)
        {
            if (string.IsNullOrWhiteSpace(aliasOrId)) throw new InvalidDataException("Resource alias/id is empty.");
            return pack.ResourceAliases.TryGetValue(aliasOrId.Trim(), out string mapped) ? mapped : aliasOrId.Trim();
        }
        private static void EnsureResource(HashSet<string> resources, string id, string context)
        {
            if (string.IsNullOrWhiteSpace(id) || !resources.Contains(id))
                throw new InvalidOperationException($"{context}: resourceId '{id}' is not present in Economy resources.");
        }
        private static Sprite ResolveIcon(BuildingPresetObjectRefSpec spec, string buildingId)
        {
            if (spec == null || string.IsNullOrWhiteSpace(spec.Path))
                return null;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spec.Path);
            if (sprite == null && File.Exists(Path.GetFullPath(spec.Path)))
            {
                AssetDatabase.ImportAsset(spec.Path, ImportAssetOptions.ForceUpdate);
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spec.Path);
            }
            if (sprite == null && spec.Required) throw new InvalidOperationException($"{buildingId}: required icon not resolved: {spec.Path}");
            return sprite;
        }
        private static GameObject ResolvePrefab(BuildingPresetObjectRefSpec spec, string buildingId)
        {
            if (spec == null) return null;
            string root = string.IsNullOrWhiteSpace(spec.Root) ? "Assets" : spec.Root.TrimEnd('/');
            if (!AssetDatabase.IsValidFolder(root))
            {
                if (spec.Required) throw new InvalidOperationException($"{buildingId}: prefab root not found: {root}");
                return null;
            }
            var candidates = LoadCandidates(root);
            GameObject resolved = ResolveTier(candidates, spec.Exact, exact:true, out string exactError);
            if (resolved != null) return resolved;
            if (!string.IsNullOrWhiteSpace(exactError)) throw new InvalidOperationException($"{buildingId}: {exactError}");
            resolved = ResolveTier(candidates, spec.Search, exact:false, out string searchError);
            if (resolved != null) return resolved;
            if (!string.IsNullOrWhiteSpace(searchError)) throw new InvalidOperationException($"{buildingId}: {searchError}");
            resolved = ResolveTier(candidates, spec.Fallback, exact:true, out string fallbackError);
            if (resolved != null) return resolved;
            if (!string.IsNullOrWhiteSpace(fallbackError)) throw new InvalidOperationException($"{buildingId}: fallback {fallbackError}");
            if (spec.Required)
                throw new InvalidOperationException($"{buildingId}: required prefab not resolved under {root}. exact=[{string.Join(", ",spec.Exact)}], search=[{string.Join(", ",spec.Search)}], fallback=[{string.Join(", ",spec.Fallback)}]");
            return null;
        }
        // MOYVA_PASS67_PREFAB_RESOLVER_FIX
        // KayKit's Unity package primarily stores buildings as imported model assets (FBX/GLTF),
        // not necessarily as prefab assets discoverable by the `t:GameObject` search filter.
        // Enumerate asset paths under the configured root and load any path that Unity can expose
        // as a GameObject. Cache the result per root so applying 11 presets scans KayKit only once.
        private sealed class Candidate
        {
            public string Path;
            public string Name;
            public GameObject Object;
            public int SourcePriority;
            public int ScopePriority;
        }

        private static readonly Dictionary<string, List<Candidate>> CandidateCache =
            new Dictionary<string, List<Candidate>>(StringComparer.OrdinalIgnoreCase);

        static BuildingJsonPresetResolver()
        {
            EditorApplication.projectChanged += ClearCandidateCache;
        }

        private static void ClearCandidateCache() => CandidateCache.Clear();

        // MOYVA_PASS68_ASSET_DATABASE_DISCOVERY_FIX
        // Pass 67 still depended on AssetDatabase.FindAssets with an empty filter.
        // On the current project that query returned zero paths even though the KayKit
        // folder exists. Use GetAllAssetPaths instead, then progressively widen the
        // search scope in a deterministic way:
        //   1) requested root;
        //   2) any imported path containing "KayKit";
        //   3) all supported model/prefab assets under Assets as a last-resort pool.
        // The existing scoring/ambiguity rules still decide whether a preset may commit.
        private static List<Candidate> LoadCandidates(string root)
        {
            string normalizedRoot = NormalizeAssetFolder(root);
            if (CandidateCache.TryGetValue(normalizedRoot, out List<Candidate> cached))
                return cached;

            string[] allPaths = AssetDatabase.GetAllAssetPaths();
            var list = new List<Candidate>();
            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int supportedInDatabase = 0;
            int supportedUnderRequestedRoot = 0;
            int supportedUnderAnyKayKitPath = 0;
            int physicalUnderRequestedRoot = CountPhysicalSupportedAssets(normalizedRoot);

            // Stage 1: exact configured root.
            for (int index = 0; index < allPaths.Length; index++)
            {
                string path = allPaths[index];
                if (!IsSupportedBuildingAsset(path))
                    continue;

                supportedInDatabase++;
                if (IsUnderAssetFolder(path, normalizedRoot))
                {
                    supportedUnderRequestedRoot++;
                    TryAddCandidate(list, seenPaths, path, 1000);
                }

                if (ContainsKayKitPath(path))
                    supportedUnderAnyKayKitPath++;
            }

            string scope = "requested-root";

            // Stage 2: package moved/renamed but still contains a KayKit path segment.
            if (list.Count == 0)
            {
                scope = "kaykit-anywhere";
                for (int index = 0; index < allPaths.Length; index++)
                {
                    string path = allPaths[index];
                    if (!IsSupportedBuildingAsset(path) || !ContainsKayKitPath(path))
                        continue;

                    TryAddCandidate(list, seenPaths, path, 500);
                }
            }

            // Stage 3: last-resort candidate pool. This does NOT silently accept a model:
            // exact/search scoring and equal-score ambiguity checks still apply later.
            if (list.Count == 0)
            {
                scope = "assets-global-fallback";
                for (int index = 0; index < allPaths.Length; index++)
                {
                    string path = allPaths[index];
                    if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
                        || !IsSupportedBuildingAsset(path))
                    {
                        continue;
                    }

                    TryAddCandidate(list, seenPaths, path, 0);
                }
            }

            CandidateCache[normalizedRoot] = list;

            string sample = list.Count == 0
                ? "<none>"
                : string.Join(", ", list.GetRange(0, Math.Min(8, list.Count)).ConvertAll(candidate => candidate.Path));

            Debug.Log(
                $"[MoyvaBuildingPresets] PREFAB_DISCOVERY requestedRoot={normalizedRoot} " +
                $"scope={scope} assetDbPaths={allPaths.Length} supportedDb={supportedInDatabase} " +
                $"underRoot={supportedUnderRequestedRoot} kaykitAnywhere={supportedUnderAnyKayKitPath} " +
                $"physicalUnderRoot={physicalUnderRequestedRoot} loadedCandidates={list.Count} " +
                $"sample=[{sample}]");

            return list;
        }

        private static void TryAddCandidate(
            List<Candidate> list,
            HashSet<string> seenPaths,
            string path,
            int scopePriority)
        {
            if (string.IsNullOrWhiteSpace(path)
                || !seenPaths.Add(path)
                || AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null)
            {
                UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                for (int index = 0; index < subAssets.Length; index++)
                {
                    if (subAssets[index] is GameObject candidateObject)
                    {
                        go = candidateObject;
                        break;
                    }
                }
            }

            if (go == null)
                return;

            list.Add(new Candidate
            {
                Path = path,
                Name = Path.GetFileNameWithoutExtension(path),
                Object = go,
                SourcePriority = GetSourcePriority(path),
                ScopePriority = scopePriority,
            });
        }

        private static string NormalizeAssetFolder(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
                return "Assets";

            string value = root.Replace('\\', '/').Trim().TrimEnd('/');
            if (string.Equals(value, "Assets", StringComparison.OrdinalIgnoreCase))
                return "Assets";

            if (!value.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                value = "Assets/" + value.TrimStart('/');

            return value;
        }

        private static bool IsUnderAssetFolder(string path, string root)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(root))
                return false;

            if (string.Equals(path, root, StringComparison.OrdinalIgnoreCase))
                return true;

            return path.StartsWith(root.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsKayKitPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path)
                   && path.IndexOf("kaykit", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int CountPhysicalSupportedAssets(string assetRoot)
        {
            try
            {
                string normalized = NormalizeAssetFolder(assetRoot);
                string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
                if (string.IsNullOrWhiteSpace(projectRoot))
                    return -1;

                string absoluteRoot = Path.GetFullPath(
                    Path.Combine(projectRoot, normalized.Replace('/', Path.DirectorySeparatorChar)));

                if (!Directory.Exists(absoluteRoot))
                    return 0;

                int count = 0;
                foreach (string file in Directory.EnumerateFiles(absoluteRoot, "*", SearchOption.AllDirectories))
                {
                    if (IsSupportedBuildingAsset(file))
                        count++;
                }

                return count;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[MoyvaBuildingPresets] physical KayKit scan failed for '{assetRoot}': {exception.Message}");
                return -1;
            }
        }

        private static bool IsSupportedBuildingAsset(string path)
        {
            string extension = Path.GetExtension(path)?.ToLowerInvariant();
            return extension == ".prefab"
                   || extension == ".fbx"
                   || extension == ".obj"
                   || extension == ".gltf"
                   || extension == ".glb"
                   || extension == ".dae"
                   || extension == ".blend";
        }

        private static int GetSourcePriority(string path)
        {
            switch (Path.GetExtension(path)?.ToLowerInvariant())
            {
                case ".prefab": return 500;
                case ".fbx": return 400;
                case ".glb": return 350;
                case ".gltf": return 300;
                case ".obj": return 200;
                case ".dae": return 150;
                case ".blend": return 100;
                default: return 0;
            }
        }
        private static GameObject ResolveTier(List<Candidate> candidates, List<string> queries, bool exact, out string error)
        {
            error = null; if (queries == null || queries.Count == 0) return null;
            int best = int.MinValue; var winners = new List<Candidate>();
            foreach (Candidate c in candidates)
            {
                int score = Score(c, queries, exact); if (score <= 0) continue;
                if (score > best) { best = score; winners.Clear(); winners.Add(c); } else if (score == best) winners.Add(c);
            }
            if (winners.Count == 0) return null;
            if (winners.Count == 1) return winners[0].Object;
            error = $"ambiguous prefab resolution (score={best}): {string.Join(" | ", winners.Select(w => w.Path))}";
            return null;
        }
        private static int Score(Candidate candidate, List<string> queries, bool exact)
        {
            string name = Normalize(candidate.Name); int best = 0;
            for (int i=0;i<queries.Count;i++)
            {
                string query = Normalize(queries[i]); if (query.Length==0) continue; int score=0;
                if (name == query) score = 10000 - i*10;
                else if (!exact && name.Contains(query)) score = 6000 - i*10;
                else if (!exact && Normalize(candidate.Path).Contains(query)) score = 3000 - i*10;
                if (score > 0)
                {
                    if (name.EndsWith("_blue", StringComparison.Ordinal))
                        score += 100;
                    // Prefer an authored Unity prefab when available, then FBX, then GLTF/other model sources.
                    // This also prevents the same KayKit model in FBX + GLTF form from becoming an artificial ambiguity.
                    score += candidate.SourcePriority;
                    score += candidate.ScopePriority;
                }
                if (score > best) best = score;
            }
            return best;
        }
        private static string Normalize(string text) => (text ?? string.Empty).Trim().ToLowerInvariant().Replace('-', '_').Replace(' ', '_');
        internal static string GetDataString(BuildingPresetModuleSpec module, string key)
            => module.Data.TryGetValue(key, out object node) && node is string text ? text : null;
        internal static int GetDataInt(BuildingPresetModuleSpec module, string key, int fallback=0)
        { if (!module.Data.TryGetValue(key,out object node) || node==null) return fallback; try { return Convert.ToInt32(node); } catch { return fallback; } }
    }
}
