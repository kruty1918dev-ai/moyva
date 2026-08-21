using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Units.API;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal sealed class BotAnalyzerRecruitmentCollector
    {
        private readonly IUnitRecruitmentStateStore _stateStore;

        public BotAnalyzerRecruitmentCollector(IUnitRecruitmentStateStore stateStore)
        {
            _stateStore = stateStore;
        }

        public List<BotAnalyzerRecruitmentState> Collect(
            string ownerId,
            BotWorldSnapshot world)
        {
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> source = null;

            if (_stateStore != null)
            {
                try
                {
                    source = _stateStore.CaptureState();
                }
                catch
                {
                    source = null;
                }
            }

            if (source == null)
                source = world?.ReadyRecruitmentItems ?? Array.Empty<UnitRecruitmentQueueItemSnapshot>();

            return MapItems(source, ownerId);
        }

        internal static List<BotAnalyzerRecruitmentState> MapItems(
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> source,
            string ownerId)
        {
            var result = new List<BotAnalyzerRecruitmentState>();
            if (source == null)
                return result;

            for (int i = 0; i < source.Count; i++)
            {
                UnitRecruitmentQueueItemSnapshot item = source[i];
                if (!string.IsNullOrWhiteSpace(ownerId) &&
                    !string.Equals(item.OwnerId, ownerId, StringComparison.Ordinal))
                {
                    continue;
                }

                result.Add(new BotAnalyzerRecruitmentState
                {
                    QueueId = item.QueueId,
                    OwnerId = item.OwnerId,
                    BuildingCell = item.RecruitingBuildingPosition,
                    BuildingId = item.RecruitingBuildingId,
                    UnitTypeId = item.UnitTypeId,
                    CompletedTurns = item.CompletedTurns,
                    TrainingTurns = item.TrainingTurns,
                    RemainingTurns = item.RemainingTurns,
                    EnqueuedGlobalTurn = item.EnqueuedGlobalTurn,
                    LastProgressGlobalTurn = item.LastProgressGlobalTurn,
                    Status = item.Status.ToString(),
                    Ready = item.IsReady,
                });
            }

            result.Sort((a, b) => a.QueueId.CompareTo(b.QueueId));
            return result;
        }
    }
}
