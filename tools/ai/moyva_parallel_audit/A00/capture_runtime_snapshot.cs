// capture_runtime_snapshot.cs — runtime probe for the parallel audit bench.
// Run inside the serving Editor for THIS worktree:
//   tools/unity-cli/moyva-unity eval-file tools/ai/moyva_parallel_audit/A00/capture_runtime_snapshot.cs
// Writes Temp/ai/moyva-parallel/A00/runtime-snapshot.json (fields merge into the
// run header via collect_run_header.py --runtime-json).
// Set env MOYVA_AUDIT_TRACE=1 to also arm a SignalBus JSONL trace at
// Temp/ai/moyva-parallel/A00/event-trace.jsonl for the rest of the play session.
// NOTE: read-only by contract — resolves services and fires NO commands/signals.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Zenject;
using Kruty1918.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GameMode.API;

var outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "ai", "moyva-parallel", "A00"));
Directory.CreateDirectory(outDir);

string Escape(string s) => s == null ? null : s.Replace("\\", "\\\\").Replace("\"", "\\\"");
string J(object v) => v == null ? "null"
    : v is bool b ? (b ? "true" : "false")
    : v is string s2 ? "\"" + Escape(s2) + "\""
    : v is float f ? f.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
    : v is double d ? d.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
    : v.ToString();

var containers = new List<DiContainer>();
foreach (var sc in UnityEngine.Object.FindObjectsByType<SceneContext>(FindObjectsSortMode.None))
    if (sc != null && sc.Container != null) containers.Add(sc.Container);
try { if (ProjectContext.Instance != null) containers.Add(ProjectContext.Instance.Container); } catch { }

T Resolve<T>() where T : class {
    foreach (var c in containers) { try { var v = c.TryResolve<T>(); if (v != null) return v; } catch { } }
    return null;
}

// --- game state / pause ownership -------------------------------------------------
var gameState = Resolve<IGameStateService>();
string gameStateName = gameState == null ? "unknown" : gameState.CurrentState.ToString();
bool? pauseOwns = null;
if (gameState != null) {
    var f = gameState.GetType().GetField("_ownsSimulationPause", BindingFlags.NonPublic | BindingFlags.Instance);
    if (f != null) pauseOwns = (bool)f.GetValue(gameState);
}

// --- network role -----------------------------------------------------------------
string netSession = "unknown";
try {
    var roleResolver = Resolve<Kruty1918.Moyva.Multiplayer.Core.ILocalGameplayRoleResolver>();
    if (roleResolver != null)
        netSession = roleResolver.Resolve().Role.ToString().ToLowerInvariant();
} catch { }

// --- world identity (last published payload) ---------------------------------------
long? startupSequence = null; int? snapshotRevision = null; string startupSessionId = null;
int? worldW = null, worldH = null;
var worldState = Resolve<IWorldGenerationSignalState>();
if (worldState != null && worldState.TryGetWorldGeneratedData(out var wg)) {
    startupSequence = wg.StartupSequence; snapshotRevision = wg.SnapshotRevision;
    startupSessionId = wg.StartupSessionId; worldW = wg.Width; worldH = wg.Height;
}

// --- play mode ---------------------------------------------------------------------
bool playing = UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
bool editorPaused = UnityEditor.EditorApplication.isPaused;
string playMode = !playing ? "stopped" : (editorPaused ? "paused" : "playing");

int? globalSeed = null;
try { globalSeed = GlobalSeed.Current; } catch { }

float? fps = null, frameMs = null;
if (playing) {
    fps = 1f / Mathf.Max(Time.smoothDeltaTime, 1e-5f);
    frameMs = Time.deltaTime * 1000f;
}

var fields = new List<string> {
    "\"unityVersion\":" + J(Application.unityVersion),
    "\"playMode\":" + J(playMode),
    "\"gameState\":" + J(gameStateName == "GameOver" ? "gameOver" : gameStateName.ToLowerInvariant()),
    "\"launchMode\":" + J(GameLaunchContext.Mode.ToString()),
    "\"networkSession\":" + J(netSession),
    "\"globalSeedCurrent\":" + J(globalSeed),
    // GlobalSeed exposes no flag for InitializeDeterministic having run; emit null
    // rather than claiming determinism we cannot observe.
    "\"unityRandomInitialized\":" + "null",
    "\"playerCount\":" + J(GameLaunchContext.MaxPlayers > 0 ? (int?)GameLaunchContext.MaxPlayers : null),
    "\"timeScale\":" + J(playing ? (float?)Time.timeScale : null),
    "\"fixedDeltaTime\":" + J(playing ? (float?)Time.fixedDeltaTime : null),
    "\"maximumDeltaTime\":" + J(playing ? (float?)Time.maximumDeltaTime : null),
    "\"pauseOwnsTimeScale\":" + J(pauseOwns),
    "\"fps\":" + J(fps),
    "\"frameTimeMs\":" + J(frameMs),
    "\"frame\":" + J(playing ? (int?)Time.frameCount : null),
    "\"gameTime\":" + J(playing ? (float?)Time.time : null),
    "\"worldWidth\":" + J(worldW),
    "\"worldHeight\":" + J(worldH),
    "\"startupSequence\":" + J(startupSequence),
    "\"snapshotRevision\":" + J(snapshotRevision),
    "\"startupSessionId\":" + J(startupSessionId),
    "\"mapTypeRaw\":" + J(GameLaunchContext.HasWorldSettings ? (int?)GameLaunchContext.MapType : null),
};

var snapPath = Path.Combine(outDir, "runtime-snapshot.json");
File.WriteAllText(snapPath, "{\n  " + string.Join(",\n  ", fields) + "\n}\n");

// --- optional: arm live signal trace ------------------------------------------------
string traceInfo = "trace=off";
var tracePath = Path.Combine(outDir, "event-trace.jsonl");
var armedFlag = Path.Combine(outDir, "event-trace.armed");
if (Environment.GetEnvironmentVariable("MOYVA_AUDIT_TRACE") == "1" && !File.Exists(armedFlag)) {
    var bus = Resolve<SignalBus>();
    if (bus != null) {
        Action<string, object> emit = (name, sig) => {
            var sb = new StringBuilder();
            foreach (var fi in sig.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                sb.Append(sb.Length == 0 ? "\"" : ",\"").Append(fi.Name).Append("\":")
                  .Append(J(fi.GetValue(sig)));
            var row = "{\"runId\":\"A00\",\"sequence\":-1,\"frame\":" +
                      (Time.frameCount.ToString()) + ",\"gameTime\":" + J(Time.time) +
                      ",\"wallTimeUtc\":\"" + DateTime.UtcNow.ToString("o") +
                      "\",\"kind\":\"signal\",\"name\":\"" + name +
                      "\",\"source\":\"SignalBus\",\"payload\":{" + sb + "},\"provenance\":\"live-game\"}";
            try { File.AppendAllText(tracePath, row + "\n"); } catch { }
        };
        bus.Subscribe<GameStartedSignal>(s => emit("GameStartedSignal", s));
        bus.Subscribe<GameEndedSignal>(s => emit("GameEndedSignal", s));
        bus.Subscribe<GamePausedSignal>(s => emit("GamePausedSignal", s));
        bus.Subscribe<WorldGeneratedDataSignal>(s => emit("WorldGeneratedDataSignal", s));
        bus.Subscribe<WorldSpawnPositionsSignal>(s => emit("WorldSpawnPositionsSignal", s));
        bus.Subscribe<WorldBuiltSignal>(s => emit("WorldBuiltSignal", s));
        bus.Subscribe<OnMapObjectSpawnedSignal>(s => emit("OnMapObjectSpawnedSignal", s));
        bus.Subscribe<GridTileChangedSignal>(s => emit("GridTileChangedSignal", s));
        bus.Subscribe<OnObjectsMapChangedSignal>(s => emit("OnObjectsMapChangedSignal", s));
        bus.Subscribe<FogStateChangedSignal>(s => emit("FogStateChangedSignal", s));
        bus.Subscribe<UnitCreatedSignal>(s => emit("UnitCreatedSignal", s));
        bus.Subscribe<UnitMovedSignal>(s => emit("UnitMovedSignal", s));
        bus.Subscribe<UnitDestroyedSignal>(s => emit("UnitDestroyedSignal", s));
        bus.Subscribe<UnitMoveRejectedSignal>(s => emit("UnitMoveRejectedSignal", s));
        bus.Subscribe<MoveUnitRequestSignal>(s => emit("MoveUnitRequestSignal", s));
        bus.Subscribe<LocalUnitSelectionChangedSignal>(s => emit("LocalUnitSelectionChangedSignal", s));
        bus.Subscribe<BuildingPlacedSignal>(s => emit("BuildingPlacedSignal", s));
        bus.Subscribe<BuildingDemolishedSignal>(s => emit("BuildingDemolishedSignal", s));
        bus.Subscribe<BuildingPreviewChangedSignal>(s => emit("BuildingPreviewChangedSignal", s));
        bus.Subscribe<BuildingOperationalSignal>(s => emit("BuildingOperationalSignal", s));
        bus.Subscribe<BuildingOwnershipTransferredSignal>(s => emit("BuildingOwnershipTransferredSignal", s));
        bus.Subscribe<ConstructionPlacementRejectedSignal>(s => emit("ConstructionPlacementRejectedSignal", s));
        bus.Subscribe<PlaceBuildingConfirmRequestSignal>(s => emit("PlaceBuildingConfirmRequestSignal", s));
        bus.Subscribe<SettlementCapturedSignal>(s => emit("SettlementCapturedSignal", s));
        bus.Subscribe<EconomyTickCompletedSignal>(s => emit("EconomyTickCompletedSignal", s));
        bus.Subscribe<SaveRequestedSignal>(s => emit("SaveRequestedSignal", s));
        bus.Subscribe<LoadRequestedSignal>(s => emit("LoadRequestedSignal", s));
        bus.Subscribe<SaveCompletedSignal>(s => emit("SaveCompletedSignal", s));
        bus.Subscribe<TileClickedSignal>(s => emit("TileClickedSignal", s));
        bus.Subscribe<WorldCreationConfirmedSignal>(s => emit("WorldCreationConfirmedSignal", s));
        File.WriteAllText(armedFlag, DateTime.UtcNow.ToString("o"));
        UnityEditor.EditorApplication.playModeStateChanged += change => {
            if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode && File.Exists(armedFlag))
                File.Delete(armedFlag);
        };
        traceInfo = "trace=armed:" + tracePath;
    } else traceInfo = "trace=failed:no-signalbus";
} else if (File.Exists(armedFlag)) traceInfo = "trace=already-armed";

return "wrote " + snapPath + " | playMode=" + playMode + " | gameState=" + gameStateName + " | " + traceInfo;
