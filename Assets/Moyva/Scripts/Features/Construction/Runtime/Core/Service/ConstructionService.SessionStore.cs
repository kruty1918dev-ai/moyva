using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private readonly struct PendingPlacement
        {
            public PendingPlacement(
                Vector2Int position,
                string buildingId,
                Vector2Int? originalPosition = null,
                string replacedPendingBuildingId = null,
                ConstructionRotation rotation = ConstructionRotation.Degrees0)
            {
                Position = position;
                BuildingId = buildingId;
                OriginalPosition = originalPosition;
                ReplacedPendingBuildingId = replacedPendingBuildingId;
                Rotation = ConstructionRotationUtility.Normalize((int)rotation);
            }

            public Vector2Int Position { get; }
            public string BuildingId { get; }
            public Vector2Int? OriginalPosition { get; }
            public string ReplacedPendingBuildingId { get; }
            public ConstructionRotation Rotation { get; }
        }

        private readonly struct PendingDemolition
        {
            public PendingDemolition(Vector2Int position, string buildingId)
            {
                Position = position;
                BuildingId = buildingId;
            }

            public Vector2Int Position { get; }
            public string BuildingId { get; }
        }

        private sealed class ConstructionSessionStore
        {
            public BuildingPlacementState State = BuildingPlacementState.Idle;
            public bool IsDemolishMode;
            public string SelectedBuildingId;
            public ConstructionRotation SelectedRotation;
            public string ActiveOwnerId = DefaultOwnerId;
            public string LastActionMessage = string.Empty;
            public bool IsActive;
            public int PendingPlacementsVersion;
            public int PlacementSimulationSnapshotVersion = -1;

            public readonly List<PendingPlacement> PendingPlacements = new();
            public readonly HashSet<Vector2Int> PendingPositions = new();
            public readonly Dictionary<Vector2Int, PendingPlacement>
                PendingPlacementByPosition = new();
            public readonly Dictionary<
                Vector2Int,
                ConstructionPendingPlacementStatus> PendingPlacementStatuses = new();
            public readonly List<BuildingPlacementSimulationEntry>
                PlacementSimulationSnapshot = new();
            public readonly List<BuildingPlacementSimulationEntry>
                PlacedBuildingSimulationSnapshot = new();
            public readonly HashSet<Vector2Int> PlacementTileMatchWorkspace = new();

            public readonly List<List<PendingPlacement>> UndoSnapshots = new();
            public readonly List<List<PendingPlacement>> RedoSnapshots = new();
            public int PendingUndoBatchDepth;
            public List<PendingPlacement> PendingUndoBatchSnapshot;
            public bool PendingUndoBatchChanged;
            public bool PendingUndoBatchClearRedoHistory;

            public readonly List<PendingDemolition> PendingDemolitions = new();
            public readonly HashSet<Vector2Int> PendingDemolitionPositions = new();
            public readonly Dictionary<Vector2Int, string> PlayerPlacedBuildings = new();
            public readonly Dictionary<Vector2Int, ConstructionRotation>
                PlacedRotationByOrigin = new();
            public readonly Dictionary<
                Vector2Int,
                (string BuildingId, string FactionId)> FactionPlacedBuildings = new();

            public readonly List<PendingPlacement> ConfirmPendingSnapshot = new();
            public readonly HashSet<Vector2Int> ConfirmConfirmedPositions = new();
        }

        private readonly ConstructionSessionStore _sessionStore = new();

        public BuildingPlacementState State
        {
            get => _sessionStore.State;
            private set => _sessionStore.State = value;
        }

        public bool IsDemolishMode
        {
            get => _sessionStore.IsDemolishMode;
            private set => _sessionStore.IsDemolishMode = value;
        }

        private string _selectedBuildingId
        {
            get => _sessionStore.SelectedBuildingId;
            set => _sessionStore.SelectedBuildingId = value;
        }

        private ConstructionRotation _selectedRotation
        {
            get => _sessionStore.SelectedRotation;
            set => _sessionStore.SelectedRotation = value;
        }

        private string _activeOwnerId
        {
            get => _sessionStore.ActiveOwnerId;
            set => _sessionStore.ActiveOwnerId = value;
        }

        private string _lastActionMessage
        {
            get => _sessionStore.LastActionMessage;
            set => _sessionStore.LastActionMessage = value;
        }

        private bool _isActive
        {
            get => _sessionStore.IsActive;
            set => _sessionStore.IsActive = value;
        }

        private int _pendingPlacementsVersion
        {
            get => _sessionStore.PendingPlacementsVersion;
            set => _sessionStore.PendingPlacementsVersion = value;
        }

        private int _placementSimulationSnapshotVersion
        {
            get => _sessionStore.PlacementSimulationSnapshotVersion;
            set => _sessionStore.PlacementSimulationSnapshotVersion = value;
        }

        private int _pendingUndoBatchDepth
        {
            get => _sessionStore.PendingUndoBatchDepth;
            set => _sessionStore.PendingUndoBatchDepth = value;
        }

        private List<PendingPlacement> _pendingUndoBatchSnapshot
        {
            get => _sessionStore.PendingUndoBatchSnapshot;
            set => _sessionStore.PendingUndoBatchSnapshot = value;
        }

        private bool _pendingUndoBatchChanged
        {
            get => _sessionStore.PendingUndoBatchChanged;
            set => _sessionStore.PendingUndoBatchChanged = value;
        }

        private bool _pendingUndoBatchClearRedoHistory
        {
            get => _sessionStore.PendingUndoBatchClearRedoHistory;
            set => _sessionStore.PendingUndoBatchClearRedoHistory = value;
        }

        private List<PendingPlacement> _pendingPlacements =>
            _sessionStore.PendingPlacements;
        private HashSet<Vector2Int> _pendingPositions =>
            _sessionStore.PendingPositions;
        private Dictionary<Vector2Int, PendingPlacement> _pendingPlacementByPosition =>
            _sessionStore.PendingPlacementByPosition;
        private Dictionary<Vector2Int, ConstructionPendingPlacementStatus>
            _pendingPlacementStatuses => _sessionStore.PendingPlacementStatuses;
        private List<BuildingPlacementSimulationEntry> _placementSimulationSnapshot =>
            _sessionStore.PlacementSimulationSnapshot;
        private List<BuildingPlacementSimulationEntry>
            _placedBuildingSimulationSnapshot =>
                _sessionStore.PlacedBuildingSimulationSnapshot;
        private HashSet<Vector2Int> _placementTileMatchWorkspace =>
            _sessionStore.PlacementTileMatchWorkspace;
        private List<List<PendingPlacement>> _undoSnapshots =>
            _sessionStore.UndoSnapshots;
        private List<List<PendingPlacement>> _redoSnapshots =>
            _sessionStore.RedoSnapshots;
        private List<PendingDemolition> _pendingDemolitions =>
            _sessionStore.PendingDemolitions;
        private HashSet<Vector2Int> _pendingDemolitionPositions =>
            _sessionStore.PendingDemolitionPositions;
        private Dictionary<Vector2Int, string> _playerPlacedBuildings =>
            _sessionStore.PlayerPlacedBuildings;
        private Dictionary<Vector2Int, ConstructionRotation> _placedRotationByOrigin =>
            _sessionStore.PlacedRotationByOrigin;
        private Dictionary<Vector2Int, (string BuildingId, string FactionId)>
            _factionPlacedBuildings => _sessionStore.FactionPlacedBuildings;
        private List<PendingPlacement> _confirmPendingSnapshot =>
            _sessionStore.ConfirmPendingSnapshot;
        private HashSet<Vector2Int> _confirmConfirmedPositions =>
            _sessionStore.ConfirmConfirmedPositions;
    }
}
