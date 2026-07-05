using Sirenix.OdinInspector;
using UnityEngine;
using Kruty1918.Moyva.Construction.Runtime;

namespace Kruty1918.Moyva.Construction.API
{
    [CreateAssetMenu(menuName = "Moyva/Construction/Profiles/System", fileName = "ConstructionSystemProfile")]
    public sealed class ConstructionSystemProfileSO : ScriptableObject
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

        [BoxGroup("Defaults"), Required]
        [SerializeField] private ConstructionDiagnosticsProfileSO _diagnosticsProfile;

        [BoxGroup("Integrations")]
        [SerializeField] private ScriptableObject _economyRulesProfile;

        [BoxGroup("Integrations")]
        [SerializeField] private ScriptableObject _fogOfWarSettings;

        [BoxGroup("Registry")]
        [SerializeField] private BuildingDefinitionAsset[] _highlightedDefinitions = new BuildingDefinitionAsset[0];

        public BuildingRegistrySO BuildingRegistry => _buildingRegistry;
        public ConstructionPlacementRulesProfileSO PlacementRulesProfile => _placementRulesProfile;
        public ConstructionVisualProfileSO VisualProfile => _visualProfile;
        public ConstructionInputProfileSO InputProfile => _inputProfile;
        public ConstructionWallProfileSO WallProfile => _wallProfile;
        public ConstructionDiagnosticsProfileSO DiagnosticsProfile => _diagnosticsProfile;
        public ScriptableObject EconomyRulesProfile => _economyRulesProfile;
        public ScriptableObject FogOfWarSettings => _fogOfWarSettings;
        public BuildingDefinitionAsset[] HighlightedDefinitions => _highlightedDefinitions;
    }
}
