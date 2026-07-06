namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionTileSurfaceOffsetService
    {
        bool TryResolveTileSurfaceOffsetY(string tileId, out float offsetY);
    }
}
