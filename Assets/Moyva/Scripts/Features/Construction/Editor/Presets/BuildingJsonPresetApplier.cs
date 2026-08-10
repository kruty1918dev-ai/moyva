using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Editor.Shared;
using UnityEditor;

namespace Kruty1918.Moyva.Construction.Editor.Presets
{
    internal static class BuildingJsonPresetApplier
    {
        public static void ApplyToAsset(ResolvedBuildingPreset resolved, BuildingDefinitionAsset asset, bool preserveId=false)
        {
            if (resolved == null || resolved.Spec == null) throw new ArgumentNullException(nameof(resolved));
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            BuildingPresetSpec spec = resolved.Spec;
            asset.Identity ??= new BuildingIdentity(); asset.Presentation ??= new BuildingPresentation(); asset.Construction ??= new BuildingConstructionData(); asset.RuntimeStats ??= new BuildingRuntimeStats(); asset.Modules ??= new List<BuildingModuleDefinition>();
            if (!preserveId) asset.Identity.Id = spec.Id;
            if (spec.HasDisplayName) asset.Identity.DisplayName = spec.DisplayName;
            if (spec.HasDescription) asset.Identity.Description = spec.Description;
            if (spec.HasCategory && Enum.TryParse(spec.Category, true, out BuildingCategory category)) asset.Identity.Category = category;
            else if (spec.HasCategory) throw new InvalidOperationException($"{spec.Id}: unknown category '{spec.Category}'.");
            if (spec.HasRole && Enum.TryParse(spec.Role, true, out BuildingRole role)) asset.Identity.Role = role;
            else if (spec.HasRole) throw new InvalidOperationException($"{spec.Id}: unknown role '{spec.Role}'.");
            if (spec.HasBuildTurns) asset.Construction.BuildTurns = spec.BuildTurns;
            if (spec.Costs.Count > 0)
            {
                asset.Construction.Cost = new List<BuildingDefinition.BuildingConstructionCostEntry>();
                foreach (ResolvedCost cost in resolved.Costs)
                    asset.Construction.Cost.Add(new BuildingDefinition.BuildingConstructionCostEntry { ResourceId = cost.ResourceId, Amount = cost.Amount });
            }
            if (spec.HasMaxHp) asset.RuntimeStats.MaxHp = spec.MaxHp;
            if (resolved.Prefab != null || spec.Prefab?.Required == true) asset.Presentation.Prefab = resolved.Prefab;
            if (resolved.Icon != null || spec.Icon?.Required == true) asset.Presentation.Icon = resolved.Icon;

            if (string.Equals(spec.ModuleMode, "replace", StringComparison.OrdinalIgnoreCase)) asset.Modules.Clear();
            RemoveModules(asset.Modules, spec.RemoveModules);
            foreach (BuildingPresetModuleSpec module in spec.ActiveModules) UpsertModule(asset.Modules, module);
            asset.Normalize(); asset.NotifyEditorDataChanged();
        }

        private static void RemoveModules(List<BuildingModuleDefinition> modules, List<string> typeNames)
        {
            for (int i=modules.Count-1;i>=0;i--)
            {
                BuildingModuleDefinition module=modules[i]; if (module==null) { modules.RemoveAt(i); continue; }
                for (int j=0;j<typeNames.Count;j++) if (string.Equals(module.GetType().Name,typeNames[j],StringComparison.Ordinal)) { modules.RemoveAt(i); break; }
            }
        }
        private static void UpsertModule(List<BuildingModuleDefinition> modules, BuildingPresetModuleSpec spec)
        {
            BuildingModuleEditorDescriptor descriptor = BuildingModuleEditorCatalog.Find(spec.Type);
            if (descriptor == null) throw new InvalidOperationException($"Unknown module type '{spec.Type}'.");
            BuildingModuleDefinition module = null;
            for (int i=0;i<modules.Count;i++) if (modules[i] != null && modules[i].GetType() == descriptor.ModuleType) { module=modules[i]; break; }
            if (module == null)
            {
                string conflict=BuildingModuleEditorCatalog.GetConflictReason(modules, descriptor.ModuleType);
                if (!string.IsNullOrWhiteSpace(conflict)) throw new InvalidOperationException($"Cannot add {spec.Type}: {conflict}");
                module=descriptor.Create(); modules.Add(module);
            }
            module.IsEnabled=true;
            if (module is WorkforceBuildingModule workforce)
            {
                workforce.WorkersRequired=Math.Max(0,BuildingJsonPresetResolver.GetDataInt(spec,"workersRequired"));
            }
            else if (module is ProductionBuildingModule production)
            {
                production.ResourceId=BuildingJsonPresetResolver.GetDataString(spec,"resourceId");
                production.WorkersRequired=0;
                production.Recipes ??= new List<ProductionRecipeDefinition>();
                production.Recipes.Clear();
            }
            else if (module is StorageBuildingModule storage)
            {
                string kind=BuildingJsonPresetResolver.GetDataString(spec,"storageKind") ?? "Any";
                if (!Enum.TryParse(kind,true,out BuildingStorageKind parsedKind)) throw new InvalidOperationException($"Unknown storageKind '{kind}'.");
                storage.StorageKind=parsedKind; storage.Capacity=BuildingJsonPresetResolver.GetDataInt(spec,"capacity",-1); storage.AcceptedResourceIds=Array.Empty<string>();
            }
            else throw new InvalidOperationException($"Active module '{spec.Type}' has no applier in preset system.");
        }
    }
}
