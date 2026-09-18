using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// Video/image-sequence capture backend. The editor layer registers a
    /// Unity Recorder implementation while play mode runs; if none is
    /// registered, the runtime falls back to image sequences (always works).
    /// </summary>
    public interface IMarketingVideoBackend
    {
        bool IsAvailable { get; }
        void BeginVideo(string outputFile, int width, int height, int frameRate, bool withAudio);
        void EndVideo();
        void BeginImageSequence(string outputDir, int width, int height, int frameRate);
        void EndImageSequence();
    }

    /// <summary>Static hook: editor assigns Backend after domain reload.</summary>
    public static class MarketingRuntimeBridge
    {
        public static IMarketingVideoBackend Backend;
    }

    /// <summary>Progress report the runtime writes for the editor monitor.</summary>
    [Serializable]
    public sealed class MarketingRunStatus
    {
        public string phase = "boot";
        public float progress01;
        public string message = string.Empty;
        public string runFolder = string.Empty;
        public string manifestPath = string.Empty;
        public bool done;
        public bool cancelled;
        public string error = string.Empty;

        public string StatusFilePath { get; set; }

        public void Write()
        {
            if (string.IsNullOrEmpty(StatusFilePath)) return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(StatusFilePath));
                File.WriteAllText(StatusFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MarketingStudio] Status write failed: {e.Message}");
            }
        }

        public static MarketingRunStatus Read(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                return JsonConvert.DeserializeObject<MarketingRunStatus>(File.ReadAllText(path));
            }
            catch { return null; }
        }
    }
}
