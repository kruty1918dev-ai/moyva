namespace Kruty1918.Moyva.GraphSystem.API
{
    /// <summary>
    /// Marker for typed execution data that is not exposed as a connectable output port.
    /// </summary>
    public interface INodeExecutionArtifact
    {
    }

    /// <summary>
    /// Позначає явний термінальний вузол, що визначає авторитетний результат scope.
    /// </summary>
    public interface IGraphOutputNode
    {
    }

    /// <summary>
    /// Marks a node that intentionally bridges data between graph layers.
    /// </summary>
    public interface IGraphLayerReferenceNode
    {
        string SourceLayerId { get; }
    }

    /// <summary>
    /// Defines explicit compatibility for a legacy or feature-owned input contract.
    /// </summary>
    public interface IGraphConnectionCompatibility
    {
        bool AcceptsConnection(PortDefinition sourcePort, int targetPortIndex);
    }

    /// <summary>
    /// Allows GraphRunner to publish only a finalized layer mask to LayerMaskRegistry.
    /// </summary>
    public interface ILayerMaskArtifact : INodeExecutionArtifact
    {
        bool[,] LayerMask { get; }
    }
}
