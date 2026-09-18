using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class RecruitmentBotCapability : IBotCapabilityProvider
    {
        private readonly IBotTurnGateway _turns;
        private readonly IUnitRecruitmentQuery _query;
        private readonly IUnitRecruitmentService _commands;
        private readonly IFogOwnerStateReader _fog;
        public BotCapabilityId Id => BotCapabilityId.Recruitment;
        public RecruitmentBotCapability(IBotTurnGateway turns, IUnitRecruitmentQuery query,
            IUnitRecruitmentService commands, IFogOwnerStateReader fog)
        { _turns = turns; _query = query; _commands = commands; _fog = fog; }
        public string UnavailableReason(string player) => _query == null || _commands == null || _fog == null
            ? "Recruitment requires eligibility, queue/deployment commands and player visibility." : null;
        public IEnumerable<BotCandidateAction> Enumerate(string player)
        {
            if (UnavailableReason(player) != null || !_turns.Read(player).CanAct) yield break;
            int count = 0;
            foreach (var ready in _commands.GetReadyItems(player).OrderBy(x => x.QueueId))
                foreach (var tile in _commands.GetDeploymentTiles(player, ready.RecruitingBuildingPosition, ready.QueueId)
                    .Where(x => x.IsValid && _fog.IsVisible(player, x.Position)).OrderBy(x => x.Position.y).ThenBy(x => x.Position.x))
                {
                    if (count++ >= 32) yield break;
                    string key = "queue:" + ready.QueueId.ToString(CultureInfo.InvariantCulture);
                    var features = new float[BotDecisionContract.CandidateFeatureCount];
                    features[17] = 1; features[19] = 1;
                    features[25] = Stable01(ready.UnitTypeId);
                    features[26] = 0.65f;
                    features[27] = tile.Position.x / 128f;
                    features[28] = tile.Position.y / 128f;
                    yield return new BotCandidateAction(key + ":deploy:" + tile.Position.x + ":" + tile.Position.y,
                        Id, BotIntentType.Recruit, key, ready.UnitTypeId, tile.Position.x, tile.Position.y, features);
                }
            foreach (var option in _query.GetOptions(player))
            {
                if (!_fog.IsVisible(player, option.Source)) continue;
                if (count++ >= 32) yield break;
                var features = new float[BotDecisionContract.CandidateFeatureCount];
                features[17] = 1;
                features[14] = Normalize(option.TotalCost, 100);
                features[20] = Normalize(option.TrainingTurns, 10);
                features[21] = Normalize(option.QueueSlots, 3);
                features[22] = Normalize(option.AvailablePopulation, 20);
                features[23] = 1; // authoritative affordability and eligibility passed
                features[25] = Stable01(option.UnitTypeId);
                features[26] = 0.6f;
                features[27] = option.Source.x / 128f;
                features[28] = option.Source.y / 128f;
                features[29] = features[20];
                yield return new BotCandidateAction("recruit:" + option.Source.x + ":" + option.Source.y + ":" + option.UnitTypeId,
                    Id, BotIntentType.Recruit, "enqueue", option.UnitTypeId, option.Source.x, option.Source.y, features);
            }
        }
        private static float Normalize(float value, float scale)
            => float.IsNaN(value) || float.IsInfinity(value) || value < 0 ? 0 : value / (value + scale);
        private static float Stable01(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                if (!string.IsNullOrEmpty(value))
                    foreach (char c in value) hash = (hash ^ c) * 16777619;
                return (hash & 0xffff) / 65535f;
            }
        }
        private bool FindReady(string player, string key, out UnitRecruitmentQueueItemSnapshot ready)
        {
            ready = default;
            if (key == null || !key.StartsWith("queue:", StringComparison.Ordinal)
                || !long.TryParse(key.Substring(6), NumberStyles.None, CultureInfo.InvariantCulture, out long id)) return false;
            foreach (var item in _commands.GetReadyItems(player))
                if (item.QueueId == id) { ready = item; return true; }
            return false;
        }
        public bool Validate(string player, BotCandidateAction candidate, out string reason)
        {
            reason = "Recruitment action is no longer legal.";
            if (UnavailableReason(player) != null || candidate == null || candidate.Capability != Id
                || candidate.Intent != BotIntentType.Recruit || !_turns.Read(player).CanAct) return false;
            var position = new Vector2Int(candidate.X, candidate.Y);
            if (!_fog.IsVisible(player, position)) return false;
            if (candidate.ActorKey == "enqueue") return _query.CanEnqueue(player, position, candidate.TargetKey, out reason);
            return FindReady(player, candidate.ActorKey, out var ready) && ready.UnitTypeId == candidate.TargetKey
                && _commands.GetDeploymentTiles(player, ready.RecruitingBuildingPosition, ready.QueueId)
                    .Any(tile => tile.IsValid && tile.Position == position);
        }
        public Task<BotExecutionResult> Execute(string player, BotCandidateAction candidate, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!Validate(player, candidate, out string reason))
                return Task.FromResult(new BotExecutionResult(BotExecutionStatus.Rejected, candidate, reason));
            var position = new Vector2Int(candidate.X, candidate.Y);
            bool success;
            if (candidate.ActorKey == "enqueue") success = _commands.TryEnqueue(player, position, candidate.TargetKey, out reason);
            else if (FindReady(player, candidate.ActorKey, out var ready))
                success = _commands.TryDeployReady(player, ready.RecruitingBuildingPosition, ready.QueueId, position, out _, out reason);
            else success = false;
            return Task.FromResult(new BotExecutionResult(success ? BotExecutionStatus.Completed : BotExecutionStatus.Rejected, candidate, reason));
        }
    }
}
