using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Kruty1918.Telemetry.Upload;

namespace Kruty1918.Telemetry.Unity
{
    /// <summary>
    /// Production Unity transport on UnityWebRequest — correct proxy/cert/HTTP stack
    /// on every platform incl. Android/iOS where raw HttpClient is unreliable.
    /// </summary>
    public sealed class UnityWebRequestTransport : ITelemetryTransport
    {
        private readonly int _timeoutSeconds;

        public UnityWebRequestTransport(int timeoutSeconds = 30) { _timeoutSeconds = timeoutSeconds; }

        public async Task<UploadResult> UploadAsync(TelemetryUploadRequest request, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(request.Endpoint)) return UploadResult.Offline("no endpoint");
            var uwr = new UnityWebRequest(request.Endpoint, UnityWebRequest.kHttpVerbPOST);
            try
            {
                uwr.uploadHandler = new UploadHandlerRaw(request.Batch.Payload);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                uwr.timeout = _timeoutSeconds;
                uwr.SetRequestHeader("Content-Type", "application/octet-stream");
                foreach (var h in request.EnvelopeHeaders())
                    uwr.SetRequestHeader(h.Key, h.Value);

                var op = uwr.SendWebRequest();
                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested) { uwr.Abort(); ct.ThrowIfCancellationRequested(); }
                    await Task.Yield();
                }

                int status = (int)uwr.responseCode;
                if (uwr.result == UnityWebRequest.Result.ConnectionError
                    || uwr.result == UnityWebRequest.Result.DataProcessingError)
                    return UploadResult.Retryable("net: " + uwr.error);
                if (uwr.result == UnityWebRequest.Result.ProtocolError && status == 0)
                    return UploadResult.Retryable("protocol-no-status");

                string body = uwr.downloadHandler?.text ?? "";
                var result = LoopbackTransport.Classify(status, body);
                string retryAfter = uwr.GetResponseHeader("Retry-After");
                if (double.TryParse(retryAfter, out double ra)) result.RetryAfterSeconds = ra;
                return result;
            }
            finally { uwr.Dispose(); }
        }
    }

    /// <summary>Connectivity via Application.internetReachability.</summary>
    public sealed class UnityNetworkStatus : INetworkStatus
    {
        public bool IsOnline => Application.internetReachability != NetworkReachability.NotReachable;
    }

    /// <summary>Maps telemetry logs to Unity's logger without leaking payloads.</summary>
    public sealed class UnityTelemetryLogger : Kruty1918.Telemetry.Diagnostics.ITelemetryLogger
    {
        private const string Tag = "telemetry";
        public void Info(string msg) => Debug.unityLogger.Log(Tag, msg);
        public void Warn(string msg) => Debug.unityLogger.LogWarning(Tag, msg);
        public void Error(string msg) => Debug.unityLogger.LogError(Tag, msg);
    }
}
