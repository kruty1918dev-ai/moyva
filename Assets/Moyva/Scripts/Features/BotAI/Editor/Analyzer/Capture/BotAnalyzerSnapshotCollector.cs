using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal sealed class BotAnalyzerSnapshotCollector
    {
        private readonly BotAnalyzerServices _services;
        private readonly BotAnalyzerFogCollector _fog;
        private readonly BotAnalyzerEconomyCollector _economy;
        private readonly BotAnalyzerRecruitmentCollector _recruitment;
        private readonly BotAnalyzerTraceCollector _trace;
        private long _sequence;

        public BotAnalyzerSnapshotCollector(BotAnalyzerServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _fog = new BotAnalyzerFogCollector(services);
            _economy = new BotAnalyzerEconomyCollector(services.Economy);
            _recruitment = new BotAnalyzerRecruitmentCollector(services.RecruitmentState);
            _trace = new BotAnalyzerTraceCollector(services.Trace);
        }

        public BotAnalyzerFrame Capture(
            string ownerId,
            double now,
            BotAnalyzerSettings settings,
            bool forceSlowCollectors = false)
        {
            if (_services.Turns == null ||
                _services.SnapshotBuilder == null ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                return null;
            }

            long globalTurn = _services.Turns.GlobalTurn;
            BotWorldSnapshot world;
            try
            {
                world = _services.SnapshotBuilder.Build(ownerId, globalTurn);
            }
            catch
            {
                return null;
            }

            if (world == null)
                return null;

            var frame = new BotAnalyzerFrame
            {
                Sequence = ++_sequence,
                EditorTime = now,
                UtcTimestamp = DateTime.UtcNow.ToString("O"),
                OwnerId = ownerId,
                Round = world.Round,
                GlobalTurn = world.GlobalTurn,
                Phase = world.Phase.ToString(),
                ActionsThisTurn = world.ActionsThisTurn,
                ActiveOwnerId = _services.Turns.ActiveOwnerId ?? string.Empty,
                SelectedBotActive = string.Equals(
                    _services.Turns.ActiveOwnerId,
                    ownerId,
                    StringComparison.Ordinal),
            };

            frame.Strategy = CaptureStrategy(ownerId);
            frame.Goal = CaptureGoal(ownerId);
            frame.Resources = _economy.Collect(ownerId, now, settings, forceSlowCollectors);
            frame.Fog = _fog.Collect(ownerId, now, settings, forceSlowCollectors);
            frame.Recruitment = _recruitment.Collect(ownerId, world);
            frame.Candidates = _trace.Collect(ownerId, now, settings, forceSlowCollectors);
            frame.Reasoning = CaptureReasoning(ownerId);

            MapUnits(world.OwnUnits, frame.OwnUnits);
            MapUnits(world.VisibleEnemyUnits, frame.VisibleEnemyUnits);
            MapBuildings(world.OwnBuildings, frame.OwnBuildings);
            MapBuildings(world.VisibleEnemyBuildings, frame.VisibleEnemyBuildings);
            MapMemory(world.Memory, frame.Memory);

            CaptureGlobalExistence(frame);
            return frame;
        }

        private List<BotAnalyzerReasoningState> CaptureReasoning(string ownerId)
        {
            var result = new List<BotAnalyzerReasoningState>();
            if (_services.Reasoning == null || string.IsNullOrWhiteSpace(ownerId))
                return result;

            IReadOnlyList<BotReasoningEntry> entries;
            try
            {
                entries = _services.Reasoning.GetEntries(ownerId);
            }
            catch
            {
                return result;
            }

            if (entries == null)
                return result;

            int start = Math.Max(0, entries.Count - 256);
            for (int i = start; i < entries.Count; i++)
            {
                BotReasoningEntry entry = entries[i];
                var mapped = new BotAnalyzerReasoningState
                {
                    Sequence = entry.Sequence,
                    GlobalTurn = entry.GlobalTurn,
                    Stage = entry.Stage.ToString(),
                    Headline = entry.Headline,
                    Narrative = entry.Narrative,
                    Score = entry.Score,
                    HasTargetCell = entry.TargetCell.HasValue,
                    TargetCell = entry.TargetCell.GetValueOrDefault(),
                    SubjectId = entry.SubjectId,
                };

                if (entry.Factors != null)
                {
                    for (int f = 0; f < entry.Factors.Count; f++)
                    {
                        BotSiteScoreFactor factor = entry.Factors[f];
                        mapped.Factors.Add(new BotAnalyzerReasoningFactorState
                        {
                            Key = factor.Key,
                            Label = factor.Label,
                            RawValue = factor.RawValue,
                            Weight = factor.Weight,
                            Contribution = factor.Contribution,
                            Detail = factor.Detail,
                        });
                    }
                }

                result.Add(mapped);
            }

            return result;
        }

        private BotAnalyzerStrategyState CaptureStrategy(string ownerId)
        {
            var result = new BotAnalyzerStrategyState();
            if (_services.StrategicState == null)
                return result;

            IReadOnlyList<BotStrategicStateSnapshot> states;
            try
            {
                states = _services.StrategicState.CaptureStrategicState();
            }
            catch
            {
                return result;
            }

            if (states == null)
                return result;

            for (int i = 0; i < states.Count; i++)
            {
                BotStrategicStateSnapshot state = states[i];
                if (!string.Equals(state.OwnerId, ownerId, StringComparison.Ordinal))
                    continue;

                result.Available = true;
                result.Posture = state.Posture.ToString();
                result.Score = state.PostureScore;
                result.Reason = state.Reason;
                result.GlobalTurn = state.GlobalTurn;
                break;
            }
            return result;
        }

        private BotAnalyzerGoalState CaptureGoal(string ownerId)
        {
            var result = new BotAnalyzerGoalState();
            if (_services.Goals == null)
                return result;

            BotGoalSnapshot goal;
            try
            {
                if (!_services.Goals.TryGet(ownerId, out goal))
                    return result;
            }
            catch
            {
                return result;
            }

            result.Available = true;
            result.Kind = goal.Kind.ToString();
            result.Priority = goal.Priority;
            result.CreatedTurn = goal.CreatedTurn;
            result.HoldUntilTurn = goal.MinimumHoldUntilTurn;
            result.TargetId = goal.TargetId;
            result.HasTargetCell = goal.TargetCell.HasValue;
            result.TargetCell = goal.TargetCell.GetValueOrDefault();
            result.Reason = goal.Reason;
            return result;
        }

        private void MapUnits(
            IReadOnlyList<BotUnitSnapshot> source,
            List<BotAnalyzerUnitState> target)
        {
            if (source == null)
                return;

            for (int i = 0; i < source.Count; i++)
            {
                BotUnitSnapshot unit = source[i];
                target.Add(new BotAnalyzerUnitState
                {
                    UnitId = unit.UnitId,
                    OwnerId = unit.OwnerId,
                    TypeId = unit.TypeId,
                    Cell = unit.Position,
                    Stamina = unit.Stamina,
                    TacticalRole = ResolveRole(unit.TypeId),
                });
            }
        }

        private string ResolveRole(string typeId)
        {
            if (_services.UnitConfigs == null ||
                _services.RoleResolver == null ||
                string.IsNullOrWhiteSpace(typeId))
            {
                return "Unknown";
            }

            try
            {
                UnitClassConfig config = _services.UnitConfigs.GetConfig(typeId);
                if (config == null)
                    return "Unknown";
                return _services.RoleResolver.Resolve(config).ToString();
            }
            catch
            {
                return "Unknown";
            }
        }

        private void MapBuildings(
            IReadOnlyList<BotBuildingSnapshot> source,
            List<BotAnalyzerBuildingState> target)
        {
            if (source == null)
                return;

            for (int i = 0; i < source.Count; i++)
            {
                BotBuildingSnapshot building = source[i];
                var state = new BotAnalyzerBuildingState
                {
                    BuildingId = building.BuildingId,
                    OwnerId = building.OwnerId,
                    Cell = building.Position,
                    Operational = true,
                };

                if (_services.ConstructionLifecycle != null)
                {
                    try
                    {
                        state.Operational = _services.ConstructionLifecycle.IsOperational(building.Position);
                        state.HasProgress = _services.ConstructionLifecycle.TryGetProgress(
                            building.Position,
                            out state.CompletedTurns,
                            out state.RequiredTurns);
                    }
                    catch
                    {
                        state.Operational = true;
                        state.HasProgress = false;
                    }
                }

                target.Add(state);
            }
        }

        private static void MapMemory(
            IReadOnlyList<BotKnownEntityMemory> source,
            List<BotAnalyzerMemoryState> target)
        {
            if (source == null)
                return;

            for (int i = 0; i < source.Count; i++)
            {
                BotKnownEntityMemory memory = source[i];
                target.Add(new BotAnalyzerMemoryState
                {
                    EntityId = memory.EntityId,
                    Kind = memory.Kind.ToString(),
                    OwnerId = memory.OwnerId,
                    TypeId = memory.TypeId,
                    LastKnownCell = memory.LastKnownPosition,
                    LastSeenGlobalTurn = memory.LastSeenGlobalTurn,
                    LastKnownHp = memory.LastKnownHp,
                    ConfirmedDestroyed = memory.WasConfirmedDestroyed,
                });
            }
        }

        private void CaptureGlobalExistence(BotAnalyzerFrame frame)
        {
            IReadOnlyCollection<string> unitIds = null;
            try
            {
                unitIds = _services.Units?.GetAllUnitIds();
            }
            catch
            {
                unitIds = null;
            }

            if (unitIds != null)
            {
                foreach (string id in unitIds)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                        frame.ExistingUnitIds.Add(id);
                }
            }

            IReadOnlyList<ConstructionSavedPlacement> placements = null;
            try
            {
                placements = _services.ConstructionSnapshot?.GetSavedPlacements();
            }
            catch
            {
                placements = null;
            }

            if (placements == null)
                return;

            for (int i = 0; i < placements.Count; i++)
            {
                ConstructionSavedPlacement placement = placements[i];
                frame.ExistingBuildingKeys.Add(
                    BotAnalyzerFrame.BuildingKey(
                        placement.OwnerId,
                        placement.BuildingId,
                        placement.Position));
            }
        }
    }
}
