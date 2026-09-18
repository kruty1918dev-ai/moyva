using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    internal sealed class BuildingValidationCollector
    {
        private readonly BuildingDefinition _definition;
        private readonly List<BuildingValidationIssue> _issues = new List<BuildingValidationIssue>();

        public BuildingValidationCollector(BuildingDefinition definition)
        {
            _definition = definition;
        }

        public IReadOnlyList<BuildingValidationIssue> Issues => _issues;

        public string BuildingLabel
            => !string.IsNullOrWhiteSpace(_definition?.Id) ? _definition.Id : "<unnamed-building>";

        public void AddError(string code, string message)
        {
            _issues.Add(new BuildingValidationIssue
            {
                Severity = BuildingValidationSeverity.Error,
                Code = code,
                Message = message,
            });
        }

        public void AddWarning(string code, string message)
        {
            _issues.Add(new BuildingValidationIssue
            {
                Severity = BuildingValidationSeverity.Warning,
                Code = code,
                Message = message,
            });
        }

        public void ImportIssues(IReadOnlyList<BuildingValidationIssue> issues)
        {
            if (issues == null)
                return;

            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i] != null)
                    _issues.Add(issues[i]);
            }
        }

    }
}
