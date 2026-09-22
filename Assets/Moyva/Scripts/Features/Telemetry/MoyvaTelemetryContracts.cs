using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Fingerprinting;

namespace Kruty1918.Moyva.Telemetry
{
    /// <summary>
    /// Moyva's telemetry data contracts + fingerprint composition.
    /// One ContractId per event type (ContractId == EventType); bump Version when a
    /// payload or its semantic interpretation changes — the fingerprint follows.
    /// </summary>
    public static class MoyvaTelemetryContracts
    {
        public const string ProducerId = "com.kruty1918.moyva";

        public static ContractRegistry CreateRegistry()
        {
            var r = new ContractRegistry();
            StandardContracts.RegisterAll(r);

            // ---- Session / match lifecycle ----
            Register(r, new EventContract("moyva.match.started", 1, "moyva.match.started", ProducerId)
            {
                Description = "Match/session began (new or loaded world).",
                Priority = TelemetryPriority.Critical,
            }
                .Add(F("mode", ContractFieldType.String, false, "normal|multiplayer|training"))
                .Add(F("playerCount", ContractFieldType.Int, false))
                .Add(F("source", ContractFieldType.String, false, "new|save|direct")));

            Register(r, new EventContract("moyva.match.ended", 1, "moyva.match.ended", ProducerId)
            {
                Description = "Match ended with outcome.",
                Priority = TelemetryPriority.Critical,
            }
                .Add(F("winnerId", ContractFieldType.String, false, "winner faction; null = draw/cancel"))
                .Add(F("durationMs", ContractFieldType.Long, false, unit: "ms")));

            Register(r, new EventContract("moyva.match.paused", 1, "moyva.match.paused", ProducerId)
                .Add(F("isPaused", ContractFieldType.Bool, true)));

            Register(r, new EventContract("moyva.match.modeChanged", 1, "moyva.match.modeChanged", ProducerId)
                .Add(F("newMode", ContractFieldType.String, true, "Normal|Construction|Lobby")));

            // ---- Turns ----
            Register(r, new EventContract("moyva.turn.lifecycle", 1, "moyva.turn.lifecycle", ProducerId)
            {
                Description = "Turn/round boundaries: started, ending, roundCompleted.",
                Priority = TelemetryPriority.High,
            }
                .Add(F("phase", ContractFieldType.String, true, "started|ending|roundCompleted"))
                .Add(F("round", ContractFieldType.Int, true))
                .Add(F("globalTurn", ContractFieldType.Long, true))
                .Add(F("factionIndex", ContractFieldType.Int, false))
                .Add(F("ownerId", ContractFieldType.String, false)));

            // ---- Construction ----
            Register(r, new EventContract("moyva.construction.placed", 1, "moyva.construction.placed", ProducerId)
            {
                Description = "Authoritative building placement committed.",
                Priority = TelemetryPriority.High,
            }
                .Add(F("buildingId", ContractFieldType.String, true, "building type id"))
                .Add(F("x", ContractFieldType.Int, true, unit: "tiles"))
                .Add(F("y", ContractFieldType.Int, true, unit: "tiles"))
                .Add(F("ownerId", ContractFieldType.String, true))
                .Add(F("rotation", ContractFieldType.Int, false, "quarter turns"))
                .Add(F("relocated", ContractFieldType.Bool, false)));

            Register(r, new EventContract("moyva.construction.cancelled", 1, "moyva.construction.cancelled", ProducerId)
                .Add(F("reason", ContractFieldType.String, false)));

            Register(r, new EventContract("moyva.construction.rejected", 1, "moyva.construction.rejected", ProducerId)
            {
                Description = "Placement rejected by authority (local or host).",
            }
                .Add(F("buildingId", ContractFieldType.String, true))
                .Add(F("x", ContractFieldType.Int, true))
                .Add(F("y", ContractFieldType.Int, true))
                .Add(F("reason", ContractFieldType.String, true)));

            Register(r, new EventContract("moyva.construction.demolished", 1, "moyva.construction.demolished", ProducerId)
                .Add(F("buildingId", ContractFieldType.String, true))
                .Add(F("x", ContractFieldType.Int, true))
                .Add(F("y", ContractFieldType.Int, true))
                .Add(F("ownerId", ContractFieldType.String, false)));

            Register(r, new EventContract("moyva.construction.transferred", 1, "moyva.construction.transferred", ProducerId)
            {
                Description = "Building ownership moved between factions.",
                Priority = TelemetryPriority.High,
            }
                .Add(F("buildingId", ContractFieldType.String, true))
                .Add(F("x", ContractFieldType.Int, true))
                .Add(F("y", ContractFieldType.Int, true))
                .Add(F("previousOwnerId", ContractFieldType.String, false))
                .Add(F("newOwnerId", ContractFieldType.String, false)));

            // ---- Units ----
            Register(r, new EventContract("moyva.units.created", 1, "moyva.units.created", ProducerId)
            {
                Description = "Unit spawned into the world.",
                Priority = TelemetryPriority.High,
            }
                .Add(F("unitId", ContractFieldType.String, true))
                .Add(F("unitTypeId", ContractFieldType.String, true))
                .Add(F("x", ContractFieldType.Int, true, unit: "tiles"))
                .Add(F("y", ContractFieldType.Int, true, unit: "tiles"))
                .Add(F("ownerId", ContractFieldType.String, false)));

            Register(r, new EventContract("moyva.units.moved", 1, "moyva.units.moved", ProducerId)
            {
                Description = "Canonical unit move committed.",
                Priority = TelemetryPriority.High,
            }
                .Add(F("unitId", ContractFieldType.String, true))
                .Add(F("x", ContractFieldType.Int, true, unit: "tiles"))
                .Add(F("y", ContractFieldType.Int, true, unit: "tiles"))
                .Add(F("cost", ContractFieldType.Float, true, unit: "movement points"))
                .Add(F("ownerId", ContractFieldType.String, false)));

            Register(r, new EventContract("moyva.units.destroyed", 1, "moyva.units.destroyed", ProducerId)
                .Add(F("unitId", ContractFieldType.String, true)));

            Register(r, new EventContract("moyva.units.moveRejected", 1, "moyva.units.moveRejected", ProducerId)
                .Add(F("unitId", ContractFieldType.String, true))
                .Add(F("x", ContractFieldType.Int, true))
                .Add(F("y", ContractFieldType.Int, true))
                .Add(F("reason", ContractFieldType.String, true)));

            // ---- Recruitment ----
            Register(r, new EventContract("moyva.recruitment.queueChanged", 1, "moyva.recruitment.queueChanged", ProducerId)
            {
                Description = "Recruitment queue progress changed.",
            }
                .Add(F("ownerId", ContractFieldType.String, true))
                .Add(F("unitTypeId", ContractFieldType.String, false))
                .Add(F("queueId", ContractFieldType.Long, false))
                .Add(F("buildingX", ContractFieldType.Int, true))
                .Add(F("buildingY", ContractFieldType.Int, true)));

            Register(r, new EventContract("moyva.recruitment.ready", 1, "moyva.recruitment.ready", ProducerId)
            {
                Description = "Queue head transitioned Training → Ready.",
                Priority = TelemetryPriority.High,
            }
                .Add(F("ownerId", ContractFieldType.String, true))
                .Add(F("unitTypeId", ContractFieldType.String, true))
                .Add(F("queueId", ContractFieldType.Long, true))
                .Add(F("buildingX", ContractFieldType.Int, true))
                .Add(F("buildingY", ContractFieldType.Int, true)));

            Register(r, new EventContract("moyva.recruitment.deployed", 1, "moyva.recruitment.deployed", ProducerId)
            {
                Description = "Ready unit deployed into the world.",
                Priority = TelemetryPriority.High,
            }
                .Add(F("ownerId", ContractFieldType.String, true))
                .Add(F("unitTypeId", ContractFieldType.String, true))
                .Add(F("queueId", ContractFieldType.Long, true))
                .Add(F("buildingX", ContractFieldType.Int, true))
                .Add(F("buildingY", ContractFieldType.Int, true))
                .Add(F("unitId", ContractFieldType.String, true)));

            Register(r, new EventContract("moyva.recruitment.rejected", 1, "moyva.recruitment.rejected", ProducerId)
                .Add(F("reason", ContractFieldType.String, true)));

            // ---- Economy ----
            Register(r, new EventContract("moyva.economy.tick", 1, "moyva.economy.tick", ProducerId)
            {
                Description = "Economic tick summary per settlement.",
            }
                .Add(F("settlementId", ContractFieldType.String, true))
                .Add(F("ownerId", ContractFieldType.String, true))
                .Add(F("turn", ContractFieldType.Int, true))
                .Add(F("population", ContractFieldType.Int, true))
                .Add(F("arrivals", ContractFieldType.Int, false))
                .Add(F("deaths", ContractFieldType.Int, false))
                .Add(F("productionCycles", ContractFieldType.Int, false)));

            Register(r, new EventContract("moyva.economy.settlement", 1, "moyva.economy.settlement", ProducerId)
            {
                Description = "Settlement lifecycle: created/deactivated/captured.",
                Priority = TelemetryPriority.High,
            }
                .Add(F("kind", ContractFieldType.String, true, "created|deactivated|captured|buildingTransferred"))
                .Add(F("settlementId", ContractFieldType.String, true))
                .Add(F("ownerId", ContractFieldType.String, false))
                .Add(F("previousOwnerId", ContractFieldType.String, false))
                .Add(F("newOwnerId", ContractFieldType.String, false))
                .Add(F("reason", ContractFieldType.String, false)));

            Register(r, new EventContract("moyva.economy.resource", 1, "moyva.economy.resource", ProducerId)
            {
                Description = "Significant resource delta or deficit on a settlement.",
            }
                .Add(F("kind", ContractFieldType.String, true, "changed|deficit"))
                .Add(F("settlementId", ContractFieldType.String, true))
                .Add(F("resourceId", ContractFieldType.String, true))
                .Add(F("amount", ContractFieldType.Float, false))
                .Add(F("delta", ContractFieldType.Float, false)));

            // ---- Fog / world / save ----
            Register(r, new EventContract("moyva.fog.changed", 1, "moyva.fog.changed", ProducerId)
            {
                Description = "Aggregate fog-of-war reveal change (tile count only).",
            }
                .Add(F("changedTiles", ContractFieldType.Int, true)));

            Register(r, new EventContract("moyva.world.generated", 1, "moyva.world.generated", ProducerId)
            {
                Description = "World build finished: parameters + source.",
                Priority = TelemetryPriority.Critical,
            }
                .Add(F("source", ContractFieldType.String, true))
                .Add(F("width", ContractFieldType.Int, true, unit: "tiles"))
                .Add(F("height", ContractFieldType.Int, true, unit: "tiles"))
                .Add(F("cellSize", ContractFieldType.Float, false))
                .Add(F("startupSessionId", ContractFieldType.String, false))
                .Add(F("snapshotRevision", ContractFieldType.Int, false)));

            Register(r, new EventContract("moyva.save.saved", 1, "moyva.save.saved", ProducerId)
                .Add(F("slot", ContractFieldType.Int, true))
                .Add(F("success", ContractFieldType.Bool, true)));

            Register(r, new EventContract("moyva.save.loadRequested", 1, "moyva.save.loadRequested", ProducerId)
                .Add(F("slot", ContractFieldType.Int, true)));

            // ---- UI ----
            Register(r, new EventContract("moyva.ui.panel", 1, "moyva.ui.panel", ProducerId)
            {
                Description = "Info panel open/close and world selection changes.",
            }
                .Add(F("kind", ContractFieldType.String, true, "worldInfo|buildingInfo|unitInfo|mapObjectInfo|selection"))
                .Add(F("action", ContractFieldType.String, true, "opened|closed|changed"))
                .Add(F("objectId", ContractFieldType.String, false))
                .Add(F("selectionKind", ContractFieldType.String, false)));

            // ---- AI / bot decisions ----
            Register(r, new EventContract("moyva.ai.bot.decision", 1, "moyva.ai.bot.decision", ProducerId)
            {
                Description = "One bot decision: state hash + chosen intent + result. Research consent.",
                Priority = TelemetryPriority.High,
                Privacy = PrivacyClass.Research,
            }
                .Add(F("turn", ContractFieldType.Long, true))
                .Add(F("sequence", ContractFieldType.Long, true))
                .Add(F("intent", ContractFieldType.String, true))
                .Add(F("capability", ContractFieldType.String, true))
                .Add(F("result", ContractFieldType.String, true))
                .Add(F("candidates", ContractFieldType.Int, true))
                .Add(F("realCandidates", ContractFieldType.Int, true))
                .Add(F("latencyMs", ContractFieldType.Float, true, unit: "ms"))
                .Add(F("policyMode", ContractFieldType.String, true))
                .Add(F("contractHash", ContractFieldType.String, true))
                .Add(F("observationHash", ContractFieldType.String, true))
                .Add(F("failure", ContractFieldType.String, false))
                .Add(F("reason", ContractFieldType.String, false)));

            Register(r, new EventContract("moyva.ai.bot.episode", 1, "moyva.ai.bot.episode", ProducerId)
            {
                Description = "Training episode metrics rollup.",
                Privacy = PrivacyClass.Research,
            }
                .Add(F("episode", ContractFieldType.Long, true))
                .Add(F("reward", ContractFieldType.Float, true))
                .Add(F("shaping", ContractFieldType.Float, true))
                .Add(F("won", ContractFieldType.Bool, true))
                .Add(F("lost", ContractFieldType.Bool, true))
                .Add(F("draw", ContractFieldType.Bool, true))
                .Add(F("timeout", ContractFieldType.Bool, true))
                .Add(F("decisions", ContractFieldType.Int, true))
                .Add(F("turns", ContractFieldType.Int, true)));

            // ---- Multiplayer session boundary ----
            Register(r, new EventContract("moyva.net.peer", 1, "moyva.net.peer", ProducerId)
            {
                Description = "Multiplayer peer connect/disconnect transitions.",
                Priority = TelemetryPriority.High,
            }
                .Add(F("kind", ContractFieldType.String, true, "connected|disconnected"))
                .Add(F("peerIdHash", ContractFieldType.String, true, "SHA-256 of peer id; raw id never leaves device")));

            return r;
        }

        /// <summary>
        /// Semantic fingerprint inputs for Moyva: every registered contract plus the
        /// product marker. Rule/balance values that change dataset semantics should be
        /// appended by the caller via input.WithSemantic before computing.
        /// </summary>
        public static FingerprintInput CreateFingerprintInput(ContractRegistry registry)
        {
            var input = new FingerprintInput();
            foreach (var c in registry.All()) input.WithContract(c);
            input.WithSemantic("product", ProducerId);
            input.WithPipeline("validator", "1");
            input.WithPipeline("normalizer", "1");
            return input;
        }

        private static ContractField F(string name, ContractFieldType type, bool required = true,
            string semantics = null, string unit = null)
            => new ContractField(name, type, required) { Semantics = semantics, Unit = unit };

        private static void Register(ContractRegistry r, EventContract c)
        {
            if (r.TryResolve(c.ContractId, c.Version, out _)) return;
            r.Register(c);
        }
    }
}
