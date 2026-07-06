using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionInfluenceRadiusVisualService : IConstructionInfluenceRadiusVisualService
    {
        private readonly IConstructionVisualRootService _roots;
        private readonly IConstructionTerrainAlignmentService _terrainAlignmentService;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly IConstructionInfluenceMeshOverlayRenderer _meshOverlayRenderer;
        private readonly IConstructionRadiusVisualObjectFactory _objectFactory;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;
        private readonly IGridProjection _gridProjection;
        private readonly ConstructionInfluenceRadiusOverlayState _previewOverlay = new();
        private readonly ConstructionInfluenceRadiusOverlayState _inspectionOverlay = new();

        private ConstructionRadiusVisualHandle _preview;
        private ConstructionRadiusVisualHandle _inspection;

        [Inject]
        public ConstructionInfluenceRadiusVisualService(
            IConstructionVisualRootService roots,
            IConstructionTerrainAlignmentService terrainAlignmentService,
            IConstructionGridGeometryService gridGeometry,
            IConstructionInfluenceMeshOverlayRenderer meshOverlayRenderer,
            IConstructionRadiusVisualObjectFactory objectFactory,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null)
        {
            _roots = roots;
            _terrainAlignmentService = terrainAlignmentService;
            _gridGeometry = gridGeometry;
            _meshOverlayRenderer = meshOverlayRenderer;
            _objectFactory = objectFactory;
            _gridProjection = gridProjection;
            _settingsProvider = settingsProvider;
        }

        public void Initialize()
        {
            Mesh mesh = ConstructionQuadMeshFactory.Create("InfluenceRadiusQuad");
            _preview = _objectFactory.Create("PreviewInfluenceRadius", 30, mesh);
            _inspection = _objectFactory.Create("SelectedInfluenceRadius", 31, mesh);
        }

        public void ShowPreview(Vector2Int center, int radius)
            => Show(_preview, _previewOverlay, center, radius);

        public void HidePreview() => Hide(_preview, _previewOverlay);

        public void ShowInspection(Vector2Int center, int radius)
            => Show(_inspection, _inspectionOverlay, center, radius);

        public void HideInspection() => Hide(_inspection, _inspectionOverlay);

        public void Tick()
        {
            if (!Uses3DWorldPlane())
                return;

            _meshOverlayRenderer.Draw(_previewOverlay);
            _meshOverlayRenderer.Draw(_inspectionOverlay);
        }

        public void Dispose()
        {
            HidePreview();
            HideInspection();
        }

        private void Show(ConstructionRadiusVisualHandle handle, ConstructionInfluenceRadiusOverlayState overlay, Vector2Int center, int radius)
        {
            if (handle?.Material == null || !ResolveUseOverlay())
            {
                Hide(handle, overlay);
                return;
            }

            if (Uses3DWorldPlane())
            {
                handle.Renderer.enabled = false;
                _meshOverlayRenderer.Show(overlay, CreateOverlayRequest(center, radius, handle.Material));
                return;
            }

            float size = radius * 2f + 1f;
            handle.GameObject.transform.position = _terrainAlignmentService.ResolveWorldPosition(center, 0.05f);
            handle.GameObject.transform.rotation = ResolveRotation();
            handle.GameObject.transform.localScale = new Vector3(size, size, 1f);
            ApplyMaterial(handle.Material);
            handle.Renderer.enabled = true;
        }

        private void Hide(ConstructionRadiusVisualHandle handle, ConstructionInfluenceRadiusOverlayState overlay)
        {
            _meshOverlayRenderer.Hide(overlay);
            if (handle?.Renderer != null)
                handle.Renderer.enabled = false;
        }

        private ConstructionInfluenceRadiusOverlayRequest CreateOverlayRequest(Vector2Int center, int radius, Material material)
        {
            Vector3 centerWorld = _terrainAlignmentService.ResolveWorldPosition(center, 0f);
            float cellSize = _gridGeometry != null && _gridGeometry.TryGetCellSize(out Vector2 size)
                ? Mathf.Min(size.x, size.y)
                : 1f;
            return new ConstructionInfluenceRadiusOverlayRequest(
                centerWorld,
                radius,
                cellSize,
                material,
                ResolveFillAlpha(),
                ResolveBorderWidth(),
                _roots.RadiusRoot);
        }

        private Quaternion ResolveRotation()
        {
            return _gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? Quaternion.Euler(90f, 0f, 0f)
                : Quaternion.identity;
        }

        private void ApplyMaterial(Material material)
        {
            material.SetColor("_Color", new Color(1f, 1f, 1f, 0.95f));
            material.SetColor("_FillColor", new Color(1f, 1f, 1f, ResolveFillAlpha()));
            material.SetFloat("_BorderWidth", ResolveBorderWidth());
        }

        private bool Uses3DWorldPlane() => GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection);
        private bool ResolveUseOverlay() => _settingsProvider?.UseInfluenceRadiusOverlay ?? true;
        private float ResolveFillAlpha() => Mathf.Clamp01(_settingsProvider?.InfluenceRadiusFillAlpha ?? 0.055f);
        private float ResolveBorderWidth() => Mathf.Max(0f, _settingsProvider?.InfluenceRadiusBorderWidth ?? 0.5f);
    }
}
