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
    /// Allows GraphRunner to publish only a finalized layer mask to LayerMaskRegistry.
    /// </summary>
    public interface ILayerMaskArtifact : INodeExecutionArtifact
    {
        bool[,] LayerMask { get; }
    }
}
