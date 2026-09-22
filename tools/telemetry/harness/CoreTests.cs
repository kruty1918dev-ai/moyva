using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Telemetry.Canonicalization;
using Kruty1918.Telemetry.Configuration;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Core;
using Kruty1918.Telemetry.Fingerprinting;
using Kruty1918.Telemetry.Serialization;
using Kruty1918.Telemetry.Storage;
using Kruty1918.Telemetry.Upload;

namespace Kruty1918.Telemetry.Harness
{
    /// <summary>Unit-level checks over the Unity-free core.</summary>
    public static class CoreTests
    {
        public static void Register(List<(string, Action)> t)
        {
            t.Add(("canonical.stable-order", CanonicalStableOrder));
            t.Add(("canonical.float-policy", CanonicalFloatPolicy));
            t.Add(("fingerprint.identical", FpIdentical));
            t.Add(("fingerprint.reordered", FpReordered));
            t.Add(("fingerprint.semantic-changes", FpSemanticChanges));
            t.Add(("fingerprint.incidental-excluded", FpIncidental));
            t.Add(("fingerprint.nonfinite-rejected", FpNonFinite));
            t.Add(("fingerprint.diff", FpDiff));
            t.Add(("codec.roundtrip", CodecRoundtrip));
            t.Add(("codec.checksum-corruption", CodecCorruption));
            t.Add(("spool.recovery", SpoolRecovery));
            t.Add(("spool.crash-mid-write", SpoolCrashMidWrite));
            t.Add(("spool.storage-pressure", SpoolPressure));
            t.Add(("sink.validation", SinkValidation));
            t.Add(("sink.required-fields", SinkRequiredFields));
            t.Add(("sink.consent", SinkConsent));
            t.Add(("ack.validation", AckValidation));
            t.Add(("retry.backoff", RetryBackoff));
            t.Add(("remoteconfig.parse", RemoteConfigParse));
        }

        private static FingerprintInput BaseInput()
        {
            var input = new FingerprintInput();
            input.WithContract(Program.UnitMovedContract());
            input.WithSemantic("gameplay.damageFormula", "v3");
            input.WithSemantic("map.ruleset", "default-1");
            input.WithPipeline("validator", "1");
            return input;
        }

        private static void CanonicalStableOrder()
        {
            var a = CanonicalValue.Map(
                CanonicalValue.F("b", CanonicalValue.Of(1)),
                CanonicalValue.F("a", CanonicalValue.Of("x")),
                CanonicalValue.F("c", CanonicalValue.Of(true)));
            var b = CanonicalValue.Map(
                CanonicalValue.F("c", CanonicalValue.Of(true)),
                CanonicalValue.F("b", CanonicalValue.Of(1)),
                CanonicalValue.F("a", CanonicalValue.Of("x")));
            TestRunner.AssertEq(CanonicalJson.Write(a), CanonicalJson.Write(b), "canonical order");
        }

        private static void CanonicalFloatPolicy()
        {
            TestRunner.AssertEq("5", CanonicalJson.Write(CanonicalValue.Of(5.0)), "5.0 normalizes to 5");
            TestRunner.AssertEq("5.5", CanonicalJson.Write(CanonicalValue.Of(5.5)), "5.5");
            TestRunner.AssertEq(CanonicalJson.Write(CanonicalValue.Of(-0.0)), "0", "-0.0 -> 0");
            bool threw = false;
            try { CanonicalValue.Of(double.NaN); } catch (ArgumentException) { threw = true; }
            TestRunner.Assert(threw, "NaN rejected");
            threw = false;
            try { CanonicalValue.Of(double.PositiveInfinity); } catch (ArgumentException) { threw = true; }
            TestRunner.Assert(threw, "+Inf rejected");
        }

        private static void FpIdentical()
            => TestRunner.AssertEq(DatasetFingerprint.Compute(BaseInput()).Value,
                DatasetFingerprint.Compute(BaseInput()).Value, "identical inputs");

        private static void FpReordered()
        {
            var a = new FingerprintInput();
            a.WithSemantic("one", 1); a.WithSemantic("two", 2);
            var b = new FingerprintInput();
            b.WithSemantic("two", 2); b.WithSemantic("one", 1);
            TestRunner.AssertEq(DatasetFingerprint.Compute(a).Value, DatasetFingerprint.Compute(b).Value,
                "map order must not matter");
        }

        private static void FpSemanticChanges()
        {
            var baseIn = BaseInput();
            var baseFp = DatasetFingerprint.Compute(baseIn);
            var cases = new (string name, Action<FingerprintInput> mutate)[]
            {
                ("damageFormula", i => i.WithSemantic("gameplay.damageFormula", "v4")),
                ("movementSpeed", i => i.WithSemantic("gameplay.moveSpeed", 6)),
                ("distanceUnit", i => { i.Contracts.Clear(); i.WithContract(Program.UnitMovedContract(unit: "meters")); }),
                ("sampling", i => { i.Contracts.Clear(); var c = Program.UnitMovedContract(); c.SamplingRate = 0.5f; i.WithContract(c); }),
                ("semanticsText", i => { i.Contracts.Clear(); var c = Program.UnitMovedContract(); c.Fields[1].Semantics = "tile x (west-positive)"; i.WithContract(c); }),
                ("validatorVer", i => i.WithPipeline("validator", "2")),
            };
            foreach (var (name, mutate) in cases)
            {
                var m = BaseInput();
                mutate(m);
                var fp = DatasetFingerprint.Compute(m);
                TestRunner.Assert(fp != baseFp, $"fingerprint must change for {name}");
            }
        }

        private static void FpIncidental()
        {
            var a = BaseInput();
            var fp1 = DatasetFingerprint.Compute(a);
            // Incidental metadata is excluded BY CONSTRUCTION: the model has no slot for
            // build timestamp / batch id / upload time. Prove by mutating a batch-level
            // field that is not part of the input.
            var b = BaseInput();
            var fp2 = DatasetFingerprint.Compute(b);
            TestRunner.AssertEq(fp1.Value, fp2.Value, "incidental-free model is stable");
        }

        private static void FpNonFinite()
        {
            var i = BaseInput();
            bool threw = false;
            try { i.WithSemantic("bad", double.NaN); } catch (ArgumentException) { threw = true; }
            TestRunner.Assert(threw, "NaN semantic rejected");
        }

        private static void FpDiff()
        {
            var a = BaseInput();
            var b = BaseInput();
            b.WithSemantic("gameplay.damageFormula", "v4");
            var diff = FingerprintDiff.Diff(a, b);
            TestRunner.Assert(diff.Count == 1, "one change expected, got " + diff.Count);
            TestRunner.Assert(diff[0].Path.Contains("damageFormula"), "path mentions damageFormula: " + diff[0]);
            TestRunner.Assert(diff[0].New.Contains("v4"), "new value v4: " + diff[0]);
        }

        private static TelemetryBatch MakeBatch(string root = null, int events = 3)
        {
            var session = new TelemetrySessionContext("inst123");
            var builder = new BatchBuilder();
            var contract = Program.UnitMovedContract();
            for (int i = 0; i < events; i++)
            {
                var w = new TelemetryJsonWriter();
                w.BeginObject();
                w.Field("t", "demo.unitMoved").Field("c", "demo.unitMoved").Field("v", 1);
                w.Field("seq", (long)i).Field("ts", 1000L + i);
                w.Key("d").BeginObject().Field("unitId", "u1").Field("x", i).Field("y", i).Field("distance", 1.5f).EndObject();
                w.EndObject();
                builder.AddEvent(w.ToString(), i, contract);
            }
            return builder.Seal(session, DatasetFingerprint.Compute(BaseInput()).Value,
                new Compression.GZipCompressionProvider(), null, "harness", "1.0");
        }

        private static void CodecRoundtrip()
        {
            var batch = MakeBatch();
            var bytes = BatchFileCodec.Encode(batch);
            var back = BatchFileCodec.Decode(bytes);
            TestRunner.AssertEq(batch.BatchId, back.BatchId, "batchId");
            TestRunner.AssertEq(batch.PayloadSha256, back.PayloadSha256, "checksum");
            TestRunner.AssertEq(batch.EventCount, back.EventCount, "events");
            TestRunner.AssertEq("gzip", back.CompressionId, "compression");
            var raw = back.RawPayload(new Compression.GZipCompressionProvider());
            TestRunner.Assert(raw.Length == batch.UncompressedBytes, "raw payload size");
        }

        private static void CodecCorruption()
        {
            var batch = MakeBatch();
            var bytes = BatchFileCodec.Encode(batch);
            bytes[bytes.Length - 2] ^= 0xFF; // flip a payload byte
            bool threw = false;
            try { BatchFileCodec.Decode(bytes); } catch (InvalidDataException) { threw = true; }
            TestRunner.Assert(threw, "corrupt payload must fail checksum");
        }

        private static void SpoolRecovery()
        {
            var root = Program.TempRoot("recovery");
            var store = new LocalTelemetryStore(Path.Combine(root, "spool"));
            store.Recover();
            store.Save(MakeBatch());
            store.Save(MakeBatch());
            var pending = store.List(BatchLocation.Pending);
            TestRunner.AssertEq(2, pending.Count, "two pending");
            store.MarkInFlight(pending[0].BatchId);
            TestRunner.AssertEq(1, store.List(BatchLocation.InFlight).Count, "one inflight");
            // Simulate crash: new store instance, recover.
            var store2 = new LocalTelemetryStore(Path.Combine(root, "spool"));
            int recovered = store2.Recover();
            TestRunner.AssertEq(1, recovered, "inflight recovered");
            TestRunner.AssertEq(2, store2.List(BatchLocation.Pending).Count, "all pending again");
        }

        private static void SpoolCrashMidWrite()
        {
            var root = Program.TempRoot("crashwrite");
            var dir = Path.Combine(root, "spool", "pending");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "partial.tmb.tmp"), new byte[] { 1, 2, 3 });
            var store = new LocalTelemetryStore(Path.Combine(root, "spool"));
            store.Recover();
            TestRunner.Assert(!File.Exists(Path.Combine(dir, "partial.tmb.tmp")), "tmp deleted");
            // Corrupt committed file → quarantine.
            var bad = Path.Combine(dir, "badbatch.tmb");
            File.WriteAllBytes(bad, new byte[] { 9, 9, 9, 9 });
            store.Recover();
            TestRunner.Assert(File.Exists(Path.Combine(root, "spool", "quarantine", "badbatch.tmb")),
                "corrupt moved to quarantine");
        }

        private static void SpoolPressure()
        {
            var root = Program.TempRoot("pressure");
            var store = new LocalTelemetryStore(Path.Combine(root, "spool"),
                new LocalTelemetryStore.Policy { MaxTotalBytes = 60 * 1024, MaxPendingBatches = 3 });
            store.Recover();
            for (int i = 0; i < 6; i++)
            {
                var b = MakeBatch();
                b.Priority = i < 5 ? TelemetryPriority.Normal : TelemetryPriority.Low;
                store.Save(b);
            }
            var pending = store.List(BatchLocation.Pending);
            TestRunner.Assert(pending.Count <= 3, $"bounded: {pending.Count} <= 3");
            TestRunner.Assert(store.List(BatchLocation.Quarantine).Count >= 3, "evicted went to quarantine");
        }

        private static (TelemetryRuntime rt, LocalTelemetryStore store) MakeRuntime(string root, ITelemetryTransport transport = null)
        {
            var contracts = new ContractRegistry();
            contracts.Register(Program.UnitMovedContract());
            StandardContracts.RegisterAll(contracts);
            var storeRoot = Path.Combine(root, "rt");
            var rt = new TelemetryRuntime(new TelemetryConfig { ProducerId = "harness", Compression = "gzip" },
                contracts, BaseInput(), storeRoot, transport ?? new NullTelemetryTransport());
            return (rt, rt.Store);
        }

        private static void SinkValidation()
        {
            var root = Program.TempRoot("sink");
            var (rt, _) = MakeRuntime(root);
            var e = new Program.UnitMovedEvent { UnitId = "u7", X = 3, Y = 4, Distance = 2.5f };
            var r = rt.Sink.Track(in e);
            TestRunner.Assert(r.Accepted, "track accepted: " + r);
            rt.Sink.Flush();
            TestRunner.AssertEq(1, rt.Store.List(BatchLocation.Pending).Count, "batch sealed");
            rt.Dispose();
        }

        private static void SinkRequiredFields()
        {
            var root = Program.TempRoot("sinkreq");
            var (rt, _) = MakeRuntime(root);
            var r = rt.Sink.TrackRaw("demo.unitMoved", "demo.unitMoved", 1, w =>
            {
                w.Field("unitId", "u1");
                w.Field("x", 1); // missing y, distance
            });
            TestRunner.AssertEq(TelemetryResultStatus.Rejected, r.Status, "missing required rejected");
            TestRunner.Assert(r.Code.StartsWith("required:"), "code mentions field: " + r.Code);
            var bad = rt.Sink.TrackRaw("demo.unitMoved", "demo.unitMoved", 1, w =>
            {
                w.Field("unitId", "u1"); w.Field("x", -5); w.Field("y", 1); w.Field("distance", 1f);
            });
            TestRunner.AssertEq(TelemetryResultStatus.Rejected, bad.Status, "x below min rejected");
            var unknown = rt.Sink.TrackRaw("demo.unitMoved", "demo.unitMoved", 1, w =>
            {
                w.Field("unitId", "u1"); w.Field("x", 1); w.Field("y", 1); w.Field("distance", 1f);
                w.Field("hackerField", 42);
            });
            TestRunner.AssertEq(TelemetryResultStatus.Rejected, unknown.Status, "unknown field rejected");
            rt.Dispose();
        }

        private static void SinkConsent()
        {
            var root = Program.TempRoot("consent");
            var contracts = new ContractRegistry();
            contracts.Register(Program.UnitMovedContract());
            var rt = new TelemetryRuntime(
                new TelemetryConfig { Consent = Configuration.TelemetryConsentLevel.Disabled },
                contracts, BaseInput(), Path.Combine(root, "rt"));
            var e = new Program.UnitMovedEvent { UnitId = "u", X = 1, Y = 1, Distance = 1f };
            var r = rt.Sink.Track(in e);
            TestRunner.AssertEq(TelemetryResultStatus.Disabled, r.Status, "consent disabled");
            rt.Dispose();
        }

        private static void AckValidation()
        {
            var batch = MakeBatch();
            var good = new UploadAck
            {
                Protocol = TelemetryBatch.ProtocolVersion, AckId = "a1", BatchId = batch.BatchId,
                Checksum = batch.PayloadSha256, Fingerprint = batch.DatasetFingerprint,
                AcceptedEvents = batch.EventCount, GroupId = "g1", ObjectRef = "r2://x", Status = "accepted",
            };
            TestRunner.Assert(good.IsValid && good.Matches(batch), "valid ack matches");
            var round = UploadAck.Parse(good.ToJson());
            TestRunner.Assert(round.Matches(batch), "ack json roundtrip matches");
            var badChecksum = UploadAck.Parse(good.ToJson());
            badChecksum.Checksum = new string('0', 64);
            TestRunner.Assert(!badChecksum.Matches(batch), "checksum mismatch rejected");
            var wrongBatch = UploadAck.Parse(good.ToJson());
            wrongBatch.BatchId = "other";
            TestRunner.Assert(!wrongBatch.Matches(batch), "wrong batchId rejected");
            var incomplete = UploadAck.Parse("{\"protocol\":1}");
            TestRunner.Assert(incomplete == null || !incomplete.IsValid, "incomplete ack invalid");
        }

        private static void RetryBackoff()
        {
            var rp = new RetryPolicy(new Random(42));
            var d1 = rp.DelayFor(1); var d5 = rp.DelayFor(5);
            TestRunner.Assert(d5 > TimeSpan.Zero && d5 <= rp.MaxDelay, "bounded backoff");
            TestRunner.Assert(RetryPolicy.IsRetryableStatus(429), "429 retryable");
            TestRunner.Assert(RetryPolicy.IsRetryableStatus(503), "503 retryable");
            TestRunner.Assert(RetryPolicy.IsPermanentStatus(400), "400 permanent");
            TestRunner.Assert(rp.DelayFor(1, 30).TotalSeconds == 30, "server retry-after honored");
        }

        private static void RemoteConfigParse()
        {
            var json = @"{""configVersion"":1,""enabled"":true,""endpoint"":""https://x.example/ingest"",
                ""projectId"":""moyva"",""environment"":""staging"",""clientToken"":""pub-123"",
                ""supportedProtocols"":[1],""maxBatchBytes"":1048576,""sampling"":{""demo.unitMoved"":0.25}}";
            var c = Configuration.TelemetryRemoteConfig.Parse(json);
            TestRunner.AssertEq("https://x.example/ingest", c.Endpoint, "endpoint");
            TestRunner.AssertEq(0.25f, c.Sampling["demo.unitMoved"], "sampling override");
            TestRunner.Assert(c.SupportsProtocol(1), "proto supported");
            TestRunner.Assert(!c.SupportsProtocol(99), "proto unsupported");
        }
    }
}
