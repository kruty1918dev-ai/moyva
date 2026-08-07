using GiantGrey.TileWorldCreator;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Editor
{
    [InitializeOnLoad]
    internal static class TileWorldCreatorChunkGizmoDrawer
    {
        private const float LiftY = 0.08f;
        private const float SelectedChunkLineWidth = 3.25f;
        private const float ActualChunkLineWidth = 2f;

        private static readonly Color ManagerChunkColor = new(1f, 1f, 1f, 0.78f);
        private static readonly Color SelectedChunkColor = Color.white;
        private static readonly Color ActualChunkColor = new(1f, 0.48f, 0.12f, 0.9f);

        private static Vector3[] _managerGridLines = System.Array.Empty<Vector3>();
        private static int _cachedManagerId;
        private static int _cachedWidth;
        private static int _cachedHeight;
        private static int _cachedChunkSize;
        private static float _cachedCellSize;
        private static Vector3 _cachedOrigin;

        private static bool _selectedCacheDirty = true;
        private static int _cachedSelectedId;
        private static Vector3 _cachedSelectedPosition;
        private static Quaternion _cachedSelectedRotation;
        private static Vector3 _cachedSelectedScale;
        private static bool _hasSelectedLogicalBounds;
        private static bool _hasSelectedActualBounds;
        private static Bounds _selectedLogicalBounds;
        private static Bounds _selectedActualBounds;

        static TileWorldCreatorChunkGizmoDrawer()
        {
            SceneView.duringSceneGui -= DrawSceneGizmos;
            SceneView.duringSceneGui += DrawSceneGizmos;

            Selection.selectionChanged -= InvalidateSelectedCache;
            Selection.selectionChanged += InvalidateSelectedCache;

            EditorApplication.hierarchyChanged -= InvalidateSelectedCache;
            EditorApplication.hierarchyChanged += InvalidateSelectedCache;

            Undo.undoRedoPerformed -= InvalidateAllCaches;
            Undo.undoRedoPerformed += InvalidateAllCaches;
        }

        private static void DrawSceneGizmos(SceneView sceneView)
        {
            if (sceneView == null || Event.current?.type != EventType.Repaint)
                return;

            Transform selected = Selection.activeTransform;
            if (selected == null)
                return;

            TileWorldCreatorManager manager = selected.GetComponent<TileWorldCreatorManager>();
            if (manager != null)
            {
                DrawManagerGrid(manager);
                return;
            }

            DrawSelectedChunk(selected);
        }

        private static void DrawManagerGrid(TileWorldCreatorManager manager)
        {
            if (!EnsureManagerGridCache(manager))
                return;

            Color previousColor = Handles.color;
            CompareFunction previousZTest = Handles.zTest;

            Handles.color = ManagerChunkColor;
            Handles.zTest = CompareFunction.Always;
            Handles.DrawLines(_managerGridLines);

            Handles.zTest = previousZTest;
            Handles.color = previousColor;
        }

        private static bool EnsureManagerGridCache(TileWorldCreatorManager manager)
        {
            if (manager == null || manager.configuration == null)
                return false;

            var configuration = manager.configuration;
            int width = Mathf.Max(0, configuration.width);
            int height = Mathf.Max(0, configuration.height);
            int chunkSize = Mathf.Max(1, configuration.clusterCellSize);
            float cellSize = configuration.cellSize > 0.0001f
                ? configuration.cellSize
                : 1f;
            Vector3 origin = manager.transform.position;
            int managerId = manager.GetInstanceID();

            if (width <= 0 || height <= 0)
            {
                _managerGridLines = System.Array.Empty<Vector3>();
                return false;
            }

            bool cacheValid =
                _managerGridLines.Length > 0
                && _cachedManagerId == managerId
                && _cachedWidth == width
                && _cachedHeight == height
                && _cachedChunkSize == chunkSize
                && Mathf.Approximately(_cachedCellSize, cellSize)
                && _cachedOrigin == origin;

            if (cacheValid)
                return true;

            int chunkCountX = Mathf.CeilToInt(width / (float)chunkSize);
            int chunkCountY = Mathf.CeilToInt(height / (float)chunkSize);
            int verticalLines = chunkCountX + 1;
            int horizontalLines = chunkCountY + 1;

            _managerGridLines = new Vector3[(verticalLines + horizontalLines) * 2];

            float y = origin.y + LiftY;
            float maxX = origin.x + width * cellSize;
            float maxZ = origin.z + height * cellSize;
            int pointIndex = 0;

            for (int x = 0; x <= chunkCountX; x++)
            {
                int tileX = Mathf.Min(x * chunkSize, width);
                float worldX = origin.x + tileX * cellSize;

                _managerGridLines[pointIndex++] =
                    new Vector3(worldX, y, origin.z);
                _managerGridLines[pointIndex++] =
                    new Vector3(worldX, y, maxZ);
            }

            for (int z = 0; z <= chunkCountY; z++)
            {
                int tileZ = Mathf.Min(z * chunkSize, height);
                float worldZ = origin.z + tileZ * cellSize;

                _managerGridLines[pointIndex++] =
                    new Vector3(origin.x, y, worldZ);
                _managerGridLines[pointIndex++] =
                    new Vector3(maxX, y, worldZ);
            }

            _cachedManagerId = managerId;
            _cachedWidth = width;
            _cachedHeight = height;
            _cachedChunkSize = chunkSize;
            _cachedCellSize = cellSize;
            _cachedOrigin = origin;

            return _managerGridLines.Length > 0;
        }

        private static void DrawSelectedChunk(Transform selected)
        {
            if (selected == null)
                return;

            if (ShouldRefreshSelectedCache(selected))
                RefreshSelectedCache(selected);

            if (!_hasSelectedLogicalBounds && !_hasSelectedActualBounds)
                return;

            CompareFunction previousZTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;

            if (_hasSelectedLogicalBounds)
            {
                DrawRaisedOutline(
                    _selectedLogicalBounds,
                    SelectedChunkColor,
                    SelectedChunkLineWidth);
            }

            if (_hasSelectedActualBounds)
            {
                DrawRaisedOutline(
                    _selectedActualBounds,
                    ActualChunkColor,
                    ActualChunkLineWidth);
            }

            Handles.zTest = previousZTest;
        }

        private static bool ShouldRefreshSelectedCache(Transform selected)
        {
            if (_selectedCacheDirty)
                return true;

            if (_cachedSelectedId != selected.GetInstanceID())
                return true;

            if (_cachedSelectedPosition != selected.position)
                return true;

            if (_cachedSelectedRotation != selected.rotation)
                return true;

            if (_cachedSelectedScale != selected.lossyScale)
                return true;

            return false;
        }

        private static void RefreshSelectedCache(Transform selected)
        {
            _cachedSelectedId = selected != null
                ? selected.GetInstanceID()
                : 0;

            _cachedSelectedPosition = selected != null
                ? selected.position
                : default;

            _cachedSelectedRotation = selected != null
                ? selected.rotation
                : Quaternion.identity;

            _cachedSelectedScale = selected != null
                ? selected.lossyScale
                : Vector3.one;

            _hasSelectedLogicalBounds =
                selected != null
                && TileWorldCreatorChunkGizmoBoundsCollector.TryCollectSelectedChunk(
                    selected,
                    out _selectedLogicalBounds);

            _hasSelectedActualBounds =
                selected != null
                && TileWorldCreatorChunkGizmoBoundsCollector.TryCollectSelectedActualBounds(
                    selected,
                    out _selectedActualBounds);

            _selectedCacheDirty = false;
        }

        private static void DrawRaisedOutline(
            Bounds bounds,
            Color color,
            float lineWidth)
        {
            float y = bounds.max.y + LiftY;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            var p0 = new Vector3(min.x, y, min.z);
            var p1 = new Vector3(max.x, y, min.z);
            var p2 = new Vector3(max.x, y, max.z);
            var p3 = new Vector3(min.x, y, max.z);

            Color previous = Handles.color;
            Handles.color = color;
            Handles.DrawAAPolyLine(
                lineWidth,
                p0,
                p1,
                p2,
                p3,
                p0);
            Handles.color = previous;
        }

        private static void InvalidateSelectedCache()
        {
            _selectedCacheDirty = true;
        }

        private static void InvalidateAllCaches()
        {
            _managerGridLines = System.Array.Empty<Vector3>();
            _cachedManagerId = 0;
            _selectedCacheDirty = true;
        }
    }
}
