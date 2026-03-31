using System.Collections.Generic;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitService : IUnitService, IInitializable, System.IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IGridService _gridService;
        private readonly ITileSettingsService _tileSettings;
        private readonly IUnitClassConfig _unitClassConfig;

        private readonly Dictionary<string, float> _unitStamina = new();
        private readonly Dictionary<string, Vector2Int> _unitPositions = new();
        private readonly Dictionary<string, string> _unitTypeMapping = new();

        // Словник для зберігання посилань на GameObject юнітів
        private readonly Dictionary<string, GameObject> _unitObjects = new();

        public UnitService(SignalBus signalBus, IGridService gridService,
            ITileSettingsService tileSettings, IUnitClassConfig unitClassConfig)
        {
            _signalBus = signalBus;
            _gridService = gridService;
            _tileSettings = tileSettings;
            _unitClassConfig = unitClassConfig;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Unsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Unsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            var config = _unitClassConfig.GetConfig(signal.UnitTypeId);
            if (config == null) return;

            float randomMod = Random.Range(config.StaminaRandomRange.x, config.StaminaRandomRange.y);
            float startStamina = config.BaseStamina + randomMod;

            _unitStamina[signal.UnitId] = startStamina;
            _unitPositions[signal.UnitId] = signal.Position;
            _unitTypeMapping[signal.UnitId] = signal.UnitTypeId;

            // Зберігаємо GameObject
            _unitObjects[signal.UnitId] = signal.UnitObject;

            Debug.Log($"[UnitService] Unit {signal.UnitId} registered and cached. Stamina: {startStamina}, Position: {signal.Position}  ");
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (_unitStamina.ContainsKey(signal.UnitId))
            {
                if (!CanUnitMove(signal.UnitId, signal.NewPosition))
                {
                    // Посилаємо команду на зупинку
                    _signalBus.Fire(new InterruptMovementSignal
                    {
                        UnitId = signal.UnitId,
                    });
                    Debug.Log($"[UnitService] Unit {signal.UnitId} cannot move to {signal.NewPosition} due to insufficient stamina.");
                    return;
                }

                // Логіка окупації делегована ObjectsMapService — він слухає UnitMovedSignal напряму
                _unitStamina[signal.UnitId] -= signal.Cost;
                _unitPositions[signal.UnitId] = signal.NewPosition;

                Debug.Log($"[UnitService] Unit {signal.UnitId} moved to {signal.NewPosition}. Stamina now: {_unitStamina[signal.UnitId]}");
            }
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            _unitStamina.Remove(signal.UnitId);
            _unitPositions.Remove(signal.UnitId);
            _unitTypeMapping.Remove(signal.UnitId);
            _unitObjects.Remove(signal.UnitId); // Видаляємо посилання
        }

        // --- API методи ---

        public GameObject GetUnitObject(string unitId)
        {
            if (_unitObjects.TryGetValue(unitId, out var unitObj))
            {
                return unitObj;
            }
            return null; // --- IGNORE ---
        }

        public float GetStamina(string unitId) => _unitStamina.GetValueOrDefault(unitId, 0);

        public bool TryGetUnitPosition(string unitId, out Vector2Int position)
            => _unitPositions.TryGetValue(unitId, out position);

        private bool CanUnitMove(string unitId, Vector2Int targetPosition)
        {
            if (!_unitStamina.ContainsKey(unitId) || !_unitPositions.ContainsKey(unitId))
                return false;

            var currentPos = _unitPositions[unitId];
            var tileCost = _tileSettings.GetTileWeight(_gridService.GetTileData(targetPosition).TileTypeId);
            return _unitStamina[unitId] >= tileCost;
        }
    }
}