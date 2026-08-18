using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Faction.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Legacy compatibility shim retained so old editor/tests that construct the
    /// historical scheduler still compile. It is intentionally not a Zenject
    /// IInitializable/ITickable and never creates or ticks BotBrain instances.
    ///
    /// Runtime bot execution must originate from the authoritative turn loop.
    /// </summary>
    internal sealed class BotTickScheduler
    {
        public BotTickScheduler(
            IFactionRegistry factionRegistry,
            DiContainer container,
            IBotDifficultySettings settings)
        {
            // P10: dependencies are intentionally accepted for source compatibility
            // but no wall-clock scheduler state is created.
        }

        public bool IsRuntimeEnabled => false;

        public void Initialize()
        {
            // Intentionally inert. Do not discover or instantiate BotBrain here.
        }

        public void Tick()
        {
            // Intentionally inert. Bot actions are turn-scoped, never frame-time-scoped.
        }
    }
}
