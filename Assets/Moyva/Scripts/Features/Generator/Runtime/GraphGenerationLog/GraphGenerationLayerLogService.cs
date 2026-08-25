using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphGenerationLayerLogService : IGraphGenerationLayerLogService
    {
        private readonly IGraphGenerationLayerReportBuilder _builder;

        public GraphGenerationLayerLogService(IGraphGenerationLayerReportBuilder builder)
        {
            _builder = builder;
        }

        public void Emit(GraphGenerationLayerLogRequest request)
        {
        }

        public string Build(GraphGenerationLayerLogRequest request)
        {
            return _builder.Build(request);
        }
    }
}
