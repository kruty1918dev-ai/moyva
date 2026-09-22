using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Kruty1918.Moyva.Telemetry;
using Kruty1918.Telemetry.Configuration;
using Kruty1918.Telemetry.Core;
using Kruty1918.Telemetry.Storage;
using Kruty1918.Telemetry.Upload;

namespace Kruty1918.Telemetry.Harness
{
    /// <summary>
    /// Acceptance scenario B: the real Moyva contract set + event payloads flow
    /// through the real pipeline to the real Node ingestion backend over HTTP.
    /// Covers: contract coverage, server checksum/group/batch persistence, ACK,
    /// and safe local cleanup — the full Moyva → backend roundtrip.
    /// </summary>
    public static class ScenarioBTests
    {
        public static void Register(List<(string, Action)> t)
            => t.Add(("scenarioB.moyva-to-node-backend", ScenarioB));

        private static void ScenarioB()
        {
            var root = Program.TempRoot("scenarioB");
            int port = 8977;
            var serverJs = FindUp("Packages/com.kruty1918.telemetry/BackendReference/local/server.js");
            if (serverJs == null) throw new Exception("server.js not found");

            Process node = null;
            try
            {
                node = Process.Start(new ProcessStartInfo("node",
                    $"\"{serverJs}\" --port {port} --data \"{Path.Combine(root, "backend")}\"")
                { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false });
                WaitForHealth(port);

                var registry = MoyvaTelemetryContracts.CreateRegistry();
                var rt = new TelemetryRuntime(
                    new TelemetryConfig
                    {
                        ProducerId = MoyvaTelemetryContracts.ProducerId,
                        Consent = TelemetryConsentLevel.Research,
                        Compression = "gzip",
                    },
                    registry,
                    MoyvaTelemetryContracts.CreateFingerprintInput(registry),
                    Path.Combine(root, "client"),
                    new HttpTelemetryTransport());
                rt.Uploader.Endpoint = () => $"http://127.0.0.1:{port}/v1/batch";
                rt.Uploader.ProjectId = () => "moyva";
                rt.Uploader.EnvironmentName = () => "test";

                // A representative spread across every contract family.
                var events = new ITelemetryEvent[]
                {
                    new MoyvaMatchLifecycleEvent { EventType = "moyva.match.started", Mode = "normal", Source = "new" },
                    new MoyvaWorldGeneratedEvent { Source = "GeneratedHost", Width = 64, Height = 64, CellSize = 1f },
                    new MoyvaTurnLifecycleEvent { Phase = "started", Round = 1, GlobalTurn = 1, FactionIndex = 0, OwnerId = "p1" },
                    new MoyvaConstructionEvent { EventType = "moyva.construction.placed", BuildingId = "farm", X = 3, Y = 3, OwnerId = "p1", Rotation = 0 },
                    new MoyvaUnitEvent { EventType = "moyva.units.created", UnitId = "u1", UnitTypeId = "warrior", X = 2, Y = 2, OwnerId = "p1" },
                    new MoyvaUnitEvent { EventType = "moyva.units.moved", UnitId = "u1", X = 3, Y = 2, Cost = 1f, OwnerId = "p1" },
                    new MoyvaRecruitmentEvent { EventType = "moyva.recruitment.deployed", OwnerId = "p1", UnitTypeId = "warrior", QueueId = 9, BuildingX = 4, BuildingY = 4, UnitId = "u2" },
                    new MoyvaEconomyTickEvent { SettlementId = "s1", OwnerId = "p1", Turn = 1, Population = 8 },
                    new MoyvaSettlementEvent { Kind = "created", SettlementId = "s1", OwnerId = "p1" },
                    new MoyvaFogChangedEvent { ChangedTiles = 12 },
                    new MoyvaUiPanelEvent { Kind = "buildingInfo", Action = "opened", ObjectId = "farm" },
                    new MoyvaNetPeerEvent { Kind = "connected", PeerIdHash = "abc123" },
                    new MoyvaSaveEvent { EventType = "moyva.save.saved", Slot = 0, Success = true },
                    new MoyvaMatchLifecycleEvent { EventType = "moyva.match.ended", WinnerId = "p1", DurationMs = 60000 },
                };
                foreach (var e in events)
                {
                    var r = Track(rt.Sink, e);
                    TestRunner.Assert(r.Accepted, $"track {e.EventType}: {r}");
                }
                rt.Sink.Flush();
                TestRunner.Assert(rt.Store.List(BatchLocation.Pending).Count > 0, "spooled");

                int acked = rt.Uploader.TickAsync().GetAwaiter().GetResult();
                TestRunner.AssertEq(1, acked, "uploaded+acked over real HTTP");
                TestRunner.AssertEq(0, rt.Store.List(BatchLocation.Pending).Count, "local cleanup after ACK");
                TestRunner.AssertEq(0, rt.Store.List(BatchLocation.InFlight).Count, "no inflight");

                // Server-side verification over HTTP.
                using (var http = new HttpClient())
                {
                    var state = http.GetStringAsync($"http://127.0.0.1:{port}/debug/state")
                        .GetAwaiter().GetResult();
                    var s = Serialization.TelemetryJsonReader.Parse(state);
                    s.TryGet("groups", out var groups);
                    s.TryGet("batches", out var batches);
                    s.TryGet("objects", out var objects);
                    TestRunner.AssertEq(1, groups.Obj.Count, "one dataset group on server");
                    TestRunner.Assert(batches.Obj.Count >= 1, "batch registered");
                    TestRunner.Assert(objects.Arr.Count >= 1, "raw object persisted");
                }
                rt.Dispose();
            }
            finally
            {
                try { node?.Kill(true); } catch { }
                node?.Dispose();
            }
        }

        private static TelemetryResult Track(ITelemetrySink sink, ITelemetryEvent e)
        {
            // ITelemetryEvent is a struct-bound generic API; dispatch via TrackRaw-free path.
            return (TelemetryResult)typeof(ITelemetrySink)
                .GetMethod("Track")
                .MakeGenericMethod(e.GetType())
                .Invoke(sink, new object[] { e });
        }

        private static string FindUp(string rel)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var p = Path.Combine(dir.FullName, rel.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(p)) return p;
                dir = dir.Parent;
            }
            return null;
        }

        private static void WaitForHealth(int port)
        {
            using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) })
            {
                for (int i = 0; i < 50; i++)
                {
                    try
                    {
                        var r = http.GetStringAsync($"http://127.0.0.1:{port}/health").GetAwaiter().GetResult();
                        if (r.Contains("\"ok\":true")) return;
                    }
                    catch { System.Threading.Thread.Sleep(200); }
                }
            }
            throw new Exception("node backend did not come up");
        }
    }
}
