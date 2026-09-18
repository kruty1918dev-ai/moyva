using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal readonly struct BuildingPerPlayerLimitEvaluation
    {
        public BuildingPerPlayerLimitEvaluation(
            int limit,
            int existingCount,
            int pendingCount,
            string reason)
        {
            Limit = Mathf.Max(0, limit);
            ExistingCount = Mathf.Max(0, existingCount);
            PendingCount = Mathf.Max(0, pendingCount);
            Reason = reason;
        }

        public int Limit { get; }
        public int ExistingCount { get; }
        public int PendingCount { get; }
        public int TotalCount => ExistingCount + PendingCount;
        public string Reason { get; }
        public bool IsEnabled => Limit > 0;
        public bool IsValid => !IsEnabled || TotalCount < Limit;

        public static BuildingPerPlayerLimitEvaluation Disabled
            => new BuildingPerPlayerLimitEvaluation(0, 0, 0, null);
    }
}
