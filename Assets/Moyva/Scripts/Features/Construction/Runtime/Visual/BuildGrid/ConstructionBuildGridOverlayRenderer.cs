using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridOverlayRenderer : IConstructionBuildGridOverlayRenderer
    {
        private const int BuildGridRenderQueue = 3990;
        private static readonly int EdgeMaskPropertyId = Shader.PropertyToID("_EdgeMask");
        private static readonly int LineColorPropertyId = Shader.PropertyToID("_LineColor");
        private static readonly int FillColorPropertyId = Shader.PropertyToID("_FillColor");
        private static readonly int ValidLineColorPropertyId = Shader.PropertyToID("_ValidLineColor");
        private static readonly int ValidFillColorPropertyId = Shader.PropertyToID("_ValidFillColor");
        private static readonly int InvalidLineColorPropertyId = Shader.PropertyToID("_InvalidLineColor");
        private static readonly int InvalidFillColorPropertyId = Shader.PropertyToID("_InvalidFillColor");
        private static readonly int GridOriginXZPropertyId = Shader.PropertyToID("_GridOriginXZ");
        private static readonly int CellSizeXZPropertyId = Shader.PropertyToID("_CellSizeXZ");
        private static readonly int UseCellMaskPropertyId = Shader.PropertyToID("_UseCellMask");
        private static readonly int SurfaceLiftPropertyId = Shader.PropertyToID("_SurfaceLift");
        private static readonly int MinUpNormalYPropertyId = Shader.PropertyToID("_MinUpNormalY");

        private readonly IConstructionGridGeometryService _gridGeometry;

        private GameObject _overlayGo;
        private Material _material;
        private MaterialPropertyBlock _propertyBlock;
        private Color _generalLineColor;
        private Color _generalFillColor;
        private Color _validLineColor;
        private Color _validFillColor;
        private Color _invalidLineColor;
        private Color _invalidFillColor;

        public bool MaterialReady => _material != null;

        [Inject]
        public ConstructionBuildGridOverlayRenderer(
            [InjectOptional] IConstructionGridGeometryService gridGeometry = null)
        {
            _gridGeometry = gridGeometry;
        }

        public void Initialize(Transform parent, string shaderName)
        {
            Transform existing = parent.Find("ConstructionBuildGridOverlay");
            if (existing != null)
                Object.Destroy(existing.gameObject);

            _overlayGo = new GameObject("ConstructionBuildGridOverlay");
            _overlayGo.transform.SetParent(parent, false);
            _overlayGo.SetActive(false);

            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"[ConstructionVisual] Shader '{shaderName}' not found. Construction build grid overlay is disabled.");
                return;
            }

            _material = new Material(shader)
            {
                name = "ConstructionBuildGridOverlay_Material",
                renderQueue = BuildGridRenderQueue
            };

            ApplySharedGridProperties();
        }

        public void ApplyStyle(Color lineColor, Color fillColor, float lineWidth)
        {
            if (_material == null)
                return;

            ApplySharedGridProperties();
            _generalLineColor = lineColor;
            _generalFillColor = fillColor;
            _validLineColor = new Color(0.28f, 1f, 0.42f, lineColor.a);
            _validFillColor = new Color(0.20f, 0.82f, 0.32f, fillColor.a);
            _invalidLineColor = new Color(1f, 0.26f, 0.22f, lineColor.a);
            _invalidFillColor = new Color(0.92f, 0.12f, 0.10f, fillColor.a);

            _material.SetColor(LineColorPropertyId, _generalLineColor);
            _material.SetColor(FillColorPropertyId, _generalFillColor);
            _material.SetColor(ValidLineColorPropertyId, _validLineColor);
            _material.SetColor(ValidFillColorPropertyId, _validFillColor);
            _material.SetColor(InvalidLineColorPropertyId, _invalidLineColor);
            _material.SetColor(InvalidFillColorPropertyId, _invalidFillColor);
            _material.SetFloat("_LineWidth", lineWidth);
        }

        public void SetVisible(bool visible)
        {
            if (_overlayGo != null)
                _overlayGo.SetActive(visible);
        }

        public void Draw(List<ConstructionBuildGridOverlayEntry> entries, IConstructionBuildGridDiagnostics diagnostics)
        {
            if (_material == null || entries.Count == 0)
                return;

            int prunedCount = 0;
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                ConstructionBuildGridOverlayEntry entry = entries[i];
                if (entry.Mesh == null)
                {
                    entries.RemoveAt(i);
                    prunedCount++;
                    continue;
                }

                if (entry.SourceRenderer != null && !entry.SourceRenderer.enabled)
                {
                    entries.RemoveAt(i);
                    prunedCount++;
                    continue;
                }

                DrawEntry(entry);
            }

            diagnostics.LogEntriesPruned(prunedCount, entries.Count);
        }

        private void DrawEntry(ConstructionBuildGridOverlayEntry entry)
        {
            _propertyBlock ??= new MaterialPropertyBlock();
            _propertyBlock.Clear();
            _propertyBlock.SetVector(EdgeMaskPropertyId, entry.EdgeMask);
            ResolveEntryColors(entry.VisualState, out Color lineColor, out Color fillColor);
            _propertyBlock.SetColor(LineColorPropertyId, lineColor);
            _propertyBlock.SetColor(FillColorPropertyId, fillColor);
            Graphics.DrawMesh(entry.Mesh, entry.Matrix, _material, entry.Layer, null, 0, _propertyBlock);
        }

        private void ResolveEntryColors(
            ConstructionBuildGridTileVisualState visualState,
            out Color lineColor,
            out Color fillColor)
        {
            switch (visualState)
            {
                case ConstructionBuildGridTileVisualState.Valid:
                    lineColor = _validLineColor;
                    fillColor = _validFillColor;
                    return;
                case ConstructionBuildGridTileVisualState.Invalid:
                    lineColor = _invalidLineColor;
                    fillColor = _invalidFillColor;
                    return;
                default:
                    lineColor = _generalLineColor;
                    fillColor = _generalFillColor;
                    return;
            }
        }

        private void ApplySharedGridProperties()
        {
            if (_material == null)
                return;

            if (_gridGeometry != null
                && _gridGeometry.TryGetCellSize(out Vector2 cellSize)
                && _gridGeometry.TryGetCellCenter(Vector2Int.zero, out Vector3 center))
            {
                _material.SetVector(GridOriginXZPropertyId, new Vector4(
                    center.x - cellSize.x * 0.5f,
                    center.z - cellSize.y * 0.5f,
                    0f,
                    0f));
                _material.SetVector(CellSizeXZPropertyId, new Vector4(cellSize.x, cellSize.y, 0f, 0f));
            }

            _material.SetFloat(UseCellMaskPropertyId, 0f);
            _material.SetFloat(SurfaceLiftPropertyId, 0f);
            _material.SetFloat(MinUpNormalYPropertyId, 0.2f);
        }
    }
}
