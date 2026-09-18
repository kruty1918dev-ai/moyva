namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal interface ICompositionRuleTable
    {
        bool TryCompare(
            TileLayerSample current,
            TileLayerSample candidate,
            TileNeighborhood neighborhood,
            out int result,
            out string reason);
    }

    internal sealed class DefaultCompositionRuleTable : ICompositionRuleTable
    {
        public bool TryCompare(
            TileLayerSample current,
            TileLayerSample candidate,
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
