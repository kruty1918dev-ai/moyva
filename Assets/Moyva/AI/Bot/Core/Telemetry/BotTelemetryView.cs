using System;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class BotTelemetryView : MonoBehaviour
    {
        private Func<IBotDecisionOrchestrator> _provider;
        public IBotDecisionOrchestrator Orchestrator => _provider?.Invoke();
        [Inject] public void Configure(IBotDecisionOrchestrator orchestrator) { _provider = () => orchestrator; }
        public void ConfigureProvider(Func<IBotDecisionOrchestrator> provider) { _provider = provider; }
    }
}
