using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [System.Serializable]
    public class WallCollectionDefinition
    {
        [Header("IDs")]
        public string CollectionId = "default-wall";
        public string WallBuildingId = "wall";
        public string GateBuildingId = "gate";

        [Header("Wall Variants (9)")]
        public GameObject IsolatedPrefab;
        public GameObject HorizontalPrefab;
        public GameObject VerticalPrefab;
        public GameObject CornerNorthEastPrefab;
        public GameObject CornerNorthWestPrefab;
        public GameObject CornerSouthEastPrefab;
        public GameObject CornerSouthWestPrefab;
        public GameObject TJunctionPrefab;
        public GameObject CrossPrefab;

        [Header("Gate")]
        public GameObject GatePrefab;

        public bool ContainsBuilding(string buildingId)
        {
            return buildingId == WallBuildingId || buildingId == GateBuildingId;
        }

        public bool IsWall(string buildingId)
        {
            return buildingId == WallBuildingId;
        }

        public bool IsGate(string buildingId)
        {
            return buildingId == GateBuildingId;
        }
    }
}
