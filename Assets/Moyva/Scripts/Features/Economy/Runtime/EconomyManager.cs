using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Головний фасад економічної системи.
    /// Підписується на Calendar (кожен хід) та Construction (розміщення/знесення будівель).
    /// Запускає <see cref="EconomyTickOrchestrator"/> для кожного поселення кожного ходу.
    ///
    /// Використання: додати <see cref="EconomyInstaller"/> в сцену — все інше автоматично.
    /// </summary>
    public sealed class EconomyManager : IInitializable, IDisposable
    {
        public const string DefaultOwnerId = "player_0";

        private readonly ICalendarService _calendar;
        private readonly SignalBus _signalBus;
        private readonly EconomyDatabaseSO _database;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IEconomyOwnerResourcePoolService _ownerResourcePoolService;
        private readonly ISettlementRegistry _settlementRegistry;
        private readonly IEconomyBuildingIntegration _buildingIntegration;
        private readonly IEconomyTurnProcessor _turnProcessor;

        private readonly Dictionary<string, Dictionary<string, float>> _ownerResourcePools =
            new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);

        private EconomyRulesConfigSO Rules => _database?.RulesConfig;

        // Keeps direct construction in tests/backward-compatible call sites.
        public EconomyManager(
            ICalendarService calendar,
            SignalBus signalBus,
            EconomyDatabaseSO database,
            IBuildingRegistry buildingRegistry)
            : this(calendar, signalBus, database, buildingRegistry, null, null, null, null)
        {
        }

        [Inject]
        internal EconomyManager(
            ICalendarService calendar,
            SignalBus signalBus,
            EconomyDatabaseSO database,
            IBuildingRegistry buildingRegistry,
            [InjectOptional] IEconomyOwnerResourcePoolService ownerResourcePoolService = null,
            [InjectOptional] ISettlementRegistry settlementRegistry = null,
            [InjectOptional] IEconomyBuildingIntegration buildingIntegration = null,
            [InjectOptional] IEconomyTurnProcessor turnProcessor = null)
        {
            _calendar = calendar;
            _signalBus = signalBus;
            _database = database;
            _buildingRegistry = buildingRegistry;
            _ownerResourcePoolService = ownerResourcePoolService ?? new EconomyOwnerResourcePoolService();
            _settlementRegistry = settlementRegistry ?? new EconomySettlementRegistryService();
            _buildingIntegration = buildingIntegration ?? new EconomyBuildingIntegrationService();
            _turnProcessor = turnProcessor ?? new EconomyTurnProcessorService();
        }

        public IReadOnlyDictionary<string, EconomySettlementState> Settlements => _settlementRegistry.AllSettlements;

        // ───────────────────────── Lifecycle

        public void Initialize()
        {
            _calendar.OnHourChanged += OnTurnAdvanced;
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.Subscribe<GrantStarterPackResourcesSignal>(OnGrantStarterPackResources);
        }

        public void Dispose()
        {
            _calendar.OnHourChanged -= OnTurnAdvanced;
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.TryUnsubscribe<GrantStarterPackResourcesSignal>(OnGrantStarterPackResources);
        }

        // ───────────────────────── Turn Processing

        private void OnTurnAdvanced()
        {
            if (_database == null || Rules == null)
                return;

            float turnDurationSeconds = _calendar.Config.HoursPerTurn * 3600f;
            _turnProcessor.ProcessTurn(_settlementRegistry, _signalBus, _database, turnDurationSeconds);
        }

        // ───────────────────────── Construction Events

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            var createdSettlement = _buildingIntegration.OnBuildingPlaced(
                signal,
                _settlementRegistry,
                _signalBus,
                _database,
                _buildingRegistry);

            if (createdSettlement != null)
                TransferOwnerResourcesToSettlement(createdSettlement.OwnerId, createdSettlement);
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
        {
            _buildingIntegration.OnBuildingDemolished(
                signal,
                _settlementRegistry,
                _signalBus,
                _database,
                _buildingRegistry);
        }

        private void OnGrantStarterPackResources(GrantStarterPackResourcesSignal signal)
        {
            if (signal.Entries == null || signal.Entries.Length == 0)
                return;

            if (string.IsNullOrWhiteSpace(signal.SettlementId))
            {
                string ownerId = NormalizeOwnerId(signal.OwnerId);
                for (int index = 0; index < signal.Entries.Length; index++)
                {
                    var entry = signal.Entries[index];
                    if (string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                        continue;

                    AddOwnerResource(ownerId, entry.ResourceId.Trim(), entry.Amount);
                }

                return;
            }

            var state = _settlementRegistry.GetSettlement(signal.SettlementId);
            if (state == null || !state.IsActive)
                return;

            string ownerFromSignal = NormalizeOwnerId(signal.OwnerId);
            string ownerFromSettlement = NormalizeOwnerId(state.OwnerId);
            if (!string.Equals(ownerFromSignal, ownerFromSettlement, StringComparison.Ordinal))
            {
                Debug.LogWarning($"[Economy] Пропущено стартовий пакет: owner mismatch signal='{ownerFromSignal}', settlement='{ownerFromSettlement}'.");
                return;
            }

            for (int index = 0; index < signal.Entries.Length; index++)
            {
                var entry = signal.Entries[index];
                if (string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                    continue;

                AddResource(signal.SettlementId, entry.ResourceId.Trim(), entry.Amount);
            }
        }

        private void AddOwnerResource(string ownerId, string resourceId, float amount)
        {
            _ownerResourcePoolService.AddOwnerResource(
                _ownerResourcePools,
                NormalizeOwnerId(ownerId),
                resourceId,
                amount,
                _signalBus);

            TransferOwnerResourcesToExistingSettlement(NormalizeOwnerId(ownerId));
        }

        private void TransferOwnerResourcesToSettlement(string ownerId, EconomySettlementState state)
        {
            _ownerResourcePoolService.TransferOwnerResourcesToSettlement(
                _ownerResourcePools,
                NormalizeOwnerId(ownerId),
                state,
                _signalBus);
        }

        // ───────────────────────── Public API for UI / other systems

        /// <summary>Отримати стан поселення за ID.</summary>
        public EconomySettlementState GetSettlement(string settlementId)
        {
            return _settlementRegistry.GetSettlement(settlementId);
        }

        public bool TryGetSettlementByPosition(Vector2Int position, out EconomySettlementState state)
        {
            return _settlementRegistry.TryGetSettlementByPosition(position, out state);
        }

        public bool TryResolveConstructionSettlement(Vector2Int position, string ownerId, out EconomySettlementState state)
        {
            return _settlementRegistry.TryFindNearestSettlement(position, NormalizeOwnerId(ownerId), out state);
        }

        public bool TryConsumeSettlementResources(string settlementId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(settlementId))
            {
                errorMessage = "Не визначено поселення для списання ресурсів.";
                return false;
            }

            var state = _settlementRegistry.GetSettlement(settlementId);
            if (state == null || !state.IsActive)
            {
                errorMessage = $"Поселення '{settlementId}' недоступне або неактивне.";
                return false;
            }

            if (resourceCosts == null || resourceCosts.Count == 0)
                return true;

            foreach (var pair in resourceCosts)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    errorMessage = "Спроба списати ресурс з порожнім ID.";
                    return false;
                }

                if (pair.Value <= 0f)
                    continue;

                float currentAmount = state.GetResource(pair.Key);
                if (currentAmount + 0.0001f < pair.Value)
                {
                    errorMessage = $"Недостатньо ресурсу '{ResolveResourceDisplayName(pair.Key)}' у поселенні '{GetSettlementNameOrFallback(settlementId)}': потрібно {pair.Value:0.#}, зараз {currentAmount:0.#}.";
                    return false;
                }
            }

            foreach (var pair in resourceCosts)
            {
                if (pair.Value <= 0f)
                    continue;

                float before = state.GetResource(pair.Key);
                if (!state.ConsumeResource(pair.Key, pair.Value))
                {
                    errorMessage = $"Не вдалося списати ресурс '{ResolveResourceDisplayName(pair.Key)}' у поселенні '{GetSettlementNameOrFallback(settlementId)}'.";
                    return false;
                }

                _signalBus.Fire(new SettlementResourceChangedSignal
                {
                    SettlementId = settlementId,
                    OwnerId = NormalizeOwnerId(state.OwnerId),
                    ResourceId = pair.Key,
                    NewAmount = state.GetResource(pair.Key),
                    Delta = state.GetResource(pair.Key) - before,
                });
            }

            return true;
        }

        public bool TryGetBuildingAtPosition(Vector2Int position, out string buildingId, out string ownerId)
        {
            return _settlementRegistry.TryGetBuildingAtPosition(position, out buildingId, out ownerId);
        }

        public Dictionary<string, float> GetWarehouseResourceTotalsByPosition(Vector2Int warehousePosition)
        {
            if (!_settlementRegistry.TryGetSettlementByPosition(warehousePosition, out var state) || state == null)
                return new Dictionary<string, float>(StringComparer.Ordinal);

            return state.GetWarehouseSnapshot(ToWarehouseKey(warehousePosition));
        }

        public Dictionary<string, float> GetSettlementWarehousesTotal(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
                return new Dictionary<string, float>(StringComparer.Ordinal);

            var state = _settlementRegistry.GetSettlement(settlementId);
            if (state == null)
                return new Dictionary<string, float>(StringComparer.Ordinal);

            return state.GetAllWarehousesTotalSnapshot();
        }

        public Dictionary<string, float> GetSettlementResourceTotals(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
                return new Dictionary<string, float>(StringComparer.Ordinal);

            var state = _settlementRegistry.GetSettlement(settlementId);
            if (state == null)
                return new Dictionary<string, float>(StringComparer.Ordinal);

            return new Dictionary<string, float>(state.ResourcePool, StringComparer.Ordinal);
        }

        public Dictionary<string, float> GetOwnerResourceTotals(string ownerId)
        {
            return _ownerResourcePoolService.GetOwnerResourceTotals(
                _ownerResourcePools,
                _settlementRegistry.AllSettlements,
                NormalizeOwnerId(ownerId));
        }

        public Dictionary<string, Dictionary<string, float>> GetOwnerResourcePoolsSnapshot()
        {
            return _ownerResourcePoolService.GetOwnerResourcePoolsSnapshot(_ownerResourcePools);
        }

        public Dictionary<string, Dictionary<string, float>> GetOwnerResourceTotalsSnapshot()
        {
            var ownerIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var ownerPair in _ownerResourcePools)
                ownerIds.Add(NormalizeOwnerId(ownerPair.Key));

            foreach (var settlement in _settlementRegistry.AllSettlements.Values)
            {
                if (settlement == null)
                    continue;

                ownerIds.Add(NormalizeOwnerId(settlement.OwnerId));
            }

            var snapshot = new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);
            foreach (string ownerId in ownerIds)
            {
                var totals = GetOwnerResourceTotals(ownerId);
                if (totals.Count > 0)
                    snapshot[ownerId] = totals;
            }

            return snapshot;
        }

        public void RestoreOwnerResourcePools(Dictionary<string, Dictionary<string, float>> snapshot)
        {
            _ownerResourcePoolService.RestoreOwnerResourcePools(_ownerResourcePools, snapshot, _signalBus);
            TransferOwnerResourcesToExistingSettlements();
        }

        private void TransferOwnerResourcesToExistingSettlements()
        {
            _ownerResourcePoolService.TransferOwnerResourcesToExistingSettlements(
                _ownerResourcePools,
                _settlementRegistry.AllSettlements,
                _signalBus);
        }

        private void TransferOwnerResourcesToExistingSettlement(string ownerId)
        {
            if (_ownerResourcePools.Count == 0 || _settlementRegistry.AllSettlements.Count == 0)
                return;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            foreach (var settlement in _settlementRegistry.AllSettlements.Values)
            {
                if (settlement == null || !settlement.IsActive)
                    continue;

                if (!string.Equals(NormalizeOwnerId(settlement.OwnerId), normalizedOwnerId, StringComparison.Ordinal))
                    continue;

                TransferOwnerResourcesToSettlement(normalizedOwnerId, settlement);
                break;
            }
        }

        /// <summary>Додати ресурс до поселення вручну (караван, чіт, тестування).</summary>
        public void AddResource(string settlementId, string resourceId, float amount)
        {
            var state = _settlementRegistry.GetSettlement(settlementId);
            if (state == null)
                return;

            state.AddResource(resourceId, amount);

            _signalBus.Fire(new SettlementResourceChangedSignal
            {
                SettlementId = settlementId,
                OwnerId = NormalizeOwnerId(state.OwnerId),
                ResourceId = resourceId,
                NewAmount = state.GetResource(resourceId),
                Delta = amount,
            });
        }

        public string GetSettlementNameOrFallback(string settlementId)
        {
            return _settlementRegistry.GetSettlementNameOrFallback(settlementId);
        }

        private string ResolveResourceDisplayName(string resourceId)
        {
            string fallback = string.IsNullOrWhiteSpace(resourceId) ? string.Empty : resourceId.Trim();
            if (_database?.Resources == null || string.IsNullOrEmpty(fallback))
                return fallback;

            for (int i = 0; i < _database.Resources.Count; i++)
            {
                var resource = _database.Resources[i];
                if (resource == null || !string.Equals(resource.Id, fallback, StringComparison.Ordinal))
                    continue;

                return string.IsNullOrWhiteSpace(resource.DisplayName)
                    ? fallback
                    : resource.DisplayName;
            }

            return fallback;
        }

        private static string ToWarehouseKey(Vector2Int position)
        {
            return $"{position.x}:{position.y}";
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId) ? DefaultOwnerId : ownerId.Trim();
        }
    }
}
