using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridOverlayRenderer {
        private const int BuildGridRenderQueue = 3990;
        private const string LateOverlayCommandName = "Moyva Grid Action Overlay";
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
        private static readonly int ZTestPropertyId = Shader.PropertyToID("_ZTest");

        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;

        private GameObject _overlayGo;
        private Material _material;
        private MaterialPropertyBlock _propertyBlock;
        private readonly List<ConstructionBuildGridOverlayEntry> _lateEntries = new();
        private CommandBuffer _lateCommandBuffer;
        private bool _lateRenderingHooked;
        private Color _generalLineColor;
        private Color _generalFillColor;
        private Color _validLineColor;
        private Color _validFillColor;
        private Color _unaffordableLineColor;
        private Color _unaffordableFillColor;
        private Color _invalidLineColor;
        private Color _invalidFillColor;

        public bool MaterialReady => _material != null;

        [Inject]
        public ConstructionBuildGridOverlayRenderer(
            [InjectOptional] IConstructionGridGeometryService gridGeometry = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null)
        {
            _gridGeometry = gridGeometry;
            _settingsProvider = settingsProvider;
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
            _validLineColor = ResolveLineColor(
                _settingsProvider?.BuildGridValidLineColor
                    ?? new Color(0.28f, 1f, 0.42f, 1f),
                lineColor.a);
            _validFillColor = ResolveFillColor(
                _settingsProvider?.BuildGridValidFillColor
                    ?? new Color(0.20f, 0.82f, 0.32f, 1f),
                fillColor.a);
            _unaffordableLineColor = ResolveLineColor(
                _settingsProvider?.BuildGridUnaffordableLineColor
                    ?? new Color(1f, 0.68f, 0.12f, 1f),
                lineColor.a);
            _unaffordableFillColor = ResolveFillColor(
                _settingsProvider?.BuildGridUnaffordableFillColor
                    ?? new Color(0.95f, 0.48f, 0.08f, 1f),
                fillColor.a);
            _invalidLineColor = ResolveLineColor(
                _settingsProvider?.BuildGridInvalidLineColor
                    ?? new Color(1f, 0.26f, 0.22f, 1f),
                lineColor.a);
            _invalidFillColor = ResolveFillColor(
                _settingsProvider?.BuildGridInvalidFillColor
                    ?? new Color(0.92f, 0.12f, 0.10f, 1f),
                fillColor.a);

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

            if (!visible)
                DisableLateRendering();
        }

        public void Draw(
            List<ConstructionBuildGridOverlayEntry> entries,
            bool afterCameraRendering = false)
        {
            if (afterCameraRendering)
            {
                CaptureLateEntries(entries);
                return;
            }

            DisableLateRendering();

            if (_material == null || entries == null || entries.Count == 0)
                return;

            ApplyZTest(CompareFunction.LessEqual);
            PruneInvalidEntries(entries);
            if (entries.Count == 0)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                ConstructionBuildGridOverlayEntry entry = entries[i];
                DrawImmediateEntry(entry);
            }
        }

        public void Dispose()
        {
            DisableLateRendering();
            _lateCommandBuffer?.Release();
            _lateCommandBuffer = null;
        }

        private void CaptureLateEntries(
            List<ConstructionBuildGridOverlayEntry> entries)
        {
            if (_material == null || entries == null || entries.Count == 0)
            {
                DisableLateRendering();
                return;
            }

            PruneInvalidEntries(entries);
            if (entries.Count == 0)
            {
                DisableLateRendering();
                return;
            }

            _lateEntries.Clear();
            _lateEntries.AddRange(entries);
            ApplyZTest(CompareFunction.Always);
            EnsureLateRenderingHook();
        }

        private void EnsureLateRenderingHook()
        {
            if (_lateRenderingHooked)
                return;

            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            _lateRenderingHooked = true;
        }

        private void DisableLateRendering()
        {
            if (_lateRenderingHooked)
            {
                RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
                _lateRenderingHooked = false;
            }

            _lateEntries.Clear();
            _lateCommandBuffer?.Clear();
            ApplyZTest(CompareFunction.LessEqual);
        }

        private void OnEndCameraRendering(
            ScriptableRenderContext context,
            Camera camera)
        {
            if (_material == null
                || camera == null
                || camera.cameraType == CameraType.Preview
                || _lateEntries.Count == 0)
            {
                return;
            }

            _lateCommandBuffer ??= new CommandBuffer
            {
                name = LateOverlayCommandName
            };
            _lateCommandBuffer.Clear();
            _lateCommandBuffer.SetViewProjectionMatrices(
                camera.worldToCameraMatrix,
                camera.projectionMatrix);

            bool hasDraw = false;
            for (int i = 0; i < _lateEntries.Count; i++)
            {
                ConstructionBuildGridOverlayEntry entry = _lateEntries[i];
                if (!IsDrawable(entry)
                    || !CameraCanRenderLayer(camera, entry.Layer))
                {
                    continue;
                }

                DrawCommandBufferEntry(_lateCommandBuffer, entry);
                hasDraw = true;
            }

            if (!hasDraw)
                return;

            context.ExecuteCommandBuffer(_lateCommandBuffer);
            _lateCommandBuffer.Clear();
        }

        private void DrawImmediateEntry(ConstructionBuildGridOverlayEntry entry)
        {
            MaterialPropertyBlock propertyBlock =
                PrepareEntryPropertyBlock(entry);

            Graphics.DrawMesh(
                entry.Mesh,
                entry.Matrix,
                _material,
                entry.Layer,
                null,
                0,
                propertyBlock);
        }

        private void DrawCommandBufferEntry(
            CommandBuffer commandBuffer,
            ConstructionBuildGridOverlayEntry entry)
        {
            MaterialPropertyBlock propertyBlock =
                PrepareEntryPropertyBlock(entry);

            commandBuffer.DrawMesh(
                entry.Mesh,
                entry.Matrix,
                _material,
                0,
                -1,
                propertyBlock);
        }

        private MaterialPropertyBlock PrepareEntryPropertyBlock(
            ConstructionBuildGridOverlayEntry entry)
        {
            _propertyBlock ??= new MaterialPropertyBlock();
            _propertyBlock.Clear();
            _propertyBlock.SetVector(EdgeMaskPropertyId, entry.EdgeMask);
            ResolveEntryColors(entry.VisualState, out Color lineColor, out Color fillColor);
            _propertyBlock.SetColor(LineColorPropertyId, lineColor);
            _propertyBlock.SetColor(FillColorPropertyId, fillColor);
            return _propertyBlock;
        }

        private static void PruneInvalidEntries(
            List<ConstructionBuildGridOverlayEntry> entries)
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (!IsDrawable(entries[i]))
                    entries.RemoveAt(i);
            }
        }

        private static bool IsDrawable(
            ConstructionBuildGridOverlayEntry entry)
        {
            return entry.Mesh != null
                   && (entry.SourceRenderer == null
                       || entry.SourceRenderer.enabled);
        }

        private static bool CameraCanRenderLayer(
            Camera camera,
            int layer)
        {
            if (layer < 0 || layer > 31)
                return true;

            return (camera.cullingMask & (1 << layer)) != 0;
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
                case ConstructionBuildGridTileVisualState.Unaffordable:
                    lineColor = _unaffordableLineColor;
                    fillColor = _unaffordableFillColor;
                    return;
                default:
                    lineColor = _generalLineColor;
                    fillColor = _generalFillColor;
                    return;
            }
        }

        private static Color ResolveLineColor(Color configured, float fallbackAlpha)
        {
            if (configured.a <= 0f)
                configured.a = fallbackAlpha;
            return configured;
        }

        private static Color ResolveFillColor(Color configured, float fallbackAlpha)
        {
            if (configured.a <= 0f)
                configured.a = fallbackAlpha;
            return configured;
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
            ApplyZTest(CompareFunction.LessEqual);
        }

        private void ApplyZTest(CompareFunction function)
        {
            if (_material == null)
                return;

            _material.SetFloat(ZTestPropertyId, (float)function);
        }
    }
}
