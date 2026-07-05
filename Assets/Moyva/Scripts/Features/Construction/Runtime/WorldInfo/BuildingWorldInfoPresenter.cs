using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.Construction.API;
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

        public BuildingWorldInfoPresenter(
            SignalBus signalBus,
            IBuildingRegistry buildingRegistry,
            [InjectOptional] IEconomyInfoMediator economyInfoMediator)
        {
            _signalBus = signalBus;
            _buildingRegistry = buildingRegistry;
            _economyInfoMediator = economyInfoMediator;
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
                    ConstructionCostItems = BuildConstructionCostItems(definition),
                });
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

        private string BuildFallbackText(BuildingDefinition definition)
        {
            var details = new StringBuilder();
            details.AppendLine("Базова інформація");
            details.AppendLine($"ID: {definition.Id}");
            details.AppendLine($"Категорія: {definition.Category}");

            if (BuildingDefaultInfoExtractor.AppendMeaningfulFacts(definition, details, ResolveResourceDisplayName))
                return details.ToString().TrimEnd();

            return details.ToString().TrimEnd();
        }

        private string BuildResourcesText(BuildingDefinition definition, EconomySettlementContext settlementContext, Vector2Int position)
        {
            IReadOnlyDictionary<string, float> resources;

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
            {
                resources = _economyInfoMediator.GetWarehouseResourceTotals(position);
                return FormatResources(resources, "Ресурси складу");
            }

            if (BuildingDefinitionCapabilities.IsTownHall(definition))
            {
                resources = _economyInfoMediator.GetSettlementWarehousesTotal(settlementContext.SettlementId);
                return FormatResources(resources, "Ресурси всіх складів поселення");
            }

            if (BuildingDefinitionCapabilities.IsCastle(definition))
            {
                resources = _economyInfoMediator.GetOwnerResourceTotals(settlementContext.OwnerId);
                return FormatResources(resources, "Зведення по капіталу власника");
            }

            resources = _economyInfoMediator.GetSettlementResourceTotals(settlementContext.SettlementId);
            var details = new StringBuilder();
            details.AppendLine(FormatResources(resources, "Ресурси поселення"));

            int beforeFacts = details.Length;
            if (BuildingDefaultInfoExtractor.AppendMeaningfulFacts(definition, details, ResolveResourceDisplayName))
            {
                if (beforeFacts > 0)
                    details.Insert(beforeFacts, Environment.NewLine);
            }

            return details.ToString().TrimEnd();
        }

        private string FormatResources(IReadOnlyDictionary<string, float> resources, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);

            if (resources == null || resources.Count == 0)
            {
                sb.Append("Немає ресурсів.");
                return sb.ToString();
            }

            foreach (var entry in resources)
                sb.AppendLine($"- {ResolveResourceDisplayName(entry.Key)}: {entry.Value:0.#}");

            return sb.ToString().TrimEnd();
        }

        private BuildingConstructionCostItemData[] BuildConstructionCostItems(BuildingDefinition definition)
        {
            if (definition.ConstructionCost == null || definition.ConstructionCost.Count == 0)
                return System.Array.Empty<BuildingConstructionCostItemData>();

            var items = new List<BuildingConstructionCostItemData>(definition.ConstructionCost.Count);
            for (int i = 0; i < definition.ConstructionCost.Count; i++)
            {
                var entry = definition.ConstructionCost[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0)
                    continue;

                string resourceId = entry.ResourceId.Trim();
                string displayName = ResolveResourceDisplayName(resourceId);
                items.Add(new BuildingConstructionCostItemData
                {
                    ResourceId = resourceId,
                    DisplayName = displayName,
                    Amount = entry.Amount,
                    Icon = null,
                });
            }

            return items.Count == 0
                ? System.Array.Empty<BuildingConstructionCostItemData>()
                : items.ToArray();
        }

        private string ResolveResourceDisplayName(string resourceId)
            => _economyInfoMediator?.GetResourceDisplayName(resourceId)
               ?? (string.IsNullOrWhiteSpace(resourceId) ? string.Empty : resourceId.Trim());
    }
}