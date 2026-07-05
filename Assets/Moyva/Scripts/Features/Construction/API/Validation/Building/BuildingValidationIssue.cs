using System;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingValidationIssue
    {
        public BuildingValidationSeverity Severity;
        public string Code;
        public string Message;
    }
}
