using System;

using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Точки входу фокусування камери з боку геймплею. Проксує запити через
    /// переривний ICameraFocusService та відстежує останнє виділення у світі,
    /// щоб FocusSelected (клавіша F за замовчуванням) міг кадрувати його.
    /// </summary>
    internal sealed class GameplayCameraFocusService : IGameplayCameraFocusService, IInitializable, IDisposable
    {
        // Nominal focus extents per selection kind — enough to frame a unit
        // tightly and a building/settlement with breathing room.
        private const float UnitFocusExtent = 1.5f;
        private const float BuildingFocusExtent = 4f;
        private const float MapObjectFocusExtent = 3f;

        private readonly ICameraMovement _camera;
        private readonly IGridProjection _grid;
        private readonly SignalBus _signals;
        private readonly ICameraFocusService _focusService;
        private readonly ITurnService _turns;
        private readonly IConstructionPlacedBuildingQuery _constructionQuery;
        private readonly IBuildingRegistry _buildingRegistry;

        private bool _hasSelection;
        private WorldInfoSelectionKind _selectionKind;
        private Vector2Int _selectionPosition;

        /// <summary>Створює сервіс із залежностями камери, сітки та шини сигналів.</summary>
        public GameplayCameraFocusService(
            ICameraMovement camera,
            IGridProjection grid,
            SignalBus signals,
            [InjectOptional] ICameraFocusService focusService = null,
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] IConstructionPlacedBuildingQuery constructionQuery = null,
            [InjectOptional] IBuildingRegistry buildingRegistry = null)
        {
            _camera = camera;
            _grid = grid;
            _signals = signals;
            _focusService = focusService;
            _turns = turns;
            _constructionQuery = constructionQuery;
            _buildingRegistry = buildingRegistry;
        }

        /// <summary>Ініціалізує компонент і підписує на події.</summary>
        public void Initialize()
        {
            _signals.Subscribe<WorldInfoSelectionChangedSignal>(OnWorldSelectionChanged);
            _signals.Subscribe<LocalUnitSelectionChangedSignal>(OnUnitSelectionChanged);
        }

        /// <summary>Звільняє ресурси та відписує від подій.</summary>
        public void Dispose()
        {
            _signals.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnWorldSelectionChanged);
            _signals.TryUnsubscribe<LocalUnitSelectionChangedSignal>(OnUnitSelectionChanged);
        }

        /// <summary>Фокусує камеру на клітинці сітки зі збереженням поточного zoom.</summary>
        public void FocusGridPosition(Vector2Int gridPosition, string targetId = null)
        {
            Vector3 worldPoint = _grid.GridToWorld(gridPosition);
            if (_focusService != null)
                _focusService.FocusWorldPoint(worldPoint, new CameraFocusRequest { KeepCurrentZoom = true });
            else
                _camera.MoveCameraFocusToWorldPoint(worldPoint, false);

            _signals.Fire(new WorldFocusPingRequestedSignal
            {
                TargetId = targetId ?? string.Empty,
                Position = gridPosition,
                DurationSeconds = 1.2f,
            });
        }

        /// <summary>Фокусує вибраного.</summary>
        public void FocusSelected()
        {
            if (!_hasSelection)
                return;

            Vector3 worldPoint = _grid.GridToWorld(_selectionPosition);
            if (_focusService == null)
            {
                _camera.MoveCameraFocusToWorldPoint(worldPoint, false);
                return;
            }

            float extent = _selectionKind switch
            {
                WorldInfoSelectionKind.Building => BuildingFocusExtent,
                WorldInfoSelectionKind.MapObject => MapObjectFocusExtent,
                _ => UnitFocusExtent,
            };

            _focusService.FocusBounds(new Bounds(worldPoint, Vector3.one * extent * 2f));
        }

        /// <summary>Фокусує камеру на столиці локального гравця (замок → ратуша → будь-яка будівля).</summary>
        public void FocusCapital()
        {
            string ownerId = _turns?.LocalOwnerId;
            if (string.IsNullOrEmpty(ownerId) || _constructionQuery == null)
                return;

            var placed = _constructionQuery.GetPlacedBuildings(ownerId);
            if (placed == null || placed.Count == 0)
                return;

            if (TryFindCapital(placed, BuildingDefinitionCapabilities.IsCastle, out Vector2Int position)
                || TryFindCapital(placed, BuildingDefinitionCapabilities.IsTownHall, out position)
                || TryFirstPlaced(placed, out position))
            {
                FocusGridPosition(position, "capital");
            }
        }

        private bool TryFindCapital(
            System.Collections.Generic.IReadOnlyDictionary<Vector2Int, string> placed,
            Func<BuildingDefinition, bool> predicate,
            out Vector2Int position)
        {
            foreach (var pair in placed)
            {
                var definition = _buildingRegistry?.GetById(pair.Value);
                if (definition != null && predicate(definition))
                {
                    position = pair.Key;
                    return true;
                }
            }

            position = default;
            return false;
        }

        private static bool TryFirstPlaced(
            System.Collections.Generic.IReadOnlyDictionary<Vector2Int, string> placed,
            out Vector2Int position)
        {
            foreach (var pair in placed)
            {
                position = pair.Key;
                return true;
            }

            position = default;
            return false;
        }

        private void OnWorldSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            _hasSelection = signal.Kind != WorldInfoSelectionKind.None;
            _selectionKind = signal.Kind;
            _selectionPosition = signal.Position;
        }

        private void OnUnitSelectionChanged(LocalUnitSelectionChangedSignal signal)
        {
            _hasSelection = signal.IsSelected;
            if (signal.IsSelected)
            {
                _selectionKind = WorldInfoSelectionKind.Unit;
                _selectionPosition = signal.Position;
            }
        }
    }
}
