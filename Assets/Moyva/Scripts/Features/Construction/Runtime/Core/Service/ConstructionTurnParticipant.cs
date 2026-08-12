using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionTurnParticipant : ITurnParticipant
    {
        private readonly IConstructionService _construction;

        public ConstructionTurnParticipant(IConstructionService construction)
            => _construction = construction;

        public int TurnOrder => 10;

        public void OnTurnStarted(TurnContext context)
            => _construction.SetActiveOwner(context.Faction.OwnerId);

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }
    }
}
