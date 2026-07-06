using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorLayerPositionApplier
    {
        void Apply(Dictionary<string, HashSet<Vector2>> positionsByLayerGuid);
    }

    internal sealed class TileWorldCreatorLayerPositionApplier : ITileWorldCreatorLayerPositionApplier
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private readonly TileWorldCreatorManager _manager;

        public TileWorldCreatorLayerPositionApplier(ITileWorldCreatorBuildEnvironment environment)
        {
            _manager = environment.Manager;
        }

        public void Apply(Dictionary<string, HashSet<Vector2>> positionsByLayerGuid)
        {
            foreach (var layerPositions in positionsByLayerGuid)
            {
                if (layerPositions.Value == null || layerPositions.Value.Count == 0)
                    continue;

                Debug.Log($"{LogTag} AddCellsToLayerByGuid layerGuid='{layerPositions.Key}', positions={layerPositions.Value.Count}, bounds={TileWorldCreatorMapFormatUtility.FormatPositionBounds(layerPositions.Value)}.");
                _manager.AddCellsToLayerByGuid(layerPositions.Key, layerPositions.Value);
            }
        }
    }
}
