
namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionVisualSettingsProvider
    {
        int BuildingLayerMinSortingOrder { get; }
        float BuildingSurfaceOffsetY { get; }
        float PreviewSurfaceOffsetY { get; }
        float GhostAlpha { get; }
        float BlockedFlashDurationSeconds { get; }
        bool UseInfluenceRadiusOverlay { get; }
        float InfluenceRadiusFillAlpha { get; }
        float InfluenceRadiusBorderWidth { get; }
        string InfluenceRadiusShaderName2D { get; }
        string InfluenceRadiusShaderName3D { get; }
        bool UseBuildGridOverlay { get; }
        ConstructionBuildGridRenderMode BuildGridRenderMode { get; }
        bool BuildGridSurfacePlaneUseBuildableFilter { get; }
        float BuildGridFillAlpha { get; }
        float BuildGridLineAlpha { get; }
        float BuildGridLineWidthNormalized { get; }
        float BuildGridSurfaceOffsetY { get; }
        float BuildGridTileInsetNormalized { get; }
        string BuildGridShaderName { get; }
        string PreviewRootName { get; }
        string PlacedRootName { get; }
        string RadiusRootName { get; }
    }
}