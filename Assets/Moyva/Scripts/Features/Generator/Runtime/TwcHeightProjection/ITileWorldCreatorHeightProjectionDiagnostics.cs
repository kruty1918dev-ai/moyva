namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorHeightProjectionDiagnostics
    {
        void LogConfigured(TileWorldCreatorHeightProjectionState state);
        void LogWorldStart(TileWorldCreatorHeightProjectionState state);
        void LogWorldEnd(TileWorldCreatorHeightProjectionState state);
        void LogPass(string message);
        string FormatSamples(System.Collections.Generic.List<string> samples);
    }
}
