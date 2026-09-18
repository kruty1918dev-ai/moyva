using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Marketing.Contracts;

namespace Kruty1918.Moyva.Marketing.Text
{
    /// <summary>Feature facts derived from the live content index — the only
    /// source marketing copy may draw on. Claims can never advertise features
    /// that are not present.</summary>
    public sealed class MarketingFeatureFacts
    {
        public int unitCount;
        public int buildingCount;
        public int tileCount;
        public int musicCount;
        public int sfxCount;
        public bool hasCombatUnits;
        public bool hasRangedUnits;
        public bool hasEconomyBuildings;
        public bool hasDefenseBuildings;
        public bool hasSettlementCenter;
        public bool hasNavalUnits;

        public static MarketingFeatureFacts FromIndex(ContentIndexSnapshot index)
        {
            var f = new MarketingFeatureFacts();
            if (index == null) return f;
            foreach (var e in index.entries)
            {
                switch (e.category)
                {
                    case MarketingContentCategory.Unit:
                        f.unitCount++;
                        if (HasTag(e, "military") || HasTag(e, "combat") || e.role.Contains("Military"))
                            f.hasCombatUnits = true;
                        if (e.id.Contains("archer") || e.id.Contains("ranged")) f.hasRangedUnits = true;
                        if (e.id.Contains("boat") || e.id.Contains("ship")) f.hasNavalUnits = true;
                        break;
                    case MarketingContentCategory.Building:
                        f.buildingCount++;
                        if (HasTag(e, "production") || e.role.Contains("Production") || e.role.Contains("Economy"))
                            f.hasEconomyBuildings = true;
                        if (HasTag(e, "defense") || e.id.Contains("wall") || e.id.Contains("gate") || e.id.Contains("tower"))
                            f.hasDefenseBuildings = true;
                        if (e.role.Contains("SettlementCenter") || e.id.Contains("castle") || e.id.Contains("townhall"))
                            f.hasSettlementCenter = true;
                        break;
                    case MarketingContentCategory.Terrain:
                        f.tileCount++;
                        break;
                }
            }
            f.musicCount = index.musicKeys?.Count ?? 0;
            f.sfxCount = index.sfxKeys?.Count ?? 0;
            return f;
        }

        public bool Satisfies(string requirement)
        {
            if (string.IsNullOrWhiteSpace(requirement)) return true;
            switch (requirement.Trim().ToLowerInvariant())
            {
                case "units": return unitCount > 0;
                case "units:2+": return unitCount >= 2;
                case "units:3+": return unitCount >= 3;
                case "buildings": return buildingCount > 0;
                case "buildings:3+": return buildingCount >= 3;
                case "combat": return hasCombatUnits;
                case "ranged": return hasRangedUnits;
                case "economy": return hasEconomyBuildings;
                case "defense": return hasDefenseBuildings;
                case "settlement": return hasSettlementCenter;
                case "naval": return hasNavalUnits;
                case "music": return musicCount > 0;
                case "sfx": return sfxCount > 0;
                default:
                    // "units:5+" style numeric predicate
                    if (requirement.StartsWith("units:") && requirement.EndsWith("+")
                        && int.TryParse(requirement.Substring(6, requirement.Length - 7), out int n))
                        return unitCount >= n;
                    if (requirement.StartsWith("buildings:") && requirement.EndsWith("+")
                        && int.TryParse(requirement.Substring(10, requirement.Length - 11), out int b))
                        return buildingCount >= b;
                    return false;
            }
        }

        private static bool HasTag(ContentIndexEntry e, string tag)
        {
            if (e.tags == null) return false;
            foreach (var t in e.tags)
                if (string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }

    [Serializable]
    public sealed class MarketingClaim
    {
        public string id = string.Empty;
        public string category = "mechanic"; // mechanic | challenge | world | imperative
        public string en = string.Empty;
        public string uk = string.Empty;
        public string de = string.Empty;
        public string[] requires = Array.Empty<string>();

        public string For(string language)
        {
            switch ((language ?? "en").ToLowerInvariant())
            {
                case "uk": return string.IsNullOrEmpty(uk) ? en : uk;
                case "de": return string.IsNullOrEmpty(de) ? en : de;
                default: return en;
            }
        }
    }

    [Serializable]
    public sealed class MarketingClaimCatalogData
    {
        public string schema = "moyva.marketing-claims";
        public int version = 1;
        public List<MarketingClaim> claims = new List<MarketingClaim>();
    }

    /// <summary>
    /// Claim catalog + cliché filter. Only claims whose requirements are
    /// satisfied by the live index are ever emitted.
    /// </summary>
    public sealed class MarketingClaimCatalog
    {
        private static readonly string[] BannedFragments =
        {
            "epic journey", "every choice matters", "embark on", "unleash",
            "immerse yourself", "breathtaking", "stunning visuals", "forge your destiny",
            "like never before", "limitless", "endless possibilities", "epic adventure",
            "master the art of", "rich, vibrant world",
        };

        private readonly List<MarketingClaim> _claims;

        public MarketingClaimCatalog(MarketingClaimCatalogData data)
        {
            _claims = data?.claims ?? new List<MarketingClaim>();
        }

        public IReadOnlyList<MarketingClaim> All => _claims;

        public static bool IsBanned(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;
            string lower = text.ToLowerInvariant();
            foreach (var frag in BannedFragments)
                if (lower.Contains(frag)) return true;
            return false;
        }

        /// <summary>All claims valid for the current game content and language.</summary>
        public List<MarketingClaim> Eligible(MarketingFeatureFacts facts, string language)
        {
            var result = new List<MarketingClaim>();
            foreach (var c in _claims)
            {
                if (c == null || string.IsNullOrWhiteSpace(c.For(language))) continue;
                if (IsBanned(c.For(language))) continue;
                bool ok = true;
                foreach (var req in c.requires ?? Array.Empty<string>())
                    if (!facts.Satisfies(req)) { ok = false; break; }
                if (ok) result.Add(c);
            }
            return result;
        }

        /// <summary>Deterministically pick a claim for a beat (seeded).</summary>
        public MarketingClaim PickFor(MarketingFeatureFacts facts, string language, string preferredCategory, int seed)
        {
            var eligible = Eligible(facts, language);
            if (eligible.Count == 0) return null;
            var pool = eligible.FindAll(c => string.Equals(c.category, preferredCategory, StringComparison.OrdinalIgnoreCase));
            if (pool.Count == 0) pool = eligible;
            return pool[Math.Abs(seed) % pool.Count];
        }
    }
}
