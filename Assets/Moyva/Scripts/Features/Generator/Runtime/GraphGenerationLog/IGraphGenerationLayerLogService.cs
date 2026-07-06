namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphGenerationLayerLogService
    {
        void Emit(GraphGenerationLayerLogRequest request);
        string Build(GraphGenerationLayerLogRequest request);
    }
}
