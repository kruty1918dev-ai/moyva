using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Last-known entity intel per fog owner.
    ///
    /// The store tracks authoritative entity state from gameplay signals and
    /// per-owner fog state to decide what each owner legitimately remembers:
    ///
    /// - an entity inside an owner's vision is recorded fresh;
    /// - an entity leaving vision leaves the record stale at its last
    ///   observed state;
    /// - re-observing a record's cell reconciles it (entity still there →
    ///   refreshed; entity gone → record removed);
    /// - hidden movement, death, demolition and ownership transfer never
    ///   update the record.
    ///
    /// Intel records are memory, not gameplay entities — nothing in this store
    /// mutates world state.
    /// </summary>
    internal sealed class FogIntelStore
        : IFogIntelReader
        , IFogIntelSnapshotStore
        , IFogIntelReplicationSink
        , IInitializable
        , IDisposable
    {
        private sealed class UnitTruth
        {
            public string TypeId;
            public string OwnerId;
            public Vector2Int Position;
            public bool Garrisoned;
        }

        private sealed class BuildingTruth
        {
            public string BuildingId;
            public string OwnerId;
            public int RotationQuarterTurns;
        }

        private sealed class OwnerIntel
        {
            public readonly Dictionary<string, FogIntelUnitRecord> Units =
                new Dictionary<string, FogIntelUnitRecord>(StringComparer.Ordinal);
            public readonly Dictionary<Vector2Int, FogIntelBuildingRecord> Buildings =
                new Dictionary<Vector2Int, FogIntelBuildingRecord>();
            public readonly Dictionary<Vector2Int, HashSet<string>> UnitsByCell =
                new Dictionary<Vector2Int, HashSet<string>>();
        }

        private readonly SignalBus _signalBus;
        private readonly IFogOwnerStateReader _fog;
        private readonly IFogOwnerExplorationSnapshotStore _ownerSnapshots;
        private readonly IFogOwnerVisibilityFeed _visibilityFeed;

        private readonly Dictionary<string, UnitTruth> _unitTruth =
            new Dictionary<string, UnitTruth>(StringComparer.Ordinal);
        private readonly Dictionary<Vector2Int, HashSet<string>> _unitTruthByCell =
            new Dictionary<Vector2Int, HashSet<string>>();
        private readonly Dictionary<Vector2Int, BuildingTruth> _buildingTruth =
            new Dictionary<Vector2Int, BuildingTruth>();
        private readonly Dictionary<string, OwnerIntel> _intel =
            new Dictionary<string, OwnerIntel>(StringComparer.Ordinal);

        private long _sequence;

        public event Action<string> IntelChanged;

        public FogIntelStore(
            SignalBus signalBus,
            IFogOwnerStateReader fog,
            [InjectOptional] IFogOwnerExplorationSnapshotStore ownerSnapshots = null,
            [InjectOptional] IFogOwnerVisibilityFeed visibilityFeed = null)
        {
            _signalBus = signalBus;
            _fog = fog;
            _ownerSnapshots = ownerSnapshots;
            _visibilityFeed = visibilityFeed;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<UnitGarrisonStateChangedSignal>(OnUnitGarrisonStateChanged);
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.Subscribe<BuildingOwnershipTransferredSignal>(OnBuildingOwnershipTransferred);
            if (_visibilityFeed != null)
                _visibilityFeed.CellsBecameVisible += OnCellsBecameVisible;
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<UnitGarrisonStateChangedSignal>(OnUnitGarrisonStateChanged);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.TryUnsubscribe<BuildingOwnershipTransferredSignal>(OnBuildingOwnershipTransferred);
            if (_visibilityFeed != null)
                _visibilityFeed.CellsBecameVisible -= OnCellsBecameVisible;
        }

        // ── IFogIntelReader ──────────────────────────────────────────────────

        public IReadOnlyCollection<FogIntelUnitRecord> GetRememberedUnits(string ownerId)
        {
            ownerId = NormalizeOwner(ownerId);
            if (ownerId == null || !_intel.TryGetValue(ownerId, out var intel) || intel.Units.Count == 0)
                return Array.Empty<FogIntelUnitRecord>();

            // Records are mutable; hand out clones so callers cannot corrupt
            // the per-cell index by editing a returned record.
            var clones = new List<FogIntelUnitRecord>(intel.Units.Count);
            foreach (var record in intel.Units.Values)
                clones.Add(record.Clone());
            return clones;
        }

        public IReadOnlyCollection<FogIntelBuildingRecord> GetRememberedBuildings(string ownerId)
        {
            ownerId = NormalizeOwner(ownerId);
            if (ownerId == null || !_intel.TryGetValue(ownerId, out var intel) || intel.Buildings.Count == 0)
                return Array.Empty<FogIntelBuildingRecord>();

            var clones = new List<FogIntelBuildingRecord>(intel.Buildings.Count);
            foreach (var record in intel.Buildings.Values)
                clones.Add(record.Clone());
            return clones;
        }

        public bool TryGetRememberedUnit(string ownerId, string unitId, out FogIntelUnitRecord record)
        {
            ownerId = NormalizeOwner(ownerId);
            record = null;
            if (ownerId == null
                || !_intel.TryGetValue(ownerId, out var intel)
                || !intel.Units.TryGetValue(unitId ?? string.Empty, out var stored))
            {
                return false;
            }
            record = stored.Clone();
            return true;
        }

        public bool TryGetRememberedBuilding(string ownerId, Vector2Int position, out FogIntelBuildingRecord record)
        {
            ownerId = NormalizeOwner(ownerId);
            record = null;
            if (ownerId == null
                || !_intel.TryGetValue(ownerId, out var intel)
                || !intel.Buildings.TryGetValue(position, out var stored))
            {
                return false;
            }
            record = stored.Clone();
            return true;
        }

        // ── IFogIntelSnapshotStore ───────────────────────────────────────────

        public IReadOnlyCollection<string> GetIntelOwnerIds() => _intel.Keys;

        public FogIntelSnapshot CaptureSnapshot(string ownerId)
        {
            ownerId = NormalizeOwner(ownerId);
            var snapshot = new FogIntelSnapshot();
            if (ownerId == null || !_intel.TryGetValue(ownerId, out var intel))
                return snapshot;

            foreach (var record in intel.Units.Values)
                snapshot.Units.Add(record.Clone());
            foreach (var record in intel.Buildings.Values)
                snapshot.Buildings.Add(record.Clone());
            return snapshot;
        }

        public void LoadSnapshot(string ownerId, FogIntelSnapshot snapshot)
        {
            ownerId = NormalizeOwner(ownerId);
            if (ownerId == null || snapshot == null)
                return;

            OwnerIntel intel = GetOrCreateIntel(ownerId);
            bool changed = false;
            if (snapshot.Units != null)
            {
                foreach (var record in snapshot.Units)
                {
                    if (record == null || string.IsNullOrWhiteSpace(record.UnitId))
                        continue;
                    UpsertUnitRecord(intel, record.Clone());
                    changed = true;
                }
            }
            if (snapshot.Buildings != null)
            {
                foreach (var record in snapshot.Buildings)
                {
                    if (record == null)
                        continue;
                    intel.Buildings[record.Position] = record.Clone();
                    changed = true;
                }
            }

            if (changed)
                FireIntelChanged(ownerId);
        }

        // ── IFogIntelReplicationSink ─────────────────────────────────────────

        public void ApplyReplicatedUnit(string ownerId, FogIntelUnitRecord record)
        {
            ownerId = NormalizeOwner(ownerId);
            if (ownerId == null || record == null || string.IsNullOrWhiteSpace(record.UnitId))
                return;

            UpsertUnitRecord(GetOrCreateIntel(ownerId), record.Clone());
            FireIntelChanged(ownerId);
        }

        public void ApplyReplicatedBuilding(string ownerId, FogIntelBuildingRecord record)
        {
            ownerId = NormalizeOwner(ownerId);
            if (ownerId == null || record == null)
                return;

            GetOrCreateIntel(ownerId).Buildings[record.Position] = record.Clone();
            FireIntelChanged(ownerId);
        }

        public void RemoveReplicatedUnit(string ownerId, string unitId)
        {
            ownerId = NormalizeOwner(ownerId);
            if (ownerId == null || string.IsNullOrWhiteSpace(unitId))
                return;
            if (!_intel.TryGetValue(ownerId, out var intel))
                return;

            if (RemoveUnitRecord(intel, unitId))
                FireIntelChanged(ownerId);
        }

        public void RemoveReplicatedBuilding(string ownerId, Vector2Int position)
        {
            ownerId = NormalizeOwner(ownerId);
            if (ownerId == null)
                return;
            if (!_intel.TryGetValue(ownerId, out var intel))
                return;

            if (intel.Buildings.Remove(position))
                FireIntelChanged(ownerId);
        }

        // ── Signal handlers: authoritative truth ─────────────────────────────

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.UnitId))
                return;

            var truth = GetOrCreateUnitTruth(signal.UnitId);
            RemoveUnitFromCellIndex(signal.UnitId, truth.Position);
            truth.TypeId = signal.UnitTypeId ?? string.Empty;
            truth.OwnerId = signal.OwnerId ?? string.Empty;
            truth.Position = signal.Position;
            truth.Garrisoned = false;
            AddUnitToCellIndex(signal.UnitId, signal.Position);

            EvaluateUnitForAllOwners(signal.UnitId, signal.Position);
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.UnitId))
                return;

            var truth = GetOrCreateUnitTruth(signal.UnitId);
            RemoveUnitFromCellIndex(signal.UnitId, truth.Position);
            truth.Position = signal.NewPosition;
            truth.Garrisoned = false;
            if (!string.IsNullOrWhiteSpace(signal.SourceFactionId))
                truth.OwnerId = signal.SourceFactionId;
            AddUnitToCellIndex(signal.UnitId, signal.NewPosition);

            EvaluateUnitForAllOwners(signal.UnitId, signal.NewPosition);
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.UnitId)
                || !_unitTruth.TryGetValue(signal.UnitId, out var truth))
            {
                return;
            }

            Vector2Int lastPosition = truth.Position;
            bool wasGarrisoned = truth.Garrisoned;
            RemoveUnitFromCellIndex(signal.UnitId, truth.Position);
            _unitTruth.Remove(signal.UnitId);

            List<string> changedOwners = null;
            foreach (var pair in _intel)
            {
                OwnerIntel intel = pair.Value;
                if (!intel.Units.ContainsKey(signal.UnitId))
                    continue;

                // Owners watching the death location see the unit die and
                // forget it; everyone else keeps the stale memory.
                bool sawDeath = !wasGarrisoned && IsVisibleTo(pair.Key, lastPosition);
                if (sawDeath && RemoveUnitRecord(intel, signal.UnitId))
                    (changedOwners ??= new List<string>()).Add(pair.Key);
            }
            FireIntelChanged(changedOwners);
        }

        private void OnUnitGarrisonStateChanged(UnitGarrisonStateChangedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.UnitId))
                return;

            var truth = GetOrCreateUnitTruth(signal.UnitId);
            if (signal.IsGarrisoned)
            {
                Vector2Int lastPosition = truth.Position;
                RemoveUnitFromCellIndex(signal.UnitId, truth.Position);
                truth.Garrisoned = true;

                List<string> changedOwners = null;
                foreach (var pair in _intel)
                {
                    OwnerIntel intel = pair.Value;
                    if (!intel.Units.ContainsKey(signal.UnitId))
                        continue;
                    // Owners watching the garrison see the unit disappear.
                    if (IsVisibleTo(pair.Key, signal.BuildingPosition)
                        || IsVisibleTo(pair.Key, lastPosition))
                    {
                        if (RemoveUnitRecord(intel, signal.UnitId))
                            (changedOwners ??= new List<string>()).Add(pair.Key);
                    }
                }
                FireIntelChanged(changedOwners);
                return;
            }

            truth.Garrisoned = false;
            truth.Position = signal.UnitPosition;
            if (!string.IsNullOrWhiteSpace(signal.OwnerId))
                truth.OwnerId = signal.OwnerId;
            AddUnitToCellIndex(signal.UnitId, signal.UnitPosition);
            EvaluateUnitForAllOwners(signal.UnitId, signal.UnitPosition);
        }

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            var truth = new BuildingTruth
            {
                BuildingId = signal.BuildingId ?? string.Empty,
                OwnerId = signal.OwnerId ?? string.Empty,
                RotationQuarterTurns = signal.RotationQuarterTurns,
            };
            if (signal.HasRelocationSource
                && signal.RelocationSourcePosition != signal.Position)
            {
                _buildingTruth.Remove(signal.RelocationSourcePosition);
            }
            _buildingTruth[signal.Position] = truth;

            EvaluateBuildingForAllOwners(signal.Position);
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
        {
            _buildingTruth.Remove(signal.Position);

            List<string> changedOwners = null;
            foreach (var pair in _intel)
            {
                OwnerIntel intel = pair.Value;
                if (!intel.Buildings.ContainsKey(signal.Position))
                    continue;
                // Owners watching the demolition forget the building;
                // everyone else keeps the stale memory.
                if (IsVisibleTo(pair.Key, signal.Position)
                    && intel.Buildings.Remove(signal.Position))
                {
                    (changedOwners ??= new List<string>()).Add(pair.Key);
                }
            }
            FireIntelChanged(changedOwners);
        }

        private void OnBuildingOwnershipTransferred(BuildingOwnershipTransferredSignal signal)
        {
            if (_buildingTruth.TryGetValue(signal.Position, out var truth))
                truth.OwnerId = signal.NewOwnerId ?? string.Empty;

            List<string> changedOwners = null;
            foreach (var pair in _intel)
            {
                OwnerIntel intel = pair.Value;
                if (!intel.Buildings.TryGetValue(signal.Position, out var record))
                    continue;
                // Only observers of the transfer learn the new owner.
                if (IsVisibleTo(pair.Key, signal.Position))
                {
                    record.OwnerId = signal.NewOwnerId ?? string.Empty;
                    record.LastSeenSequence = ++_sequence;
                    (changedOwners ??= new List<string>()).Add(pair.Key);
                }
            }
            FireIntelChanged(changedOwners);
        }

        // ── Reveal reconciliation ────────────────────────────────────────────

        private void OnCellsBecameVisible(string ownerId, IReadOnlyCollection<Vector2Int> cells)
        {
            ownerId = NormalizeOwner(ownerId);
            if (ownerId == null || cells == null || cells.Count == 0)
                return;

            OwnerIntel intel = GetOrCreateIntel(ownerId);
            bool changed = false;

            foreach (var cell in cells)
            {
                // Discovery: entities standing on a newly visible cell get
                // recorded fresh.
                if (_unitTruthByCell.TryGetValue(cell, out var unitIds))
                {
                    foreach (var unitId in unitIds)
                    {
                        if (!_unitTruth.TryGetValue(unitId, out var truth) || truth.Garrisoned)
                            continue;
                        if (IsOwnEntity(ownerId, truth.OwnerId))
                            continue;
                        changed |= UpsertUnitRecord(intel, NewUnitRecord(unitId, truth));
                    }
                }

                if (_buildingTruth.TryGetValue(cell, out var building)
                    && !IsOwnEntity(ownerId, building.OwnerId))
                {
                    intel.Buildings[cell] = NewBuildingRecord(cell, building);
                    changed = true;
                }

                // Reconciliation: remembered entities whose cell turned out to
                // be empty are forgotten. Snapshot first — upserts mutate the
                // per-cell index while we iterate it.
                if (intel.UnitsByCell.TryGetValue(cell, out var recorded))
                {
                    var stale = new List<string>();
                    var refreshed = new List<FogIntelUnitRecord>();
                    foreach (var unitId in new List<string>(recorded))
                    {
                        if (_unitTruth.TryGetValue(unitId, out var truth)
                            && !truth.Garrisoned
                            && truth.Position == cell)
                        {
                            refreshed.Add(NewUnitRecord(unitId, truth));
                        }
                        else
                        {
                            stale.Add(unitId);
                        }
                    }
                    foreach (var unitId in stale)
                        changed |= RemoveUnitRecord(intel, unitId);
                    foreach (var record in refreshed)
                        changed |= UpsertUnitRecord(intel, record);
                }

                if (intel.Buildings.TryGetValue(cell, out var remembered)
                    && !_buildingTruth.ContainsKey(cell))
                {
                    intel.Buildings.Remove(cell);
                    changed = true;
                }
            }

            if (changed)
                FireIntelChanged(ownerId);
        }

        // ── Evaluation ───────────────────────────────────────────────────────

        private void EvaluateUnitForAllOwners(string unitId, Vector2Int position)
        {
            if (!_unitTruth.TryGetValue(unitId, out var truth))
                return;

            foreach (string ownerId in EnumerateKnownOwners())
            {
                if (IsOwnEntity(ownerId, truth.OwnerId))
                    continue;
                if (!IsVisibleTo(ownerId, position))
                    continue;

                OwnerIntel intel = GetOrCreateIntel(ownerId);
                if (UpsertUnitRecord(intel, NewUnitRecord(unitId, truth)))
                    FireIntelChanged(ownerId);
            }
        }

        private void EvaluateBuildingForAllOwners(Vector2Int position)
        {
            if (!_buildingTruth.TryGetValue(position, out var truth))
                return;

            foreach (string ownerId in EnumerateKnownOwners())
            {
                if (IsOwnEntity(ownerId, truth.OwnerId))
                    continue;
                if (!IsVisibleTo(ownerId, position))
                    continue;

                OwnerIntel intel = GetOrCreateIntel(ownerId);
                intel.Buildings[position] = NewBuildingRecord(position, truth);
                FireIntelChanged(ownerId);
            }
        }

        private IEnumerable<string> EnumerateKnownOwners()
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (_ownerSnapshots != null)
            {
                foreach (string ownerId in _ownerSnapshots.GetKnownFogOwnerIds())
                {
                    string normalized = NormalizeOwner(ownerId);
                    if (normalized != null)
                        seen.Add(normalized);
                }
            }
            foreach (string ownerId in _intel.Keys)
                seen.Add(ownerId);
            return seen;
        }

        // ── Record helpers ───────────────────────────────────────────────────

        private FogIntelUnitRecord NewUnitRecord(string unitId, UnitTruth truth)
            => new FogIntelUnitRecord
            {
                UnitId = unitId,
                TypeId = truth.TypeId,
                OwnerId = truth.OwnerId,
                LastKnownPosition = truth.Position,
                LastSeenSequence = ++_sequence,
            };

        private FogIntelBuildingRecord NewBuildingRecord(Vector2Int position, BuildingTruth truth)
            => new FogIntelBuildingRecord
            {
                BuildingId = truth.BuildingId,
                OwnerId = truth.OwnerId,
                Position = position,
                RotationQuarterTurns = truth.RotationQuarterTurns,
                LastSeenSequence = ++_sequence,
            };

        private bool UpsertUnitRecord(OwnerIntel intel, FogIntelUnitRecord record)
        {
            Vector2Int previousCell = default;
            bool hadPrevious = intel.Units.TryGetValue(record.UnitId, out var existing);
            if (hadPrevious)
                previousCell = existing.LastKnownPosition;

            intel.Units[record.UnitId] = record;
            if (!intel.UnitsByCell.TryGetValue(record.LastKnownPosition, out var cellSet))
            {
                cellSet = new HashSet<string>(StringComparer.Ordinal);
                intel.UnitsByCell[record.LastKnownPosition] = cellSet;
            }
            cellSet.Add(record.UnitId);

            if (hadPrevious
                && previousCell != record.LastKnownPosition
                && intel.UnitsByCell.TryGetValue(previousCell, out var previousSet))
            {
                previousSet.Remove(record.UnitId);
                if (previousSet.Count == 0)
                    intel.UnitsByCell.Remove(previousCell);
            }
            return true;
        }

        private bool RemoveUnitRecord(OwnerIntel intel, string unitId)
        {
            if (!intel.Units.TryGetValue(unitId, out var record))
                return false;
            intel.Units.Remove(unitId);
            if (intel.UnitsByCell.TryGetValue(record.LastKnownPosition, out var cellSet))
            {
                cellSet.Remove(unitId);
                if (cellSet.Count == 0)
                    intel.UnitsByCell.Remove(record.LastKnownPosition);
            }
            return true;
        }

        private OwnerIntel GetOrCreateIntel(string ownerId)
        {
            if (!_intel.TryGetValue(ownerId, out var intel))
            {
                intel = new OwnerIntel();
                _intel.Add(ownerId, intel);
            }
            return intel;
        }

        private UnitTruth GetOrCreateUnitTruth(string unitId)
        {
            if (!_unitTruth.TryGetValue(unitId, out var truth))
            {
                truth = new UnitTruth();
                _unitTruth.Add(unitId, truth);
            }
            return truth;
        }

        private void AddUnitToCellIndex(string unitId, Vector2Int cell)
        {
            if (!_unitTruthByCell.TryGetValue(cell, out var set))
            {
                set = new HashSet<string>(StringComparer.Ordinal);
                _unitTruthByCell[cell] = set;
            }
            set.Add(unitId);
        }

        private void RemoveUnitFromCellIndex(string unitId, Vector2Int cell)
        {
            if (!_unitTruthByCell.TryGetValue(cell, out var set))
                return;
            set.Remove(unitId);
            if (set.Count == 0)
                _unitTruthByCell.Remove(cell);
        }

        private bool IsVisibleTo(string ownerId, Vector2Int position)
            => _fog != null && _fog.IsVisible(ownerId, position);

        private static bool IsOwnEntity(string ownerId, string entityOwnerId)
            => !string.IsNullOrWhiteSpace(entityOwnerId)
               && string.Equals(
                   ownerId,
                   entityOwnerId.Trim(),
                   StringComparison.Ordinal);

        private static string NormalizeOwner(string ownerId)
            => string.IsNullOrWhiteSpace(ownerId) ? null : ownerId.Trim();

        private void FireIntelChanged(string ownerId)
            => IntelChanged?.Invoke(ownerId);

        private void FireIntelChanged(List<string> ownerIds)
        {
            if (ownerIds == null)
                return;
            foreach (string ownerId in ownerIds)
                IntelChanged?.Invoke(ownerId);
        }
    }
}
