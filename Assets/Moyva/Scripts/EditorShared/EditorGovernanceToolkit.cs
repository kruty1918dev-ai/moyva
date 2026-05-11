using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Kruty1918.Moyva.Editor.Shared
{
    public static class EditorRegistryWriteLock
    {
        private const string Prefix = "Moyva.Editor.WriteLock.";

        public static bool IsUnlocked(string lockKey)
        {
            if (string.IsNullOrWhiteSpace(lockKey))
                return true;

            return EditorPrefs.GetBool(Prefix + lockKey, false);
        }

        public static void SetUnlocked(string lockKey, bool unlocked)
        {
            if (string.IsNullOrWhiteSpace(lockKey))
                return;

            EditorPrefs.SetBool(Prefix + lockKey, unlocked);
        }
    }

    public sealed class EditorAssetStaleTracker
    {
        private DateTime _lastKnownWriteUtc;
        private string _lastPath = string.Empty;

        public void Capture(UnityEngine.Object asset)
        {
            _lastPath = AssetDatabase.GetAssetPath(asset) ?? string.Empty;
            _lastKnownWriteUtc = ResolveWriteUtc(_lastPath);
        }

        public bool IsStale(UnityEngine.Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (!string.Equals(path, _lastPath, StringComparison.OrdinalIgnoreCase))
            {
                _lastPath = path;
                _lastKnownWriteUtc = ResolveWriteUtc(path);
                return false;
            }

            var current = ResolveWriteUtc(path);
            return current > _lastKnownWriteUtc;
        }

        private static DateTime ResolveWriteUtc(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return DateTime.MinValue;

            var fullPath = Path.GetFullPath(assetPath);
            return File.Exists(fullPath) ? File.GetLastWriteTimeUtc(fullPath) : DateTime.MinValue;
        }
    }

    public static class EditorContentChangeLog
    {
        private const string LogPath = "Logs/MoyvaContentChanges.jsonl";

        public static void Write(string windowName, string operation, UnityEngine.Object targetAsset, IReadOnlyList<string> changedFields)
        {
            try
            {
                var root = Directory.GetCurrentDirectory();
                var full = Path.Combine(root, LogPath);
                var dir = Path.GetDirectoryName(full);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                var entry = new Entry
                {
                    timestampUtc = DateTime.UtcNow.ToString("o"),
                    user = Environment.UserName,
                    window = windowName ?? string.Empty,
                    operation = operation ?? string.Empty,
                    assetPath = AssetDatabase.GetAssetPath(targetAsset) ?? string.Empty,
                    changedFields = changedFields != null ? string.Join(" | ", changedFields) : string.Empty,
                };

                File.AppendAllText(full, JsonUtility.ToJson(entry) + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EditorContentChangeLog] Failed to write log: {ex.Message}");
            }
        }

        [Serializable]
        private struct Entry
        {
            public string timestampUtc;
            public string user;
            public string window;
            public string operation;
            public string assetPath;
            public string changedFields;
        }
    }

    public sealed class EditorWindowPerformanceProfiler
    {
        private readonly Stopwatch _frameStopwatch = new Stopwatch();
        private readonly Dictionary<string, double> _sectionDurationsMs = new Dictionary<string, double>();
        private readonly Dictionary<string, Stopwatch> _runningSections = new Dictionary<string, Stopwatch>();
        private double _frameAvgMs;
        private double _frameMaxMs;

        public void BeginFrame()
        {
            _sectionDurationsMs.Clear();
            _frameStopwatch.Restart();
        }

        public void EndFrame()
        {
            _frameStopwatch.Stop();
            var elapsed = _frameStopwatch.Elapsed.TotalMilliseconds;
            _frameAvgMs = _frameAvgMs <= 0d ? elapsed : (_frameAvgMs * 0.9d + elapsed * 0.1d);
            if (elapsed > _frameMaxMs)
                _frameMaxMs = elapsed;
        }

        public void BeginSection(string sectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName) || _runningSections.ContainsKey(sectionName))
                return;

            var sw = Stopwatch.StartNew();
            _runningSections[sectionName] = sw;
        }

        public void EndSection(string sectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
                return;

            if (!_runningSections.TryGetValue(sectionName, out var sw))
                return;

            sw.Stop();
            _runningSections.Remove(sectionName);
            _sectionDurationsMs[sectionName] = sw.Elapsed.TotalMilliseconds;
        }

        public string BuildSummary()
        {
            string heavySection = string.Empty;
            double heavyMs = 0d;
            foreach (var pair in _sectionDurationsMs)
            {
                if (pair.Value > heavyMs)
                {
                    heavyMs = pair.Value;
                    heavySection = pair.Key;
                }
            }

            var heavyPart = heavyMs > 0d ? $" | heavy: {heavySection} {heavyMs:0.00}ms" : string.Empty;
            return $"OnGUI avg {_frameAvgMs:0.00}ms, max {_frameMaxMs:0.00}ms{heavyPart}";
        }
    }

    public static class ProjectContextSelectorUI
    {
        public static T DrawSelectorRow<T>(string label, T currentAsset, Func<T> autoFind, float fieldWidth = 240f)
            where T : UnityEngine.Object
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(label, GUILayout.Width(62f));

            var next = (T)EditorGUILayout.ObjectField(currentAsset, typeof(T), false, GUILayout.Width(fieldWidth));

            if (GUILayout.Button("Auto", EditorStyles.toolbarButton, GUILayout.Width(44f)) && autoFind != null)
                next = autoFind();

            using (new EditorGUI.DisabledScope(next == null))
            {
                if (GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(44f)))
                    EditorGUIUtility.PingObject(next);
            }

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(48f)))
                next = null;

            EditorGUILayout.EndHorizontal();
            return next;
        }
    }
}
