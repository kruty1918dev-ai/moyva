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
        private readonly Kruty1918.Localization.ILocalizationService _loca;

        public BuildingWorldInfoPresenter(
            SignalBus signalBus,
            IBuildingRegistry buildingRegistry,
            [InjectOptional] IEconomyInfoMediator economyInfoMediator,
            [InjectOptional] Kruty1918.Localization.ILocalizationService localization = null)
        {
            _signalBus = signalBus;
            _buildingRegistry = buildingRegistry;
            _economyInfoMediator = economyInfoMediator;
            _loca = localization;
        }

        private string T(string key) => _loca?.T(key) ?? key ?? string.Empty;
        private string TF(string key, params object[] args) => _loca?.TF(key, args) ?? key ?? string.Empty;

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
                    ? T("Building")
                    : T(definition.DisplayName);

                var subtitle = BuildSubtitle(definition, settlementContext.SettlementName);
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

        private string BuildSubtitle(BuildingDefinition definition, string settlementName)
        {
            if (string.IsNullOrWhiteSpace(settlementName))
                settlementName = "No settlement";

            if (BuildingDefinitionCapabilities.IsTownHall(definition))
                return T("Town Hall") + " • " + settlementName;

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return T("Warehouse") + " • " + settlementName;

            if (BuildingDefinitionCapabilities.IsCastle(definition))
                return T("Capital") + " • " + settlementName;

            return T("Building") + " • " + settlementName;
        }

        private string BuildFallbackText(BuildingDefinition definition)
        {
            var details = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(definition.Description))
                details.AppendLine(T(definition.Description.Trim()));

            BuildingDefaultInfoExtractor.AppendMeaningfulFacts(
                definition,
                details,
                ResolveResourceDisplayName,
                _loca);

            return details.Length > 0
                ? details.ToString().TrimEnd()
                : T("No additional information.");
        }

        private string BuildResourcesText(BuildingDefinition definition, EconomySettlementContext settlementContext, Vector2Int position)
        {
            IReadOnlyDictionary<string, float> resources;

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
            {
                resources = _economyInfoMediator.GetWarehouseResourceTotals(position);
                return FormatResources(resources, T("Warehouse resources"));
            }

            if (BuildingDefinitionCapabilities.IsTownHall(definition))
            {
                resources = _economyInfoMediator.GetSettlementWarehousesTotal(settlementContext.SettlementId);
                return FormatResources(resources, T("Resources of all settlement warehouses"));
            }

            if (BuildingDefinitionCapabilities.IsCastle(definition))
            {
                resources = _economyInfoMediator.GetOwnerResourceTotals(settlementContext.OwnerId);
                return FormatResources(resources, T("Owner capital summary"));
            }

            resources = _economyInfoMediator.GetSettlementResourceTotals(settlementContext.SettlementId);
            var details = new StringBuilder();
            details.AppendLine(FormatResources(resources, T("Settlement resources")));

            int beforeFacts = details.Length;
            if (BuildingDefaultInfoExtractor.AppendMeaningfulFacts(definition, details, ResolveResourceDisplayName, _loca))
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
                sb.Append(T("No resources."));
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
        {
            string mediated = _economyInfoMediator?.GetResourceDisplayName(resourceId);
            if (!string.IsNullOrWhiteSpace(mediated)
                && !string.Equals(mediated, resourceId, StringComparison.OrdinalIgnoreCase))
            {
                return mediated;
            }

            switch ((resourceId ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "material":
                case "materials": return T("Materials");
                case "food": return T("Food");
                case "money": return T("Money");
                case "wood": return T("Wood");
                case "hardwood": return T("Processed wood");
                case "stone": return T("Stone");
                case "iron":
                case "iron-ore": return T("Iron");
                default: return T("Resource");
            }
        }
    }
}
