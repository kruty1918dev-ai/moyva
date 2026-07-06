using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewKingdomPlacementOrchestrator : IMenuPreviewKingdomPlacementOrchestrator
    {
        private readonly IMenuPreviewKingdomCandidatePicker _picker;
        private readonly IMenuPreviewKingdomPlacementWriter _writer;
        private readonly IMenuPreviewKingdomPlacementGeometry _geometry;

        public MenuPreviewKingdomPlacementOrchestrator(
            IMenuPreviewKingdomCandidatePicker picker,
            IMenuPreviewKingdomPlacementWriter writer,
            IMenuPreviewKingdomPlacementGeometry geometry)
        {
            _picker = picker;
            _writer = writer;
            _geometry = geometry;
        }

        public void Run(MenuWorldPreviewKingdomPlacementContext context)
        {
            if (context.Data.BuildingMap == null)
                context.Report.Warning = "BuildingMap is null and cannot be modified.";

            Vector2Int? castleA = PlaceCastle(context, context.Settings.KingdomAZone, null);
            Vector2Int? castleB = PlaceCastle(context, context.Settings.KingdomBZone, castleA);

            if (castleA.HasValue)
                PlaceKingdomAroundCastle(context, castleA.Value);
            if (castleB.HasValue)
                PlaceKingdomAroundCastle(context, castleB.Value);

            PlaceSmallTowns(context);

            if (castleA.HasValue && castleB.HasValue)
                context.Report.CastleDistance = _geometry.Manhattan(castleA.Value, castleB.Value);
        }

        private Vector2Int? PlaceCastle(
            MenuWorldPreviewKingdomPlacementContext context,
            RectInt zone,
            Vector2Int? farFrom)
        {
            var position = _picker.PickInZone(
                context,
                zone,
                context.Settings.CastleBuildingId,
                farFrom,
                context.Settings.CastleMinDistance);

            if (!position.HasValue)
            {
                context.Report.FailedCastles++;
                return null;
            }

            _writer.Place(context, position.Value, context.Settings.CastleBuildingId);
            context.Report.PlacedCastles++;
            return position;
        }

        private void PlaceKingdomAroundCastle(MenuWorldPreviewKingdomPlacementContext context, Vector2Int castlePos)
        {
            PlaceRepeated(
                context,
                context.Settings.WarehousesPerKingdom,
                context.Settings.WarehouseBuildingId,
                castlePos,
                placed => context.Report.PlacedWarehouses += placed ? 1 : 0,
                failed => context.Report.FailedWarehouses += failed ? 1 : 0);

            PlaceRepeated(
                context,
                context.Settings.KingdomLocalSettlementCount,
                context.Settings.LocalSettlementBuildingId,
                castlePos,
                placed => context.Report.PlacedLocalSettlements += placed ? 1 : 0,
                failed => context.Report.FailedLocalSettlements += failed ? 1 : 0);
        }

        private void PlaceRepeated(
            MenuWorldPreviewKingdomPlacementContext context,
            int count,
            string buildingId,
            Vector2Int center,
            System.Action<bool> onPlaced,
            System.Action<bool> onFailed)
        {
            for (int i = 0; i < count; i++)
            {
                var position = _picker.PickNear(context, center, context.Settings.KingdomSettlementRadius, buildingId);
                if (position.HasValue)
                {
                    _writer.Place(context, position.Value, buildingId);
                    onPlaced(true);
                }
                else
                {
                    onFailed(true);
                }
            }
        }

        private void PlaceSmallTowns(MenuWorldPreviewKingdomPlacementContext context)
        {
            int maxAttempts = Mathf.Max(context.Settings.MaxAttemptsPerPlacement, context.Settings.SmallTownCount * 24);
            int attempts = 0;
            while (context.Report.PlacedSmallTowns < context.Settings.SmallTownCount && attempts < maxAttempts)
            {
                attempts++;
                var candidate = _picker.PickSmallTown(context);
                if (!candidate.HasValue)
                    continue;

                _writer.Place(context, candidate.Value, context.Settings.TownHallBuildingId);
                context.Report.PlacedSmallTowns++;
            }

            context.Report.FailedSmallTowns += Mathf.Max(0, context.Settings.SmallTownCount - context.Report.PlacedSmallTowns);
        }
    }
}
