namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMapVisualWorldSignalPublisher
    {
        void Publish(GeneratedWorldData worldData, string source);
        void PublishSavedSpawns(GeneratedWorldData worldData, long startupSequence, string startupSessionId);
    }
}
