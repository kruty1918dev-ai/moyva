using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal sealed class BotAnalyzerDiffEngine
    {
        public List<BotAnalyzerEvent> Diff(BotAnalyzerFrame previous, BotAnalyzerFrame current)
        {
            var events = new List<BotAnalyzerEvent>();
            if (current == null)
                return events;

            if (previous == null ||
                !string.Equals(previous.OwnerId, current.OwnerId, StringComparison.Ordinal))
            {
                events.Add(Create(
                    current,
                    BotAnalyzerEventType.System,
                    BotAnalyzerEventSeverity.Info,
                    "Analyzer capture started",
                    $"Monitoring bot '{current.OwnerId}' at round {current.Round}, global turn {current.GlobalTurn}."));
                return events;
            }

            DiffTurn(previous, current, events);
            DiffStrategy(previous, current, events);
            DiffGoal(previous, current, events);
            DiffCandidates(previous, current, events);
            DiffUnits(previous, current, events);
            DiffBuildings(previous, current, events);
            DiffRecruitment(previous, current, events);
            DiffResources(previous, current, events);
            DiffFog(previous, current, events);
            DiffMemory(previous, current, events);

            return events;
        }

        private static void DiffTurn(
            BotAnalyzerFrame previous,
            BotAnalyzerFrame current,
            List<BotAnalyzerEvent> events)
        {
            if (!previous.SelectedBotActive && current.SelectedBotActive)
            {
                events.Add(Create(
                    current,
                    BotAnalyzerEventType.TurnStarted,
                    BotAnalyzerEventSeverity.Info,
                    "Bot turn started",
                    $"Round {current.Round}, global turn {current.GlobalTurn}, phase {current.Phase}."));
            }
            else if (previous.SelectedBotActive && !current.SelectedBotActive)
            {
                events.Add(Create(
                    current,
                    BotAnalyzerEventType.TurnEnded,
                    BotAnalyzerEventSeverity.Info,
                    "Bot turn ended",
                    $"Bot '{current.OwnerId}' is no longer the active owner."));
            }
            else if (previous.GlobalTurn != current.GlobalTurn && current.SelectedBotActive)
            {
                events.Add(Create(
                    current,
                    BotAnalyzerEventType.TurnStarted,
                    BotAnalyzerEventSeverity.Info,
                    "New bot turn epoch",
                    $"Global turn {previous.GlobalTurn} → {current.GlobalTurn}."));
            }
        }

        private static void DiffStrategy(
            BotAnalyzerFrame previous,
            BotAnalyzerFrame current,
            List<BotAnalyzerEvent> events)
        {
            BotAnalyzerStrategyState a = previous.Strategy;
            BotAnalyzerStrategyState b = current.Strategy;
            if (a == null || b == null)
                return;

            if (a.Available != b.Available ||
                !string.Equals(a.Posture, b.Posture, StringComparison.Ordinal) ||
                a.Score != b.Score ||
                !string.Equals(a.Reason, b.Reason, StringComparison.Ordinal))
            {
                var e = Create(
                    current,
                    BotAnalyzerEventType.StrategyChanged,
                    BotAnalyzerEventSeverity.Decision,
                    "Strategy changed",
                    $"{Display(a.Posture)} → {Display(b.Posture)}\nScore: {b.Score}\nReason: {Display(b.Reason)}");
                e.HasScore = b.Available;
                e.Score = b.Score;
                events.Add(e);
            }
        }

        private static void DiffGoal(
            BotAnalyzerFrame previous,
            BotAnalyzerFrame current,
            List<BotAnalyzerEvent> events)
        {
            BotAnalyzerGoalState a = previous.Goal;
            BotAnalyzerGoalState b = current.Goal;
            if (a == null || b == null)
                return;

            if (a.Available != b.Available ||
                !string.Equals(a.Kind, b.Kind, StringComparison.Ordinal) ||
                a.Priority != b.Priority ||
                !string.Equals(a.TargetId, b.TargetId, StringComparison.Ordinal) ||
                a.HasTargetCell != b.HasTargetCell ||
                (a.HasTargetCell && a.TargetCell != b.TargetCell) ||
                !string.Equals(a.Reason, b.Reason, StringComparison.Ordinal))
            {
                string target = b.HasTargetCell
                    ? $" cell={b.TargetCell}"
                    : string.IsNullOrWhiteSpace(b.TargetId) ? string.Empty : $" target={b.TargetId}";

                var e = Create(
                    current,
                    BotAnalyzerEventType.GoalChanged,
                    BotAnalyzerEventSeverity.Decision,
                    "Goal changed",
                    $"{Display(a.Kind)} → {Display(b.Kind)}\nPriority: {b.Priority}{target}\nReason: {Display(b.Reason)}");
                e.TargetId = b.TargetId ?? string.Empty;
                e.HasToCell = b.HasTargetCell;
                e.ToCell = b.TargetCell;
                e.HasScore = b.Available;
                e.Score = b.Priority;
                events.Add(e);
            }
        }

        private static void DiffCandidates(
            BotAnalyzerFrame previous,
            BotAnalyzerFrame current,
            List<BotAnalyzerEvent> events)
        {
            string before = BotAnalyzerTraceCollector.Fingerprint(previous.Candidates);
            string after = BotAnalyzerTraceCollector.Fingerprint(current.Candidates);
            if (string.Equals(before, after, StringComparison.Ordinal))
                return;

            int count = current.Candidates?.Count ?? 0;
            events.Add(Create(
                current,
                BotAnalyzerEventType.CandidateSetChanged,
                BotAnalyzerEventSeverity.Decision,
                "Decision candidates updated",
                $"Planner trace contains {count} ranked candidate(s)."));

            BotAnalyzerCandidateState oldTop =
                previous.Candidates != null && previous.Candidates.Count > 0 ? previous.Candidates[0] : null;
            BotAnalyzerCandidateState newTop =
                current.Candidates != null && current.Candidates.Count > 0 ? current.Candidates[0] : null;

            if (!SameCandidate(oldTop, newTop))
            {
                string detail = newTop == null
                    ? "No candidate currently passes into the recorded top set."
                    : $"{newTop.Kind} / score {newTop.Score}\nCandidate: {newTop.CandidateId}\n{Display(newTop.Explanation)}";

                var e = Create(
                    current,
                    BotAnalyzerEventType.TopCandidateChanged,
                    BotAnalyzerEventSeverity.Decision,
                    "Top candidate changed",
                    detail);
                if (newTop != null)
                {
                    e.HasScore = true;
                    e.Score = newTop.Score;
                    e.ActorId = newTop.ActorId ?? string.Empty;
                    e.TargetId = newTop.TargetId ?? string.Empty;
                    e.HasToCell = newTop.HasTargetCell;
                    e.ToCell = newTop.TargetCell;
                }
                events.Add(e);
            }
        }

        private static void DiffUnits(
            BotAnalyzerFrame previous,
            BotAnalyzerFrame current,
            List<BotAnalyzerEvent> events)
        {
            var oldOwn = MapUnits(previous.OwnUnits);
            var newOwn = MapUnits(current.OwnUnits);

            foreach (KeyValuePair<string, BotAnalyzerUnitState> pair in newOwn)
            {
                if (!oldOwn.TryGetValue(pair.Key, out BotAnalyzerUnitState oldUnit))
                {
                    var e = Create(
                        current,
                        BotAnalyzerEventType.UnitSpawned,
                        BotAnalyzerEventSeverity.Action,
                        "Owned unit appeared",
                        $"{pair.Value.TypeId} '{pair.Value.UnitId}' at {pair.Value.Cell}.");
                    e.ActorId = pair.Value.UnitId;
                    e.HasToCell = true;
                    e.ToCell = pair.Value.Cell;
                    events.Add(e);
                    continue;
                }

                if (oldUnit.Cell != pair.Value.Cell)
                {
                    var e = Create(
                        current,
                        BotAnalyzerEventType.UnitMoved,
                        BotAnalyzerEventSeverity.Action,
                        "Unit moved",
                        $"{pair.Value.UnitId}: {oldUnit.Cell} → {pair.Value.Cell}.");
                    e.ActorId = pair.Value.UnitId;
                    e.HasFromCell = true;
                    e.FromCell = oldUnit.Cell;
                    e.HasToCell = true;
                    e.ToCell = pair.Value.Cell;
                    events.Add(e);
                }

                if (!Mathf.Approximately(oldUnit.Stamina, pair.Value.Stamina))
                {
                    events.Add(Create(
                        current,
                        BotAnalyzerEventType.UnitStateChanged,
                        BotAnalyzerEventSeverity.Trace,
                        "Unit stamina changed",
                        $"{pair.Value.UnitId}: {oldUnit.Stamina:0.##} → {pair.Value.Stamina:0.##}."));
                }
            }

            foreach (KeyValuePair<string, BotAnalyzerUnitState> pair in oldOwn)
            {
                if (newOwn.ContainsKey(pair.Key))
                    continue;

                var e = Create(
                    current,
                    BotAnalyzerEventType.UnitRemoved,
                    BotAnalyzerEventSeverity.Action,
                    "Owned unit removed",
                    $"{pair.Value.UnitId} is no longer present in the bot's own-unit snapshot.");
                e.ActorId = pair.Value.UnitId;
                e.HasFromCell = true;
                e.FromCell = pair.Value.Cell;
                events.Add(e);
            }

            var oldEnemies = MapUnits(previous.VisibleEnemyUnits);
            var newEnemies = MapUnits(current.VisibleEnemyUnits);

            foreach (KeyValuePair<string, BotAnalyzerUnitState> pair in newEnemies)
            {
                if (!oldEnemies.TryGetValue(pair.Key, out BotAnalyzerUnitState oldEnemy))
                {
                    var e = Create(
                        current,
                        BotAnalyzerEventType.Observation,
                        BotAnalyzerEventSeverity.Info,
                        "Enemy unit became visible",
                        $"{pair.Value.TypeId} '{pair.Value.UnitId}' observed at {pair.Value.Cell}.");
                    e.TargetId = pair.Value.UnitId;
                    e.HasToCell = true;
                    e.ToCell = pair.Value.Cell;
                    events.Add(e);
                }
                else if (oldEnemy.Cell != pair.Value.Cell)
                {
                    var e = Create(
                        current,
                        BotAnalyzerEventType.Observation,
                        BotAnalyzerEventSeverity.Info,
                        "Visible enemy moved",
                        $"{pair.Value.UnitId}: {oldEnemy.Cell} → {pair.Value.Cell}.");
                    e.TargetId = pair.Value.UnitId;
                    e.HasFromCell = true;
                    e.FromCell = oldEnemy.Cell;
                    e.HasToCell = true;
                    e.ToCell = pair.Value.Cell;
                    events.Add(e);
                }
            }

            foreach (KeyValuePair<string, BotAnalyzerUnitState> pair in oldEnemies)
            {
                if (newEnemies.ContainsKey(pair.Key))
                    continue;

                bool noLongerExists = current.ExistingUnitIds != null &&
                                     current.ExistingUnitIds.Count > 0 &&
                                     !current.ExistingUnitIds.Contains(pair.Key);

                var e = Create(
                    current,
                    noLongerExists ? BotAnalyzerEventType.CombatObserved : BotAnalyzerEventType.ObservationLost,
                    noLongerExists ? BotAnalyzerEventSeverity.Action : BotAnalyzerEventSeverity.Info,
                    noLongerExists ? "Visible enemy removed from world" : "Enemy left current vision",
                    noLongerExists
                        ? $"{pair.Value.UnitId} was previously visible at {pair.Value.Cell} and no longer exists in the unit registry."
                        : $"{pair.Value.UnitId} is no longer visible; no hidden position is exposed.");
                e.TargetId = pair.Value.UnitId;
                e.HasFromCell = true;
                e.FromCell = pair.Value.Cell;
                events.Add(e);
            }
        }

        private static void DiffBuildings(
            BotAnalyzerFrame previous,
            BotAnalyzerFrame current,
            List<BotAnalyzerEvent> events)
        {
            var oldOwn = MapBuildings(previous.OwnBuildings);
            var newOwn = MapBuildings(current.OwnBuildings);

            foreach (KeyValuePair<string, BotAnalyzerBuildingState> pair in newOwn)
            {
                BotAnalyzerBuildingState b = pair.Value;
                if (!oldOwn.TryGetValue(pair.Key, out BotAnalyzerBuildingState a))
                {
                    var e = Create(
                        current,
                        b.Operational ? BotAnalyzerEventType.BuildingPlaced : BotAnalyzerEventType.BuildingStarted,
                        BotAnalyzerEventSeverity.Action,
                        b.Operational ? "Building placed" : "Construction started",
                        $"{b.BuildingId} at {b.Cell}" +
                        (b.HasProgress ? $" ({b.CompletedTurns}/{b.RequiredTurns})" : string.Empty));
                    e.HasToCell = true;
                    e.ToCell = b.Cell;
                    events.Add(e);
                    continue;
                }

                if (!a.Operational && b.Operational)
                {
                    var e = Create(
                        current,
                        BotAnalyzerEventType.BuildingPlaced,
                        BotAnalyzerEventSeverity.Action,
                        "Construction completed",
                        $"{b.BuildingId} at {b.Cell} is now operational.");
                    e.HasToCell = true;
                    e.ToCell = b.Cell;
                    events.Add(e);
                }
                else if (b.HasProgress &&
                         (!a.HasProgress ||
                          a.CompletedTurns != b.CompletedTurns ||
                          a.RequiredTurns != b.RequiredTurns))
                {
                    var e = Create(
                        current,
                        BotAnalyzerEventType.BuildingProgress,
                        BotAnalyzerEventSeverity.Info,
                        "Construction progressed",
                        $"{b.BuildingId} at {b.Cell}: {b.CompletedTurns}/{b.RequiredTurns}.");
                    e.HasToCell = true;
                    e.ToCell = b.Cell;
                    events.Add(e);
                }
            }

            foreach (KeyValuePair<string, BotAnalyzerBuildingState> pair in oldOwn)
            {
                if (newOwn.ContainsKey(pair.Key))
                    continue;

                var e = Create(
                    current,
                    BotAnalyzerEventType.BuildingRemoved,
                    BotAnalyzerEventSeverity.Action,
                    "Owned building removed",
                    $"{pair.Value.BuildingId} at {pair.Value.Cell} is no longer present.");
                e.HasFromCell = true;
                e.FromCell = pair.Value.Cell;
                events.Add(e);
            }

            var oldEnemies = MapBuildings(previous.VisibleEnemyBuildings);
            var newEnemies = MapBuildings(current.VisibleEnemyBuildings);
            foreach (KeyValuePair<string, BotAnalyzerBuildingState> pair in newEnemies)
            {
                if (oldEnemies.ContainsKey(pair.Key))
                    continue;

                var e = Create(
                    current,
                    BotAnalyzerEventType.Observation,
                    BotAnalyzerEventSeverity.Info,
                    "Enemy building became visible",
                    $"{pair.Value.BuildingId} at {pair.Value.Cell}.");
                e.HasToCell = true;
                e.ToCell = pair.Value.Cell;
                events.Add(e);
            }

            foreach (KeyValuePair<string, BotAnalyzerBuildingState> pair in oldEnemies)
            {
                if (newEnemies.ContainsKey(pair.Key))
                    continue;

                bool removed = current.ExistingBuildingKeys != null &&
                               current.ExistingBuildingKeys.Count > 0 &&
                               !current.ExistingBuildingKeys.Contains(pair.Key);

                var e = Create(
                    current,
                    removed ? BotAnalyzerEventType.BuildingRemoved : BotAnalyzerEventType.ObservationLost,
                    removed ? BotAnalyzerEventSeverity.Action : BotAnalyzerEventSeverity.Info,
                    removed ? "Observed enemy building removed" : "Enemy building left current vision",
                    removed
                        ? $"{pair.Value.BuildingId} at {pair.Value.Cell} no longer exists in construction state."
                        : $"{pair.Value.BuildingId} is no longer visible; no hidden state is exposed.");
                e.HasFromCell = true;
                e.FromCell = pair.Value.Cell;
                events.Add(e);
            }
        }

        private static void DiffRecruitment(
            BotAnalyzerFrame previous,
            BotAnalyzerFrame current,
            List<BotAnalyzerEvent> events)
        {
            var oldMap = MapRecruitment(previous.Recruitment);
            var newMap = MapRecruitment(current.Recruitment);

            foreach (KeyValuePair<long, BotAnalyzerRecruitmentState> pair in newMap)
            {
                BotAnalyzerRecruitmentState b = pair.Value;
                if (!oldMap.TryGetValue(pair.Key, out BotAnalyzerRecruitmentState a))
                {
                    var e = Create(
                        current,
                        b.Ready ? BotAnalyzerEventType.RecruitmentReady : BotAnalyzerEventType.RecruitmentStarted,
                        BotAnalyzerEventSeverity.Action,
                        b.Ready ? "Recruitment ready" : "Recruitment started",
                        $"{b.UnitTypeId} queue #{b.QueueId} at {b.BuildingCell}: {b.CompletedTurns}/{b.TrainingTurns}.");
                    e.HasToCell = true;
                    e.ToCell = b.BuildingCell;
                    events.Add(e);
                    continue;
                }

                if (!a.Ready && b.Ready)
                {
                    var e = Create(
                        current,
                        BotAnalyzerEventType.RecruitmentReady,
                        BotAnalyzerEventSeverity.Action,
                        "Recruitment became ready",
                        $"{b.UnitTypeId} queue #{b.QueueId} is ready for deployment.");
                    e.HasToCell = true;
                    e.ToCell = b.BuildingCell;
                    events.Add(e);
                }
                else if (a.CompletedTurns != b.CompletedTurns)
                {
                    events.Add(Create(
                        current,
                        BotAnalyzerEventType.RecruitmentProgress,
                        BotAnalyzerEventSeverity.Info,
                        "Recruitment progressed",
                        $"{b.UnitTypeId} queue #{b.QueueId}: {b.CompletedTurns}/{b.TrainingTurns}."));
                }
            }

            foreach (KeyValuePair<long, BotAnalyzerRecruitmentState> pair in oldMap)
            {
                if (newMap.ContainsKey(pair.Key))
                    continue;

                BotAnalyzerRecruitmentState old = pair.Value;
                var e = Create(
                    current,
                    old.Ready ? BotAnalyzerEventType.RecruitmentDeployed : BotAnalyzerEventType.RecruitmentProgress,
                    old.Ready ? BotAnalyzerEventSeverity.Action : BotAnalyzerEventSeverity.Info,
                    old.Ready ? "Ready recruitment left queue" : "Recruitment queue entry removed",
                    $"{old.UnitTypeId} queue #{old.QueueId} is no longer present.");
                e.HasFromCell = true;
                e.FromCell = old.BuildingCell;
                events.Add(e);
            }
        }

        private static void DiffResources(
            BotAnalyzerFrame previous,
            BotAnalyzerFrame current,
            List<BotAnalyzerEvent> events)
        {
            var before = MapResources(previous.Resources);
            var after = MapResources(current.Resources);
            var ids = new HashSet<string>(before.Keys, StringComparer.Ordinal);
            ids.UnionWith(after.Keys);

            foreach (string id in ids)
            {
                before.TryGetValue(id, out BotAnalyzerResourceState a);
                after.TryGetValue(id, out BotAnalyzerResourceState b);
                float oldAmount = a?.Amount ?? 0f;
                float newAmount = b?.Amount ?? 0f;
                if (Mathf.Approximately(oldAmount, newAmount))
                    continue;

                string display = b?.DisplayName ?? a?.DisplayName ?? id;
                float delta = newAmount - oldAmount;
                events.Add(Create(
                    current,
                    BotAnalyzerEventType.ResourceChanged,
                    BotAnalyzerEventSeverity.Action,
                    "Resource changed",
                    $"{display}: {oldAmount:0.##} → {newAmount:0.##} ({delta:+0.##;-0.##;0})."));
            }
        }

        private static void DiffFog(
            BotAnalyzerFrame previous,
            BotAnalyzerFrame current,
            List<BotAnalyzerEvent> events)
        {
            BotAnalyzerFogState a = previous.Fog ?? new BotAnalyzerFogState();
            BotAnalyzerFogState b = current.Fog ?? new BotAnalyzerFogState();

            var oldVisible = new HashSet<Vector2Int>(a.VisibleCells ?? new List<Vector2Int>());
            var newVisible = new HashSet<Vector2Int>(b.VisibleCells ?? new List<Vector2Int>());
            var oldExplored = new HashSet<Vector2Int>(a.ExploredCells ?? new List<Vector2Int>());
            var newExplored = new HashSet<Vector2Int>(b.ExploredCells ?? new List<Vector2Int>());

            int becameVisible = 0;
            foreach (Vector2Int cell in newVisible)
                if (!oldVisible.Contains(cell))
                    becameVisible++;

            int becameExplored = 0;
            foreach (Vector2Int cell in newExplored)
                if (!oldExplored.Contains(cell))
                    becameExplored++;

            if (becameVisible > 0)
            {
                events.Add(Create(
                    current,
                    BotAnalyzerEventType.FogVisible,
                    BotAnalyzerEventSeverity.Info,
                    "Vision expanded",
                    $"{becameVisible} cell(s) became currently visible. Visible now: {b.VisibleCount}."));
            }

            if (becameExplored > 0)
            {
                events.Add(Create(
                    current,
                    BotAnalyzerEventType.FogExplored,
                    BotAnalyzerEventSeverity.Trace,
                    "Visible cells became remembered terrain",
                    $"{becameExplored} cell(s) are now Explored but not currently Visible."));
            }
        }

        private static void DiffMemory(
            BotAnalyzerFrame previous,
            BotAnalyzerFrame current,
            List<BotAnalyzerEvent> events)
        {
            string a = MemoryFingerprint(previous.Memory);
            string b = MemoryFingerprint(current.Memory);
            if (string.Equals(a, b, StringComparison.Ordinal))
                return;

            events.Add(Create(
                current,
                BotAnalyzerEventType.MemoryChanged,
                BotAnalyzerEventSeverity.Trace,
                "Bot memory changed",
                $"Known entity memory records: {previous.Memory?.Count ?? 0} → {current.Memory?.Count ?? 0}."));
        }

        private static BotAnalyzerEvent Create(
            BotAnalyzerFrame frame,
            BotAnalyzerEventType type,
            BotAnalyzerEventSeverity severity,
            string title,
            string detail)
        {
            return new BotAnalyzerEvent
            {
                EditorTime = frame.EditorTime,
                UtcTimestamp = frame.UtcTimestamp,
                GlobalTurn = frame.GlobalTurn,
                OwnerId = frame.OwnerId,
                Type = type,
                Severity = severity,
                Title = title ?? string.Empty,
                Detail = detail ?? string.Empty,
            };
        }

        private static Dictionary<string, BotAnalyzerUnitState> MapUnits(IReadOnlyList<BotAnalyzerUnitState> list)
        {
            var map = new Dictionary<string, BotAnalyzerUnitState>(StringComparer.Ordinal);
            if (list == null) return map;
            foreach (BotAnalyzerUnitState item in list)
                if (item != null && !string.IsNullOrWhiteSpace(item.UnitId))
                    map[item.UnitId] = item;
            return map;
        }

        private static Dictionary<string, BotAnalyzerBuildingState> MapBuildings(IReadOnlyList<BotAnalyzerBuildingState> list)
        {
            var map = new Dictionary<string, BotAnalyzerBuildingState>(StringComparer.Ordinal);
            if (list == null) return map;
            foreach (BotAnalyzerBuildingState item in list)
            {
                if (item == null) continue;
                string key = BotAnalyzerFrame.BuildingKey(item.OwnerId, item.BuildingId, item.Cell);
                map[key] = item;
            }
            return map;
        }

        private static Dictionary<long, BotAnalyzerRecruitmentState> MapRecruitment(IReadOnlyList<BotAnalyzerRecruitmentState> list)
        {
            var map = new Dictionary<long, BotAnalyzerRecruitmentState>();
            if (list == null) return map;
            foreach (BotAnalyzerRecruitmentState item in list)
                if (item != null)
                    map[item.QueueId] = item;
            return map;
        }

        private static Dictionary<string, BotAnalyzerResourceState> MapResources(IReadOnlyList<BotAnalyzerResourceState> list)
        {
            var map = new Dictionary<string, BotAnalyzerResourceState>(StringComparer.Ordinal);
            if (list == null) return map;
            foreach (BotAnalyzerResourceState item in list)
                if (item != null && !string.IsNullOrWhiteSpace(item.ResourceId))
                    map[item.ResourceId] = item;
            return map;
        }

        private static bool SameCandidate(BotAnalyzerCandidateState a, BotAnalyzerCandidateState b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return a.Score == b.Score &&
                   string.Equals(a.CandidateId, b.CandidateId, StringComparison.Ordinal) &&
                   string.Equals(a.Kind, b.Kind, StringComparison.Ordinal);
        }

        private static string MemoryFingerprint(IReadOnlyList<BotAnalyzerMemoryState> memory)
        {
            if (memory == null || memory.Count == 0)
                return string.Empty;

            var parts = new List<string>(memory.Count);
            foreach (BotAnalyzerMemoryState item in memory)
            {
                if (item == null) continue;
                parts.Add($"{item.EntityId}|{item.Kind}|{item.LastKnownCell.x},{item.LastKnownCell.y}|{item.LastSeenGlobalTurn}|{item.LastKnownHp}|{item.ConfirmedDestroyed}");
            }
            parts.Sort(StringComparer.Ordinal);
            return string.Join(";", parts);
        }

        private static string Display(string value)
            => string.IsNullOrWhiteSpace(value) ? "Unavailable" : value;
    }
}
