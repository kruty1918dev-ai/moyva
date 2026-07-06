namespace Kruty1918.Moyva.MapChunks.API
{
    public interface IMapFogChunkCoverageService
    {
        bool IsChunkFullyHidden(MapChunkDescriptor descriptor);
    }
}
