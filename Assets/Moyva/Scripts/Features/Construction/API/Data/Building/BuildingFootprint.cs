using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public enum BuildingFootprintAnchor
    {
        Center = 0,
        SouthWest = 1,
        Custom = 2,
    }

    [Serializable]
    public sealed class BuildingFootprint
    {
        [MinValue(1)]
        public Vector2Int Size = Vector2Int.one;

        public BuildingFootprintAnchor Anchor = BuildingFootprintAnchor.Center;

        [ShowIf(nameof(Anchor), BuildingFootprintAnchor.Custom)]
        public Vector2Int CustomAnchor;

        public bool BlocksMovement = true;
        public bool BlocksConstruction = true;
        public bool RequiresFlatGround = true;

        [TableList(AlwaysExpanded = false)]
        public Vector2Int[] OccupiedCells = Array.Empty<Vector2Int>();

        [TableList(AlwaysExpanded = false)]
        public Vector2Int[] EntranceCells = Array.Empty<Vector2Int>();

        [TableList(AlwaysExpanded = false)]
        public Vector2Int[] RoadConnectionCells = Array.Empty<Vector2Int>();
    }
}
