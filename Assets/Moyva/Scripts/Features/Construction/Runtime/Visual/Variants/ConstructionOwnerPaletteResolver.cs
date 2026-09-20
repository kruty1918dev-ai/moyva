using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Maps a building owner id to a stable palette key and resolves the
    /// matching prefab variant declared on the building definition.
    /// The palette index follows the turn-faction order so owner colors stay
    /// consistent across local play, multiplayer replicas and restores.
    /// </summary>
    internal sealed class ConstructionOwnerPaletteResolver
    {
        internal const string DefaultPaletteKey = "blue";

        // Index 0 stays blue so the historical single-player look is unchanged.
        private static readonly string[] PaletteOrder =
        {
            "blue",
            "red",
            "green",
            "yellow",
        };

        private readonly ITurnService _turnService;

        [Inject]
        public ConstructionOwnerPaletteResolver(
            [InjectOptional] ITurnService turnService = null)
        {
            _turnService = turnService;
        }

        public GameObject ResolvePlacedPrefab(BuildingDefinition definition, string ownerId)
        {
            GameObject fallback = definition?.Prefab;
            if (fallback == null)
                return null;

            Dictionary<string, GameObject> variants =
                definition.Presentation?.Variants?.PrefabVariants;
            if (variants == null || variants.Count == 0)
                return fallback;

            return variants.TryGetValue(ResolvePaletteKey(ownerId), out GameObject variant)
                   && variant != null
                ? variant
                : fallback;
        }

        public GameObject ResolvePreviewPrefab(BuildingDefinition definition, string ownerId)
        {
            GameObject explicitPreview = definition?.Presentation?.PreviewPrefab;
            return explicitPreview != null
                ? explicitPreview
                : ResolvePlacedPrefab(definition, ownerId);
        }

        public string ResolvePaletteKey(string ownerId)
        {
            int index = ResolveOwnerIndex(ownerId);
            return index < 0
                ? DefaultPaletteKey
                : PaletteOrder[index % PaletteOrder.Length];
        }

        private int ResolveOwnerIndex(string ownerId)
        {
            string normalized = ownerId?.Trim() ?? string.Empty;
            IReadOnlyList<TurnFaction> factions = _turnService?.Factions;
            if (factions != null)
            {
                for (int i = 0; i < factions.Count; i++)
                {
                    if (string.Equals(factions[i].OwnerId, normalized, StringComparison.Ordinal))
                        return i;
                }
            }

            int suffix = ParseTrailingIndex(normalized);
            if (suffix >= 0)
                return suffix;

            return normalized.Length == 0 ? -1 : (int)(StableHash(normalized) % 1024u);
        }

        private static int ParseTrailingIndex(string ownerId)
        {
            int end = ownerId.Length - 1;
            int start = end;
            while (start >= 0 && char.IsDigit(ownerId[start]))
                start--;

            return start < end && int.TryParse(
                ownerId.Substring(start + 1),
                out int index)
                ? index
                : -1;
        }

        // FNV-1a so unknown owner ids still resolve to a stable palette entry.
        private static uint StableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                for (int i = 0; i < value.Length; i++)
                    hash = (hash ^ value[i]) * 16777619u;
                return hash;
            }
        }
    }
}
