using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotGoalSaveModule : ISaveModule, ISaveModuleExecutionOrder
    {
        private const int FormatVersion = 1;
        private const int SaveOrder = 560;
        private readonly IBotGoalStore _goals;

        [Inject]
        public BotGoalSaveModule(IBotGoalStore goals) => _goals = goals;
        public int SaveLoadOrder => SaveOrder;

        public void OnSave(ISaveContext context)
        {
            context.Writer.Write(FormatVersion);
            IReadOnlyList<BotGoalSnapshot> goals = _goals?.Capture();
            context.Writer.Write(goals?.Count ?? 0);
            if (goals == null) return;
            for (int i = 0; i < goals.Count; i++)
            {
                BotGoalSnapshot g = goals[i];
                context.Writer.Write(g.OwnerId ?? string.Empty);
                context.Writer.Write((int)g.Kind);
                context.Writer.Write(g.CreatedTurn);
                context.Writer.Write(g.MinimumHoldUntilTurn);
                context.Writer.Write(g.Priority);
                context.Writer.Write(g.TargetId ?? string.Empty);
                context.Writer.Write(g.TargetCell.HasValue);
                if (g.TargetCell.HasValue) { context.Writer.Write(g.TargetCell.Value.x); context.Writer.Write(g.TargetCell.Value.y); }
                context.Writer.Write(g.Reason ?? string.Empty);
            }
        }

        public void OnLoad(ISaveContext context)
        {
            int version = context.Reader.ReadInt32();
            if (version != FormatVersion)
            {
                Debug.LogWarning($"[BotGoalSave] Unsupported format {version}; goal block skipped.");
                return;
            }
            int count = Math.Max(0, context.Reader.ReadInt32());
            var goals = new List<BotGoalSnapshot>(count);
            for (int i = 0; i < count; i++)
            {
                string owner = context.Reader.ReadString();
                var kind = (BotGoalKind)context.Reader.ReadInt32();
                long created = context.Reader.ReadInt64();
                long hold = context.Reader.ReadInt64();
                int priority = context.Reader.ReadInt32();
                string target = context.Reader.ReadString();
                bool hasCell = context.Reader.ReadBoolean();
                Vector2Int? cell = hasCell ? new Vector2Int(context.Reader.ReadInt32(), context.Reader.ReadInt32()) : (Vector2Int?)null;
                string reason = context.Reader.ReadString();
                goals.Add(new BotGoalSnapshot(owner, kind, created, hold, priority, target, cell, reason));
            }
            _goals?.Restore(goals);
        }
    }
}
