using System;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class BotTelemetryView : MonoBehaviour
    {
        private Func<IBotDecisionOrchestrator> _provider;
        public IBotDecisionOrchestrator Orchestrator => _provider?.Invoke();
        // Func<> resolves lazily so a save-load can restore the bot identity
        // before the orchestrator reads the launch difficulty.
        [Inject] public void Configure(Func<IBotDecisionOrchestrator> orchestrator) { _provider = orchestrator; }
        public void ConfigureProvider(Func<IBotDecisionOrchestrator> provider) { _provider = provider; }
    }
}
