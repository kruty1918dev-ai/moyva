using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.JsonConfig;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.ObjectsMap.Runtime;
using Kruty1918.Moyva.Pathfinding.Runtime;
using Kruty1918.Moyva.Shared.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityHTML.Runtime;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Kruty1918.Moyva.Tests.Performance
{
    /// <summary>
    /// One-shot performance audit driver. Run headless:
    /// Unity -batchmode -projectPath . -executeMethod
    ///   Kruty1918.Moyva.Tests.Performance.MoyvaPerformanceAudit.RunAll -quit
    /// Writes Temp/ai/perf-results.json plus a summary block in the editor log.
    /// Every measurement is isolated: a failure records an error, never aborts.
    /// </summary>
    public static class MoyvaPerformanceAudit
    {
        private sealed class Row
        {
            public string Category, Name;
            public int Iterations;
            public double MinMs, MedianMs, MeanMs;
            public long AllocBytes;
            public string Note = "", Error = "";
        }

        private static readonly List<Row> Rows = new List<Row>();

        [MenuItem("Moyva/Audit/Run Performance Audit")]
        public static void RunAll()
        {
            var suite = Stopwatch.StartNew();
            Rows.Clear();
            Debug.Log("[PerfAudit] === suite start ===");

            Try(JsonAndConfig); Try(MenuMarkup); Try(MenuXmlParse); Try(MenuMount);
            Try(ControlsEditorCosts); Try(MenuPreview); Try(WorldDataIntegrity);
            Try(GridAndPath); Try(FogAndTexture); Try(ObjectChurn); Try(ZenjectCosts);

            suite.Stop();
            var path = Path.GetFullPath("Temp/ai/perf-results.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, ToJson());
            Debug.Log("[PerfAudit] === suite done in " + suite.Elapsed.TotalSeconds.ToString("0.0") + "s ===\n" + Summary());
            Debug.Log("[PerfAudit] results: " + path);
        }

        private static void Try(Action suite)
        {
            try { suite(); }
            catch (Exception ex)
            {
                Rows.Add(new Row { Category = "suite", Name = suite.Method.Name, Error = ex.GetBaseException().Message });
            }
        }

        // ---------- measurement helpers ----------

        private static void Measure(string category, string name, int iterations, Action action, string note = "")
        {
            var row = new Row { Category = category, Name = name, Iterations = iterations, Note = note };
            Rows.Add(row);
            try
            {
                action(); // warmup / JIT
                var times = new List<double>(iterations);
                GC.Collect();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < iterations; i++)
                {
                    var sw = Stopwatch.StartNew();
                    action();
                    sw.Stop();
                    times.Add(sw.Elapsed.TotalMilliseconds);
                }
                row.AllocBytes = GC.GetAllocatedBytesForCurrentThread() - before;
                times.Sort();
                row.MinMs = times[0];
                row.MedianMs = times[times.Count / 2];
                row.MeanMs = times.Average();
            }
            catch (Exception ex) { row.Error = ex.GetBaseException().Message; }
        }

        private static void MeasureCold(string category, string name, Action action, string note = "")
        {
            var row = new Row { Category = category, Name = name, Iterations = 1, Note = note };
            Rows.Add(row);
            try
            {
                long before = GC.GetAllocatedBytesForCurrentThread();
                var sw = Stopwatch.StartNew();
                action();
                sw.Stop();
                row.MinMs = row.MedianMs = row.MeanMs = sw.Elapsed.TotalMilliseconds;
                row.AllocBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            }
            catch (Exception ex) { row.Error = ex.GetBaseException().Message; }
        }

        // ---------- suites ----------

        private static void JsonAndConfig()
        {
            const string C = "json-startup";
            MeasureCold(C, "JsonConfigRuntime.EnsureLoaded (cold, all presets)",
                () => JsonConfigRuntime.EnsureLoaded());
            Measure(C, "JsonConfigRuntime.GetAll<GeneratorMapRecipe>", 20,
                () => JsonConfigRuntime.GetAll<GeneratorMapRecipe>());
            Measure(C, "JsonConfigRuntime.Get<GeneratorMapRecipe> cached", 50,
                () => JsonConfigRuntime.Get<GeneratorMapRecipe>(
                    JsonConfigRuntime.GetAll<GeneratorMapRecipe>().FirstOrDefault()?.JsonId ?? "none"));
        }

        private static HomeMenuMoyvaUiState _uiState;
        private static HomeMenuMoyvaUiViewController _uiView;
        private static readonly List<UnityEngine.Object> _spawned = new List<UnityEngine.Object>();

        private static void EnsureMenuView()
        {
            if (_uiView != null) return;
            _uiState = new HomeMenuMoyvaUiState();
            _uiView = new HomeMenuMoyvaUiViewController(_uiState);
        }

        private static void MenuMarkup()
        {
            const string C = "menu-markup";
            EnsureMenuView();
            var routes = new (string route, string label)[]
            {
                ("Main", "Main"), ("PlayModePanel", "PlayMode"), ("ContinuePanel", "Continue"),
                ("SelectMultiplayerType", "Multiplayer"), ("JoinRoomPanel", "JoinRoom"),
                ("WorldSetupPanel", "WorldSetup"), ("LobbyPanel", "Lobby"),
            };
            foreach (var r in routes)
            {
                _uiState.Open(r.route);
                string html = null;
                Measure(C, "Markup.Build " + r.label, 20,
                    () => html = HomeMenuMoyvaUiMarkup.Build(_uiState, _uiView, "vp-720"),
                    "len=" + (html?.Length ?? 0));
            }
            foreach (HomeMenuSettingsSection sec in Enum.GetValues(typeof(HomeMenuSettingsSection)))
            {
                _uiState.Open("SettingsPanel");
                _uiState.SetSettingsSection(sec);
                string html = null;
                Measure(C, "Markup.Build Settings." + sec, 20,
                    () => html = HomeMenuMoyvaUiMarkup.Build(_uiState, _uiView, "vp-720"),
                    "len=" + (html?.Length ?? 0));
            }
        }

        private static void MenuXmlParse()
        {
            const string C = "menu-xml-parse";
            EnsureMenuView();
            _uiState.Open("SettingsPanel");
            _uiState.SetSettingsSection(HomeMenuSettingsSection.Controls);
            var controlsHtml = HomeMenuMoyvaUiMarkup.Build(_uiState, _uiView, "vp-720");
            _uiState.Open("Main");
            var mainHtml = HomeMenuMoyvaUiMarkup.Build(_uiState, _uiView, "vp-720");
            Measure(C, "XmlDocument.LoadXml Controls markup", 20,
                () => new XmlDocument { XmlResolver = null }.LoadXml("<root>" + controlsHtml + "</root>"),
                "len=" + controlsHtml.Length);
            Measure(C, "XmlDocument.LoadXml Main markup", 20,
                () => new XmlDocument { XmlResolver = null }.LoadXml("<root>" + mainHtml + "</root>"),
                "len=" + mainHtml.Length);
        }

        private static string MenuCss()
        {
            var guids = AssetDatabase.FindAssets("t:TextAsset MoyvaUI");
            foreach (var g in guids)
            {
                var a = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(g));
                if (a != null && a.text.Contains("moyva-ui-app")) return a.text;
            }
            guids = AssetDatabase.FindAssets("t:TextAsset");
            foreach (var g in guids)
            {
                var a = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(g));
                if (a != null && a.text.Contains(".keyboard-key")) return a.text;
            }
            return "";
        }

        private static RectTransform _mountRoot;
        private static UnityHtmlHost _host;

        private static void MenuMount()
        {
            const string C = "menu-mount";
            EnsureMenuView();
            var css = MenuCss();
            var go = new GameObject("PerfMountRoot", typeof(RectTransform));
            _spawned.Add(go);
            _mountRoot = (RectTransform)go.transform;
            _mountRoot.SetParent(FindOrCreateCanvas(), false);
            _mountRoot.sizeDelta = new Vector2(1280, 720);
            _host = new UnityHtmlHost();

            foreach (var r in new (string route, HomeMenuSettingsSection? sec, string label)[]
            {
                ("Main", null, "Main"),
                ("SettingsPanel", HomeMenuSettingsSection.Controls, "Settings.Controls"),
                ("SettingsPanel", HomeMenuSettingsSection.Graphics, "Settings.Graphics"),
                ("WorldSetupPanel", null, "WorldSetup"),
            })
            {
                _uiState.Open(r.route);
                if (r.sec.HasValue) _uiState.SetSettingsSection(r.sec.Value);
                var html = HomeMenuMoyvaUiMarkup.Build(_uiState, _uiView, "vp-720");
                var doc = new UnityHtmlDocument(html, css, "perf");
                var globals = new Dictionary<string, object> { ["moyvaMenu"] = NewBridge() };

                // cold mount: fresh host each time = first-mount cost
                Measure(C, "Host.Mount cold " + r.label, 5,
                    () => { using (var h = new UnityHtmlHost()) h.Mount(_mountRoot, doc, globals); });

                // warm path: existing tree, state dirtied -> full re-mount+reconcile+layout
                _host.Mount(_mountRoot, doc, globals);
                Measure(C, "Host.Mount warm " + r.label + " (rebuild)", 10,
                    () => _host.Mount(_mountRoot, doc, globals));

                // dirty to a different panel then back = realistic navigation churn
                _uiState.Open("Main");
                var alt = new UnityHtmlDocument(
                    HomeMenuMoyvaUiMarkup.Build(_uiState, _uiView, "vp-720"), css, "perf2");
                _host.Mount(_mountRoot, alt, globals);
                Measure(C, "Host.Mount navigate Main->" + r.label, 10,
                    () => { _host.Mount(_mountRoot, doc, globals); _host.Mount(_mountRoot, alt, globals); });
            }
        }

        private static Transform FindOrCreateCanvas()
        {
            var canvas = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include)
                .FirstOrDefault();
            if (canvas != null) return canvas.transform;
            var go = new GameObject("PerfCanvas", typeof(Canvas), typeof(CanvasScaler));
            _spawned.Add(go);
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            return go.transform;
        }

        private static HomeMenuMoyvaUiBridge NewBridge()
            => new HomeMenuMoyvaUiBridge(null, null, _uiView, null, _uiState);

        private static void ControlsEditorCosts()
        {
            const string C = "controls-editor";
            EnsureMenuView();
            var ed = _uiView.Controls;
            ed.SelectDevice(0);
            Measure(C, "LiveSignature() (12.5Hz poll)", 20, () => ed.GetType()
                .GetMethod("LiveSignature", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(ed, null));
            Measure(C, "Conflict getter x1", 100, () => { var _ = ed.Conflict; });
            Measure(C, "ReservedAction x1", 100, () => ed.ReservedAction("w", PlayerControlModifiers.None));
            Measure(C, "BoundAction x1", 100, () => ed.BoundAction("w", PlayerControlModifiers.None));
            // full keyboard render cost: 85 keys x (IsLive+BoundAction+ReservedAction+Conflict)
            Measure(C, "All-keys highlight pass (~85 keys)", 20, () =>
            {
                foreach (var k in Keys()) { ed.BoundAction(k, ed.Modifiers); ed.ReservedAction(k, ed.Modifiers); }
            });
        }

        private static IEnumerable<string> Keys()
        {
            string[] rows = {
                "escape f1 f2 f3 f4 f5 f6 f7 f8 f9 f10 f11 f12 delete",
                "backquote digit1 digit2 digit3 digit4 digit5 digit6 digit7 digit8 digit9 digit0 minus equals backspace",
                "tab q w e r t y u i o p leftBracket rightBracket backslash",
                "capsLock a s d f g h j k l semicolon quote enter",
                "leftShift z x c v b n m comma period slash rightShift upArrow",
                "leftCtrl leftAlt space rightAlt rightCtrl leftArrow downArrow rightArrow" };
            foreach (var r in rows) foreach (var k in r.Split(' ')) yield return k;
        }

        private static void MenuPreview()
        {
            const string C = "menu-preview";
            GeneratorMapRecipe recipe = null;
            IReadOnlyList<GeneratorMapRecipe> recipes = null;
            try { recipes = JsonConfigRuntime.GetAll<GeneratorMapRecipe>(); } catch { }
            MenuWorldPreviewData data = null;
            foreach (var r in recipes ?? (IReadOnlyList<GeneratorMapRecipe>)Array.Empty<GeneratorMapRecipe>())
            {
                string probeError = null;
                if (r != null && MenuWorldPreviewGenerator.TryGenerate(r, 64, 64, 12345, out var probe, out probeError))
                {
                    recipe = r;
                    data = probe;
                    break;
                }
                Rows.Add(new Row { Category = C, Name = $"MenuWorldPreviewGenerator probe {r?.JsonId}", Error = probeError ?? "returned false" });
            }
            if (recipe == null)
                Rows.Add(new Row { Category = C, Name = "MenuWorldPreviewGenerator", Error = "no working recipe preset" });
            else
            {
                Measure(C, "MenuWorldPreviewGenerator 192x108", 5,
                    () => MenuWorldPreviewGenerator.TryGenerate(recipe, 192, 108, 12345, out data, out _));
                Measure(C, "MenuWorldPreviewGenerator 128x128 (Large world)", 5,
                    () => MenuWorldPreviewGenerator.TryGenerate(recipe, 128, 128, 12345, out _, out _));
                Measure(C, "MenuWorldPreviewGenerator 64x64 (Medium world)", 10,
                    () => MenuWorldPreviewGenerator.TryGenerate(recipe, 64, 64, 12345, out _, out _));
            }
            if (data == null)
            {
                // Synthetic data still exercises the texture-build half of the preview pipeline.
                const int w = 192, h = 108;
                data = new MenuWorldPreviewData(w, h, 1, Fill(w, h, "grass"), Fill(w, h, ""), FillF(w, h, 0.5f), Fill(w, h, ""));
            }
            TileRegistrySO tiles = null; MoyvaProjectSettingsSO proj = null;
            try { tiles = JsonConfigRuntime.GetAll<TileRegistrySO>().FirstOrDefault();
                  proj = JsonConfigRuntime.Get<MoyvaProjectSettingsSO>("moyvaprojectsettings"); } catch { }
            if (tiles == null) return;
            Measure(C, "MenuWorldPreviewTextureBuilder.Build 192x108 (synthetic)", 3,
                () => MenuWorldPreviewTextureBuilder.Build(data, tiles, null, null, 4, 1024, proj));
        }

        private static void WorldDataIntegrity()
        {
            const string C = "world-data";
            int w = 128, h = 128;
            var data = new GeneratedWorldData
            {
                Width = w, Height = h,
                GameplayTileMap = Fill(w, h, "grass"),
                BiomeMap = Fill(w, h, "grass"),
                VisualTileMap = Fill(w, h, "grass"),
                HeightMap = FillF(w, h, 0.5f),
            };
            var svc = new GeneratedWorldDataIntegrityService(null, null);
            Measure(C, "IntegrityService.EnsureReadyForBuild 128x128", 10,
                () => svc.EnsureReadyForBuild(data, "bench"));
        }

        private static string[,] Fill(int w, int h, string v)
        { var m = new string[w, h]; for (int x = 0; x < w; x++) for (int y = 0; y < h; y++) m[x, y] = v; return m; }
        private static float[,] FillF(int w, int h, float v)
        { var m = new float[w, h]; for (int x = 0; x < w; x++) for (int y = 0; y < h; y++) m[x, y] = v; return m; }

        private static void GridAndPath()
        {
            const string C = "grid-path";
            TileRegistrySO tiles = null; MoyvaProjectSettingsSO proj = null;
            try { tiles = JsonConfigRuntime.GetAll<TileRegistrySO>().FirstOrDefault();
                  proj = JsonConfigRuntime.Get<MoyvaProjectSettingsSO>("moyvaprojectsettings"); } catch { }
            if (tiles == null || proj == null) { Rows.Add(new Row { Category = C, Name = "grid", Error = "registries null" }); return; }
            const int w = 128, h = 128;
            var c = new DiContainer();
            var installGo = new GameObject("PerfObjectsMapInstall");
            _spawned.Add(installGo);
            var signalsInstaller = c.InstantiateComponent<Kruty1918.Moyva.Signals.SignalBusInstaller>(installGo);
            signalsInstaller.InstallBindings();
            GridInstaller.InstallPreviewBindings(c, tiles, proj, w, h);
            var objectsInstaller = c.InstantiateComponent<ObjectsMapInstaller>(installGo);
            objectsInstaller.InstallBindings();
            UnityEngine.Object.DestroyImmediate(objectsInstaller);
            UnityEngine.Object.DestroyImmediate(signalsInstaller);
            var grid = c.Resolve<IGridService>();
            var rng = new System.Random(1);
            Measure(C, "Grid SetTileData 128x128 (world write)", 5,
                () => { for (int i = 0; i < w * h; i++) grid.SetTileData(new Vector2Int(rng.Next(w), rng.Next(h)), "grass"); });
            Measure(C, "Grid GetTileData x10000", 5,
                () => { for (int i = 0; i < 10000; i++) grid.GetTileData(new Vector2Int(rng.Next(w), rng.Next(h))); });

            var objects = c.Resolve<IObjectsMapService>();
            var tileSettings = c.Resolve<ITileSettingsService>();
            var pf = new Pathfinder(grid, tileSettings, objects);
            Measure(C, "Pathfinder.FindPath corner-to-corner 128x128", 20,
                () => pf.FindPath(new Vector2Int(1, 1), new Vector2Int(w - 2, h - 2)));
            Measure(C, "Pathfinder.FindPath mid 40-cell", 20,
                () => pf.FindPath(new Vector2Int(10, 10), new Vector2Int(50, 50)));
        }

        private static void FogAndTexture()
        {
            const string C = "fog-texture";
            MoyvaProjectSettingsSO proj = null;
            try { proj = JsonConfigRuntime.Get<MoyvaProjectSettingsSO>("moyvaprojectsettings"); } catch { }
            var projection = proj != null ? GridProjectionFactory.Create(proj) : null;

            foreach (var sz in new[] { 128, 256, 512, 1024 })
            {
                var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
                _spawned.Add(tex);
                var px = new Color32[sz * sz];
                Measure(C, $"Texture2D SetPixels32+Apply {sz}x{sz}", 20,
                    () => { tex.SetPixels32(px); tex.Apply(false); });
            }
            if (projection == null) return;
            try
            {
                var fog = new FogScreenSpaceTextureUpdater();
                var ctx = new FogWorldVisualContext(128, 128,
                    GridTopology.Orthogonal, GridProjectionMode.Orthographic3D, GridRenderMode.Mesh3D,
                    GridNeighborhoodMode.Moore8, 1f, false, new Bounds(), null, null);
                fog.Initialize(128, 128, ctx);
                var changes = new List<FogCellVisualChange>();
                var r = new System.Random(2);
                for (int i = 0; i < 200; i++)
                    changes.Add(new FogCellVisualChange(
                        new Vector2Int(r.Next(128), r.Next(128)),
                        FogStateType.Unexplored, FogStateType.Visible, 0, 0));
                Measure(C, "Fog RequestCellsUpdate 200 cells (commit+upload)", 10,
                    () => fog.RequestCellsUpdate(null, changes, ctx));
            }
            catch (Exception ex) { Rows.Add(new Row { Category = C, Name = "fog updater", Error = ex.Message }); }
        }

        private static void ObjectChurn()
        {
            const string C = "object-churn";
            Measure(C, "GameObject create+destroy x500", 5, () =>
            {
                for (int i = 0; i < 500; i++) { var g = new GameObject("churn"); UnityEngine.Object.DestroyImmediate(g); }
            });
        }

        private static void ZenjectCosts()
        {
            const string C = "zenject";
            Measure(C, "new DiContainer()", 100, () => { var _ = new DiContainer(); });
            Measure(C, "DiContainer bind x100", 10, () =>
            {
                var c = new DiContainer();
                for (int i = 0; i < 100; i++) c.Bind<string>().FromInstance("x");
            });
            var rc = new DiContainer();
            rc.Bind<string>().FromInstance("x");
            Measure(C, "DiContainer resolve x100 (pre-bound)", 10, () =>
            {
                for (int i = 0; i < 100; i++) rc.Resolve<string>();
            });
        }

        // ---------- report ----------

        private static string Summary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("category | name | median ms | min ms | mean ms | alloc KB | note | error");
            foreach (var r in Rows)
                sb.AppendLine($"{r.Category} | {r.Name} | {r.MedianMs:F3} | {r.MinMs:F3} | {r.MeanMs:F3} | {r.AllocBytes / 1024.0:F1} | {r.Note} | {r.Error}");
            return sb.ToString();
        }

        private static string ToJson()
        {
            var sb = new StringBuilder();
            sb.Append("{\"hardware\":\"Ryzen AI 5 340 / Radeon 840M / 16GB\",\"results\":[");
            for (int i = 0; i < Rows.Count; i++)
            {
                var r = Rows[i];
                if (i > 0) sb.Append(',');
                sb.Append("{\"category\":\"").Append(r.Category)
                  .Append("\",\"name\":\"").Append(r.Name.Replace("\"", "'"))
                  .Append("\",\"iterations\":").Append(r.Iterations)
                  .Append(",\"medianMs\":").Append(r.MedianMs.ToString("F3", System.Globalization.CultureInfo.InvariantCulture))
                  .Append(",\"minMs\":").Append(r.MinMs.ToString("F3", System.Globalization.CultureInfo.InvariantCulture))
                  .Append(",\"meanMs\":").Append(r.MeanMs.ToString("F3", System.Globalization.CultureInfo.InvariantCulture))
                  .Append(",\"allocBytes\":").Append(r.AllocBytes)
                  .Append(",\"note\":\"").Append(r.Note.Replace("\"", "'"))
                  .Append("\",\"error\":\"").Append(r.Error.Replace("\"", "'")).Append("\"}");
            }
            sb.Append("]}");
            return sb.ToString();
        }
    }
}
