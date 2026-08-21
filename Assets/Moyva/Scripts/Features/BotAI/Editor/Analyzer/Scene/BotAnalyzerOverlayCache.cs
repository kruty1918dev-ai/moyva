using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal sealed class BotAnalyzerOverlayCache
    {
        internal sealed class MovementSegment
        {
            public string UnitId = string.Empty;
            public Vector2Int From;
            public Vector2Int To;
            public double EventTime;
        }

        internal sealed class Marker
        {
            public string Text = string.Empty;
            public Vector2Int Cell;
            public double EventTime;
            public BotAnalyzerEventType Type;
        }

        private long _frameSequence = -1;
        private long _lastEventSequence = -1;

        public BotAnalyzerFrame Frame { get; private set; }
        public readonly List<MovementSegment> MovementSegments = new();
        public readonly List<Marker> Markers = new();

        public void Refresh(
            BotAnalyzerFrame frame,
            IReadOnlyList<BotAnalyzerEvent> events,
            double now,
            BotAnalyzerSettings settings)
        {
            if (frame == null)
            {
                Clear();
                return;
            }

            long newestEvent = events != null && events.Count > 0
                ? events[events.Count - 1]?.Sequence ?? -1
                : -1;

            bool sameFrame = _frameSequence == frame.Sequence;
            bool sameEvents = _lastEventSequence == newestEvent;
            if (sameFrame && sameEvents)
            {
                PurgeExpired(now, settings);
                return;
            }

            Frame = frame;
            _frameSequence = frame.Sequence;
            _lastEventSequence = newestEvent;

            MovementSegments.Clear();
            Markers.Clear();

            double trailAge = Math.Max(1f, settings?.MovementTrailSeconds ?? 8f);
            double markerAge = Math.Max(0.5f, settings?.ActionMarkerSeconds ?? 5f);

            if (events != null)
            {
                for (int i = Math.Max(0, events.Count - 500); i < events.Count; i++)
                {
                    BotAnalyzerEvent e = events[i];
                    if (e == null)
                        continue;

                    double age = Math.Max(0d, now - e.EditorTime);

                    if (e.Type == BotAnalyzerEventType.UnitMoved &&
                        e.HasFromCell &&
                        e.HasToCell &&
                        age <= trailAge)
                    {
                        MovementSegments.Add(new MovementSegment
                        {
                            UnitId = e.ActorId,
                            From = e.FromCell,
                            To = e.ToCell,
                            EventTime = e.EditorTime,
                        });
                    }

                    if (age <= markerAge &&
                        TryResolveMarkerCell(e, out Vector2Int cell))
                    {
                        string text = MarkerText(e);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            Markers.Add(new Marker
                            {
                                Text = text,
                                Cell = cell,
                                EventTime = e.EditorTime,
                                Type = e.Type,
                            });
                        }
                    }
                }
            }

            PurgeExpired(now, settings);
        }

        public void Clear()
        {
            Frame = null;
            _frameSequence = -1;
            _lastEventSequence = -1;
            MovementSegments.Clear();
            Markers.Clear();
        }

        private void PurgeExpired(double now, BotAnalyzerSettings settings)
        {
            double trailAge = Math.Max(1f, settings?.MovementTrailSeconds ?? 8f);
            double markerAge = Math.Max(0.5f, settings?.ActionMarkerSeconds ?? 5f);

            MovementSegments.RemoveAll(x => x == null || now - x.EventTime > trailAge);
            Markers.RemoveAll(x => x == null || now - x.EventTime > markerAge);
        }

        private static bool TryResolveMarkerCell(BotAnalyzerEvent e, out Vector2Int cell)
        {
            if (e.HasToCell)
            {
                cell = e.ToCell;
                return true;
            }
            if (e.HasFromCell)
            {
                cell = e.FromCell;
                return true;
            }

            cell = default;
            return false;
        }

        private static string MarkerText(BotAnalyzerEvent e)
        {
            return e.Type switch
            {
                BotAnalyzerEventType.BuildingStarted => "BUILD START",
                BotAnalyzerEventType.BuildingPlaced => "BUILD",
                BotAnalyzerEventType.UnitSpawned => "UNIT SPAWN",
                BotAnalyzerEventType.UnitMoved => "MOVE",
                BotAnalyzerEventType.RecruitmentReady => "READY",
                BotAnalyzerEventType.RecruitmentDeployed => "DEPLOY",
                BotAnalyzerEventType.CombatObserved => "COMBAT",
                BotAnalyzerEventType.GoalChanged => "GOAL",
                _ => e.Severity == BotAnalyzerEventSeverity.Action ? e.Title : string.Empty,
            };
        }
    }
}
