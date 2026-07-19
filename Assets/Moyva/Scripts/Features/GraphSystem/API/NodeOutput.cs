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
        public object Artifact { get; }
        public NodeStatus Status { get; }
        public string Message { get; }

        private NodeOutput(object[] values, object artifact, NodeStatus status, string message)
        {
            Values = values ?? System.Array.Empty<object>();
            Artifact = artifact;
            Status = status;
            Message = message;
        }

        public static NodeOutput Success(params object[] values) =>
            new(values, null, NodeStatus.Success, null);

        public static NodeOutput SuccessWithArtifact(object artifact, params object[] values) =>
            new(values, artifact, NodeStatus.Success, null);

        public static NodeOutput Warning(string message, params object[] values) =>
            new(values, null, NodeStatus.Warning, message);

        public static NodeOutput WarningWithArtifact(string message, object artifact, params object[] values) =>
            new(values, artifact, NodeStatus.Warning, message);

        public static NodeOutput Error(string message) =>
            new(System.Array.Empty<object>(), null, NodeStatus.Error, message);
    }
}
