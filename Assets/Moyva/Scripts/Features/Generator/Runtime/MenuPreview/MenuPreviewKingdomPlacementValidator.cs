using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewKingdomPlacementValidator : IMenuPreviewKingdomPlacementValidator
    {
        private readonly IMenuPreviewKingdomPlacementGeometry _geometry;

        public MenuPreviewKingdomPlacementValidator(IMenuPreviewKingdomPlacementGeometry geometry)
        {
            _geometry = geometry;
        }

        public bool IsValid(
            MenuWorldPreviewKingdomPlacementContext context,
            Vector2Int position,
            string buildingId,
            Vector2Int? farFrom,
            int minDistance)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return false;
            if (!_geometry.IsInBounds(context, position))
                return false;
            if (context.Data.BuildingMap == null)
                return false;
            if (!string.IsNullOrWhiteSpace(context.Data.BuildingMap[position.x, position.y]))
                return false;
            if (!IsHeightAllowed(context, position))
                return false;
            if (IsForbiddenTile(context, position))
                return false;
            if (IsTooCloseToRequiredPoint(context, position, farFrom, minDistance))
                return false;

            return IsFarEnoughFromNewPlacements(context, position);
        }

        private bool IsHeightAllowed(MenuWorldPreviewKingdomPlacementContext context, Vector2Int position)
        {
            if (context.Data.HeightMap == null)
                return true;

            float height = context.Data.HeightMap[position.x, position.y];
            return height >= context.Settings.MinHeight && height <= context.Settings.MaxHeight;
        }

        private bool IsForbiddenTile(MenuWorldPreviewKingdomPlacementContext context, Vector2Int position)
        {
            if (context.Data.BiomeMap == null || context.ForbiddenTiles.Count == 0)
                return false;

            string biome = context.Data.BiomeMap[position.x, position.y];
            if (string.IsNullOrWhiteSpace(biome))
                return false;
            if (context.ForbiddenTiles.Contains(biome))
                return true;
            if (!context.Settings.MatchBaseTileType)
                return false;

            int sepIndex = biome.IndexOf(context.Settings.TileSeparator);
            return sepIndex > 0 && context.ForbiddenTiles.Contains(biome.Substring(0, sepIndex));
        }

        private bool IsTooCloseToRequiredPoint(
            MenuWorldPreviewKingdomPlacementContext context,
            Vector2Int position,
            Vector2Int? farFrom,
            int minDistance)
        {
            if (!farFrom.HasValue)
                return false;

            int safeDistance = Mathf.Max(1, minDistance);
            return _geometry.Manhattan(position, farFrom.Value) < safeDistance;
        }

        private bool IsFarEnoughFromNewPlacements(
            MenuWorldPreviewKingdomPlacementContext context,
            Vector2Int position)
        {
            int safeDistance = Mathf.Max(1, context.Settings.MinSettlementDistance);
            foreach (var placed in context.NewPlacements)
            {
                if (_geometry.Manhattan(placed, position) < safeDistance)
                    return false;
            }

            return true;
        }
    }
}
