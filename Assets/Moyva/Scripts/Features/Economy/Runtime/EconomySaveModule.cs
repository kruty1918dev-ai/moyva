using System;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    [SaveModuleId("Kruty1918.Moyva.Economy.Runtime.EconomySaveModule")]
    internal sealed class EconomySaveModule : ISaveModule
    {
        private const int SchemaVersion = 4;
        private const string ModuleLogTag =
            "[MoyvaConstructionModules]";
        private const string PerfLogTag =
            "[MoyvaConstructionPerf]";
        private const double SlowSaveThresholdMs = 2d;
        private readonly EconomyManager _economyManager;

        public EconomySaveModule(EconomyManager economyManager)
        {
            _economyManager = economyManager;
        }

        public void OnSave(ISaveContext context)
        {
            long startedAt = Stopwatch.GetTimestamp();

            // Schema v3 stores owner-only resources here. Settlement and
            // warehouse resources are persisted separately in the exact runtime
            // snapshot below; using owner totals here would duplicate resources
            // after load.
            var ownerPools =
                _economyManager
                    ?.GetOwnerResourcePoolsSnapshot()
                ?? new Dictionary<
                    string,
                    Dictionary<string, float>>(
                        StringComparer.Ordinal);

            context.Writer.Write(SchemaVersion);
            context.Writer.Write(ownerPools.Count);

            foreach (var ownerPair in ownerPools)
            {
                context.Writer.Write(
                    ownerPair.Key ?? string.Empty);

                var resources = ownerPair.Value;
                int resourceCount =
                    resources?.Count ?? 0;
                context.Writer.Write(resourceCount);

                if (resources == null)
                    continue;

                foreach (var resourcePair in resources)
                {
                    context.Writer.Write(
                        resourcePair.Key
                        ?? string.Empty);
                    context.Writer.Write(
                        resourcePair.Value);
                }
            }

            EconomyRuntimeSaveSnapshot runtime =
                _economyManager
                    ?.CaptureRuntimeSaveSnapshot()
                ?? new EconomyRuntimeSaveSnapshot();

            WriteRuntimeSnapshot(
                context,
                runtime);

            double elapsedMs =
                (Stopwatch.GetTimestamp() - startedAt)
                * 1000d
                / Stopwatch.Frequency;

            if (Debug.isDebugBuild
                && elapsedMs >= SlowSaveThresholdMs)
            {
            }
        }

        public void OnLoad(ISaveContext context)
        {
            long startedAt = Stopwatch.GetTimestamp();

            int version =
                context.Reader.ReadInt32();
            if (version != 1
                && version != 2
                && version != 3
                && version != SchemaVersion)
            {
                return;
            }

            int ownerCount =
                Math.Max(
                    0,
                    context.Reader.ReadInt32());
            var restored =
                new Dictionary<
                    string,
                    Dictionary<string, float>>(
                        StringComparer.Ordinal);

            for (int ownerIndex = 0;
                 ownerIndex < ownerCount;
                 ownerIndex++)
            {
                string ownerId =
                    context.Reader.ReadString();
                int resourceCount =
                    Math.Max(
                        0,
                        context.Reader.ReadInt32());

                var pool =
                    new Dictionary<string, float>(
                        StringComparer.Ordinal);

                for (int resourceIndex = 0;
                     resourceIndex < resourceCount;
                     resourceIndex++)
                {
                    string resourceId =
                        context.Reader.ReadString();
                    float amount =
                        context.Reader.ReadSingle();

                    if (string.IsNullOrWhiteSpace(
                            resourceId)
                        || amount <= 0f)
                    {
                        continue;
                    }

                    pool[resourceId.Trim()] = amount;
                }

                if (!string.IsNullOrWhiteSpace(ownerId)
                    && pool.Count > 0)
                {
                    restored[ownerId.Trim()] = pool;
                }
            }

            _economyManager
                ?.RestoreOwnerResourcePools(restored);

            EconomyRuntimeSaveSnapshot runtime = null;
            if (version >= 2)
            {
                runtime =
                    ReadRuntimeSnapshot(context, version);
                _economyManager
                    ?.RestoreRuntimeSaveSnapshot(runtime);
            }

            double elapsedMs =
                (Stopwatch.GetTimestamp() - startedAt)
                * 1000d
                / Stopwatch.Frequency;

            if (Debug.isDebugBuild
                && elapsedMs >= SlowSaveThresholdMs)
            {
            }
        }

        private static void WriteRuntimeSnapshot(
            ISaveContext context,
            EconomyRuntimeSaveSnapshot snapshot)
        {
            context.Writer.Write(
                snapshot.Settlements.Count);

            for (int settlementIndex = 0;
                 settlementIndex < snapshot.Settlements.Count;
                 settlementIndex++)
            {
                EconomySettlementRuntimeSnapshot settlement =
                    snapshot.Settlements[settlementIndex];

                context.Writer.Write(
                    settlement.SettlementId
                    ?? string.Empty);
                context.Writer.Write(settlement.CurrentTurn);
                context.Writer.Write(
                    settlement.OwnerId
                    ?? string.Empty);
                context.Writer.Write(
                    settlement.SettlementName
                    ?? string.Empty);
                context.Writer.Write(settlement.IsActive);
                context.Writer.Write(settlement.Residents != null);
                if (settlement.Residents != null)
                {
                    context.Writer.Write(settlement.Residents.Count);
                    foreach (var resident in settlement.Residents)
                    {
                        context.Writer.Write(resident.Age);
                        context.Writer.Write(resident.Hp);
                        context.Writer.Write(resident.Comfort);
                        context.Writer.Write(resident.HouseCollapsed);
                        context.Writer.Write(resident.ProfessionId ?? string.Empty);
                        context.Writer.Write(resident.RecruitmentQueueId);
                        context.Writer.Write(resident.MilitaryUnitId ?? string.Empty);
                    }
                }

                WriteFloatMap(
                    context,
                    settlement.ResourcePool);

                context.Writer.Write(
                    settlement.Warehouses.Count);
                foreach (var warehousePair
                         in settlement.Warehouses)
                {
                    context.Writer.Write(
                        warehousePair.Key
                        ?? string.Empty);
                    WriteFloatMap(
                        context,
                        warehousePair.Value);
                }

                context.Writer.Write(
                    settlement.WorkerAssignments.Count);
                foreach (var assignment
                         in settlement.WorkerAssignments)
                {
                    context.Writer.Write(
                        assignment.Key
                        ?? string.Empty);
                    context.Writer.Write(
                        assignment.Value);
                }

                context.Writer.Write(
                    settlement.Buildings.Count);
                for (int buildingIndex = 0;
                     buildingIndex < settlement.Buildings.Count;
                     buildingIndex++)
                {
                    EconomyBuildingRuntimeSnapshot building =
                        settlement.Buildings[buildingIndex];

                    context.Writer.Write(
                        building.InstanceKey
                        ?? string.Empty);
                    context.Writer.Write(
                        building.BuildingId
                        ?? string.Empty);
                    context.Writer.Write(
                        building.GridPosition.x);
                    context.Writer.Write(
                        building.GridPosition.y);
                    context.Writer.Write(
                        building.AssignedWorkers);
                    context.Writer.Write(
                        building.ProductionProgress);

                    context.Writer.Write(
                        building.RecipeProgress.Count);
                    foreach (var progress
                             in building.RecipeProgress)
                    {
                        context.Writer.Write(
                            progress.Key
                            ?? string.Empty);
                        context.Writer.Write(
                            progress.Value);
                    }
                }
            }
        }

        private static EconomyRuntimeSaveSnapshot
            ReadRuntimeSnapshot(ISaveContext context, int version)
        {
            var result =
                new EconomyRuntimeSaveSnapshot();

            int settlementCount =
                Math.Max(
                    0,
                    context.Reader.ReadInt32());

            for (int settlementIndex = 0;
                 settlementIndex < settlementCount;
                 settlementIndex++)
            {
                var settlement =
                    new EconomySettlementRuntimeSnapshot
                    {
                        SettlementId =
                            context.Reader.ReadString(),
                        CurrentTurn =
                            context.Reader.ReadInt32(),
                    };
                if (version >= 3)
                {
                    settlement.OwnerId =
                        context.Reader.ReadString();
                    settlement.SettlementName =
                        context.Reader.ReadString();
                    settlement.IsActive =
                        context.Reader.ReadBoolean();
                }

                if (version >= 4 && context.Reader.ReadBoolean())
                {
                    int count = context.Reader.ReadInt32();
                    if (count < 0 || count > 1000000)
                        throw new System.IO.InvalidDataException("Invalid resident count.");
                    settlement.Residents = new List<EconomyResidentState>(count);
                    for (int index = 0; index < count; index++)
                        settlement.Residents.Add(new EconomyResidentState(
                            context.Reader.ReadInt32(), context.Reader.ReadSingle(), context.Reader.ReadSingle(),
                            context.Reader.ReadBoolean(), context.Reader.ReadString(),
                            context.Reader.ReadInt64(), context.Reader.ReadString()));
                }

                ReadFloatMap(
                    context,
                    settlement.ResourcePool);

                int warehouseCount =
                    Math.Max(
                        0,
                        context.Reader.ReadInt32());
                for (int warehouseIndex = 0;
                     warehouseIndex < warehouseCount;
                     warehouseIndex++)
                {
                    string warehouseKey =
                        context.Reader.ReadString();
                    var pool =
                        new Dictionary<string, float>(
                            StringComparer.Ordinal);
                    ReadFloatMap(context, pool);
                    settlement.Warehouses[
                        warehouseKey] = pool;
                }

                int assignmentCount =
                    Math.Max(
                        0,
                        context.Reader.ReadInt32());
                for (int assignmentIndex = 0;
                     assignmentIndex < assignmentCount;
                     assignmentIndex++)
                {
                    settlement.WorkerAssignments[
                        context.Reader.ReadString()] =
                        context.Reader.ReadInt32();
                }

                int buildingCount =
                    Math.Max(
                        0,
                        context.Reader.ReadInt32());
                for (int buildingIndex = 0;
                     buildingIndex < buildingCount;
                     buildingIndex++)
                {
                    var building =
                        new EconomyBuildingRuntimeSnapshot
                        {
                            InstanceKey =
                                context.Reader.ReadString(),
                            BuildingId =
                                context.Reader.ReadString(),
                            GridPosition =
                                new Vector2Int(
                                    context.Reader.ReadInt32(),
                                    context.Reader.ReadInt32()),
                            AssignedWorkers =
                                context.Reader.ReadInt32(),
                            ProductionProgress =
                                context.Reader.ReadSingle(),
                        };

                    int recipeCount =
                        Math.Max(
                            0,
                            context.Reader.ReadInt32());
                    for (int recipeIndex = 0;
                         recipeIndex < recipeCount;
                         recipeIndex++)
                    {
                        building.RecipeProgress[
                            context.Reader.ReadString()] =
                            context.Reader.ReadSingle();
                    }

                    settlement.Buildings.Add(building);
                }

                result.Settlements.Add(settlement);
            }

            return result;
        }

        private static void WriteFloatMap(
            ISaveContext context,
            IReadOnlyDictionary<string, float> map)
        {
            int count = map?.Count ?? 0;
            context.Writer.Write(count);
            if (map == null)
                return;

            foreach (var pair in map)
            {
                context.Writer.Write(
                    pair.Key ?? string.Empty);
                context.Writer.Write(pair.Value);
            }
        }

        private static void ReadFloatMap(
            ISaveContext context,
            IDictionary<string, float> map)
        {
            int count =
                Math.Max(
                    0,
                    context.Reader.ReadInt32());
            for (int index = 0;
                 index < count;
                 index++)
            {
                string key =
                    context.Reader.ReadString();
                float value =
                    context.Reader.ReadSingle();
                if (!string.IsNullOrWhiteSpace(key))
                    map[key] = value;
            }
        }
    }

    internal sealed class EconomyRuntimeSaveSnapshot
    {
        public readonly List<EconomySettlementRuntimeSnapshot>
            Settlements = new();

        public int BuildingCount
        {
            get
            {
                int count = 0;
                for (int index = 0;
                     index < Settlements.Count;
                     index++)
                {
                    count +=
                        Settlements[index]
                            ?.Buildings.Count ?? 0;
                }
                return count;
            }
        }
    }

    internal sealed class EconomySettlementRuntimeSnapshot
    {
        public List<EconomyResidentState> Residents;
        public string SettlementId;
        public string OwnerId;
        public string SettlementName;
        public int CurrentTurn;
        public bool IsActive = true;

        public readonly Dictionary<string, float>
            ResourcePool =
                new(StringComparer.Ordinal);
        public readonly Dictionary<
            string,
            Dictionary<string, float>>
            Warehouses =
                new(StringComparer.Ordinal);
        public readonly Dictionary<string, int>
            WorkerAssignments =
                new(StringComparer.Ordinal);
        public readonly List<EconomyBuildingRuntimeSnapshot>
            Buildings = new();
    }

    internal sealed class EconomyBuildingRuntimeSnapshot
    {
        public string InstanceKey;
        public string BuildingId;
        public Vector2Int GridPosition;
        public int AssignedWorkers;
        public float ProductionProgress;

        public readonly Dictionary<string, float>
            RecipeProgress =
                new(StringComparer.Ordinal);
    }
}
