using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class CaptureBotCapability : IBotCapabilityProvider
    {
        private readonly IBotTurnGateway _turns;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _owners;
        private readonly IFogOwnerStateReader _fog;
        private readonly IConstructionSaveSnapshotSource _placements;
        private readonly ISettlementCaptureQuery _query;
        private readonly ISettlementCaptureService _commands;
        public BotCapabilityId Id => BotCapabilityId.Capture;

        public CaptureBotCapability(IBotTurnGateway turns, IUnitService units, IUnitOwnershipQuery owners,
            IFogOwnerStateReader fog, IConstructionSaveSnapshotSource placements,
            ISettlementCaptureQuery query, ISettlementCaptureService commands)
        { _turns = turns; _units = units; _owners = owners; _fog = fog; _placements = placements; _query = query; _commands = commands; }

        public string UnavailableReason(string player)
            => _turns == null || _units == null || _owners == null || _fog == null || _placements == null
                || _query == null || _commands == null
                ? "Capture requires authoritative eligibility/command, units, ownership, building and visibility APIs." : _query.UnavailableReason;

        public IEnumerable<BotCandidateAction> Enumerate(string player)
        {
            if (UnavailableReason(player) != null || !_turns.Read(player).CanAct) yield break;
            // Filter positions through player visibility before consuming target identity/ownership.
            var targets = _placements.GetSavedPlacements().Where(p => _fog.IsVisible(player, p.Position))
                .OrderBy(p => p.Position.x).ThenBy(p => p.Position.y).ToArray();
            int count = 0;
            foreach (var actor in _units.GetAllUnitIds().OrderBy(x => x, StringComparer.Ordinal))
            {
                if (_owners.GetUnitOwnerId(actor) != player || !_units.TryGetUnitPosition(actor, out var origin)) continue;
                foreach (var target in targets)
                {
                    string targetId = $"{target.BuildingId}@{target.Position.x},{target.Position.y}";
                    if (!_query.TryEvaluateCapture(player, actor, targetId, target.Position, out var evaluated, out _)) continue;
                    var features = new float[BotDecisionContract.CandidateFeatureCount];
                    features[17] = 1; features[18] = 1;
                    yield return new BotCandidateAction(ActionId(player, actor, targetId, evaluated.OwnerId, origin), Id, BotIntentType.Capture,
                        actor, targetId, target.Position.x, target.Position.y, features: features);
                    if (++count >= BotDecisionContract.MaxCandidateSlots) yield break;
                }
            }
        }

        public bool Validate(string player, BotCandidateAction candidate, out string reason)
        {
            reason = "Capture candidate is no longer legal.";
            if (UnavailableReason(player) != null || candidate == null || candidate.Capability != Id
                || candidate.Intent != BotIntentType.Capture || !_turns.Read(player).CanAct) return false;
            if (!_query.TryEvaluateCapture(player, candidate.ActorKey, candidate.TargetKey,
                    new Vector2Int(candidate.X, candidate.Y), out var target, out reason)) return false;
            reason = "Capture actor position, target owner, or turn has changed.";
            if (!_units.TryGetUnitPosition(candidate.ActorKey, out var origin)
                || candidate.Id != ActionId(player, candidate.ActorKey, candidate.TargetKey, target.OwnerId, origin)) return false;
            reason = null;
            return true;
        }

        private string ActionId(string player, string actor, string target, string owner, Vector2Int origin)
            => string.Format(CultureInfo.InvariantCulture, "{0}:capture:{1}:{2}:{3}:{4}:{5}:{6}",
                actor, target, owner, player, origin.x, origin.y, _turns.Read(player).Turn);

        public Task<BotExecutionResult> Execute(string player, BotCandidateAction candidate, CancellationToken token)
        {
            if (token.IsCancellationRequested) return Task.FromResult(BotExecutionResult.Cancelled(candidate));
            if (!Validate(player, candidate, out var reason))
                return Task.FromResult(new BotExecutionResult(BotExecutionStatus.Rejected, candidate, reason));
            var result = _commands.CaptureWithUnit(player, candidate.ActorKey, candidate.TargetKey,
                new Vector2Int(candidate.X, candidate.Y));
            return Task.FromResult(new BotExecutionResult(result.Succeeded ? BotExecutionStatus.Completed : BotExecutionStatus.Rejected,
                candidate, result.Reason));
        }
    }
}
