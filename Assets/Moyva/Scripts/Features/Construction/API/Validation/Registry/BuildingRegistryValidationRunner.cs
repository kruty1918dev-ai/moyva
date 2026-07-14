using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    internal sealed class BuildingRegistryValidationRunner
    {
        private const string LogTag = "[BuildingValidator]";

        private readonly BuildingValidationRunner _definitionRunner;

        public BuildingRegistryValidationRunner(BuildingValidationRunner definitionRunner)
        {
            _definitionRunner = definitionRunner;
        }

        public IReadOnlyList<BuildingValidationIssue> Validate(
            IBuildingRegistry registry,
            ISet<string> resourceIds = null)
        {
            var collector = new BuildingValidationCollector(null);
            if (registry == null)
            {
                collector.AddError("REGISTRY_NULL", "BuildingRegistry не задано.");
                collector.LogSummary("registry");
                return collector.Issues;
            }

            var definitions = registry.GetAll() ?? Array.Empty<BuildingDefinition>();
            Debug.Log($"{LogTag} ValidateRegistry started. definitions={definitions.Length}, knownResources={resourceIds?.Count ?? 0}.");

            var ids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                if (definition == null)
                {
                    collector.AddError("REGISTRY_NULL_ENTRY", $"Запис реєстру [{i}] порожній.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(definition.Id))
                {
                    if (ids.ContainsKey(definition.Id))
                        collector.AddError("REGISTRY_DUPLICATE_ID", $"ID будівлі «{definition.Id}» дублюється.");
                    else
                        ids.Add(definition.Id, i);
                }

                collector.ImportIssues(_definitionRunner.Validate(definition, new BuildingValidationContext
                {
                    Registry = registry,
                    ResourceIds = resourceIds,
                    RequireRegistryInclusion = false,
                }));
            }

            collector.LogSummary("registry");
            return collector.Issues;
        }
    }
}
