using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    internal sealed class BuildingModuleValidationCollector
    {
        private const string LogTag = "[BuildingModuleValidation]";

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
            Debug.LogError($"{LogTag} [{BuildingLabel}] {code}: {message}");
        }

        public void AddWarning(string code, string message)
        {
            AddIssue(BuildingValidationSeverity.Warning, code, message);
            Debug.LogWarning($"{LogTag} [{BuildingLabel}] {code}: {message}");
        }

        public void LogSummary()
        {
            int warningCount = 0;
            int errorCount = 0;
            for (int i = 0; i < _issues.Count; i++)
            {
                if (_issues[i] == null)
                    continue;

                if (_issues[i].Severity == BuildingValidationSeverity.Error)
                    errorCount++;
                else if (_issues[i].Severity == BuildingValidationSeverity.Warning)
                    warningCount++;
            }

            Debug.Log($"{LogTag} Validation finished for '{BuildingLabel}'. issues={_issues.Count}, errors={errorCount}, warnings={warningCount}.");
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
