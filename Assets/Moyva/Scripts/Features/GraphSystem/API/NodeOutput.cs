namespace Kruty1918.Moyva.GraphSystem.API
{
    public enum NodeStatus
    {
        Success,
        Warning,
        Error
    }

    public sealed class NodeOutput
    {
        public object[] Values { get; }
        public NodeStatus Status { get; }
        public string Message { get; }

        private NodeOutput(object[] values, NodeStatus status, string message)
        {
            Values = values;
            Status = status;
            Message = message;
        }

        public static NodeOutput Success(params object[] values) =>
            new(values, NodeStatus.Success, null);

        public static NodeOutput Warning(string message, params object[] values) =>
            new(values, NodeStatus.Warning, message);

        public static NodeOutput Error(string message) =>
            new(null, NodeStatus.Error, message);
    }
}
