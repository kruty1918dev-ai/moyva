using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuWorldPreviewKingdomPlacementContext
    {
        public MenuWorldPreviewKingdomPlacementContext(
            MenuWorldPreviewData data,
            MenuPreviewKingdomPlacementSettings settings,
            MenuWorldPreviewKingdomPlacementReport report)
        {
            Data = data;
            Settings = settings;
            Report = report;
            Random = new System.Random(unchecked(data.Seed ^ 0x41D1A53B));
            ForbiddenTiles = BuildForbiddenTileSet(settings);
        }

        public MenuWorldPreviewData Data { get; }
        public MenuPreviewKingdomPlacementSettings Settings { get; }
        public MenuWorldPreviewKingdomPlacementReport Report { get; }
        public System.Random Random { get; }
        public HashSet<Vector2Int> NewPlacements { get; } = new HashSet<Vector2Int>();
        public HashSet<string> ForbiddenTiles { get; }

        private static HashSet<string> BuildForbiddenTileSet(MenuPreviewKingdomPlacementSettings settings)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (settings.ForbiddenBiomeTileIds == null)
                return result;

            foreach (var tileId in settings.ForbiddenBiomeTileIds)
            {
                if (!string.IsNullOrWhiteSpace(tileId))
                    result.Add(tileId.Trim());
            }

            return result;
        }
    }
}
