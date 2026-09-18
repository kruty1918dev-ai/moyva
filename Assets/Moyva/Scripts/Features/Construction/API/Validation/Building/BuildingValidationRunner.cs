using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    internal interface IBuildingDefinitionValidator
    {
        void Validate(BuildingDefinitionValidationContext context);
    }

    internal sealed class BuildingValidationRunner
    {
        private readonly IReadOnlyList<IBuildingDefinitionValidator> _validators;

        public BuildingValidationRunner(IReadOnlyList<IBuildingDefinitionValidator> validators)
        {
            _validators = validators ?? Array.Empty<IBuildingDefinitionValidator>();
        }

        public IReadOnlyList<BuildingValidationIssue> Validate(
            BuildingDefinition definition,
            BuildingValidationContext context = null)
        {
            var collector = new BuildingValidationCollector(definition);
            if (definition == null)
            {
                collector.AddError("BUILDING_NULL", "BuildingDefinition відсутній.");
                return collector.Issues;
            }
            var validationContext = new BuildingDefinitionValidationContext(definition, context, collector);

            for (int i = 0; i < _validators.Count; i++)
                _validators[i]?.Validate(validationContext);

            collector.ImportIssues(BuildingModuleValidation.Validate(definition));
            return collector.Issues;
        }
    }

    internal sealed class BuildingDefinitionValidationContext
    {
        public BuildingDefinitionValidationContext(
            BuildingDefinition definition,
            BuildingValidationContext options,
            BuildingValidationCollector collector)
        {
            Definition = definition;
            Options = options;
            Collector = collector;
        }

        public BuildingDefinition Definition { get; }
        public BuildingValidationContext Options { get; }
        public BuildingValidationCollector Collector { get; }
    }
}
