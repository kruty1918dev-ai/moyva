using UnityEngine;

namespace Kruty1918.Moyva.Economy.Editor
{
    public enum EconomyValidationSeverity
    {
        Warning = 0,
        Error = 1,
    }

    public readonly struct EconomyValidationIssue
    {
        public EconomyValidationIssue(EconomyValidationSeverity severity, string message, Object context = null)
        {
            Severity = severity;
            Message = message;
            Context = context;
        }

        public EconomyValidationSeverity Severity { get; }
        public string Message { get; }
        public Object Context { get; }
    }
}
