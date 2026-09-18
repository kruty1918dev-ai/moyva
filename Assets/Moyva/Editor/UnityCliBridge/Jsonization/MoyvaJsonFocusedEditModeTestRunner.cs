using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    /// <summary>
    /// Малий editor-only міст для focused EditMode tests у вже відкритому Unity Editor.
    /// </summary>
    public static class MoyvaJsonFocusedEditModeTestRunner
    {
        private static TestRunnerApi _api;
        private static Callback _callback;
        private static bool _running;

        public static string Run(string reportPath, string[] assemblyNames)
        {
            reportPath = NormalizeReportPath(reportPath);
            if (_running)
                return "MOYVA_FOCUSED_EDITMODE_TESTS running";

            if (File.Exists(reportPath))
                File.Delete(reportPath);

            _api = ScriptableObject.CreateInstance<TestRunnerApi>();
            _callback = new Callback(reportPath);
            _api.RegisterCallbacks(_callback);
            _running = true;

            var filter = new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = assemblyNames ?? Array.Empty<string>(),
            };
            string runId = _api.Execute(new ExecutionSettings(filter));
            return "MOYVA_FOCUSED_EDITMODE_TESTS started runId=" + runId;
        }

        public static string Status(string reportPath)
        {
            reportPath = NormalizeReportPath(reportPath);
            if (File.Exists(reportPath))
                return File.ReadAllText(reportPath);

            return _running
                ? "MOYVA_FOCUSED_EDITMODE_TESTS running"
                : "MOYVA_FOCUSED_EDITMODE_TESTS idle";
        }

        private static void Finish(Callback callback)
        {
            if (_api != null && callback != null)
                _api.UnregisterCallbacks(callback);

            if (_api != null)
                UnityEngine.Object.DestroyImmediate(_api);

            _api = null;
            _callback = null;
            _running = false;
        }

        private static string NormalizeReportPath(string reportPath)
        {
            if (string.IsNullOrWhiteSpace(reportPath))
                reportPath = "Temp/moyva-focused-editmode-tests.json";

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string path = reportPath.Replace('\\', '/');
            return Path.IsPathRooted(path)
                ? path
                : Path.Combine(projectRoot, path);
        }

        [Serializable]
        private sealed class Report
        {
            public string state;
            public int passed;
            public int failed;
            public int skipped;
            public int inconclusive;
            public double durationSeconds;
            public List<string> failures = new();
        }

        private sealed class Callback : ICallbacks
        {
            private readonly string _reportPath;
            private readonly List<string> _failures = new();

            public Callback(string reportPath)
            {
                _reportPath = reportPath;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result?.Test == null || result.Test.HasChildren)
                    return;

                string state = result.ResultState ?? string.Empty;
                if (!state.Contains("Fail", StringComparison.OrdinalIgnoreCase) &&
                    !state.Contains("Error", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                _failures.Add(
                    $"{result.FullName}: {state}\n{result.Message}\n{result.StackTrace}");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                var report = new Report
                {
                    state = result?.ResultState ?? "null-result",
                    passed = result?.PassCount ?? 0,
                    failed = result?.FailCount ?? 1,
                    skipped = result?.SkipCount ?? 0,
                    inconclusive = result?.InconclusiveCount ?? 0,
                    durationSeconds = result?.Duration ?? 0d,
                    failures = _failures,
                };

                Directory.CreateDirectory(Path.GetDirectoryName(_reportPath));
                File.WriteAllText(
                    _reportPath,
                    JsonConvert.SerializeObject(report, Formatting.Indented) + "\n");
                Finish(this);
            }
        }
    }
}
