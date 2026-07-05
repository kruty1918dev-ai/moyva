using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    internal static class BuildingValidationIssueInspector
    {
        public static bool HasErrors(IReadOnlyList<BuildingValidationIssue> issues)
        {
            if (issues == null)
                return false;

            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i] != null && issues[i].Severity == BuildingValidationSeverity.Error)
                    return true;
            }

            return false;
        }
    }
}
