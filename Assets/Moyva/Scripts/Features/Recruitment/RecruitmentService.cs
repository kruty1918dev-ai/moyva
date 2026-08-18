using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Recruitment
{
    public enum RecruitmentJobStatus { Training, ReadyToDeploy }

    public readonly struct RecruitmentJobView
    {
        public RecruitmentJobView(string id, string unitTypeId, int remainingTurns, RecruitmentJobStatus status)
        {
            Id = id;
            UnitTypeId = unitTypeId;
            RemainingTurns = remainingTurns;
            Status = status;
        }
        public string Id { get; }
        public string UnitTypeId { get; }
        public int RemainingTurns { get; }
        public RecruitmentJobStatus Status { get; }
    }

    public interface IRecruitmentService
    {
        bool TryEnqueue(Vector2Int buildingPosition, string unitTypeId, string requesterOwnerId, out string reason);
        bool TryCancel(Vector2Int buildingPosition, string jobId, string requesterOwnerId, out string reason);
        IReadOnlyList<RecruitmentJobView> GetQueue(Vector2Int buildingPosition);
    }

    internal sealed class RecruitmentService : IRecruitmentService, ITurnParticipant, ISaveModule
    {
        private const int SaveMagic = unchecked((int)0x52454352);
        private const int SaveVersion = 1;

        private sealed class Job
        {
            public string Id;
            public string UnitTypeId;
            public int RemainingTurns;
            public RecruitmentJobStatus Status;
            public long EnqueuedTurn;
            public Dictionary<string, float> Costs;
        }

        private sealed class QueueState
        {
            public Vector2Int Position;
            public string BuildingId;
            public string OwnerId;
            public readonly List<Job> Jobs = new();
        }

        private readonly ITurnService _turns;
        private readonly IConstructionService _construction;
        private readonly IConstructionLifecycle _lifecycle;
        private readonly IBuildingRegistry _buildings;
        private readonly IEconomyInfoMediator _economy;
        private readonly IUnitFactory _units;
        private readonly IGridService _grid;
        private readonly IObjectsMapService _objects;
        private readonly Dictionary<Vector2Int, QueueState> _queues = new();
        private long _nextJobId = 1;

        public RecruitmentService(
            ITurnService turns,
            IConstructionService construction,
            IConstructionLifecycle lifecycle,
            IBuildingRegistry buildings,
            IEconomyInfoMediator economy,
            IUnitFactory units,
            IGridService grid,
            IObjectsMapService objects)
        {
            _turns = turns;
            _construction = construction;
            _lifecycle = lifecycle;
            _buildings = buildings;
            _economy = economy;
            _units = units;
            _grid = grid;
            _objects = objects;
        }

        public int TurnOrder => 50;

        public bool TryEnqueue(Vector2Int position, string unitTypeId, string requesterOwnerId, out string reason)
        {
            if (!_turns.CanOwnerAct(requesterOwnerId, out reason))
                return false;
            if (!TryResolveBuilding(position, requesterOwnerId, out BuildingDefinition building, out UnitRecruitmentBuildingModule module, out reason))
                return false;
            if (!_lifecycle.IsOperational(position))
            {
                reason = "Будівництво ще не завершено.";
                return false;
            }

            UnitRecruitmentRecipeDefinition recipe = module.Recipes?.Find(r => string.Equals(r?.UnitTypeId, unitTypeId, StringComparison.Ordinal));
            if (recipe == null)
            {
                reason = $"Будівля не виробляє '{unitTypeId}'.";
                return false;
            }

            QueueState queue = GetOrCreateQueue(position, building.Id, requesterOwnerId);
            if (queue.Jobs.Count >= Mathf.Max(1, module.QueueCapacity))
            {
                reason = "Черга найму заповнена.";
                return false;
            }

            Dictionary<string, float> costs = BuildCosts(recipe);
            if (!_economy.TryConsumeOwnerPoolResources(requesterOwnerId, costs, out reason))
                return false;

            queue.Jobs.Add(new Job
            {
                Id = $"recruit-{_nextJobId++}",
                UnitTypeId = recipe.UnitTypeId,
                RemainingTurns = Mathf.Max(1, recipe.TrainingTurns),
                Status = RecruitmentJobStatus.Training,
                EnqueuedTurn = _turns.GlobalTurn,
                Costs = costs,
            });
            _turns.TryRecordAction(requesterOwnerId, "unit-recruitment");
            reason = null;
            return true;
        }

        public bool TryCancel(Vector2Int position, string jobId, string requesterOwnerId, out string reason)
        {
            if (!_turns.CanOwnerAct(requesterOwnerId, out reason)
                || !_queues.TryGetValue(position, out QueueState queue)
                || !string.Equals(queue.OwnerId, requesterOwnerId, StringComparison.Ordinal))
                return false;
            Job job = queue.Jobs.Find(candidate => string.Equals(candidate.Id, jobId, StringComparison.Ordinal));
            if (job == null || job.Status != RecruitmentJobStatus.Training)
            {
                reason = "Замовлення не можна скасувати.";
                return false;
            }
            queue.Jobs.Remove(job);
            _economy.RefundOwnerPoolResources(requesterOwnerId, job.Costs);
            reason = null;
            return true;
        }

        public IReadOnlyList<RecruitmentJobView> GetQueue(Vector2Int position)
        {
            var result = new List<RecruitmentJobView>();
            if (!_queues.TryGetValue(position, out QueueState queue))
                return result;
            foreach (Job job in queue.Jobs)
                result.Add(new RecruitmentJobView(job.Id, job.UnitTypeId, job.RemainingTurns, job.Status));
            return result;
        }

        public void OnTurnStarted(TurnContext context)
        {
            foreach (QueueState queue in _queues.Values)
            {
                if (!string.Equals(queue.OwnerId, context.Faction.OwnerId, StringComparison.Ordinal) || queue.Jobs.Count == 0)
                    continue;
                Job job = queue.Jobs[0];
                if (job.Status == RecruitmentJobStatus.Training && job.EnqueuedTurn < context.GlobalTurn)
                {
                    job.RemainingTurns--;
                    if (job.RemainingTurns <= 0)
                        job.Status = RecruitmentJobStatus.ReadyToDeploy;
                }
            }
        }

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }

        public void OnSave(ISaveContext context)
        {
            context.Writer.Write(SaveMagic);
            context.Writer.Write(SaveVersion);
            context.Writer.Write(_nextJobId);
            context.Writer.Write(_queues.Count);
            foreach (QueueState queue in _queues.Values)
            {
                context.Writer.Write(queue.Position.x);
                context.Writer.Write(queue.Position.y);
                context.Writer.Write(queue.BuildingId ?? string.Empty);
                context.Writer.Write(queue.OwnerId ?? string.Empty);
                context.Writer.Write(queue.Jobs.Count);
                foreach (Job job in queue.Jobs)
                {
                    context.Writer.Write(job.Id);
                    context.Writer.Write(job.UnitTypeId);
                    context.Writer.Write(job.RemainingTurns);
                    context.Writer.Write((int)job.Status);
                    context.Writer.Write(job.EnqueuedTurn);
                    context.Writer.Write(job.Costs.Count);
                    foreach (KeyValuePair<string, float> cost in job.Costs)
                    {
                        context.Writer.Write(cost.Key);
                        context.Writer.Write(cost.Value);
                    }
                }
            }
        }

        public void OnLoad(ISaveContext context)
        {
            if (context.Reader.ReadInt32() != SaveMagic || context.Reader.ReadInt32() != SaveVersion)
                throw new System.IO.InvalidDataException("Unsupported recruitment save block.");
            _nextJobId = context.Reader.ReadInt64();
            _queues.Clear();
            int queueCount = context.Reader.ReadInt32();
            for (int queueIndex = 0; queueIndex < queueCount; queueIndex++)
            {
                var queue = new QueueState
                {
                    Position = new Vector2Int(context.Reader.ReadInt32(), context.Reader.ReadInt32()),
                    BuildingId = context.Reader.ReadString(),
                    OwnerId = context.Reader.ReadString(),
                };
                int jobCount = context.Reader.ReadInt32();
                for (int jobIndex = 0; jobIndex < jobCount; jobIndex++)
                {
                    var job = new Job
                    {
                        Id = context.Reader.ReadString(),
                        UnitTypeId = context.Reader.ReadString(),
                        RemainingTurns = context.Reader.ReadInt32(),
                        Status = (RecruitmentJobStatus)context.Reader.ReadInt32(),
                        EnqueuedTurn = context.Reader.ReadInt64(),
                        Costs = new Dictionary<string, float>(StringComparer.Ordinal),
                    };
                    int costs = context.Reader.ReadInt32();
                    for (int costIndex = 0; costIndex < costs; costIndex++)
                        job.Costs[context.Reader.ReadString()] = context.Reader.ReadSingle();
                    queue.Jobs.Add(job);
                }
                _queues[queue.Position] = queue;
            }
        }

        private bool TryResolveBuilding(Vector2Int position, string ownerId, out BuildingDefinition definition, out UnitRecruitmentBuildingModule module, out string reason)
        {
            definition = null;
            module = null;
            reason = "На клітинці немає власної військової будівлі.";
            if (_construction is not IConstructionSaveSnapshotSource source)
                return false;
            foreach (ConstructionSavedPlacement placement in source.GetSavedPlacements())
            {
                if (placement.Position != position || !string.Equals(placement.OwnerId, ownerId, StringComparison.Ordinal))
                    continue;
                definition = _buildings.GetById(placement.BuildingId);
                if (definition != null && BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out module))
                    return true;
            }
            return false;
        }

        private QueueState GetOrCreateQueue(Vector2Int position, string buildingId, string ownerId)
        {
            if (!_queues.TryGetValue(position, out QueueState queue))
            {
                queue = new QueueState { Position = position, BuildingId = buildingId, OwnerId = ownerId };
                _queues[position] = queue;
            }
            return queue;
        }

        private UnitRecruitmentBuildingModule ResolveModule(string buildingId)
        {
            BuildingDefinition definition = _buildings.GetById(buildingId);
            return definition != null && BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out UnitRecruitmentBuildingModule module) ? module : null;
        }

        private bool TryFindSpawn(Vector2Int origin, int maxRadius, out Vector2Int result)
        {
            for (int radius = 1; radius <= Mathf.Max(1, maxRadius); radius++)
            {
                for (int x = -radius; x <= radius; x++)
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) != radius)
                        continue;
                    Vector2Int candidate = origin + new Vector2Int(x, y);
                    if (_grid.TryGetTileData(candidate, out string tileId) && !string.IsNullOrWhiteSpace(tileId) && !_objects.IsOccupied(candidate))
                    {
                        result = candidate;
                        return true;
                    }
                }
            }
            result = origin;
            return false;
        }

        private static Dictionary<string, float> BuildCosts(UnitRecruitmentRecipeDefinition recipe)
        {
            var result = new Dictionary<string, float>(StringComparer.Ordinal);
            if (recipe.Costs == null)
                return result;
            foreach (BuildingResourceAmount entry in recipe.Costs)
                if (entry != null && !string.IsNullOrWhiteSpace(entry.ResourceId) && entry.Amount > 0)
                    result[entry.ResourceId] = entry.Amount;
            return result;
        }
    }

    public static class RecruitmentBindings
    {
        public static void Install(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<RecruitmentService>().AsSingle().NonLazy();
            container.BindInterfacesTo<SaveModuleRegistrar<RecruitmentService>>().AsSingle().NonLazy();
        }
    }
}
