namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct TileWorldCreatorClusterStats
    {
        public TileWorldCreatorClusterStats(int rendererComponents, int renderableMeshRenderers, int meshFiltersWithMesh)
        {
            RendererComponents = rendererComponents;
            RenderableMeshRenderers = renderableMeshRenderers;
            MeshFiltersWithMesh = meshFiltersWithMesh;
        }

        public int RendererComponents { get; }
        public int RenderableMeshRenderers { get; }
        public int MeshFiltersWithMesh { get; }
    }
}
