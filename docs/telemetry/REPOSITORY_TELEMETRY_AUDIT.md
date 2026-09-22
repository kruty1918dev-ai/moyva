# Repository Telemetry Audit — Moyva

Date: 2026-09-22. Branch at audit: `improvement/moyva-production-polish` @ `644657b56`. Clean tree.

## Toolchain

- Unity editor **6000.6.2f1** (`ProjectSettings/ProjectVersion.txt`); migrated same day in `e888cc3ff`.
  Local machine has **6000.3.10f1** installed — Unity cannot open a newer-version project,
  so editor verification of the package runs in a minimal harness project at `6000.3.10f1`
  (package declares `"unity": "6000.3"` minimum; APIs used are stable across 6000.3–6000.6).
- dotnet SDK 8.0.425, Python 3.14, Node 22 available for out-of-editor verification.
- Scripting backend: IL2CPP-capable project; API compatibility .NET Standard 2.1 era (Unity 6).
- CI: `.github/workflows/` — file-size guardrails, naming policy, source quality,
  artifact hygiene, multiplayer smoke, pages. Guardrail scripts target `Assets/Moyva/Scripts/**`.

## Package / asmdef graph

- One embedded local package already exists: `Packages/com.kruty1918.moyva.unityhtml`
  (convention: `com.kruty1918.<name>`, `Runtime/`, `Editor/`, README, meta files tracked).
- ~60 first-party assemblies `Kruty1918.Moyva.*`, each with own `.asmdef`.
- Test assemblies: `Kruty1918.Moyva.Tests.<Feature>`, `includePlatforms:["Editor"]`,
  `overrideReferences` with `nunit.framework.dll` (+ `Zenject-usage.dll` where needed),
  `optionalUnityReferences: ["TestAssemblies"]`.
- External deps relevant here: `com.unity.nuget.newtonsoft-json` 3.2.2,
  `com.unity.ml-agents` 4.1.0, `com.unity.ai.inference` 2.6.1 (ONNX),
  `com.unity.netcode.gameobjects`, Unity Services (auth/relay/lobby), Zenject (plugins).

## DI / frameworks

- Zenject everywhere. `SignalBusInstaller` (`Features/Signals/Runtime`) declares ~60 signals.
- Composition roots: `Bootstrap/Runtime/ProjectServicesInstaller.cs` (project),
  `Bootstrap/Runtime/BootstrapInstaller.cs` (gameplay scene).
- Feature modules own their bindings; installers delegate, do not duplicate.

## Event buses / domain events

- `Zenject.SignalBus` is THE domain event surface. Declared signals cover:
  game start/end/pause, game mode, building placed/cancelled/preview/demolished/ownership/
  operational, construction rejections, unit created/moved/destroyed/garrison, recruitment
  queue/ready/deployed/rejected, move requests/rejections, economy tick, settlement
  created/deactivated/captured/resource changed/deficit/starter pack, caravan delivery,
  construction supply, population, fog state changed, tile changed, world built/generated/
  spawn positions, info panels open/close, world selection, focus ping, save requested/
  completed, load requested, multiplayer room lifecycle (lobby services).
- `Shared/Diagnostics/RuntimeDiagnostics.cs` — diagnostic logging (no DI installer).
- `UIActions/Runtime/UiActionRouter.cs` + `UiEscapeRouter.cs` — UI action journal (`_journal.Record`).

## Lifecycle

- Application: Unity `Application` callbacks; boot scene 0 in `Bootstrap/Runtime/Boot/`.
- Session: `GameStateService` fires `GameStartedSignal` / `GameEndedSignal{WinnerId}` /
  `GamePausedSignal`. Menu → `GameplaySession` → `GameplayStartupPipeline` → `GameLaunchContext`.
- Turns: `Turns/API/TurnContracts.cs` — `ITurnParticipant` with OnTurnStarted/OnTurnEnding/
  OnRoundCompleted; `TurnService`, `RoundResolutionService`.
- Match end: `GameEndedSignal.WinnerId` (null = draw/cancel). `ExitMatchCoordinator` handles exit.

## Save / replay / determinism

- `SaveSystem` feature: `SaveService`, `SaveModuleRegistry`, per-feature save modules
  (e.g. `UnitsSaveModule`, `FogOfWarSaveModule`). `SavePlayModeOptions` for launch options.
- No general deterministic replay system found; bot decisions carry `ContractHash` +
  `ObservationHash` (FNV-1a) in `BotDecisionTrace` — partial determinism evidence for AI.

## Serialization

- `Jsonization` feature: `MoyvaJsonRuntime`, `MoyvaJsonTypeRegistry`, `MoyvaJsonAssetCatalog`
  — JSON is the config source of truth under `Assets/Moyva/Presets/` (AGENTS.md policy:
  Load → Validate → Resolve → Freeze → Consume; no AssetDatabase in runtime).

## Networking / multiplayer

- `Features/Multiplayer`: `ISessionManager`, LAN + UGS Relay providers, `GameCommandSyncService`,
  `MultiplayerAuthorityService` (host authorizes construction/unit commands), host migration,
  startup barrier. Signals surface command rejections (`UnitMoveRejectedSignal`,
  `ConstructionPlacementRejectedSignal`, `UnitRecruitmentCommandRejectedSignal`).

## AI / bot / training

- `Assets/Moyva/AI/Bot/Core`: Perception (`BotPerceptionSnapshot`, tactical encoder) →
  Decision (`BotDecisionOrchestrator`, `BotDecisionFrameBuilder`, `BotDecisionContract` with
  contract hash) → Capabilities (`Capture/Combat/Construction/Movement/Recruitment/EndTurn`)
  → Execution (`BotExecutionResult`). Policy drivers: heuristic + ML-Agents
  (`MoyvaMlAgentsPolicyDriverFactory`, `MoyvaBotPolicyAgent`).
- **`BotTelemetryHub`** (`AI/Bot/Core/Telemetry/`): in-memory ring buffer of `BotDecisionTrace`
  (session, turn, sequence, candidates, intent, capability, mode, result, failure, latency,
  contract hash, observation hash) + `BotRunMetrics` + `BotLearningHealthEvaluator`.
  Called from `BotDecisionOrchestrator` (~line 192). Bound in `BotRuntimeInstaller`.
  → Integration: add `TraceRecorded` event hook (additive) and bridge to the new sink.
- `AI/Training`: `TrainingEnvironment`, `TrainingRewardEvent`, `TrainingMetricsHub`,
  curriculum config, `TrainingDiagnostics`, training CLI in `tools/ai/` (`moyva_train.py`).
- Training artifacts (decisions.jsonl style) are produced by the Python/tooling side;
  the runtime bridge is `TrainingBotBridge` + `GameplayTrainingEpisode`.

## UI architecture

- HomeMenu: dynamic `MoyvaUI` shell (`HomeMenuMoyvaUiPresenter/ViewController/State/Markup/Bridge`)
  on embedded `com.kruty1918.moyva.unityhtml` package (ReactUnity-based HTML UI).
- Gameplay HUD: `GameplayHudBindings`, `GameplayHtmlPresenter/Bridge/State/Markup`.
- Panel signals exist (`WorldInfoPanelRequested/Closed`, `BuildingInfoPanelRequested/Closed`,
  `UnitInfoPanelRequested`, `MapObjectInfoPanelRequested`, `WorldInfoSelectionChangedSignal`).
- `UiActionRouter` journal records UI action requests/results — natural telemetry tap.

## ScriptableObjects / configuration

- Presets under `Assets/Moyva/Presets/` are JSON (policy). A few legacy SOs exist
  (`SelectionHighlightSettingsSO`, audio configs). Telemetry config must be JSON-compatible.

## Tests / runners / CI

- EditMode tests per feature under `Tests/Editor/`; run via
  `tools/ai/unity-editmode-tests-quiet.sh <filter>` (Unity batch `-runTests`).
- Quiet build: `tools/ai/dotnet-build-quiet.sh <csproj>` (uses Unity-generated csproj files;
  NOTE: csproj files are gitignored and only regenerated by the editor).
- `Library/ScriptAssemblies/*.dll` present → allows compile-checking new code via csc/dotnet
  against the already-built Moyva assemblies without opening the editor.

## Existing logging / analytics / telemetry

- `RuntimeDiagnostics` (shared diagnostics log), `BotTelemetryHub` (in-memory),
  training reward/metrics hubs, UI action journal. **No persisted/uploaded telemetry exists.**
- No Unity Analytics / external analytics SDK wired for gameplay events.

## Candidate telemetry sources (chosen integration points)

| Boundary | Mechanism |
|---|---|
| Session/match lifecycle | `GameStarted/GameEnded/GamePausedSignal` |
| Mode | `GameModeChangedSignal` |
| Construction | `BuildingPlaced/Cancelled/…`, `ConstructionPlacementRejectedSignal` |
| Units/movement | `UnitCreated/Moved/Destroyed`, `MoveUnitRequest`, `UnitMoveRejected` |
| Recruitment | `UnitRecruitmentQueue/Ready/Deployed/Rejected` signals |
| Economy | `EconomyTickCompleted`, `Settlement*`, `ResourceDeficit` signals |
| Fog | `FogStateChangedSignal` (aggregate counts only) |
| World gen | `WorldBuilt/WorldGeneratedData/WorldSpawnPositions` signals |
| Save | `SaveCompleted/LoadRequested` signals |
| UI | panel + selection signals, `UiActionRouter` journal bridge (optional) |
| AI decisions | `BotTelemetryHub.TraceRecorded` (new hook) → decision events |
| Turns | `ITurnParticipant` adapter registered with `TurnService` |
| App lifecycle/perf | Unity layer `TelemetryHostBehaviour` (pause/quit/perf sampling) |

## Systems intentionally NOT instrumented

- Per-frame cursor position, per-frame camera state, full world snapshots (cost >> value).
- Chat/free-text user input (none exists in gameplay path), device precise location,
  hardware fingerprinting, account PII (product layer does not expose it).
- Visual/rendering internals (FlatKit, fog screen-space textures) — presentation only.
- Marketing Studio capture internals (dev tool, not gameplay evidence).

## Risks / integration notes

- Project targets Unity 6000.6.2f1 but only 6000.3.10f1 installed → editor-open of the
  Moyva project is blocked; mitigated by harness-project verification + assembly compile.
- Zenject `SignalBus` in tests requires signals to be declared or `.OptionalSubscriber()`.
- `BotTelemetryHub` is created in 3 places (installer, training bridge, tests) — the
  `TraceRecorded` hook is additive and safe for all.
- `file-size-guardrails`/`naming-policy` CI applies to `Assets/Moyva/Scripts/**` — new
  adapter code must use sanctioned suffixes (Service/Adapter/Installer/Resolver/…).
