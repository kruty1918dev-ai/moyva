namespace Kruty1918.Moyva.Construction.API
{
    internal sealed class BuildingModuleCapabilitySnapshot
    {
        public BuildingModuleCapabilitySnapshot(BuildingDefinition definition)
        {
            BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out ProductionBuildingModule production);
            BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out StorageBuildingModule storage);
            BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out FogRevealBuildingModule fogReveal);
            BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out TileRequirementBuildingModule tileRequirement);

            HasTownHall = BuildingDefinitionCapabilities.HasEnabledModule<TownHallBuildingModule>(definition);
            HasHousing = BuildingDefinitionCapabilities.HasEnabledModule<HousingBuildingModule>(definition);
            HasWorkerless = BuildingDefinitionCapabilities.HasEnabledModule<WorkerlessBuildingModule>(definition);
            HasWall = BuildingDefinitionCapabilities.HasEnabledModule<WallBuildingModule>(definition);
            HasGate = BuildingDefinitionCapabilities.HasEnabledModule<GateBuildingModule>(definition);
            HasProduction = BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out production);
            HasStorage = BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out storage);
            HasFogReveal = BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out fogReveal);
            HasTileRequirement = BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out tileRequirement);
            Production = production;
            Storage = storage;
            FogReveal = fogReveal;
            TileRequirement = tileRequirement;
        }

        public bool HasTownHall { get; }
        public bool HasHousing { get; }
        public bool HasWorkerless { get; }
        public bool HasWall { get; }
        public bool HasGate { get; }
        public bool HasProduction { get; }
        public bool HasStorage { get; }
        public bool HasFogReveal { get; }
        public bool HasTileRequirement { get; }
        public ProductionBuildingModule Production { get; }
        public StorageBuildingModule Storage { get; }
        public FogRevealBuildingModule FogReveal { get; }
        public TileRequirementBuildingModule TileRequirement { get; }

        public bool HasWorkerlessSemantics => HasWorkerless || HasWall || HasGate;
    }
}
