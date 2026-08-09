using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Editor.Shared;

namespace Kruty1918.Moyva.Construction.Editor.Presets
{
    internal static class BuildingPresetValidation
    {
        private static readonly HashSet<string> SupportedActiveModules = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(WorkforceBuildingModule), nameof(ProductionBuildingModule), nameof(StorageBuildingModule)
        };

        public static void ValidateSpec(BuildingPresetSpec spec)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            if (string.IsNullOrWhiteSpace(spec.Id)) throw new InvalidOperationException("Preset id is empty.");
            if (spec.Version != 1) throw new InvalidOperationException($"{spec.Id}: unsupported preset version {spec.Version}.");
            if (!string.Equals(spec.ModuleMode, "merge", StringComparison.OrdinalIgnoreCase) && !string.Equals(spec.ModuleMode, "replace", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"{spec.Id}: modules.mode must be merge or replace.");
            if (spec.HasMaxHp && spec.MaxHp <= 0) throw new InvalidOperationException($"{spec.Id}: maxHp must be > 0.");
            if (spec.HasBuildTurns && spec.BuildTurns < 0) throw new InvalidOperationException($"{spec.Id}: buildTurns must be >= 0.");
            foreach (BuildingPresetCostSpec cost in spec.Costs)
                if (string.IsNullOrWhiteSpace(cost.Resource) || cost.Amount <= 0) throw new InvalidOperationException($"{spec.Id}: invalid construction cost entry.");
            foreach (BuildingPresetModuleSpec module in spec.ActiveModules)
            {
                if (!SupportedActiveModules.Contains(module.Type))
                    throw new InvalidOperationException($"{spec.Id}: active module '{module.Type}' is not in this patch's runtime-safe whitelist.");
                if (BuildingModuleEditorCatalog.Find(module.Type) == null)
                    throw new InvalidOperationException($"{spec.Id}: active module '{module.Type}' is missing from BuildingModuleEditorCatalog.");
            }
            foreach (string remove in spec.RemoveModules)
                if (string.IsNullOrWhiteSpace(remove)) throw new InvalidOperationException($"{spec.Id}: empty modules.remove entry.");
        }

        public static void ValidateRuntimeAsset(BuildingDefinitionAsset asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            IReadOnlyList<BuildingValidationIssue> issues = BuildingValidator.Validate(asset.ToRuntimeDefinition());
            var errors = new List<string>();
            for (int i=0;i<issues.Count;i++)
            {
                BuildingValidationIssue issue = issues[i];
                if (issue != null && issue.Severity == BuildingValidationSeverity.Error) errors.Add($"{issue.Code}: {issue.Message}");
            }
            if (errors.Count > 0) throw new InvalidOperationException($"{asset.Id}: BuildingValidator errors: {string.Join(" | ", errors)}");
        }
    }
}
