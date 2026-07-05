using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    internal sealed class BuildingValidationCollector
    {
        private const string LogTag = "[BuildingValidator]";

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
            Debug.LogError($"{LogTag} [{BuildingLabel}] {code}: {message}");
        }

        public void AddWarning(string code, string message)
        {
            _issues.Add(new BuildingValidationIssue
            {
                Severity = BuildingValidationSeverity.Warning,
                Code = code,
                Message = message,
            });
            Debug.LogWarning($"{LogTag} [{BuildingLabel}] {code}: {message}");
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

        public void LogSummary(string scope = null)
        {
            int errorCount = 0;
            int warningCount = 0;
            for (int i = 0; i < _issues.Count; i++)
            {
                if (_issues[i] == null)
                    continue;

                if (_issues[i].Severity == BuildingValidationSeverity.Error)
                    errorCount++;
                else if (_issues[i].Severity == BuildingValidationSeverity.Warning)
                    warningCount++;
            }

            string label = string.IsNullOrWhiteSpace(scope) ? BuildingLabel : scope;
            Debug.Log($"{LogTag} Validation finished for '{label}'. issues={_issues.Count}, errors={errorCount}, warnings={warningCount}.");
        }
    }
}
