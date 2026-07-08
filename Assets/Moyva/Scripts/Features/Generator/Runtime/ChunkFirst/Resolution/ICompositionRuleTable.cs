namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal interface ICompositionRuleTable
    {
        bool TryCompare(
            GraphTileLayerSample current,
            GraphTileLayerSample candidate,
            TileNeighborhood neighborhood,
            out int result,
            out string reason);
    }

    internal sealed class DefaultCompositionRuleTable : ICompositionRuleTable
    {
        public bool TryCompare(
            GraphTileLayerSample current,
            GraphTileLayerSample candidate,
            TileNeighborhood neighborhood,
            out int result,
            out string reason)
        {
            result = 0;
            reason = null;
            return false;
        }
    }
}
