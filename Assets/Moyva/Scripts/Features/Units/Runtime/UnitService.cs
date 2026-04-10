using System.Collections.Generic;
using System;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
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
        private readonly IObjectsMapService _objectsMapService;
        private readonly ICalendarService _calendarService;

        private readonly Dictionary<string, float> _unitStamina = new();
        private readonly Dictionary<string, Vector2Int> _unitPositions = new();
        private readonly Dictionary<string, string> _unitTypeIds = new();

        // Словник для зберігання посилань на GameObject юнітів
        private readonly Dictionary<string, GameObject> _unitObjects = new();

        public UnitService(
            SignalBus signalBus,
            IGridService gridService,
            ITileSettingsService tileSettings,
            IUnitClassConfig unitClassConfig,
            IObjectsMapService objectsMapService,
            ICalendarService calendarService = null)
        {
            _signalBus = signalBus;
            _gridService = gridService;
            _tileSettings = tileSettings;
            _unitClassConfig = unitClassConfig;
            _objectsMapService = objectsMapService;
            _calendarService = calendarService;
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
            if (config == null)
            {
                Debug.LogError($"[UnitService] OnUnitCreated: конфігурація для typeId='{signal.UnitTypeId}' (unitId='{signal.UnitId}') НЕ ЗНАЙДЕНА! Юніт НЕ буде зареєстрований.");
                return;
            }

            float randomMod = UnityEngine.Random.Range(config.StaminaRandomRange.x, config.StaminaRandomRange.y);
            float startStamina = config.BaseStamina + randomMod;

            _unitStamina[signal.UnitId] = startStamina;
            _unitPositions[signal.UnitId] = signal.Position;
            _unitTypeIds[signal.UnitId] = signal.UnitTypeId;

            // Зберігаємо GameObject
            _unitObjects[signal.UnitId] = signal.UnitObject;

            Debug.Log($"[UnitService] Unit {signal.UnitId} registered and cached. Stamina: {startStamina}, Position: {signal.Position}  ");
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (!_unitStamina.ContainsKey(signal.UnitId))
            {
                Debug.LogWarning($"[UnitService] OnUnitMoved: юніт '{signal.UnitId}' не зареєстрований у _unitStamina. Сигнал ігнорується.");
                return;
            }

            float staminaBefore = _unitStamina[signal.UnitId];

            if (!CanUnitMove(signal.UnitId, signal.NewPosition))
            {
                _signalBus.Fire(new InterruptMovementSignal
                {
                    UnitId = signal.UnitId,
                });
                Debug.LogWarning($"[UnitService] Unit {signal.UnitId} не може рухатись до {signal.NewPosition}. Стаміна={staminaBefore}, вартість={signal.Cost}. Надіслано InterruptMovementSignal.");
                return;
            }

            _unitStamina[signal.UnitId] -= signal.Cost;
            _unitPositions[signal.UnitId] = signal.NewPosition;

            Debug.Log($"[UnitService] Unit {signal.UnitId} рух до {signal.NewPosition}. Стаміна: {staminaBefore} → {_unitStamina[signal.UnitId]} (витрачено {signal.Cost})");

            if (_calendarService != null)
            {
                try
                {
                    _calendarService.AdvanceTurn();
                }
                catch (InvalidOperationException)
                {
                    // ClientCalendarProxy is read-only and advances only via snapshots.
                }

                var now = _calendarService.Current;
                Debug.Log($"[Time] Після кроку {signal.UnitId}: {now.Year:D4}-{now.Month:D2}-{now.Day:D2} {now.Hour:D2}:00, фаза={_calendarService.CurrentDayPhase}, totalHours={_calendarService.TotalHoursSinceEpoch}");
            }
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            _unitStamina.Remove(signal.UnitId);
            _unitPositions.Remove(signal.UnitId);
            _unitTypeIds.Remove(signal.UnitId);
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

        public void SetStamina(string unitId, float stamina)
        {
            if (string.IsNullOrEmpty(unitId) || !_unitStamina.ContainsKey(unitId))
                return;

            _unitStamina[unitId] = Mathf.Max(0f, stamina);
        }

        public bool TryGetUnitPosition(string unitId, out Vector2Int position)
            => _unitPositions.TryGetValue(unitId, out position);

        public IReadOnlyCollection<string> GetAllUnitIds()
            => _unitPositions.Keys;

        public string GetUnitTypeId(string unitId)
            => _unitTypeIds.TryGetValue(unitId, out var typeId) ? typeId : null;

        private bool CanUnitMove(string unitId, Vector2Int targetPosition)
        {
            if (!_unitStamina.ContainsKey(unitId))
                return false;

            var tileCost = _tileSettings.GetTileWeight(_gridService.GetTileData(targetPosition));
            return _unitStamina[unitId] >= tileCost;
        }
    }
}