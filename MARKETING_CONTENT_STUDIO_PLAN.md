# Moyva Marketing Content Studio — Architecture & Implementation Plan

Status: implemented on `tooling/marketing-content-studio`. This document is the design
contract for the studio; `MARKETING_CONTENT_STUDIO.md` is the user guide.

## 1. Existing reusable systems (audit results)

| Need | Canonical system |
|---|---|
| World build | `MapVisualInstantiator.BuildWorld()` via `GeneratorWorldStartupBuilder`, driven by `GameLaunchContext` (seed/size/mapType via `ConfigureMenuNewGame`) |
| Gameplay scene | `Assets/Moyva/Scenes/Gamplay_Scene.unity` — full Zenject bootstrap, self-fallback to `DirectGameplayTest` |
| Units | `Presets/Units/*.json` (`id`, `prefab.editorPath`, role, combat stats); runtime `IUnitService` |
| Buildings | `Presets/Buildings/*.json` (`identity.id`, `presentation.prefab.editorPath`, category/role) |
| Tiles / map objects | `Presets/Tiles/*.json`, `Presets/Generator/map-object-registry/*.json` |
| Audio | `IAudioService`/`AudioService`, `IMusicService`/`MusicService`, `AudioRegistrySO` (JSON: `moyva-audio-registry--moyvaaudioregistry.json`), `Assets/Moyva/Audio/MoyvaMixer.mixer`, NaPH RPG & Fantasy bundle under `Assets/ThirdParty/Audio/` |
| Camera (gameplay) | `Features/Camera` (`ICameraMovement`, `ICameraZoom`) — left untouched; studio uses its own capture camera |
| Menu cinematic preview | `HomeMenu/UI/Preview/HomeMenuBackgroundPreviewController*` — endless background; reused as reference only, not as trailer director |
| Legacy capture lab | `Editor/UnityCliBridge/GameplayUiCaptureLab/` — `#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR`; ideas reused (window pixel capture, scene presets), code not depended on |
| Capture backend | `com.unity.recorder` 5.1.7 already in `Packages/manifest.json` — primary video/image-sequence/audio backend |
| JSON pipeline | `Load → Validate → Resolve → Freeze → Consume`; presets live under `Assets/Moyva/Presets/` |
| DI | Zenject. Studio scene uses a lightweight composition root; gameplay services are resolved from the gameplay `SceneContext` container, not re-bound |

## 2. Current capture capabilities

- Unity Recorder 5.1.7 (editor): movie (MP4/WebM), image sequence (PNG), audio capture, constant-frame-rate mode → deterministic pacing independent of editor FPS.
- `ScreenCapture`/render-to-`RenderTexture` path for exact-resolution stills without GameView coupling.
- Legacy window-pixel capture exists but is legacy-gated → not a dependency.

## 3. Existing audio capabilities

- `AudioBus` (Master/Music/Sfx/Ui/Ambience), pooled `AudioService` with `Play`/`PlayAt`, per-bus volume, `AudioRegistrySO` key→definition map (clip, variants, bus, mixer group, loop).
- `IMusicService` scene profiles + epic mode; scene profiles via `SceneMusicProfileSO`.
- Licensed-for-project audio: NaPH bundle (music loops incl. `Epic Combat Music Loop`, `Tavern Music Loop`; SFX families: animals, crafting, impacts, footsteps, water, ambience). No external/copyrighted music is introduced.

## 4. Existing cinematic capabilities

- HomeMenu world preview builds a live world mesh + camera pan (endless loop, no narrative arc, no cuts, no ending) — explicitly not a trailer director. Studio adds `MarketingTrailerDirector` with beats/structure/ending.

## 5. Existing camera capabilities

- Player camera = free-pan/zoom strategy camera (`CameraMovement`, `CameraZoom`, edge scroll, bounds provider). Not suitable for shots; the studio owns a dedicated capture camera and politely suspends the gameplay camera during capture (single `AudioListener` guarantee).

## 6. Asset discovery architecture

`MarketingContentIndex` (snapshot) is built **editor-side** by `MarketingContentIndexBuilder`:

- Scans `Presets/Units`, `Presets/Buildings`, `Presets/Tiles`, `Presets/Generator/map-object-registry`, audio registry JSON, and `ThirdParty/Audio` clip paths.
- Each entry: `id`, `category` (Unit/Building/Environment/Terrain/Effect/Landmark/Audio), `editorPath`, `assetGuid`, `role/tags`, prefab stats (renderer count, material check, animator/clip presence, bounds size class).
- Metadata overlay `Presets/Marketing/metadata/*.json` adds marketing fields (value tier, size, animation support, shot suitability, biome, pin/ban/prefer/avoid, disabled flag) keyed by content id — third-party prefabs are never edited.
- Fingerprint = hash over (file paths + GUIDs + registry ids). Stored in the snapshot; runtime/editor compare on open/play → stale index triggers automatic rebuild. New/removed units, buildings, audio, animations are picked up after every `game-process` merge with zero manual list maintenance.

## 7. Studio architecture

```
Kruty1918.Moyva.Marketing (runtime asmdef, mostly plain C#)
├── Contracts/            recipes, platform profiles, shots, beats, manifest DTOs, enums
├── Content/              index snapshot model, scoring, validation rules
├── Planning/             ShotPlanner, ShotSequencePlanner, GoldenFrameEvaluator (pure C#)
├── Trailer/              TrailerBeatLibrary, MarketingTrailerDirector sequencing model
├── Text/                 MarketingClaimCatalog, claim predicates, typography templates
├── Runtime/              MonoBehaviour host: MarketingStudioController, camera director,
│                         lighting director, UI visibility policy, audio director,
│                         scenario staging, RT screenshot capture, overlay canvas
└── Output/               OutputManager (folder layout, manifest, report)

Kruty1918.Moyva.Marketing.Editor (editor asmdef)
├── MarketingContentIndexBuilder   AssetDatabase scan → index snapshot JSON
├── MarketingStudioWindow          Moyva → Marketing Content Studio
├── MarketingRecorderPipeline      Unity Recorder orchestration (movie/sequence/audio)
├── MarketingCli                   -executeMethod entry points for batch runs
├── ContactSheetBuilder            thumbnail sheet from run manifest
└── MarketingSceneBuilder          generates/updates MarketingStudio.unity
```

Zenject is not used to bind studio services into the gameplay container; the studio is a
separate concern. `MarketingStudioController` is the composition root. Gameplay APIs
(`IUnitService`, `IGridService`, `IAudioService`, `IMusicService`…) are resolved lazily
from the gameplay `SceneContext` (`SceneContext.Container`) when the gameplay scene loads.

## 8. Scene architecture

`Assets/Moyva/Scenes/MarketingStudio.unity` — thin controlled capture environment:

- `MarketingStudio` root: `MarketingStudioController` (recipe id, autoStart, preview mode),
  capture camera rig root (own `Camera` + `AudioListener`), marketing overlay canvas
  (text/logo/end card), lighting override rig.
- On Play: `GameLaunchContext.ConfigureMenuNewGame(slot:95, seed, size, mapType, difficulty, 2 players)`
  → load `Gamplay_Scene` additively → set active → world builds via canonical pipeline.
- `AUTO START CAPTURE` toggle on the controller (and window): ON = run the selected recipe
  end-to-end and write outputs; OFF = stage the world and leave the rig live for inspection.
- Cancellation: ESC or window button → safe teardown (stop Recorder, unload gameplay scene,
  release RTs, restore lighting/UI, write cancelled report).

## 9. Capture pipeline

- Stills: capture camera → `RenderTexture` (master resolution, HDR off, MSAA per quality
  profile) → `ReadPixels` → PNG. No GameView dependency; exact resolution.
- Video: editor `RecorderController` + `MovieRecorderSettings` (GameView or camera input,
  constant frame rate, `PreserveAudio` for audio variants). Prepared/started/stopped by the
  editor pipeline while play mode runs; runtime exposes `MarketingRunState` progress via a
  JSON status file the editor polls.
- Image sequence: `ImageRecorderSettings` (PNG) for editorial-grade export.
- Timeouts per phase (world build, staging, per-shot, recorder flush); failure → retry with
  alternate subject/seed → marked failed in report, never silently dropped into output.

## 10. Trailer pipeline

`MarketingTrailerDirector` turns a recipe into a `TrailerPlan`:

1. `TrailerBeatLibrary.BeatsFor(duration)` → ordered beats (HOOK/WORLD/BUILD/EXPAND/
   TENSION/ACTION/CLIMAX/END) scaled to duration; 15s/30s variants collapse beats.
2. `ShotPlanner` maps each beat → shot candidates from the live world (subjects from
   content index + staged scenario state) with rig, framing, duration, transition.
3. `MarketingCameraDirector` executes shots on the capture camera; `MarketingAudioDirector`
   runs music + accent SFX + ducking; overlay canvas renders beat text/end card when the
   recipe allows text.
4. Recorder writes `master` (+ `clean`, `no-text` variants by re-render or overlay-off pass).
5. Manifest + `shots.json` + `capture-report.json` written per run.

Restraint rules are enforced in the planner: min shot length, max cut rate, transition
whitelist, no per-second random cameras, no zoom spam (motion envelope per shot).

## 11. Screenshot pipeline

`ScreenshotDirector` plans a diverse shot set (categories: WORLD_BEAUTY, SETTLEMENT,
ECONOMY, BUILDING, ARMY, TACTICAL, COMBAT, EXPLORATION, ATMOSPHERE, UI_GAMEPLAY) with
diversity rules (max repeats per category/subject/biome/lighting). For each shot the camera
samples a short candidate window; `GoldenFrameEvaluator` scores frames (subject visibility,
screen-space occupancy, clipping, overlap, lighting balance, negative space) and the best
frame is written as `master` → platform variants.

## 12. Text/branding pipeline

- `MarketingClaimCatalog` (`Presets/Marketing/claims.json`): each line declares
  `requires` predicates (features detected from the content index: unit count, building
  categories, combat presence, economy presence…) — copy is only emitted when predicates
  hold, so marketing text never invents mechanics. A ban list rejects cliché filler.
- Typography templates (TITLE/BEAT_WORD/SHORT_PHRASE/END_CARD) with per-aspect safe-area
  anchors; localization tables (en/uk/de/custom) per claim id.
- Logo via `Presets/Marketing/branding.json` (sprite `editorPath`, placement modes:
  none/corner/end-card). No baked text in textures.

## 13. Audio pipeline

`MarketingAudioDirector` uses `IAudioService`/`IMusicService` only:

- music key per recipe (default from registry keys marked `Music` bus);
- ambience layer (village/nature keys);
- accent SFX on events (impact on cut to combat, build-complete on settle reveal, sting on
  logo reveal) — sparse by design;
- ducking via `IAudioService.SetBusVolume(Music, …)` envelopes on impacts/end card;
- optional deterministic amplitude analysis of the music clip (editor-side, PCM scan) to
  nudge cut points toward energy peaks — simple signal analysis, no ML.

## 14. Platform profiles

`Presets/Marketing/platforms/*.json` — `PlatformCaptureProfile`:
`id, aspect, resolution(s), safeArea, textAllowed, gameplayOnly, logoAllowed,
uiRecommended, imageFormat, videoFormat, preferred/maxDuration, notes`.
Shipped defaults: generic, steam-screenshot (16:9, gameplay-only, text OFF, prerendered
OFF), steam-capsule, youtube-16x9, youtube-shorts-9x16, tiktok-9x16, instagram-reel-9x16,
instagram-post-1x1/4x5, appstore, google-play, custom.

## 15. Quality profiles

`preview` (960px, MSAA off, fast staging), `production` (1920/2160 master, MSAA x4),
`master` (4K master, MSAA x8, image-sequence capable). Quality scales rendering — never
swaps game assets.

## 16. Automated content-selection logic

`ContentScorer`: `marketingScore = gameplayRelevance + visualQuality + animationQuality +
silhouetteReadability + novelty + shotCompatibility + biomeCompatibility
- repetitionPenalty - knownIssuePenalty`. Pin/ban/prefer/avoid overrides from metadata.
Validator rejects missing prefab/material/animation, magenta shader, invalid scale,
invisible renderer, missing clip before capture.

## 17. Validation

`MarketingValidator` (edit-time + pre-capture): recipe sanity (resolution/aspect/fps/
duration), platform compliance (Steam gameplay screenshot ⇒ no marketing text, no logo,
no prerendered staging flag), content validity, audio key existence, safe-area fit for
text templates. Violations block export or auto-generate a compliant clean variant.

## 18. Testing

EditMode suite `Kruty1918.Moyva.Marketing.Tests` (outside `Runtime/`): index discovery +
fingerprint invalidation, recipe validation, platform compliance (Steam clean),
shot planner diversity + scoring + repeat penalty, beat scaling, golden-frame scoring
determinism, manifest schema, claims predicates + banned-phrase filter, crop/safe-area
math, cancel/cleanup lifecycle, fixed-seed determinism of plans.

## 19. Export structure

```
MarketingOutput/<yyyy-MM-dd_HHmmss>_<recipe>/
├── manifest.json            (commit SHAs, recipe, seeds, files, validation)
├── Metadata/{capture-report.json, shots.json, sources.json}
├── Master/…                 (clean masters: stills, video, sequence)
├── Steam/ YouTube/ Shorts/ Instagram/ AppStore/ …
└── ContactSheet.png         (for review)
```

`MarketingOutput/` is git-ignored; `.temp/` subdir is cleaned on success, detected stale
on next run. `SOURCE_GAME_PROCESS_SHA` recorded via `git merge-base`/HEAD of game-process.

## 20. Definition of Done

Tracked against the spec: scene + window + recipes + platform profiles + content index +
shot planner + screenshot/video/sequence pipelines + audio + text/branding + crops +
manifest + tests + docs + one-click flows + cancel + cleanup + no PR + no merge into
`game-process`.

### Explicit decisions

- **Cinemachine**: evaluated — not in `manifest.json`; adding an unverifiable package on a
  tooling branch risks breaking compile for everyone. Studio implements self-contained
  deterministic camera rigs (the rig abstraction maps 1:1 onto Cinemachine virtual-camera
  concepts if adopted later).
- **FFmpeg**: optional; Unity Recorder covers MP4/WebM/PNG-seq/audio. No hard dependency.
- **Gameplay scene reuse**: additive load of `Gamplay_Scene` inside `MarketingStudio` —
  final frames always reflect a valid, canonically-bootstrapped game state.
