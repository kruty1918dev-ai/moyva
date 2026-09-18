using Newtonsoft.Json;
using System;
using Kruty1918.Moyva.Marketing.Contracts;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// Hand-off channel between the editor Studio window / CLI and the
    /// MarketingStudio scene. The editor serializes a request before entering
    /// play mode; the runtime controller consumes it on start. Written to disk
    /// so it survives domain reload.
    /// </summary>
    public static class MarketingStudioSession
    {
        [Serializable]
        public sealed class RunRequest
        {
            public string recipeId = "steam-screenshots";
            public string recipeJson = string.Empty;   // inline recipe override
            public string platformOverride = string.Empty;
            public string outputRoot = string.Empty;
            public string language = string.Empty;
            public bool autoStart = true;
            public bool exitPlayModeOnFinish = true;   // editor-initiated runs
            public bool batch;                          // CLI/batch run
            public string statusFile = string.Empty;   // runtime progress report
        }

        public static string RequestFilePath =>
            System.IO.Path.Combine(Output.MarketingOutputLayout.ProjectRoot(),
                "MarketingOutput", ".cache", "run-request.json");

        public static void WriteRequest(RunRequest request)
        {
            var dir = System.IO.Path.GetDirectoryName(RequestFilePath);
            System.IO.Directory.CreateDirectory(dir);
            System.IO.File.WriteAllText(RequestFilePath,
                JsonConvert.SerializeObject(request, Formatting.Indented));
        }

        public static RunRequest ReadRequest()
        {
            try
            {
                if (!System.IO.File.Exists(RequestFilePath)) return null;
                return JsonConvert.DeserializeObject<RunRequest>(
                    System.IO.File.ReadAllText(RequestFilePath));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void ClearRequest()
        {
            try { if (System.IO.File.Exists(RequestFilePath)) System.IO.File.Delete(RequestFilePath); }
            catch (Exception) { /* best effort */ }
        }
    }
}
