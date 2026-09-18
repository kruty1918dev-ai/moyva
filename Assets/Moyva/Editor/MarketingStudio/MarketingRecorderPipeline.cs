using System;
using Kruty1918.Moyva.Marketing.Runtime;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.EditorTools
{
    /// <summary>
    /// Unity Recorder-backed video/image-sequence capture. Registered on
    /// <see cref="MarketingRuntimeBridge.Backend"/> while a play-mode run is
    /// active. Constant frame-rate + manual record mode → deterministic pacing
    /// independent of editor FPS.
    /// </summary>
    public sealed class MarketingRecorderPipeline : IMarketingVideoBackend
    {
        private RecorderController _controller;
        private RecorderControllerSettings _settings;
        private bool _recording;
        private string _lastError;

        public bool IsAvailable => true;
        public bool IsRecording => _controller != null && _controller.IsRecording();
        public string LastError => _lastError;

        public void BeginVideo(string outputFile, int width, int height, int frameRate, bool withAudio)
        {
            try
            {
                EnsureController(frameRate);
                string file = StripExt(outputFile);
                var movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
                movie.name = "MoyvaMarketingMovie";
                movie.Enabled = true;
                movie.EncoderSettings = new CoreEncoderSettings
                {
                    Codec = outputFile.EndsWith(".webm", StringComparison.OrdinalIgnoreCase)
                        ? CoreEncoderSettings.OutputCodec.WEBM
                        : CoreEncoderSettings.OutputCodec.MP4,
                    EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
                };
                movie.CaptureAudio = withAudio;
                movie.AudioInputSettings.PreserveAudio = withAudio;
                movie.ImageInputSettings = new CameraInputSettings
                {
                    Source = ImageSource.MainCamera,
                    OutputWidth = width,
                    OutputHeight = height,
                    CaptureUI = true,
                    FlipFinalOutput = false,
                };
                movie.OutputFile = file;
                _settings.AddRecorderSettings(movie);
                _controller.PrepareRecording();
                if (!_controller.StartRecording())
                    _lastError = "RecorderController.StartRecording() returned false.";
                _recording = _controller.IsRecording();
            }
            catch (Exception e)
            {
                _lastError = e.ToString();
                Debug.LogError($"[MarketingStudio] Recorder begin failed: {e}");
            }
        }

        public void EndVideo() => StopInternal();

        public void BeginImageSequence(string outputDir, int width, int height, int frameRate)
        {
            try
            {
                EnsureController(frameRate);
                var image = ScriptableObject.CreateInstance<ImageRecorderSettings>();
                image.name = "MoyvaMarketingSequence";
                image.Enabled = true;
                image.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
                image.CaptureAlpha = false;
                image.imageInputSettings = new CameraInputSettings
                {
                    Source = ImageSource.MainCamera,
                    OutputWidth = width,
                    OutputHeight = height,
                    CaptureUI = true,
                    FlipFinalOutput = false,
                };
                image.OutputFile = System.IO.Path.Combine(outputDir, "frame_<Frame>");
                _settings.AddRecorderSettings(image);
                _controller.PrepareRecording();
                if (!_controller.StartRecording())
                    _lastError = "RecorderController.StartRecording() returned false.";
                _recording = _controller.IsRecording();
            }
            catch (Exception e)
            {
                _lastError = e.ToString();
                Debug.LogError($"[MarketingStudio] Recorder sequence begin failed: {e}");
            }
        }

        public void EndImageSequence() => StopInternal();

        private void EnsureController(int frameRate)
        {
            StopInternal();
            _settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            _settings.SetRecordModeToManual();
            _settings.FrameRatePlayback = FrameRatePlayback.Constant;
            _settings.FrameRate = Mathf.Clamp(frameRate, 12, 120);
            _settings.CapFrameRate = true;
            _controller = new RecorderController(_settings);
        }

        private void StopInternal()
        {
            try
            {
                if (_controller != null && _controller.IsRecording())
                    _controller.StopRecording();
            }
            catch (Exception e)
            {
                _lastError = e.ToString();
            }
            _recording = false;
        }

        private static string StripExt(string path)
        {
            int dot = path.LastIndexOf('.');
            int sep = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
            return dot > sep ? path.Substring(0, dot) : path;
        }
    }
}
