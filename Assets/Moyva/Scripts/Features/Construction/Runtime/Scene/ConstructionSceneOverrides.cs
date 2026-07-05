using Sirenix.OdinInspector;
using UnityEngine;
using Kruty1918.Moyva.Construction.API;

namespace Kruty1918.Moyva.Construction.Runtime
{
    [System.Serializable]
    public sealed class ConstructionSceneOverrides
    {
        [BoxGroup("Overrides")]
        [SerializeField] private ConstructionPlacementRulesProfileSO _placementRulesProfileOverride;

        [BoxGroup("Overrides")]
        [SerializeField] private ConstructionVisualProfileSO _visualProfileOverride;

        [BoxGroup("Overrides")]
        [SerializeField] private ConstructionInputProfileSO _inputProfileOverride;

        [BoxGroup("Overrides")]
        [SerializeField] private ConstructionWallProfileSO _wallProfileOverride;

        [BoxGroup("Overrides")]
        [SerializeField] private ConstructionDiagnosticsProfileSO _diagnosticsProfileOverride;

        public ConstructionPlacementRulesProfileSO PlacementRulesProfileOverride => _placementRulesProfileOverride;
        public ConstructionVisualProfileSO VisualProfileOverride => _visualProfileOverride;
        public ConstructionInputProfileSO InputProfileOverride => _inputProfileOverride;
        public ConstructionWallProfileSO WallProfileOverride => _wallProfileOverride;
        public ConstructionDiagnosticsProfileSO DiagnosticsProfileOverride => _diagnosticsProfileOverride;
    }
}
