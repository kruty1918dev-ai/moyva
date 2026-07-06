using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public static class MenuWorldPreviewKingdomPlacer
    {
        public static MenuWorldPreviewKingdomPlacementReport Apply(MenuWorldPreviewData previewData, MenuPreviewKingdomPlacementSettings settings)
        {
            return MenuWorldPreviewKingdomPlacementComposition.Create()
                .Apply(previewData, settings);
        }
    }

    public sealed class MenuWorldPreviewKingdomPlacementReport
    {
        public int PlacedCastles;
        public int FailedCastles;
        public int PlacedWarehouses;
        public int FailedWarehouses;
        public int PlacedLocalSettlements;
        public int FailedLocalSettlements;
        public int PlacedSmallTowns;
        public int FailedSmallTowns;
        public int CastleDistance;
        public string Warning;

        public override string ToString()
        {
            return $"castles={PlacedCastles}/{PlacedCastles + FailedCastles}, "
                   + $"warehouses={PlacedWarehouses}/{PlacedWarehouses + FailedWarehouses}, "
                   + $"local={PlacedLocalSettlements}/{PlacedLocalSettlements + FailedLocalSettlements}, "
                   + $"smallTowns={PlacedSmallTowns}/{PlacedSmallTowns + FailedSmallTowns}, "
                   + $"castleDistance={CastleDistance}";
        }
    }
}
