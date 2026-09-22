namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal enum LayerKind
    {
        BaseTerrain = 0,
        OverlayTerrain = 1,
        Road = 2,
        Shore = 3,
        Cliff = 4,
        ObjectSpawn = 5,
        Building = 6,
        Decoration = 7,
        MaskOnly = 8,
        /// <summary>Generated stair corridor cell; not terrain-like, owns its tile id.</summary>
        StairPassage = 9
    }
}
