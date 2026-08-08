using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Public facade for building-module validation.
    /// Keeps the old API stable while delegating rule checks to focused validators.
    /// </summary>
    public static class BuildingModuleValidation
    {
        private static readonly BuildingModuleValidationRunner Runner = new BuildingModuleValidationRunner(
            new IBuildingModuleValidator[]
            {
                new BuildingModuleCompatibilityValidator(),
                new BuildingModuleConstructionCostValidator(),
                new BuildingModuleProductionValidator(),
                new BuildingModuleStorageValidator(),
                new BuildingModuleFogRevealValidator(),
                new BuildingModuleTileRequirementValidator(),
                new BuildingModulePerPlayerLimitValidator(),
                new BuildingModuleSingletonValidator(),
            });

        public static IReadOnlyList<BuildingValidationIssue> Validate(BuildingDefinition definition)
            => Runner.Validate(definition);

        public static bool HasErrors(IReadOnlyList<BuildingValidationIssue> issues)
            => BuildingValidationIssueInspector.HasErrors(issues);
    }
}
