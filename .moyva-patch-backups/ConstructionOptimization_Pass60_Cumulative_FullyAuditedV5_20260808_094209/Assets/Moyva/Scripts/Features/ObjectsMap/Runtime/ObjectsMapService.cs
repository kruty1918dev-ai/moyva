using System.Collections.Generic;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.ObjectsMap.Runtime
{
    public sealed class ObjectsMapService : IObjectsMapService, IInitializable, System.IDisposable
    {
        private readonly Dictionary<Vector2Int, string> _occupants = new();
        private readonly Dictionary<string, Vector2Int> _positions = new();
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
            if (!_positions.TryGetValue(signal.UnitId, out var oldPos))
            {
                Debug.LogWarning($"[ObjectsMap] OnUnitMoved: unit '{signal.UnitId}' not found in map.");
                return;
            }

            if (oldPos == signal.NewPosition)
            {
                return;
            }

            if (_occupants.TryGetValue(signal.NewPosition, out var occupantId) && occupantId != signal.UnitId)
            {
                Debug.LogWarning($"[ObjectsMap] OnUnitMoved: destination {signal.NewPosition} is already occupied by '{occupantId}'. Movement skipped for '{signal.UnitId}'.");
                return;
            }

            MoveInternal(oldPos, signal.NewPosition, signal.UnitId);
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (_positions.TryGetValue(signal.UnitId, out var pos))
            {
                UnregisterInternal(pos);
            }
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
            _signalBus.Fire(new OnObjectsMapChangedSignal { Position = to, OccupantId = occupantId });
        }

        private void UnregisterInternal(Vector2Int position)
        {
            if (_occupants.TryGetValue(position, out var id))
                _positions.Remove(id);
            _occupants.Remove(position);
            _signalBus.Fire(new OnObjectsMapChangedSignal { Position = position, OccupantId = null });
        }
    }
}
