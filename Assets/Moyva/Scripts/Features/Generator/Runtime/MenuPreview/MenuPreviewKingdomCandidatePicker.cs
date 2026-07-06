using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewKingdomCandidatePicker : IMenuPreviewKingdomCandidatePicker
    {
        private readonly IMenuPreviewKingdomPlacementGeometry _geometry;
        private readonly IMenuPreviewKingdomPlacementValidator _validator;

        public MenuPreviewKingdomCandidatePicker(
            IMenuPreviewKingdomPlacementGeometry geometry,
            IMenuPreviewKingdomPlacementValidator validator)
        {
            _geometry = geometry;
            _validator = validator;
        }

        public Vector2Int? PickInZone(
            MenuWorldPreviewKingdomPlacementContext context,
            RectInt zone,
            string buildingId,
            Vector2Int? farFrom,
            int minDistance)
        {
            if (string.IsNullOrWhiteSpace(buildingId) || context.Data.BuildingMap == null)
                return null;

            var clamped = _geometry.ClampToMap(context, zone);
            if (clamped.width <= 0 || clamped.height <= 0)
                return null;

            int maxAttempts = Mathf.Max(8, context.Settings.MaxAttemptsPerPlacement);
            for (int i = 0; i < maxAttempts; i++)
            {
                var candidate = new Vector2Int(
                    context.Random.Next(clamped.xMin, clamped.xMax),
                    context.Random.Next(clamped.yMin, clamped.yMax));

                if (_validator.IsValid(context, candidate, buildingId, farFrom, minDistance))
                    return candidate;
            }

            return null;
        }

        public Vector2Int? PickNear(
            MenuWorldPreviewKingdomPlacementContext context,
            Vector2Int center,
            int radius,
            string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId) || context.Data.BuildingMap == null)
                return null;

            int maxAttempts = Mathf.Max(8, context.Settings.MaxAttemptsPerPlacement);
            int safeRadius = Mathf.Max(1, radius);
            for (int i = 0; i < maxAttempts; i++)
            {
                var candidate = PickOffset(context, center, safeRadius);
                if (_validator.IsValid(context, candidate, buildingId, null, context.Settings.MinSettlementDistance))
                    return candidate;
            }

            return null;
        }

        public Vector2Int? PickSmallTown(MenuWorldPreviewKingdomPlacementContext context)
        {
            var data = context.Data;
            var candidate = new Vector2Int(context.Random.Next(0, data.Width), context.Random.Next(0, data.Height));
            return _validator.IsValid(
                context,
                candidate,
                context.Settings.TownHallBuildingId,
                null,
                context.Settings.MinSettlementDistance)
                ? candidate
                : null;
        }

        private Vector2Int PickOffset(
            MenuWorldPreviewKingdomPlacementContext context,
            Vector2Int center,
            int safeRadius)
        {
            int dx = context.Random.Next(-safeRadius, safeRadius + 1);
            int dy = context.Random.Next(-safeRadius, safeRadius + 1);
            return dx == 0 && dy == 0
                ? new Vector2Int(-1, -1)
                : new Vector2Int(center.x + dx, center.y + dy);
        }
    }
}
