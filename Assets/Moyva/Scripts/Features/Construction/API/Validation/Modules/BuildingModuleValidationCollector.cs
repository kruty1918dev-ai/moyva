using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    internal sealed class BuildingModuleValidationCollector
    {
        private readonly List<BuildingValidationIssue> _issues = new List<BuildingValidationIssue>();
        private readonly BuildingDefinition _definition;

        public BuildingModuleValidationCollector(BuildingDefinition definition)
        {
            _definition = definition;
        }

        public IReadOnlyList<BuildingValidationIssue> Issues => _issues;

        public string BuildingLabel
            => !string.IsNullOrWhiteSpace(_definition?.Id) ? _definition.Id : "<unnamed-building>";

        public void AddError(string code, string message)
        {
            AddIssue(BuildingValidationSeverity.Error, code, message);
        }

        public void AddWarning(string code, string message)
        {
            AddIssue(BuildingValidationSeverity.Warning, code, message);
        }

        private void AddIssue(BuildingValidationSeverity severity, string code, string message)
        {
            _issues.Add(new BuildingValidationIssue
            {
                Severity = severity,
                Code = code,
                Message = message,
            });
        }
    }
}
