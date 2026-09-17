using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kruty1918.Moyva.AI.Bot;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.AI.Training.Editor
{
    // Local filesystem IPC only. There is deliberately no eval, shell, arbitrary scene or arbitrary method command.
    [InitializeOnLoad]
    public static class MoyvaControlBridge
    {
        private const string PendingKey = "Moyva.ControlBridge.Pending";
        private const string GameplayScene = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
        private static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        private static readonly string DirectoryPath = Path.Combine(Root, ".moyva-local", "bridge");
        private static double _heartbeat;
        private static Request _pending;
        private static TestRunnerApi _runner;

        [Serializable] private sealed class Request
        {
            public string id, project, command, argument, target, submittedUtc;
        }
        [Serializable] private sealed class Response
        {
            public string id, state, message, report, contract;
            public int passed, failed;
        }
        [Serializable] private sealed class EditorState
        {
            public string project, state, scene, contract, unityVersion, updatedUtc;
            public int pid;
            public bool playing, compiling, dirty;
        }

        static MoyvaControlBridge()
        {
            Directory.CreateDirectory(Path.Combine(DirectoryPath, "requests"));
            Directory.CreateDirectory(Path.Combine(DirectoryPath, "responses"));
            string pending = SessionState.GetString(PendingKey, "");
            if (!string.IsNullOrEmpty(pending))
            {
                _pending = JsonUtility.FromJson<Request>(pending);
                if (_pending.command == "tests") RegisterTests();
            }
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        private static bool Dirty()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
                if (SceneManager.GetSceneAt(index).isDirty) return true;
            return false;
        }

        private static void Write(string path, object value)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(value, true));
            try
            {
                if (File.Exists(path)) File.Replace(temp, path, null);
                else File.Move(temp, path);
            }
            catch (IOException)
            {
                // A file watcher may hold the destination transiently; heartbeat
                // files are best-effort, so fall back to an in-place overwrite
                // and otherwise let the next beat retry.
                try { File.Copy(temp, path, true); File.Delete(temp); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            catch (UnauthorizedAccessException)
            {
                try { File.Delete(temp); } catch (IOException) { }
            }
        }

        private static void Update()
        {
            if (EditorApplication.timeSinceStartup >= _heartbeat)
            {
                _heartbeat = EditorApplication.timeSinceStartup + 2;
                Write(Path.Combine(DirectoryPath, "editor.json"), new EditorState
                {
                    project = Root, state = _pending != null ? "BUSY" : "READY", pid = Process.GetCurrentProcess().Id,
                    playing = EditorApplication.isPlaying, compiling = EditorApplication.isCompiling, dirty = Dirty(),
                    scene = SceneManager.GetActiveScene().path, contract = BotDecisionContract.Hash,
                    unityVersion = Application.unityVersion, updatedUtc = DateTime.UtcNow.ToString("O")
                });
            }
            if (_pending != null || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            string path = Directory.GetFiles(Path.Combine(DirectoryPath, "requests"), "*.json").OrderBy(p => p, StringComparer.Ordinal).FirstOrDefault();
            if (path == null) return;
            Request request = null;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(path));
                File.Delete(path);
                if (request == null || !Regex.IsMatch(request.id ?? "", "^[a-f0-9]{32}$")) return;
                if (!string.Equals(Path.GetFullPath(request.project), Root, Application.platform == RuntimePlatform.WindowsEditor ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    throw new InvalidOperationException("Request belongs to another project.");
                if (!DateTime.TryParse(request.submittedUtc, out var submitted) || DateTime.UtcNow - submitted.ToUniversalTime() > TimeSpan.FromMinutes(10))
                    throw new InvalidOperationException("Request expired; submit it again explicitly.");
                _pending = request;
                SessionState.SetString(PendingKey, JsonUtility.ToJson(request));
                Respond("RUNNING", "Unity accepted the request.", complete: false);
                Execute(request);
            }
            catch (Exception error)
            {
                if (request != null && Regex.IsMatch(request.id ?? "", "^[a-f0-9]{32}$"))
                {
                    _pending = request;
                    Respond("FAILED", error.Message);
                }
                else if (File.Exists(path)) File.Move(path, path + ".rejected");
            }
        }

        private static void Execute(Request request)
        {
            if (request.command != "status" && request.command != "stop" && request.command != "monitor"
                && request.command != "control-center" && request.command != "save-scenes" && request.command != "refresh" && Dirty())
                throw new InvalidOperationException("Unsaved scene changes: save them explicitly with 'moyva unity save-scenes' before this operation.");
            switch (request.command)
            {
                case "status": Respond("COMPLETED", "Editor connected."); break;
                case "refresh": AssetDatabase.Refresh(); Respond("COMPLETED", "Asset refresh requested; consult editor compiling state."); break;
                case "save-scenes":
                    if (!EditorSceneManager.SaveOpenScenes()) throw new InvalidOperationException("Scene save was cancelled.");
                    Respond("COMPLETED", "Open scenes saved."); break;
                case "training-scene": OpenScene(TrainingPlayerBuilder.Scene); break;
                case "gameplay-scene": OpenScene(GameplayScene); break;
                case "play":
                    if (EditorApplication.isPlaying) Respond("COMPLETED", "Already playing.");
                    else EditorApplication.EnterPlaymode();
                    break;
                case "stop":
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Respond("COMPLETED", "Already stopped.");
                    else EditorApplication.ExitPlaymode();
                    break;
                case "monitor": TrainingMonitorWindow.Open(); Respond("COMPLETED", "Training monitor opened."); break;
                case "control-center": MoyvaTrainingControlCenter.Open(); Respond("COMPLETED", "Control center opened."); break;
                case "build-training":
                    RequireEditMode();
                    string output = Path.GetFullPath(request.argument);
                    string allowed = Path.Combine(Root, "Build", "Training") + Path.DirectorySeparatorChar;
                    if (!output.StartsWith(allowed, Application.platform == RuntimePlatform.WindowsEditor ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                        throw new InvalidOperationException("Editor builds are restricted to Build/Training.");
                    if (request.target != "StandaloneLinux64" && request.target != "StandaloneWindows64" && request.target != "StandaloneOSX")
                        throw new InvalidOperationException("Unsupported training build target.");
                    TrainingPlayerBuilder.BuildPlayer((BuildTarget)Enum.Parse(typeof(BuildTarget), request.target), output);
                    Respond("COMPLETED", "Training player built.", output + ".contract.json"); break;
                case "readiness":
                    RequireEditMode(); TrainingPlayerBuilder.ValidateFullGameScope();
                    Respond("COMPLETED", "READY_FOR_REAL_TRAINING: FullGame scope validation passed."); break;
                case "tests":
                    RequireEditMode();
                    if (!new[] { "editmode", "playmode", "ai", "fullgame" }.Contains(request.argument))
                        throw new InvalidOperationException("Unknown test suite.");
                    RegisterTests();
                    var filter = new Filter { testMode = request.argument == "playmode" ? TestMode.PlayMode : TestMode.EditMode };
                    if (request.argument == "ai" || request.argument == "fullgame") filter.assemblyNames = new[] { "Kruty1918.Moyva.AI.Training.Tests" };
                    if (request.argument == "fullgame") filter.testNames = new[] { "Kruty1918.Moyva.AI.Training.Tests.FullGameIntegrationTests" };
                    _runner.Execute(new ExecutionSettings(filter)); break;
                default: throw new InvalidOperationException("Command is not allow-listed.");
            }
        }

        private static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        }
        private static void OpenScene(string scene)
        {
            RequireEditMode();
            if (!File.Exists(scene)) throw new FileNotFoundException("Configured project scene is missing", scene);
            EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);
            Respond("COMPLETED", "Scene opened: " + scene);
        }
        private static void OnPlayMode(PlayModeStateChange change)
        {
            if (_pending?.command == "play" && change == PlayModeStateChange.EnteredPlayMode
                || _pending?.command == "stop" && change == PlayModeStateChange.EnteredEditMode)
                Respond("COMPLETED", change.ToString());
        }
        private static void Respond(string state, string message, string report = null, bool complete = true, int passed = 0, int failed = 0)
        {
            if (_pending == null) return;
            Write(Path.Combine(DirectoryPath, "responses", _pending.id + ".json"), new Response
            { id = _pending.id, state = state, message = message, report = report, passed = passed, failed = failed, contract = BotDecisionContract.Hash });
            if (complete) { _pending = null; SessionState.EraseString(PendingKey); }
            _heartbeat = 0;
        }
        private static void RegisterTests()
        {
            if (_runner != null) return;
            _runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            _runner.RegisterCallbacks(new TestResults());
        }
        private sealed class TestResults : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                if (_pending?.command != "tests") return;
                string report = Path.Combine(Root, ".moyva-local", "validation", _pending.id + ".xml");
                Directory.CreateDirectory(Path.GetDirectoryName(report));
                TestRunnerApi.SaveResultToFile(result, report);
                bool passed = result.FailCount == 0 && result.PassCount > 0;
                Respond(passed ? "COMPLETED" : "FAILED", $"Tests: {result.PassCount} passed, {result.FailCount} failed. {result.Message}", report,
                    passed: result.PassCount, failed: result.FailCount);
                if (_runner != null) UnityEngine.Object.DestroyImmediate(_runner);
                _runner = null;
            }
        }
    }
}
