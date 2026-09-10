#if UNITY_EDITOR
using System;
using System.IO;
using System.Threading.Tasks;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zenject;
[InitializeOnLoad]
public static class StartupBarrierSmoke
{
    const string Key = "Moyva.BarrierSmoke";
    static double began;
    static int errors;
    static bool launched, stopping, sawInputBlock;
    static Task transition;
    static StartupBarrierSmoke()
    {
        EditorApplication.update += Update;
        EditorApplication.playModeStateChanged += state => {
            if (state == PlayModeStateChange.EnteredPlayMode) began = EditorApplication.timeSinceStartup;
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetString(Key, "") != "") {
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SessionState.GetString(Key + ".previous", ""));
                SessionState.EraseString(Key);
            }
        };
        Application.logMessageReceived += (condition, stack, type) => {
            if (SessionState.GetString(Key, "") != "" && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) {
                errors++; File.AppendAllText("Temp/ai/barrier-smoke-errors.log", condition + "\n" + stack + "\n");
            }
        };
    }
    static void Update()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || stopping) return;
        string mode = SessionState.GetString(Key, "");
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (mode != "" || !File.Exists("Temp/ai/barrier-smoke.request")) return;
            mode = File.ReadAllText("Temp/ai/barrier-smoke.request").Trim(); File.Delete("Temp/ai/barrier-smoke.request");
            SessionState.SetString(Key, mode);
            SessionState.SetString(Key + ".previous", AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(mode == "direct" ? "Assets/Moyva/Scenes/Gamplay_Scene.unity" : "Assets/Moyva/Scenes/HomeMenu.unity");
            GameLaunchContext.ConfigureDirectGameplayTest(); EditorApplication.isPlaying = true; return;
        }
        if (!EditorApplication.isPlaying || mode == "" || began == 0) return;
        try
        {
            if (EditorApplication.timeSinceStartup - began > 160) { Finish(mode, "FAIL timeout"); return; }
            foreach (var context in UnityEngine.Object.FindObjectsByType<SceneContext>(FindObjectsSortMode.None))
            {
                if (mode == "host" && !launched && context.gameObject.scene.name == "HomeMenu")
                {
                    var starter = context.Container.TryResolve<IHomeMenuGameStarter>(); if (starter == null) continue;
                    launched = true;
                    GameLaunchContext.ConfigureMenuMultiplayerGame("Barrier smoke", 1974457151, 128, 0, 0, 2, false, 128, 128, true, "smoke-host");
                    transition = starter.StartGameAsync();
                }
                if (context.gameObject.scene.name != "Gamplay_Scene") continue;
                var barrier = context.Container.TryResolve<IMultiplayerStartupBarrier>();
                var input = context.Container.TryResolve<IGameplayInputPolicy>();
                if (mode == "host" && barrier != null && !barrier.IsReadyToPlay)
                {
                    if (input != null && !input.CanProcess(GameplayInputKind.KeyboardNavigation, Vector2.zero)) sawInputBlock = true;
                    else if (Time.frameCount > 5) throw new Exception("Input was not blocked while waiting for startup.");
                }
                var world = context.Container.TryResolve<IWorldGenerationSignalState>();
                if (world == null || !world.TryGetWorldGeneratedData(out var data) || !world.TryGetWorldSpawnPositions(out var spawns)) continue;
                var stateType = typeof(StartingPositionInitializerSettings).Assembly.GetType("Kruty1918.Moyva.Bootstrap.Runtime.IStartingPositionWorkflowState");
                var workflow = context.Container.Resolve(stateType);
                if (!(bool)stateType.GetProperty("StartRevealApplied").GetValue(workflow)) continue;
                if (transition != null && !transition.IsCompleted) continue;
                if (transition != null && transition.IsFaulted) throw transition.Exception;
                if (mode == "host" && (!barrier.IsHostReady || !barrier.IsReadyToPlay || !sawInputBlock)) throw new Exception("Host readiness/input gate was not applied.");
                if (mode == "host" && !input.CanProcess(GameplayInputKind.KeyboardNavigation, Vector2.zero)) continue;
                Finish(mode, $"{(errors == 0 ? "PASS" : "FAIL")} world={data.Width}x{data.Height}, assignments={spawns.Assignments.Length}, hostReady={barrier?.IsHostReady}, inputBlockedDuringLoad={sawInputBlock}, errors={errors}"); return;
            }
        }
        catch (Exception e) { Finish(mode, "FAIL " + e); }
    }
    static void Finish(string mode, string result)
    {
        stopping = true; File.WriteAllText("Temp/ai/barrier-smoke-" + mode + ".summary", result); EditorApplication.isPlaying = false;
    }
}
#endif