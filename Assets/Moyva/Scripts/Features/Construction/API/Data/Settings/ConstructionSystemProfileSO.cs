using Sirenix.OdinInspector;
using UnityEngine;
using Kruty1918.Moyva.Construction.Runtime;

using Kruty1918.JsonConfig;
namespace Kruty1918.Moyva.Construction.API
{
[System.Serializable]
public sealed class ConstructionSystemProfileSO : JsonConfigObject
    {
        [BoxGroup("Registry"), Required]
        [SerializeField] private BuildingRegistrySO _buildingRegistry;

        [BoxGroup("Defaults"), Required]
        [SerializeField] private ConstructionPlacementRulesProfileSO _placementRulesProfile;

        [BoxGroup("Defaults"), Required]
        [SerializeField] private ConstructionVisualProfileSO _visualProfile;

        [BoxGroup("Defaults"), Required]
        [SerializeField] private ConstructionInputProfileSO _inputProfile;

        [BoxGroup("Defaults"), Required]
        [SerializeField] private ConstructionWallProfileSO _wallProfile;

        [BoxGroup("Integrations")]
        [SerializeField] private JsonConfigObject _economyRulesProfile;

        [BoxGroup("Integrations")]
        [SerializeField] private JsonConfigObject _fogOfWarSettings;

        [BoxGroup("Registry")]
        [SerializeField] private BuildingDefinitionAsset[] _highlightedDefinitions = new BuildingDefinitionAsset[0];

        public BuildingRegistrySO BuildingRegistry => _buildingRegistry;
        public ConstructionPlacementRulesProfileSO PlacementRulesProfile => _placementRulesProfile;
        public ConstructionVisualProfileSO VisualProfile => _visualProfile;
        public ConstructionInputProfileSO InputProfile => _inputProfile;
        public ConstructionWallProfileSO WallProfile => _wallProfile;
        public JsonConfigObject EconomyRulesProfile => _economyRulesProfile;
        public JsonConfigObject FogOfWarSettings => _fogOfWarSettings;
        public BuildingDefinitionAsset[] HighlightedDefinitions => _highlightedDefinitions;
    }
}
