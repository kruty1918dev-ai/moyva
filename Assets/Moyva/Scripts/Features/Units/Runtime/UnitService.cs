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
        private readonly UnitRegistrySO _registry;

        // Словники для легких даних
        private readonly Dictionary<string, float> _unitStamina = new();
        private readonly Dictionary<string, Vector2Int> _unitPositions = new();
        private readonly Dictionary<string, string> _unitTypeMapping = new();

        public UnitService(SignalBus signalBus, IGridService gridService,
            ITileSettingsService tileSettings, UnitRegistrySO registry)
        {
            _signalBus = signalBus;
            _gridService = gridService;
            _tileSettings = tileSettings;
            _registry = registry;
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
            var config = _registry.Configs.Find(c => c.TypeId == signal.UnitTypeId);
            if (config == null) return;

            // Рандомізація при створенні
            float randomMod = Random.Range(config.StaminaRandomRange.x, config.StaminaRandomRange.y);
            float startStamina = config.BaseStamina * randomMod;

            _unitStamina[signal.UnitId] = startStamina;
            _unitPositions[signal.UnitId] = signal.Position;
            _unitTypeMapping[signal.UnitId] = signal.UnitTypeId;

            // ОКУПАЦІЯ: Повідомляємо GridService
            // Використовуємо UnitId як ідентифікатор окупанта
            _gridService.OccupyTile(signal.Position, signal.UnitId);

            Debug.Log($"Unit {signal.UnitId} registered with {startStamina} stamina");
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (_unitStamina.ContainsKey(signal.UnitId))
            {
                _unitStamina[signal.UnitId] -= signal.Cost;
                _unitPositions[signal.UnitId] = signal.NewPosition;
            }
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            _unitStamina.Remove(signal.UnitId);
            _unitPositions.Remove(signal.UnitId);
            _unitTypeMapping.Remove(signal.UnitId);
        }

        public void ProcessTurnUpdate()
        {
            foreach (var unitId in new List<string>(_unitStamina.Keys))
            {
                UpdateUnitStamina(unitId);
            }
        }

        private void UpdateUnitStamina(string unitId)
        {
            var pos = _unitPositions[unitId];
            var typeId = _unitTypeMapping[unitId];
            var config = _registry.Configs.Find(c => c.TypeId == typeId);

            _gridService.TryGetTileData(pos, out var tileData);

            // Логіка "Зон комфорту" та погоди (спрощено)
            float regenModifier = 1.0f;

            // Приклад: якщо в будівлі — баф, якщо на болоті — дебаф
            if (tileData.TileTypeId == "swamp") regenModifier = -0.5f;
            if (tileData.TileTypeId == "castle") regenModifier = 2.0f;

            float regenAmount = config.StaminaRegenBase * regenModifier;
            _unitStamina[unitId] += regenAmount;

            // Якщо стаміна < 0, тут можна кидати сигнал на втрату HP
            if (_unitStamina[unitId] < 0)
            {
                Debug.LogWarning($"Unit {unitId} is starving! Stamina: {_unitStamina[unitId]}");
            }
        }

        public float GetStamina(string unitId) => _unitStamina.GetValueOrDefault(unitId, 0);
        public bool TryGetUnitPosition(string unitId, out Vector2Int position) => _unitPositions.TryGetValue(unitId, out position);
    }
}