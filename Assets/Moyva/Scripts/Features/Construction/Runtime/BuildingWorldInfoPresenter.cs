using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class BuildingWorldInfoPresenter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IEconomyInfoMediator _economyInfoMediator;
        private readonly ILocalPlayerIdentityProvider _localPlayerIdentityProvider;
        private readonly EconomyDatabaseSO _economyDatabase;
        private readonly EconomyManager _economyManager;

        public BuildingWorldInfoPresenter(
            SignalBus signalBus,
            IBuildingRegistry buildingRegistry,
            [InjectOptional] IEconomyInfoMediator economyInfoMediator,
            [InjectOptional] ILocalPlayerIdentityProvider localPlayerIdentityProvider,
            [InjectOptional] EconomyDatabaseSO economyDatabase,
            [InjectOptional] EconomyManager economyManager)
        {
            _signalBus = signalBus;
            _buildingRegistry = buildingRegistry;
            _economyInfoMediator = economyInfoMediator;
            _localPlayerIdentityProvider = localPlayerIdentityProvider;
            _economyDatabase = economyDatabase;
            _economyManager = economyManager;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BuildingInfoPanelRequestedSignal>(OnBuildingInfoRequested);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<BuildingInfoPanelRequestedSignal>(OnBuildingInfoRequested);
        }

        private void OnBuildingInfoRequested(BuildingInfoPanelRequestedSignal signal)
        {
            try
            {
                var definition = _buildingRegistry?.GetById(signal.BuildingId);
                if (definition == null)
                    return;

                if (!CanLocalPlayerSeeBuilding(signal.Position))
                {
                    _signalBus.Fire(new WorldInfoPanelClosedSignal());
                    return;
                }

                var hasEconomyContext = false;
                EconomySettlementContext settlementContext = default;

                if (_economyInfoMediator != null
                    && _economyInfoMediator.TryGetBuildingContext(signal.Position, out _, out _)
                    && _economyInfoMediator.TryGetSettlementContext(signal.Position, out settlementContext))
                {
                    hasEconomyContext = true;
                }

                var title = string.IsNullOrWhiteSpace(definition.DisplayName)
                    ? definition.Id
                    : definition.DisplayName;

                var subtitle = BuildSubtitle(definition, settlementContext.SettlementId, settlementContext.SettlementName);
                var content = hasEconomyContext
                    ? BuildResourcesText(definition, settlementContext, signal.Position)
                    : BuildFallbackText(definition);

                _signalBus.Fire(new WorldInfoPanelRequestedSignal
                {
                    Title = title,
                    Subtitle = subtitle,
                    Content = content,
                });

                if (hasEconomyContext && (BuildingDefinitionCapabilities.IsCastle(definition) || BuildingDefinitionCapabilities.IsBarn(definition)))
                {
                    _signalBus.Fire(new SettlementStatisticsMenuRequestedSignal
                    {
                        SettlementId = settlementContext.SettlementId,
                        OwnerId = settlementContext.OwnerId,
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildingWorldInfoPresenter] Помилка формування інформації: {ex.GetType().Name} - {ex.Message}");
            }
        }

        private static string BuildSubtitle(BuildingDefinition definition, string settlementId, string settlementName)
        {
            if (string.IsNullOrWhiteSpace(settlementName))
                settlementName = settlementId;

            if (string.IsNullOrWhiteSpace(settlementName))
                settlementName = "Без поселення";

            if (BuildingDefinitionCapabilities.IsTownHall(definition))
                return $"Ратуша • {settlementName}";

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return $"Склад • {settlementName}";

            if (BuildingDefinitionCapabilities.IsCastle(definition))
                return $"Капітал • {settlementName}";

            return $"Будівля • {settlementName}";
        }

        private static string BuildFallbackText(BuildingDefinition definition)
        {
            var details = new StringBuilder();
            details.AppendLine("Базова інформація");
            details.AppendLine($"ID: {definition.Id}");
            details.AppendLine($"Категорія: {definition.Category}");

            if (BuildingDefaultInfoExtractor.AppendMeaningfulFacts(definition, details))
                return details.ToString().TrimEnd();

            return details.ToString().TrimEnd();
        }

        private bool CanLocalPlayerSeeBuilding(Vector2Int position)
        {
            if (_localPlayerIdentityProvider == null)
                return true;

            string localPlayerId = _localPlayerIdentityProvider.LocalPlayerId;
            if (string.IsNullOrWhiteSpace(localPlayerId))
                return true;

            if (_economyInfoMediator == null)
                return true;

            if (!_economyInfoMediator.TryGetBuildingContext(position, out _, out var ownerId))
                return true;

            if (string.IsNullOrWhiteSpace(ownerId))
                return true;

            return string.Equals(ownerId, localPlayerId, StringComparison.Ordinal);
        }

        private string BuildResourcesText(BuildingDefinition definition, EconomySettlementContext settlementContext, Vector2Int position)
        {
            IReadOnlyDictionary<string, float> resources;

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
            {
                resources = _economyInfoMediator.GetWarehouseResourceTotals(position);
                
                // Використовуємо спеціалізований форматер для складів з категоризацією
                if (_economyDatabase != null)
                    return WarehouseInfoFormatter.FormatWarehouseResources(resources, _economyDatabase, "Ресурси складу");
                
                // Fallback на стандартний формат якщо база не доступна
                return FormatResources(resources, "Ресурси складу");
            }

            if (BuildingDefinitionCapabilities.IsTownHall(definition))
            {
                resources = _economyInfoMediator.GetSettlementWarehousesTotal(settlementContext.SettlementId);
                return FormatResources(resources, "Ресурси всіх складів поселення");
            }

            if (BuildingDefinitionCapabilities.IsCastle(definition))
            {
                // Детальна інформація про замок з статистикою поселення
                resources = _economyInfoMediator.GetOwnerResourceTotals(settlementContext.OwnerId);
                
                // Отримати позиції будівель для детальної інформації
                var buildingPositions = _economyInfoMediator.GetSettlementBuildingPositions(settlementContext.SettlementId);
                
                // Отримати повне стан поселення для статистики
                var settlements = _economyManager?.Settlements;
                EconomySettlementState settlementState = null;
                
                if (settlements != null && settlements.TryGetValue(settlementContext.SettlementId, out settlementState))
                {
                    return CastleSettlementStatistics.FormatCastleInfoDetailed(
                        settlementState,
                        _buildingRegistry,
                        buildingPositions?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<Vector2Int, string>());
                }
                
                // Fallback якщо не можна отримати детальну інформацію
                return FormatResources(resources, "Зведення по капіталу власника");
            }

            if (BuildingDefinitionCapabilities.IsBarn(definition))
            {
                // Детальна інформація про амбар з інформацією про жителів
                var settlements = _economyManager?.Settlements;
                EconomySettlementState settlementState = null;
                
                if (settlements != null && settlements.TryGetValue(settlementContext.SettlementId, out settlementState))
                {
                    return BarnSettlementStatistics.FormatBarnInfoDetailed(settlementState);
                }
                
                // Fallback якщо не можна отримати деталі
                return "Інформація про жителів недоступна.";
            }

            resources = _economyInfoMediator.GetSettlementResourceTotals(settlementContext.SettlementId);
            var details = new StringBuilder();
            details.AppendLine(FormatResources(resources, "Ресурси поселення"));

            int beforeFacts = details.Length;
            if (BuildingDefaultInfoExtractor.AppendMeaningfulFacts(definition, details))
            {
                if (beforeFacts > 0)
                    details.Insert(beforeFacts, Environment.NewLine);
            }

            return details.ToString().TrimEnd();
        }

        private static string FormatResources(IReadOnlyDictionary<string, float> resources, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);

            if (resources == null || resources.Count == 0)
            {
                sb.Append("Немає ресурсів.");
                return sb.ToString();
            }

            foreach (var entry in resources)
                sb.AppendLine($"- {entry.Key}: {entry.Value:0.#}");

            return sb.ToString().TrimEnd();
        }
    }
}