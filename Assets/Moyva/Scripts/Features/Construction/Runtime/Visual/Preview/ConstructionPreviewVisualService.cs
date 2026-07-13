using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionPreviewVisualService : IConstructionPreviewVisualService
    {
        private const float PreviewMoveSharpness = 18f;
        private const float PreviewDragSharpness = 28f;
        private const float PreviewSnapSharpness = 14f;
        private const int SnapHighlightRenderQueue = 3995;

        private static readonly int EdgeMaskPropertyId = Shader.PropertyToID("_EdgeMask");
        private static readonly int LineColorPropertyId = Shader.PropertyToID("_LineColor");
        private static readonly int FillColorPropertyId = Shader.PropertyToID("_FillColor");
        private static readonly int LineWidthPropertyId = Shader.PropertyToID("_LineWidth");
        private static readonly int SurfaceLiftPropertyId = Shader.PropertyToID("_SurfaceLift");
        private static readonly int MinUpNormalYPropertyId = Shader.PropertyToID("_MinUpNormalY");
        private static readonly int UseCellMaskPropertyId = Shader.PropertyToID("_UseCellMask");

        private readonly Dictionary<Vector2Int, GameObject> _previewByPosition = new();
        private readonly List<GameObject> _gridHoverHighlights = new();
        private readonly List<MeshRenderer> _gridHoverRenderers = new();
        private readonly IConstructionVisualRootService _roots;
        private readonly IConstructionVisualFactory _visualFactory;
        private readonly IConstructionVisualStyleService _styleService;
        private readonly IWallVisualResolver _wallVisualResolver;
        private readonly IConstructionTerrainAlignmentService _terrainAlignment;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;
        private GameObject _snapHighlight;
        private Mesh _snapHighlightMesh;
        private Material _snapHighlightMaterial;
        private MaterialPropertyBlock _gridHoverPropertyBlock;

        [Inject]
        public ConstructionPreviewVisualService(
            IConstructionVisualRootService roots,
            IConstructionVisualFactory visualFactory,
            IConstructionVisualStyleService styleService,
            IWallVisualResolver wallVisualResolver,
            [InjectOptional] IConstructionTerrainAlignmentService terrainAlignment = null,
            [InjectOptional] IConstructionGridGeometryService gridGeometry = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null)
        {
            _roots = roots;
            _visualFactory = visualFactory;
            _styleService = styleService;
            _wallVisualResolver = wallVisualResolver;
            _terrainAlignment = terrainAlignment;
            _gridGeometry = gridGeometry;
            _settingsProvider = settingsProvider;
        }

        public GameObject Show(BuildingPreviewChangedSignal signal, BuildingDefinition def)
        {
            if (TryGetReusablePreview(signal, out GameObject existing))
            {
                _styleService.ApplyGhostStyle(existing, true);
                return existing;
            }

            Remove(signal.Position);
            GameObject prefab = ResolvePrefab(signal.Position, signal.BuildingId, def.Prefab);
            GameObject instance = CreatePreview(prefab, signal.Position, signal.BuildingId, def.VisualYOffset);
            if (instance == null)
                return null;

            _previewByPosition[signal.Position] = instance;
            _styleService.ApplyGhostStyle(instance, true);
            return instance;
        }

        public bool TryGet(Vector2Int position, out GameObject visual)
        {
            if (_previewByPosition.TryGetValue(position, out visual) && visual != null)
                return true;

            visual = null;
            return false;
        }

        public bool Has(Vector2Int position) => TryGet(position, out _);

        public bool TryMove(Vector2Int fromPosition, Vector2Int toPosition, string buildingId, float visualOffsetY = 0f)
        {
            if (!_previewByPosition.TryGetValue(fromPosition, out GameObject instance) || instance == null)
                return false;

            if (!MatchesBuildingId(instance, buildingId))
                return false;

            _previewByPosition.Remove(fromPosition);

            if (_previewByPosition.TryGetValue(toPosition, out GameObject existing) && existing != null && existing != instance)
                Object.Destroy(existing);

            _previewByPosition[toPosition] = instance;
            ConstructionBuildingPointerTarget.AttachOrUpdate(instance, buildingId, toPosition, isPreviewVisual: true);
            MoveVisualToTile(instance, toPosition, isPreviewVisual: true, visualOffsetY, PreviewMoveSharpness);
            return true;
        }

        public void MoveDragVisual(Vector2Int position, string buildingId, Vector3 worldPosition, bool snapToGrid, bool hasSnapTarget, Vector2Int snapTargetPosition, float visualOffsetY = 0f)
        {
            if (!TryGet(position, out GameObject instance) || !MatchesBuildingId(instance, buildingId))
                return;

            Vector3 target = ResolveAlignedTarget(instance, position, isPreviewVisual: true, visualOffsetY);
            if (!snapToGrid && _terrainAlignment != null)
            {
                Vector3 surfaceAnchor = _terrainAlignment.ResolveWorldPosition(position, 0f);
                target.x = worldPosition.x + (target.x - surfaceAnchor.x);
                target.y = worldPosition.y + (target.y - surfaceAnchor.y);
                target.z = worldPosition.z + (target.z - surfaceAnchor.z);
                Vector2 cursorOffset = _settingsProvider?.PreviewDragCursorOffsetXZ ?? Vector2.zero;
                target.x += cursorOffset.x;
                target.z += cursorOffset.y;
            }

            MoveVisual(instance, target, snapToGrid ? PreviewSnapSharpness : ResolvePreviewDragSharpness());
        }

        public bool TryRelease(Vector2Int position, out GameObject visual)
        {
            if (!_previewByPosition.TryGetValue(position, out visual) || visual == null)
            {
                visual = null;
                return false;
            }

            _previewByPosition.Remove(position);
            return true;
        }

        public void ReplaceWallPreview(Vector2Int position, string buildingId, GameObject prefab, float visualOffsetY = 0f)
        {
            if (!Has(position))
                return;

            Remove(position);
            GameObject instance = CreatePreview(prefab, position, buildingId, visualOffsetY);
            if (instance == null)
                return;

            _previewByPosition[position] = instance;
            _styleService.ApplyGhostStyle(instance, true);
        }

        public void Remove(Vector2Int position)
        {
            if (!_previewByPosition.TryGetValue(position, out GameObject instance))
                return;

            if (instance != null)
                Object.Destroy(instance);

            _previewByPosition.Remove(position);
        }

        public void Clear()
        {
            HideSnapTargetHighlight();

            foreach (KeyValuePair<Vector2Int, GameObject> pair in _previewByPosition)
            {
                if (pair.Value != null)
                    Object.Destroy(pair.Value);
            }

            _previewByPosition.Clear();
        }

        public void Dispose()
        {
            Clear();
            for (int index = 0; index < _gridHoverHighlights.Count; index++)
                DestroyUnityObject(_gridHoverHighlights[index]);

            _gridHoverHighlights.Clear();
            _gridHoverRenderers.Clear();
            DestroyUnityObject(_snapHighlightMaterial);
            DestroyUnityObject(_snapHighlightMesh);
            _snapHighlight = null;
            _snapHighlightMaterial = null;
            _snapHighlightMesh = null;
            _gridHoverPropertyBlock = null;
        }

        public void ShowGridHover(BuildGridHoverChangedSignal signal)
        {
            if (!signal.HasTile)
            {
                ClearGridHover();
                return;
            }

            Vector2Int[] positions = signal.FootprintPositions;
            int count = positions != null && positions.Length > 0 ? positions.Length : 1;
            for (int index = 0; index < count; index++)
            {
                EnsureGridHoverHighlight(index);
                if (index >= _gridHoverHighlights.Count)
                    break;

                GameObject highlight = _gridHoverHighlights[index];
                Vector2Int position = positions != null && positions.Length > index
                    ? positions[index]
                    : signal.Position;
                PositionGridHoverHighlight(highlight, position);
                ApplyGridHoverStyle(
                    index,
                    hasBuildingSelection: !string.IsNullOrWhiteSpace(signal.BuildingId),
                    isInvalid: ContainsPosition(signal.InvalidFootprintPositions, position));
                highlight.SetActive(true);
            }

            for (int index = count; index < _gridHoverHighlights.Count; index++)
            {
                if (_gridHoverHighlights[index] != null)
                    _gridHoverHighlights[index].SetActive(false);
            }
        }

        public void ClearGridHover()
        {
            for (int index = 0; index < _gridHoverHighlights.Count; index++)
            {
                if (_gridHoverHighlights[index] != null)
                    _gridHoverHighlights[index].SetActive(false);
            }
        }

        private bool TryGetReusablePreview(BuildingPreviewChangedSignal signal, out GameObject existing)
        {
            return _previewByPosition.TryGetValue(signal.Position, out existing)
                && existing != null
                && existing.name.Contains(signal.BuildingId);
        }

        private GameObject ResolvePrefab(Vector2Int position, string buildingId, GameObject defaultPrefab)
        {
            return _wallVisualResolver.TryResolvePreviewVisual(position, buildingId, out GameObject wallPrefab)
                ? wallPrefab
                : defaultPrefab;
        }

        private GameObject CreatePreview(GameObject prefab, Vector2Int position, string buildingId, float visualOffsetY)
        {
            string prefabTag = prefab != null ? prefab.name : "NULL";
            GameObject instance = _visualFactory.CreateInstance(
                prefab,
                position,
                _roots.PreviewRoot,
                $"Preview_{buildingId}_{prefabTag}_{position.x}_{position.y}",
                ResolveSortingOrder(),
                isPreviewVisual: true,
                visualOffsetY: visualOffsetY);
            if (instance != null)
                ConstructionBuildingPointerTarget.AttachOrUpdate(instance, buildingId, position, isPreviewVisual: true);

            ConstructionSmoothVisualMotion.AttachOrUpdate(instance)?.JumpToCurrent();
            return instance;
        }

        private void MoveVisualToTile(GameObject instance, Vector2Int position, bool isPreviewVisual, float visualOffsetY, float sharpness)
        {
            MoveVisual(instance, ResolveAlignedTarget(instance, position, isPreviewVisual, visualOffsetY), sharpness);
        }

        private void MoveVisual(GameObject instance, Vector3 targetPosition, float sharpness)
        {
            var motion = ConstructionSmoothVisualMotion.AttachOrUpdate(instance);
            if (motion != null)
                motion.MoveTo(targetPosition, sharpness);
            else if (instance != null)
                instance.transform.position = targetPosition;
        }

        private Vector3 ResolveAlignedTarget(GameObject instance, Vector2Int position, bool isPreviewVisual, float visualOffsetY)
        {
            if (_terrainAlignment != null)
                return _terrainAlignment.ResolveAlignedInstancePosition(instance, position, isPreviewVisual, visualOffsetY);

            Vector3 fallback = instance != null ? instance.transform.position : Vector3.zero;
            fallback.x = position.x;
            fallback.z = position.y;
            return fallback;
        }

        private static bool MatchesBuildingId(GameObject instance, string buildingId)
        {
            return instance != null
                && (string.IsNullOrWhiteSpace(buildingId)
                    || instance.name.Contains(buildingId));
        }

        private void ShowSnapTargetHighlight(Vector2Int position)
        {
            if (_settingsProvider != null && !_settingsProvider.ShowSnapTargetHighlight)
            {
                HideSnapTargetHighlight();
                return;
            }

            EnsureSnapTargetHighlight();
            if (_snapHighlight == null)
                return;

            Vector3 center = _terrainAlignment != null
                ? _terrainAlignment.ResolveWorldPosition(position, ResolveSnapHighlightSurfaceOffsetY())
                : new Vector3(position.x, ResolveSnapHighlightSurfaceOffsetY(), position.y);
            Vector2 cellSize = ResolveCellSize();
            float inset = Mathf.Clamp(_settingsProvider?.SnapTargetHighlightInsetNormalized ?? 0.04f, 0f, 0.45f);
            float scale = Mathf.Max(0.01f, 1f - inset * 2f);

            _snapHighlight.transform.position = center;
            _snapHighlight.transform.rotation = Quaternion.identity;
            _snapHighlight.transform.localScale = new Vector3(cellSize.x * scale, 1f, cellSize.y * scale);
            ApplySnapHighlightStyle();
            _snapHighlight.SetActive(true);
        }

        private void HideSnapTargetHighlight()
        {
            ClearGridHover();
        }

        private void EnsureSnapTargetHighlight()
        {
            if (_snapHighlight != null)
                return;

            _snapHighlight = new GameObject("ConstructionSnapTargetHighlight");
            _snapHighlight.transform.SetParent(_roots.PreviewRoot, false);
            _snapHighlight.SetActive(false);

            _snapHighlightMesh = CreateXZQuadMesh("ConstructionSnapTargetHighlightMesh");
            _snapHighlight.AddComponent<MeshFilter>().sharedMesh = _snapHighlightMesh;

            var renderer = _snapHighlight.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            Shader shader = Shader.Find(_settingsProvider?.BuildGridShaderName ?? "Moyva/Overlay/ConstructionBuildGrid");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");

            if (shader == null)
            {
                Debug.LogWarning("[ConstructionVisual] Snap target highlight shader not found.");
                Object.Destroy(_snapHighlight);
                _snapHighlight = null;
                return;
            }

            _snapHighlightMaterial = new Material(shader)
            {
                name = "ConstructionSnapTargetHighlight_Material",
                renderQueue = SnapHighlightRenderQueue
            };
            renderer.sharedMaterial = _snapHighlightMaterial;
            ApplySnapHighlightStyle();
            _gridHoverHighlights.Add(_snapHighlight);
            _gridHoverRenderers.Add(renderer);
        }

        private void EnsureGridHoverHighlight(int index)
        {
            EnsureSnapTargetHighlight();
            if (_snapHighlight == null || _snapHighlightMesh == null || _snapHighlightMaterial == null)
                return;

            while (_gridHoverHighlights.Count <= index)
            {
                var highlight = new GameObject($"ConstructionGridHover_{_gridHoverHighlights.Count}");
                highlight.transform.SetParent(_roots.PreviewRoot, false);
                highlight.SetActive(false);
                highlight.AddComponent<MeshFilter>().sharedMesh = _snapHighlightMesh;
                var renderer = highlight.AddComponent<MeshRenderer>();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.sharedMaterial = _snapHighlightMaterial;
                _gridHoverHighlights.Add(highlight);
                _gridHoverRenderers.Add(renderer);
            }
        }

        private void PositionGridHoverHighlight(GameObject highlight, Vector2Int position)
        {
            Vector3 center = _terrainAlignment != null
                ? _terrainAlignment.ResolveWorldPosition(position, ResolveSnapHighlightSurfaceOffsetY())
                : new Vector3(position.x, ResolveSnapHighlightSurfaceOffsetY(), position.y);
            Vector2 cellSize = ResolveCellSize();
            float inset = Mathf.Clamp(_settingsProvider?.SnapTargetHighlightInsetNormalized ?? 0.04f, 0f, 0.45f);
            float scale = Mathf.Max(0.01f, 1f - inset * 2f);

            highlight.transform.position = center;
            highlight.transform.rotation = Quaternion.identity;
            highlight.transform.localScale = new Vector3(cellSize.x * scale, 1f, cellSize.y * scale);
        }

        private void ApplyGridHoverStyle(int index, bool hasBuildingSelection, bool isInvalid)
        {
            MeshRenderer renderer = index >= 0 && index < _gridHoverRenderers.Count
                ? _gridHoverRenderers[index]
                : null;
            if (renderer == null)
                return;

            Color lineColor;
            Color fillColor;
            if (!hasBuildingSelection)
            {
                lineColor = new Color(0.70f, 0.95f, 1f, 0.9f);
                fillColor = new Color(0.70f, 0.95f, 1f, 0.16f);
            }
            else if (isInvalid)
            {
                lineColor = new Color(1f, 0.26f, 0.22f, 0.9f);
                fillColor = new Color(0.92f, 0.12f, 0.10f, 0.16f);
            }
            else
            {
                lineColor = new Color(0.28f, 1f, 0.42f, 0.9f);
                fillColor = new Color(0.20f, 0.82f, 0.32f, 0.16f);
            }

            _gridHoverPropertyBlock ??= new MaterialPropertyBlock();
            _gridHoverPropertyBlock.Clear();
            _gridHoverPropertyBlock.SetColor(LineColorPropertyId, lineColor);
            _gridHoverPropertyBlock.SetColor(FillColorPropertyId, fillColor);
            _gridHoverPropertyBlock.SetVector(EdgeMaskPropertyId, Vector4.one);
            renderer.SetPropertyBlock(_gridHoverPropertyBlock);
        }

        private static bool ContainsPosition(Vector2Int[] positions, Vector2Int target)
        {
            if (positions == null)
                return false;

            for (int index = 0; index < positions.Length; index++)
            {
                if (positions[index] == target)
                    return true;
            }

            return false;
        }

        private void ApplySnapHighlightStyle()
        {
            if (_snapHighlightMaterial == null)
                return;

            SetColorIfExists(_snapHighlightMaterial, LineColorPropertyId, _settingsProvider?.SnapTargetHighlightLineColor ?? new Color(1f, 0.58f, 0.12f, 0.9f));
            SetColorIfExists(_snapHighlightMaterial, FillColorPropertyId, _settingsProvider?.SnapTargetHighlightFillColor ?? new Color(1f, 0.58f, 0.12f, 0.16f));
            SetFloatIfExists(_snapHighlightMaterial, LineWidthPropertyId, _settingsProvider?.SnapTargetHighlightLineWidthNormalized ?? 0.06f);
            SetVectorIfExists(_snapHighlightMaterial, EdgeMaskPropertyId, Vector4.one);
            SetFloatIfExists(_snapHighlightMaterial, SurfaceLiftPropertyId, 0f);
            SetFloatIfExists(_snapHighlightMaterial, MinUpNormalYPropertyId, 0.2f);
            SetFloatIfExists(_snapHighlightMaterial, UseCellMaskPropertyId, 0f);
        }

        private Vector2 ResolveCellSize()
        {
            return _gridGeometry != null && _gridGeometry.TryGetCellSize(out Vector2 cellSize)
                ? cellSize
                : Vector2.one;
        }

        private float ResolveSnapHighlightSurfaceOffsetY()
            => _settingsProvider?.SnapTargetHighlightSurfaceOffsetY ?? 0.16f;

        private float ResolvePreviewDragSharpness()
            => _settingsProvider?.PreviewDragFollowSharpness ?? PreviewDragSharpness;

        private static Mesh CreateXZQuadMesh(string name)
        {
            var mesh = new Mesh { name = name };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, 0.5f),
                new Vector3(-0.5f, 0f, 0.5f),
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
            };
            mesh.normals = new[]
            {
                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up,
            };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void SetColorIfExists(Material material, int propertyId, Color color)
        {
            if (material.HasProperty(propertyId))
                material.SetColor(propertyId, color);
        }

        private static void SetFloatIfExists(Material material, int propertyId, float value)
        {
            if (material.HasProperty(propertyId))
                material.SetFloat(propertyId, value);
        }

        private static void SetVectorIfExists(Material material, int propertyId, Vector4 value)
        {
            if (material.HasProperty(propertyId))
                material.SetVector(propertyId, value);
        }

        private static void DestroyUnityObject(Object unityObject)
        {
            if (unityObject == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(unityObject);
            else
                Object.DestroyImmediate(unityObject);
        }

        private int ResolveSortingOrder()
            => _settingsProvider?.BuildingLayerMinSortingOrder ?? 5;
    }
}
