using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.Editor.Presets
{
    internal sealed class BuildingPresetPackSpec
    {
        public string PackId;
        public int Version;
        public int RequiresSchemaVersion;
        public bool ApplyProposedModules;
        public readonly List<string> PresetIds = new List<string>();
        public readonly Dictionary<string, string> ResourceAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    internal sealed class BuildingPresetSpec
    {
        public string Id;
        public int Version;
        public bool HasDisplayName;
        public string DisplayName;
        public bool HasCategory;
        public string Category;
        public bool HasRole;
        public string Role;
        public bool HasBuildTurns;
        public int BuildTurns;
        public bool HasMaxHp;
        public int MaxHp;
        public readonly List<BuildingPresetCostSpec> Costs = new List<BuildingPresetCostSpec>();
        public BuildingPresetObjectRefSpec Prefab;
        public BuildingPresetObjectRefSpec Icon;
        public string ModuleMode = "merge";
        public readonly List<BuildingPresetModuleSpec> ActiveModules = new List<BuildingPresetModuleSpec>();
        public readonly List<string> RemoveModules = new List<string>();
        public readonly List<BuildingPresetModuleSpec> ConditionalModules = new List<BuildingPresetModuleSpec>();
        public readonly List<BuildingPresetModuleSpec> DeferredModules = new List<BuildingPresetModuleSpec>();
    }

    internal sealed class BuildingPresetCostSpec
    {
        public string Resource;
        public int Amount;
    }

    internal sealed class BuildingPresetObjectRefSpec
    {
        public string Path;
        public string Root = "Assets";
        public bool Required;
        public readonly List<string> Exact = new List<string>();
        public readonly List<string> Search = new List<string>();
        public readonly List<string> Fallback = new List<string>();
    }

    internal sealed class BuildingPresetModuleSpec
    {
        public string Type;
        public readonly Dictionary<string, object> Data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }

    internal sealed class ResolvedBuildingPreset
    {
        public BuildingPresetSpec Spec;
        public UnityEngine.GameObject Prefab;
        public UnityEngine.Sprite Icon;
        public readonly List<ResolvedCost> Costs = new List<ResolvedCost>();
    }

    internal sealed class ResolvedCost
    {
        public string ResourceId;
        public int Amount;
    }

    public sealed class BuildingPresetBatchResult
    {
        public string PackId;
        public int PresetCount;
        public int ChangedCount;
        public bool ValidateOnly;
        public string Summary;
    }

    internal static class BuildingPresetPaths
    {
        public const string PresetRoot = "Assets/Moyva/Presets/Buildings";
        public const string PackManifestPath = PresetRoot + "/preset-pack.json";
        public const string IconRoot = "Assets/Moyva/Art/UI/Icons/Buildings";
        public const string DefinitionRoot = "Assets/Moyva/Data/ScriptableObjects/Construction/Buildings";
        public const string ExpectedPackId = "moyva.base-buildings.2026-08";
    }
}
