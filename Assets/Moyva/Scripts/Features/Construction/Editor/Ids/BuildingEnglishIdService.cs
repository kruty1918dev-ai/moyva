#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor.Ids
{
    public static class BuildingEnglishIdService
    {
        private const string LogPrefix = "[MoyvaBuildingIds]";
        private static readonly Regex StableId = new Regex("^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$", RegexOptions.Compiled);
        private static readonly HashSet<string> Placeholders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "new-building", "newbuilding", "newbuildingdefinition", "building", "unnamed-building", "temp-building"
        };

        private static readonly Dictionary<string, string> Known = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"замок","castle"}, {"будинок","house"}, {"дім","house"}, {"вітряк","windmill"}, {"млин","windmill"},
            {"кам'яна стіна","stone-wall"}, {"камяна стіна","stone-wall"}, {"кам'яні ворота","stone-gate"}, {"камяні ворота","stone-gate"},
            {"дерев'яний міст","wood-bridge"}, {"деревяний міст","wood-bridge"}, {"лісозаготівельний табір","wood-camp"},
            {"табір лісорубів","wood-camp"}, {"лісопилка","sawmill"}, {"кам'яний кар'єр","stone-quarry"}, {"камяний карєр","stone-quarry"},
            {"кар'єр","quarry"}, {"карєр","quarry"}, {"залізна шахта","iron-mine"}, {"шахта","mine"}, {"склад","storage"},
            {"сховище","storage"}, {"амбар","barn"}, {"збройова майстерня","weapon-workshop"}, {"майстерня зброї","weapon-workshop"},
            {"бронярська майстерня","armor-workshop"}, {"майстерня броні","armor-workshop"}, {"кузня","blacksmith"}, {"ковальня","blacksmith"},
            {"казарма","barracks"}, {"стайня","stable"}, {"гільдія інженерів","engineers-guild"}, {"інженерна гільдія","engineers-guild"},
            {"castle","castle"}, {"house","house"}, {"windmill","windmill"}, {"stone wall","stone-wall"}, {"stone gate","stone-gate"},
            {"wood bridge","wood-bridge"}, {"wooden bridge","wood-bridge"}, {"wood camp","wood-camp"}, {"lumber camp","wood-camp"},
            {"sawmill","sawmill"}, {"stone quarry","stone-quarry"}, {"quarry","quarry"}, {"iron mine","iron-mine"}, {"storage","storage"},
            {"warehouse","storage"}, {"barn","barn"}, {"weapon workshop","weapon-workshop"}, {"armor workshop","armor-workshop"},
            {"blacksmith","blacksmith"}, {"barrack","barracks"}, {"barracks","barracks"}, {"stable","stable"},
            {"engineers guild","engineers-guild"}, {"engineer's guild","engineers-guild"}, {"engineers' guild","engineers-guild"}
        };

        private sealed class Record
        {
            public BuildingDefinitionAsset Asset;
            public string Path;
            public string OldId;
            public string NewId;
            public string Source;
            public string Error;
            public bool Change;
        }

        private sealed class Plan
        {
            public readonly List<Record> Records = new List<Record>();
            public readonly Dictionary<string,string> Map = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            public bool Errors;
            public int Changed;
            public int Preserved;
        }

        [MenuItem("Moyva/Tools/Building Designer/IDs/Analyze English IDs", priority = 341)]
        public static void AnalyzeMenu() => LogPlan(BuildPlan());

        [MenuItem("Moyva/Tools/Building Designer/IDs/Apply English IDs (safe)", priority = 342)]
        public static void ApplyMenu() => ApplySafe(false);

        public static bool NeedsNormalization()
        {
            foreach (var asset in LoadAssets())
                if (!IsStable(asset.Id)) return true;
            return false;
        }

        public static bool ApplySafe(bool automatic)
        {
            Plan plan = BuildPlan();
            LogPlan(plan);
            if (plan.Errors)
            {
                Debug.LogError(LogPrefix + " APPLY_BLOCKED: unresolved or duplicate IDs; nothing changed.");
                return false;
            }
            if (plan.Changed == 0)
            {
                Debug.Log(LogPrefix + $" APPLY_OK changed=0 preserved={plan.Preserved}");
                return true;
            }

            try
            {
                int refUpdates = 0;
                foreach (var r in plan.Records)
                {
                    if (!r.Change) continue;
                    r.Asset.Identity.Id = r.NewId;
                    r.Asset.NotifyEditorDataChanged();
                    EditorUtility.SetDirty(r.Asset);
                }

                foreach (var r in plan.Records)
                {
                    var asset = r.Asset;
                    if (asset == null || asset.Modules == null) continue;
                    bool dirty = false;
                    foreach (var module in asset.Modules)
                    {
                        if (module is BuildingPrerequisiteModule p)
                        {
                            int n = Replace(p.BuildingIds, plan.Map); refUpdates += n; dirty |= n > 0;
                        }
                        else if (module is ReplacementPlacementRuleModule x)
                        {
                            int n = Replace(x.ReplaceableBuildingIds, plan.Map); refUpdates += n; dirty |= n > 0;
                        }
                    }
                    if (dirty) { asset.NotifyEditorDataChanged(); EditorUtility.SetDirty(asset); }
                }

                foreach (string guid in AssetDatabase.FindAssets("t:BuildingRegistrySO"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var registry = AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(path);
                    if (registry == null || registry.WallCollections == null) continue;
                    bool dirty = false;
                    foreach (var c in registry.WallCollections)
                    {
                        if (c == null) continue;
                        string next = MapId(c.WallBuildingId, plan.Map);
                        if (next != c.WallBuildingId) { c.WallBuildingId = next; dirty = true; refUpdates++; }
                        next = MapId(c.GateBuildingId, plan.Map);
                        if (next != c.GateBuildingId) { c.GateBuildingId = next; dirty = true; refUpdates++; }
                        if (c.TopologyBindings == null) continue;
                        foreach (var b in c.TopologyBindings)
                        {
                            if (b?.VariantBuildingIds == null) continue;
                            for (int i = 0; i < b.VariantBuildingIds.Count; i++)
                            {
                                string old = b.VariantBuildingIds[i];
                                string nn = MapId(old, plan.Map);
                                if (nn == old) continue;
                                b.VariantBuildingIds[i] = nn; dirty = true; refUpdates++;
                            }
                        }
                    }
                    if (dirty) EditorUtility.SetDirty(registry);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!Verify(plan))
                {
                    Debug.LogError(LogPrefix + " VERIFY_FAILED: use the external installer backup to restore if needed.");
                    return false;
                }

                Debug.Log(LogPrefix + $" APPLY_OK mode={(automatic ? "auto" : "manual")} changed={plan.Changed} preserved={plan.Preserved} referenceUpdates={refUpdates}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(LogPrefix + " APPLY_EXCEPTION: " + ex);
                return false;
            }
        }

        private static Plan BuildPlan()
        {
            var plan = new Plan();
            var records = LoadAssets().Select(a => new Record
            {
                Asset = a,
                Path = AssetDatabase.GetAssetPath(a),
                OldId = (a.Id ?? string.Empty).Trim()
            }).OrderBy(r => r.Path, StringComparer.OrdinalIgnoreCase).ToList();

            var occupied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in records)
            {
                if (!IsStable(r.OldId)) continue;
                if (!occupied.Add(r.OldId))
                {
                    r.Error = "duplicate existing ID '" + r.OldId + "'"; plan.Errors = true; continue;
                }
                r.NewId = r.OldId; r.Source = "existing-stable-id"; plan.Preserved++;
            }

            foreach (var r in records)
            {
                if (r.NewId != null || r.Error != null) continue;
                string source;
                string baseId = Derive(r.Asset, r.Path, out source);
                if (string.IsNullOrWhiteSpace(baseId))
                {
                    r.Error = "cannot derive semantic English ID from display name, asset name or prefab name";
                    plan.Errors = true; continue;
                }
                r.NewId = Allocate(baseId, occupied);
                if (string.IsNullOrWhiteSpace(r.NewId))
                {
                    r.Error = "cannot allocate unique ID from base '" + baseId + "'"; plan.Errors = true; continue;
                }
                r.Source = source; r.Change = r.NewId != r.OldId; occupied.Add(r.NewId);
                if (r.Change) plan.Changed++; else plan.Preserved++;
                if (r.Change && !string.IsNullOrWhiteSpace(r.OldId) && !plan.Map.ContainsKey(r.OldId)) plan.Map.Add(r.OldId, r.NewId);
            }

            plan.Records.AddRange(records);
            return plan;
        }

        private static List<BuildingDefinitionAsset> LoadAssets()
        {
            var result = new List<BuildingDefinitionAsset>();
            foreach (string guid in AssetDatabase.FindAssets("t:BuildingDefinitionAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<BuildingDefinitionAsset>(path);
                if (asset != null) result.Add(asset);
            }
            return result;
        }

        private static string Derive(BuildingDefinitionAsset asset, string path, out string source)
        {
            source = string.Empty;
            string display = asset.DisplayName ?? string.Empty;
            string known = KnownValue(display);
            if (!string.IsNullOrWhiteSpace(known)) { source = "display-dictionary"; return known; }
            if (LooksEnglish(display))
            {
                string slug = Slug(display);
                if (Usable(slug)) { source = "display-name"; return slug; }
            }

            string assetName = Path.GetFileNameWithoutExtension(path);
            if (LooksEnglish(assetName))
            {
                string slug = RemoveVariant(Slug(assetName));
                if (Usable(slug)) { source = "asset-name"; return slug; }
            }

            string prefab = asset.Presentation?.Prefab != null ? asset.Presentation.Prefab.name : string.Empty;
            if (!string.IsNullOrWhiteSpace(prefab))
            {
                string cleaned = Regex.Replace(prefab.ToLowerInvariant().Replace('_','-'), "^building-", "");
                cleaned = Regex.Replace(cleaned, "-(blue|red|green|purple|yellow|orange|black|white)$", "");
                cleaned = RemoveVariant(cleaned).Replace('-', ' ');
                known = KnownValue(cleaned);
                if (!string.IsNullOrWhiteSpace(known)) { source = "prefab-dictionary"; return known; }
                if (LooksEnglish(cleaned))
                {
                    string slug = Slug(cleaned);
                    if (Usable(slug)) { source = "prefab-name"; return slug; }
                }
            }
            return string.Empty;
        }

        private static string KnownValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            string n = Regex.Replace(value.Trim().ToLowerInvariant().Replace('’','\''), "[_-]+", " ");
            n = Regex.Replace(n, "\\s+", " ").Trim();
            return Known.TryGetValue(n, out string v) ? v : string.Empty;
        }

        private static bool LooksEnglish(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            bool latin = false;
            foreach (char c in value)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) latin = true;
                if (c >= '\u0400' && c <= '\u052F') return false;
            }
            return latin;
        }

        private static string Slug(string value)
        {
            var sb = new StringBuilder(); bool dash = false;
            foreach (char raw in value.ToLowerInvariant())
            {
                bool ok = (raw >= 'a' && raw <= 'z') || (raw >= '0' && raw <= '9');
                if (ok) { sb.Append(raw); dash = false; }
                else if (!dash && sb.Length > 0) { sb.Append('-'); dash = true; }
            }
            return sb.ToString().Trim('-');
        }

        private static string RemoveVariant(string value) => Regex.Replace(value ?? string.Empty, "-\\d{1,3}$", "");
        private static bool IsStable(string id) => !string.IsNullOrWhiteSpace(id) && !Placeholders.Contains(id.Trim()) && StableId.IsMatch(id.Trim());
        private static bool Usable(string id) => IsStable(id) && id != "new" && id != "definition";

        private static string Allocate(string baseId, HashSet<string> occupied)
        {
            bool family = occupied.Any(id => id.Equals(baseId, StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(id, "^" + Regex.Escape(baseId) + "-\\d{2,3}$", RegexOptions.IgnoreCase));
            if (!family && !occupied.Contains(baseId)) return baseId;
            for (int i = 1; i <= 999; i++)
            {
                string c = baseId + "-" + i.ToString("00");
                if (!occupied.Contains(c)) return c;
            }
            return string.Empty;
        }

        private static int Replace(string[] values, Dictionary<string,string> map)
        {
            if (values == null) return 0; int n = 0;
            for (int i = 0; i < values.Length; i++)
            {
                string next = MapId(values[i], map); if (next == values[i]) continue; values[i] = next; n++;
            }
            return n;
        }

        private static string MapId(string id, Dictionary<string,string> map) => !string.IsNullOrWhiteSpace(id) && map.TryGetValue(id, out string n) ? n : id;

        private static void LogPlan(Plan plan)
        {
            Debug.Log(LogPrefix + $" ANALYSIS assets={plan.Records.Count} changes={plan.Changed} preserved={plan.Preserved} errors={(plan.Errors ? 1 : 0)}");
            foreach (var r in plan.Records)
            {
                if (!string.IsNullOrWhiteSpace(r.Error)) Debug.LogError(LogPrefix + $" UNRESOLVED path={r.Path} display='{r.Asset?.DisplayName}' oldId='{r.OldId}' reason={r.Error}");
                else if (r.Change) Debug.Log(LogPrefix + $" PLAN path={r.Path} display='{r.Asset.DisplayName}' oldId='{r.OldId}' newId='{r.NewId}' source={r.Source}");
            }
        }

        private static bool Verify(Plan plan)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in plan.Records)
            {
                var a = AssetDatabase.LoadAssetAtPath<BuildingDefinitionAsset>(r.Path);
                if (a == null || !IsStable(a.Id) || !seen.Add(a.Id)) return false;
                if (!string.IsNullOrWhiteSpace(r.NewId) && a.Id != r.NewId) return false;
            }
            return true;
        }
    }

    public static class BuildingEnglishIdCommandLine
    {
        public static void Analyze()
        {
            BuildingEnglishIdService.AnalyzeMenu();
        }
        public static void Apply()
        {
            bool ok = BuildingEnglishIdService.ApplySafe(false);
            if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 32);
        }
    }

    [InitializeOnLoad]
    internal static class BuildingEnglishIdAutoCoordinator
    {
        private static bool _attempted;
        static BuildingEnglishIdAutoCoordinator() { EditorApplication.delayCall += TryApply; }
        private static void TryApply()
        {
            if (_attempted) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += TryApply; return; }
            _attempted = true;
            if (!BuildingEnglishIdService.NeedsNormalization()) return;
            Debug.Log("[MoyvaBuildingIds] AUTO: IDs need normalization; running safe English-ID analysis/apply.");
            BuildingEnglishIdService.ApplySafe(true);
        }
    }
}

#endif
