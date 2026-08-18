using Sirenix.OdinInspector;
using UnityEngine;
using Kruty1918.Moyva.WorldCreation.API;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Construction.API
{
[System.Serializable]
public sealed class ConstructionPlacementRulesProfileSO : MoyvaJsonConfigObject
    {
        [BoxGroup("Відступ")]
        [MinValue(0)]
        [LabelText("Мінімальний відступ")]
        [Tooltip("Глобальна мінімальна відстань у клітинках між будівлями.")]
        [SerializeField] private int _minSpacing;

        [BoxGroup("Вплив поселення")]
        [MinValue(0)]
        [LabelText("Радіус поселення")]
        [Tooltip("Глобальний fallback-радіус впливу поселення.")]
        [SerializeField] private int _townHallBuildRadius;

        [BoxGroup("Вплив поселення")]
        [LabelText("Перевіряти вплив поселення")]
        [SerializeField] private bool _enableInfluenceZoneRules = true;

        [BoxGroup("Terrain")]
        [LabelText("Перевіряти terrain")]
        [SerializeField] private bool _enableTerrainRules = true;

        [BoxGroup("Terrain")]
        [LabelText("Дозволити будівництво на воді")]
        [Tooltip("Глобальне правило для будівель у режимі «Успадкувати». Залиште вимкненим для звичайних наземних будівель.")]
        [SerializeField] private bool _allowBuildingOnWater;

        [BoxGroup("Terrain")]
        [LabelText("Дозволити пагорби")]
        [SerializeField] private bool _allowBuildingOnHills = true;

        [BoxGroup("Terrain")]
        [LabelText("Блокувати край перепаду")]
        [SerializeField] private bool _blockEdgeTerrainTiles = true;

        [BoxGroup("Туман війни")]
        [LabelText("Перевіряти туман війни")]
        [SerializeField] private bool _enableFogRules = true;

        [BoxGroup("Туман війни")]
        [LabelText("Вимагати видиму клітинку")]
        [SerializeField] private bool _requireVisibleFogTile = true;

        [BoxGroup("Terrain")]
        [LabelText("Заборонені точні terrain ID")]
        [SerializeField] private string[] _blockedTileIds = new string[0];

        [BoxGroup("Terrain")]
        [LabelText("Дозволені точні terrain ID")]
        [SerializeField] private string[] _allowedTileIds = new string[0];

        [BoxGroup("Terrain")]
        [LabelText("Заборонені діапазони висоти")]
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
