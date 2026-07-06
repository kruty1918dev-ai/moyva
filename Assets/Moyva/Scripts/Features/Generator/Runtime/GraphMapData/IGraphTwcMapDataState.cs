namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcMapDataState : IGraphTwcMapDataDiagnostics
    {
        void Apply(GraphTwcMapGenerationResult result);
    }
}
