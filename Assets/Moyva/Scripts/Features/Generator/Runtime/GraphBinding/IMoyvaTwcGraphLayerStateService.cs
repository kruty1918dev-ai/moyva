using System.Collections.Generic;
using GiantGrey.TileWorldCreator;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMoyvaTwcGraphLayerStateService
    {
        bool EnableOnly(Configuration configuration, string layerName, out List<MoyvaTwcGraphLayerState> previousStates);
        void Restore(List<MoyvaTwcGraphLayerState> states);
    }
}
