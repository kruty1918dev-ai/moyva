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
        public bool UseInfluenceRadiusOverlay => _useInfluenceRadiusOverlay;
        public float InfluenceRadiusFillAlpha => Mathf.Clamp01(_influenceRadiusFillAlpha);
        public float InfluenceRadiusBorderWidth => Mathf.Max(0f, _influenceRadiusBorderWidth);
        public string InfluenceRadiusShaderName2D => string.IsNullOrWhiteSpace(_influenceRadiusShaderName2D) ? "Moyva/2D/InfluenceRadius" : _influenceRadiusShaderName2D;
        public string InfluenceRadiusShaderName3D => string.IsNullOrWhiteSpace(_influenceRadiusShaderName3D) ? "Moyva/3D/InfluenceRadiusExistingMeshOverlay" : _influenceRadiusShaderName3D;
        public string PreviewRootName => string.IsNullOrWhiteSpace(_previewRootName) ? "ConstructionPreviewRoot" : _previewRootName;
        public string PlacedRootName => string.IsNullOrWhiteSpace(_placedRootName) ? "PlayerBuildingsRoot" : _placedRootName;
        public string RadiusRootName => string.IsNullOrWhiteSpace(_radiusRootName) ? "ConstructionRadiusRoot" : _radiusRootName;
    }
}
