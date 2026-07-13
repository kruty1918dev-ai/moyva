
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [CreateAssetMenu(menuName = "Moyva/Construction/Profiles/Visual", fileName = "ConstructionVisualProfile")]
    public sealed class ConstructionVisualProfileSO : ScriptableObject
    {
        [BoxGroup("Sorting")]
        [MinValue(0)]
        [SerializeField] private int _buildingLayerMinSortingOrder = 5;

        [BoxGroup("Placement")]
        [SerializeField] private float _buildingSurfaceOffsetY = 0.5f;

        [BoxGroup("Placement")]
        [SerializeField] private float _previewSurfaceOffsetY = 0.7f;

        [BoxGroup("Preview")]
        [Range(0f, 1f)]
        [SerializeField] private float _ghostAlpha = 0.55f;

        [BoxGroup("Preview")]
        [MinValue(0f)]
        [SerializeField] private float _blockedFlashDurationSeconds = 0.35f;

        [BoxGroup("Preview Drag")]
        [MinValue(0.01f)]
        [SerializeField] private float _previewDragFollowSharpness = 28f;

        [BoxGroup("Preview Drag")]
        [SerializeField] private Vector2 _previewDragCursorOffsetXZ = Vector2.zero;

        [BoxGroup("Preview Drag")]
        [SerializeField] private bool _showSnapTargetHighlight = true;

        [BoxGroup("Preview Drag")]
        [SerializeField] private Color _snapTargetHighlightLineColor = new Color(1f, 0.58f, 0.12f, 0.9f);

        [BoxGroup("Preview Drag")]
        [SerializeField] private Color _snapTargetHighlightFillColor = new Color(1f, 0.58f, 0.12f, 0.16f);

        [BoxGroup("Preview Drag")]
        [Range(0.005f, 0.49f)]
        [SerializeField] private float _snapTargetHighlightLineWidthNormalized = 0.06f;

        [BoxGroup("Preview Drag")]
        [Range(0f, 0.45f)]
        [SerializeField] private float _snapTargetHighlightInsetNormalized = 0.04f;

        [BoxGroup("Preview Drag")]
        [MinValue(0f)]
        [SerializeField] private float _snapTargetHighlightSurfaceOffsetY = 0.16f;

        [BoxGroup("Build Grid")]
        [SerializeField] private bool _useBuildGridOverlay = true;

        [BoxGroup("Build Grid")]
        [SerializeField] private ConstructionBuildGridRenderMode _buildGridRenderMode =
            ConstructionBuildGridRenderMode.LegacyDrawMeshPerTile;

        [BoxGroup("Build Grid")]
        [SerializeField] private bool _buildGridSurfacePlaneUseBuildableFilter = false;

        [BoxGroup("Build Grid")]
        [Range(0f, 1f)]
        [SerializeField] private float _buildGridFillAlpha = 0.045f;

        [BoxGroup("Build Grid")]
        [Range(0f, 1f)]
        [SerializeField] private float _buildGridLineAlpha = 0.22f;

        [BoxGroup("Build Grid")]
        [Range(0.005f, 0.49f)]
        [SerializeField] private float _buildGridLineWidthNormalized = 0.035f;

        [BoxGroup("Build Grid")]
        [MinValue(0f)]
        [SerializeField] private float _buildGridSurfaceOffsetY = 0.06f;

        [BoxGroup("Build Grid")]
        [Range(0f, 0.45f)]
        [SerializeField] private float _buildGridTileInsetNormalized = 0.08f;

        [BoxGroup("Build Grid")]
        [MinValue(0.1f)]
        [SerializeField] private float _buildGridUpdateBudgetMilliseconds = 2f;

        [BoxGroup("Influence Radius")]
        [SerializeField] private bool _useInfluenceRadiusOverlay = true;

        [BoxGroup("Influence Radius")]
        [Range(0f, 1f)]
        [SerializeField] private float _influenceRadiusFillAlpha = 0.055f;

        [BoxGroup("Influence Radius")]
        [MinValue(0f)]
        [SerializeField] private float _influenceRadiusBorderWidth = 0.5f;

        [BoxGroup("Shaders")]
        [SerializeField] private string _influenceRadiusShaderName2D = "Moyva/2D/InfluenceRadius";

        [BoxGroup("Shaders")]
        [SerializeField] private string _influenceRadiusShaderName3D = "Moyva/3D/InfluenceRadiusExistingMeshOverlay";

        [BoxGroup("Shaders")]
        [SerializeField] private string _buildGridShaderName = "Moyva/Overlay/ConstructionBuildGrid";

        [BoxGroup("Roots")]
        [SerializeField] private string _previewRootName = "ConstructionPreviewRoot";

        [BoxGroup("Roots")]
        [SerializeField] private string _placedRootName = "PlayerBuildingsRoot";

        [BoxGroup("Roots")]
        [SerializeField] private string _radiusRootName = "ConstructionRadiusRoot";

        public int BuildingLayerMinSortingOrder => Mathf.Max(0, _buildingLayerMinSortingOrder);
        public float BuildingSurfaceOffsetY => _buildingSurfaceOffsetY;
        public float PreviewSurfaceOffsetY => _previewSurfaceOffsetY;
        public float GhostAlpha => Mathf.Clamp01(_ghostAlpha);
        public float BlockedFlashDurationSeconds => Mathf.Max(0f, _blockedFlashDurationSeconds);
        public float PreviewDragFollowSharpness => Mathf.Max(0.01f, _previewDragFollowSharpness);
        public Vector2 PreviewDragCursorOffsetXZ => _previewDragCursorOffsetXZ;
        public bool ShowSnapTargetHighlight => _showSnapTargetHighlight;
        public Color SnapTargetHighlightLineColor => _snapTargetHighlightLineColor;
        public Color SnapTargetHighlightFillColor => _snapTargetHighlightFillColor;
        public float SnapTargetHighlightLineWidthNormalized => Mathf.Clamp(_snapTargetHighlightLineWidthNormalized, 0.005f, 0.49f);
        public float SnapTargetHighlightInsetNormalized => Mathf.Clamp(_snapTargetHighlightInsetNormalized, 0f, 0.45f);
        public float SnapTargetHighlightSurfaceOffsetY => Mathf.Max(0f, _snapTargetHighlightSurfaceOffsetY);
        public bool UseBuildGridOverlay => _useBuildGridOverlay;
        public ConstructionBuildGridRenderMode BuildGridRenderMode => _buildGridRenderMode;
        public bool BuildGridSurfacePlaneUseBuildableFilter => _buildGridSurfacePlaneUseBuildableFilter;
        public float BuildGridFillAlpha => Mathf.Clamp01(_buildGridFillAlpha);
        public float BuildGridLineAlpha => Mathf.Clamp01(_buildGridLineAlpha);
        public float BuildGridLineWidthNormalized => Mathf.Clamp(_buildGridLineWidthNormalized, 0.005f, 0.49f);
        public float BuildGridSurfaceOffsetY => Mathf.Max(0f, _buildGridSurfaceOffsetY);
        public float BuildGridTileInsetNormalized => Mathf.Clamp(_buildGridTileInsetNormalized, 0f, 0.45f);
        public float BuildGridUpdateBudgetMilliseconds => Mathf.Max(0.1f, _buildGridUpdateBudgetMilliseconds);
        public bool UseInfluenceRadiusOverlay => _useInfluenceRadiusOverlay;
        public float InfluenceRadiusFillAlpha => Mathf.Clamp01(_influenceRadiusFillAlpha);
        public float InfluenceRadiusBorderWidth => Mathf.Max(0f, _influenceRadiusBorderWidth);
        public string InfluenceRadiusShaderName2D => string.IsNullOrWhiteSpace(_influenceRadiusShaderName2D) ? "Moyva/2D/InfluenceRadius" : _influenceRadiusShaderName2D;
        public string InfluenceRadiusShaderName3D => string.IsNullOrWhiteSpace(_influenceRadiusShaderName3D) ? "Moyva/3D/InfluenceRadiusExistingMeshOverlay" : _influenceRadiusShaderName3D;
        public string BuildGridShaderName => string.IsNullOrWhiteSpace(_buildGridShaderName) ? "Moyva/Overlay/ConstructionBuildGrid" : _buildGridShaderName;
        public string PreviewRootName => string.IsNullOrWhiteSpace(_previewRootName) ? "ConstructionPreviewRoot" : _previewRootName;
        public string PlacedRootName => string.IsNullOrWhiteSpace(_placedRootName) ? "PlayerBuildingsRoot" : _placedRootName;
        public string RadiusRootName => string.IsNullOrWhiteSpace(_radiusRootName) ? "ConstructionRadiusRoot" : _radiusRootName;
    }
}
