using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.AI.Bot
{
    /// Single authority for capability naming: scenario JSON availableCapabilities
    /// strings, intent->capability gating and the capability observation bitmask all
    /// resolve through this map. Adding a BotCapabilityId or BotIntentType member
    /// without extending this map fails fast (unknown names reject at load time).
    public static class BotCapabilityMap
    {
        // Canonical JSON name for each capability. "end-turn"/"scouting" are the
        // authored names; enum-name aliases ("turn", "exploration") are accepted too.
        private static readonly Dictionary<string, BotCapabilityId> ByName =
            new Dictionary<string, BotCapabilityId>(StringComparer.OrdinalIgnoreCase)
            {
                ["end-turn"] = BotCapabilityId.Turn,
                ["turn"] = BotCapabilityId.Turn,
                ["movement"] = BotCapabilityId.Movement,
                ["combat"] = BotCapabilityId.Combat,
                ["recruitment"] = BotCapabilityId.Recruitment,
                ["construction"] = BotCapabilityId.Construction,
                ["capture"] = BotCapabilityId.Capture,
                ["economy"] = BotCapabilityId.Economy,
                ["scouting"] = BotCapabilityId.Exploration,
                ["exploration"] = BotCapabilityId.Exploration,
            };

        private static readonly BotCapabilityId?[] ByIntent = BuildIntentMap();

        private static BotCapabilityId?[] BuildIntentMap()
        {
            var map = new BotCapabilityId?[Enum.GetValues(typeof(BotIntentType)).Length];
            map[(int)BotIntentType.EndTurn] = BotCapabilityId.Turn;
            map[(int)BotIntentType.Wait] = BotCapabilityId.Turn;
            map[(int)BotIntentType.Move] = BotCapabilityId.Movement;
            map[(int)BotIntentType.Reposition] = BotCapabilityId.Movement;
            map[(int)BotIntentType.Attack] = BotCapabilityId.Combat;
            map[(int)BotIntentType.Defend] = BotCapabilityId.Combat;
            map[(int)BotIntentType.Capture] = BotCapabilityId.Capture;
            map[(int)BotIntentType.Recruit] = BotCapabilityId.Recruitment;
            map[(int)BotIntentType.Build] = BotCapabilityId.Construction;
            map[(int)BotIntentType.Economy] = BotCapabilityId.Economy;
            map[(int)BotIntentType.Explore] = BotCapabilityId.Exploration;
            return map;
        }

        public static bool TryParse(string name, out BotCapabilityId id)
        {
            if (!string.IsNullOrWhiteSpace(name) && ByName.TryGetValue(name.Trim(), out id)) return true;
            id = default;
            return false;
        }

        // Null means the intent is always legal regardless of capability gating.
        public static BotCapabilityId? ForIntent(BotIntentType intent)
        {
            int index = (int)intent;
            return index >= 0 && index < ByIntent.Length ? ByIntent[index] : null;
        }

        // Capability bitmask for the observation vector. Unknown names are ignored
        // here because scenario validation rejects them before this runs.
        public static int Mask(IEnumerable<string> names)
        {
            int mask = 1 << (int)BotCapabilityId.Turn; // EndTurn always remains legal.
            if (names != null)
                foreach (var name in names)
                    if (TryParse(name, out var id))
                        mask |= 1 << (int)id;
            return mask;
        }
    }
}
