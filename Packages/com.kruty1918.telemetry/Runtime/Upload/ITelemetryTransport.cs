using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Telemetry.Serialization;

namespace Kruty1918.Telemetry.Upload
{
    /// <summary>
    /// One upload request: batch metadata travels in the envelope (HTTP headers),
    /// the sealed payload travels as the raw body. Transport implementations must
    /// not retry internally — retry/backoff is owned by the upload coordinator.
    /// </summary>
    public sealed class TelemetryUploadRequest
    {
        public string Endpoint;
        public string ProjectId;
        public string Environment;
        public string AuthToken; // public client token only — never a secret
        public TelemetryBatch Batch;
        public Dictionary<string, string> EnvelopeHeaders()
        {
            var h = new Dictionary<string, string>(System.StringComparer.Ordinal)
            {
                ["X-Telemetry-Protocol"] = TelemetryBatch.ProtocolVersion.ToString(),
                ["X-Telemetry-Batch-Id"] = Batch.BatchId,
                ["X-Telemetry-Fingerprint"] = Batch.DatasetFingerprint,
                ["X-Telemetry-Payload-Sha256"] = Batch.PayloadSha256,
                ["X-Telemetry-Event-Count"] = Batch.EventCount.ToString(),
                ["X-Telemetry-Installation-Id"] = Batch.InstallationId,
                ["X-Telemetry-App-Session-Id"] = Batch.ApplicationSessionId,
                ["X-Telemetry-Compression"] = Batch.CompressionId,
                ["X-Telemetry-Uncompressed-Bytes"] = Batch.UncompressedBytes.ToString(),
                ["X-Telemetry-Priority"] = ((int)Batch.Priority).ToString(),
            };
            if (Batch.GameplaySessionId != null) h["X-Telemetry-Gameplay-Session-Id"] = Batch.GameplaySessionId;
            if (Batch.MatchId != null) h["X-Telemetry-Match-Id"] = Batch.MatchId;
            if (Batch.PlayerId != null) h["X-Telemetry-Player-Id"] = Batch.PlayerId;
            if (!string.IsNullOrEmpty(ProjectId)) h["X-Telemetry-Project"] = ProjectId;
            if (!string.IsNullOrEmpty(Environment)) h["X-Telemetry-Environment"] = Environment;
            if (!string.IsNullOrEmpty(AuthToken)) h["Authorization"] = "Bearer " + AuthToken;
            return h;
        }
    }

    public enum UploadOutcome
    {
        /// <summary>Server ACK received (accepted / duplicate / quarantined).</summary>
        Acknowledged = 0,
        /// <summary>Transient failure (timeout, 5xx, network down): retry later.</summary>
        Retryable = 1,
        /// <summary>Permanent failure (4xx, checksum conflict): quarantine.</summary>
        Permanent = 2,
        /// <summary>Response malformed/untrusted: do NOT delete; retry with backoff.</summary>
        InvalidResponse = 3,
        /// <summary>No endpoint configured / offline by policy.</summary>
        Offline = 4,
    }

    public sealed class UploadResult
    {
        public UploadOutcome Outcome;
        public int HttpStatus;
        /// <summary>Parsed ACK when Outcome == Acknowledged; null otherwise.</summary>
        public UploadAck Ack;
        public string Error;
        /// <summary>Server-requested retry delay (Retry-After), seconds. -1 = none.</summary>
        public double RetryAfterSeconds = -1;

        public static UploadResult Retryable(string err, int http = 0, double retryAfter = -1)
            => new UploadResult { Outcome = UploadOutcome.Retryable, Error = err, HttpStatus = http, RetryAfterSeconds = retryAfter };
        public static UploadResult Permanent(string err, int http = 0)
            => new UploadResult { Outcome = UploadOutcome.Permanent, Error = err, HttpStatus = http };
        public static UploadResult Invalid(string err, int http = 0)
            => new UploadResult { Outcome = UploadOutcome.InvalidResponse, Error = err, HttpStatus = http };
        public static UploadResult Offline(string err = "offline")
            => new UploadResult { Outcome = UploadOutcome.Offline, Error = err };
    }

    /// <summary>Server acknowledgement contract. Client deletes local data ONLY when
    /// Verify() passes — HTTP 200 alone is never sufficient.</summary>
    public sealed class UploadAck
    {
        public int Protocol;
        public string AckId;
        public string BatchId;
        public string Checksum;
        public string Fingerprint;
        public int AcceptedEvents;
        public string GroupId;
        public string ObjectRef;
        /// <summary>accepted | duplicate | quarantined</summary>
        public string Status;
        public string ValidatorVersion;

        public bool IsValid =>
            Protocol == TelemetryBatch.ProtocolVersion &&
            !string.IsNullOrEmpty(AckId) &&
            !string.IsNullOrEmpty(BatchId) &&
            !string.IsNullOrEmpty(Checksum) &&
            !string.IsNullOrEmpty(Fingerprint) &&
            !string.IsNullOrEmpty(Status) &&
            !string.IsNullOrEmpty(ObjectRef);

        /// <summary>Strict match against the batch that was uploaded.</summary>
        public bool Matches(TelemetryBatch batch)
            => IsValid
               && BatchId == batch.BatchId
               && Checksum == batch.PayloadSha256
               && Fingerprint == batch.DatasetFingerprint
               && AcceptedEvents == batch.EventCount
               && (Status == "accepted" || Status == "duplicate");

        /// <summary>ACK signals the server rejected the data; move local batch to quarantine.</summary>
        public bool IsQuarantine => IsValid && Status == "quarantined";

        public static UploadAck Parse(string json)
        {
            var v = TelemetryJsonReader.Parse(json);
            if (!v.IsObject) return null;
            return new UploadAck
            {
                Protocol = (int)v.GetInt("protocol"),
                AckId = v.GetString("ackId"),
                BatchId = v.GetString("batchId"),
                Checksum = v.GetString("checksum"),
                Fingerprint = v.GetString("fingerprint"),
                AcceptedEvents = (int)v.GetInt("acceptedEvents", -1),
                GroupId = v.GetString("groupId"),
                ObjectRef = v.GetString("objectRef"),
                Status = v.GetString("status"),
                ValidatorVersion = v.GetString("validatorVersion"),
            };
        }

        public string ToJson()
        {
            var w = new TelemetryJsonWriter();
            w.BeginObject();
            w.Field("protocol", Protocol);
            w.Field("ackId", AckId);
            w.Field("batchId", BatchId);
            w.Field("checksum", Checksum);
            w.Field("fingerprint", Fingerprint);
            w.Field("acceptedEvents", AcceptedEvents);
            w.Field("groupId", GroupId);
            w.Field("objectRef", ObjectRef);
            w.Field("status", Status);
            w.Field("validatorVersion", ValidatorVersion);
            w.EndObject();
            return w.ToString();
        }
    }

    /// <summary>Transport seam. Implementations: UnityWebRequest (Unity layer),
    /// HttpClient (tools/tests), Null (local-only), Loopback (in-process tests).</summary>
    public interface ITelemetryTransport
    {
        Task<UploadResult> UploadAsync(TelemetryUploadRequest request, CancellationToken cancellationToken);
    }
}
