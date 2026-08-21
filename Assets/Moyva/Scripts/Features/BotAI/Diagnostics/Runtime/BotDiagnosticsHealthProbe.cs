using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    internal sealed class BotDiagnosticsHealthProbe : IInitializable, ITickable
    {
        private readonly DiContainer _container;
        private readonly ITurnService _turns;
        private readonly IBotDiagnosticsLogger _log;
        private readonly BotDiagnosticsSettings _settings;

        private string _lastSignature = string.Empty;
        private double _nextProbeTime;

        public BotDiagnosticsHealthProbe(
            DiContainer container,
            IBotDiagnosticsLogger log,
            BotDiagnosticsSettings settings,
            [InjectOptional] ITurnService turns = null)
        {
            _container = container;
            _log = log;
            _settings = settings;
            _turns = turns;
        }

        public void Initialize()
        {
            _log.Info(
                BotDiagnosticCategory.Health,
                "HEALTH.SYSTEM_ONLINE",
                "Bot diagnostics subsystem initialized.",
                details:
                    $"prefix={BotDiagnosticsConstants.Prefix}; " +
                    $"turnServiceInjected={_turns != null}; " +
                    $"containerAvailable={_container != null}");

            Probe(force: true);
        }

        public void Tick()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            if (now < _nextProbeTime)
                return;

            _nextProbeTime =
                now + Math.Max(0.25, _settings.HealthProbeSeconds);

            Probe(force: false);
        }

        private void Probe(bool force)
        {
            string[] required =
            {
                TypeName<IBotTurnExecutor>(),
                TypeName<IBotWorldSnapshotBuilder>(),
                TypeName<IBotTurnPlanner>(),
                TypeName<IBotActionExecutor>(),
                TypeName<IBotStrategicPlanner>(),
                TypeName<IBotGoalStore>(),
                TypeName<IBotDecisionTrace>(),
            };

            string[] expectedFromFullIntegration =
            {
                TypeName<IBotReasoningTrace>(),
                TypeName<IBotPerceptionService>(),
            };

            var missingRequired = new List<string>();
            var missingExpected = new List<string>();

            for (int i = 0; i < required.Length; i++)
            {
                if (!HasType(required[i]))
                    missingRequired.Add(required[i]);
            }

            for (int i = 0; i < expectedFromFullIntegration.Length; i++)
            {
                if (!HasType(expectedFromFullIntegration[i]))
                    missingExpected.Add(expectedFromFullIntegration[i]);
            }

            bool hasBotFaction = false;
            int factionCount = _turns?.Factions?.Count ?? 0;
            if (_turns?.Factions != null)
            {
                for (int i = 0; i < _turns.Factions.Count; i++)
                {
                    if (_turns.Factions[i].IsBot)
                    {
                        hasBotFaction = true;
                        break;
                    }
                }
            }

            string signature =
                $"required={string.Join(",", missingRequired)};" +
                $"expected={string.Join(",", missingExpected)};" +
                $"turns={_turns != null};factions={factionCount};hasBot={hasBotFaction}";

            if (!force &&
                string.Equals(signature, _lastSignature, StringComparison.Ordinal))
            {
                return;
            }

            _lastSignature = signature;

            if (_turns == null)
            {
                _log.Critical(
                    BotDiagnosticCategory.Health,
                    "HEALTH.TURN_SERVICE_MISSING",
                    "ITurnService is unavailable to bot diagnostics.",
                    reason:
                        "Without turn authority the bot cannot become the active faction or execute a turn.");
            }

            if (missingRequired.Count > 0)
            {
                _log.Error(
                    BotDiagnosticCategory.Health,
                    "HEALTH.BOT_RUNTIME_BINDINGS_MISSING",
                    "One or more canonical BotAI runtime bindings are missing.",
                    reason:
                        "This usually means BotInstaller is absent, not executed, or its assembly failed to compile.",
                    details:
                        $"missing=[{string.Join(", ", missingRequired)}]");
            }
            else
            {
                _log.Info(
                    BotDiagnosticCategory.Health,
                    "HEALTH.BOT_RUNTIME_READY",
                    "Canonical BotAI runtime bindings are present.",
                    details:
                        "IBotTurnExecutor, snapshot builder, planner, action executor, strategy, goals and decision trace are resolvable.");
            }

            if (missingExpected.Count > 0)
            {
                _log.Warning(
                    BotDiagnosticCategory.Health,
                    "HEALTH.FULL_INTEGRATION_BINDINGS_MISSING",
                    "Some Player-vs-Bot integration services are missing.",
                    reason:
                        "Terrain/perception/reasoning integration from the previous patch may not be installed or may have failed compilation.",
                    details:
                        $"missing=[{string.Join(", ", missingExpected)}]");
            }

            if (!hasBotFaction)
            {
                _log.Error(
                    BotDiagnosticCategory.Faction,
                    "FACTION.NO_BOT_REGISTERED",
                    "Turn authority currently contains no bot faction.",
                    reason:
                        "If the player ends the only registered faction turn, the round advances immediately.",
                    details:
                        $"factionCount={factionCount}; this directly matches the symptom 'round changes immediately after player End Turn'.");
            }
            else
            {
                _log.Info(
                    BotDiagnosticCategory.Faction,
                    "FACTION.BOT_PRESENT",
                    "At least one bot faction is registered in ITurnService.",
                    details:
                        $"factionCount={factionCount}");
            }
        }

        private bool HasType(string typeName)
        {
            Type type = ResolveKnownType(typeName);
            if (type == null || _container == null)
                return false;

            try
            {
                return _container.HasBinding(type);
            }
            catch
            {
                return false;
            }
        }

        private static string TypeName<T>() => typeof(T).AssemblyQualifiedName;

        private static Type ResolveKnownType(string assemblyQualifiedName)
            => string.IsNullOrWhiteSpace(assemblyQualifiedName)
                ? null
                : Type.GetType(assemblyQualifiedName, throwOnError: false);
    }
}
