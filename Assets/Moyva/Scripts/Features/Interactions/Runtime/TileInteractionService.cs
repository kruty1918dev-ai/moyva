using Kruty1918.Moyva.Interactions.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Visuals;
using System;
using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Interactions.Runtime
{
    internal sealed class TileInteractionService : ITileInteractionService, IInitializable
    {
        private readonly IGridService _gridService;
        private Vector2Int? _firstSelectedTile;
        private readonly SignalBus _signalBus;

        public TileInteractionService(IGridService gridService, SignalBus signalBus)
        {
            _gridService = gridService;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            Debug.Log("TileInteractionService initialized");
            _signalBus.Subscribe<TileClickedSignal>(OnTileClicked);
        }

        private void OnTileClicked(TileClickedSignal signal)
        {
            HandleTileClick(signal.Position);
        }

        public void HandleTileClick(Vector2Int position)
        {
            Debug.Log($"Handling tile click at: {position}");
            if (!_gridService.TryGetTileData(position, out var tileData)) return;

            if (_firstSelectedTile == null)
            {
                // Логіка 1: Окупація тайла та сусідів
                OccupyTileAndNeighbors(position);
                _firstSelectedTile = position;
            }
            else
            {
                // Логіка 2: Прокладання маршруту A* (заглушка)
                Debug.Log($"Pathfinding: From {_firstSelectedTile.Value} to {position}");
                _firstSelectedTile = null; // Скидаємо вибір
            }
        }

        private void OccupyTileAndNeighbors(Vector2Int center)
        {
            // Окупуємо центр, якщо він у межах гріду
            if (_gridService.TryGetTileData(center, out _))
                _gridService.OccupyTile(center, "Player");

            // Окупуємо сусідів (простий перебір 3х3 навколо)
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var neighborPos = new Vector2Int(center.x + x, center.y + y);
                    if (_gridService.TryGetTileData(neighborPos, out _))
                        _gridService.OccupyTile(neighborPos, "PlayerNeighbor");
                }
            }
        }
    }
}