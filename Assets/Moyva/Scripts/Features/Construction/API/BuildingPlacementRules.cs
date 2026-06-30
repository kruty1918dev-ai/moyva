using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingPlacementRules
    {
        public bool CanPlaceInFog;
        public bool RequiresSettlementInfluence = true;
        public bool CreatesSettlementInfluence;
        public bool BlockIfSettlementCenterInRange;

        [MinValue(0)]
        public int InfluenceRadius;

        [MinValue(0)]
        public int MinDistanceFromSettlementCenters;

        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = false)]
        public string[] RequiredTerrainIds = Array.Empty<string>();

        [TableList(AlwaysExpanded = false)]
        public Vector2Int[] RequiredNeighborOffsets = Array.Empty<Vector2Int>();

        public bool RequiresWaterNearby;
        public bool RequiresForestNearby;
        public bool RequiresMountainNearby;
        public bool RequiresRoadNearby;

        [TableList(AlwaysExpanded = false)]
        public TileRequirementDefinition[] NearbyTileRequirements = Array.Empty<TileRequirementDefinition>();
    }
}
