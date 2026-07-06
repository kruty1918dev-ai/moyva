using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [InitializeOnLoad]
    internal static class TileWorldCreatorChunkGizmoDrawer
    {
        private const float LiftY = 0.08f;
        private const float ManagerLineWidth = 2f;
        private const float SelectedChunkLineWidth = 3.25f;
        private static readonly Color ManagerChunkColor = new(1f, 1f, 1f, 0.78f);
        private static readonly Color SelectedChunkColor = Color.white;
        private static readonly Color ActualChunkColor = new(1f, 0.48f, 0.12f, 0.9f);
        private static readonly System.Collections.Generic.List<Bounds> BoundsBuffer = new(128);

        static TileWorldCreatorChunkGizmoDrawer()
        {
            SceneView.duringSceneGui -= DrawSelectedChunks;
            SceneView.duringSceneGui += DrawSelectedChunks;
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        private static void DrawManagerChunks(TileWorldCreatorManager manager, GizmoType gizmoType)
        {
            DrawManager(manager);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        private static void DrawSelectedChunk(ClusterIdentifier cluster, GizmoType gizmoType)
        {
            DrawCluster(cluster, SelectedChunkColor, SelectedChunkLineWidth);
        }

        private static void DrawSelectedChunks(SceneView sceneView)
        {
            if (sceneView == null || Event.current?.type != EventType.Repaint)
                return;

            Transform selected = Selection.activeTransform;
            if (selected == null)
                return;

            TileWorldCreatorManager manager = selected.GetComponent<TileWorldCreatorManager>();
            if (manager != null)
            {
                DrawManager(manager);
                return;
            }

            if (TileWorldCreatorChunkGizmoBoundsCollector.TryCollectSelectedChunk(selected, out Bounds bounds))
                DrawRaisedOutline(bounds, SelectedChunkColor, SelectedChunkLineWidth);

            if (TileWorldCreatorChunkGizmoBoundsCollector.TryCollectSelectedActualBounds(selected, out Bounds actualBounds))
                DrawRaisedOutline(actualBounds, ActualChunkColor, ManagerLineWidth);
        }

        private static void DrawCluster(ClusterIdentifier cluster, Color color, float lineWidth)
        {
            if (cluster == null || !cluster.gameObject.scene.IsValid())
                return;

            if (TileWorldCreatorChunkGizmoBoundsCollector.TryCollectSelectedChunk(cluster.transform, out Bounds bounds))
                DrawRaisedOutline(bounds, color, lineWidth);

            if (TileWorldCreatorChunkGizmoBoundsCollector.TryCollectSelectedActualBounds(cluster.transform, out Bounds actualBounds))
                DrawRaisedOutline(actualBounds, ActualChunkColor, ManagerLineWidth);
        }

        private static void DrawManager(TileWorldCreatorManager manager)
        {
            if (manager == null)
                return;

            TileWorldCreatorChunkGizmoBoundsCollector.CollectManagerChunks(manager, BoundsBuffer);
            for (int i = 0; i < BoundsBuffer.Count; i++)
                DrawRaisedOutline(BoundsBuffer[i], ManagerChunkColor, ManagerLineWidth);

            BoundsBuffer.Clear();
        }

        private static void DrawRaisedOutline(Bounds bounds, Color color, float lineWidth)
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
            Handles.DrawAAPolyLine(lineWidth, p0, p1, p2, p3, p0);
            Handles.color = previous;
        }
    }
}
