using System.Collections.Generic;
using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Combat.Runtime;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitService :
        IUnitService,
        IUnitOwnershipQuery,
        IConstructionUnitGarrisonRuntime,
        IConstructionUnitTraversalQuery,
        IInitializable,
        System.IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IGridService _gridService;
        private readonly ITileSettingsService _tileSettings;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IConstructionGateStateService _gateStateService;

        private readonly Dictionary<string, float> _unitStamina = new();
        private readonly Dictionary<string, Vector2Int> _unitPositions = new();
        private readonly Dictionary<string, string> _unitTypeIds = new();
        private readonly Dictionary<string, string> _unitOwnerIds = new();
        private readonly Dictionary<string, int> _unitVisionRanges = new();
        private readonly Dictionary<string, Vector2Int> _garrisonedUnitPositions = new();

        // Словник для зберігання посилань на GameObject юнітів
        private readonly Dictionary<string, GameObject> _unitObjects = new();

        // Система здоров'я юнітів
        private readonly IHealthRegistry _healthRegistry;

        public UnitService(
            SignalBus signalBus,
            IGridService gridService,
            ITileSettingsService tileSettings,
            IUnitClassConfig unitClassConfig,
            IObjectsMapService objectsMapService)
            : this(signalBus, gridService, tileSettings, unitClassConfig, objectsMapService, null, null)
        {
        }

        public UnitService(
            SignalBus signalBus,
            IGridService gridService,
            ITileSettingsService tileSettings,
            IUnitClassConfig unitClassConfig,
            IObjectsMapService objectsMapService,
            IHealthRegistry healthRegistry)
            : this(signalBus, gridService, tileSettings, unitClassConfig, objectsMapService, healthRegistry, null)
        {
        }

        [Inject]
        public UnitService(
            SignalBus signalBus,
            IGridService gridService,
            ITileSettingsService tileSettings,
            IUnitClassConfig unitClassConfig,
            IObjectsMapService objectsMapService,
            IHealthRegistry healthRegistry = null,
            [InjectOptional] IBuildingRegistry buildingRegistry = null,
            [InjectOptional] IConstructionGateStateService gateStateService = null)
        {
            _signalBus = signalBus;
            _gridService = gridService;
            _tileSettings = tileSettings;
            _unitClassConfig = unitClassConfig;
            _objectsMapService = objectsMapService;
            _healthRegistry = healthRegistry;
            _buildingRegistry = buildingRegistry;
            _gateStateService = gateStateService;
        }

        [System.Diagnostics.Conditional("MOYVA_VERBOSE_MOVEMENT")]
        private static void LogMovementVerbose(string message)
            => Debug.Log(message);

        public void Initialize()
        {
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            var config = _unitClassConfig.GetConfig(signal.UnitTypeId);
            if (config == null)
            {
                Debug.LogWarning($"[UnitService] OnUnitCreated: конфігурація для typeId='{signal.UnitTypeId}' (unitId='{signal.UnitId}') НЕ ЗНАЙДЕНА! Юніт НЕ буде зареєстрований.");
                return;
            }

            float startStamina = ResolveStartingStamina(signal.UnitTypeId, config);

            _unitStamina[signal.UnitId] = startStamina;
            _unitPositions[signal.UnitId] = signal.Position;
            _unitTypeIds[signal.UnitId] = signal.UnitTypeId;
            _unitOwnerIds[signal.UnitId] =
                string.IsNullOrWhiteSpace(signal.OwnerId)
                    ? "player_0"
                    : signal.OwnerId.Trim();
            _unitVisionRanges[signal.UnitId] =
                Mathf.Max(0, signal.VisionRange);

            // Зберігаємо GameObject
            _unitObjects[signal.UnitId] = signal.UnitObject;

            // Реєструємо health
            if (_healthRegistry != null)
            {
                int maxHp = Math.Max(1, config.HitPoints);
                var health = new HealthComponent();
                health.Initialize(signal.UnitId, maxHp);
                health.OnDestroyed += entityId =>
                    _signalBus.Fire(new UnitDestroyedSignal
                    {
                        UnitId = entityId,
                    });
                _healthRegistry.Register(health);
            }

            LogMovementVerbose(
                $"[UnitService] Unit {signal.UnitId} registered. " +
                $"Stamina={startStamina}, Position={signal.Position}");
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (!_unitStamina.ContainsKey(signal.UnitId))
            {
                Debug.LogWarning($"[UnitService] OnUnitMoved: юніт '{signal.UnitId}' не зареєстрований у _unitStamina. Сигнал ігнорується.");
                return;
            }

            float staminaBefore = _unitStamina[signal.UnitId];

            bool movementStateValid =
                !_garrisonedUnitPositions.ContainsKey(signal.UnitId)
                && signal.Cost >= 0f
                && staminaBefore >= signal.Cost;
            if (!movementStateValid)
            {
                _signalBus.Fire(new InterruptMovementSignal
                {
                    UnitId = signal.UnitId,
                });
                Debug.LogWarning(
                    $"[UnitService] Completed move signal rejected for " +
                    $"{signal.UnitId}: stamina={staminaBefore}, " +
                    $"cost={signal.Cost}, garrisoned=" +
                    $"{_garrisonedUnitPositions.ContainsKey(signal.UnitId)}.");
                return;
            }

            _unitStamina[signal.UnitId] -= signal.Cost;
            _unitPositions[signal.UnitId] = signal.NewPosition;

            LogMovementVerbose(
                $"[UnitService] Unit {signal.UnitId} -> {signal.NewPosition}. " +
                $"Stamina {staminaBefore} -> {_unitStamina[signal.UnitId]}");

        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (_unitObjects.TryGetValue(
                    signal.UnitId,
                    out GameObject unitObject)
                && unitObject != null)
            {
                UnityEngine.Object.Destroy(unitObject);
            }

            _unitStamina.Remove(signal.UnitId);
            _unitPositions.Remove(signal.UnitId);
            _unitTypeIds.Remove(signal.UnitId);
            _unitOwnerIds.Remove(signal.UnitId);
            _unitVisionRanges.Remove(signal.UnitId);
            _garrisonedUnitPositions.Remove(signal.UnitId);
            _unitObjects.Remove(signal.UnitId); // Видаляємо посилання
            _healthRegistry?.Unregister(signal.UnitId);
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

        public bool TryEnterGarrison(
            string unitId,
            Vector2Int buildingPosition,
            out string reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(unitId)
                || !_unitPositions.TryGetValue(
                    unitId,
                    out Vector2Int currentPosition))
            {
                reason = "Юніт не зареєстрований.";
                return false;
            }

            if (_garrisonedUnitPositions.ContainsKey(unitId))
            {
                reason = "Юніт уже перебуває в гарнізоні.";
                return false;
            }

            if (!_objectsMapService.IsOccupied(buildingPosition))
            {
                reason = "Будівля гарнізону не зареєстрована на карті.";
                return false;
            }

            int garrisonDistance = Mathf.Max(
                Mathf.Abs(currentPosition.x - buildingPosition.x),
                Mathf.Abs(currentPosition.y - buildingPosition.y));
            if (garrisonDistance > 1)
            {
                reason = "Юніт має стояти поруч із будівлею гарнізону.";
                return false;
            }

            if (_objectsMapService.TryGetOccupant(
                    currentPosition,
                    out string occupantId)
                && string.Equals(
                    occupantId,
                    unitId,
                    StringComparison.Ordinal))
            {
                if (_objectsMapService
                is IObjectsMapSharedOccupancy sharedOccupancy)
            {
                sharedOccupancy.TryUnregisterOccupant(unitId);
            }
            else
            {
                _objectsMapService.Unregister(currentPosition);
            }
            }

            _garrisonedUnitPositions[unitId] = buildingPosition;
            if (_unitObjects.TryGetValue(unitId, out GameObject unitObject)
                && unitObject != null)
            {
                unitObject.SetActive(false);
            }

            _signalBus.Fire(
                new UnitGarrisonStateChangedSignal
                {
                    UnitId = unitId,
                    IsGarrisoned = true,
                    BuildingPosition = buildingPosition,
                    UnitPosition = currentPosition,
                    VisionRange = _unitVisionRanges.TryGetValue(
                        unitId,
                        out int visionRange)
                        ? visionRange
                        : 0,
                    OwnerId = GetUnitOwnerId(unitId),
                });
            return true;
        }

        public bool TryRestoreGarrison(
            string unitId,
            Vector2Int buildingPosition,
            out string reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(unitId)
                || !_unitPositions.TryGetValue(
                    unitId,
                    out Vector2Int currentPosition))
            {
                reason = "Юніт не зареєстрований.";
                return false;
            }

            if (_garrisonedUnitPositions.TryGetValue(
                    unitId,
                    out Vector2Int existing))
            {
                if (existing == buildingPosition)
                    return true;

                reason = "Юніт уже перебуває в іншому гарнізоні.";
                return false;
            }

            if (!_objectsMapService.IsOccupied(buildingPosition))
            {
                reason = "Будівля гарнізону ще не відновлена на карті.";
                return false;
            }

            if (_objectsMapService
                is IObjectsMapSharedOccupancy sharedOccupancy)
            {
                sharedOccupancy.TryUnregisterOccupant(unitId);
            }
            else if (_objectsMapService.TryGetOccupant(
                         currentPosition,
                         out string occupantId)
                     && string.Equals(
                         occupantId,
                         unitId,
                         StringComparison.Ordinal))
            {
                _objectsMapService.Unregister(currentPosition);
            }

            _garrisonedUnitPositions[unitId] =
                buildingPosition;

            if (_unitObjects.TryGetValue(
                    unitId,
                    out GameObject unitObject)
                && unitObject != null)
            {
                unitObject.SetActive(false);
            }

            _signalBus.Fire(
                new UnitGarrisonStateChangedSignal
                {
                    UnitId = unitId,
                    IsGarrisoned = true,
                    BuildingPosition = buildingPosition,
                    UnitPosition = currentPosition,
                    VisionRange = _unitVisionRanges.TryGetValue(
                        unitId,
                        out int visionRange)
                        ? visionRange
                        : 0,
                    OwnerId = GetUnitOwnerId(unitId),
                });

            Debug.Log(
                $"[MoyvaConstructionModules] garrison restore-unit " +
                $"unit={unitId} building={buildingPosition}");
            return true;
        }

        public bool TryExitGarrison(
            string unitId,
            Vector2Int targetPosition,
            out string reason)
        {
            reason = null;
            if (!_garrisonedUnitPositions.ContainsKey(unitId))
            {
                reason = "Юніт не перебуває в гарнізоні.";
                return false;
            }

            if (!_gridService.TryGetTileData(
                    targetPosition,
                    out string tileTypeId)
                || string.IsNullOrWhiteSpace(tileTypeId))
            {
                reason = "Цільовий тайл не існує.";
                return false;
            }

            if (_objectsMapService.IsOccupied(targetPosition))
            {
                reason = "Цільова клітинка зайнята.";
                return false;
            }

            _objectsMapService.Register(targetPosition, unitId);
            _unitPositions[unitId] = targetPosition;
            _garrisonedUnitPositions.Remove(unitId);

            if (_unitObjects.TryGetValue(unitId, out GameObject unitObject)
                && unitObject != null)
            {
                unitObject.SetActive(true);
            }

            _signalBus.Fire(
                new UnitGarrisonStateChangedSignal
                {
                    UnitId = unitId,
                    IsGarrisoned = false,
                    UnitPosition = targetPosition,
                    VisionRange = _unitVisionRanges.TryGetValue(
                        unitId,
                        out int visionRange)
                        ? visionRange
                        : 0,
                    OwnerId = GetUnitOwnerId(unitId),
                });
            return true;
        }

        public bool TryExitGarrisonNear(
            string unitId,
            Vector2Int origin,
            int maxRadius,
            out Vector2Int targetPosition,
            out string reason)
        {
            targetPosition = origin;
            reason = null;

            int clampedRadius =
                Mathf.Max(0, maxRadius);

            for (int radius = 0;
                 radius <= clampedRadius;
                 radius++)
            {
                if (radius == 0)
                {
                    if (TryExitGarrison(
                            unitId,
                            origin,
                            out reason))
                    {
                        targetPosition = origin;
                        return true;
                    }
                    continue;
                }

                int minX = origin.x - radius;
                int maxX = origin.x + radius;
                int minY = origin.y - radius;
                int maxY = origin.y + radius;

                // Deterministic clockwise perimeter.
                for (int x = minX; x <= maxX; x++)
                {
                    var candidate = new Vector2Int(x, maxY);
                    if (TryExitGarrison(unitId, candidate, out reason))
                    {
                        targetPosition = candidate;
                        return true;
                    }
                }

                for (int y = maxY - 1; y >= minY; y--)
                {
                    var candidate = new Vector2Int(maxX, y);
                    if (TryExitGarrison(unitId, candidate, out reason))
                    {
                        targetPosition = candidate;
                        return true;
                    }
                }

                for (int x = maxX - 1; x >= minX; x--)
                {
                    var candidate = new Vector2Int(x, minY);
                    if (TryExitGarrison(unitId, candidate, out reason))
                    {
                        targetPosition = candidate;
                        return true;
                    }
                }

                for (int y = minY + 1; y < maxY; y++)
                {
                    var candidate = new Vector2Int(minX, y);
                    if (TryExitGarrison(unitId, candidate, out reason))
                    {
                        targetPosition = candidate;
                        return true;
                    }
                }
            }

            reason =
                $"Не знайдено вільну клітинку для виходу " +
                $"з гарнізону в радіусі {clampedRadius}.";
            return false;
        }

        public bool IsGarrisoned(string unitId)
            => !string.IsNullOrWhiteSpace(unitId)
               && _garrisonedUnitPositions.ContainsKey(unitId);

        public string GetUnitOwnerId(string unitId)
            => !string.IsNullOrWhiteSpace(unitId)
               && _unitOwnerIds.TryGetValue(unitId, out string ownerId)
                ? ownerId
                : "player_0";

        public bool CanTraverseOccupiedConstructionCell(
            string unitId,
            Vector2Int position,
            bool openGateIfNeeded,
            out string reason)
        {
            reason = null;
            if (!_objectsMapService.TryGetOccupant(
                    position,
                    out string occupantId)
                || occupantId == unitId)
            {
                return true;
            }

            BuildingDefinition building =
                _buildingRegistry?.GetById(occupantId);
            if (building == null)
            {
                reason = "Клітинка зайнята не будівлею.";
                return false;
            }

            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    building,
                    out GateBuildingModule _))
            {
                string ownerId = GetUnitOwnerId(unitId);
                if (_gateStateService == null)
                {
                    reason = "Gate runtime не підключений.";
                    return false;
                }

                return openGateIfNeeded
                    ? _gateStateService.TryEnsureOpenForUnit(
                        position,
                        ownerId,
                        out reason)
                    : _gateStateService.CanUnitPassGate(
                        position,
                        ownerId,
                        out reason);
            }

            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    building,
                    out WallBuildingModule wall))
            {
                if (wall.IsPassable)
                    return true;

                reason = "Стіна непрохідна.";
                return false;
            }

            if (building.Footprint?.BlocksMovement == true)
            {
                reason = "Будівля блокує рух.";
                return false;
            }

            return true;
        }

        private bool CanUnitMove(string unitId, Vector2Int targetPosition)
        {
            if (!_unitStamina.ContainsKey(unitId)
                || _garrisonedUnitPositions.ContainsKey(unitId))
            {
                return false;
            }

            if (!_gridService.TryGetTileData(targetPosition, out var tileTypeId))
                return false;

            if (string.IsNullOrEmpty(tileTypeId))
                return false;

            if (_objectsMapService.TryGetOccupant(
                    targetPosition,
                    out string occupantId))
            {
                BuildingDefinition building =
                    _buildingRegistry?.GetById(occupantId);
                if (building == null)
                    return false;

                if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                        building,
                        out WallBuildingModule wall))
                {
                    if (!wall.IsPassable)
                        return false;
                }
                else if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                             building,
                             out GateBuildingModule _))
                {
                    string ownerId = GetUnitOwnerId(unitId);
                    if (_gateStateService == null
                        || !_gateStateService.TryEnsureOpenForUnit(
                            targetPosition,
                            ownerId,
                            out _))
                    {
                        return false;
                    }
                }
                else if (building.Footprint?.BlocksMovement == true)
                {
                    return false;
                }
            }

            var tileCost =
                _tileSettings.GetTileWeight(tileTypeId);
            return _unitStamina[unitId] >= tileCost;
        }

        private float ResolveStartingStamina(string unitTypeId, UnitClassConfig config)
        {
            return config == null ? 0f : Mathf.Max(0f, config.BaseStamina);
        }
    }
}
