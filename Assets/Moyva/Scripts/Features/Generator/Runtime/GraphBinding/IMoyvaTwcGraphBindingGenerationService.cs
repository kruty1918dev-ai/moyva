namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMoyvaTwcGraphBindingGenerationService
    {
        bool GenerateFromGraph(IMoyvaTwcGraphBindingContext context);
        bool GenerateFromGraph(IMoyvaTwcGraphBindingContext context, int seed);
    }
}
