using System;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.ObjectsMap.Runtime
{
    public sealed class ChunkedObjectsMapService : IObjectsMapService, IInitializable, IDisposable
    {
        private readonly IChunkedObjectStore _store;
        private readonly SignalBus _signalBus;

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
        public bool TryGetPosition(string occupantId, out Vector2Int position) => _store.TryGetPosition(occupantId, out position);

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
            FireChanged(to, occupantId);
        }

        public void Unregister(Vector2Int position)
        {
            if (!_store.IsOccupied(position))
                return;

            _store.Unregister(position);
            FireChanged(position, null);
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
            if (!_store.TryGetPosition(signal.UnitId, out var oldPos) || oldPos == signal.NewPosition)
                return;

            if (_store.TryGetOccupant(signal.NewPosition, out string occupantId) && occupantId != signal.UnitId)
            {
                Debug.LogWarning($"[ObjectsMap] Destination {signal.NewPosition} already occupied by '{occupantId}'.");
                return;
            }

            Move(oldPos, signal.NewPosition);
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (_store.TryGetPosition(signal.UnitId, out var position))
                Unregister(position);
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
