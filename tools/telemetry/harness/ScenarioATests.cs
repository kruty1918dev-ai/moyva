using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kruty1918.Telemetry.Configuration;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Core;
using Kruty1918.Telemetry.Fingerprinting;
using Kruty1918.Telemetry.Storage;
using Kruty1918.Telemetry.Upload;

namespace Kruty1918.Telemetry.Harness
{
    /// <summary>
    /// Acceptance scenario A (section 17): offline spool → restart → auto-sync →
    /// verified ACK → dedup resend → semantic change → second dataset group.
    /// Runs against the reference in-process server through the real coordinator path.
    /// </summary>
    public static class ScenarioATests
    {
        public static void Register(List<(string, Action)> t)
        {
            t.Add(("scenarioA.full-pipeline", ScenarioA));
            t.Add(("upload.lost-ack-resend", LostAckResend));
            t.Add(("upload.429-backoff", RetryOn429));
            t.Add(("upload.no-endpoint-stays-pending", NoEndpointStaysPending));
        }

        private static ContractRegistry RegistryWithDemo()
        {
            var r = new ContractRegistry();
            r.Register(Program.UnitMovedContract());
            StandardContracts.RegisterAll(r);
            return r;
        }

        private static FingerprintInput Fp(double speed = 5)
        {
            var i = new FingerprintInput();
            i.WithContract(Program.UnitMovedContract());
            i.WithSemantic("gameplay.moveSpeed", speed);
            i.WithSemantic("gameplay.damageFormula", "v3");
            return i;
        }

        private static void ScenarioA()
        {
            var root = Program.TempRoot("scenarioA");
            var server = new MockIngestionServer(Path.Combine(root, "raw"));
            var network = new ManualNetworkStatus { IsOnline = true };

            // Step 1-4: remote upload DISABLED (no endpoint) → generate → flush.
            var rt = new TelemetryRuntime(
                new TelemetryConfig { ProducerId = "harness", Compression = "gzip" },
                RegistryWithDemo(), Fp(), Path.Combine(root, "client"));
            for (int i = 0; i < 5; i++)
            {
                var e = new Program.UnitMovedEvent { UnitId = "u1", X = i, Y = i + 1, Distance = 1.4f };
                TestRunner.Assert(rt.Sink.Track(in e).Accepted, "track " + i);
            }
            rt.Sink.Flush();
            var pending = rt.Store.List(BatchLocation.Pending);
            TestRunner.AssertEq(1, pending.Count, "local batch exists (5)");
            string batchId = pending[0].BatchId;
            rt.Dispose(); // process "kill"

            // Step 6-7: restart → data survives + checksum valid.
            var rt2 = new TelemetryRuntime(
                new TelemetryConfig { ProducerId = "harness", Compression = "gzip" },
                RegistryWithDemo(), Fp(), Path.Combine(root, "client"));
            var pending2 = rt2.Store.List(BatchLocation.Pending);
            TestRunner.AssertEq(1, pending2.Count, "batch survived restart");
            var loaded = rt2.Store.Load(pending2[0]); // throws if checksum bad
            TestRunner.AssertEq(5, loaded.EventCount, "5 events survived");

            // Step 8-9: backend appears → automatic sync without manual upload.
            var transport = new LoopbackTransport
            {
                Handler = (h, b, ct) => Task.FromResult(server.Ingest(h, b)),
            };
            var rt3 = new TelemetryRuntime(
                new TelemetryConfig { ProducerId = "harness", Compression = "gzip" },
                RegistryWithDemo(), Fp(), Path.Combine(root, "client"),
                transport, network);
            // Wire endpoint via remote config.
            rt3.Uploader.Endpoint = () => "https://ingest.test/v1/batch";
            rt3.Uploader.ProjectId = () => "moyva-test";
            rt3.Uploader.EnvironmentName = () => "test";
            var acked = rt3.Uploader.TickAsync().GetAwaiter().GetResult();
            TestRunner.AssertEq(1, acked, "exactly one batch uploaded");
            TestRunner.AssertEq(0, rt3.Store.List(BatchLocation.Pending).Count, "local batch deleted after valid ACK");
            TestRunner.AssertEq(0, rt3.Store.List(BatchLocation.InFlight).Count, "no inflight left");

            // Step 10-14: server-side verification.
            TestRunner.AssertEq(1, server.Batches.Count, "server got exactly one batch");
            TestRunner.AssertEq(1, server.Groups.Count, "exactly one dataset group");
            var rec = server.Batches[batchId];
            TestRunner.AssertEq("accepted", rec.Status, "status");
            TestRunner.AssertEq(loaded.PayloadSha256, rec.Checksum, "checksum verified");
            TestRunner.AssertEq(rt3.Fingerprint.Value, rec.Fingerprint, "fingerprint verified");
            TestRunner.Assert(File.Exists(Path.Combine(root, "raw",
                rec.ObjectRef.Replace('/', Path.DirectorySeparatorChar))), "raw object persisted");

            // Step 15-17: resend same batch → duplicate, no new records.
            var dup = server.Ingest(new TelemetryUploadRequest
            {
                Batch = loaded, ProjectId = "moyva-test", Environment = "test",
            }.EnvelopeHeaders(), loaded.Payload);
            TestRunner.AssertEq(200, dup.Item1, "duplicate returns 200");
            var dupAck = UploadAck.Parse(dup.Item2);
            TestRunner.AssertEq("duplicate", dupAck.Status, "duplicate status");
            TestRunner.AssertEq(1, server.Batches.Count, "still one batch record");
            TestRunner.AssertEq(1, server.Groups.Count, "still one group");

            // Step 18-22: semantic change → new fingerprint → second group.
            var rt4 = new TelemetryRuntime(
                new TelemetryConfig { ProducerId = "harness", Compression = "gzip" },
                RegistryWithDemo(), Fp(speed: 6), Path.Combine(root, "client"),
                transport, network);
            rt4.Uploader.Endpoint = () => "https://ingest.test/v1/batch";
            rt4.Uploader.ProjectId = () => "moyva-test";
            rt4.Uploader.EnvironmentName = () => "test";
            TestRunner.Assert(rt4.Fingerprint != rt3.Fingerprint, "semantic change → new fingerprint");
            var e2 = new Program.UnitMovedEvent { UnitId = "u9", X = 1, Y = 1, Distance = 1f };
            rt4.Sink.Track(in e2);
            rt4.Sink.Flush();
            TestRunner.AssertEq(1, rt4.Uploader.TickAsync().GetAwaiter().GetResult(), "second batch uploaded");
            TestRunner.AssertEq(2, server.Groups.Count, "second group created for new fingerprint");
            TestRunner.AssertEq(2, server.Objects.Count, "both raw objects immutable and separate");
            var keys = server.Objects.Keys.ToList();
            TestRunner.Assert(keys[0].Contains(rt3.Fingerprint.Value) != keys[1].Contains(rt3.Fingerprint.Value),
                "objects live under different fingerprint prefixes");
            rt2.Dispose(); rt3.Dispose(); rt4.Dispose();
        }

        private static void LostAckResend()
        {
            var root = Program.TempRoot("lostack");
            var server = new MockIngestionServer();
            // Server accepts but the "network" drops the response.
            var flaky = new LoopbackTransport
            {
                Handler = (h, b, ct) =>
                {
                    var r = server.Ingest(h, b);
                    if (server.RequestCount == 1) return Task.FromResult((status: 599, body: "")); // simulated lost response
                    return Task.FromResult(r);
                },
            };
            var rt = new TelemetryRuntime(new TelemetryConfig { Compression = "none" },
                RegistryWithDemo(), Fp(), Path.Combine(root, "c"), flaky);
            rt.Uploader.Endpoint = () => "x";
            var clock = DateTime.UtcNow;
            rt.Uploader.UtcNow = () => clock;
            var e = new Program.UnitMovedEvent { UnitId = "u", X = 1, Y = 1, Distance = 1f };
            rt.Sink.Track(in e); rt.Sink.Flush();
            var acked1 = rt.Uploader.TickAsync().GetAwaiter().GetResult();
            TestRunner.AssertEq(0, acked1, "lost ACK → not deleted");
            TestRunner.AssertEq(1, server.Batches.Count, "server persisted once");
            clock += TimeSpan.FromMinutes(1); // past backoff
            var acked2 = rt.Uploader.TickAsync().GetAwaiter().GetResult();
            TestRunner.AssertEq(1, acked2, "resend ACKed as duplicate");
            TestRunner.AssertEq(1, server.Batches.Count, "no duplicate record");
            rt.Dispose();
        }

        private static void RetryOn429()
        {
            var root = Program.TempRoot("r429");
            int calls = 0;
            var t = new LoopbackTransport
            {
                Handler = (h, b, ct) => Task.FromResult(++calls == 1 ? (429, "{\"error\":\"rate\"}") : (500, "")),
            };
            var rt = new TelemetryRuntime(new TelemetryConfig { Compression = "none" },
                RegistryWithDemo(), Fp(), Path.Combine(root, "c"), t);
            rt.Uploader.Endpoint = () => "x";
            var e = new Program.UnitMovedEvent { UnitId = "u", X = 1, Y = 1, Distance = 1f };
            rt.Sink.Track(in e); rt.Sink.Flush();
            var r = rt.Uploader.TickAsync().GetAwaiter().GetResult();
            TestRunner.AssertEq(0, r, "429 → no ack, batch retained");
            TestRunner.AssertEq(1, rt.Store.List(BatchLocation.Pending).Count, "batch back to pending");
            rt.Dispose();
        }

        private static void NoEndpointStaysPending()
        {
            var root = Program.TempRoot("noendpoint");
            var rt = new TelemetryRuntime(new TelemetryConfig(), RegistryWithDemo(), Fp(),
                Path.Combine(root, "c"), new NullTelemetryTransport());
            var e = new Program.UnitMovedEvent { UnitId = "u", X = 1, Y = 1, Distance = 1f };
            rt.Sink.Track(in e); rt.Sink.Flush();
            var acked = rt.Uploader.TickAsync().GetAwaiter().GetResult();
            TestRunner.AssertEq(0, acked, "no endpoint → nothing uploaded");
            TestRunner.AssertEq(1, rt.Store.List(BatchLocation.Pending).Count, "batch pending");
            rt.Dispose();
        }
    }
}
