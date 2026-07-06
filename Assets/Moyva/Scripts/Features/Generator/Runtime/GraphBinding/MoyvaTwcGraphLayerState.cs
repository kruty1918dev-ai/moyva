using GiantGrey.TileWorldCreator;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct MoyvaTwcGraphLayerState
    {
        public MoyvaTwcGraphLayerState(BlueprintLayer layer, bool enabled)
        {
            Layer = layer;
            Enabled = enabled;
        }

        public BlueprintLayer Layer { get; }
        public bool Enabled { get; }
    }
}
