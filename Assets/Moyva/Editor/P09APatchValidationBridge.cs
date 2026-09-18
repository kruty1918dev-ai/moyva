#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Kruty1918.Moyva.EditorTools
{
    /// <summary>
    /// One-shot file-system bridge used by the P09A shell installer when this
    /// project is already open in Unity. It never enters Play Mode. It can only
    /// request leaving Play Mode, wait for compilation/import to settle, and run
    /// EditMode tests through Unity's official TestRunnerApi.
    /// </summary>
    [InitializeOnLoad]
    public static class P09APatchValidationBridge
    {
        private const string ControlDirectoryName = ".moyva-p09a-control";
        private const string RequestFileName = "request-validation";
        private const string ReadyFileName = "editor-ready";
        private const string TestResultFileName = "unity-tests-result";

        private static TestRunnerApi _runner;
        private static ValidationCallbacks _callbacks;
        private static bool _testRunStarted;

        static P09APatchValidationBridge()
        {
            EditorApplication.delayCall += ProcessRequest;
        }

        [MenuItem("Moyva/P09A/Process Patch Validation Request")]
        public static void ProcessRequest()
        {
            string root = ResolveRoot();
            if (string.IsNullOrWhiteSpace(root))
                return;

            string request = Path.Combine(root, ControlDirectoryName, RequestFileName);
            if (!File.Exists(request))
                return;

            Debug.Log(
                "[MOYVA_P09A][EDITOR] request detected; " +
                $"playing={EditorApplication.isPlaying}; " +
                $"changing={EditorApplication.isPlayingOrWillChangePlaymode}; " +
                $"compiling={EditorApplication.isCompiling}; " +
                $"updating={EditorApplication.isUpdating}");

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.Log("[MOYVA_P09A][EDITOR] Leaving Play Mode before validation.");
                // This bridge only stops Play Mode. There is intentionally no
                // code path that can assign true to EditorApplication.isPlaying.
                EditorApplication.isPlaying = false;
            }

            EditorApplication.update -= WaitForEditorReady;
            EditorApplication.update += WaitForEditorReady;
        }

        /// <summary>
        /// Batch-mode compile gate used when no interactive editor owns the
        /// project lock. Reaching this method proves scripts compiled enough for
        /// the editor assembly to load.
        /// </summary>
        public static void CommandLineCompileGate()
        {
            string root = ResolveRoot();
            if (string.IsNullOrWhiteSpace(root))
                throw new InvalidOperationException("Unable to resolve Unity project root.");

            Directory.CreateDirectory(Path.Combine(root, ControlDirectoryName));

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.isPlaying = false;

            if (EditorApplication.isCompiling)
                throw new InvalidOperationException("Unity is still compiling when P09A compile gate executes.");

            WriteMarker(root, ReadyFileName, "batchmode-compile-gate");
            Debug.Log("[MOYVA_P09A][EDITOR] Batchmode compile gate PASS.");
        }

        private static void WaitForEditorReady()
        {
            if (EditorApplication.isPlaying
                || EditorApplication.isPlayingOrWillChangePlaymode
                || EditorApplication.isCompiling
                || EditorApplication.isUpdating)
            {
                return;
            }

            EditorApplication.update -= WaitForEditorReady;

            string root = ResolveRoot();
            if (string.IsNullOrWhiteSpace(root))
                return;

            WriteMarker(root, ReadyFileName, "existing-editor-ready");
            Debug.Log("[MOYVA_P09A][EDITOR] Play Mode is off and compilation/import is idle.");
            StartEditModeTests(root);
        }

        private static void StartEditModeTests(string root)
        {
            if (_testRunStarted)
                return;

            _testRunStarted = true;
            DeleteMarker(root, TestResultFileName);

            _runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            _callbacks = new ValidationCallbacks(root);
            _runner.RegisterCallbacks(_callbacks);

            var filter = new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[]
                {
                    "Kruty1918.Moyva.Tests.Units",
                },
            };

            var settings = new ExecutionSettings(filter);
            Debug.Log("[MOYVA_P09A][EDITOR] Starting focused EditMode tests.");
            string runId = _runner.Execute(settings);
            Debug.Log($"[MOYVA_P09A][EDITOR] TestRunner runId={runId}");
        }

        private sealed class ValidationCallbacks : ICallbacks
        {
            private readonly string _root;

            public ValidationCallbacks(string root)
            {
                _root = root;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
                Debug.Log(
                    $"[MOYVA_P09A][UNITY_TEST] RUN START name={testsToRun?.FullName} " +
                    $"cases={testsToRun?.TestCaseCount}");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                bool pass = result != null && result.FailCount == 0;
                string state = result?.ResultState ?? "null-result";
                int passed = result?.PassCount ?? 0;
                int failed = result?.FailCount ?? 1;
                int skipped = result?.SkipCount ?? 0;
                int inconclusive = result?.InconclusiveCount ?? 0;

                string summary =
                    $"{(pass ? "PASS" : "FAIL")} state={state} " +
                    $"passed={passed} failed={failed} skipped={skipped} " +
                    $"inconclusive={inconclusive}";

                Debug.Log($"[MOYVA_P09A][UNITY_TEST] RUN FINISH {summary}");
                WriteMarker(_root, TestResultFileName, summary);

                if (_runner != null && _callbacks != null)
                    _runner.UnregisterCallbacks(_callbacks);

                _callbacks = null;
                _runner = null;
            }

            public void TestStarted(ITestAdaptor test)
            {
                Debug.Log($"[MOYVA_P09A][UNITY_TEST] START {test?.FullName}");
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result == null)
                {
                    Debug.LogError("[MOYVA_P09A][UNITY_TEST] FINISH null-result");
                    return;
                }

                string line =
                    $"[MOYVA_P09A][UNITY_TEST] FINISH {result.FullName} " +
                    $"state={result.ResultState} duration={result.Duration:F4}s";

                if (result.FailCount > 0 || string.Equals(result.ResultState, "Failed", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError(
                        line + Environment.NewLine +
                        (result.Message ?? string.Empty) + Environment.NewLine +
                        (result.StackTrace ?? string.Empty));
                }
                else
                {
                    Debug.Log(line);
                }
            }
        }

        private static string ResolveRoot()
            => Directory.GetParent(Application.dataPath)?.FullName;

        private static void WriteMarker(string root, string fileName, string value)
        {
            string control = Path.Combine(root, ControlDirectoryName);
            Directory.CreateDirectory(control);
            File.WriteAllText(
                Path.Combine(control, fileName),
                DateTime.UtcNow.ToString("O") + Environment.NewLine + value + Environment.NewLine);
        }

        private static void DeleteMarker(string root, string fileName)
        {
            string path = Path.Combine(root, ControlDirectoryName, fileName);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
#endif
