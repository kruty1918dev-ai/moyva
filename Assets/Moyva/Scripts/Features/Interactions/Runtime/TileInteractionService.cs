using Kruty1918.Moyva.Interactions.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;
using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Interactions.Runtime
{
    internal sealed class TileInteractionService : ITileInteractionService, IInitializable, IDisposable
    {
        private readonly IGridService _gridService;
        private readonly IPathfinder _pathfinder;
        private readonly SignalBus _signalBus;
        
        private Vector2Int? _firstSelectedTile;

        public TileInteractionService(
            IGridService gridService, 
            IPathfinder pathfinder, 
            SignalBus signalBus)
        {
            _gridService = gridService;
            _pathfinder = pathfinder;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<TileClickedSignal>(OnTileClicked);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<TileClickedSignal>(OnTileClicked);
        }

        private void OnTileClicked(TileClickedSignal signal)
        {
            HandleTileClick(signal.Position);
        }

        public void HandleTileClick(Vector2Int position)
        {
            // Перевірка валідності тайла через Grid API
            if (!_gridService.TryGetTileData(position, out _)) return;

            if (_firstSelectedTile == null)
            {
                // ЛОГІКА 1: Окупація себе та сусідів через IPathfinder
                ExecuteOccupationWithNeighbors(position);
                _firstSelectedTile = position;
            }
            else
            {
                // ЛОГІКА 2: Побудова маршруту через IPathfinder
                ExecutePathfinding(_firstSelectedTile.Value, position);
                _firstSelectedTile = null; 
            }
        }

        private void ExecuteOccupationWithNeighbors(Vector2Int center)
        {
            // Окупуємо сам тайл
            _gridService.OccupyTile(center, "Player");

            // Окупуємо сусідів, яких нам повернув Pathfinder
            // Тепер нам не важливо, як Pathfinder їх шукає (4 чи 8 напрямків)
            foreach (var neighbor in _pathfinder.GetNeighbors(center))
            {
                _gridService.OccupyTile(neighbor, "PlayerNeighbor");
            }
            
            Debug.Log($"Occupied center {center} and its neighbors.");
        }

        private void ExecutePathfinding(Vector2Int start, Vector2Int end)
        {
            List<Vector2Int> path = _pathfinder.FindPath(start, end);

            if (path != null && path.Count > 0)
            {
                foreach (var step in path)
                {
                    _gridService.OccupyTile(step, "PathSegment");
                }
                Debug.Log($"Path found: {path.Count} tiles.");
            }
            else
            {
                Debug.LogWarning("No path found between selected tiles.");
            }
        }
    }
}