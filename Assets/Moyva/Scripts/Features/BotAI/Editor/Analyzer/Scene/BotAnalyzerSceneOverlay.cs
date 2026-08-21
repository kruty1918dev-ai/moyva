using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal sealed class BotAnalyzerSceneOverlay : IDisposable
    {
        private const float OverlayLift = 0.12f;

        private readonly BotAnalyzerRuntimeBridge _bridge;
        private readonly BotAnalyzerSession _session;
        private readonly BotAnalyzerSettings _settings;
        private readonly BotAnalyzerOverlayCache _cache = new();
        private bool _enabled;

        public BotAnalyzerSceneOverlay(
            BotAnalyzerRuntimeBridge bridge,
            BotAnalyzerSession session,
            BotAnalyzerSettings settings)
        {
            _bridge = bridge;
            _session = session;
            _settings = settings;
        }

        public void Enable()
        {
            if (_enabled)
                return;

            _enabled = true;
            SceneView.duringSceneGui -= OnSceneGui;
            SceneView.duringSceneGui += OnSceneGui;
        }

        public void Disable()
        {
            if (!_enabled)
                return;

            _enabled = false;
            SceneView.duringSceneGui -= OnSceneGui;
            _cache.Clear();
            SceneView.RepaintAll();
        }

        public void NotifyDataChanged()
        {
            if (_enabled)
                SceneView.RepaintAll();
        }

        public void Dispose() => Disable();

        private void OnSceneGui(SceneView sceneView)
        {
            if (!_enabled ||
                !EditorApplication.isPlaying ||
                !_bridge.Connected ||
                Event.current == null ||
                Event.current.type != EventType.Repaint)
            {
                return;
            }

            BotAnalyzerFrame frame = _bridge.LatestFrame ?? _session.LatestFrame;
            IGridProjection projection = _bridge.Services?.GridProjection;
            if (frame == null || projection == null)
                return;

            double now = EditorApplication.timeSinceStartup;
            _cache.Refresh(frame, _session.Events, now, _settings);

            Color previousColor = Handles.color;
            CompareFunction previousZTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;

            try
            {
                DrawFog(frame, projection);
                DrawMovement(projection, now);
                DrawBuildings(frame, projection);
                DrawUnits(frame, projection);
                DrawGoal(frame, projection);
                DrawTopCandidate(frame, projection);
                DrawMarkers(projection, now);
                DrawCoordinates(frame, projection);
            }
            finally
            {
                Handles.color = previousColor;
                Handles.zTest = previousZTest;
            }
        }

        private void DrawFog(BotAnalyzerFrame frame, IGridProjection projection)
        {
            if (frame.Fog == null)
                return;

            if (_settings.ShowFogExplored && frame.Fog.ExploredCells != null)
            {
                Color fill = BotAnalyzerSceneStyles.WithAlpha(
                    BotAnalyzerSceneStyles.ExploredFog,
                    _settings.ExploredFogAlpha);

                Color outline = BotAnalyzerSceneStyles.WithAlpha(
                    BotAnalyzerSceneStyles.ExploredFog,
                    Mathf.Min(0.75f, _settings.ExploredFogAlpha * 3f));

                foreach (Vector2Int cell in frame.Fog.ExploredCells)
                    DrawCell(cell, projection, fill, outline);
            }

            if (_settings.ShowFogVisible && frame.Fog.VisibleCells != null)
            {
                Color fill = BotAnalyzerSceneStyles.WithAlpha(
                    BotAnalyzerSceneStyles.VisibleFog,
                    _settings.VisibleFogAlpha);

                Color outline = BotAnalyzerSceneStyles.WithAlpha(
                    BotAnalyzerSceneStyles.VisibleFog,
                    Mathf.Min(1f, _settings.VisibleFogAlpha * 3f));

                foreach (Vector2Int cell in frame.Fog.VisibleCells)
                    DrawCell(cell, projection, fill, outline);
            }
        }

        private void DrawUnits(BotAnalyzerFrame frame, IGridProjection projection)
        {
            if (_settings.ShowOwnUnits)
            {
                DrawUnitList(
                    frame.OwnUnits,
                    projection,
                    BotAnalyzerSceneStyles.OwnUnit,
                    "BOT");
            }

            if (_settings.ShowVisibleEnemies)
            {
                DrawUnitList(
                    frame.VisibleEnemyUnits,
                    projection,
                    BotAnalyzerSceneStyles.Enemy,
                    "VISIBLE ENEMY");
            }
        }

        private static void DrawUnitList(
            IReadOnlyList<BotAnalyzerUnitState> units,
            IGridProjection projection,
            Color color,
            string prefix)
        {
            if (units == null)
                return;

            foreach (BotAnalyzerUnitState unit in units)
            {
                if (unit == null)
                    continue;

                Vector3 center = Lift(projection.GridToWorld(unit.Cell), 0.24f);
                Handles.color = color;
                Handles.DrawWireDisc(center, Vector3.up, 0.37f);

                string label =
                    $"{prefix} • {unit.UnitId}\n{unit.TypeId} • {unit.TacticalRole}\nStamina {unit.Stamina:0.##}";
                Handles.Label(center + Vector3.up * 0.42f, label, BotAnalyzerSceneStyles.SmallLabel);
            }
        }

        private void DrawBuildings(BotAnalyzerFrame frame, IGridProjection projection)
        {
            if (_settings.ShowOwnBuildings)
            {
                DrawBuildingList(
                    frame.OwnBuildings,
                    projection,
                    BotAnalyzerSceneStyles.OwnBuilding,
                    "BOT");
            }

            if (_settings.ShowVisibleEnemies)
            {
                DrawBuildingList(
                    frame.VisibleEnemyBuildings,
                    projection,
                    BotAnalyzerSceneStyles.Enemy,
                    "VISIBLE ENEMY");
            }
        }

        private static void DrawBuildingList(
            IReadOnlyList<BotAnalyzerBuildingState> buildings,
            IGridProjection projection,
            Color color,
            string prefix)
        {
            if (buildings == null)
                return;

            foreach (BotAnalyzerBuildingState building in buildings)
            {
                if (building == null)
                    continue;

                Vector3 center = Lift(projection.GridToWorld(building.Cell), 0.25f);
                Handles.color = color;
                Handles.DrawWireCube(center, new Vector3(0.78f, 0.28f, 0.78f));

                string lifecycle = building.Operational
                    ? "Operational"
                    : building.HasProgress
                        ? $"Building {building.CompletedTurns}/{building.RequiredTurns}"
                        : "Under construction";

                Handles.Label(
                    center + Vector3.up * 0.48f,
                    $"{prefix} • {building.BuildingId}\n{lifecycle}",
                    BotAnalyzerSceneStyles.SmallLabel);
            }
        }

        private void DrawMovement(IGridProjection projection, double now)
        {
            if (!_settings.ShowMovementTrails)
                return;

            double lifetime = Math.Max(1f, _settings.MovementTrailSeconds);
            foreach (BotAnalyzerOverlayCache.MovementSegment segment in _cache.MovementSegments)
            {
                if (segment == null)
                    continue;

                float normalized = 1f - Mathf.Clamp01((float)((now - segment.EventTime) / lifetime));
                Color color = BotAnalyzerSceneStyles.WithAlpha(
                    BotAnalyzerSceneStyles.Movement,
                    0.18f + normalized * 0.82f);

                Vector3 a = Lift(projection.GridToWorld(segment.From), 0.32f);
                Vector3 b = Lift(projection.GridToWorld(segment.To), 0.32f);

                Handles.color = color;
                Handles.DrawAAPolyLine(3.5f, a, b);
                DrawArrow(a, b, color);

                if (!string.IsNullOrWhiteSpace(segment.UnitId))
                {
                    Handles.Label(
                        Vector3.Lerp(a, b, 0.5f) + Vector3.up * 0.15f,
                        segment.UnitId,
                        BotAnalyzerSceneStyles.SmallLabel);
                }
            }
        }

        private void DrawGoal(BotAnalyzerFrame frame, IGridProjection projection)
        {
            if (!_settings.ShowCurrentGoal ||
                frame.Goal?.Available != true ||
                !frame.Goal.HasTargetCell)
            {
                return;
            }

            Vector3 center = Lift(projection.GridToWorld(frame.Goal.TargetCell), 0.40f);
            Handles.color = BotAnalyzerSceneStyles.Goal;
            Handles.DrawWireDisc(center, Vector3.up, 0.58f);
            Handles.DrawWireDisc(center, Vector3.up, 0.66f);
            Handles.Label(
                center + Vector3.up * 0.62f,
                $"GOAL • {frame.Goal.Kind}\nPriority {frame.Goal.Priority}",
                BotAnalyzerSceneStyles.GoalLabel);
        }

        private void DrawTopCandidate(BotAnalyzerFrame frame, IGridProjection projection)
        {
            if (!_settings.ShowTopCandidate ||
                frame.Candidates == null ||
                frame.Candidates.Count == 0)
            {
                return;
            }

            BotAnalyzerCandidateState top = frame.Candidates[0];
            if (top == null || !top.HasTargetCell)
                return;

            Vector3 center = Lift(projection.GridToWorld(top.TargetCell), 0.46f);
            Handles.color = BotAnalyzerSceneStyles.Candidate;
            Handles.DrawWireCube(center, new Vector3(0.64f, 0.16f, 0.64f));

            if (_settings.ShowCandidateLabels)
            {
                Handles.Label(
                    center + Vector3.up * 0.58f,
                    $"TOP CANDIDATE\n{top.Kind} • {top.Score}",
                    BotAnalyzerSceneStyles.GoalLabel);
            }
        }

        private void DrawMarkers(IGridProjection projection, double now)
        {
            if (!_settings.ShowLastActions)
                return;

            double lifetime = Math.Max(0.5f, _settings.ActionMarkerSeconds);
            foreach (BotAnalyzerOverlayCache.Marker marker in _cache.Markers)
            {
                if (marker == null)
                    continue;

                float normalized = 1f - Mathf.Clamp01((float)((now - marker.EventTime) / lifetime));
                Color color = BotAnalyzerSceneStyles.WithAlpha(
                    BotAnalyzerSceneStyles.Action,
                    0.20f + normalized * 0.80f);

                Vector3 center = Lift(projection.GridToWorld(marker.Cell), 0.55f);
                Handles.color = color;
                Handles.DrawWireDisc(center, Vector3.up, 0.46f);
                Handles.Label(
                    center + Vector3.up * 0.72f,
                    marker.Text,
                    BotAnalyzerSceneStyles.Label);
            }
        }

        private void DrawCoordinates(BotAnalyzerFrame frame, IGridProjection projection)
        {
            if (!_settings.ShowCellCoordinates || frame.Fog?.VisibleCells == null)
                return;

            int cap = Math.Min(300, frame.Fog.VisibleCells.Count);
            for (int i = 0; i < cap; i++)
            {
                Vector2Int cell = frame.Fog.VisibleCells[i];
                Vector3 center = Lift(projection.GridToWorld(cell), 0.08f);
                Handles.Label(center, $"{cell.x},{cell.y}", BotAnalyzerSceneStyles.SmallLabel);
            }
        }

        private static void DrawCell(
            Vector2Int cell,
            IGridProjection projection,
            Color fill,
            Color outline)
        {
            Vector3 center = projection.GridToWorld(cell);
            Vector3 right = projection.GridToWorld(cell + Vector2Int.right) - center;
            Vector3 up = projection.GridToWorld(cell + Vector2Int.up) - center;

            if (right.sqrMagnitude < 0.0001f)
                right = Vector3.right;
            if (up.sqrMagnitude < 0.0001f)
                up = Vector3.forward;

            center = Lift(center, OverlayLift);
            right *= 0.49f;
            up *= 0.49f;

            Vector3[] points =
            {
                center - right - up,
                center + right - up,
                center + right + up,
                center - right + up,
            };

            Handles.color = fill;
            Handles.DrawAAConvexPolygon(points);

            Handles.color = outline;
            Handles.DrawAAPolyLine(
                1.3f,
                points[0],
                points[1],
                points[2],
                points[3],
                points[0]);
        }

        private static void DrawArrow(Vector3 from, Vector3 to, Color color)
        {
            Vector3 direction = to - from;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Handles.color = color;
            Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            Handles.ArrowHandleCap(
                0,
                Vector3.Lerp(from, to, 0.78f),
                rotation,
                0.35f,
                EventType.Repaint);
        }

        private static Vector3 Lift(Vector3 position, float amount)
            => position + Vector3.up * amount;
    }
}
