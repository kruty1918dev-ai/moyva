using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    public static class BuildingValidator
    {
        private static readonly BuildingValidationRunner DefinitionRunner = new BuildingValidationRunner(
            new IBuildingDefinitionValidator[]
            {
                new BuildingIdentityValidator(),
                new BuildingPresentationValidator(),
                new BuildingConstructionValidator(),
                new BuildingPlacementValidator(),
                new BuildingRuntimeStatsValidator(),
                new BuildingRegistryInclusionValidator(),
                new BuildingResourceReferenceValidator(),
            });

        private static readonly BuildingRegistryValidationRunner RegistryRunner = new BuildingRegistryValidationRunner(DefinitionRunner);

        public static IReadOnlyList<BuildingValidationIssue> Validate(
            BuildingDefinition definition,
            BuildingValidationContext context = null)
            => DefinitionRunner.Validate(definition, context);

        public static IReadOnlyList<BuildingValidationIssue> ValidateRegistry(
            IBuildingRegistry registry,
            ISet<string> resourceIds = null)
            => RegistryRunner.Validate(registry, resourceIds);

        public static bool HasErrors(IReadOnlyList<BuildingValidationIssue> issues)
            => BuildingValidationIssueInspector.HasErrors(issues);
    }
}
