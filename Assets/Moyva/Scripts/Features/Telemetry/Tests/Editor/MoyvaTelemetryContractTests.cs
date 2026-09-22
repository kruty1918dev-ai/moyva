using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Telemetry;
using Kruty1918.Telemetry.Configuration;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Core;
using Kruty1918.Telemetry.Fingerprinting;
using Kruty1918.Telemetry.Upload;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Telemetry
{
    public class MoyvaTelemetryContractTests
    {
        private TelemetryRuntime _runtime;
        private string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "moyva-tel-" + Path.GetRandomFileName());
            var registry = MoyvaTelemetryContracts.CreateRegistry();
            _runtime = new TelemetryRuntime(
                new TelemetryConfig
                {
                    Consent = TelemetryConsentLevel.Research, // allow Research-class events
                    Enabled = true,
                },
                registry,
                MoyvaTelemetryContracts.CreateFingerprintInput(registry),
                _root,
                transport: new NullTelemetryTransport());
        }

        [TearDown]
        public void TearDown()
        {
            _runtime?.Dispose();
            try { if (Directory.Exists(_root)) Directory.Delete(_root, true); } catch { }
        }

        private static readonly string[] AllMoyvaEventTypes =
        {
            "moyva.match.started", "moyva.match.ended", "moyva.match.paused", "moyva.match.modeChanged",
            "moyva.turn.lifecycle",
            "moyva.construction.placed", "moyva.construction.cancelled", "moyva.construction.rejected",
            "moyva.construction.demolished", "moyva.construction.transferred",
            "moyva.units.created", "moyva.units.moved", "moyva.units.destroyed", "moyva.units.moveRejected",
            "moyva.recruitment.queueChanged", "moyva.recruitment.ready", "moyva.recruitment.deployed",
            "moyva.recruitment.rejected",
            "moyva.economy.tick", "moyva.economy.settlement", "moyva.economy.resource",
            "moyva.fog.changed", "moyva.world.generated",
            "moyva.save.saved", "moyva.save.loadRequested",
            "moyva.ui.panel",
            "moyva.ai.bot.decision", "moyva.ai.bot.episode",
            "moyva.net.peer",
        };

        [Test]
        public void Registry_ResolvesEveryMoyvaEventContract()
        {
            var registry = MoyvaTelemetryContracts.CreateRegistry();
            foreach (var type in AllMoyvaEventTypes)
            {
                Assert.IsTrue(registry.TryResolve(type, 1, out var c), $"no contract for {type}");
                Assert.IsTrue(c.MatchesEventType(type));
            }
        }

        [Test]
        public void Sink_AcceptsRepresentativeEvents()
        {
            var sink = _runtime.Sink;
            Assert.IsTrue(sink.Track(new MoyvaMatchLifecycleEvent { EventType = "moyva.match.started" }).Accepted);
            Assert.IsTrue(sink.Track(new MoyvaUnitEvent
            {
                EventType = "moyva.units.moved", UnitId = "u1", X = 3, Y = 4, Cost = 1.5f,
            }).Accepted);
            Assert.IsTrue(sink.Track(new MoyvaEconomyTickEvent
            {
                SettlementId = "s1", OwnerId = "p1", Turn = 2, Population = 10,
            }).Accepted);
            Assert.IsTrue(sink.Track(new MoyvaWorldGeneratedEvent
            {
                Source = "GeneratedHost", Width = 64, Height = 64, CellSize = 1f,
            }).Accepted);
            Assert.IsTrue(sink.Track(new MoyvaBotEpisodeEvent
            {
                Episode = 1, Reward = 0.5f, Won = true, Decisions = 10, Turns = 3,
            }).Accepted);
        }

        [Test]
        public void Sink_RejectsMissingRequiredField()
        {
            var r = _runtime.Sink.Track(new MoyvaUnitEvent
            {
                EventType = "moyva.units.moved", X = 1, Y = 1, Cost = 1f, // no UnitId
            });
            Assert.IsFalse(r.Accepted);
            Assert.AreEqual(TelemetryResultStatus.Rejected, r.Status);
        }

        [Test]
        public void Fingerprint_DeterministicAndSemanticSensitive()
        {
            var a = MoyvaTelemetryContracts.CreateFingerprintInput(MoyvaTelemetryContracts.CreateRegistry());
            var b = MoyvaTelemetryContracts.CreateFingerprintInput(MoyvaTelemetryContracts.CreateRegistry());
            Assert.AreEqual(DatasetFingerprint.Compute(a).Value, DatasetFingerprint.Compute(b).Value);

            b.WithSemantic("worldgen.rules", "v2");
            Assert.AreNotEqual(DatasetFingerprint.Compute(a).Value, DatasetFingerprint.Compute(b).Value);
        }

        [Test]
        public void Consent_Analytics_DropsResearchEvents()
        {
            _runtime.Dispose();
            var registry = MoyvaTelemetryContracts.CreateRegistry();
            _runtime = new TelemetryRuntime(
                new TelemetryConfig { Consent = TelemetryConsentLevel.Analytics },
                registry,
                MoyvaTelemetryContracts.CreateFingerprintInput(registry),
                _root,
                transport: new NullTelemetryTransport());

            var r = _runtime.Sink.Track(new MoyvaBotEpisodeEvent
            { Episode = 1, Reward = 1f, Won = true, Decisions = 5, Turns = 2 });
            Assert.IsFalse(r.Accepted, "research-class event must not record under analytics consent");

            var ok = _runtime.Sink.Track(new MoyvaFogChangedEvent { ChangedTiles = 3 });
            Assert.IsTrue(ok.Accepted);
        }
    }
}
