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
                Debug.LogWarning($"{LogTag} Validation skipped because BuildingDefinition is null.");
                return collector.Issues;
            }

            if (definition.Modules == null || definition.Modules.Count == 0)
            {
                Debug.Log($"{LogTag} Validation skipped for '{collector.BuildingLabel}': no modules configured.");
                return collector.Issues;
            }

            var context = new BuildingModuleValidationContext(definition, collector);
            Debug.Log($"{LogTag} Validation started for '{collector.BuildingLabel}'. modules={definition.Modules.Count}, validators={_validators.Count}.");

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
