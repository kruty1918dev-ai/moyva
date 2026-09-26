#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.Signals;
using Kruty1918.SaveSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zenject;

[InitializeOnLoad]
public static class WorldVisualSmoke
{
    const string Key = "Moyva.WorldVisualSmoke";
    static double began;
    static bool captured;
    static bool gameCaptured;
    static double gameShotAt;
    static int lastMeshCount = -1;
    static int stableTicks;
    static string lastStats = "";
    static WorldGeneratedDataSignal lastSignal;
    static object lastLogicalMap;
    static DiContainer lastContainer;
    static bool gameplayRan;
    static readonly List<Vector3> gameplayBuildingTargets = new List<Vector3>();
    static readonly List<Vector3> gameplayUnitTargets = new List<Vector3>();
    static readonly List<Vector3> gameplayMoveTargets = new List<Vector3>();

    static WorldVisualSmoke()
    {
        EditorApplication.update += Update;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode) began = EditorApplication.timeSinceStartup;
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetString(Key, "") != "")
            {
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SessionState.GetString(Key + ".previous", ""));
                SessionState.EraseString(Key);
                // Play-mode exit leaves scene backups that block the next
                // launch behind a "Scene Backup Detected" modal dialog.
                if (Directory.Exists("Temp/__Backupscenes"))
                    Directory.Delete("Temp/__Backupscenes", true);
                EditorApplication.Exit(0);
            }
        };
        Application.logMessageReceived += (condition, stack, type) =>
        {
            if (SessionState.GetString(Key, "") == "") return;
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                File.AppendAllText("Library/ai/visual-smoke-errors.log", condition + "\n" + stack + "\n");
        };
    }

    static void Update()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (SessionState.GetString(Key, "") != "" || !File.Exists("Library/ai/visual-smoke.request")) return;
            var req = File.ReadAllText("Library/ai/visual-smoke.request");
            File.Delete("Library/ai/visual-smoke.request");
            string outDir = "Library/ai/shots";
            foreach (var raw in req.Split('\n'))
            {
                var line = raw.Trim();
                int i = line.IndexOf('=');
                if (i < 0) continue;
                string k = line.Substring(0, i).Trim(), v = line.Substring(i + 1).Trim();
                if (k == "out" && v.Length > 0) outDir = v;
            }
            SessionState.SetString(Key, "direct");
            SessionState.SetString(Key + ".out", outDir);
            File.WriteAllText("Library/ai/visual-smoke-errors.log", "");
            SessionState.SetString(Key + ".previous", AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Moyva/Scenes/Gamplay_Scene.unity");
            GameLaunchContext.ConfigureDirectGameplayTest();
            EditorApplication.isPlaying = true;
            return;
        }
        if (!EditorApplication.isPlaying || SessionState.GetString(Key, "") == "" || began == 0) return;
        try
        {
            if (EditorApplication.timeSinceStartup - began > 300) { Finish("FAIL timeout"); return; }
            if (captured)
            {
                // Run the gameplay probe as soon as the world is captured so the
                // spawned buildings/units have the full pre-capture window to
                // materialize before the game-camera shots are taken.
                if (!gameplayRan) { gameplayRan = true; RunGameplayProbe(lastSignal); }
                if (gameCaptured || EditorApplication.timeSinceStartup < gameShotAt) return;
                gameCaptured = true;
                CaptureGameShots(lastSignal);
                Finish("PASS " + lastStats);
                return;
            }
            foreach (var context in UnityEngine.Object.FindObjectsByType<SceneContext>())
            {
                if (context.gameObject.scene.name != "Gamplay_Scene") continue;
                var world = context.Container.TryResolve<IWorldGenerationSignalState>();
                if (world == null || !world.TryGetWorldGeneratedData(out var signal) || signal.TileMap == null) continue;
                lastContainer = context.Container;
                var stateType = FindGeneratorType("Kruty1918.Moyva.Generator.Runtime.IMapGenerationState");
                if (stateType != null)
                {
                    object state = context.Container.TryResolve(stateType);
                    lastLogicalMap = state?.GetType().GetProperty("LastLogicalMap")?.GetValue(state);
                }
                int meshes = CountTerrainMeshes();
                if (meshes < 1) continue;
                stableTicks = meshes == lastMeshCount ? stableTicks + 1 : 0;
                lastMeshCount = meshes;
                if (stableTicks < 30) continue;
                captured = true;
                lastSignal = signal;
                lastStats = Capture(signal, meshes);
                gameShotAt = EditorApplication.timeSinceStartup + 2.5;
                return;
            }
        }
        catch (Exception e) { Finish("FAIL " + e); }
    }

    static int CountTerrainMeshes()
    {
        int n = 0;
        foreach (var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>())
            if (mf != null && mf.gameObject.name == "TerrainMesh") n++;
        return n;
    }

    static bool IsWaterId(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        id = id.ToLowerInvariant();
        return id.Contains("water") || id.Contains("ocean") || id.Contains("river") || id.Contains("lake");
    }

    static string Capture(WorldGeneratedDataSignal signal, int meshCount)
    {
        Directory.CreateDirectory("Temp/ai");
        float cs = signal.CellSize <= 0.0001f ? 1f : signal.CellSize;
        Vector3 center = signal.HasMapWorldBounds
            ? signal.MapWorldBoundsCenter
            : new Vector3(signal.Width * cs * 0.5f, 0f, signal.Height * cs * 0.5f);
        Vector3 size = signal.HasMapWorldBounds
            ? signal.MapWorldBoundsSize
            : new Vector3(signal.Width * cs, 8f, signal.Height * cs);

        var hist = new Dictionary<string, int>(StringComparer.Ordinal);
        int water = 0, land = 0, empty = 0;
        Vector2Int shoreCell = new Vector2Int(-1, -1), highCell = new Vector2Int(0, 0);
        Vector2Int transitionCell = new Vector2Int(-1, -1);
        float maxH = float.MinValue;
        for (int x = 0; x < signal.Width; x++)
        for (int y = 0; y < signal.Height; y++)
        {
            string id = signal.TileMap[x, y];
            if (string.IsNullOrEmpty(id)) { empty++; continue; }
            hist[id] = hist.TryGetValue(id, out int c) ? c + 1 : 1;
            if (IsWaterId(id)) water++; else land++;
            float h = signal.HeightMap != null ? signal.HeightMap[x, y] : 0f;
            if (h > maxH) { maxH = h; highCell = new Vector2Int(x, y); }
            if (shoreCell.x < 0 && !IsWaterId(id) && HasWaterNeighbour(signal, x, y))
                shoreCell = new Vector2Int(x, y);
            if (transitionCell.x < 0 && !IsWaterId(id) && HasDiffLandNeighbour(signal, x, y))
                transitionCell = new Vector2Int(x, y);
        }

        // Hydrology-targeted shots: winner layer names from the logical map
        // let us aim close-ups at real river/lake cells instead of guessing.
        Vector2Int riverCell = new Vector2Int(-1, -1), lakeCell = new Vector2Int(-1, -1);
        Vector2Int fallsCell = new Vector2Int(-1, -1), confluenceCell = new Vector2Int(-1, -1);
        float riverTop = float.MinValue;
        string[,] winnerLayers = null;
        if (lastLogicalMap != null)
        {
            try { winnerLayers = lastLogicalMap.GetType().GetProperty("LayerNames")?.GetValue(lastLogicalMap) as string[,]; }
            catch { }
        }
        if (winnerLayers != null && signal.SurfaceHeightMap != null)
        {
            for (int x = 0; x < signal.Width; x++)
            for (int y = 0; y < signal.Height; y++)
            {
                string ln = winnerLayers[x, y];
                if (ln == "River")
                {
                    float sv = signal.SurfaceHeightMap[x, y];
                    if (riverCell.x < 0 || sv > riverTop)
                    {
                        // Prefer a mid-channel cell: ≥2 river neighbours.
                        if (CountLayerNeighbours(winnerLayers, x, y, signal.Width, signal.Height, "River") >= 2)
                        {
                            riverCell = new Vector2Int(x, y);
                            riverTop = sv;
                        }
                        else if (riverCell.x < 0) riverCell = new Vector2Int(x, y);
                    }
                    if (confluenceCell.x < 0
                        && CountLayerNeighbours(winnerLayers, x, y, signal.Width, signal.Height, "River") >= 3)
                        confluenceCell = new Vector2Int(x, y);
                }
                else if (ln == "Lake" && lakeCell.x < 0)
                {
                    lakeCell = new Vector2Int(x, y);
                }
                if (fallsCell.x < 0 && (ln == "River" || ln == "Lake"))
                {
                    float sv = signal.SurfaceHeightMap[x, y];
                    float lo = float.MaxValue;
                    for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= signal.Width || ny >= signal.Height) continue;
                        float nv = signal.SurfaceHeightMap[nx, ny];
                        if (!float.IsNaN(nv) && nv < lo) lo = nv;
                    }
                    if (sv - lo >= 0.45f) fallsCell = new Vector2Int(x, y);
                }
            }
        }

        long verts = 0, nanVerts = 0;
        foreach (var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>())
        {
            if (mf == null || mf.sharedMesh == null || mf.gameObject.name != "TerrainMesh") continue;
            var vs = mf.sharedMesh.vertices;
            verts += vs.Length;
            foreach (var v in vs)
                if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z)) nanVerts++;
        }

        var stats = new StringBuilder();
        stats.Append($"world={signal.Width}x{signal.Height} cell={cs} meshes={meshCount} verts={verts} nanVerts={nanVerts} land={land} water={water} empty={empty} tiles=[");
        bool first = true;
        foreach (var kv in hist)
        {
            if (!first) stats.Append(',');
            stats.Append(kv.Key).Append(':').Append(kv.Value);
            first = false;
        }
        stats.Append(']');
        stats.Append($" worldHash={WorldHash(signal):X16}");
        File.WriteAllText("Library/ai/visual-smoke-stats.txt", stats.ToString());

        var diag = new StringBuilder();
        foreach (var r in UnityEngine.Object.FindObjectsByType<Renderer>())
        {
            if (r == null || r.gameObject.name != "TerrainMesh") continue;
            var m = r.sharedMaterial;
            diag.Append($"[renderer b={r.bounds.center}+{r.bounds.size} layer={r.gameObject.layer} mat={(m ? m.name + "/" + m.shader.name : "null")} vis={r.isVisible}] ");
            break;
        }
        foreach (var c in UnityEngine.Object.FindObjectsByType<Camera>())
            diag.Append($"[cam {c.name} pos={c.transform.position} ortho={c.orthographic} tag={c.tag} enabled={c.enabled}] ");
        File.AppendAllText("Library/ai/visual-smoke-shots.log", diag.ToString() + "\n");

        var targets = new List<MeshFilter>();
        foreach (var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include))
        {
            if (mf == null || mf.sharedMesh == null) continue;
            targets.Add(mf);
        }

        string outDir = SessionState.GetString(Key + ".out", "Library/ai/shots");
        Directory.CreateDirectory(outDir);
        int launchSeed = GameLaunchContext.TryGetSeed(out int s) ? s : 0;
        var manifest = new StringBuilder();
        manifest.AppendLine($"launchSeed={(launchSeed != 0 ? launchSeed.ToString() : "default(42)")}");
        manifest.AppendLine($"world={signal.Width}x{signal.Height} cellSize={cs}");
        manifest.AppendLine($"center={center} size={size}");
        manifest.AppendLine($"shoreCell={shoreCell} transitionCell={transitionCell} highCell={highCell} maxH={maxH}");
        manifest.AppendLine($"riverCell={riverCell} lakeCell={lakeCell} fallsCell={fallsCell} confluenceCell={confluenceCell}");
        manifest.AppendLine(stats.ToString());
        int goCount = 0, mfCount = 0, rCount = 0;
        foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)) { if (t != null) goCount++; }
        foreach (var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include)) { if (mf != null) mfCount++; }
        foreach (var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include)) { if (r != null) rCount++; }
        manifest.AppendLine(
            $"genTimeSec={(EditorApplication.timeSinceStartup - began):F1} " +
            $"gameObjects={goCount} meshFilters={mfCount} renderers={rCount} " +
            $"memMB={UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1048576}");

        // Out-of-bounds geometry scan: vertices outside the map rect or far
        // below the surface reveal fragments that protrude past the border.
        var oob = new StringBuilder();
        float rMinX = center.x - size.x * 0.5f - 0.02f, rMaxX = center.x + size.x * 0.5f + 0.02f;
        float rMinZ = center.z - size.z * 0.5f - 0.02f, rMaxZ = center.z + size.z * 0.5f + 0.02f;
        foreach (var mf in targets)
        {
            var vs = mf.sharedMesh.vertices;
            var mtx = mf.transform.localToWorldMatrix;
            int n = 0;
            float loY = float.MaxValue, hiY = float.MinValue;
            float ox0 = float.MaxValue, ox1 = float.MinValue, oz0 = float.MaxValue, oz1 = float.MinValue;
            float oy0 = float.MaxValue, oy1 = float.MinValue;
            for (int i = 0; i < vs.Length; i++)
            {
                Vector3 wv = mtx.MultiplyPoint3x4(vs[i]);
                if (wv.y < loY) loY = wv.y;
                if (wv.y > hiY) hiY = wv.y;
                if (wv.x < rMinX || wv.x > rMaxX || wv.z < rMinZ || wv.z > rMaxZ)
                {
                    n++;
                    if (wv.x < ox0) ox0 = wv.x; if (wv.x > ox1) ox1 = wv.x;
                    if (wv.z < oz0) oz0 = wv.z; if (wv.z > oz1) oz1 = wv.z;
                    if (wv.y < oy0) oy0 = wv.y; if (wv.y > oy1) oy1 = wv.y;
                }
            }
            if (n > 0)
            {
                var mats = "";
                var rend = mf.GetComponent<MeshRenderer>();
                if (rend != null)
                    foreach (var mm in rend.sharedMaterials) mats += (mm ? mm.name : "null") + ";";
                oob.AppendLine($"{mf.gameObject.name} oobVerts={n}/{vs.Length} " +
                    $"x[{ox0:F2}..{ox1:F2}] z[{oz0:F2}..{oz1:F2}] oobY[{oy0:F2}..{oy1:F2}] minY={loY:F2} mats={mats}");
            }
        }
        File.WriteAllText(Path.Combine(outDir, "oob.txt"),
            $"mapRect x[{rMinX:F2}..{rMaxX:F2}] z[{rMinZ:F2}..{rMaxZ:F2}]\n" + oob.ToString());

        float R = Mathf.Max(size.x, size.z);
        ShotAt(targets, outDir, "topdown",
            center + Vector3.up * (size.y + R + 60f), Quaternion.Euler(90f, 0f, 0f),
            ortho: true, orthoSize: R * 0.55f);

        string[] sideNames = { "side_n", "side_e", "side_s", "side_w" };
        Vector3[] sideDirs = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
        for (int i = 0; i < 4; i++)
        {
            Vector3 pos = center + sideDirs[i] * (R * 1.0f) + Vector3.up * (R * 0.45f);
            ShotAt(targets, outDir, sideNames[i], pos,
                Quaternion.LookRotation(center - pos, Vector3.up));
        }

        Vector3 lowPos = center + new Vector3(-1f, 0f, -1f).normalized * (R * 0.85f) + Vector3.up * 7f;
        ShotAt(targets, outDir, "low_sw", lowPos,
            Quaternion.LookRotation(center + Vector3.up * 3f - lowPos, Vector3.up));

        Vector3 southT = CellWorld(new Vector2Int(signal.Width / 2, 1), signal, cs);
        ShotAt(targets, outDir, "edge_s",
            southT + new Vector3(0f, 7f, -12f),
            Quaternion.LookRotation(southT - (southT + new Vector3(0f, 7f, -12f)), Vector3.up));

        Vector3 cornerT = CellWorld(new Vector2Int(2, 2), signal, cs);
        ShotAt(targets, outDir, "corner_sw",
            cornerT + new Vector3(-11f, 6f, -11f),
            Quaternion.LookRotation(cornerT - (cornerT + new Vector3(-11f, 6f, -11f)), Vector3.up));

        if (shoreCell.x >= 0)
        {
            Vector3 t = CellWorld(shoreCell, signal, cs);
            ShotAt(targets, outDir, "shore",
                t + new Vector3(10f, 14f, -14f),
                Quaternion.LookRotation(t - (t + new Vector3(10f, 14f, -14f)), Vector3.up));
            ShotAt(targets, outDir, "shore2",
                t + new Vector3(-14f, 9f, 10f),
                Quaternion.LookRotation(t - (t + new Vector3(-14f, 9f, 10f)), Vector3.up));
            ShotAt(targets, outDir, "detail_shore",
                t + Vector3.up * 16f, Quaternion.Euler(90f, 0f, 0f), ortho: true, orthoSize: 7f);
        }

        if (transitionCell.x >= 0)
        {
            Vector3 t = CellWorld(transitionCell, signal, cs);
            ShotAt(targets, outDir, "transition",
                t + new Vector3(9f, 12f, -9f),
                Quaternion.LookRotation(t - (t + new Vector3(9f, 12f, -9f)), Vector3.up));
            ShotAt(targets, outDir, "detail_transition",
                t + Vector3.up * 14f, Quaternion.Euler(90f, 0f, 0f), ortho: true, orthoSize: 6f);
        }

        if (riverCell.x >= 0)
        {
            Vector3 t = CellWorld(riverCell, signal, cs);
            ShotAt(targets, outDir, "river",
                t + new Vector3(10f, 13f, -13f),
                Quaternion.LookRotation(t - (t + new Vector3(10f, 13f, -13f)), Vector3.up));
            ShotAt(targets, outDir, "detail_river",
                t + Vector3.up * 14f, Quaternion.Euler(90f, 0f, 0f), ortho: true, orthoSize: 5.5f);
        }
        if (fallsCell.x >= 0)
        {
            Vector3 t = CellWorld(fallsCell, signal, cs);
            ShotAt(targets, outDir, "waterfall",
                t + new Vector3(9f, 8f, -11f),
                Quaternion.LookRotation(t - (t + new Vector3(9f, 8f, -11f)), Vector3.up));
            ShotAt(targets, outDir, "detail_waterfall",
                t + Vector3.up * 12f, Quaternion.Euler(90f, 0f, 0f), ortho: true, orthoSize: 4.5f);
        }
        if (lakeCell.x >= 0)
        {
            Vector3 t = CellWorld(lakeCell, signal, cs);
            ShotAt(targets, outDir, "lake",
                t + new Vector3(11f, 13f, -13f),
                Quaternion.LookRotation(t - (t + new Vector3(11f, 13f, -13f)), Vector3.up));
            ShotAt(targets, outDir, "detail_lake",
                t + Vector3.up * 15f, Quaternion.Euler(90f, 0f, 0f), ortho: true, orthoSize: 6f);
        }
        if (confluenceCell.x >= 0)
        {
            Vector3 t = CellWorld(confluenceCell, signal, cs);
            ShotAt(targets, outDir, "confluence",
                t + new Vector3(10f, 12f, -12f),
                Quaternion.LookRotation(t - (t + new Vector3(10f, 12f, -12f)), Vector3.up));
            ShotAt(targets, outDir, "detail_confluence",
                t + Vector3.up * 13f, Quaternion.Euler(90f, 0f, 0f), ortho: true, orthoSize: 5f);
        }

        Vector3 hp = CellWorld(highCell, signal, cs);
        ShotAt(targets, outDir, "peak",
            hp + new Vector3(12f, 16f, -16f),
            Quaternion.LookRotation(hp - (hp + new Vector3(12f, 16f, -16f)), Vector3.up));

        ShotAt(targets, outDir, "topdown_persp",
            center + Vector3.up * (size.y + R + 60f), Quaternion.Euler(90f, 0f, 0f));
        ShotAt(targets, outDir, "topdown_late",
            center + Vector3.up * (size.y + R + 60f), Quaternion.Euler(90f, 0f, 0f),
            ortho: true, orthoSize: R * 0.55f);

        // Raw per-cell dumps for offline geometric checks (shore band width,
        // sand distance-to-water, biome adjacency) that screenshots can't prove.
        var mapDump = new StringBuilder();
        var hDump = new StringBuilder();
        var sDump = new StringBuilder();
        var lDump = new StringBuilder();
        for (int y = 0; y < signal.Height; y++)
        {
            for (int x = 0; x < signal.Width; x++)
            {
                if (x > 0) { mapDump.Append(','); hDump.Append(','); sDump.Append(','); lDump.Append(','); }
                mapDump.Append(signal.TileMap != null ? signal.TileMap[x, y] ?? "" : "");
                float hv = signal.HeightMap != null ? signal.HeightMap[x, y] : 0f;
                hDump.Append(hv.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                float sv = signal.SurfaceHeightMap != null ? signal.SurfaceHeightMap[x, y] : float.NaN;
                sDump.Append(sv.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                int lv = signal.TerrainLevelMap != null ? signal.TerrainLevelMap[x, y] : 0;
                lDump.Append(lv);
            }
            mapDump.Append('\n'); hDump.Append('\n'); sDump.Append('\n'); lDump.Append('\n');
        }
        File.WriteAllText(Path.Combine(outDir, "tilemap.csv"), mapDump.ToString());
        File.WriteAllText(Path.Combine(outDir, "heightmap.csv"), hDump.ToString());
        File.WriteAllText(Path.Combine(outDir, "surfaceheightmap.csv"), sDump.ToString());
        File.WriteAllText(Path.Combine(outDir, "terrainlevelmap.csv"), lDump.ToString());
        DumpLogicalMap(outDir, signal);
        DumpHydrology(outDir, signal);
        DumpSeabed(outDir, signal, targets);
        DumpWaterfalls(outDir, signal, targets);
        DumpWaterMaterial(outDir, targets);
        DumpArtifacts(outDir, signal, targets);
        DumpDecorations(outDir, signal);

        File.WriteAllText(Path.Combine(outDir, "manifest.txt"), manifest.ToString());
        return stats.ToString();
    }

    static Type FindGeneratorType(string fullName)
    {
        // A public type in the Kruty1918.Moyva.Generator assembly gives access
        // to its internals by name without scanning AppDomain (UAC0005).
        return typeof(Kruty1918.Moyva.Generator.Runtime.MoyvaTerrainHeightAwareTilesBuildLayer)
            .Assembly.GetType(fullName);
    }

    // Reflection-only access to the internal LogicalTileMap: the editor
    // assembly has no InternalsVisibleTo for Kruty1918.Moyva.Generator.
    static void DumpHydrology(string outDir, WorldGeneratedDataSignal signal)
    {
        try
        {
            var storeType = FindGeneratorType("Kruty1918.Moyva.Generator.Runtime.RecipeHydrologyStore");
            if (storeType == null || lastContainer == null) return;
            object store = lastContainer.TryResolve(storeType);
            object plan = store?.GetType().GetProperty("Plan")?.GetValue(store);
            if (plan == null) return;
            var pt = plan.GetType();
            var river = pt.GetField("RiverMask")?.GetValue(plan) as bool[,];
            var lake = pt.GetField("LakeMask")?.GetValue(plan) as bool[,];
            var surface = pt.GetField("WaterSurface")?.GetValue(plan) as float[,];
            var bed = pt.GetField("BedHeight")?.GetValue(plan) as float[,];
            var parentArr = pt.GetField("FlowParent")?.GetValue(plan) as int[,];
            var acc = pt.GetField("Accumulation")?.GetValue(plan) as float[,];
            if (river == null) return;
            var sb = new StringBuilder();
            for (int y = 0; y < signal.Height; y++)
            {
                for (int x = 0; x < signal.Width; x++)
                {
                    if (x > 0) sb.Append(';');
                    string kind = river[x, y] ? "R" : lake != null && lake[x, y] ? "L" : ".";
                    float s = surface != null ? surface[x, y] : float.NaN;
                    float b = bed != null ? bed[x, y] : float.NaN;
                    int p = parentArr != null ? parentArr[x, y] : -1;
                    float a = acc != null ? acc[x, y] : float.NaN;
                    sb.Append(kind)
                        .Append('|').Append(s.ToString("F2", System.Globalization.CultureInfo.InvariantCulture))
                        .Append('|').Append(b.ToString("F2", System.Globalization.CultureInfo.InvariantCulture))
                        .Append('|').Append(p)
                        .Append('|').Append(a.ToString("F0", System.Globalization.CultureInfo.InvariantCulture));
                }
                sb.Append('\n');
            }
            File.WriteAllText(Path.Combine(outDir, "hydromap.csv"), sb.ToString());
        }
        catch (Exception e)
        {
            File.AppendAllText("Library/ai/visual-smoke-errors.log", "DumpHydrology: " + e + "\n");
        }
    }

    // Reflection-only access to the internal seabed service: dumps the shared
    // bed field as CSV and renders the chunk-built seabed mesh alone (water
    // sheet off) so the sloping bottom is inspectable from top and section.
    static void DumpSeabed(string outDir, WorldGeneratedDataSignal signal, List<MeshFilter> targets)
    {
        GameObject probe = null;
        Mesh seabedMesh = null;
        try
        {
            float cs = signal.CellSize <= 0.0001f ? 1f : signal.CellSize;
            var svcType = FindGeneratorType("Kruty1918.Moyva.Generator.Runtime.ChunkFirst.SeabedChunkMeshService");
            if (svcType == null || lastContainer == null) return;
            object svc = lastContainer.TryResolve(svcType);
            if (svc == null) return;
            var tryGetBed = svcType.GetMethod("TryGetBedY");
            var tryGetDist = svcType.GetMethod("TryGetShoreDistance");
            var tryBuild = svcType.GetMethod("TryBuildChunkMesh");
            var isActiveProp = svcType.GetProperty("IsActive");
            bool active = isActiveProp != null && (bool)isActiveProp.GetValue(svc);
            object field = svcType.GetField("_field",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(svc);
            File.WriteAllText(Path.Combine(outDir, "seabed-active.txt"),
                $"seabedActive={active} fieldBuilt={(field != null)}\n");
            if (!active || tryGetBed == null) return;

            var sb = new StringBuilder();
            var beds = new float[signal.Width, signal.Height];
            Vector2Int deepest = new Vector2Int(-1, -1);
            float deepestDist = -1f;
            int waterCells = 0, beddedCells = 0;
            for (int y = 0; y < signal.Height; y++)
            {
                for (int x = 0; x < signal.Width; x++)
                {
                    if (x > 0) sb.Append(';');
                    beds[x, y] = float.NaN;
                    object[] args = { new Vector2Int(x, y), 0f };
                    bool hasBed = (bool)tryGetBed.Invoke(svc, args);
                    float bed = (float)args[1];
                    object[] dargs = { new Vector2Int(x, y), 0f };
                    bool hasDist = tryGetDist != null && (bool)tryGetDist.Invoke(svc, dargs);
                    float dist = hasDist ? (float)dargs[1] : float.NaN;
                    if (hasBed)
                    {
                        beds[x, y] = bed;
                        beddedCells++;
                        if (hasDist && dist > deepestDist)
                        {
                            deepestDist = dist;
                            deepest = new Vector2Int(x, y);
                        }
                    }
                    if (IsWaterId(signal.TileMap != null ? signal.TileMap[x, y] : null)
                        || hasBed)
                    {
                        waterCells++;
                    }
                    sb.Append(hasBed
                        ? bed.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                        : "")
                      .Append('|')
                      .Append(hasDist
                          ? dist.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                          : "");
                }
                sb.Append('\n');
            }
            File.WriteAllText(Path.Combine(outDir, "seabedmap.csv"), sb.ToString());

            // Bed-step audit: no two adjacent water cells may jump by more
            // than a chunk-worthy seam; the shared lattice must keep borders
            // smooth, and every water sheet must have a bed under it.
            float maxStep = 0f;
            int uncovered = 0;
            for (int y = 0; y < signal.Height; y++)
            for (int x = 0; x < signal.Width; x++)
            {
                bool waterTile = IsWaterId(signal.TileMap != null ? signal.TileMap[x, y] : null);
                if (waterTile && float.IsNaN(beds[x, y])) uncovered++;
                if (x + 1 < signal.Width && !float.IsNaN(beds[x, y]) && !float.IsNaN(beds[x + 1, y]))
                    maxStep = Mathf.Max(maxStep, Mathf.Abs(beds[x + 1, y] - beds[x, y]));
                if (y + 1 < signal.Height && !float.IsNaN(beds[x, y]) && !float.IsNaN(beds[x, y + 1]))
                    maxStep = Mathf.Max(maxStep, Mathf.Abs(beds[x, y + 1] - beds[x, y]));
            }
            File.AppendAllText(Path.Combine(outDir, "seabed-active.txt"),
                $"waterCells={waterCells} beddedCells={beddedCells} uncoveredWaterTiles={uncovered} maxAdjacentBedStep={maxStep:F3} deepest={deepest}@{deepestDist:F2}m\n");

            if (tryBuild != null)
            {
                object[] bargs = { new RectInt(0, 0, signal.Width, signal.Height), null, null };
                if ((bool)tryBuild.Invoke(svc, bargs) && bargs[1] is Mesh mesh && bargs[2] is Material mat)
                {
                    seabedMesh = mesh;
                    probe = new GameObject("__seabed_probe");
                    var mf = probe.AddComponent<MeshFilter>();
                    var mr = probe.AddComponent<MeshRenderer>();
                    mf.sharedMesh = mesh;
                    mr.sharedMaterial = mat;
                    var alone = new List<MeshFilter> { mf };
                    Vector3 center = signal.HasMapWorldBounds
                        ? signal.MapWorldBoundsCenter
                        : new Vector3(signal.Width * cs * 0.5f, 0f, signal.Height * cs * 0.5f);
                    Vector3 size = signal.HasMapWorldBounds
                        ? signal.MapWorldBoundsSize
                        : new Vector3(signal.Width * cs, 8f, signal.Height * cs);
                    float R = Mathf.Max(size.x, size.z);
                    ShotAt(alone, outDir, "seabed_topdown",
                        center + Vector3.up * (size.y + R + 60f),
                        Quaternion.Euler(90f, 0f, 0f), ortho: true, orthoSize: R * 0.55f);
                    ShotAt(alone, outDir, "seabed_iso",
                        center + new Vector3(0f, R * 0.6f, -R * 0.6f),
                        Quaternion.LookRotation(center - (center + new Vector3(0f, R * 0.6f, -R * 0.6f)), Vector3.up));
                    if (deepest.x >= 0)
                    {
                        Vector3 t = CellWorld(deepest, signal, cs);
                        ShotAt(alone, outDir, "seabed_detail_deep",
                            t + Vector3.up * 10f, Quaternion.Euler(90f, 0f, 0f), ortho: true, orthoSize: 8f);
                        ShotAt(alone, outDir, "seabed_section",
                            t + new Vector3(0f, 2.5f, -14f),
                            Quaternion.LookRotation(new Vector3(t.x, t.y - 1.5f, t.z) - (t + new Vector3(0f, 2.5f, -14f)), Vector3.up));
                    }
                    // Context: the same bed inside the real terrain scene.
                    var withTerrain = new List<MeshFilter>(targets) { mf };
                    ShotAt(withTerrain, outDir, "seabed_with_terrain_iso",
                        center + new Vector3(0f, R * 0.6f, -R * 0.6f),
                        Quaternion.LookRotation(center - (center + new Vector3(0f, R * 0.6f, -R * 0.6f)), Vector3.up));

                    // Acceptance strip: water ON, camera sliding along the
                    // shore->deep gradient so the visibility fade reads in
                    // one frame. Ortho top-down over the deepest body.
                    if (deepest.x >= 0)
                    {
                        Vector3 deepWs = CellWorld(deepest, signal, cs);
                        // Find a shore cell inside the same body: scan a
                        // straight line from the map edge toward deepest.
                        Vector2Int shore = deepest;
                        float best = float.MaxValue;
                        for (int yy = 0; yy < signal.Height; yy++)
                        for (int xx = 0; xx < signal.Width; xx++)
                        {
                            object[] dargs2 = { new Vector2Int(xx, yy), 0f };
                            if (tryGetDist != null
                                && (bool)tryGetDist.Invoke(svc, dargs2)
                                && (float)dargs2[1] < 0.5f)
                            {
                                float dd = (new Vector2(xx - deepest.x, yy - deepest.y)).sqrMagnitude;
                                if (dd < best) { best = dd; shore = new Vector2Int(xx, yy); }
                            }
                        }
                        Vector3 shoreWs = CellWorld(shore, signal, cs);
                        Vector3 mid = (shoreWs + deepWs) * 0.5f;
                        Vector3 dir = (deepWs - shoreWs); dir.y = 0f;
                        float len = Mathf.Max(4f, dir.magnitude);
                        ShotAt(targets, outDir, "water_transect_top",
                            mid + Vector3.up * (len + 14f),
                            Quaternion.Euler(90f, 0f, 0f), ortho: true,
                            orthoSize: len * 0.62f + 2f);
                        ShotAt(targets, outDir, "water_transect_low",
                            shoreWs + new Vector3(0f, 7f, -len * 0.9f),
                            Quaternion.LookRotation(
                                deepWs + Vector3.down * 2f
                                - (shoreWs + new Vector3(0f, 7f, -len * 0.9f)),
                                Vector3.up));
                    }
                }
            }
        }
        catch (Exception e)
        {
            File.AppendAllText("Library/ai/visual-smoke-errors.log", "DumpSeabed: " + e + "\n");
        }
        finally
        {
            if (probe != null) UnityEngine.Object.Destroy(probe);
            if (seabedMesh != null) UnityEngine.Object.Destroy(seabedMesh);
        }
    }

    static void DumpWaterfalls(string outDir, WorldGeneratedDataSignal signal, List<MeshFilter> targets)
    {
        GameObject probe = null;
        Mesh curtainMesh = null;
        try
        {
            float cs = signal.CellSize <= 0.0001f ? 1f : signal.CellSize;
            var svcType = FindGeneratorType("Kruty1918.Moyva.Generator.Runtime.ChunkFirst.WaterfallChunkMeshService");
            if (svcType == null || lastContainer == null) return;
            object svc = lastContainer.TryResolve(svcType);
            if (svc == null) return;
            bool active = svcType.GetProperty("IsActive")?.GetValue(svc) is bool a && a;
            bool hasField = svcType.GetProperty("HasField")?.GetValue(svc) is bool f && f;
            float minDrop = svcType.GetProperty("MinDropMeters")?.GetValue(svc) is float md ? md : float.NaN;
            var fronts = svcType.GetProperty("Fronts")?.GetValue(svc) as System.Collections.IList;

            int vfxCount = 0;
            var spawned = UnityEngine.Object.FindObjectsByType<ParticleSystem>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < spawned.Length; i++)
            {
                if (spawned[i].name.StartsWith("wfall_", StringComparison.Ordinal))
                    vfxCount++;
            }
            File.WriteAllText(Path.Combine(outDir, "waterfall-active.txt"),
                $"waterfallsActive={active} fieldBuilt={hasField} minDropM={minDrop:F2} fronts={(fronts != null ? fronts.Count : -1)} vfxSystems={vfxCount}\n");

            var sb = new StringBuilder("anchorX;anchorY;dirX;dirY;widthCells;topY;bottomY;drop\n");
            if (fronts != null)
            {
                for (int i = 0; i < fronts.Count; i++)
                {
                    object fr = fronts[i];
                    var anchor = (Vector2Int)GetMember(fr, "Anchor");
                    var dir = (Vector2Int)GetMember(fr, "Dir");
                    int w = (int)GetMember(fr, "WidthCells");
                    float top = (float)GetMember(fr, "TopY");
                    float bot = (float)GetMember(fr, "BottomY");
                    float drop = (float)GetMember(fr, "Drop");
                    sb.Append(anchor.x).Append(';').Append(anchor.y).Append(';')
                      .Append(dir.x).Append(';').Append(dir.y).Append(';')
                      .Append(w).Append(';')
                      .Append(top.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)).Append(';')
                      .Append(bot.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)).Append(';')
                      .Append(drop.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)).Append('\n');
                }
            }
            File.WriteAllText(Path.Combine(outDir, "waterfalls.csv"), sb.ToString());
            if (!hasField || fronts == null || fronts.Count == 0)
                return;

            // Isolated curtain render over the whole map + close-up of the
            // largest front inside the real terrain.
            var tryBuild = svcType.GetMethod("TryBuildChunkMesh");
            if (tryBuild != null)
            {
                object[] bargs = { new RectInt(0, 0, signal.Width, signal.Height), null, null };
                if ((bool)tryBuild.Invoke(svc, bargs) && bargs[1] is Mesh mesh && bargs[2] is Material mat)
                {
                    curtainMesh = mesh;
                    probe = new GameObject("__waterfall_probe");
                    var mf = probe.AddComponent<MeshFilter>();
                    var mr = probe.AddComponent<MeshRenderer>();
                    mf.sharedMesh = mesh;
                    mr.sharedMaterial = mat;
                    var alone = new List<MeshFilter> { mf };
                    Vector3 center = signal.HasMapWorldBounds
                        ? signal.MapWorldBoundsCenter
                        : new Vector3(signal.Width * cs * 0.5f, 0f, signal.Height * cs * 0.5f);
                    float R = Mathf.Max(signal.Width, signal.Height) * cs;
                    ShotAt(alone, outDir, "waterfalls_topdown",
                        center + Vector3.up * (R + 60f),
                        Quaternion.Euler(90f, 0f, 0f), ortho: true, orthoSize: R * 0.55f);
                }
            }

            // Close-up: widest front — camera looks at the fall face from
            // the lower side, like a player panning the game camera.
            int best = 0;
            float bestScore = -1f;
            for (int i = 0; i < fronts.Count; i++)
            {
                object fr = fronts[i];
                float score = (float)GetMember(fr, "Drop")
                    * (int)GetMember(fr, "WidthCells");
                if (score > bestScore) { bestScore = score; best = i; }
            }
            object front = fronts[best];
            var fdir = (Vector2Int)GetMember(front, "Dir");
            var fcenter = (Vector3)GetMember(front, "Center");
            float ftop = (float)GetMember(front, "TopY");
            float fbot = (float)GetMember(front, "BottomY");
            int fwidth = (int)GetMember(front, "WidthCells");
            Vector3 n = new Vector3(fdir.x, 0f, fdir.y).normalized;
            Vector3 look = new Vector3(fcenter.x * cs, (ftop + fbot) * 0.5f, fcenter.z * cs);
            float dist = Mathf.Max(6f, fwidth * cs * 1.6f + 4f);
            Vector3 cam = look + n * dist + Vector3.up * (dist * 0.45f);
            ShotAt(targets, outDir, "waterfall_closeup",
                cam, Quaternion.LookRotation(look - cam, Vector3.up));
            ShotAt(targets, outDir, "waterfall_closeup_top",
                look + Vector3.up * (dist + 6f),
                Quaternion.Euler(70f, 0f, 0f), ortho: true, orthoSize: dist * 0.7f);
        }
        catch (Exception e)
        {
            File.AppendAllText("Library/ai/visual-smoke-errors.log", "DumpWaterfalls: " + e + "\n");
        }
        finally
        {
            if (probe != null) UnityEngine.Object.Destroy(probe);
            if (curtainMesh != null) UnityEngine.Object.Destroy(curtainMesh);
        }
    }

    // Dumps the ACTUAL water material state of a rendered chunk: effective
    // shader, keywords and the depth-shading parameters that drive the
    // shore->deep gradient. Answers "which material is really on the water".
    /// <summary>
    /// Artifact forensics for the water/seam task: a per-chunk
    /// submesh-vs-material audit, a waterfall VFX placement dump, the
    /// tallest land wall close-up, a water-to-map-border shot and a
    /// uniform "clay" material pass that removes every transparency /
    /// refraction / foam variable so geometry-vs-material causes can be
    /// separated from shading ones.
    /// </summary>
    static void DumpArtifacts(string outDir, WorldGeneratedDataSignal signal, List<MeshFilter> targets)
    {
        try
        {
            float cs = signal.CellSize <= 0.0001f ? 1f : signal.CellSize;

            // 1) Combined-mesh material audit: slots must align with
            //    submeshes, and list which slots hold transparent water.
            var sb = new StringBuilder("renderer;mesh;submeshes;materialSlots;slots(name|queue|shader)\n");
            foreach (var mf in targets)
            {
                var mr = mf.GetComponent<MeshRenderer>();
                var mesh = mf.sharedMesh;
                var mats = mr != null ? mr.sharedMaterials : null;
                sb.Append(mf.name).Append(';')
                  .Append(mesh != null ? mesh.name : "null").Append(';')
                  .Append(mesh != null ? mesh.subMeshCount : -1).Append(';')
                  .Append(mats != null ? mats.Length : -1).Append(';');
                if (mats != null)
                    for (int i = 0; i < mats.Length; i++)
                        sb.Append('[').Append(i).Append("]=")
                          .Append(mats[i] != null
                              ? mats[i].name + "|q" + mats[i].renderQueue + "|" + mats[i].shader.name
                              : "null");
                sb.Append('\n');
            }
            File.WriteAllText(Path.Combine(outDir, "material-audit.csv"), sb.ToString());

            // 2) Waterfall VFX audit: every spawned instance with its
            //    effective emitter width and particle cap.
            var vsb = new StringBuilder("name;posX;posY;posZ;scaleX;scaleY;scaleZ;system;shapeScaleX;maxParticles\n");
            foreach (var ps in UnityEngine.Object.FindObjectsByType<ParticleSystem>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var t = ps.transform;
                bool ours = t.name.StartsWith("wfall_", StringComparison.Ordinal)
                            || (t.parent != null && t.parent.name.StartsWith("wfall_", StringComparison.Ordinal));
                if (!ours) continue;
                var p = t.position;
                var s = t.localScale;
                vsb.Append(t.name).Append(';')
                   .Append(p.x.ToString("F2")).Append(';')
                   .Append(p.y.ToString("F2")).Append(';')
                   .Append(p.z.ToString("F2")).Append(';')
                   .Append(s.x.ToString("F2")).Append(';')
                   .Append(s.y.ToString("F2")).Append(';')
                   .Append(s.z.ToString("F2")).Append(';')
                   .Append(ps.name).Append(';')
                   .Append(ps.shape.scale.x.ToString("F2")).Append(';')
                   .Append(ps.main.maxParticles).Append('\n');
            }
            File.WriteAllText(Path.Combine(outDir, "waterfall-vfx.csv"), vsb.ToString());

            // 3) Tallest land-to-land wall: largest 4-neighbour surface
            //    drop where both cells are non-water.
            Vector3 wallLook = Vector3.zero, wallCam = Vector3.zero;
            bool haveWall = false;
            if (signal.SurfaceHeightMap != null && signal.TileMap != null)
            {
                float bestDrop = 0f;
                Vector2Int low = default, high = default;
                int dx = 0, dy = 0;
                for (int y = 0; y < signal.Height; y++)
                for (int x = 0; x < signal.Width; x++)
                {
                    if (IsWaterId(signal.TileMap[x, y])) continue;
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + (d == 0 ? 1 : d == 1 ? -1 : 0);
                        int ny = y + (d == 2 ? 1 : d == 3 ? -1 : 0);
                        if (nx < 0 || ny < 0 || nx >= signal.Width || ny >= signal.Height) continue;
                        if (IsWaterId(signal.TileMap[nx, ny])) continue;
                        float drop = signal.SurfaceHeightMap[x, y] - signal.SurfaceHeightMap[nx, ny];
                        if (drop > bestDrop)
                        {
                            bestDrop = drop; low = new Vector2Int(nx, ny); high = new Vector2Int(x, y);
                            dx = nx - x; dy = ny - y;
                        }
                    }
                }
                if (bestDrop > 0.5f)
                {
                    haveWall = true;
                    Vector3 lowW = new Vector3(low.x * cs, signal.SurfaceHeightMap[low.x, low.y], low.y * cs);
                    Vector3 highW = new Vector3(high.x * cs, signal.SurfaceHeightMap[high.x, high.y], high.y * cs);
                    var n = new Vector3(dx, 0f, dy);
                    wallLook = new Vector3(highW.x + n.x * 0.5f * cs, (lowW.y + highW.y) * 0.5f, highW.z + n.z * 0.5f * cs);
                    wallCam = lowW + n * (bestDrop * 1.6f + 5f) + Vector3.up * (bestDrop * 0.45f + 1f);
                    File.AppendAllText(Path.Combine(outDir, "artifact-notes.txt"),
                        $"wall: drop={bestDrop:F2} high={high}->{highW.y:F2} low={low}->{lowW.y:F2} dir={dx},{dy}\n");
                    ShotAt(targets, outDir, "wall_closeup",
                        wallCam, Quaternion.LookRotation(wallLook - wallCam, Vector3.up));
                }
            }

            // 4) Water at the map border: sheet edge at the world rim.
            if (signal.TileMap != null)
            {
                Vector2Int bw = new Vector2Int(-1, -1);
                for (int x = 0; x < signal.Width && bw.x < 0; x++)
                for (int y = 0; y < signal.Height; y++)
                    if (IsWaterId(signal.TileMap[x, y]) && y + 1 == signal.Height)
                    { bw = new Vector2Int(x, y); break; }
                if (bw.x >= 0)
                {
                    float wy = signal.SurfaceHeightMap != null ? signal.SurfaceHeightMap[bw.x, bw.y] : 0f;
                    var wpos = new Vector3(bw.x * cs, wy + 1.5f, (bw.y + 1) * cs + 9f);
                    ShotAt(targets, outDir, "border_water",
                        wpos, Quaternion.LookRotation(
                            new Vector3(bw.x * cs, wy - 0.5f, bw.y * cs) - wpos, Vector3.up));
                }
            }

            // 5) Clay pass: one opaque grey material on every slot kills
            //    all transparency/refraction/foam variables at once — any
            //    stripe or gap still visible is geometry, not shading.
            var clay = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            try
            {
                clay.color = new Color(0.62f, 0.6f, 0.55f);
                var saved = new Dictionary<MeshRenderer, Material[]>();
                foreach (var mf in targets)
                {
                    var mr = mf.GetComponent<MeshRenderer>();
                    if (mr == null || mr.sharedMaterials == null) continue;
                    saved[mr] = mr.sharedMaterials;
                    var repl = new Material[mr.sharedMaterials.Length];
                    for (int i = 0; i < repl.Length; i++) repl[i] = clay;
                    mr.sharedMaterials = repl;
                }
                try
                {
                    Vector3 center = signal.HasMapWorldBounds
                        ? signal.MapWorldBoundsCenter
                        : new Vector3(signal.Width * cs * 0.5f, 0f, signal.Height * cs * 0.5f);
                    float R = Mathf.Max(signal.Width, signal.Height) * cs;
                    ShotAt(targets, outDir, "clay_iso",
                        center + new Vector3(0f, R * 0.6f, -R * 0.6f),
                        Quaternion.LookRotation(center - (center + new Vector3(0f, R * 0.6f, -R * 0.6f)), Vector3.up));
                    if (haveWall)
                        ShotAt(targets, outDir, "clay_wall",
                            wallCam, Quaternion.LookRotation(wallLook - wallCam, Vector3.up));
                }
                finally
                {
                    foreach (var kv in saved) if (kv.Key != null) kv.Key.sharedMaterials = kv.Value;
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(clay); }
        }
        catch (Exception e)
        {
            File.AppendAllText("Library/ai/visual-smoke-errors.log", "DumpArtifacts: " + e + "\n");
        }
    }

    /// <summary>
    /// Decoration density forensics: a per-seed candidate/reject/placed
    /// table replayed through the canonical generator, plus a scene census
    /// of actually-spawned objects by asset/type prefix.
    /// </summary>
    static void DumpDecorations(string outDir, WorldGeneratedDataSignal signal)
    {
        try
        {
            // Scene census: every spawned decoration object by asset prefix.
            var census = new Dictionary<string, int>();
            int spawned = 0;
            foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.parent == null || !t.parent.name.StartsWith("EnvironmentDecorations"))
                    continue;
                string id = t.name;
                int us = id.LastIndexOf('_');
                if (us > 0) us = id.LastIndexOf('_', us - 1);
                if (us > 0) id = id.Substring(0, us);
                census[id] = census.TryGetValue(id, out int n) ? n + 1 : 1;
                spawned++;
            }
            var csb = new StringBuilder("assetId;count\n");
            foreach (var kv in census.OrderByDescending(k => k.Value))
                csb.Append(kv.Key).Append(';').Append(kv.Value).Append('\n');
            csb.Append("TOTAL;").Append(spawned).Append('\n');
            File.WriteAllText(Path.Combine(outDir, "decorations.csv"), csb.ToString());

            // Replay the placement stage for several seeds on the SAME map:
            // candidate/reject/placed-by-type table per seed.
            void Bail(string why)
                => File.AppendAllText("Library/ai/visual-smoke-errors.log",
                    "DumpDecorations bail: " + why + "\n");
            if (lastContainer == null) { Bail("container"); return; }
            var stateType = FindGeneratorType("Kruty1918.Moyva.Generator.Runtime.IMapVisualWorldState");
            var genType = FindGeneratorType("Kruty1918.Moyva.Generator.Runtime.EnvironmentDecorationGenerator");
            var statsType = FindGeneratorType("Kruty1918.Moyva.Generator.Runtime.DecorationPlacementStats");
            if (stateType == null || genType == null || statsType == null)
            {
                Bail($"types s={stateType != null} g={genType != null} t={statsType != null}");
                return;
            }
            object state = lastContainer.TryResolve(stateType);
            object gen = lastContainer.TryResolve(genType);
            if (state == null || gen == null)
            {
                Bail($"resolve s={state != null} g={gen != null}");
                return;
            }
            object[] gargs = { null };
            if (stateType.GetMethod("TryGetCurrentWorldData")?.Invoke(state, gargs) is not bool ok || !ok)
            { Bail("noWorldData"); return; }
            object worldData = gargs[0];
            if (worldData == null) { Bail("worldData-null"); return; }
            var seedField = worldData.GetType().GetField("Seed");
            var generate = genType.GetMethod("Generate");
            if (seedField == null || generate == null)
            { Bail($"members sp={seedField != null} gen={generate != null}"); return; }

            int[] seeds = { (int)seedField.GetValue(worldData), 6130, 777, 2024, 31337, 42 };
            var sb = new StringBuilder();
            var watch = new System.Diagnostics.Stopwatch();
            int origSeed = (int)seedField.GetValue(worldData);
            foreach (int s in seeds)
            {
                seedField.SetValue(worldData, s);
                object stats = Activator.CreateInstance(statsType, nonPublic: true);
                watch.Restart();
                object result = generate.Invoke(gen, new[] { worldData, stats });
                watch.Stop();
                int placed = statsType.GetProperty("PlacedTotal")?.GetValue(stats) is int p ? p : -1;
                int attempts = (int)GetMember(stats, "Attempts");
                int water = (int)GetMember(stats, "WaterCells");
                int sceneCount = result?.GetType().GetProperty("Count")?.GetValue(result) is int c ? c : -1;
                sb.Append($"seed={s} placements={sceneCount} attempts={attempts} waterCells={water} genMs={watch.ElapsedMilliseconds}\n  placed: ");
                var pbt = GetMember(stats, "PlacedByType") as System.Collections.IDictionary;
                if (pbt != null)
                    foreach (System.Collections.DictionaryEntry e in pbt)
                        sb.Append(e.Key).Append('=').Append(e.Value).Append(' ');
                sb.Append("\n  rejects: ");
                var rej = GetMember(stats, "Rejects") as System.Collections.IDictionary;
                if (rej != null)
                    foreach (System.Collections.DictionaryEntry e in rej)
                        sb.Append(e.Key).Append('=').Append(e.Value).Append(' ');
                sb.Append('\n');
            }
            seedField.SetValue(worldData, origSeed);
            File.WriteAllText(Path.Combine(outDir, "decoration-stats.txt"), sb.ToString());
        }
        catch (Exception e)
        {
            File.AppendAllText("Library/ai/visual-smoke-errors.log", "DumpDecorations: " + e + "\n");
        }
    }

    static void DumpWaterMaterial(string outDir, List<MeshFilter> targets)
    {
        try
        {
            var sb = new StringBuilder();
            Material water = null;
            int matCount = 0;
            var matNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var mf in targets)
            {
                if (mf == null) continue;
                var r = mf.GetComponent<MeshRenderer>();
                if (r == null) continue;
                foreach (var m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    matCount++;
                    matNames.Add(m.name);
                    string sn = m.shader != null ? m.shader.name : "";
                    if (water == null && (m.name.ToLowerInvariant().Contains("water")
                        || sn.ToLowerInvariant().Contains("water")))
                        water = m;
                }
            }
            sb.AppendLine($"materials={matCount} names=[{string.Join(", ", matNames)}]");
            if (water == null)
            {
                sb.AppendLine("waterMaterial=NONE");
            }
            else
            {
                sb.AppendLine($"waterMaterial={water.name} shader={water.shader.name}");
                sb.AppendLine($"keywords=[{string.Join(", ", water.enabledKeywords.Select(k => k.name))}]");
                string[] props =
                {
                    "_FogSource", "_DisableDepthTexture", "_DepthHorizontal",
                    "_DepthVertical", "_ShallowColor", "_BaseColor",
                    "_WaterColor", "_WaterShallowColor", "_HorizonColor",
                    "_IntersectionColor", "_IntersectionLength", "_FoamColor",
                    "_RefractionStrength", "_Direction", "_Speed", "_ColorAbsorption",
                    "_WaveTint", "_TranslucencyStrength", "_SunReflectionSize"
                };
                foreach (string p in props)
                {
                    if (!water.HasProperty(p)) continue;
                    var prop = p;
                    object val = null;
                    int idx = water.shader.FindPropertyIndex(prop);
                    var pt = water.shader.GetPropertyType(idx);
                    if (pt == UnityEngine.Rendering.ShaderPropertyType.Color
                        || pt == UnityEngine.Rendering.ShaderPropertyType.Vector)
                        val = water.GetVector(prop);
                    else if (pt == UnityEngine.Rendering.ShaderPropertyType.Float
                        || pt == UnityEngine.Rendering.ShaderPropertyType.Range)
                        val = water.GetFloat(prop);
                    sb.AppendLine($"{prop}={val}");
                }
                var depthTex = Shader.GetGlobalTexture("_CameraDepthTexture");
                sb.AppendLine($"globalDepthTextureBound={(depthTex != null)}");
                sb.AppendLine($"renderQueue={water.renderQueue}");
            }
            File.WriteAllText(Path.Combine(outDir, "watermat.txt"), sb.ToString());
        }
        catch (Exception e)
        {
            File.AppendAllText("Library/ai/visual-smoke-errors.log", "DumpWaterMaterial: " + e + "\n");
        }
    }

    static void DumpLogicalMap(string outDir, WorldGeneratedDataSignal signal)
    {
        if (lastLogicalMap == null) return;
        try
        {
            var mapType = lastLogicalMap.GetType();
            var layerNames = mapType.GetProperty("LayerNames")?.GetValue(lastLogicalMap) as string[,];
            var getCell = mapType.GetMethod("GetCellStack");
            if (layerNames != null)
            {
                var lm = new StringBuilder();
                for (int y = 0; y < signal.Height; y++)
                {
                    for (int x = 0; x < signal.Width; x++)
                    {
                        if (x > 0) lm.Append(',');
                        lm.Append(layerNames[x, y] ?? "");
                    }
                    lm.Append('\n');
                }
                File.WriteAllText(Path.Combine(outDir, "layernames.csv"), lm.ToString());
            }
            if (getCell == null || signal.HeightMap == null || signal.TileMap == null) return;
            var stacks = new StringBuilder();
            var waterStacks = new StringBuilder();
            for (int y = 0; y < signal.Height; y++)
            for (int x = 0; x < signal.Width; x++)
            {
                string id = signal.TileMap[x, y];
                float h = signal.HeightMap[x, y];
                float sv = signal.SurfaceHeightMap != null ? signal.SurfaceHeightMap[x, y] : float.NaN;
                bool sandCell = !string.IsNullOrEmpty(id)
                    && id.ToLowerInvariant().Contains("sand")
                    && (h > 1.05f || sv > 1.05f);
                bool waterCell = IsWaterWinner(layerNames?[x, y]) || IsWaterId(id);
                if (!sandCell && !waterCell) continue;
                object stack = getCell.Invoke(lastLogicalMap, new object[] { x, y });
                if (stack == null) continue;
                if (!(stack.GetType().GetProperty("Samples")?.GetValue(stack) is System.Collections.IList samples)) continue;
                var line = new StringBuilder();
                line.Append($"({x},{y}) tile={id} H={h:F2} S={sv:F2} winnerLayer={layerNames?[x, y]} :: ");
                foreach (var smp in samples)
                {
                    var st = smp.GetType();
                    line.Append('[')
                        .Append(st.GetProperty("LayerName")?.GetValue(smp)).Append('|')
                        .Append(st.GetProperty("TileId")?.GetValue(smp)).Append('|')
                        .Append(st.GetProperty("LayerKind")?.GetValue(smp)).Append('|')
                        .Append(st.GetProperty("TileGeometryMode")?.GetValue(smp)).Append('|')
                        .Append(string.Format("{0:F2}", st.GetProperty("Height")?.GetValue(smp))).Append('|')
                        .Append(string.Format("{0:F2}", st.GetProperty("SurfaceHeight")?.GetValue(smp)))
                        .Append("] ");
                }
                line.Append('\n');
                if (sandCell) stacks.Append(line);
                if (waterCell) waterStacks.Append(line);
            }
            File.WriteAllText(Path.Combine(outDir, "sandstack.txt"), stacks.ToString());
            File.WriteAllText(Path.Combine(outDir, "waterstack.txt"), waterStacks.ToString());
        }
        catch (Exception e)
        {
            File.AppendAllText("Library/ai/visual-smoke-errors.log", "DumpLogicalMap: " + e + "\n");
        }
    }

    static bool IsWaterWinner(string layerName)
    {
        if (string.IsNullOrEmpty(layerName)) return false;
        string n = layerName.ToLowerInvariant();
        return n.Contains("water") || n.Contains("river") || n.Contains("lake") || n.Contains("sea");
    }

    static void ShotCam(Camera cam, string outDir, string name, Vector3 pos, Quaternion rot,
        bool ortho, float orthoSize, float fov, int w = 1600, int h = 1200)
    {
        try
        {
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.orthographic = ortho;
            if (ortho) cam.orthographicSize = orthoSize;
            else cam.fieldOfView = fov;
            cam.transform.position = pos;
            cam.transform.rotation = rot;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
            byte[] png = tex.EncodeToPNG();
            UnityEngine.Object.Destroy(tex);
            rt.Release();
            string path = Path.GetFullPath(Path.Combine(outDir, name + ".png"));
            File.WriteAllBytes(path, png);
            File.AppendAllText("Library/ai/visual-smoke-shots.log", $"{name} {png.Length}B -> {path}\n");
        }
        catch (Exception e)
        {
            File.AppendAllText("Library/ai/visual-smoke-shots.log", $"{name} FAILED {e}\n");
        }
    }

    static ulong WorldHash(WorldGeneratedDataSignal signal)
    {
        const ulong prime = 1099511628211UL;
        ulong h = 14695981039346656037UL;
        for (int x = 0; x < signal.Width; x++)
        for (int y = 0; y < signal.Height; y++)
        {
            h = HashStr(h, signal.TileMap != null ? signal.TileMap[x, y] : null, prime);
            h = HashStr(h, signal.ObjectMap != null ? signal.ObjectMap[x, y] : null, prime);
            float hv = signal.HeightMap != null ? signal.HeightMap[x, y] : 0f;
            long q = (long)Mathf.RoundToInt(hv * 1000f);
            for (int b = 0; b < 8; b++) { h ^= (byte)(q >> (b * 8)); h *= prime; }
        }
        return h;
    }

    static ulong HashStr(ulong h, string s, ulong prime)
    {
        if (s == null) { h ^= 0xFF; return h * prime; }
        foreach (char c in s) { h ^= c; h *= prime; }
        h ^= 0xFE; return h * prime;
    }

    static bool HasDiffLandNeighbour(WorldGeneratedDataSignal signal, int x, int y)
    {
        string id = signal.TileMap[x, y];
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            int nx = x + dx, ny = y + dy;
            if (nx < 0 || ny < 0 || nx >= signal.Width || ny >= signal.Height) continue;
            string nid = signal.TileMap[nx, ny];
            if (!string.IsNullOrEmpty(nid) && !IsWaterId(nid) && nid != id) return true;
        }
        return false;
    }

    static void ShotAt(List<MeshFilter> targets, string outDir,
        string name, Vector3 pos, Quaternion rot, bool ortho = false, float orthoSize = 0f,
        int w = 1600, int h = 1200)
    {
        var pru = new PreviewRenderUtility();
        try
        {
            pru.camera.nearClipPlane = 0.1f;
            pru.camera.farClipPlane = 4000f;
            pru.camera.orthographic = ortho;
            if (ortho) pru.camera.orthographicSize = orthoSize;
            else pru.camera.fieldOfView = 55f;
            pru.camera.transform.position = pos;
            pru.camera.transform.rotation = rot;
            ShotPru(pru, targets, outDir, name, w, h);
        }
        finally
        {
            pru.Cleanup();
        }
    }

    static void CaptureGameShots(WorldGeneratedDataSignal signal)
    {
        string outDir = SessionState.GetString(Key + ".out", "Library/ai/shots");
        float cs = signal.CellSize <= 0.0001f ? 1f : signal.CellSize;
        Vector3 center = signal.HasMapWorldBounds
            ? signal.MapWorldBoundsCenter
            : new Vector3(signal.Width * cs * 0.5f, 0f, signal.Height * cs * 0.5f);
        Vector3 size = signal.HasMapWorldBounds
            ? signal.MapWorldBoundsSize
            : new Vector3(signal.Width * cs, 8f, signal.Height * cs);
        float R = Mathf.Max(size.x, size.z);

        var gameCam = Camera.main;
        if (gameCam == null)
            foreach (var c in UnityEngine.Object.FindObjectsByType<Camera>())
                if (c != null && c.enabled) { gameCam = c; break; }
        var note = new StringBuilder();
        if (gameCam != null)
        {
            var origPos = gameCam.transform.position;
            var origRot = gameCam.transform.rotation;
            bool origOrtho = gameCam.orthographic;
            float origSize = gameCam.orthographicSize;
            float origFov = gameCam.fieldOfView;
            note.AppendLine($"gamecam={gameCam.name} pos={origPos} rot={origRot.eulerAngles} ortho={origOrtho} size={origSize} fov={origFov} far={gameCam.farClipPlane}");

            ShotCam(gameCam, outDir, "game_default", origPos, origRot, origOrtho, origSize, origFov);
            ShotCam(gameCam, outDir, "game_topdown",
                center + Vector3.up * (size.y + R + 60f), Quaternion.Euler(90f, 0f, 0f), true, R * 0.55f, 0f);
            ShotCam(gameCam, outDir, "game_iso",
                center + new Vector3(0f, R * 0.75f, -R * 0.75f),
                Quaternion.LookRotation(center - (center + new Vector3(0f, R * 0.75f, -R * 0.75f)), Vector3.up),
                false, 0f, 55f);

            // Orbit the real game camera at a low gameplay-like angle around
            // the whole map — approximates inspection during camera movement.
            string[] orbitNames = { "game_orbit_n", "game_orbit_e", "game_orbit_s", "game_orbit_w" };
            Vector3[] orbitDirs = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = center + orbitDirs[i] * (R * 0.8f) + Vector3.up * (R * 0.45f);
                ShotCam(gameCam, outDir, orbitNames[i], pos,
                    Quaternion.LookRotation(center - pos, Vector3.up), false, 0f, 55f);
            }

            // Control render: authored square fill modules placed just south of
            // the map border, rendered through the preview path together with
            // the real terrain chunk meshes so a single frame compares the
            // imported FBX shape against the generated border tiles behind it.
            var control = SpawnControlFillRow(signal, cs,
                out Vector3 controlAim, out List<MeshFilter> controlMfs);
            if (controlMfs.Count > 0)
            {
                var ct = new List<MeshFilter>(controlMfs);
                foreach (var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>())
                    if (mf != null && mf.gameObject.name == "TerrainMesh") ct.Add(mf);
                ShotAt(ct, outDir, "control_fbx",
                    controlAim + new Vector3(0f, 9f, -5f),
                    Quaternion.Euler(62f, 0f, 0f));
                foreach (var go in control) UnityEngine.Object.Destroy(go);
            }

            Vector2Int shoreCell = new Vector2Int(-1, -1);
            for (int x = 0; x < signal.Width && shoreCell.x < 0; x++)
            for (int y = 0; y < signal.Height; y++)
                if (!IsWaterId(signal.TileMap[x, y]) && HasWaterNeighbour(signal, x, y))
                { shoreCell = new Vector2Int(x, y); break; }
            if (shoreCell.x >= 0)
            {
                Vector3 t = CellWorld(shoreCell, signal, cs);
                ShotCam(gameCam, outDir, "game_shore",
                    t + new Vector3(8f, 10f, -10f),
                    Quaternion.LookRotation(t - (t + new Vector3(8f, 10f, -10f)), Vector3.up),
                    false, 0f, 55f);
            }

            // Proof shots for the gameplay probe: a close orbit on each placed
            // building and spawned unit recorded by RunGameplayProbe.
            int bi = 0;
            foreach (Vector3 t in gameplayBuildingTargets)
            {
                Vector3 p = t + new Vector3(7f, 8f, -7f);
                ShotCam(gameCam, outDir, "game_building_" + (bi++), p,
                    Quaternion.LookRotation(t - p, Vector3.up), false, 0f, 55f);
            }
            int ui = 0;
            foreach (Vector3 t in gameplayUnitTargets)
            {
                Vector3 p = t + new Vector3(4f, 5f, -4f);
                ShotCam(gameCam, outDir, "game_unit_" + (ui++), p,
                    Quaternion.LookRotation(t - p, Vector3.up), false, 0f, 55f);
            }
            int mi = 0;
            foreach (Vector3 t in gameplayMoveTargets)
            {
                Vector3 p = t + new Vector3(4f, 5f, -4f);
                ShotCam(gameCam, outDir, "game_move_" + (mi++), p,
                    Quaternion.LookRotation(t - p, Vector3.up), false, 0f, 55f);
            }

            gameCam.transform.position = origPos;
            gameCam.transform.rotation = origRot;
            gameCam.orthographic = origOrtho;
            gameCam.orthographicSize = origSize;
            gameCam.fieldOfView = origFov;
        }
        else note.AppendLine("gamecam=none");
        File.WriteAllText(Path.Combine(outDir, "gamecam.txt"), note.ToString());
    }

    // Spawns a short row of the authored square fill prefab just south of the
    // map's minimum-Z border so a game-camera frame can compare the imported
    // FBX module against generated terrain tiles. Returns the spawned objects
    // so the caller can destroy them after the shot.
    static List<GameObject> SpawnControlFillRow(
        WorldGeneratedDataSignal signal, float cs,
        out Vector3 aim, out List<MeshFilter> filters)
    {
        var list = new List<GameObject>();
        filters = new List<MeshFilter>();
        aim = Vector3.zero;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Moyva/Art/World/Tiles/AtlasV3/Generated/Prefabs/grass_fill.prefab");
        if (prefab == null)
        {
            File.AppendAllText("Library/ai/visual-smoke-errors.log",
                "control: grass_fill prefab not found\n");
            return list;
        }

        // Lowest land surface height near the south border, so the authored
        // quads sit on roughly the same plane as flat generated tiles.
        float southZ = signal.HasMapWorldBounds
            ? signal.MapWorldBoundsCenter.z - signal.MapWorldBoundsSize.z * 0.5f
            : 0f;
        float baseY = 0f;
        for (int x = 0; x < signal.Width; x++)
        for (int y = 0; y < Mathf.Min(6, signal.Height); y++)
        {
            if (IsWaterId(signal.TileMap[x, y])) continue;
            float sv = signal.SurfaceHeightMap != null ? signal.SurfaceHeightMap[x, y] : 0f;
            baseY = sv; break;
        }

        float rowZ = southZ - 0.6f * cs;
        float startX = (signal.HasMapWorldBounds
            ? signal.MapWorldBoundsCenter.x : signal.Width * cs * 0.5f) - 1.5f * cs;
        for (int i = 0; i < 4; i++)
        {
            var go = UnityEngine.Object.Instantiate(prefab,
                new Vector3(startX + i * cs, baseY, rowZ),
                Quaternion.Euler(0f, 90f * (i % 4), 0f));
            go.transform.localScale = Vector3.one * cs;
            var mf = go.GetComponentInChildren<MeshFilter>();
            if (mf != null && mf.sharedMesh != null) filters.Add(mf);
            list.Add(go);
        }
        aim = new Vector3(startX + 1.5f * cs, baseY, rowZ);
        return list;
    }

    // Gameplay-level QA probe. Drives the canonical gameplay services (not a
    // parallel authority): authoritative setup placement, direct-place
    // validation rejections, recruitment eligibility/enqueue and the unit
    // factory. Results are written to gameplay.txt and the spawned entity world
    // positions are queued for dedicated proof shots in CaptureGameShots.
    static void RunGameplayProbe(WorldGeneratedDataSignal signal)
    {
        string outDir = SessionState.GetString(Key + ".out", "Library/ai/shots");
        var rep = new StringBuilder();
        gameplayBuildingTargets.Clear();
        gameplayUnitTargets.Clear();
        gameplayMoveTargets.Clear();
        try
        {
            if (lastContainer == null || signal.TileMap == null)
            {
                rep.AppendLine("no-container-or-signal");
                File.WriteAllText(Path.Combine(outDir, "gameplay.txt"), rep.ToString());
                return;
            }
            float cs = signal.CellSize <= 0.0001f ? 1f : signal.CellSize;

            // Resolve the concrete canonical services (interfaces are not in
            // TypeCache.GetTypesDerivedFrom<object>; the concrete types are).
            object session = TryResolveByName("Kruty1918.Moyva.Construction.Runtime.ConstructionService");
            object recruitment = TryResolveByName("Kruty1918.Moyva.Units.Runtime.UnitRecruitmentService");
            object unitFactory = TryResolveByName("Kruty1918.Moyva.Units.API.IUnitFactory");
            rep.AppendLine($"services session={(session != null)} recruitment={(recruitment != null)} unitFactory={(unitFactory != null)}");

            string ownerId = "player_0";
            if (session != null)
            {
                object o = session.GetType().GetMethod("GetActiveOwner")?.Invoke(session, null);
                if (o is string s && !string.IsNullOrWhiteSpace(s)) ownerId = s.Trim();
            }
            rep.AppendLine($"ownerId={ownerId}");

            var applySetup = session?.GetType().GetMethod("TryApplySetupPlacement",
                new[] { typeof(string), typeof(Vector2Int), typeof(string) });
            var directPlace = session?.GetType().GetMethod("TryDirectPlace",
                new[] { typeof(string), typeof(Vector2Int), typeof(string) });
            var lastMsg = session?.GetType().GetMethod("GetLastActionMessage");

            // Candidate cells: interior flat, dry, unoccupied land; plus one
            // water cell and the occupied cell for rejection probes.
            Vector2Int waterCell = new Vector2Int(-1, -1);
            var flatCells = new List<Vector2Int>();
            for (int x = 2; x < signal.Width - 2; x++)
            for (int y = 2; y < signal.Height - 2; y++)
            {
                if (IsWaterId(signal.TileMap[x, y]))
                {
                    if (waterCell.x < 0) waterCell = new Vector2Int(x, y);
                    continue;
                }
                if (signal.ObjectMap != null && !string.IsNullOrEmpty(signal.ObjectMap[x, y])) continue;
                if (!IsFlatCell(signal, x, y)) continue;
                flatCells.Add(new Vector2Int(x, y));
            }
            rep.AppendLine($"cells flat={flatCells.Count} waterCell={waterCell}");

            // Authoritative placement requires the cell inside owned/buildable
            // territory, so prefer flat land nearest the local player's spawn
            // hint rather than the geometric map centre.
            Vector2Int cx = new Vector2Int(signal.Width / 2, signal.Height / 2);
            if (signal.SpawnHints != null && signal.SpawnHints.Length > 0)
                cx = signal.SpawnHints[0];
            rep.AppendLine($"anchor={cx} spawnHints={(signal.SpawnHints?.Length ?? 0)}");
            flatCells.Sort((a, b) =>
                (a - cx).sqrMagnitude.CompareTo((b - cx).sqrMagnitude));

            // 1) Authoritative setup placement: the bootstrap rule requires the
            //    castle before any other building, so place castle-01 first,
            //    then a recruiting barrack inside its build radius.
            Vector2Int castleCell = new Vector2Int(-1, -1);
            if (applySetup != null)
            {
                for (int i = 0; i < flatCells.Count && castleCell.x < 0; i++)
                {
                    Vector2Int c = flatCells[i];
                    bool ok = (bool)(applySetup.Invoke(session, new object[] { "castle-01", c, ownerId }) ?? false);
                    if (ok) castleCell = c;
                    else if (i < 4) rep.AppendLine($"castle place try@{c} -> {InvokeMsg(lastMsg, session)} {EvaluateReason(session, "castle-01", c, ownerId)}");
                }
            }
            if (castleCell.x >= 0)
            {
                rep.AppendLine($"castle placed @{castleCell}");
                gameplayBuildingTargets.Add(CellWorld(castleCell, signal, cs));
            }
            else rep.AppendLine("castle placement FAILED");

            Vector2Int barrackCell = new Vector2Int(-1, -1);
            if (applySetup != null && castleCell.x >= 0)
            {
                // Barrack must sit inside the castle's build radius, so rank
                // candidates by distance to the placed castle, not the anchor.
                var nearCastle = new List<Vector2Int>(flatCells);
                nearCastle.Sort((a, b) =>
                    (a - castleCell).sqrMagnitude.CompareTo((b - castleCell).sqrMagnitude));
                for (int i = 0; i < nearCastle.Count && barrackCell.x < 0; i++)
                {
                    Vector2Int c = nearCastle[i];
                    if (c == castleCell) continue;
                    bool ok = (bool)(applySetup.Invoke(session, new object[] { "barrack", c, ownerId }) ?? false);
                    if (ok) barrackCell = c;
                }
            }
            if (barrackCell.x >= 0)
            {
                rep.AppendLine($"barrack placed @{barrackCell} castle@{castleCell}");
                gameplayBuildingTargets.Add(CellWorld(barrackCell, signal, cs));
            }
            else rep.AppendLine("barrack placement FAILED");

            // 2) Rejection probes: water + occupied cell must be refused.
            if (directPlace != null && waterCell.x >= 0)
            {
                bool ok = (bool)(directPlace.Invoke(session, new object[] { "farm", waterCell, ownerId }) ?? false);
                rep.AppendLine($"reject water farm @{waterCell} ok={ok} msg={InvokeMsg(lastMsg, session)}");
            }
            if (directPlace != null && barrackCell.x >= 0)
            {
                bool ok = (bool)(directPlace.Invoke(session, new object[] { "farm", barrackCell, ownerId }) ?? false);
                rep.AppendLine($"reject occupied farm @{barrackCell} ok={ok} msg={InvokeMsg(lastMsg, session)}");
            }

            // 3) Fast-forward the barrack to operational via the canonical
            //    save-restore path, then drive recruitment eligibility + enqueue.
            if (barrackCell.x >= 0)
            {
                object lifecycle = TryResolveByName("Kruty1918.Moyva.Construction.Runtime.ConstructionLifecycleService");
                var restore = lifecycle?.GetType().GetMethod("TryRestoreOperational");
                object ro = restore?.Invoke(lifecycle, new object[] { barrackCell });
                rep.AppendLine($"barrack operational restore={ro}");
            }

            if (recruitment != null && barrackCell.x >= 0)
            {
                var getOptions = recruitment.GetType().GetMethod("GetOptions");
                var opts = getOptions?.Invoke(recruitment, new object[] { ownerId }) as System.Collections.IList;
                rep.AppendLine($"recruit options={(opts != null ? opts.Count : -1)}");
                var tryEnqueue = recruitment.GetType().GetMethod("TryEnqueue");
                if (tryEnqueue != null)
                {
                    object[] args = { ownerId, barrackCell, "warrior", null };
                    bool ok = (bool)(tryEnqueue.Invoke(recruitment, args) ?? false);
                    rep.AppendLine($"enqueue warrior @{barrackCell} ok={ok} reason={args[3]}");
                }

                // Inspect the resulting queue + deployment surface. The unit
                // will not be ready inside the smoke window, but querying the
                // canonical queue/deploy APIs exercises the rest of the path.
                var getQueue = recruitment.GetType().GetMethod("GetQueue");
                var queue = getQueue?.Invoke(recruitment, new object[] { ownerId, barrackCell }) as System.Collections.IList;
                rep.AppendLine($"queue count={(queue != null ? queue.Count : -1)}");
                long qid = 0;
                if (queue != null && queue.Count > 0)
                {
                    object item = queue[0];
                    object q = item?.GetType().GetProperty("QueueId")?.GetValue(item)
                               ?? item?.GetType().GetField("QueueId")?.GetValue(item);
                    if (q != null) qid = System.Convert.ToInt64(q);
                }
                var getTiles = recruitment.GetType().GetMethod("GetDeploymentTiles");
                var tiles = getTiles?.Invoke(recruitment, new object[] { ownerId, barrackCell, qid }) as System.Collections.IList;
                rep.AppendLine($"deploy tiles={(tiles != null ? tiles.Count : -1)} qid={qid}");
                var tryDeploy = recruitment.GetType().GetMethod("TryDeployReady");
                if (tryDeploy != null && qid > 0)
                {
                    object[] dargs = { ownerId, barrackCell, qid, barrackCell, null, null };
                    bool ok = (bool)(tryDeploy.Invoke(recruitment, dargs) ?? false);
                    rep.AppendLine($"deploy ready qid={qid} ok={ok} unit={dargs[4]} reason={dargs[5]}");
                }
            }

            // 4) Direct unit factory spawn (visual proof) on a separate flat cell.
            string spawnedUnitId = null;
            Vector2Int spawnedUnitCell = new Vector2Int(-1, -1);
            if (unitFactory != null)
            {
                Vector2Int unitCell = new Vector2Int(-1, -1);
                foreach (Vector2Int c in flatCells)
                    if (c != barrackCell && c != castleCell) { unitCell = c; break; }
                if (unitCell.x >= 0)
                {
                    var create = unitFactory.GetType().GetMethod("CreateUnit",
                        new[] { typeof(string), typeof(Vector2Int), typeof(string) });
                    spawnedUnitId = create?.Invoke(unitFactory, new object[] { "warrior", unitCell, ownerId }) as string;
                    spawnedUnitCell = unitCell;
                    rep.AppendLine($"unit warrior @{unitCell} id={spawnedUnitId}");
                    if (spawnedUnitId != null) gameplayUnitTargets.Add(CellWorld(unitCell, signal, cs));
                }
                else rep.AppendLine("unit spawn: no free flat cell");
            }

            RunTurnDeployProbe(rep, signal, cs, ownerId, barrackCell);
            RunMovementProbe(rep, signal, cs, spawnedUnitId, spawnedUnitCell, waterCell);
            RunWallProbe(rep, signal, cs, ownerId, castleCell);
        }
        catch (Exception e)
        {
            rep.AppendLine("EX " + e);
            File.AppendAllText("Library/ai/visual-smoke-errors.log", "RunGameplayProbe: " + e + "\n");
        }
        File.WriteAllText(Path.Combine(outDir, "gameplay.txt"), rep.ToString());
    }

    // Wall/gate probe: lays a 6x6 wall perimeter with a replaced-in gate on
    // flat land plus a short run on uneven cells, exercises the canonical
    // gate-state service (closed/open/passability) and renders proof shots of
    // both gate visual states through the preview path.
    static void RunWallProbe(StringBuilder rep, WorldGeneratedDataSignal signal,
        float cs, string ownerId, Vector2Int castleCell)
    {
        string outDir = SessionState.GetString(Key + ".out", "Library/ai/shots");
        Vector2Int anchor = castleCell.x >= 0
            ? castleCell
            : new Vector2Int(signal.Width / 2, signal.Height / 2);
        object session = TryResolveByName("Kruty1918.Moyva.Construction.Runtime.ConstructionService");
        object topo = TryResolveByName("Kruty1918.Moyva.Construction.Runtime.WallTopologyService");
        object units = TryResolveByName("Kruty1918.Moyva.Units.Runtime.UnitService");
        rep.AppendLine($"wallprobe session={(session != null)} topo={(topo != null)} units={(units != null)}");
        if (session == null || topo == null) return;

        var applySetup = session.GetType().GetMethod("TryApplySetupPlacement",
            new[] { typeof(string), typeof(Vector2Int), typeof(string) });
        var isOpen = topo.GetType().GetMethod("IsGateOpen");
        var setOpen = topo.GetType().GetMethod("TrySetGateOpen");
        var canPass = topo.GetType().GetMethod("CanUnitPassGate");
        var canTraverse = units?.GetType().GetMethod("CanTraverseOccupiedConstructionCell");
        var lastMsg = session.GetType().GetMethod("GetLastActionMessage");
        if (applySetup == null || isOpen == null || setOpen == null)
        {
            rep.AppendLine("wallprobe: missing methods");
            return;
        }

        // Best 6x6 window inside the castle's Chebyshev-5 influence square:
        // walls require settlement influence, so the search is bounded to
        // windows fully inside it. Score prefers free dry flat border cells.
        int x0 = -1, y0 = -1, bestScore = -1;
        int ya = Mathf.Max(2, anchor.y - 5), yb = Mathf.Min(anchor.y, signal.Height - 8);
        int xa = Mathf.Max(2, anchor.x - 5), xb = Mathf.Min(anchor.x, signal.Width - 8);
        for (int y = ya; y <= yb; y++)
        for (int x = xa; x <= xb; x++)
        {
            int free = 0, flat = 0;
            for (int bx = x; bx < x + 6; bx++)
            for (int by = y; by < y + 6; by++)
            {
                bool border = bx == x || bx == x + 5 || by == y || by == y + 5;
                if (!border) continue;
                if (IsWaterId(signal.TileMap[bx, by])) continue;
                if (signal.ObjectMap != null && !string.IsNullOrEmpty(signal.ObjectMap[bx, by])) continue;
                free++;
                if (IsFlatCell(signal, bx, by)) flat++;
            }
            int score = free * 100 + flat
                - (new Vector2Int(x + 2, y + 2) - anchor).sqrMagnitude;
            if (score > bestScore) { bestScore = score; x0 = x; y0 = y; }
        }
        rep.AppendLine($"wallprobe window=({x0},{y0}) score={bestScore}");

        var placed = new List<Vector2Int>();
        Vector2Int gateCell = new Vector2Int(-1, -1);
        if (x0 >= 0)
        {
            // Placement requires visible tiles: reveal the window area through
            // the canonical fog service, then ensure an influence center exists
            // inside the perimeter (castle doubles as the test anchor).
            object fog = TryResolveByName("Kruty1918.Moyva.FogOfWar.Runtime.FogOfWarService");
            var revealArea = fog?.GetType().GetMethod("RevealArea",
                new[] { typeof(Vector2Int), typeof(int),
                    typeof(Kruty1918.Moyva.FogOfWar.API.FogRevealShape),
                    typeof(bool), typeof(string) });
            revealArea?.Invoke(fog, new object[] {
                new Vector2Int(x0 + 2, y0 + 2), 10,
                Kruty1918.Moyva.FogOfWar.API.FogRevealShape.PixelCircle,
                true, "wall-smoke" });
            rep.AppendLine($"wallprobe fogReveal={(revealArea != null)}");

            // No castle placed by the earlier probe → anchor one inside the
            // window interior so wall cells fall inside its influence.
            if (castleCell.x < 0)
            {
                for (int bx = x0 + 1; bx < x0 + 5; bx++)
                for (int by = y0 + 1; by < y0 + 5; by++)
                {
                    var c = new Vector2Int(bx, by);
                    if (castleCell.x >= 0) break;
                    if (IsWaterId(signal.TileMap[bx, by])) continue;
                    if (signal.ObjectMap != null && !string.IsNullOrEmpty(signal.ObjectMap[bx, by])) continue;
                    if ((bool)(applySetup.Invoke(session,
                            new object[] { "castle-01", c, ownerId }) ?? false))
                    {
                        castleCell = c;
                        rep.AppendLine($"wallprobe castle placed @{c}");
                    }
                }
            }

            gateCell = new Vector2Int(x0 + 2, y0);
            int okCount = 0, failCount = 0;
            for (int bx = x0; bx < x0 + 6; bx++)
            for (int by = y0; by < y0 + 6; by++)
            {
                bool border = bx == x0 || bx == x0 + 5 || by == y0 || by == y0 + 5;
                if (!border) continue;
                var cell = new Vector2Int(bx, by);
                bool ok = (bool)(applySetup.Invoke(session,
                    new object[] { "stone-wall", cell, ownerId }) ?? false);
                if (ok) { okCount++; placed.Add(cell); }
                else
                {
                    failCount++;
                    string reason = InvokeMsg(lastMsg, session);
                    if (string.IsNullOrEmpty(reason))
                        reason = EvaluateReason(session, "stone-wall", cell, ownerId);
                    rep.AppendLine($"wall @{cell} FAILED {reason}");
                }
            }
            rep.AppendLine($"wallprobe walls placed={okCount} failed={failCount}");

            // Gate replaces a south-edge wall segment (canonical replacement);
            // fall back to any placed segment if the preferred cell failed.
            if (!placed.Contains(gateCell))
            {
                gateCell = new Vector2Int(-1, -1);
                foreach (var c in placed)
                    if (c.y == y0) { gateCell = c; break; }
                if (gateCell.x < 0 && placed.Count > 0)
                    gateCell = placed[placed.Count / 2];
            }
            bool gok = gateCell.x >= 0 && (bool)(applySetup.Invoke(session,
                new object[] { "stone-gate", gateCell, ownerId }) ?? false);
            rep.AppendLine($"wallprobe gate @{gateCell} ok={gok} {InvokeMsg(lastMsg, session)}");
            if (gok && !placed.Contains(gateCell)) placed.Add(gateCell);
        }

        // Uneven-terrain rejection probe: stone-wall requires flat ground, so
        // sloped cells must fail with the canonical buildability rejection.
        if (signal.TerrainLevelMap != null)
        {
            int tested = 0, okCount = 0;
            for (int y = 2; y < signal.Height - 3 && tested < 4; y++)
            for (int x = 2; x < signal.Width - 2 && tested < 4; x++)
            {
                if (IsWaterId(signal.TileMap[x, y])) continue;
                if (signal.ObjectMap != null && !string.IsNullOrEmpty(signal.ObjectMap[x, y])) continue;
                if (IsFlatCell(signal, x, y)) continue;
                var cell = new Vector2Int(x, y);
                bool ok = (bool)(applySetup.Invoke(session,
                    new object[] { "stone-wall", cell, ownerId }) ?? false);
                rep.AppendLine($"wall uneven @{cell} ok={ok} {EvaluateReason(session, "stone-wall", cell, ownerId)}");
                tested++;
                if (ok) { okCount++; placed.Add(cell); }
            }
            rep.AppendLine($"wallprobe uneven placed={okCount}/tested={tested}");
        }

        // Fresh placements render as construction scaffolding; fast-forward to
        // operational via the canonical lifecycle restore so the real wall
        // meshes appear.
        object lifecycle = TryResolveByName("Kruty1918.Moyva.Construction.Runtime.ConstructionLifecycleService");
        var restore = lifecycle?.GetType().GetMethod("TryRestoreOperational");
        if (restore != null)
        {
            int restored = 0;
            foreach (var c in placed)
                if ((bool)(restore.Invoke(lifecycle, new object[] { c }) ?? false))
                    restored++;
            rep.AppendLine($"wallprobe restored={restored}/{placed.Count}");
        }

        // Gate state + traversal through the canonical services.
        if (gateCell.x >= 0)
        {
            rep.AppendLine($"gate IsGateOpen={(bool)isOpen.Invoke(topo, new object[] { gateCell })}");
            object[] passArgs = { gateCell, ownerId, null };
            rep.AppendLine($"gate CanUnitPassGate(owner)={(bool)canPass.Invoke(topo, passArgs)} reason={passArgs[2]}");
            object[] foeArgs = { gateCell, "enemy_9", null };
            rep.AppendLine($"gate CanUnitPassGate(enemy)={(bool)canPass.Invoke(topo, foeArgs)} reason={foeArgs[2]}");
            if (canTraverse != null && placed.Count > 1)
            {
                object[] tArgs = { "probe-unit", placed[0], false, null };
                rep.AppendLine($"wall traverse={(bool)canTraverse.Invoke(units, tArgs)} reason={tArgs[3]}");
            }
        }

        // Proof shots: wall MeshFilters (active children only — the inactive
        // gate state mesh must not render) plus terrain for context.
        var wallMfs = CollectWallMeshFilters();
        var targets = new List<MeshFilter>(wallMfs);
        foreach (var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>())
            if (mf != null && mf.gameObject.name == "TerrainMesh") targets.Add(mf);
        rep.AppendLine($"wallprobe meshfilters walls={wallMfs.Count} total={targets.Count}");

        if (placed.Count > 0)
        {
            Vector3 aim = gateCell.x >= 0
                ? CellWorld(gateCell, signal, cs)
                : CellWorld(placed[placed.Count / 2], signal, cs);
            aim += new Vector3(0.5f * cs, 0f, 0.5f * cs);
            Vector3 center = CellWorld(placed[0], signal, cs);
            if (x0 >= 0)
                center = CellWorld(new Vector2Int(x0 + 2, y0 + 2), signal, cs) + new Vector3(0.5f * cs, 0f, 0.5f * cs);

            ShotAt(targets, outDir, "wall_topdown",
                center + Vector3.up * 26f, Quaternion.Euler(90f, 0f, 0f),
                ortho: true, orthoSize: 6.5f * cs);
            ShotAt(targets, outDir, "wall_iso",
                center + new Vector3(9f, 10f, -9f),
                Quaternion.LookRotation(center - (center + new Vector3(9f, 10f, -9f)), Vector3.up));
            ShotAt(targets, outDir, "wall_gate_closed",
                aim + new Vector3(4.5f, 4f, -5.5f),
                Quaternion.LookRotation(aim + Vector3.up * 0.8f - (aim + new Vector3(4.5f, 4f, -5.5f)), Vector3.up));
            ShotAt(targets, outDir, "wall_side_south",
                center + new Vector3(0f, 4.5f, -11f),
                Quaternion.LookRotation(new Vector3(center.x, center.y + 0.8f, center.z) - (center + new Vector3(0f, 4.5f, -11f)), Vector3.up));

            // Open the gate through the canonical service and reshoot.
            if (gateCell.x >= 0)
            {
                object[] oArgs = { gateCell, true, 0f, null };
                bool opened = (bool)setOpen.Invoke(topo, oArgs);
                rep.AppendLine($"gate TrySetOpen={opened} IsOpen={(bool)isOpen.Invoke(topo, new object[] { gateCell })} reason={oArgs[3]}");
                var openTargets = new List<MeshFilter>(CollectWallMeshFilters());
                foreach (var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>())
                    if (mf != null && mf.gameObject.name == "TerrainMesh") openTargets.Add(mf);
                ShotAt(openTargets, outDir, "wall_gate_open",
                    aim + new Vector3(4.5f, 4f, -5.5f),
                    Quaternion.LookRotation(aim + Vector3.up * 0.8f - (aim + new Vector3(4.5f, 4f, -5.5f)), Vector3.up));
                if (canTraverse != null)
                {
                    object[] tArgs = { "probe-unit", gateCell, false, null };
                    rep.AppendLine($"gate traverse open={(bool)canTraverse.Invoke(units, tArgs)} reason={tArgs[3]}");
                }
                object[] cArgs = { gateCell, false, 0f, null };
                setOpen.Invoke(topo, cArgs);
            }
        }
    }

    // Diagnostic: re-runs the canonical placement query to recover the
    // rejection reason code when a wall placement fails silently.
    static string EvaluateReason(object session, string buildingId,
        Vector2Int cell, string ownerId)
    {
        try
        {
            var req = new Kruty1918.Moyva.Construction.API.ConstructionPlacementQueryRequest(
                buildingId, cell, includeDetails: true, ownerId: ownerId);
            object res = session.GetType().GetMethod("EvaluatePlacement")
                ?.Invoke(session, new object[] { req });
            if (res == null) return "eval=null";
            var t = res.GetType();
            var sb = new StringBuilder("eval");
            foreach (string p in new[] { "CanCommit", "AvailabilityValid",
                "SpatialValid", "ResourcesValid", "AuthorityValid",
                "Reason", "ReasonCode", "IsGateReplacement" })
            {
                object v = t.GetProperty(p)?.GetValue(res);
                if (v != null) sb.Append(' ').Append(p).Append('=').Append(v);
            }
            return sb.ToString();
        }
        catch (Exception e) { return "eval-ex " + e.Message; }
    }

    // Active-in-hierarchy MeshFilters under every placed wall/gate visual
    // (placed visuals are named Building_{buildingId}_{x}_{y}).
    static List<MeshFilter> CollectWallMeshFilters()
    {
        var list = new List<MeshFilter>();
        foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>())
        {
            if (t == null
                || (!t.name.StartsWith("Building_stone-wall_", StringComparison.Ordinal)
                    && !t.name.StartsWith("Building_stone-gate_", StringComparison.Ordinal)))
                continue;
            foreach (var mf in t.GetComponentsInChildren<MeshFilter>())
                if (mf != null && mf.gameObject.activeInHierarchy && mf.sharedMesh != null)
                    list.Add(mf);
        }
        return list;
    }

    // Advances the canonical turn service so a queued recruitment job completes
    // a training turn, then re-exercises the deployment surface end-to-end.
    // Ends whichever faction is active each step so a full round can elapse.
    static void RunTurnDeployProbe(StringBuilder rep, WorldGeneratedDataSignal signal,
        float cs, string ownerId, Vector2Int barrackCell)
    {
        if (barrackCell.x < 0) { rep.AppendLine("turn/deploy: no barrack"); return; }
        object turns = TryResolveByName("Kruty1918.Moyva.Turns.Runtime.TurnService");
        object recruitment = TryResolveByName("Kruty1918.Moyva.Units.Runtime.UnitRecruitmentService");
        rep.AppendLine($"turnsvc={(turns != null)}");
        if (turns == null || recruitment == null) return;

        var tt = turns.GetType();
        var roundP = tt.GetProperty("Round");
        var gturnP = tt.GetProperty("GlobalTurn");
        var activeP = tt.GetProperty("ActiveOwnerId");
        var tryEnd = tt.GetMethod("TryEndTurn",
            new[] { typeof(string), typeof(string).MakeByRefType() });
        var rt = recruitment.GetType();
        var getQueue = rt.GetMethod("GetQueue");
        var getTiles = rt.GetMethod("GetDeploymentTiles");
        var tryDeploy = rt.GetMethod("TryDeployReady");

        System.Collections.IList queue = getQueue?.Invoke(recruitment, new object[] { ownerId, barrackCell }) as System.Collections.IList;
        long qid = 0;
        if (queue != null && queue.Count > 0)
        {
            object item = queue[0];
            object q = GetMember(item, "QueueId");
            if (q != null) qid = System.Convert.ToInt64(q);
            rep.AppendLine($"preTurn qid={qid} done={GetMember(item, "CompletedTurns")}/{GetMember(item, "TrainingTurns")} ready={GetMember(item, "IsReady")}");
        }

        bool ready = false;
        for (int t = 0; t < 10 && qid > 0 && tryEnd != null; t++)
        {
            string requester = activeP?.GetValue(turns) as string;
            if (string.IsNullOrWhiteSpace(requester)) requester = ownerId;
            object[] eargs = { requester, null };
            bool ok = (bool)(tryEnd.Invoke(turns, eargs) ?? false);
            queue = getQueue?.Invoke(recruitment, new object[] { ownerId, barrackCell }) as System.Collections.IList;
            object it = queue != null && queue.Count > 0 ? queue[0] : null;
            ready = it != null && GetMember(it, "IsReady") is bool rb && rb;
            rep.AppendLine($"endTurn#{t} req={requester} ok={ok} reason={eargs[1]} round={roundP?.GetValue(turns)} gturn={gturnP?.GetValue(turns)} active={activeP?.GetValue(turns)} done={(it != null ? GetMember(it, "CompletedTurns") : "-")} ready={ready}");
            if (ready || !ok) break;
        }

        var tiles = getTiles?.Invoke(recruitment, new object[] { ownerId, barrackCell, qid }) as System.Collections.IList;
        rep.AppendLine($"deploy tiles={(tiles != null ? tiles.Count : -1)} qid={qid} ready={ready}");
        if (tryDeploy == null || qid <= 0) return;
        Vector2Int deployCell = barrackCell;
        if (tiles != null)
            foreach (var tile in tiles)
                if (GetMember(tile, "IsValid") is bool v && v) { deployCell = ToCell(tile); break; }
        object[] dargs = { ownerId, barrackCell, qid, deployCell, null, null };
        bool deployed = (bool)(tryDeploy.Invoke(recruitment, dargs) ?? false);
        rep.AppendLine($"deploy ready qid={qid} @{deployCell} ok={deployed} unit={dargs[4]} reason={dargs[5]}");
        if (deployed && dargs[4] is string du && !string.IsNullOrWhiteSpace(du))
            gameplayUnitTargets.Add(CellWorld(deployCell, signal, cs));
    }

    // Exercises the canonical movement surface for a spawned unit: the reachable
    // set (water must be excluded), per-step traversal validation (flat step
    // allowed, water rejected) and terrain-transition classification, then kicks
    // a real MoveUnitAsync to a reachable cell for visual proof.
    static void RunMovementProbe(StringBuilder rep, WorldGeneratedDataSignal signal,
        float cs, string unitId, Vector2Int unitCell, Vector2Int waterCell)
    {
        if (string.IsNullOrWhiteSpace(unitId)) { rep.AppendLine("movement: no unit"); return; }
        object units = TryResolveByName("Kruty1918.Moyva.Units.Runtime.UnitService");
        object traversal = TryResolveByName("Kruty1918.Moyva.Units.Runtime.UnitTraversalPolicy");
        // The public movement surface is bound via its interfaces, not the
        // concrete UnitTurnAuthorityMovementService, so resolve the contracts.
        object moveQuery = TryResolveByName("Kruty1918.Moyva.Units.API.IUnitMovementQuery");
        object movement = TryResolveByName("Kruty1918.Moyva.Units.API.IUnitMovementService");
        rep.AppendLine($"move services units={(units != null)} traversal={(traversal != null)} moveQuery={(moveQuery != null)} movement={(movement != null)}");
        Type modeType = FindType("Kruty1918.Moyva.Units.API.UnitTraversalMode");
        object pathMode = modeType != null ? Enum.ToObject(modeType, 1) : 1;

        Vector2Int from = unitCell;
        if (units != null)
        {
            var getPos = units.GetType().GetMethod("TryGetUnitPosition");
            if (getPos != null)
            {
                object[] pargs = { unitId, from };
                if (getPos.Invoke(units, pargs) is bool gp && gp) from = (Vector2Int)pargs[1];
            }
        }
        rep.AppendLine($"unit @{from}");

        // Reachable set: every tile the movement query reports, with a count of
        // reachable vs blocked and a water cell that must NOT be reachable.
        if (moveQuery != null)
        {
            var getTiles = moveQuery.GetType().GetMethod("GetMovementTiles");
            var tiles = getTiles?.Invoke(moveQuery, new object[] { unitId }) as System.Collections.IList;
            int reachable = 0, blocked = 0;
            Vector2Int moveTarget = new Vector2Int(-1, -1);
            bool waterReachable = false, sawWater = false;
            if (tiles != null)
            {
                foreach (var t in tiles)
                {
                    bool isR = GetMember(t, "IsReachable") is bool r && r;
                    if (isR) reachable++; else blocked++;
                    Vector2Int cell = ToCell(t);
                    if (IsWaterId(signal.TileMap[cell.x, cell.y]))
                    {
                        sawWater = true;
                        if (isR) waterReachable = true;
                    }
                    else if (isR && moveTarget.x < 0) moveTarget = cell;
                }
            }
            rep.AppendLine($"moveTiles total={(tiles != null ? tiles.Count : -1)} reachable={reachable} blocked={blocked} waterInSet={sawWater} waterReachable={waterReachable}");

            // Per-step validation: an orthogonal flat land step must pass, a
            // water step must be refused with a reason.
            if (traversal != null)
            {
                var tryStep = traversal.GetType().GetMethod("TryEvaluateStep");
                var tryTrans = traversal.GetType().GetMethod("TryEvaluateTransition");
                if (tryStep != null)
                {
                    foreach (var d in OrthoDirs())
                    {
                        Vector2Int to = from + d;
                        if (to.x < 0 || to.y < 0 || to.x >= signal.Width || to.y >= signal.Height) continue;
                        object[] sargs = { unitId, from, to, 8f, pathMode, null, null };
                        bool sok = (bool)(tryStep.Invoke(traversal, sargs) ?? false);
                        string kind = "";
                        if (tryTrans != null)
                        {
                            object[] targs = { from, to, "ground-default", null };
                            tryTrans.Invoke(traversal, targs);
                            kind = GetMember(targs[3], "Kind")?.ToString() ?? "";
                        }
                        rep.AppendLine($"step {from}->{to} tile={signal.TileMap[to.x, to.y]} ok={sok} cost={sargs[5]} reason={sargs[6]} trans={kind}");
                    }
                    if (waterCell.x >= 0)
                    {
                        // Force a water evaluation regardless of adjacency by
                        // stepping a neighbour of a water cell into it.
                        Vector2Int adj = from;
                        foreach (var d in OrthoDirs())
                        {
                            Vector2Int n = waterCell + d;
                            if (n.x >= 0 && n.y >= 0 && n.x < signal.Width && n.y < signal.Height
                                && !IsWaterId(signal.TileMap[n.x, n.y])) { adj = n; break; }
                        }
                        object[] sargs = { unitId, adj, waterCell, 8f, pathMode, null, null };
                        bool sok = (bool)(tryStep.Invoke(traversal, sargs) ?? false);
                        rep.AppendLine($"step water {adj}->{waterCell} ok={sok} cost={sargs[5]} reason={sargs[6]}");
                    }
                }
            }

            // Kick the real movement to a reachable cell for visual proof; the
            // animation runs over the pre-capture window, so we record the
            // destination for a dedicated shot rather than blocking.
            if (moveTarget.x >= 0 && movement != null)
            {
                var moveAsync = movement.GetType().GetMethod("MoveUnitAsync");
                moveAsync?.Invoke(movement, new object[] { unitId, moveTarget, System.Threading.CancellationToken.None });
                rep.AppendLine($"move issued @{moveTarget}");
                gameplayMoveTargets.Add(CellWorld(moveTarget, signal, cs));
            }
        }
    }

    static Vector2Int[] OrthoDirs()
        => new[] { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

    static object GetMember(object o, string name)
    {
        if (o == null) return null;
        var t = o.GetType();
        var p = t.GetProperty(name);
        if (p != null) return p.GetValue(o);
        var f = t.GetField(name);
        return f != null ? f.GetValue(o) : null;
    }

    static Vector2Int ToCell(object o)
    {
        if (o is Vector2Int v) return v;
        object p = GetMember(o, "Position");
        return p is Vector2Int c ? c : new Vector2Int(-1, -1);
    }

    static bool IsFlatCell(WorldGeneratedDataSignal signal, int x, int y)
    {
        if (signal.TerrainLevelMap == null) return true;
        int lv = signal.TerrainLevelMap[x, y];
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            int nx = x + dx, ny = y + dy;
            if (nx < 0 || ny < 0 || nx >= signal.Width || ny >= signal.Height) continue;
            if (signal.TerrainLevelMap[nx, ny] != lv) return false;
        }
        return true;
    }

    static string InvokeMsg(System.Reflection.MethodInfo m, object target)
        => m?.Invoke(target, null) as string ?? "";

    // Cross-assembly type lookup: the gameplay services live in feature
    // assemblies the editor assembly cannot name directly. Uses the editor-safe
    // loaded-assembly list rather than AppDomain.GetAssemblies (UAC0005).
    static Type FindType(string fullName)
    {
        var assemblies = new HashSet<System.Reflection.Assembly>();
        foreach (var t in UnityEditor.TypeCache.GetTypesDerivedFrom<object>())
        {
            if (t.FullName == fullName) return t;
            if (t.Assembly != null) assemblies.Add(t.Assembly);
        }
        // Interfaces are absent from GetTypesDerivedFrom<object>; find them by
        // scanning the assemblies of the cached types instead of AppDomain.
        foreach (var a in assemblies)
        {
            var t = a.GetType(fullName);
            if (t != null) return t;
        }
        return null;
    }

    // Zenject bindings for construction/units may live in a sibling context
    // rather than the world-generation SceneContext, so resolve across every
    // context container until the service is found.
    static object TryResolveByName(string fullName)
    {
        var t = FindType(fullName);
        if (t == null) return null;
        if (lastContainer != null)
        {
            object o = lastContainer.TryResolve(t);
            if (o != null) return o;
        }
        foreach (var sc in UnityEngine.Object.FindObjectsByType<SceneContext>())
        {
            object o = sc.Container?.TryResolve(t);
            if (o != null) return o;
        }
        foreach (var gc in UnityEngine.Object.FindObjectsByType<GameObjectContext>())
        {
            object o = gc.Container?.TryResolve(t);
            if (o != null) return o;
        }
        return null;
    }

    static bool HasWaterNeighbour(WorldGeneratedDataSignal signal, int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            int nx = x + dx, ny = y + dy;
            if (nx < 0 || ny < 0 || nx >= signal.Width || ny >= signal.Height) continue;
            if (IsWaterId(signal.TileMap[nx, ny])) return true;
        }
        return false;
    }

    static int CountLayerNeighbours(string[,] layerNames, int x, int y, int w, int h, string name)
    {
        int n = 0;
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0) continue;
            int nx = x + dx, ny = y + dy;
            if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
            if (layerNames[nx, ny] == name) n++;
        }
        return n;
    }

    static Vector3 CellWorld(Vector2Int cell, WorldGeneratedDataSignal signal, float cs)
    {
        float h = signal.HeightMap != null ? signal.HeightMap[cell.x, cell.y] : 0f;
        return new Vector3(cell.x * cs, h, cell.y * cs);
    }

    static void ShotPru(PreviewRenderUtility pru, List<MeshFilter> targets, string outDir,
        string name, int w = 1600, int h = 1200)
    {
        try
        {
            pru.BeginStaticPreview(new Rect(0, 0, w, h));
            foreach (var mf in targets)
            {
                var mesh = mf.sharedMesh;
                var renderer = mf.GetComponent<MeshRenderer>();
                var mats = renderer != null ? renderer.sharedMaterials : null;
                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    var mat = mats != null && s < mats.Length ? mats[s] : null;
                    if (mat == null) mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    pru.DrawMesh(mesh, mf.transform.localToWorldMatrix, mat, s);
                }
            }
            pru.camera.Render();
            var tex = pru.EndStaticPreview();
            byte[] png = tex.EncodeToPNG();
            UnityEngine.Object.Destroy(tex);
            string path = Path.GetFullPath(Path.Combine(outDir, name + ".png"));
            File.WriteAllBytes(path, png);
            File.AppendAllText("Library/ai/visual-smoke-shots.log",
                $"{name} {png.Length}B targets={targets.Count} -> {path}\n");
        }
        catch (Exception e)
        {
            File.AppendAllText("Library/ai/visual-smoke-shots.log", $"{name} FAILED {e}\n");
        }
    }

    static void Finish(string result)
    {
        File.WriteAllText("Library/ai/visual-smoke.summary", result);
        EditorApplication.isPlaying = false;
    }
}
#endif
