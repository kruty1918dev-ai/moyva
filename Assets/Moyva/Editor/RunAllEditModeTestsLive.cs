#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

[InitializeOnLoad]
public static class RunAllEditModeTestsLive
{
    private const string Prefix = "[ALL_EDITMODE_TESTS]";
    private static readonly string ProjectRoot =
        Directory.GetParent(Application.dataPath).FullName;

    private static readonly string ControlDir =
        Path.Combine(ProjectRoot, ".all-editmode-tests");

    private static readonly string RequestFile =
        Path.Combine(ControlDir, "request");

    private static readonly string DoneFile =
        Path.Combine(ControlDir, "done");

    private static readonly string SummaryFile =
        Path.Combine(ControlDir, "summary.txt");

    private static bool _started;
    private static TestRunnerApi _api;
    private static Callback _callback;

    static RunAllEditModeTestsLive()
    {
        EditorApplication.delayCall += TryStart;
    }

    private static void TryStart()
    {
        if (_started || !File.Exists(RequestFile))
            return;

        _started = true;

        try
        {
            File.Delete(RequestFile);
            if (File.Exists(DoneFile))
                File.Delete(DoneFile);

            Debug.Log($"{Prefix} ============================================================");
            Debug.Log($"{Prefix} RUN ALL EDITMODE TESTS");
            Debug.Log($"{Prefix} Project: {ProjectRoot}");
            Debug.Log($"{Prefix} Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Debug.Log($"{Prefix} ============================================================");

            _api = ScriptableObject.CreateInstance<TestRunnerApi>();
            _callback = new Callback();

            _api.RegisterCallbacks(_callback);

            var filter = new Filter
            {
                testMode = TestMode.EditMode
            };

            var settings = new ExecutionSettings(filter);
            _api.Execute(settings);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{Prefix} BOOTSTRAP FAILED\n{ex}");

            Directory.CreateDirectory(ControlDir);
            File.WriteAllText(
                SummaryFile,
                "BOOTSTRAP FAILED\n" + ex);

            File.WriteAllText(DoneFile, "1");
        }
    }

    private sealed class Callback : ICallbacks
    {
        private int _passed;
        private int _failed;
        private int _skipped;
        private int _other;
        private int _finished;

        public void RunStarted(ITestAdaptor testsToRun)
        {
            Debug.Log($"{Prefix} RUN STARTED");
            Debug.Log($"{Prefix} ROOT: {testsToRun?.FullName}");
        }

        public void TestStarted(ITestAdaptor test)
        {
            if (test == null || test.HasChildren)
                return;

            Debug.Log($"{Prefix} START | {test.FullName}");
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (result?.Test == null || result.Test.HasChildren)
                return;

            _finished++;

            string state = result.ResultState ?? "UNKNOWN";
            string upper = state.ToUpperInvariant();

            if (upper.Contains("PASS"))
                _passed++;
            else if (upper.Contains("FAIL") || upper.Contains("ERROR"))
                _failed++;
            else if (upper.Contains("SKIP") ||
                     upper.Contains("IGNORE") ||
                     upper.Contains("NOTRUN"))
                _skipped++;
            else
                _other++;

            Debug.Log(
                $"{Prefix} RESULT | {state,-12} | " +
                $"{result.Duration,8:F3}s | {result.Test.FullName}");

            if (!string.IsNullOrWhiteSpace(result.Message))
                Debug.Log($"{Prefix} MESSAGE | {result.Test.FullName}\n{result.Message}");

            if (!string.IsNullOrWhiteSpace(result.StackTrace))
                Debug.Log($"{Prefix} STACK | {result.Test.FullName}\n{result.StackTrace}");

            if (!string.IsNullOrWhiteSpace(result.Output))
                Debug.Log($"{Prefix} OUTPUT | {result.Test.FullName}\n{result.Output}");
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string summary =
                "============================================================\n" +
                "ALL EDITMODE TESTS FINISHED\n" +
                $"Finished test cases: {_finished}\n" +
                $"PASS:    {_passed}\n" +
                $"FAIL:    {_failed}\n" +
                $"SKIP:    {_skipped}\n" +
                $"OTHER:   {_other}\n" +
                $"Duration: {result?.Duration:F3}s\n" +
                $"Result:   {result?.ResultState}\n" +
                $"Finished: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                "============================================================";

            Debug.Log($"{Prefix} {summary.Replace("\n", "\n" + Prefix + " ")}");

            try
            {
                Directory.CreateDirectory(ControlDir);
                File.WriteAllText(SummaryFile, summary);
                File.WriteAllText(DoneFile, "1");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Prefix} Cannot write completion marker: {ex}");
            }
        }
    }
}
#endif
