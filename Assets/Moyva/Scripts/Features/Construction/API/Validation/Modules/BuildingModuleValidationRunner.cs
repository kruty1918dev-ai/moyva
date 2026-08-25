using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    internal interface IBuildingModuleValidator
    {
        void Validate(BuildingModuleValidationContext context);
    }

    internal sealed class BuildingModuleValidationRunner
    {
        private const string LogTag = "[BuildingModuleValidation]";

        private readonly IReadOnlyList<IBuildingModuleValidator> _validators;

        public BuildingModuleValidationRunner(IReadOnlyList<IBuildingModuleValidator> validators)
        {
            _validators = validators ?? Array.Empty<IBuildingModuleValidator>();
        }

        public IReadOnlyList<BuildingValidationIssue> Validate(BuildingDefinition definition)
        {
            var collector = new BuildingModuleValidationCollector(definition);
            if (definition == null)
            {
                return collector.Issues;
            }

            if (definition.Modules == null || definition.Modules.Count == 0)
            {
                return collector.Issues;
            }

            var context = new BuildingModuleValidationContext(definition, collector);

            for (int i = 0; i < _validators.Count; i++)
                _validators[i]?.Validate(context);

            collector.LogSummary();
            return collector.Issues;
        }
    }

    internal sealed class BuildingModuleValidationContext
    {
        public BuildingModuleValidationContext(
            BuildingDefinition definition,
            BuildingModuleValidationCollector collector)
        {
            Definition = definition;
            Collector = collector;
            Snapshot = new BuildingModuleCapabilitySnapshot(definition);
        }

        public BuildingDefinition Definition { get; }
        public BuildingModuleValidationCollector Collector { get; }
        public BuildingModuleCapabilitySnapshot Snapshot { get; }
    }
}
