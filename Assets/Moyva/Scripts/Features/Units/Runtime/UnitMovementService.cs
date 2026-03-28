// Features/Units/Runtime/UnitMovementService.cs
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitMovementService : IUnitMovementService
    {
        private readonly IUnitService _unitService;
        private readonly IPathfinder _pathfinder;
        private readonly IMovementAnimationService _animationService;
        private readonly ITileSettingsService _tileSettings;
        private readonly IGridService _gridService;
        private readonly SignalBus _signalBus;

        public UnitMovementService(
            IUnitService unitService,
            IPathfinder pathfinder,
            IMovementAnimationService animationService,
            ITileSettingsService tileSettings,
            IGridService gridService,
            SignalBus signalBus)
        {
            _unitService = unitService;
            _pathfinder = pathfinder;
            _animationService = animationService;
            _tileSettings = tileSettings;
            _gridService = gridService;
            _signalBus = signalBus;
        }

        // Features/Units/Runtime/UnitMovementService.cs

        public async Task MoveUnitAsync(string unitId, Vector2Int targetPosition, CancellationToken token = default)
        {
            // ПЕРЕВІРКА: чи не порожній ID
            if (string.IsNullOrEmpty(unitId))
            {
                Debug.LogError("[UnitMovement] UnitId is null or empty!");
                return;
            }

            if (!_unitService.TryGetUnitPosition(unitId, out var startPosition)) return;

            var path = _pathfinder.FindPath(startPosition, targetPosition);

            // ПЕРЕВІРКА: чи знайдено шлях
            if (path == null || path.Count <= 1)
            {
                Debug.LogWarning($"[UnitMovement] Path not found for unit {unitId}");
                return;
            }

            // ПЕРЕВІРКА: чи існує View
            var unitObj = _unitService.GetUnitObject(unitId);
            if (unitObj == null)
            {
                Debug.LogError($"[UnitMovement] GameObject for unit {unitId} is missing in UnitService!");
                return;
            }

            var settings = PathAnimationSettings.Default;
            settings.OnStepCompleted = (stepPos) => OnStepCompleted(unitId, stepPos);

            await _animationService.MoveAlongPathAsync(unitObj.transform, path, settings, token);
        }
        private void OnStepCompleted(string unitId, Vector2Int stepPos)
        {
            // Отримуємо ціну тайла, на який стали
            _gridService.TryGetTileData(stepPos, out var tileData);
            float stepCost = _tileSettings.GetTileWeight(tileData.TileTypeId);

            // Відправляємо сигнал, щоб UnitService зняв стаміну та оновив окупацію на сітці
            _signalBus.Fire(new UnitMovedSignal
            {
                UnitId = unitId,
                NewPosition = stepPos,
                Cost = stepCost
            });
        }
    }
}