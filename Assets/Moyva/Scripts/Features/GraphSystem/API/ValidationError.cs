namespace Kruty1918.Moyva.GraphSystem.API
{
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public sealed class ValidationError
    {
        public string NodeId { get; }
        public string Message { get; }
        public ValidationSeverity Severity { get; }

        public ValidationError(string nodeId, string message,
            ValidationSeverity severity = ValidationSeverity.Error)
        {
            NodeId = nodeId;
            Message = message;
            Severity = severity;
        }

        public override string ToString() =>
            $"[{Severity}] Node {NodeId}: {Message}";
    }
}
