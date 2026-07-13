
namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionVisualSettingsProvider
    {
        int BuildingLayerMinSortingOrder { get; }
        float BuildingSurfaceOffsetY { get; }
        float PreviewSurfaceOffsetY { get; }
        float GhostAlpha { get; }
        float BlockedFlashDurationSeconds { get; }
        float PreviewDragFollowSharpness { get; }
        UnityEngine.Vector2 PreviewDragCursorOffsetXZ { get; }
        bool ShowSnapTargetHighlight { get; }
        UnityEngine.Color SnapTargetHighlightLineColor { get; }
        UnityEngine.Color SnapTargetHighlightFillColor { get; }
        float SnapTargetHighlightLineWidthNormalized { get; }
        float SnapTargetHighlightInsetNormalized { get; }
        float SnapTargetHighlightSurfaceOffsetY { get; }
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
        float BuildGridUpdateBudgetMilliseconds { get; }
        string BuildGridShaderName { get; }
        string PreviewRootName { get; }
        string PlacedRootName { get; }
        string RadiusRootName { get; }
    }
}
