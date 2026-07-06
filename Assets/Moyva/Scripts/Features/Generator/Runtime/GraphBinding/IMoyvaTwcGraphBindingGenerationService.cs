namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMoyvaTwcGraphBindingGenerationService
    {
        void GenerateFromGraph(IMoyvaTwcGraphBindingContext context);
        void GenerateFromGraph(IMoyvaTwcGraphBindingContext context, int seed);
    }
}
