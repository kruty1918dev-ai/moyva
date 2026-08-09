using System;
using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.ObjectsMap.Runtime
{
    public sealed class ChunkedObjectsMapService :
        IObjectsMapService,
        IObjectsMapSharedOccupancy,
        IInitializable,
        IDisposable
    {
        private readonly IChunkedObjectStore _store;
        private readonly SignalBus _signalBus;
        private readonly Dictionary<string, Vector2Int>
            _sharedUnitPositions = new(
                StringComparer.Ordinal);
        private readonly Dictionary<Vector2Int, HashSet<string>>
            _sharedUnitsByPosition = new();

        public ChunkedObjectsMapService(IChunkedObjectStore store, SignalBus signalBus)
        {
            _store = store;
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

        public bool IsOccupied(Vector2Int position) => _store.IsOccupied(position);
        public bool TryGetOccupant(Vector2Int position, out string occupantId) => _store.TryGetOccupant(position, out occupantId);
        public bool TryGetPosition(
            string occupantId,
            out Vector2Int position)
        {
            if (!string.IsNullOrWhiteSpace(occupantId)
                && _sharedUnitPositions.TryGetValue(
                    occupantId,
                    out position))
            {
                return true;
            }

            return _store.TryGetPosition(
                occupantId,
                out position);
        }

        public void Register(Vector2Int position, string occupantId)
        {
            _store.Register(position, occupantId);
            FireChanged(position, occupantId);
        }

        public void Move(Vector2Int from, Vector2Int to)
        {
            if (!_store.TryGetOccupant(from, out string occupantId))
                throw new InvalidOperationException($"[ObjectsMap] Cannot move: position {from} is empty.");

            _store.Move(from, to);
            FireChanged(from, null);
            PromoteSharedUnit(from);
            FireChanged(to, occupantId);
        }

        public void Unregister(Vector2Int position)
        {
            if (!_store.IsOccupied(position))
                return;

            _store.Unregister(position);
            FireChanged(position, null);
            PromoteSharedUnit(position);
        }

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            if (_store.IsOccupied(signal.Position))
            {
                Debug.LogWarning($"[ObjectsMap] Position {signal.Position} already occupied for unit '{signal.UnitId}'.");
                return;
            }

            Register(signal.Position, signal.UnitId);
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (!TryGetPosition(
                    signal.UnitId,
                    out Vector2Int oldPos)
                || oldPos == signal.NewPosition)
            {
                return;
            }

            bool targetHasPrimary =
                _store.TryGetOccupant(
                    signal.NewPosition,
                    out string occupantId)
                && occupantId != signal.UnitId;

            if (targetHasPrimary)
            {
                if (!signal.AllowSharedOccupancy)
                {
                    Debug.LogWarning(
                        $"[ObjectsMap] Destination {signal.NewPosition} " +
                        $"already occupied by '{occupantId}'.");
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

        public bool IsSharedOccupant(string occupantId)
            => !string.IsNullOrWhiteSpace(occupantId)
                && _sharedUnitPositions.ContainsKey(occupantId);

        public bool TryUnregisterOccupant(string occupantId)
        {
            if (string.IsNullOrWhiteSpace(occupantId))
                return false;

            if (_sharedUnitPositions.TryGetValue(
                    occupantId,
                    out Vector2Int sharedPosition))
            {
                RemoveSharedUnit(
                    occupantId,
                    sharedPosition);
                return true;
            }

            if (_store.TryGetPosition(
                    occupantId,
                    out Vector2Int position))
            {
                Unregister(position);
                return true;
            }

            return false;
        }

        private void MoveUnitToSharedCell(
            string unitId,
            Vector2Int oldPosition,
            Vector2Int sharedPosition)
        {
            if (_sharedUnitPositions.ContainsKey(unitId))
            {
                RemoveSharedUnit(unitId, oldPosition);
            }
            else if (_store.TryGetPosition(unitId, out _))
            {
                _store.Unregister(oldPosition);
                FireChanged(oldPosition, null);
                PromoteSharedUnit(oldPosition);
            }

            if (!_sharedUnitsByPosition.TryGetValue(
                    sharedPosition,
                    out HashSet<string> set))
            {
                set = new HashSet<string>(
                    StringComparer.Ordinal);
                _sharedUnitsByPosition[sharedPosition] = set;
            }

            set.Add(unitId);
            _sharedUnitPositions[unitId] =
                sharedPosition;
        }

        private void MoveUnitToPrimaryCell(
            string unitId,
            Vector2Int oldPosition,
            Vector2Int newPosition)
        {
            if (_sharedUnitPositions.ContainsKey(unitId))
            {
                RemoveSharedUnit(unitId, oldPosition);
                _store.Register(newPosition, unitId);
                FireChanged(newPosition, unitId);
                return;
            }

            _store.Move(oldPosition, newPosition);
            FireChanged(oldPosition, null);
            PromoteSharedUnit(oldPosition);
            FireChanged(newPosition, unitId);
        }

        private void RemoveSharedUnit(
            string unitId,
            Vector2Int position)
        {
            _sharedUnitPositions.Remove(unitId);
            if (!_sharedUnitsByPosition.TryGetValue(
                    position,
                    out HashSet<string> set))
            {
                return;
            }

            set.Remove(unitId);
            if (set.Count == 0)
                _sharedUnitsByPosition.Remove(position);
        }

        private void PromoteSharedUnit(Vector2Int position)
        {
            if (_store.IsOccupied(position)
                || !_sharedUnitsByPosition.TryGetValue(
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

            RemoveSharedUnit(promoted, position);
            _store.Register(position, promoted);
            FireChanged(position, promoted);
        }

        private void OnMapObjectSpawned(OnMapObjectSpawnedSignal signal)
        {
            if (_store.IsOccupied(signal.Position))
            {
                Debug.LogWarning($"[ObjectsMap] Position {signal.Position} already occupied. Skipping '{signal.ObjectId}'.");
                return;
            }

            Register(signal.Position, signal.ObjectId);
        }

        private void FireChanged(Vector2Int position, string occupantId)
            => _signalBus.Fire(new OnObjectsMapChangedSignal { Position = position, OccupantId = occupantId });
    }
}
