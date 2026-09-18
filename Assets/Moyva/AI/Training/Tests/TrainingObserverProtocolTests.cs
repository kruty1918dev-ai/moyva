using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Kruty1918.Moyva.AI.Training.Tests
{
    public sealed class TrainingObserverProtocolTests
    {
        [Test]
        public void SupportedVersionAccepted()
        {
            var command = Cmd(ObserverCommandKind.Subscribe, 0);
            byte[] frame = TrainingObserverProtocol.EncodeFrame(TrainingObserverProtocol.CreateCommand(command), 4096);
            Assert.IsTrue(TrainingObserverProtocol.TryDecodeFrame(frame, 4096, out var envelope, out string reason), reason);
            Assert.AreEqual(TrainingObserverProtocol.CurrentVersion, envelope.protocolVersion);
        }

        [Test]
        public void WrongVersionRejected()
        {
            var envelope = TrainingObserverProtocol.CreateCommand(Cmd(ObserverCommandKind.Subscribe, 0));
            string json = TrainingObserverProtocol.SerializeEnvelope(envelope).Replace("\"protocolVersion\":1", "\"protocolVersion\":99");
            Assert.IsFalse(TrainingObserverProtocol.TryDecodeMessage(Encoding.UTF8.GetBytes(json), 4096, out _, out string reason));
            StringAssert.Contains("Unsupported observer protocol version", reason);
        }

        [Test]
        public void OversizedMessageRejected()
        {
            const int max = 1024;
            int length = max + 1;
            byte[] frame = { (byte)(length & 255), (byte)((length >> 8) & 255), (byte)((length >> 16) & 255), (byte)((length >> 24) & 255) };
            Assert.IsFalse(TrainingObserverProtocol.TryDecodeFrame(frame, max, out _, out string reason));
            StringAssert.Contains("exceeds", reason);
        }

        [TestCase(ObserverCommandKind.Subscribe)]
        [TestCase(ObserverCommandKind.Unsubscribe)]
        [TestCase(ObserverCommandKind.RequestFullSnapshot)]
        [TestCase(ObserverCommandKind.SelectAgent)]
        public void CommandsParsed(ObserverCommandKind kind)
        {
            var source = Cmd(kind, 3); source.agentId = "agent-a"; source.lastSequence = 44; source.visuals = true;
            byte[] body = TrainingObserverProtocol.EncodeMessage(TrainingObserverProtocol.CreateCommand(source));
            Assert.IsTrue(TrainingObserverProtocol.TryDecodeMessage(body, 4096, out var envelope, out string decodeReason), decodeReason);
            Assert.IsTrue(TrainingObserverProtocol.TryParseCommand(envelope, out var parsed, out string reason), reason);
            Assert.AreEqual(kind, parsed.kind);
            Assert.AreEqual(3, parsed.arenaId);
            Assert.AreEqual(44, parsed.lastSequence);
            Assert.AreEqual("agent-a", parsed.agentId);
            Assert.IsTrue(parsed.visuals);
        }

        [Test]
        public void SequencePreserved()
        {
            var decision = Decision(0, 7, 123);
            var envelope = TrainingObserverProtocol.Create(TrainingObserverProtocol.AgentDecision, "session", 0, 7, 123, decision);
            byte[] body = TrainingObserverProtocol.EncodeMessage(envelope);
            Assert.IsTrue(TrainingObserverProtocol.TryDecodeMessage(body, 8192, out var decoded, out string reason), reason);
            Assert.AreEqual(123, decoded.sequence);
            Assert.AreEqual(7, decoded.episodeId);
        }

        [Test]
        public void EpisodeIdsDoNotMix()
        {
            using var server = new TrainingObserverServer("session", "test", 16, 8192);
            using var client = server.CreateInMemoryClient();
            client.Send(Cmd(ObserverCommandKind.Subscribe, 0)); client.Drain();
            Assert.That(server.CollectSnapshotTargets(new[] { 0 }, false), Does.Contain(0));
            server.PublishSnapshot(Snapshot(0, 1, 10), false); client.Drain();
            server.PublishDecision(Decision(0, 1, 11));
            Assert.That(client.Drain().Single(x => x.messageType == TrainingObserverProtocol.AgentDecision).episodeId, Is.EqualTo(1));

            server.MarkEpisodeReset(0, 2, 11);
            Assert.That(client.Drain().Any(x => x.messageType == TrainingObserverProtocol.ResyncRequired), Is.True);
            server.PublishDecision(Decision(0, 2, 12));
            Assert.That(client.Drain().Any(x => x.messageType == TrainingObserverProtocol.AgentDecision), Is.False);
            Assert.That(server.CollectSnapshotTargets(new[] { 0 }, false), Does.Contain(0));
            server.PublishSnapshot(Snapshot(0, 2, 12), false);
            Assert.That(client.Drain().Single(x => x.messageType == TrainingObserverProtocol.Snapshot).episodeId, Is.EqualTo(2));
            server.PublishDecision(Decision(0, 2, 13));
            Assert.That(client.Drain().Single(x => x.messageType == TrainingObserverProtocol.AgentDecision).episodeId, Is.EqualTo(2));
        }

        [Test]
        public void SequenceGapReturnsResyncRequirement()
        {
            using var server = new TrainingObserverServer("session", "test", 16, 8192);
            for (int i = 1; i <= 80; i++) server.PublishDecision(Decision(0, 1, i));
            using var client = server.CreateInMemoryClient();
            var command = Cmd(ObserverCommandKind.Subscribe, 0); command.lastSequence = 1;
            client.Send(command);
            Assert.That(client.Drain().Any(x => x.messageType == TrainingObserverProtocol.ResyncRequired), Is.True);
            Assert.That(server.CollectSnapshotTargets(new[] { 0 }, false), Does.Contain(0));
        }

        [Test]
        public void ReconnectRequiresFullSnapshot()
        {
            using var server = new TrainingObserverServer("session", "test", 16, 8192);
            using (var first = server.CreateInMemoryClient())
            {
                first.Send(Cmd(ObserverCommandKind.Subscribe, 0)); first.Drain();
                server.CollectSnapshotTargets(new[] { 0 }, false);
                server.PublishSnapshot(Snapshot(0, 1, 5), false); first.Drain();
            }
            using var second = server.CreateInMemoryClient();
            var reconnect = Cmd(ObserverCommandKind.Subscribe, 0); reconnect.lastSequence = 5;
            second.Send(reconnect);
            Assert.That(server.CollectSnapshotTargets(new[] { 0 }, false), Does.Contain(0));
        }

        [Test]
        public void SlowOverflowingQueueDoesNotBlockProducer()
        {
            using var server = new TrainingObserverServer("session", "test", 8, 8192);
            using var client = server.CreateInMemoryClient();
            client.Send(Cmd(ObserverCommandKind.Subscribe, 0)); client.Drain();
            server.CollectSnapshotTargets(new[] { 0 }, false);
            server.PublishSnapshot(Snapshot(0, 1, 1), false); client.Drain();
            for (int i = 2; i < 1000; i++) server.PublishDecision(Decision(0, 1, i));
            Assert.Greater(server.ResyncCount, 0);
            Assert.LessOrEqual(client.QueuedCount, 8);
        }

        [Test]
        public void MalformedMessageDoesNotThrowOutOfServerLoop()
        {
            using var server = new TrainingObserverServer("session", "test", 16, 8192);
            using var client = server.CreateInMemoryClient();
            Assert.DoesNotThrow(() => client.ProcessRaw(Encoding.UTF8.GetBytes("{not-json")));
            Assert.That(client.Drain().Any(x => x.messageType == TrainingObserverProtocol.Rejected), Is.True);
            byte[] valid = TrainingObserverProtocol.EncodeMessage(TrainingObserverProtocol.CreateCommand(Cmd(ObserverCommandKind.Subscribe, 0)));
            Assert.IsTrue(client.ProcessRaw(valid));
            Assert.That(client.Drain().Any(x => x.messageType == TrainingObserverProtocol.Subscribed), Is.True);
        }

        [Test]
        public void UnsubscribeDoesNotChangeTrainingEnvironmentState()
        {
            using var environment = NewEnvironment();
            long episode = environment.EpisodeId; bool ready = environment.IsReady; int decisions = environment.Diagnostics.Decisions;
            using var server = new TrainingObserverServer("session", "test", 16, 8192);
            using var client = server.CreateInMemoryClient();
            client.Send(Cmd(ObserverCommandKind.Subscribe, 0)); client.Send(Cmd(ObserverCommandKind.Unsubscribe, 0));
            Assert.AreEqual(episode, environment.EpisodeId);
            Assert.AreEqual(ready, environment.IsReady);
            Assert.AreEqual(decisions, environment.Diagnostics.Decisions);
        }

        [Test]
        public void ObserverDisconnectDoesNotEndEpisode()
        {
            using var environment = NewEnvironment();
            long episode = environment.EpisodeId;
            using var server = new TrainingObserverServer("session", "test", 16, 8192);
            var client = server.CreateInMemoryClient();
            client.Send(Cmd(ObserverCommandKind.Subscribe, 0));
            client.Dispose();
            Assert.AreEqual(episode, environment.EpisodeId);
            Assert.IsTrue(environment.IsReady);
            Assert.AreEqual(TrainingEpisodeResult.None, environment.Result);
        }

        private static TrainingEnvironment NewEnvironment()
        {
            var environment = new TrainingEnvironment(0, new TrainingConfig { allowScaffoldSimulation = true },
                new ScaffoldSimulationFactory().Create(0));
            environment.BeginEpisode();
            return environment;
        }

        private static ObserverCommand Cmd(ObserverCommandKind kind, int arena) => new ObserverCommand
        {
            kind = kind, arenaId = arena, sessionId = "session", visuals = true
        };

        private static AgentDecisionEvent Decision(int arena, long episode, long sequence) => new AgentDecisionEvent
        {
            sessionId = "session", arenaId = arena, episodeId = episode, sequence = sequence,
            agentId = "agent", actionId = "wait", result = "Completed"
        };

        private static ArenaSnapshot Snapshot(int arena, long episode, long sequence) => new ArenaSnapshot
        {
            sessionId = "session", arenaId = arena, episodeId = episode, sequence = sequence,
            width = 24, height = 24, isComplete = false, status = "partial"
        };
    }
}
