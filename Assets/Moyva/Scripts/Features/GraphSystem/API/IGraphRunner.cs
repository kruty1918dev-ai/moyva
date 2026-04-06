using System.Threading.Tasks;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public interface IGraphRunner
    {
        GraphExecutionResult Execute(GraphAsset graph, NodeContext context);
        Task<GraphExecutionResult> ExecuteAsync(GraphAsset graph, NodeContext context);
    }
}
