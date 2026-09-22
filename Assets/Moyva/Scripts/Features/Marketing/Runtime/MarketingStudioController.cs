using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Marketing.Content;
using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Marketing.Output;
using Kruty1918.Moyva.Marketing.Planning;
using Kruty1918.Moyva.Marketing.Text;
using Kruty1918.Moyva.Marketing.Validation;
using Kruty1918.SaveSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// MarketingStudio scene entry point and composition root. Owns the
    /// capture rig, loads the gameplay scene additively (canonical world
    /// bootstrap), runs the selected recipe end-to-end, writes output +
    /// manifest, supports cancel and auto-exit for editor/CLI runs.
    /// </summary>
    public sealed class MarketingStudioController : MonoBehaviour
    {
        private const string GameplayScenePath = "Assets/Moyva/Scenes/Gamplay_Scene.unity";

        [Header("Run")]
        public string recipeId = "steam-screenshots";
        public bool autoStartCapture = true;
        public bool exitPlayModeOnFinish = false;

        [Header("World")]
        public int worldSizePreset = 1;   // 64×64 default
        public int mapType = 0;
        public int difficulty = 0;

        [Header("Timeouts (seconds)")]
        public float worldBuildTimeout = 180f;
        public float phaseTimeout = 90f;

        private MarketingStudioSession.RunRequest _request;
        private MarketingPresetStore _presets;
        private MarketingCaptureRecipe _recipe;
        private PlatformCaptureProfile _platform;
        private MarketingSeeds _seeds;
        private MarketingOutputManager _output;
        private MarketingRunStatus _status;
        private MarketingGameplayContext _gameplay;
        private WorldSubjects _world;
        private ContentIndexSnapshot _index;
        private MarketingFeatureFacts _facts;
        private MarketingClaimCatalog _claims;
        private ShotSequence _sequence;

        private Camera _captureCamera;
        private bool _fogRevealed;
        private AudioListener _captureListener;
        private MarketingCameraDirector _cameraDirector;
        private MarketingLightingDirector _lighting;
        private MarketingUiVisibilityPolicy _uiPolicy;
        private MarketingAudioDirector _audioDirector;
        private MarketingScenarioDirector _scenario;
        private MarketingStillCapture _still;
        private MarketingOverlayRig _overlay;
        private Texture2D _logoTex;
        private Font _font;

        private readonly List<Behaviour> _suspendedCameras = new List<Behaviour>();
        private readonly List<AudioListener> _suspendedListeners = new List<AudioListener>();
        private readonly Dictionary<UnityEngine.Rendering.Universal.ScriptableRendererFeature, bool>
            _fogFeatureStates = new Dictionary<UnityEngine.Rendering.Universal.ScriptableRendererFeature, bool>();
        private bool _cancelRequested;
        private bool _running;

        public string RunFolder => _output?.Layout?.RunFolder;

        private void Start()
        {
            _status = new MarketingRunStatus();
            _request = MarketingStudioSession.ReadRequest();
            // Consume the hand-off so stale requests never leak into manual Play.
            if (_request != null) MarketingStudioSession.ClearRequest();
            if (_request != null)
            {
                recipeId = string.IsNullOrEmpty(_request.recipeId) ? recipeId : _request.recipeId;
                autoStartCapture = _request.autoStart;
                exitPlayModeOnFinish = _request.exitPlayModeOnFinish;
                if (!string.IsNullOrEmpty(_request.statusFile))
                    _status.StatusFilePath = _request.statusFile;
            }
            if (string.IsNullOrEmpty(_status.StatusFilePath))
                _status.StatusFilePath = Path.Combine(
                    MarketingOutputLayout.ProjectRoot(), "MarketingOutput", ".cache", "studio-status.json");

            BuildRig();

            if (autoStartCapture)
                StartCoroutine(RunCoroutine());
            else
            {
                SetPhase("manual", 0f, "Auto-start OFF — world is staged for inspection.");
                StartCoroutine(LoadWorldOnly());
            }
        }

        private void Update()
        {
            if (_running && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Cancel();
        }

        public void Cancel() => _cancelRequested = true;

        // ── Rig ────────────────────────────────────────────────────────────

        private void BuildRig()
        {
            var camGo = new GameObject("MarketingCaptureCamera");
            camGo.tag = "MainCamera"; // Recorder ImageSource.MainCamera target
            _captureCamera = camGo.AddComponent<Camera>();
            _captureCamera.clearFlags = CameraClearFlags.SolidColor;
            // Deep teal backdrop matching the game's palette — the stylized
            // gradient skybox renders near-black below the horizon, which is
            // where top-down and tactical rigs look.
            _captureCamera.backgroundColor = new Color(0.07f, 0.25f, 0.30f);
            _captureCamera.fieldOfView = 50f;
            _captureCamera.nearClipPlane = 0.1f;
            _captureCamera.farClipPlane = 2000f;
            _captureCamera.allowMSAA = true;
            _captureCamera.enabled = false; // enabled during capture phases
            _captureListener = camGo.AddComponent<AudioListener>();
            _captureListener.enabled = false; // enabled only after gameplay listeners are suspended
            camGo.transform.position = new Vector3(0, 30, -30);

            _cameraDirector = new MarketingCameraDirector(_captureCamera);
            _lighting = new MarketingLightingDirector();
            _uiPolicy = new MarketingUiVisibilityPolicy();
            _still = new MarketingStillCapture();

            _font = null;
            try { _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { }
            if (_font == null)
            {
                try { _font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }
        }

        // ── Pipeline ───────────────────────────────────────────────────────

        private IEnumerator LoadWorldOnly()
        {
            yield return LoadGameplayWorld();
        }

        private IEnumerator RunCoroutine()
        {
            _running = true;
            SetPhase("load-presets", 0.02f, "Loading presets");
            LoadPresetsAndValidate();
            if (Failed()) { yield return Teardown("failed"); yield break; }

            SetPhase("world", 0.08f, "Building world");
            yield return LoadGameplayWorld();
            if (Failed()) { yield return Teardown("failed"); yield break; }

            SetPhase("scan", 0.2f, "Scanning world content");
            ScanAndStage();
            if (Failed()) { yield return Teardown("failed"); yield break; }

            // Reveal the map so fog-of-war does not obscure captures; fog
            // visuals rebuild over a few frames.
            _fogRevealed = _gameplay.RevealMapForCapture();
            Debug.Log($"[MarketingStudio] Fog reveal for capture: {_fogRevealed}");
            if (_fogRevealed)
                yield return SettleFrames(20);

            bool video = _recipe.outputKind >= MarketingOutputKind.Video
                || _recipe.outputKind == MarketingOutputKind.ImageSequence;
            if (video)
                yield return RunVideo();
            else
                yield return RunScreenshots();

            yield return Teardown(_cancelRequested ? "cancelled" : AnyShotFailed() ? "partial" : "ok");
        }

        private void LoadPresetsAndValidate()
        {
            _presets = new MarketingPresetStore();
            _presets.LoadAll();

            _recipe = !string.IsNullOrEmpty(_request?.recipeJson)
                ? JsonConvert.DeserializeObject<MarketingCaptureRecipe>(_request.recipeJson)
                : _presets.FindRecipe(recipeId) ?? _presets.FindRecipe("steam-screenshots")
                  ?? new MarketingCaptureRecipe { id = recipeId };

            if (!string.IsNullOrEmpty(_request?.platformOverride))
                _recipe.platformProfileId = _request.platformOverride;
            if (!string.IsNullOrEmpty(_request?.language))
                _recipe.language = _request.language;

            _platform = _presets.FindPlatform(_recipe.platformProfileId)
                ?? new PlatformCaptureProfile { id = _recipe.platformProfileId };

            // Resolve platform defaults
            if (_recipe.resolutionWidth <= 0) _recipe.resolutionWidth = _platform.resolutionWidth;
            if (_recipe.resolutionHeight <= 0) _recipe.resolutionHeight = _platform.resolutionHeight;

            var issues = MarketingRecipeValidator.Validate(_recipe);
            issues.AddRange(PlatformCompliance.Check(_recipe, _platform));
            bool blocking = issues.Exists(i => i.blocking);
            if (blocking)
            {
                _status.error = "Recipe/platform validation failed: "
                    + string.Join("; ", issues.ConvertAll(i => i.message));
            }
            else
            {
                // Auto-generate compliant clean variant info: force flags off
                // where the platform forbids them instead of blocking.
                _recipe = PlatformCompliance.ForceCompliant(_recipe, _platform);
            }

            _seeds = _recipe.seeds.ResolveAuto(Environment.TickCount);
            _index = _presets.LoadIndexSnapshot() ?? new ContentIndexSnapshot();
            _facts = MarketingFeatureFacts.FromIndex(_index);
            _claims = new MarketingClaimCatalog(_presets.Claims);

            string root = string.IsNullOrEmpty(_request?.outputRoot) ? null : _request.outputRoot;
            _output = new MarketingOutputManager(new MarketingOutputLayout(root),
                _recipe, _seeds, GitSha("origin/game-process"), GitSha("HEAD"));
            _output.Manifest.validationIssues.AddRange(issues.ConvertAll(i => i.message));
            _status.runFolder = _output.Layout.RunFolder;
        }

        private IEnumerator LoadGameplayWorld()
        {
            // Configure deterministic launch BEFORE the gameplay scene loads.
            GameLaunchContext.Reset();
            GameLaunchContext.ConfigureMenuNewGame(
                saveSlot: 95, worldName: "Marketing", seed: _seeds?.worldSeed ?? 1,
                size: worldSizePreset, mapType: mapType, difficulty: difficulty,
                maxPlayers: 2, isPrivate: false);

            var load = SceneManager.LoadSceneAsync(GameplayScenePath, LoadSceneMode.Additive);
            if (load == null)
            {
                _status.error = $"Failed to load gameplay scene '{GameplayScenePath}'.";
                yield break;
            }
            float t0 = Time.realtimeSinceStartup;
            while (!load.isDone)
            {
                if (Time.realtimeSinceStartup - t0 > 60f) { _status.error = "Scene load timeout."; yield break; }
                yield return null;
            }
            var gameplayScene = SceneManager.GetSceneByPath(GameplayScenePath);
            if (gameplayScene.IsValid())
                SceneManager.SetActiveScene(gameplayScene);
            _gameplay = MarketingGameplayContext.Attach(gameplayScene);

            // Wait for world bootstrap. A fresh world legitimately has zero
            // units — the scenario director stages content afterwards — so the
            // gate is "grid generated", with a short grace period for starting
            // units to appear if the launch mode provides them.
            t0 = Time.realtimeSinceStartup;
            float readyAt = -1f;
            while (Time.realtimeSinceStartup - t0 < worldBuildTimeout)
            {
                if (_cancelRequested) yield break;
                bool gridReady = false;
                int unitCount = 0;
                try
                {
                    var grid = _gameplay?.Grid;
                    gridReady = grid != null && grid.GridWidth > 0;
                    unitCount = _gameplay?.Units?.GetAllUnitIds()?.Count ?? 0;
                }
                catch { }
                if (gridReady)
                {
                    if (unitCount > 0) { yield return SettleFrames(30); yield break; }
                    if (readyAt < 0f) readyAt = Time.realtimeSinceStartup;
                    // 8s grace for starting units; then proceed to staging.
                    if (Time.realtimeSinceStartup - readyAt > 8f)
                    {
                        yield return SettleFrames(30);
                        yield break;
                    }
                }
                yield return null;
            }
            _status.error = "World build timeout — no grid detected.";
        }

        private IEnumerator SettleFrames(int frames)
        {
            for (int i = 0; i < frames; i++) yield return null;
        }

        private void ScanAndStage()
        {
            _world = MarketingWorldScanner.Scan(_gameplay, _index);
            int rendererCount = 0;
            var bounds = new Bounds();
            bool hasBounds = false;
            if (_gameplay.GameplayScene.IsValid())
                foreach (var root in _gameplay.GameplayScene.GetRootGameObjects())
                {
                    var renderers = root.GetComponentsInChildren<Renderer>(true);
                    rendererCount += renderers.Length;
                    foreach (var r in renderers)
                    {
                        if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
                        else bounds.Encapsulate(r.bounds);
                    }
                }
            Debug.Log($"[MarketingStudio] World scan: {_world.units.Count} units, " +
                $"{_world.buildings.Count} buildings, {_world.landmarks.Count} landmarks, " +
                $"{rendererCount} renderers in gameplay scene" +
                (hasBounds ? $", bounds={bounds.min:F0}..{bounds.max:F0}" : ""));

            _scenario = new MarketingScenarioDirector(_gameplay);
            var staged = _scenario.Apply(_recipe, _world, _index, _seeds.scenarioSeed);
            Debug.Log($"[MarketingStudio] Staging: {staged.scenario} ok={staged.ok} " +
                $"spawned={staged.spawnedUnitIds.Count} {staged.message}");
            if (!staged.ok)
                _output.Manifest.validationIssues.Add("staging:" + staged.message);
            if (staged.spawnedUnitIds.Count > 0)
            {
                // Re-scan so staged units become shot subjects.
                _world = MarketingWorldScanner.Scan(_gameplay, _index);
                _output.Manifest.validationIssues.Add($"staging:{staged.scenario}:{staged.message}");
            }

            // Suspend gameplay cameras; capture camera takes over.
            SuspendGameplayCameras();
            SetFogRendererFeatures(false);
            _captureCamera.enabled = true;
            if (_captureListener != null) _captureListener.enabled = true;
            _captureCamera.aspect = (float)_recipe.resolutionWidth / _recipe.resolutionHeight;
            _cameraDirector.SetAspect(_captureCamera.aspect);
            _uiPolicy.Collect(_gameplay.GameplayScene);
            _uiPolicy.Apply(_recipe.uiVisibility);
            _lighting.Apply(_recipe.lightingStyle);

            _audioDirector = new MarketingAudioDirector(_gameplay.Audio, this);
            _overlay = new MarketingOverlayRig(_captureCamera, _captureCamera.transform,
                _font, _presets.Branding.textColor, _presets.Branding.shadowColor);
            _logoTex = LoadLogo();

            var planner = new ShotPlanner();
            _sequence = _recipe.outputKind >= MarketingOutputKind.Video
                ? (_recipe.contentType == MarketingContentType.MenuBackground
                    ? planner.PlanAmbient(_world, _index, _recipe, _seeds.cinematicSeed)
                    : planner.PlanTrailer(_world, _index, _recipe, _seeds.cinematicSeed))
                : planner.PlanScreenshots(_world, _index, _recipe, _seeds.cinematicSeed);
            _output.WriteMetadataJson(_sequence, "shots.json");
        }

        private Texture2D LoadLogo()
        {
            string path = !string.IsNullOrEmpty(_recipe.logoSpritePath)
                ? _recipe.logoSpritePath : _presets.Branding.logoSpritePath;
            if (string.IsNullOrEmpty(path)) return null;
            string abs = Path.Combine(MarketingOutputLayout.ProjectRoot(), path.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(abs)) return null;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try { tex.LoadImage(File.ReadAllBytes(abs)); } catch { return null; }
            return tex;
        }

        // ── Screenshots ────────────────────────────────────────────────────

        private IEnumerator RunScreenshots()
        {
            SetPhase("screenshots", 0.3f, "Capturing screenshots");
            int w = _recipe.resolutionWidth, h = _recipe.resolutionHeight;
            float aspect = (float)w / h;
            int sampleW = Mathf.Min(480, w);
            int sampleH = Mathf.Max(1, Mathf.RoundToInt(sampleW / aspect));

            for (int i = 0; i < _sequence.shots.Count; i++)
            {
                if (_cancelRequested) yield break;
                var shot = _sequence.shots[i];
                var record = new ShotRecord
                {
                    shotId = shot.shotId, category = shot.category.ToString(),
                    rig = shot.rig.ToString(), beat = shot.beat,
                    subjectId = shot.subject.contentId, seed = shot.seed,
                    durationSec = shot.durationSec,
                };
                _output.Manifest.shots.Add(record);

                float t0 = Time.realtimeSinceStartup;
                Texture2D best = null;
                float bestScore = -1f;
                Vector2 bestFocus = new Vector2(0.5f, 0.45f);
                string lastRejects = string.Empty;
                _cameraDirector.ApplyShot(shot, AzimuthBase(i));

                // Golden-frame sampling: render candidates, keep the best.
                const int samples = 4;
                for (int s = 0; s < samples; s++)
                {
                    float t = samples == 1 ? 0.5f : (s + 0.5f) / samples;
                    _cameraDirector.Evaluate(t);
                    yield return null; // let animations/fog settle a frame
                    var candidate = _still.Render(_captureCamera, sampleW, sampleH, 1);
                    var metrics = _still.Measure(candidate, _captureCamera,
                        _cameraDirector.SubjectPosition, shot.subject.approximateRadius,
                        _platform.safeAreaMargin, aspect);
                    var score = GoldenFrameEvaluator.Evaluate(metrics);
                    if (score.Rejected && score.rejectReasons.Count > 0)
                        lastRejects = string.Join(",", score.rejectReasons)
                            + $" [blk={metrics.blackFraction:F2} var={metrics.luminanceVariance:F4} "
                            + $"area={metrics.subjectScreenArea:F3} clip={metrics.subjectClippedFraction:F2}]";
                    float value = score.Rejected ? -1f : score.score;
                    if (value > bestScore)
                    {
                        if (best != null) Destroy(best);
                        // Re-render winning frame at full res at the end.
                        best = candidate;
                        bestScore = value;
                        bestFocus = _cameraDirector.SubjectViewportPoint();
                    }
                    else Destroy(candidate);
                    if (Time.realtimeSinceStartup - t0 > phaseTimeout) break;
                }

                if (bestScore < 0f)
                {
                    record.status = "failed";
                    record.failure = "all candidate frames rejected: " + lastRejects;
                    // Dump the rejected frame so failures are reviewable
                    // without re-running.
                    var dump = _still.Render(_captureCamera, sampleW, sampleH, 1);
                    if (dump != null)
                    {
                        _output.WritePng(dump, "Debug", $"rejected-{shot.shotId}.png");
                        Destroy(dump);
                    }
                    Debug.Log($"[MarketingStudio] Rejected {shot.shotId}: {lastRejects} " +
                        $"camPos={_captureCamera.transform.position:F1} fogRevealed={_fogRevealed}");
                    if (best != null) Destroy(best);
                    continue;
                }

                // Full-res render of the chosen framing.
                var final = _still.Render(_captureCamera, w, h, MsaaForQuality());
                var qa = MarketingQualityGate.Check(final, w, h, aspect, _recipe.uiVisibility != MarketingUiVisibility.Hidden);
                if (qa.Count > 0)
                {
                    record.status = "failed";
                    record.failure = string.Join(";", qa);
                    Destroy(final);
                    Destroy(best);
                    continue;
                }

                record.score = bestScore;
                record.status = "ok";
                string masterName = $"{_recipe.Prefix}_{shot.shotId}_master.png";
                _output.WriteMasterPng(final, masterName);
                record.files.Add("Master/" + masterName);
                if (!_output.Manifest.assetIds.Contains(shot.subject.contentId))
                    _output.Manifest.assetIds.Add(shot.subject.contentId);

                // Platform variant (composition-aware crop when aspect differs).
                string platName = $"{_recipe.Prefix}_{shot.shotId}.png";
                if (_platform.MatchesAspect(aspect))
                {
                    _output.WritePng(final, _platform.id, platName);
                }
                else
                {
                    int tw = _platform.resolutionWidth, th = _platform.resolutionHeight;
                    var crop = AspectCrop.Compute(w, h, (float)tw / th, bestFocus);
                    var variant = AspectCrop.Apply(final, crop, tw, th);
                    _output.WritePng(variant, _platform.id, platName);
                    Destroy(variant);
                }
                record.files.Add($"{_platform.id}/{platName}");

                // Optional text overlay variant — never replaces the master.
                if (_recipe.includeMarketingText || _recipe.emitTextVariant)
                {
                    var claim = _claims.PickFor(_facts, _recipe.language, "imperative", shot.seed);
                    if (claim != null && MarketingTypography.Fits(claim.For(_recipe.language), TextTemplate.ShortPhrase))
                    {
                        _overlay.ShowText(claim.For(_recipe.language), TextTemplate.ShortPhrase,
                            aspect, _platform.safeAreaMargin);
                        var withText = _still.Render(_captureCamera, w, h, MsaaForQuality());
                        string textName = $"{_recipe.Prefix}_{shot.shotId}_text_{_recipe.language}.png";
                        _output.WritePng(withText, _platform.id, textName);
                        record.files.Add($"{_platform.id}/{textName}");
                        _overlay.HideText();
                        Destroy(withText);
                    }
                }

                Destroy(final);
                Destroy(best);
                SetPhase("screenshots", 0.3f + 0.6f * (i + 1f) / _sequence.shots.Count,
                    $"Shot {i + 1}/{_sequence.shots.Count}");
                _output.WriteManifest();
            }
        }

        // ── Video ──────────────────────────────────────────────────────────

        private IEnumerator RunVideo()
        {
            SetPhase("video-setup", 0.3f, "Preparing video capture");
            int w = _recipe.resolutionWidth, h = _recipe.resolutionHeight;
            float aspect = (float)w / h;
            bool withAudio = _recipe.outputKind != MarketingOutputKind.VideoMuted
                && (_recipe.includeMusic || _recipe.includeSfx || _recipe.outputKind == MarketingOutputKind.VideoWithAudio);

            bool imageSequence = _recipe.outputKind == MarketingOutputKind.ImageSequence;
            string masterFile = Path.Combine(_output.Layout.RunFolder, "Master",
                $"{_recipe.Prefix}_master.{_platform.videoFormat}");
            string sequenceDir = Path.Combine(_output.Layout.RunFolder, "Master",
                $"{_recipe.Prefix}_sequence");

            var backend = MarketingRuntimeBridge.Backend;
            bool useRecorder = backend != null && backend.IsAvailable;
            if (useRecorder)
            {
                if (imageSequence)
                    backend.BeginImageSequence(sequenceDir, w, h, _recipe.frameRate);
                else
                    backend.BeginVideo(masterFile, w, h, _recipe.frameRate, withAudio);
            }

            if (withAudio)
            {
                string musicKey = !string.IsNullOrEmpty(_recipe.musicKey)
                    ? _recipe.musicKey : MarketingAudioDirector.AutoMusicKey(_index);
                string ambKey = !string.IsNullOrEmpty(_recipe.ambienceKey)
                    ? _recipe.ambienceKey : MarketingAudioDirector.AutoAmbienceKey(_index);
                _audioDirector.PlayMusic(musicKey);
                _audioDirector.PlayAmbience(ambKey);
                foreach (var k in _audioDirector.UsedKeys)
                    if (!_output.Manifest.audioKeys.Contains(k)) _output.Manifest.audioKeys.Add(k);
            }

            _captureCamera.enabled = true; // game-view-visible camera for Recorder
            int lastBeatHash = 0;
            for (int i = 0; i < _sequence.shots.Count; i++)
            {
                if (_cancelRequested) break;
                var shot = _sequence.shots[i];
                var record = new ShotRecord
                {
                    shotId = shot.shotId, category = shot.category.ToString(),
                    rig = shot.rig.ToString(), beat = shot.beat,
                    subjectId = shot.subject.contentId, seed = shot.seed,
                    durationSec = shot.durationSec,
                };
                _output.Manifest.shots.Add(record);

                // Transition-in
                yield return TransitionIn(shot.transitionIn);

                _cameraDirector.ApplyShot(shot, AzimuthBase(i));
                _cameraDirector.Evaluate(0f);

                // First-frame preview: review framing without decoding video.
                var preview = _still.Render(_captureCamera, 640,
                    Mathf.Max(1, Mathf.RoundToInt(640f / aspect)), 1);
                if (preview != null)
                {
                    _output.WritePng(preview, "Debug", $"preview-{shot.shotId}.png");
                    Destroy(preview);
                }

                // Beat text
                if (_recipe.includeMarketingText && !string.IsNullOrEmpty(shot.beat)
                    && shot.beat != "End")
                {
                    var claim = _claims.PickFor(_facts, _recipe.language, "imperative",
                        shot.seed + shot.beat.GetHashCode());
                    if (claim != null && MarketingTypography.Fits(claim.For(_recipe.language), TextTemplate.BeatWord))
                        _overlay.ShowText(claim.For(_recipe.language), TextTemplate.BeatWord, aspect, _platform.safeAreaMargin);
                }
                if (shot.beat == "End")
                {
                    _overlay.ShowEndCard(_presets.Branding.gameTitle,
                        _presets.Branding.endCardLine, aspect, _platform.safeAreaMargin);
                    if (_recipe.includeLogo && _logoTex != null)
                        _overlay.ShowLogo(_logoTex, true, aspect, _platform.safeAreaMargin);
                    if (withAudio) _audioDirector.DuckMusic(1.2f, 0.2f);
                }
                else if (_recipe.includeLogo && _logoTex != null)
                {
                    _overlay.ShowLogo(_logoTex, false, aspect, _platform.safeAreaMargin);
                }

                // Accent on combat entry
                if (withAudio && shot.category == ShotCategory.Combat
                    && shot.beat.GetHashCode() != lastBeatHash)
                {
                    _audioDirector.Accent(FindAccentKey("impact"), 0.9f);
                    _audioDirector.DuckMusic(0.5f, 0.5f);
                }
                lastBeatHash = shot.beat.GetHashCode();

                float shotT0 = Time.time;
                while (Time.time - shotT0 < shot.durationSec)
                {
                    if (_cancelRequested) break;
                    _cameraDirector.Evaluate((Time.time - shotT0) / shot.durationSec);
                    yield return null;
                }
                record.status = _cancelRequested ? "skipped" : "ok";
                _overlay.HideText();
                SetPhase("video", 0.3f + 0.6f * (i + 1f) / _sequence.shots.Count,
                    $"Shot {i + 1}/{_sequence.shots.Count} ({shot.beat})");
            }

            if (useRecorder)
            {
                if (imageSequence) backend.EndImageSequence();
                else backend.EndVideo();
            }
            _audioDirector.StopAll();
            if (imageSequence && Directory.Exists(sequenceDir))
                _output.Manifest.filesGenerated.Add("Master/" + Path.GetFileName(sequenceDir) + "/");
            else if (!imageSequence && File.Exists(masterFile))
                _output.Manifest.filesGenerated.Add("Master/" + Path.GetFileName(masterFile));
            else if (useRecorder)
                _output.Manifest.validationIssues.Add("recorder:master-missing");
            if (!useRecorder)
                _output.Manifest.validationIssues.Add(
                    "recorder-unavailable: video backend not registered; run via the Studio window or CLI");
        }

        private IEnumerator TransitionIn(ShotTransition t)
        {
            switch (t)
            {
                case ShotTransition.Fade:
                case ShotTransition.FadeThroughBlack:
                    yield return FadeQuad(0f, 1f, 0.25f);
                    yield return FadeQuad(1f, 0f, 0.35f);
                    break;
                case ShotTransition.Dissolve:
                    yield return FadeQuad(0f, 0.6f, 0.15f);
                    yield return FadeQuad(0.6f, 0f, 0.25f);
                    break;
                case ShotTransition.Whip:
                    yield return FadeQuad(0f, 0.8f, 0.08f);
                    yield return FadeQuad(0.8f, 0f, 0.12f);
                    break;
                case ShotTransition.LightImpact:
                    _audioDirector?.Accent(FindAccentKey("whoosh"), 0.5f);
                    yield return FadeQuad(0f, 0.35f, 0.1f);
                    yield return FadeQuad(0.35f, 0f, 0.2f);
                    break;
                default:
                    yield break; // Cut / MatchCut
            }
        }

        private IEnumerator FadeQuad(float from, float to, float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                _overlay?.SetFade(Mathf.Lerp(from, to, t / seconds));
                yield return null;
            }
            _overlay?.SetFade(to);
        }

        private string FindAccentKey(string hint)
        {
            // Only return a key that actually matches the hint — a random SFX
            // (e.g. a UI click) as a combat accent is worse than none.
            if (_index?.sfxKeys == null) return string.Empty;
            foreach (var k in _index.sfxKeys)
                if (k.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0) return k;
            return string.Empty;
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private void SuspendGameplayCameras()
        {
            _suspendedCameras.Clear();
            _suspendedListeners.Clear();
            if (!_gameplay.GameplayScene.IsValid()) return;
            foreach (var root in _gameplay.GameplayScene.GetRootGameObjects())
            {
                foreach (var cam in root.GetComponentsInChildren<Camera>(true))
                {
                    if (cam == _captureCamera) continue;
                    if (cam.enabled) { _suspendedCameras.Add(cam); cam.enabled = false; }
                    var listener = cam.GetComponent<AudioListener>();
                    if (listener != null && listener.enabled)
                    { _suspendedListeners.Add(listener); listener.enabled = false; }
                }
            }
        }

        /// <summary>Enables/disables URP renderer features whose type lives in
        /// the FogOfWar namespace. Marketing captures disable them entirely —
        /// fog is a gameplay readability layer, not marketing content.
        /// Original states are stored so teardown restores faithfully.</summary>
        private void SetFogRendererFeatures(bool active)
        {
            try
            {
                // Renderer feature instances live on the renderer data assets —
                // FindObjectsOfTypeAll covers loaded ScriptableObjects.
                var features = Resources.FindObjectsOfTypeAll<
                    UnityEngine.Rendering.Universal.ScriptableRendererFeature>();
                if (!active)
                {
                    _fogFeatureStates.Clear();
                    foreach (var feature in features)
                    {
                        if (feature == null) continue;
                        var tn = feature.GetType().FullName ?? string.Empty;
                        if (tn.IndexOf("FogOfWar", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _fogFeatureStates[feature] = feature.isActive;
                            feature.SetActive(false);
                        }
                    }
                }
                else
                {
                    foreach (var kv in _fogFeatureStates)
                        if (kv.Key != null) kv.Key.SetActive(kv.Value);
                    _fogFeatureStates.Clear();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MarketingStudio] Could not toggle fog renderer features: {ex.Message}");
            }
        }

        private void RestoreGameplayCameras()
        {
            foreach (var c in _suspendedCameras) if (c != null) c.enabled = true;
            foreach (var l in _suspendedListeners) if (l != null) l.enabled = true;
            _suspendedCameras.Clear();
            _suspendedListeners.Clear();
        }

        private float AzimuthBase(int shotIndex) => 35f + shotIndex * 57f; // spread angles

        private int MsaaForQuality() => _recipe.quality switch
        {
            MarketingQualityTier.Master => 8,
            MarketingQualityTier.Production => 4,
            _ => 1,
        };

        private bool AnyShotFailed()
        {
            var shots = _output?.Manifest?.shots;
            if (shots == null || shots.Count == 0) return false;
            int failed = 0;
            foreach (var s in shots) if (s.status == "failed") failed++;
            return failed > 0 && failed >= shots.Count / 2;
        }

        private bool Failed() => _cancelRequested || !string.IsNullOrEmpty(_status.error);

        private void SetPhase(string phase, float progress, string message)
        {
            _status.phase = phase;
            _status.progress01 = progress;
            _status.message = message;
            _status.Write();
            Debug.Log($"[MarketingStudio] {phase} ({progress:P0}) {message}");
        }

        private IEnumerator Teardown(string finalStatus)
        {
            SetPhase("teardown", 0.95f, "Finalizing");
            _overlay?.HideText();
            _overlay?.HideLogo();
            _overlay?.SetFade(0f);
            _audioDirector?.StopAll();
            _uiPolicy?.Restore();
            _lighting?.Restore();
            SetFogRendererFeatures(true);
            RestoreGameplayCameras();
            if (_captureListener != null) _captureListener.enabled = false;

            if (_output != null)
            {
                _output.Manifest.status = _cancelRequested ? "cancelled" : finalStatus;
                _output.WriteManifest();
                _status.manifestPath = _output.Layout.ManifestFile;
                if (finalStatus == "ok" || finalStatus == "partial")
                    _output.CleanupTemp();
            }
            _status.done = true;
            _status.cancelled = _cancelRequested;
            _status.progress01 = 1f;
            _status.Write();
            _running = false;
            Debug.Log($"[MarketingStudio] Run finished: {finalStatus}. Output: {_output?.Layout?.RunFolder}");

            if (exitPlayModeOnFinish)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            yield break;
        }

        private static string GitSha(string rev)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("git", $"rev-parse {rev}")
                {
                    WorkingDirectory = MarketingOutputLayout.ProjectRoot(),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                var p = System.Diagnostics.Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit(3000);
                return p.ExitCode == 0 ? output : string.Empty;
            }
            catch { return string.Empty; }
        }

        private void OnDestroy()
        {
            SetFogRendererFeatures(true); // renderer features persist on the URP asset
            _overlay?.Dispose();
            _lighting?.Restore();
            _uiPolicy?.Restore();
            RestoreGameplayCameras();
        }
    }
}
