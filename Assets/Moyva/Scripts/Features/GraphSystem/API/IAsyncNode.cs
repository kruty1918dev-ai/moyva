using System.Threading.Tasks;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public interface IAsyncNode
    {
        Task<NodeOutput> ExecuteAsync(object[] inputs, NodeContext context);
    }
}
