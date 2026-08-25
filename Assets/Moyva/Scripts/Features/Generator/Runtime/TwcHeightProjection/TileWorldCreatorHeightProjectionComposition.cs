namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorHeightProjectionComposition
    {
        public static ITileWorldCreatorHeightProjectionService Create()
        {
            var collector = new TileWorldCreatorTileTransformCollector();
            var offsets = new TileWorldCreatorHeightProjectionOffsetService();
            var applier = new TileWorldCreatorHeightProjectionApplier(collector, offsets);
            var stableActions = new TileWorldCreatorHeightProjectionStableActionService();
            return new TileWorldCreatorHeightProjectionService(applier, stableActions);
        }
    }
}
