using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotAISaveModule : ISaveModule, ISaveModuleExecutionOrder
    {
        private const int FormatVersion = -1;
        private const int SaveOrder = 550;

        private readonly IBotMemoryStore _memory;
        private readonly IBotStrategicStateStore _strategicState;
        private readonly BotPlanningProfile _profile;

        [Inject]
        public BotAISaveModule(
            IBotMemoryStore memory,
            [InjectOptional] IBotStrategicStateStore strategicState = null,
            [InjectOptional] BotPlanningProfile profile = null)
        {
            _memory = memory;
            _strategicState = strategicState;
            _profile = profile ?? BotPlanningProfile.Normal();
        }

        public int SaveLoadOrder => SaveOrder;

        public void OnSave(ISaveContext context)
        {
            context.Writer.Write(FormatVersion);
            WriteProfile(context);
            WriteMemory(context, _memory?.CaptureMemory());
            WriteStrategicState(context, _strategicState?.CaptureStrategicState());
        }

        public void OnLoad(ISaveContext context)
        {
            int version = context.Reader.ReadInt32();
            if (version != FormatVersion)
            {
                Debug.LogWarning($"[BotAISave] Unsupported BotAI save block version {version}.");
                return;
            }

            ReadProfile(context);
            _memory?.RestoreMemory(ReadMemory(context));
            _strategicState?.RestoreStrategicState(ReadStrategicState(context));
        }

        private void WriteProfile(ISaveContext context)
        {
            context.Writer.Write(_profile.ProfileId ?? string.Empty);
            context.Writer.Write((int)_profile.Difficulty);
            context.Writer.Write(_profile.MaxDecisionIterations);
            context.Writer.Write(_profile.MaxSuccessfulMutations);
            context.Writer.Write(_profile.MaxFailedMutations);
            context.Writer.Write(_profile.DeterministicNoiseMagnitude);
            context.Writer.Write(_profile.MinUtilityToAct);
        }

        private static void ReadProfile(ISaveContext context)
        {
            _ = context.Reader.ReadString();
            _ = context.Reader.ReadInt32();
            _ = context.Reader.ReadInt32();
            _ = context.Reader.ReadInt32();
            _ = context.Reader.ReadInt32();
            _ = context.Reader.ReadInt32();
            _ = context.Reader.ReadInt32();
        }

        private static void WriteMemory(
            ISaveContext context,
            IReadOnlyList<BotMemoryOwnerSnapshot> snapshots)
        {
            int ownerCount = snapshots?.Count ?? 0;
            context.Writer.Write(ownerCount);
            if (snapshots == null)
                return;

            for (int ownerIndex = 0; ownerIndex < snapshots.Count; ownerIndex++)
            {
                BotMemoryOwnerSnapshot owner = snapshots[ownerIndex];
                context.Writer.Write(owner.OwnerId ?? string.Empty);
                IReadOnlyList<BotKnownEntityMemory> records = owner.Records;
                int recordCount = records?.Count ?? 0;
                context.Writer.Write(recordCount);

                if (records == null)
                    continue;

                for (int recordIndex = 0; recordIndex < records.Count; recordIndex++)
                    WriteMemoryRecord(context, records[recordIndex]);
            }
        }

        private static IReadOnlyList<BotMemoryOwnerSnapshot> ReadMemory(ISaveContext context)
        {
            int ownerCount = Math.Max(0, context.Reader.ReadInt32());
            var owners = new List<BotMemoryOwnerSnapshot>(ownerCount);
            for (int ownerIndex = 0; ownerIndex < ownerCount; ownerIndex++)
            {
                string ownerId = context.Reader.ReadString();
                int recordCount = Math.Max(0, context.Reader.ReadInt32());
                var records = new List<BotKnownEntityMemory>(recordCount);
                for (int recordIndex = 0; recordIndex < recordCount; recordIndex++)
                    records.Add(ReadMemoryRecord(context));

                owners.Add(new BotMemoryOwnerSnapshot(ownerId, records));
            }

            return owners;
        }

        private static void WriteMemoryRecord(ISaveContext context, BotKnownEntityMemory memory)
        {
            context.Writer.Write(memory.EntityId ?? string.Empty);
            context.Writer.Write((int)memory.Kind);
            context.Writer.Write(memory.OwnerId ?? string.Empty);
            context.Writer.Write(memory.TypeId ?? string.Empty);
            context.Writer.Write(memory.LastKnownPosition.x);
            context.Writer.Write(memory.LastKnownPosition.y);
            context.Writer.Write(memory.LastSeenGlobalTurn);
            context.Writer.Write(memory.LastKnownHp);
            context.Writer.Write(memory.WasConfirmedDestroyed);
        }

        private static BotKnownEntityMemory ReadMemoryRecord(ISaveContext context)
        {
            string entityId = context.Reader.ReadString();
            var kind = (BotKnownEntityKind)context.Reader.ReadInt32();
            string ownerId = context.Reader.ReadString();
            string typeId = context.Reader.ReadString();
            int x = context.Reader.ReadInt32();
            int y = context.Reader.ReadInt32();
            long lastSeen = context.Reader.ReadInt64();
            int hp = context.Reader.ReadInt32();
            bool destroyed = context.Reader.ReadBoolean();
            return new BotKnownEntityMemory(
                entityId,
                kind,
                ownerId,
                typeId,
                new Vector2Int(x, y),
                lastSeen,
                hp,
                destroyed);
        }

        private static void WriteStrategicState(
            ISaveContext context,
            IReadOnlyList<BotStrategicStateSnapshot> states)
        {
            int count = states?.Count ?? 0;
            context.Writer.Write(count);
            if (states == null)
                return;

            for (int index = 0; index < states.Count; index++)
            {
                BotStrategicStateSnapshot state = states[index];
                context.Writer.Write(state.OwnerId ?? string.Empty);
                context.Writer.Write(state.GlobalTurn);
                context.Writer.Write((int)state.Posture);
                context.Writer.Write(state.PostureScore);
                context.Writer.Write(state.Reason ?? string.Empty);
            }
        }

        private static IReadOnlyList<BotStrategicStateSnapshot> ReadStrategicState(ISaveContext context)
        {
            int count = Math.Max(0, context.Reader.ReadInt32());
            var result = new List<BotStrategicStateSnapshot>(count);
            for (int index = 0; index < count; index++)
            {
                string ownerId = context.Reader.ReadString();
                long globalTurn = context.Reader.ReadInt64();
                var posture = (BotStrategicPosture)context.Reader.ReadInt32();
                int postureScore = context.Reader.ReadInt32();
                string reason = context.Reader.ReadString();
                result.Add(new BotStrategicStateSnapshot(ownerId, globalTurn, posture, postureScore, reason));
            }

            return result;
        }
    }
}
