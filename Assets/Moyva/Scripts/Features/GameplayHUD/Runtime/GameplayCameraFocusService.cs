using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Gameplay-side focus entry points. Routes focus requests through the
    /// interruptible ICameraFocusService and tracks the last world selection
    /// so FocusSelected (default key: F) can frame it.
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

        private bool _hasSelection;
        private WorldInfoSelectionKind _selectionKind;
        private Vector2Int _selectionPosition;

        public GameplayCameraFocusService(
            ICameraMovement camera,
            IGridProjection grid,
            SignalBus signals,
            [InjectOptional] ICameraFocusService focusService = null)
        {
            _camera = camera;
            _grid = grid;
            _signals = signals;
            _focusService = focusService;
        }

        public void Initialize()
        {
            _signals.Subscribe<WorldInfoSelectionChangedSignal>(OnWorldSelectionChanged);
            _signals.Subscribe<LocalUnitSelectionChangedSignal>(OnUnitSelectionChanged);
        }

        public void Dispose()
        {
            _signals.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnWorldSelectionChanged);
            _signals.TryUnsubscribe<LocalUnitSelectionChangedSignal>(OnUnitSelectionChanged);
        }

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
