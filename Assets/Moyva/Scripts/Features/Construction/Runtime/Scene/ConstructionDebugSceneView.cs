using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kruty1918.Moyva.Construction.Runtime
{
    public sealed class ConstructionDebugSceneView : MonoBehaviour
    {
        [SerializeField] private ConstructionSceneContext _sceneContext;
        [SerializeField] private Color _previewRootColor = new Color(0.35f, 1f, 0.35f, 0.9f);
        [SerializeField] private Color _placedRootColor = new Color(0.35f, 0.65f, 1f, 0.9f);
        [SerializeField] private Color _radiusRootColor = new Color(1f, 0.75f, 0.25f, 0.9f);
        [SerializeField] private Color _uiRootColor = new Color(1f, 0.45f, 0.45f, 0.9f);
        [SerializeField] private Color _debugRootColor = new Color(0.95f, 0.35f, 1f, 0.9f);
        [SerializeField] private Color _pendingPlacementColor = new Color(0.25f, 1f, 0.6f, 0.45f);
        [SerializeField] private Color _pendingWallPathColor = new Color(1f, 0.85f, 0.15f, 0.9f);
        [SerializeField] private Color _influenceCenterColor = new Color(0.15f, 0.9f, 1f, 0.9f);

        private IConstructionService _constructionService;
        private IBuildingRegistry _buildingRegistry;
        private IWallTopologyService _wallTopologyService;
        private IGridProjection _gridProjection;

        [Inject]
        private void Construct(
            [InjectOptional] IConstructionService constructionService = null,
            [InjectOptional] IBuildingRegistry buildingRegistry = null,
            [InjectOptional] IWallTopologyService wallTopologyService = null,
            [InjectOptional] IGridProjection gridProjection = null)
        {
            _constructionService = constructionService;
            _buildingRegistry = buildingRegistry;
            _wallTopologyService = wallTopologyService;
            _gridProjection = gridProjection;
        }

        private void OnDrawGizmosSelected()
        {
            if (_sceneContext == null)
                _sceneContext = GetComponent<ConstructionSceneContext>();

            ConstructionDiagnosticsProfileSO diagnostics = _sceneContext != null
                ? _sceneContext.ResolveDiagnosticsProfile()
                : null;

            if (diagnostics != null && !diagnostics.DrawSceneGizmos)
                return;

            if (diagnostics != null && !diagnostics.EnableVisualDebug)
                return;

            if (_sceneContext?.SceneRoots == null)
                return;

            DrawRootMarker(_sceneContext.SceneRoots.PreviewRoot, _previewRootColor);
            DrawRootMarker(_sceneContext.SceneRoots.PlacedRoot, _placedRootColor);
            DrawRootMarker(_sceneContext.SceneRoots.RadiusRoot, _radiusRootColor);
            DrawRootMarker(_sceneContext.SceneRoots.UIRoot, _uiRootColor);
            DrawRootMarker(_sceneContext.SceneRoots.DebugRoot, _debugRootColor);

            DrawPendingPlacementGizmos(diagnostics);
            DrawSelectedBuildingLabel(diagnostics);

            if (diagnostics != null && diagnostics.DrawInfluenceZones && _sceneContext.SceneRoots.RadiusRoot != null)
            {
                DrawInfluenceCenterGizmos();
            }

            if (diagnostics != null && diagnostics.DrawBlockedTiles && _sceneContext.SceneRoots.PreviewRoot != null)
            {
                Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.35f);
                Gizmos.DrawWireCube(_sceneContext.SceneRoots.PreviewRoot.position, Vector3.one);
            }
        }

        private void DrawPendingPlacementGizmos(ConstructionDiagnosticsProfileSO diagnostics)
        {
            if (_constructionService == null || (diagnostics != null && !diagnostics.EnablePlacementDebug))
                return;

            IReadOnlyDictionary<Vector2Int, string> pendingPlacements = _constructionService.GetPendingPlacements();
            if (pendingPlacements == null || pendingPlacements.Count == 0)
                return;

            var wallPositions = new HashSet<Vector2Int>();
            foreach (var pair in pendingPlacements)
            {
                Vector3 worldPosition = ResolveWorldPosition(pair.Key, 0.08f);
                bool isWallOrGate = _wallTopologyService != null && _wallTopologyService.IsWallOrGate(pair.Value);

                Gizmos.color = isWallOrGate
                    ? new Color(_pendingWallPathColor.r, _pendingWallPathColor.g, _pendingWallPathColor.b, 0.35f)
                    : _pendingPlacementColor;
                Gizmos.DrawCube(worldPosition, new Vector3(0.6f, 0.08f, 0.6f));

                Gizmos.color = isWallOrGate ? _pendingWallPathColor : _pendingPlacementColor;
                Gizmos.DrawWireCube(worldPosition, new Vector3(1f, 0.12f, 1f));

                DrawPendingPlacementLabel(pair.Key, worldPosition, diagnostics);

                if (isWallOrGate)
                    wallPositions.Add(pair.Key);
            }

            if (diagnostics == null || diagnostics.EnableWallDebug)
                DrawWallPathPreviewLinks(wallPositions);
        }

        private void DrawWallPathPreviewLinks(HashSet<Vector2Int> wallPositions)
        {
            if (wallPositions == null || wallPositions.Count == 0)
                return;

            Gizmos.color = _pendingWallPathColor;
            foreach (Vector2Int position in wallPositions)
            {
                DrawWallPathPreviewLink(position, position + Vector2Int.right, wallPositions);
                DrawWallPathPreviewLink(position, position + Vector2Int.up, wallPositions);
            }
        }

        private void DrawWallPathPreviewLink(Vector2Int from, Vector2Int to, HashSet<Vector2Int> wallPositions)
        {
            if (!wallPositions.Contains(to))
                return;

            Vector3 fromWorld = ResolveWorldPosition(from, 0.12f);
            Vector3 toWorld = ResolveWorldPosition(to, 0.12f);
            Gizmos.DrawLine(fromWorld, toWorld);
        }

        private void DrawInfluenceCenterGizmos()
        {
            if (_constructionService == null || _buildingRegistry == null)
                return;

            IReadOnlyDictionary<Vector2Int, string> placedBuildings = _constructionService.GetPlayerPlacedBuildings();
            if (placedBuildings == null || placedBuildings.Count == 0)
                return;

            int fallbackRadius = _sceneContext?.ResolvePlacementRulesProfile()?.TownHallBuildRadius ?? 0;
            Gizmos.color = _influenceCenterColor;

            foreach (var pair in placedBuildings)
            {
                BuildingDefinition definition = _buildingRegistry.GetById(pair.Value);
                int influenceRadius = BuildingDefinitionCapabilities.GetInfluenceRadius(definition, fallbackRadius);
                if (influenceRadius <= 0)
                    continue;

                Vector3 center = ResolveWorldPosition(pair.Key, 0.04f);
                float size = influenceRadius * 2f + 1f;
                Gizmos.DrawWireCube(center, new Vector3(size, 0.1f, size));
                Gizmos.DrawWireSphere(center, 0.15f);
            }
        }

        private void DrawSelectedBuildingLabel(ConstructionDiagnosticsProfileSO diagnostics)
        {
#if UNITY_EDITOR
            if (_constructionService == null || (diagnostics != null && !diagnostics.EnablePlacementDebug))
                return;

            string selectedBuildingId = _constructionService.GetSelectedBuildingId();
            if (string.IsNullOrWhiteSpace(selectedBuildingId))
                return;

            Vector3 labelPosition = _sceneContext?.SceneRoots?.DebugRoot != null
                ? _sceneContext.SceneRoots.DebugRoot.position + Vector3.up * 1.5f
                : transform.position + Vector3.up * 1.5f;

            int pendingCount = _constructionService.GetPendingPlacements()?.Count ?? 0;
            Handles.color = Color.white;
            Handles.Label(labelPosition, $"Selected: {selectedBuildingId}\nPending: {pendingCount}");
#endif
        }

        private void DrawPendingPlacementLabel(Vector2Int tilePosition, Vector3 worldPosition, ConstructionDiagnosticsProfileSO diagnostics)
        {
#if UNITY_EDITOR
            if (_constructionService == null || diagnostics == null || !diagnostics.EnableResourceDebug)
                return;

            if (!_constructionService.TryGetPendingPlacementStatus(tilePosition, out var status))
                return;

            string affordability = status.IsAffordable ? "Affordable" : "Blocked";
            string settlement = status.HasSettlement ? status.SettlementName : "No settlement";
            string error = string.IsNullOrWhiteSpace(status.ErrorMessage) ? string.Empty : $"\n{status.ErrorMessage}";

            Handles.color = status.IsAffordable ? Color.green : Color.red;
            Handles.Label(worldPosition + Vector3.up * 0.35f, $"{affordability}\n{settlement}{error}");
#endif
        }

        private Vector3 ResolveWorldPosition(Vector2Int tilePosition, float layerOffset)
        {
            if (_gridProjection != null)
                return _gridProjection.GridToWorld(tilePosition, 0f, layerOffset);

            return new Vector3(tilePosition.x, layerOffset, tilePosition.y);
        }

        private static void DrawRootMarker(Transform root, Color color)
        {
            if (root == null)
                return;

            Gizmos.color = color;
            Gizmos.DrawWireSphere(root.position, 0.35f);
            Gizmos.DrawLine(root.position, root.position + Vector3.up * 1.25f);
        }
    }
}
