using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Bot
{
    public interface IBotMovementGateway : IBotCapabilityProvider { }
    public sealed class MovementBotCapability : IBotMovementGateway
    {
        private readonly IBotTurnGateway _turns;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _owners;
        private readonly IUnitMovementQuery _query;
        private readonly IUnitMovementService _commands;
        private readonly IFogOwnerStateReader _fog;
        private readonly IUnitGameplayProfileService _profiles;
        private readonly IGeneratedTerrainLevelQuery _terrain;
        public BotCapabilityId Id => BotCapabilityId.Movement;
        public MovementBotCapability(IBotTurnGateway turns, IUnitService units, IUnitOwnershipQuery owners,
            IUnitMovementQuery query, IUnitMovementService commands, IFogOwnerStateReader fog,
            IUnitGameplayProfileService profiles = null, IGeneratedTerrainLevelQuery terrain = null)
        { _turns = turns; _units = units; _owners = owners; _query = query; _commands = commands; _fog = fog; _profiles = profiles; _terrain = terrain; }
        public string UnavailableReason(string player)
            => _units == null || _owners == null || _query == null || _commands == null || _fog == null
                ? "Movement requires unit, ownership, movement query/command and owner fog APIs." : null;
        public IEnumerable<BotCandidateAction> Enumerate(string player)
        {
            if (UnavailableReason(player) != null || !_turns.Read(player).CanAct) yield break;
            foreach (string id in _units.GetAllUnitIds().OrderBy(x => x, System.StringComparer.Ordinal))
            {
                if (_owners.GetUnitOwnerId(id) != player || !_units.TryGetUnitPosition(id, out var start)) continue;
                foreach (var tile in _query.GetMovementTiles(id))
                    if (tile.IsReachable && tile.Position != start && _fog.IsVisible(player, tile.Position))
                    {
                        float cost = Mathf.Max(0, tile.Cost);
                        var features = new float[BotDecisionContract.CandidateFeatureCount];
                        features[13] = Vector2Int.Distance(start, tile.Position) / (Vector2Int.Distance(start, tile.Position) + 10f);
                        features[14] = cost / (cost + 10f); features[17] = 1; features[18] = 1;
                        features[27] = tile.Position.x / 128f;
                        features[28] = tile.Position.y / 128f;
                        int terrainLevel = BotUnitTacticalFeatureEncoder.ResolveTerrainLevel(_terrain, tile.Position);
                        BotUnitTacticalFeatureEncoder.WriteActorFeatures(
                            features,
                            BotUnitTacticalFeatureEncoder.ResolveProfile(_units, _profiles, id),
                            terrainLevel,
                            terrainLevel,
                            ResolveMovementPurpose(id, terrainLevel));
                        yield return new BotCandidateAction(id + ":" + tile.Position.x + ":" + tile.Position.y,
                            Id, BotIntentType.Move, id, x: tile.Position.x, y: tile.Position.y, features: features);
                    }
            }
        }
        public bool Validate(string player, BotCandidateAction candidate, out string reason)
        {
            reason = "Movement candidate is no longer legal.";
            var position = new Vector2Int(candidate.X, candidate.Y);
            return UnavailableReason(player) == null && _turns.Read(player).CanAct
                && _owners.GetUnitOwnerId(candidate.ActorKey) == player && _fog.IsVisible(player, position)
                && _units.TryGetUnitPosition(candidate.ActorKey, out var current) && current != position
                && _query.GetMovementTiles(candidate.ActorKey).Any(x => x.Position == position && x.IsReachable);
        }
        public async Task<BotExecutionResult> Execute(string player, BotCandidateAction candidate, CancellationToken token)
        {
            if (!Validate(player, candidate, out string reason))
                return new BotExecutionResult(BotExecutionStatus.Rejected, candidate, reason);
            var target = new Vector2Int(candidate.X, candidate.Y);
            await _commands.MoveUnitAsync(candidate.ActorKey, target, token);
            if (token.IsCancellationRequested) return BotExecutionResult.Cancelled(candidate);
            bool arrived = _units.TryGetUnitPosition(candidate.ActorKey, out var actual) && actual == target;
            return new BotExecutionResult(arrived ? BotExecutionStatus.Completed : BotExecutionStatus.Failed, candidate,
                arrived ? null : "Movement completed without reaching the requested tile.");
        }

        private BotIntentType ResolveMovementPurpose(string unitId, int terrainLevel)
        {
            var profile = BotUnitTacticalFeatureEncoder.ResolveProfile(_units, _profiles, unitId);
            int effectiveVision = profile.ResolveVisionRange(terrainLevel, 1, 128);
            int attackPower = profile.CuttingDamage + profile.PenetratingDamage + profile.CrushingDamage;
            return effectiveVision >= attackPower ? BotIntentType.Explore : BotIntentType.Reposition;
        }
    }
}
