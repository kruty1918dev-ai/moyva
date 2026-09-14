using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Signals;
using System;
using System.Linq;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class MoyvaBotTurnAdapter : IBotTurnGateway
    {
        private readonly ITurnService _turns;
        private readonly ITurnAuthorityPolicy _authority;
        public MoyvaBotTurnAdapter(ITurnService turns, ITurnAuthorityPolicy authority = null)
        { _turns = turns; _authority = authority; }
        public BotGameStamp Read(string player) => new BotGameStamp(_turns.ActiveOwnerId, _turns.GlobalTurn,
            (int)_turns.Phase, (_authority?.IsAuthoritative ?? true) && _turns.CanOwnerAct(player, out _));
        public bool CanEndTurn(string player, out string reason)
        {
            reason = "Missing ITurnEndQuery or authority.";
            return (_authority?.IsAuthoritative ?? true) && _turns is ITurnEndQuery query && query.CanEndTurn(player, out reason);
        }
        public bool EndTurn(string player, out string reason)
        {
            if (!CanEndTurn(player, out reason)) return false;
            return _turns.TryEndTurn(player, out reason);
        }
    }

    public sealed class MoyvaBotPerceptionSource : IBotPerceptionSource
    {
        private readonly ITurnService _turns;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _owners;
        private readonly IFogOwnerStateReader _fog;
        private readonly IEconomyInfoMediator _economy;
        public MoyvaBotPerceptionSource(ITurnService turns, IUnitService units, IUnitOwnershipQuery owners, IFogOwnerStateReader fog, IEconomyInfoMediator economy = null)
        { _turns = turns; _units = units; _owners = owners; _fog = fog; _economy = economy; }
        public BotPerceptionSnapshot Capture(string player)
        {
            var result = new BotPerceptionSnapshot();
            result.Global[BotObservationSchema.Active] = _turns.IsOwnerActive(player) ? 1 : 0;
            result.Global[BotObservationSchema.Round] = _turns.Round / (float)(_turns.Round + 100);
            result.Global[BotObservationSchema.Phase] = (int)_turns.Phase / 5f;
            int own = 0, visibleOther = 0;
            result.Global[BotObservationSchema.UnitsAvailable] = _units != null && _owners != null ? 1 : 0;
            result.Global[BotObservationSchema.VisibilityAvailable] = _fog != null ? 1 : 0;
            if (_units != null && _owners != null)
                foreach (string id in _units.GetAllUnitIds())
                {
                    if (_owners.GetUnitOwnerId(id) == player) own++;
                    else if (_fog != null && _units.TryGetUnitPosition(id, out var position) && _fog.IsVisible(player, position))
                        visibleOther++;
                }
            result.Global[BotObservationSchema.OwnUnits] = own / (float)(own + 100);
            result.Global[BotObservationSchema.VisibleOtherUnits] = visibleOther / (float)(visibleOther + 100);
            result.Global[BotObservationSchema.EconomyAvailable] = _economy != null ? 1 : 0;
            if (_economy != null)
            {
                var resources = _economy.GetOwnerResourceTotals(player);
                double total = 0, pool = 0;
                if (resources != null) foreach (var value in resources.Values)
                    if (!float.IsNaN(value) && !float.IsInfinity(value)) total += System.Math.Max(0, value);
                var poolResources = _economy.GetOwnerPoolResourceTotals(player);
                if (poolResources != null) foreach (var value in poolResources.Values)
                    if (!float.IsNaN(value) && !float.IsInfinity(value)) pool += System.Math.Max(0, value);
                result.Global[BotObservationSchema.OwnResourcesTotal] = Normalize(total, 1000);
                result.Global[BotObservationSchema.PoolResourcesTotal] = Normalize(pool, 1000);
                result.Global[BotObservationSchema.ResourceKinds] = (resources?.Count ?? 0) / (float)((resources?.Count ?? 0) + 32);
                result.Global[BotObservationSchema.ResourceFood] = Resource(resources, "food", "grain", "wheat");
                result.Global[BotObservationSchema.ResourceWood] = Resource(resources, "wood", "lumber");
                result.Global[BotObservationSchema.ResourceStone] = Resource(resources, "stone");
                result.Global[BotObservationSchema.ResourceIron] = Resource(resources, "iron", "ore");
                result.Global[BotObservationSchema.ResourceGold] = Resource(resources, "gold", "coin");
                int settlements = 0;
                foreach (var id in resources?.Keys ?? Array.Empty<string>())
                    if (!string.IsNullOrWhiteSpace(id)) settlements++;
                result.Global[BotObservationSchema.OwnSettlements] = Normalize(settlements, 8);
            }
            return result;
        }

        private static float Normalize(double value, double scale)
        {
            value = Math.Max(0, value);
            return (float)(value / (value + Math.Max(1, scale)));
        }

        private static float Resource(System.Collections.Generic.IReadOnlyDictionary<string, float> resources, params string[] names)
        {
            if (resources == null || names == null) return 0;
            foreach (var pair in resources)
            {
                if (string.IsNullOrWhiteSpace(pair.Key)) continue;
                string key = pair.Key.ToLowerInvariant();
                if (!names.Any(key.Contains)) continue;
                return Normalize(pair.Value, 250);
            }
            return 0;
        }
    }
}
