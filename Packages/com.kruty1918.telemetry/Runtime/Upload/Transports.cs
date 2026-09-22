using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Telemetry.Upload
{
    /// <summary>Local-only production mode: everything stays in the spool.</summary>
    public sealed class NullTelemetryTransport : ITelemetryTransport
    {
        public Task<UploadResult> UploadAsync(TelemetryUploadRequest request, CancellationToken ct)
            => Task.FromResult(UploadResult.Offline("no transport"));
    }

    /// <summary>
    /// In-process transport for tests and the local backend harness: a delegate
    /// receives (envelope headers, payload bytes) and returns (http status, body).
    /// Lets tests exercise the real coordinator/ACK path without sockets.
    /// </summary>
    public sealed class LoopbackTransport : ITelemetryTransport
    {
        public Func<Dictionary<string, string>, byte[], CancellationToken, Task<(int status, string body)>> Handler;
        public int Requests;
        public readonly List<Dictionary<string, string>> SeenHeaders = new List<Dictionary<string, string>>();

        public async Task<UploadResult> UploadAsync(TelemetryUploadRequest request, CancellationToken ct)
        {
            Requests++;
            var headers = request.EnvelopeHeaders();
            SeenHeaders.Add(headers);
            if (Handler == null) return UploadResult.Offline();
            var (status, body) = await Handler(headers, request.Batch.Payload, ct).ConfigureAwait(false);
            return Classify(status, body);
        }

        internal static UploadResult Classify(int status, string body)
        {
            if (status >= 200 && status < 300)
            {
                UploadAck ack = null;
                try { ack = UploadAck.Parse(body); }
                catch { return UploadResult.Invalid("ack unparseable", status); }
                if (ack == null || !ack.IsValid) return UploadResult.Invalid("ack invalid", status);
                return new UploadResult { Outcome = UploadOutcome.Acknowledged, Ack = ack, HttpStatus = status };
            }
            if (status == 429 || status >= 500)
                return UploadResult.Retryable("http " + status, status);
            if (RetryPolicy.IsPermanentStatus(status))
                return UploadResult.Permanent("http " + status, status);
            return UploadResult.Retryable("http " + status, status);
        }
    }

    /// <summary>
    /// Reference HTTPS transport on HttpClient — used by tools and non-Unity hosts.
    /// Unity player builds should prefer the UnityWebRequest transport in the
    /// Kruty1918.Telemetry.Unity assembly (better mobile proxy/cert handling).
    /// </summary>
    public sealed class HttpTelemetryTransport : ITelemetryTransport
    {
        private readonly HttpClient _client;
        private readonly TimeSpan _timeout;

        public HttpTelemetryTransport(HttpClient client = null, TimeSpan? timeout = null)
        {
            _client = client ?? new HttpClient();
            _timeout = timeout ?? TimeSpan.FromSeconds(30);
        }

        public async Task<UploadResult> UploadAsync(TelemetryUploadRequest request, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(request.Endpoint)) return UploadResult.Offline("no endpoint");
            try
            {
                using (var msg = new HttpRequestMessage(HttpMethod.Post, request.Endpoint))
                {
                    foreach (var h in request.EnvelopeHeaders())
                        msg.Headers.TryAddWithoutValidation(h.Key, h.Value);
                    msg.Content = new ByteArrayContent(request.Batch.Payload);
                    msg.Content.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
                    using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                    {
                        cts.CancelAfter(_timeout);
                        var resp = await _client.SendAsync(msg, cts.Token).ConfigureAwait(false);
                        string body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var result = LoopbackTransport.Classify((int)resp.StatusCode, body);
                        if (resp.Headers.RetryAfter?.Delta is TimeSpan ra)
                            result.RetryAfterSeconds = ra.TotalSeconds;
                        return result;
                    }
                }
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                return UploadResult.Retryable("timeout");
            }
            catch (HttpRequestException e)
            {
                return UploadResult.Retryable("network: " + e.Message);
            }
        }
    }
}
