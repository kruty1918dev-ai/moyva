#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
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
                bool waterCell = IsWaterWinner(layerNames?[x, y]);
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

            gameCam.transform.position = origPos;
            gameCam.transform.rotation = origRot;
            gameCam.orthographic = origOrtho;
            gameCam.orthographicSize = origSize;
            gameCam.fieldOfView = origFov;
        }
        else note.AppendLine("gamecam=none");
        File.WriteAllText(Path.Combine(outDir, "gamecam.txt"), note.ToString());
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
