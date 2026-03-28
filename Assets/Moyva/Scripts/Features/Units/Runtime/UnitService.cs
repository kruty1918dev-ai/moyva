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

        private readonly Dictionary<string, float> _unitStamina = new();
        private readonly Dictionary<string, Vector2Int> _unitPositions = new();
        private readonly Dictionary<string, string> _unitTypeMapping = new();
        
        // Словник для зберігання посилань на GameObject юнітів
        private readonly Dictionary<string, GameObject> _unitObjects = new();

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

            float randomMod = Random.Range(config.StaminaRandomRange.x, config.StaminaRandomRange.y);
            float startStamina = config.BaseStamina * randomMod;

            _unitStamina[signal.UnitId] = startStamina;
            _unitPositions[signal.UnitId] = signal.Position;
            _unitTypeMapping[signal.UnitId] = signal.UnitTypeId;
            
            // Зберігаємо GameObject
            _unitObjects[signal.UnitId] = signal.UnitObject;

            _gridService.OccupyTile(signal.Position, signal.UnitId);

            Debug.Log($"[UnitService] Unit {signal.UnitId} registered and cached.");
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (_unitStamina.ContainsKey(signal.UnitId))
            {
                // Логіка окупації: звільняємо старий, займаємо новий
                if (_unitPositions.TryGetValue(signal.UnitId, out var oldPos))
                {
                    _gridService.VacateTile(oldPos);
                }

                _unitStamina[signal.UnitId] -= signal.Cost;
                _unitPositions[signal.UnitId] = signal.NewPosition;
                
                _gridService.OccupyTile(signal.NewPosition, signal.UnitId);
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

        // Реалізація з вашого інтерфейсу (якщо IUnitService вимагає object)
        public object GetUnit(string unitId) => GetUnitObject(unitId);

        // --- Оновлення стаміни по ходах ---

        public void ProcessTurnUpdate()
        {
            foreach (var unitId in new List<string>(_unitStamina.Keys))
            {
                UpdateUnitStamina(unitId);
            }
        }

        private void UpdateUnitStamina(string unitId)
        {
            if (!_unitPositions.TryGetValue(unitId, out var pos)) return;
            
            var typeId = _unitTypeMapping[unitId];
            var config = _registry.Configs.Find(c => c.TypeId == typeId);

            _gridService.TryGetTileData(pos, out var tileData);

            float regenModifier = 1.0f;
            if (tileData.TileTypeId == "swamp") regenModifier = -0.5f;
            if (tileData.TileTypeId == "castle") regenModifier = 2.0f;

            float regenAmount = config.StaminaRegenBase * regenModifier;
            _unitStamina[unitId] += regenAmount;

            if (_unitStamina[unitId] < 0)
            {
                Debug.LogWarning($"Unit {unitId} is starving!");
            }
        }
    }
}