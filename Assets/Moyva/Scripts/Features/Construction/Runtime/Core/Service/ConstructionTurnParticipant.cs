using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionTurnParticipant : ITurnParticipant
    {
        private readonly IConstructionSessionCommands _construction;

        public ConstructionTurnParticipant(IConstructionSessionCommands construction)
            => _construction = construction;

        public int TurnOrder => 10;

        public void OnTurnStarted(TurnContext context)
        {
            // Simultaneous rounds raise OnTurnStarted for every faction, but the
            // active construction owner is a local-process concept: remote
            // owners' requests arrive with an explicit owner id, so only adopt
            // the local owner's faction here.
            if (context.Mode == TurnFlowMode.Simultaneous && !context.IsLocalOwner)
                return;

            _construction.SetActiveOwner(context.Faction.OwnerId);
        }

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }
    }
}
