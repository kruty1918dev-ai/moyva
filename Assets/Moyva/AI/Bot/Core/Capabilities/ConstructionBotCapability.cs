using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class ConstructionBotCapability : IBotCapabilityProvider
    {
        private readonly IBotTurnGateway _turns;
        private readonly IBuildingRegistry _catalog;
        private readonly IConstructionPlacementQuery _query;
        private readonly IAuthoritativeConstructionPlacementExecutor _commands;
        private readonly IConstructionSaveSnapshotSource _placements;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _owners;
        private readonly IFogOwnerStateReader _fog;
        public BotCapabilityId Id => BotCapabilityId.Construction;
        public ConstructionBotCapability(IBotTurnGateway turns, IBuildingRegistry catalog, IConstructionPlacementQuery query,
            IAuthoritativeConstructionPlacementExecutor commands, IConstructionSaveSnapshotSource placements,
            IUnitService units, IUnitOwnershipQuery owners, IFogOwnerStateReader fog)
        { _turns = turns; _catalog = catalog; _query = query; _commands = commands; _placements = placements;
            _units = units; _owners = owners; _fog = fog; }
        public string UnavailableReason(string player) => _catalog == null || _query == null || _commands == null
            || _placements == null || _units == null || _owners == null || _fog == null
            ? "Construction requires catalog, placement query/command, owned anchors and player visibility." : null;
        private IEnumerable<Vector2Int> Cells(string player)
        {
            var anchors = _placements.GetSavedPlacements().Where(p => p.OwnerId == player).Select(p => p.Position).ToList();
            foreach (string unit in _units.GetAllUnitIds().OrderBy(x => x, StringComparer.Ordinal))
                if (_owners.GetUnitOwnerId(unit) == player && _units.TryGetUnitPosition(unit, out var position)) anchors.Add(position);
            anchors = anchors.Distinct().OrderBy(p => p.y).ThenBy(p => p.x).Take(16).ToList();
            var seen = new HashSet<Vector2Int>();
            int count = 0;
            for (int radius = 0; radius <= 4; radius++)
                foreach (var anchor in anchors)
                    for (int y = -radius; y <= radius; y++)
                        for (int x = -radius; x <= radius; x++)
                        {
                            if (Math.Max(Math.Abs(x), Math.Abs(y)) != radius) continue;
                            var cell = anchor + new Vector2Int(x, y);
                            if (!seen.Add(cell) || !_fog.IsVisible(player, cell)) continue;
                            yield return cell;
                            if (++count >= 64) yield break;
                        }
        }
        private ConstructionPlacementQueryResult Evaluate(string player, string building, Vector2Int cell)
            => _query.EvaluatePlacement(new ConstructionPlacementQueryRequest(building, cell, includeResources: true,
                ownerId: player, includePendingPlacements: false, attemptSource: ConstructionPlacementAttemptSource.NetworkRequest));
        public IEnumerable<BotCandidateAction> Enumerate(string player)
        {
            if (UnavailableReason(player) != null || !_turns.Read(player).CanAct) yield break;
            var cells = Cells(player).ToArray();
            if (cells.Length == 0) yield break;
            // Global eligibility uses the same query as commit; no separate bot build database.
            var buildings = _catalog.GetAll().Where(b => b != null && !string.IsNullOrEmpty(b.Id))
                .OrderBy(b => b.Id, StringComparer.Ordinal).Where(b => Evaluate(player, b.Id, cells[0]).CanSelect).ToArray();
            int emitted = 0, evaluations = 0;
            foreach (var cell in cells)
                foreach (var building in buildings)
                {
                    if (++evaluations > 512) yield break;
                    if (!Evaluate(player, building.Id, cell).CanCommit) continue;
                    var features = new float[BotDecisionContract.CandidateFeatureCount];
                    features[17] = 1;
                    double totalCost = 0;
                    if (building.ConstructionCost != null)
                        foreach (var cost in building.ConstructionCost)
                            if (cost != null && !float.IsNaN(cost.Amount) && !float.IsInfinity(cost.Amount)) totalCost += Math.Max(0, cost.Amount);
                    features[14] = (float)(totalCost / (totalCost + 100));
                    features[20] = Math.Max(0, building.BuildTurns) / (float)(Math.Max(0, building.BuildTurns) + 10);
                    features[23] = 1;
                    yield return new BotCandidateAction("build:" + building.Id + ":" + cell.x + ":" + cell.y,
                        Id, BotIntentType.Build, player, building.Id, cell.x, cell.y, features);
                    if (++emitted >= 32) yield break;
                }
        }
        public bool Validate(string player, BotCandidateAction candidate, out string reason)
        {
            reason = "Construction action is no longer legal.";
            if (UnavailableReason(player) != null || candidate == null || candidate.Capability != Id
                || candidate.Intent != BotIntentType.Build || candidate.ActorKey != player || !_turns.Read(player).CanAct) return false;
            var cell = new Vector2Int(candidate.X, candidate.Y);
            if (!_fog.IsVisible(player, cell)) return false;
            var result = Evaluate(player, candidate.TargetKey, cell);
            reason = result.Reason;
            return result.CanCommit;
        }
        public Task<BotExecutionResult> Execute(string player, BotCandidateAction candidate, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!Validate(player, candidate, out var reason))
                return Task.FromResult(new BotExecutionResult(BotExecutionStatus.Rejected, candidate, reason));
            bool success = _commands.TryPlaceAuthoritatively(candidate.TargetKey, new Vector2Int(candidate.X, candidate.Y),
                player, ConstructionPlacementCommitIntent.None);
            return Task.FromResult(new BotExecutionResult(success ? BotExecutionStatus.Completed : BotExecutionStatus.Rejected,
                candidate, success ? null : "Authoritative construction rejected placement."));
        }
    }
}
