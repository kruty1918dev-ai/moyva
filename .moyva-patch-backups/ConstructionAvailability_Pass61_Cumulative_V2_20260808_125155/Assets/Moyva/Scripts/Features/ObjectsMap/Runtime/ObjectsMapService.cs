using System.Collections.Generic;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.ObjectsMap.Runtime
{
    public sealed class ObjectsMapService :
        IObjectsMapService,
        IObjectsMapSharedOccupancy,
        IInitializable,
        System.IDisposable
    {
        private readonly Dictionary<Vector2Int, string> _occupants = new();
        private readonly Dictionary<string, Vector2Int> _positions = new();
        private readonly Dictionary<Vector2Int, HashSet<string>>
            _sharedOccupants = new();
        private readonly HashSet<string> _sharedOccupantIds = new();
        private readonly SignalBus _signalBus;

        public ObjectsMapService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<OnMapObjectSpawnedSignal>(OnMapObjectSpawned);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<OnMapObjectSpawnedSignal>(OnMapObjectSpawned);
        }

        // --- Signal handlers ---

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            if (_occupants.ContainsKey(signal.Position))
            {
                Debug.LogWarning($"[ObjectsMap] Position {signal.Position} already occupied when registering unit '{signal.UnitId}'. Registration skipped.");
                return;
            }

            RegisterInternal(signal.Position, signal.UnitId);
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (!_positions.TryGetValue(
                    signal.UnitId,
                    out Vector2Int oldPos))
            {
                Debug.LogWarning(
                    $"[ObjectsMap] OnUnitMoved: unit '{signal.UnitId}' " +
                    "not found in map.");
                return;
            }

            if (oldPos == signal.NewPosition)
                return;

            bool targetHasPrimary =
                _occupants.TryGetValue(
                    signal.NewPosition,
                    out string targetOccupantId)
                && targetOccupantId != signal.UnitId;

            if (targetHasPrimary)
            {
                if (!signal.AllowSharedOccupancy)
                {
                    Debug.LogWarning(
                        $"[ObjectsMap] OnUnitMoved: destination " +
                        $"{signal.NewPosition} is already occupied by " +
                        $"'{targetOccupantId}'. Movement skipped for " +
                        $"'{signal.UnitId}'.");
                    return;
                }

                MoveUnitToSharedCell(
                    signal.UnitId,
                    oldPos,
                    signal.NewPosition);
                return;
            }

            MoveUnitToPrimaryCell(
                signal.UnitId,
                oldPos,
                signal.NewPosition);
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            TryUnregisterOccupant(signal.UnitId);
        }

        private void OnMapObjectSpawned(OnMapObjectSpawnedSignal signal)
        {
            if (_occupants.ContainsKey(signal.Position))
            {
                // Статичний обʼєкт не може перекрити вже зайняту позицію
                Debug.LogWarning($"[ObjectsMap] OnMapObjectSpawned: position {signal.Position} already occupied. Skipping '{signal.ObjectId}'.");
                return;
            }
            RegisterInternal(signal.Position, signal.ObjectId);
        }

        // --- Public API ---

        public bool IsOccupied(Vector2Int position) => _occupants.ContainsKey(position);

        public bool TryGetOccupant(Vector2Int position, out string occupantId)
            => _occupants.TryGetValue(position, out occupantId);

        public void Register(Vector2Int position, string occupantId)
        {
            if (_occupants.ContainsKey(position))
                throw new System.InvalidOperationException(
                    $"[ObjectsMap] Cannot register '{occupantId}': position {position} is already occupied by '{_occupants[position]}'.");
            RegisterInternal(position, occupantId);
        }

        public void Move(Vector2Int from, Vector2Int to)
        {
            if (!_occupants.TryGetValue(from, out var id))
                throw new System.InvalidOperationException($"[ObjectsMap] Cannot move: position {from} is empty.");
            if (_occupants.ContainsKey(to))
                throw new System.InvalidOperationException(
                    $"[ObjectsMap] Cannot move '{id}' to {to}: position is already occupied by '{_occupants[to]}'.");
            MoveInternal(from, to, id);
        }

        public void Unregister(Vector2Int position)
        {
            if (!_occupants.ContainsKey(position)) return;
            UnregisterInternal(position);
        }

        public bool TryGetPosition(string occupantId, out Vector2Int position)
            => _positions.TryGetValue(occupantId, out position);

        public bool IsSharedOccupant(string occupantId)
            => !string.IsNullOrWhiteSpace(occupantId)
                && _sharedOccupantIds.Contains(occupantId);

        public bool TryUnregisterOccupant(string occupantId)
        {
            if (string.IsNullOrWhiteSpace(occupantId)
                || !_positions.TryGetValue(
                    occupantId,
                    out Vector2Int position))
            {
                return false;
            }

            if (_sharedOccupantIds.Contains(occupantId))
            {
                RemoveSharedOccupant(position, occupantId);
                _positions.Remove(occupantId);
                return true;
            }

            if (_occupants.TryGetValue(
                    position,
                    out string primary)
                && string.Equals(
                    primary,
                    occupantId,
                    System.StringComparison.Ordinal))
            {
                UnregisterInternal(position);
                return true;
            }

            _positions.Remove(occupantId);
            return true;
        }

        private void MoveUnitToSharedCell(
            string unitId,
            Vector2Int oldPosition,
            Vector2Int sharedPosition)
        {
            if (_sharedOccupantIds.Contains(unitId))
            {
                RemoveSharedOccupant(oldPosition, unitId);
            }
            else if (_occupants.TryGetValue(
                         oldPosition,
                         out string oldPrimary)
                     && oldPrimary == unitId)
            {
                _occupants.Remove(oldPosition);
                _signalBus.Fire(
                    new OnObjectsMapChangedSignal
                    {
                        Position = oldPosition,
                        OccupantId = null,
                    });
                PromoteSharedOccupant(oldPosition);
            }

            if (!_sharedOccupants.TryGetValue(
                    sharedPosition,
                    out HashSet<string> set))
            {
                set = new HashSet<string>(
                    System.StringComparer.Ordinal);
                _sharedOccupants[sharedPosition] = set;
            }

            set.Add(unitId);
            _sharedOccupantIds.Add(unitId);
            _positions[unitId] = sharedPosition;
        }

        private void MoveUnitToPrimaryCell(
            string unitId,
            Vector2Int oldPosition,
            Vector2Int newPosition)
        {
            if (_sharedOccupantIds.Contains(unitId))
            {
                RemoveSharedOccupant(oldPosition, unitId);
                _occupants[newPosition] = unitId;
                _positions[unitId] = newPosition;
                _signalBus.Fire(
                    new OnObjectsMapChangedSignal
                    {
                        Position = newPosition,
                        OccupantId = unitId,
                    });
                return;
            }

            MoveInternal(oldPosition, newPosition, unitId);
        }

        private void RemoveSharedOccupant(
            Vector2Int position,
            string occupantId)
        {
            _sharedOccupantIds.Remove(occupantId);
            if (!_sharedOccupants.TryGetValue(
                    position,
                    out HashSet<string> set))
            {
                return;
            }

            set.Remove(occupantId);
            if (set.Count == 0)
                _sharedOccupants.Remove(position);
        }

        private void PromoteSharedOccupant(Vector2Int position)
        {
            if (_occupants.ContainsKey(position)
                || !_sharedOccupants.TryGetValue(
                    position,
                    out HashSet<string> set)
                || set.Count == 0)
            {
                return;
            }

            string promoted = null;
            foreach (string candidate in set)
            {
                promoted = candidate;
                break;
            }

            if (string.IsNullOrWhiteSpace(promoted))
                return;

            RemoveSharedOccupant(position, promoted);
            _occupants[position] = promoted;
            _positions[promoted] = position;
            _signalBus.Fire(
                new OnObjectsMapChangedSignal
                {
                    Position = position,
                    OccupantId = promoted,
                });
        }

        // --- Private helpers ---

        private void RegisterInternal(Vector2Int position, string occupantId)
        {
            _occupants[position] = occupantId;
            _positions[occupantId] = position;
            _signalBus.Fire(new OnObjectsMapChangedSignal { Position = position, OccupantId = occupantId });
        }

        private void MoveInternal(Vector2Int from, Vector2Int to, string occupantId)
        {
            // Спочатку оновлюємо обидва словники, потім надсилаємо сигнали
            _occupants.Remove(from);
            _positions.Remove(occupantId);

            _occupants[to] = occupantId;
            _positions[occupantId] = to;

            _signalBus.Fire(new OnObjectsMapChangedSignal { Position = from, OccupantId = null });
            PromoteSharedOccupant(from);
            _signalBus.Fire(new OnObjectsMapChangedSignal { Position = to, OccupantId = occupantId });
        }

        private void UnregisterInternal(Vector2Int position)
        {
            if (_occupants.TryGetValue(position, out var id))
                _positions.Remove(id);

            _occupants.Remove(position);
            _signalBus.Fire(
                new OnObjectsMapChangedSignal
                {
                    Position = position,
                    OccupantId = null,
                });

            PromoteSharedOccupant(position);
        }
    }
}
