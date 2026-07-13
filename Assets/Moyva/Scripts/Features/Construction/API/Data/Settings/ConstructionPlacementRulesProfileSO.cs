using Sirenix.OdinInspector;
using UnityEngine;
using Kruty1918.Moyva.WorldCreation.API;

namespace Kruty1918.Moyva.Construction.API
{
    [CreateAssetMenu(menuName = "Moyva/Construction/Profiles/Placement Rules", fileName = "ConstructionPlacementRulesProfile")]
    public sealed class ConstructionPlacementRulesProfileSO : ScriptableObject
    {
        [BoxGroup("Spacing")]
        [MinValue(0)]
        [SerializeField] private int _minSpacing;

        [BoxGroup("Influence")]
        [MinValue(0)]
        [SerializeField] private int _townHallBuildRadius;

        [BoxGroup("Influence")]
        [SerializeField] private bool _enableInfluenceZoneRules = true;

        [BoxGroup("Terrain")]
        [SerializeField] private bool _enableTerrainRules = true;

        [BoxGroup("Terrain")]
        [SerializeField] private bool _allowBuildingOnWater;

        [BoxGroup("Terrain")]
        [SerializeField] private bool _allowBuildingOnHills = true;

        [BoxGroup("Terrain")]
        [SerializeField] private bool _blockEdgeTerrainTiles = true;

        [BoxGroup("Fog")]
        [SerializeField] private bool _enableFogRules = true;

        [BoxGroup("Fog")]
        [SerializeField] private bool _requireVisibleFogTile = true;

        [BoxGroup("Terrain")]
        [SerializeField] private string[] _blockedTileIds = new string[0];

        [BoxGroup("Terrain")]
        [SerializeField] private string[] _allowedTileIds = new string[0];

        [BoxGroup("Terrain")]
        [SerializeField] private TerrainLevelRestrictionRange[] _blockedTerrainLevelRanges = new TerrainLevelRestrictionRange[0];

        public int MinSpacing => Mathf.Max(0, _minSpacing);
        public int TownHallBuildRadius => Mathf.Max(0, _townHallBuildRadius);
        public bool EnableInfluenceZoneRules => _enableInfluenceZoneRules;
        public bool EnableTerrainRules => _enableTerrainRules;
        public bool AllowBuildingOnWater => _allowBuildingOnWater;
        public bool AllowBuildingOnHills => _allowBuildingOnHills;
        public bool BlockEdgeTerrainTiles => _blockEdgeTerrainTiles;
        public bool EnableFogRules => _enableFogRules;
        public bool RequireVisibleFogTile => _requireVisibleFogTile;
        public string[] BlockedTileIds => _blockedTileIds;
        public string[] AllowedTileIds => _allowedTileIds;
        public TerrainLevelRestrictionRange[] BlockedTerrainLevelRanges => _blockedTerrainLevelRanges;
    }
}
