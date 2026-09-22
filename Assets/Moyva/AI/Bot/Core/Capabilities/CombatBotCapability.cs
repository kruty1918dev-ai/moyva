using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.EntityHealth;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class CombatBotCapability : IBotCapabilityProvider
    {
        private readonly IBotTurnGateway _turns;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _owners;
        private readonly IUnitCombatQuery _query;
        private readonly ICombatCommandService _commands;
        private readonly IFogOwnerStateReader _fog;
        private readonly IConstructionBuildingCombatTargetQuery _buildings;
        private readonly IHealthRegistry _health;
        private readonly IUnitGameplayProfileService _profiles;
        private readonly IGeneratedTerrainLevelQuery _terrain;
        public bool SupportsBuildingTargets => _buildings != null && _health != null;
        public BotCapabilityId Id => BotCapabilityId.Combat;
        public CombatBotCapability(IBotTurnGateway turns, IUnitService units, IUnitOwnershipQuery owners,
            IUnitCombatQuery query, ICombatCommandService commands, IFogOwnerStateReader fog,
            IConstructionBuildingCombatTargetQuery buildings = null, IHealthRegistry health = null,
            IUnitGameplayProfileService profiles = null, IGeneratedTerrainLevelQuery terrain = null)
        { _turns = turns; _units = units; _owners = owners; _query = query; _commands = commands; _fog = fog; _buildings = buildings; _health = health; _profiles = profiles; _terrain = terrain; }
        public string UnavailableReason(string player) => _units == null || _owners == null || _query == null || _commands == null || _fog == null
            ? "Combat requires authoritative combat query/command, units, ownership and owner visibility." : null;
        public IEnumerable<BotCandidateAction> Enumerate(string player)
        {
            if (UnavailableReason(player) != null || !_turns.Read(player).CanAct) yield break;
            foreach (string actor in _units.GetAllUnitIds().OrderBy(x => x, StringComparer.Ordinal))
            {
                if (_owners.GetUnitOwnerId(actor) != player) continue;
                foreach (string target in Targets(actor).Distinct().OrderBy(x => x, StringComparer.Ordinal))
                {
                    if (!TryGetPosition(target, out var position) || !_fog.IsVisible(player, position)) continue;
                    var features = new float[BotDecisionContract.CandidateFeatureCount];
                    if (_query.TryGetHealth(actor, out var own)) features[15] = own.CurrentHp / (float)own.MaxHp;
                    if (_query.TryGetHealth(target, out var enemy)) features[16] = enemy.CurrentHp / (float)enemy.MaxHp;
                    if (_health != null && _health.TryGet(target, out var health)) features[16] = health.CurrentHp / (float)Math.Max(1, health.MaxHp);
                    features[17] = 1;
                    int actorLevel = _units.TryGetUnitPosition(actor, out var actorPosition)
                        ? BotUnitTacticalFeatureEncoder.ResolveTerrainLevel(_terrain, actorPosition)
                        : 0;
                    int targetLevel = BotUnitTacticalFeatureEncoder.ResolveTerrainLevel(_terrain, position);
                    BotUnitTacticalFeatureEncoder.WriteActorFeatures(
                        features,
                        BotUnitTacticalFeatureEncoder.ResolveProfile(_units, _profiles, actor),
                        actorLevel,
                        targetLevel,
                        BotIntentType.Attack);
                    var candidate = new BotCandidateAction(actor + ":attack:" + target, Id, BotIntentType.Attack,
                        actor, target, position.x, position.y, features: features);
                    if (Validate(player, candidate, out _)) yield return candidate;
                }
            }
        }
        private IEnumerable<string> Targets(string actor)
        {
            foreach (var target in _query.GetAttackableTargets(actor)) yield return target;
            if (!SupportsBuildingTargets) yield break;
            foreach (var target in _health.GetAll())
                if (_buildings.TryGetCombatTarget(target.EntityId, out _)) yield return target.EntityId;
        }
        private bool TryGetPosition(string target, out Vector2Int position)
        {
            if (_units.TryGetUnitPosition(target, out position)) return true;
            if (_buildings != null && _buildings.TryGetCombatTarget(target, out var building))
            { position = building.Position; return true; }
            return false;
        }
        public bool Validate(string player, BotCandidateAction candidate, out string reason)
        {
            reason = "Combat candidate is no longer legal or visible.";
            return UnavailableReason(player) == null && candidate.Capability == Id && candidate.Intent == BotIntentType.Attack
                && _turns.Read(player).CanAct && _owners.GetUnitOwnerId(candidate.ActorKey) == player
                && TryGetPosition(candidate.TargetKey, out var position) && _fog.IsVisible(player, position)
                && _commands.TryPreview(candidate.ActorKey, candidate.TargetKey, out _, out reason);
        }
        public async Task<BotExecutionResult> Execute(string player, BotCandidateAction candidate, CancellationToken token)
        {
            if (!Validate(player, candidate, out var reason)) return new BotExecutionResult(BotExecutionStatus.Rejected, candidate, reason);
            var result = await _commands.ExecuteAsync(player, candidate.ActorKey, candidate.TargetKey, token);
            return new BotExecutionResult(result.Succeeded ? BotExecutionStatus.Completed : BotExecutionStatus.Rejected, candidate, result.Reason);
        }
    }
}
