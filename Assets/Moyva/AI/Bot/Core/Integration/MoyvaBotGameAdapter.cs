using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.FogOfWar.API;

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
        public MoyvaBotPerceptionSource(ITurnService turns, IUnitService units, IUnitOwnershipQuery owners, IFogOwnerStateReader fog)
        { _turns = turns; _units = units; _owners = owners; _fog = fog; }
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
            return result;
        }
    }
}
