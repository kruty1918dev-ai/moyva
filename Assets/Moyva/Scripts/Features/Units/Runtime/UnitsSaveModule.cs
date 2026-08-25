using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class UnitsSaveModule : ISaveModule, IInitializable, IDisposable
    {
        private const int SaveMagic = unchecked((int)0x554E4954);
        private const int SaveVersion = 4;
        private const int MaxRecordCount = 100000;

        private readonly struct UnitRecord
        {
            public readonly string UnitId;
            public readonly string TypeId;
            public readonly string OwnerId;
            public readonly Vector2Int Position;
            public readonly bool HasStamina;
            public readonly float Stamina;
            public readonly bool HasHealth;
            public readonly int CurrentHp;

            public UnitRecord(
                string unitId,
                string typeId,
                string ownerId,
                Vector2Int position,
                bool hasStamina,
                float stamina,
                bool hasHealth = false,
                int currentHp = 0)
            {
                UnitId = unitId;
                TypeId = typeId;
                OwnerId = ownerId;
                Position = position;
                HasStamina = hasStamina;
                Stamina = stamina;
                HasHealth = hasHealth;
                CurrentHp = currentHp;
            }
        }

        private readonly IUnitService _unitService;
        private readonly IUnitFactory _unitFactory;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly SignalBus _signalBus;
        private readonly IUnitRecruitmentStateStore _recruitmentState;
        private readonly IHealthRegistry _healthRegistry;
        private readonly List<UnitRecord> _pendingRecords = new();
        private readonly List<UnitRecruitmentQueueItemSnapshot> _pendingRecruitment = new();
        private bool _hasPendingRecruitmentState;
        private bool _worldBuilt;

        public UnitsSaveModule(
            IUnitService unitService,
            IUnitFactory unitFactory,
            IUnitOwnershipQuery ownership,
            SignalBus signalBus,
            [InjectOptional] IUnitRecruitmentStateStore recruitmentState = null,
            [InjectOptional] IHealthRegistry healthRegistry = null)
        {
            _unitService = unitService;
            _unitFactory = unitFactory;
            _ownership = ownership;
            _signalBus = signalBus;
            _recruitmentState = recruitmentState;
            _healthRegistry = healthRegistry;
        }

        public void Initialize() => _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);
        public void Dispose() => _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);

        public void OnSave(ISaveContext context)
        {
            IReadOnlyCollection<string> unitIds = _unitService.GetAllUnitIds();
            context.Writer.Write(SaveMagic);
            context.Writer.Write(SaveVersion);
            context.Writer.Write(unitIds.Count);

            foreach (string unitId in unitIds)
            {
                string typeId = _unitService.GetUnitTypeId(unitId) ?? string.Empty;
                bool hasPos = _unitService.TryGetUnitPosition(unitId, out Vector2Int pos);
                float stamina = _unitService.GetStamina(unitId);
                context.Writer.Write(unitId ?? string.Empty);
                context.Writer.Write(typeId);
                context.Writer.Write(_ownership.GetUnitOwnerId(unitId) ?? string.Empty);
                context.Writer.Write(hasPos ? pos.x : 0);
                context.Writer.Write(hasPos ? pos.y : 0);
                context.Writer.Write(stamina);
                int currentHp = 0;
                IHealth health;
                if (_healthRegistry != null
                    && _healthRegistry.TryGet(unitId, out health)
                    && health != null)
                {
                    currentHp = health.CurrentHp;
                }
                context.Writer.Write(currentHp);
            }

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue =
                _recruitmentState?.CaptureState() ?? Array.Empty<UnitRecruitmentQueueItemSnapshot>();
            context.Writer.Write(queue.Count);
            for (int index = 0; index < queue.Count; index++)
                WriteRecruitmentRecord(context.Writer, queue[index]);
        }

        public void OnLoad(ISaveContext context)
        {
            int markerOrCount = context.Reader.ReadInt32();
            if (markerOrCount == SaveMagic)
            {
                int version = context.Reader.ReadInt32();
                if (version != 2 && version != 3 && version != SaveVersion)
                    throw new InvalidDataException($"Unsupported units save version {version}.");

                int count = ReadBoundedCount(context.Reader, "unit");
                var records = new List<UnitRecord>(count);
                for (int index = 0; index < count; index++)
                {
                    string unitId = context.Reader.ReadString();
                    string typeId = context.Reader.ReadString();
                    string ownerId = context.Reader.ReadString();
                    var position = new Vector2Int(
                        context.Reader.ReadInt32(),
                        context.Reader.ReadInt32());
                    float stamina = ReadFiniteStamina(context.Reader);
                    bool hasHealth = version >= 4;
                    int currentHp = hasHealth
                        ? Math.Max(1, context.Reader.ReadInt32())
                        : 0;
                    records.Add(new UnitRecord(
                        unitId,
                        typeId,
                        ownerId,
                        position,
                        true,
                        stamina,
                        hasHealth,
                        currentHp));
                }

                List<UnitRecruitmentQueueItemSnapshot> queue;
                if (version >= 3)
                {
                    int queueCount = ReadBoundedCount(context.Reader, "recruitment queue");
                    queue = new List<UnitRecruitmentQueueItemSnapshot>(queueCount);
                    for (int index = 0; index < queueCount; index++)
                        queue.Add(ReadRecruitmentRecord(context.Reader));
                }
                else
                {
                    queue = new List<UnitRecruitmentQueueItemSnapshot>();
                }

                QueueOrSpawn(records, queue);
                return;
            }

            int legacyCount = markerOrCount;
            if (legacyCount < 0 || legacyCount > MaxRecordCount)
                throw new InvalidDataException($"Invalid legacy unit count {legacyCount}.");
            long payloadStart = context.Reader.BaseStream.Position;

            if (!TryParseRecordsWithStamina(context.Reader, legacyCount, out List<UnitRecord> legacyRecords))
            {
                context.Reader.BaseStream.Position = payloadStart;
                if (!TryParseLegacyRecords(context.Reader, legacyCount, out legacyRecords))
                {
                    return;
                }
            }
            QueueOrSpawn(legacyRecords, Array.Empty<UnitRecruitmentQueueItemSnapshot>());
        }

        private static void WriteRecruitmentRecord(BinaryWriter writer, UnitRecruitmentQueueItemSnapshot item)
        {
            writer.Write(item.QueueId);
            writer.Write(item.OwnerId ?? string.Empty);
            writer.Write(item.RecruitingBuildingPosition.x);
            writer.Write(item.RecruitingBuildingPosition.y);
            writer.Write(item.RecruitingBuildingId ?? string.Empty);
            writer.Write(item.UnitTypeId ?? string.Empty);
            writer.Write(item.CompletedTurns);
            writer.Write(item.TrainingTurns);
            writer.Write(item.EnqueuedGlobalTurn);
            writer.Write(item.LastProgressGlobalTurn);
        }

        private static UnitRecruitmentQueueItemSnapshot ReadRecruitmentRecord(BinaryReader reader)
        {
            long queueId = reader.ReadInt64();
            string ownerId = reader.ReadString();
            var position = new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
            string buildingId = reader.ReadString();
            string unitTypeId = reader.ReadString();
            int completedTurns = reader.ReadInt32();
            int trainingTurns = reader.ReadInt32();
            long enqueuedGlobalTurn = reader.ReadInt64();
            long lastProgressGlobalTurn = reader.ReadInt64();

            if (queueId < 1 || string.IsNullOrWhiteSpace(ownerId) || string.IsNullOrWhiteSpace(unitTypeId)
                || completedTurns < 0 || trainingTurns < 1 || completedTurns > trainingTurns
                || enqueuedGlobalTurn < 1 || lastProgressGlobalTurn < enqueuedGlobalTurn)
            {
                throw new InvalidDataException($"Invalid recruitment queue record {queueId}.");
            }

            return new UnitRecruitmentQueueItemSnapshot(
                queueId,
                ownerId,
                position,
                buildingId,
                unitTypeId,
                completedTurns,
                trainingTurns,
                enqueuedGlobalTurn,
                lastProgressGlobalTurn,
                completedTurns >= trainingTurns
                    ? UnitRecruitmentQueueStatus.Ready
                    : UnitRecruitmentQueueStatus.Training);
        }

        private static int ReadBoundedCount(BinaryReader reader, string label)
        {
            int count = reader.ReadInt32();
            if (count < 0 || count > MaxRecordCount)
                throw new InvalidDataException($"Invalid {label} count {count}.");
            return count;
        }

        private static float ReadFiniteStamina(BinaryReader reader)
        {
            float stamina = reader.ReadSingle();
            if (float.IsNaN(stamina) || float.IsInfinity(stamina) || stamina < 0f || stamina > 100000f)
                throw new InvalidDataException($"Invalid unit stamina {stamina}.");
            return stamina;
        }

        private void QueueOrSpawn(
            List<UnitRecord> records,
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> recruitment)
        {
            if (!_worldBuilt)
            {
                _pendingRecords.Clear();
                _pendingRecords.AddRange(records);
                _pendingRecruitment.Clear();
                if (recruitment != null)
                    _pendingRecruitment.AddRange(recruitment);
                _hasPendingRecruitmentState = true;
                return;
            }

            // Restore active units before exposing ready recruitment state. This prevents
            // TurnService/WorldBuilt ordering from deploying a saved ready entry before
            // its already-spawned deterministic unit has been recreated from the save.
            SpawnRecords(records);
            _recruitmentState?.RestoreState(
                recruitment ?? Array.Empty<UnitRecruitmentQueueItemSnapshot>());
        }

        private void OnWorldBuilt(WorldBuiltSignal _)
        {
            _worldBuilt = true;
            if (_pendingRecords.Count > 0)
            {
                var records = new List<UnitRecord>(_pendingRecords);
                _pendingRecords.Clear();
                SpawnRecords(records);
            }

            if (_hasPendingRecruitmentState)
            {
                var queue = new List<UnitRecruitmentQueueItemSnapshot>(_pendingRecruitment);
                _pendingRecruitment.Clear();
                _hasPendingRecruitmentState = false;
                _recruitmentState?.RestoreState(queue);
            }
        }

        private void SpawnRecords(List<UnitRecord> records)
        {
            for (int i = 0; i < records.Count; i++)
            {
                UnitRecord record = records[i];
                if (string.IsNullOrEmpty(record.TypeId))
                    continue;

                string newUnitId = string.IsNullOrWhiteSpace(record.UnitId)
                    ? _unitFactory.CreateUnit(record.TypeId, record.Position, record.OwnerId)
                    : _unitFactory.CreateUnitWithId(record.UnitId, record.TypeId, record.Position, record.OwnerId);
                if (!string.IsNullOrEmpty(newUnitId) && record.HasStamina)
                    _unitService.SetStamina(newUnitId, record.Stamina);

                IHealth health;
                if (!string.IsNullOrEmpty(newUnitId)
                    && record.HasHealth
                    && _healthRegistry != null
                    && _healthRegistry.TryGet(newUnitId, out health)
                    && health != null)
                {
                    int damageToRestore =
                        Math.Max(0, health.CurrentHp - record.CurrentHp);
                    if (damageToRestore > 0)
                        health.TakeDamage(damageToRestore);
                }
            }
        }

        private static bool TryParseRecordsWithStamina(BinaryReader reader, int count, out List<UnitRecord> records)
        {
            records = new List<UnitRecord>(count);
            try
            {
                for (int i = 0; i < count; i++)
                {
                    string typeId = reader.ReadString();
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    float stamina = reader.ReadSingle();
                    if (float.IsNaN(stamina) || float.IsInfinity(stamina) || stamina < 0f || stamina > 100000f)
                        return false;
                    records.Add(new UnitRecord(string.Empty, typeId, "player_0", new Vector2Int(x, y), true, stamina));
                }
                return reader.BaseStream.Position == reader.BaseStream.Length;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryParseLegacyRecords(BinaryReader reader, int count, out List<UnitRecord> records)
        {
            records = new List<UnitRecord>(count);
            try
            {
                for (int i = 0; i < count; i++)
                {
                    string typeId = reader.ReadString();
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    records.Add(new UnitRecord(string.Empty, typeId, "player_0", new Vector2Int(x, y), false, 0f));
                }
                return reader.BaseStream.Position == reader.BaseStream.Length;
            }
            catch
            {
                return false;
            }
        }
    }
}
