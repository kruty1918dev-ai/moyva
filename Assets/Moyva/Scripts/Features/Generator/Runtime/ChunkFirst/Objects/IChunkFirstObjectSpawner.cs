namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal interface IChunkFirstObjectSpawner
    {
        int Spawn(GeneratedWorldData worldData);
        void Clear();
    }
}
