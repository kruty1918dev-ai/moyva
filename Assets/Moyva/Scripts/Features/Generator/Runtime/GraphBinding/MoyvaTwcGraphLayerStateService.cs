using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MoyvaTwcGraphLayerStateService : IMoyvaTwcGraphLayerStateService
    {
        public bool EnableOnly(Configuration configuration, string layerName, out List<MoyvaTwcGraphLayerState> previousStates)
        {
            previousStates = new List<MoyvaTwcGraphLayerState>();
            bool matched = false;
            if (configuration?.blueprintLayerFolders == null)
                return false;

            foreach (var folder in configuration.blueprintLayerFolders)
            {
                if (folder?.blueprintLayers == null)
                    continue;

                foreach (var layer in folder.blueprintLayers)
                {
                    if (layer == null)
                        continue;

                    previousStates.Add(new MoyvaTwcGraphLayerState(layer, layer.isEnabled));
                    bool isTarget = string.Equals(layer.layerName, layerName, StringComparison.Ordinal);
                    layer.isEnabled = isTarget;
                    matched |= isTarget;
                }
            }

            return matched;
        }

        public void Restore(List<MoyvaTwcGraphLayerState> states)
        {
            if (states == null)
                return;

            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].Layer != null)
                    states[i].Layer.isEnabled = states[i].Enabled;
            }
        }
    }
}
