using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [CreateAssetMenu(menuName = "Moyva/Construction/Profiles/Walls", fileName = "ConstructionWallProfile")]
    public sealed class ConstructionWallProfileSO : ScriptableObject
    {
        [BoxGroup("Behavior")]
        [SerializeField] private bool _allowGateReplacement = true;

        [BoxGroup("Behavior")]
        [SerializeField] private bool _gateRequiresHorizontalWall = true;

        [BoxGroup("Pathfinding")]
        [SerializeField] private bool _allowWallPathThroughExistingWalls = true;

        [BoxGroup("Pathfinding")]
        [SerializeField] private bool _allowWallPathThroughPendingWalls = true;

        [BoxGroup("Pathfinding")]
        [SerializeField] private bool _allowWallPathThroughGates = false;

        [BoxGroup("Pathfinding")]
        [SerializeField] private ConstructionWallPathMode _wallPathMode = ConstructionWallPathMode.OrthogonalOnly;

        [BoxGroup("Behavior")]
        [SerializeField] private bool _showWallHandles = true;

        public bool AllowGateReplacement => _allowGateReplacement;
        public bool GateRequiresHorizontalWall => _gateRequiresHorizontalWall;
        public bool AllowWallPathThroughExistingWalls => _allowWallPathThroughExistingWalls;
        public bool AllowWallPathThroughPendingWalls => _allowWallPathThroughPendingWalls;
        public bool AllowWallPathThroughGates => _allowWallPathThroughGates;
        public bool ShowWallHandles => _showWallHandles;
        public ConstructionWallPathMode WallPathMode => _wallPathMode;
    }
}
