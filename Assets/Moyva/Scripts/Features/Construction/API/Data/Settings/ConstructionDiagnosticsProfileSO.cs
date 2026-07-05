using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [CreateAssetMenu(menuName = "Moyva/Construction/Profiles/Diagnostics", fileName = "ConstructionDiagnosticsProfile")]
    public sealed class ConstructionDiagnosticsProfileSO : ScriptableObject
    {
        [BoxGroup("Logging")]
        [SerializeField] private bool _enableVerboseLogs = true;

        [BoxGroup("Logging")]
        [SerializeField] private bool _enablePlacementDebug = true;

        [BoxGroup("Logging")]
        [SerializeField] private bool _enableResourceDebug = true;

        [BoxGroup("Logging")]
        [SerializeField] private bool _enableVisualDebug = true;

        [BoxGroup("Logging")]
        [SerializeField] private bool _enableWallDebug = true;

        [BoxGroup("Scene Debug")]
        [SerializeField] private bool _drawSceneGizmos = true;

        [BoxGroup("Scene Debug")]
        [SerializeField] private bool _drawBlockedTiles;

        [BoxGroup("Scene Debug")]
        [SerializeField] private bool _drawInfluenceZones = true;

        public bool EnableVerboseLogs => _enableVerboseLogs;
        public bool EnablePlacementDebug => _enablePlacementDebug;
        public bool EnableResourceDebug => _enableResourceDebug;
        public bool EnableVisualDebug => _enableVisualDebug;
        public bool EnableWallDebug => _enableWallDebug;
        public bool DrawSceneGizmos => _drawSceneGizmos;
        public bool DrawBlockedTiles => _drawBlockedTiles;
        public bool DrawInfluenceZones => _drawInfluenceZones;
    }
}
