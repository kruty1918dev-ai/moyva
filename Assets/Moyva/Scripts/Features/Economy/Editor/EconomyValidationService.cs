using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Editor.Shared;

namespace Kruty1918.Moyva.Economy.Editor
{
    public sealed class EconomyValidationService
    {
        public IReadOnlyList<EconomyValidationIssue> Validate(EconomyDatabaseSO database, BuildingRegistrySO buildingRegistry = null)
        {
            var issues = new List<EconomyValidationIssue>();
            if (database == null)
            {
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "EconomyDatabase is not assigned."));
                return issues;
            }

            ValidateResources(database, issues);
            ValidateSettlements(database, issues);
            ValidateWarehousePolicies(database, issues);
            ValidateProduction(database, issues);
            ValidateCaravans(database, issues);
            ValidateRulesConfig(database, issues);
            ValidateBuildingModules(buildingRegistry, database.RulesConfig?.Building, issues);

            return issues;
        }

        public IReadOnlyList<EconomyValidationIssue> ValidateBuildingModules(BuildingRegistrySO buildingRegistry)
        {
            var issues = new List<EconomyValidationIssue>();
            ValidateBuildingModules(buildingRegistry, null, issues);
            return issues;
        }

        public IReadOnlyList<EconomyValidationIssue> ValidateBuildingModules(BuildingDefinition definition, int fallbackIndex = -1, EconomyBuildingRules rules = null)
        {
            var issues = new List<EconomyValidationIssue>();
            AppendBuildingModuleIssues(definition, fallbackIndex, issues, null, rules);
            return issues;
        }

        private static void ValidateBuildingModules(BuildingRegistrySO buildingRegistry, EconomyBuildingRules rules, List<EconomyValidationIssue> issues)
        {
            if (buildingRegistry == null || buildingRegistry.Buildings == null)
                return;

            for (int i = 0; i < buildingRegistry.Buildings.Length; i++)
                AppendBuildingModuleIssues(buildingRegistry.Buildings[i], i, issues, buildingRegistry, rules);
        }

        private static void AppendBuildingModuleIssues(
            BuildingDefinition definition,
            int fallbackIndex,
            List<EconomyValidationIssue> issues,
            BuildingRegistrySO context = null,
            EconomyBuildingRules rules = null)
        {
            if (definition == null)
                return;

            var moduleIssues = BuildingModuleEditorShared.GetFilteredIssues(definition, rules);
            for (int j = 0; j < moduleIssues.Count; j++)
            {
                var issue = moduleIssues[j];
                if (issue == null)
                    continue;

                var severity = issue.Severity == BuildingValidationSeverity.Error
                    ? EconomyValidationSeverity.Error
                    : EconomyValidationSeverity.Warning;

                string buildingId = string.IsNullOrWhiteSpace(definition.Id) ? $"<building:{fallbackIndex}>" : definition.Id;
                issues.Add(new EconomyValidationIssue(
                    severity,
                    $"Building '{buildingId}': [{issue.Code}] {issue.Message}",
                    context));
            }
        }

        private static void ValidateResources(EconomyDatabaseSO database, List<EconomyValidationIssue> issues)
        {
            var duplicates = new HashSet<string>(StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var resource in database.Resources.Where(r => r != null))
            {
                var id = Normalize(resource.Id);
                if (string.IsNullOrEmpty(id))
                    issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Resource has missing Id.", resource));
                else if (!seen.Add(id))
                    duplicates.Add(id);

                if (resource.Category == EconomyResourceCategory.None)
                    issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Warning, $"Resource '{id}' has no category.", resource));
            }

            if (!database.Resources.Any(r => r != null && r.Category == EconomyResourceCategory.Money))
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Warning, "No Money resource found in EconomyDatabase.", database));

            foreach (var duplicateId in duplicates)
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, $"Duplicate resource Id: '{duplicateId}'.", database));
        }

        private static void ValidateSettlements(EconomyDatabaseSO database, List<EconomyValidationIssue> issues)
        {
            var duplicates = new HashSet<string>(StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var settlement in database.Settlements.Where(s => s != null))
            {
                var id = Normalize(settlement.SettlementId);
                if (string.IsNullOrEmpty(id))
                    issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Settlement has missing SettlementId.", settlement));
                else if (!seen.Add(id))
                    duplicates.Add(id);

                if (string.IsNullOrWhiteSpace(settlement.CenterBuildingId))
                    issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, $"Settlement '{id}' has no CenterBuildingId.", settlement));

                if (settlement.BuildRadius <= 0)
                    issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, $"Settlement '{id}' has invalid BuildRadius ({settlement.BuildRadius}).", settlement));
            }

            foreach (var duplicateId in duplicates)
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, $"Duplicate settlement Id: '{duplicateId}'.", database));
        }

        private static void ValidateWarehousePolicies(EconomyDatabaseSO database, List<EconomyValidationIssue> issues)
        {
            var knownResourceIds = new HashSet<string>(database.Resources.Where(r => r != null).Select(r => Normalize(r.Id)).Where(id => !string.IsNullOrEmpty(id)), StringComparer.Ordinal);

            foreach (var warehouse in database.WarehousePolicies.Where(w => w != null))
            {
                foreach (var entry in warehouse.Entries.Where(e => e != null))
                {
                    var resourceId = Normalize(entry.ResourceId);
                    if (string.IsNullOrEmpty(resourceId))
                    {
                        issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Warehouse policy entry has missing ResourceId.", warehouse));
                        continue;
                    }

                    if (!knownResourceIds.Contains(resourceId))
                        issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, $"Warehouse policy references unknown resource '{resourceId}'.", warehouse));
                }
            }
        }

        private static void ValidateProduction(EconomyDatabaseSO database, List<EconomyValidationIssue> issues)
        {
            foreach (var profile in database.ProductionProfiles.Where(p => p != null))
            {
                var label = Normalize(profile.BuildingId);
                if (string.IsNullOrWhiteSpace(profile.BuildingId))
                    issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Production profile is missing BuildingId.", profile));

                if (string.IsNullOrWhiteSpace(profile.RecipeId))
                    issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, $"Production profile '{label}' is missing RecipeId.", profile));

                if (profile.OutputAmountPerCycle <= 0)
                    issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, $"Production profile '{label}' has invalid OutputAmountPerCycle ({profile.OutputAmountPerCycle}).", profile));

                if (profile.CycleDurationSeconds <= 0f)
                    issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, $"Production profile '{label}' has invalid CycleDurationSeconds ({profile.CycleDurationSeconds}).", profile));
            }
        }

        private static void ValidateCaravans(EconomyDatabaseSO database, List<EconomyValidationIssue> issues)
        {
            foreach (var caravan in database.CaravanTemplates.Where(c => c != null))
            {
                var id = Normalize(caravan.TemplateId);
                if (string.IsNullOrEmpty(id))
                    issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Caravan template has missing TemplateId.", caravan));

                if (caravan.Capacity <= 0)
                    issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, $"Caravan template '{id}' has invalid Capacity ({caravan.Capacity}).", caravan));
            }
        }

        private static void ValidateRulesConfig(EconomyDatabaseSO database, List<EconomyValidationIssue> issues)
        {
            var rules = database.RulesConfig;
            if (rules == null)
            {
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Economy rules config is not assigned.", database));
                return;
            }

            if (rules.Settlement.MaxSettlements <= 0)
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Rules: MaxSettlements must be > 0.", rules));

            if (rules.Settlement.MinTownHallDistance <= 0)
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Rules: MinTownHallDistance must be > 0.", rules));

            if (rules.Population.NewResidentsArrivalIntervalTurns <= 0)
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Rules: NewResidentsArrivalIntervalTurns must be > 0.", rules));

            if (rules.Caravan.MaxCaravansPerSettlement <= 0)
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Rules: MaxCaravansPerSettlement must be > 0.", rules));

            if (rules.Market.TargetStock <= 0f)
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Rules: Market TargetStock must be > 0.", rules));

            if (rules.Market.ReferenceTradeVolume <= 0f)
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Rules: Market ReferenceTradeVolume must be > 0.", rules));

            if (rules.Market.MinPriceMultiplier <= 0f || rules.Market.MaxPriceMultiplier <= 0f)
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Rules: Market multipliers must be > 0.", rules));

            if (rules.Market.MaxPriceMultiplier < rules.Market.MinPriceMultiplier)
                issues.Add(new EconomyValidationIssue(EconomyValidationSeverity.Error, "Rules: MaxPriceMultiplier cannot be lower than MinPriceMultiplier.", rules));
        }

        private static string Normalize(string value) => (value ?? string.Empty).Trim();
    }
}
