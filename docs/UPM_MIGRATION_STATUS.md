# UPM Modularization Status

Branch: `refactor/upm-modularization-20260922` — base `origin/main` @ `1258a5fcc`.
Worktree: `moyva-upm-refactor`. Unity: `6000.6.2f1` (ProjectVersion).

## Inventory (re-done on this branch)

- `Packages/manifest.json`: 15 new embedded `file:` packages added alongside existing `com.kruty1918.moyva.unityhtml`.
- Build scenes: `Assets/Moyva/Scenes/Boot.unity`, `Assets/Moyva/Scenes/HomeMenu.unity` (+ gameplay scenes via scene flow).
- asmdef DAG (`tools/quality/feature_dependency_map.py --check-cycles`): **33 modules, 168 edges, 0 cycles** — before and after extraction (packages add nodes, no cycles introduced).
- Package→game imports: none — all extracted Runtime sources compile without `Kruty1918.Moyva.*` and without Zenject.

## Verification status

- Roslyn smoke compile (`tools/ai/smoke_compile.py`): 1615 sources, 520 refs, **0 errors** (21 suppressed `Camera`-namespace false positives — single-assembly artifact; real asmdef isolation does not hit them).
- Unity Editor `6000.6.2f1` is not installed on this machine (installed: `6000.3.10f1`); `unity test --allow-install` was blocked by a cancelled elevation prompt. **EditMode/PlayMode suites were NOT run under Unity** — the smoke compile is source-level only. Required follow-up: run full EditMode suite + boot/menu/gameplay smoke on a machine with 6000.6.2f1.
- Serialized safety: all moves used `git mv` (`.meta`/GUID preserved); `m_EditorClassIdentifier` YAML refs were patched for moved managed-reference types (`JsonAssetCatalog`, `JsonBindingMarker`); scene-serialized `CalendarInstaller`/`CalendarSessionConfigSO`/`AdaptivePerformanceSettingsSO`/`AudioRegistrySO`/`SceneAudioOverridesSO`/`SceneMusicProfileSO`/`VfxCatalogConfig` kept game-side identity.

## Candidate decisions (31)

### Extracted (15)

| # | Package | Game-side remainder |
|---|---|---|
| 1 | `com.kruty1918.json-config` (`Kruty1918.JsonConfig`) | `MoyvaJsonBootstrap`, `MoyvaJsonRuntimeSettings`, editor Jsonization tooling, JSON presets |
| 2 | `com.kruty1918.save-system` (`Kruty1918.SaveSystem`) | `SaveService`, `ConfigService`, `SaveModuleRegistrar`, `ExitMatchSaveHandler`, `SaveSystemInstaller` (Signals/GameMode-coupled) |
| 3 | `com.kruty1918.game-calendar` (`Kruty1918.Calendar`) | `CalendarInstaller`, `CalendarSessionConfigSO`, `CalendarBinaryConfigStore`, `MoyvaCalendarDefaults` (scene-serialized) |
| 4 | `com.kruty1918.localization` (`Kruty1918.Localization`) | `MoyvaLocalizationDefaults` (language table, font/paths) |
| 5 | `com.kruty1918.audio` (`Kruty1918.Audio`) | `AudioRegistrySO`, `SceneAudioOverridesSO`, `SceneMusicProfileSO` (implement `IAudioCatalog`/`IAudioSceneOverrides`/`IMusicSceneProfile`), `ButtonAudioComponent`, `AudioInstaller`/`MusicInstaller` |
| 6 | `com.kruty1918.vfx` (`Kruty1918.Vfx`) | `GameplayVfxService` (domain signals), `VfxInstaller`, `VfxEventIds`, `VfxCatalogConfig`, `MoyvaVfxQualityPolicy` |
| 7 | `com.kruty1918.notifications` (`Kruty1918.Notifications`) | `NotificationsInstaller`; optional DOTween via version define |
| 8 | `com.kruty1918.input-context` (`Kruty1918.InputRouting`) | `InputRoutingBindings` (Zenject adapter) |
| 9 | `com.kruty1918.ui-actions` (`Kruty1918.UIActions`) | `UiActionIds`, `MoyvaUiActionCatalog`, `UiActionsInstaller` (tick/escape wiring) |
| 10 | `com.kruty1918.ui-foundation` (`Kruty1918.UiFoundation`) | `UiCanvasScalePolicy` (Moyva 1280×720 policy), `UiTooltipTrigger` (Zenject component), `UiFoundationTickable` adapter |
| 11 | `com.kruty1918.runtime-diagnostics` (`Kruty1918.Diagnostics`) | `StartupFrameCapture` (Moyva env vars, e2e dev tool) |
| 12 | `com.kruty1918.connectivity` (`Kruty1918.Connectivity`) | Zenject lifecycle wiring in `SharedInstaller` |
| 13 | `com.kruty1918.adaptive-performance` (`Kruty1918.Performance`) | `AdaptivePerformanceSettingsSO`, `AdaptivePerformanceDefaultsProvider`, tuning structs |
| 14 | `com.kruty1918.motion` (`Kruty1918.Motion`) | — (pure: `MotionEaseKind`, `EntityMotion`, `PathTraversalMotion`) |
| 28 | `com.kruty1918.entity-health` (`Kruty1918.EntityHealth`) | Combat damage/death rules, replication, `CombatInstaller` binding |

### Retained this pass (evidence-based)

| # | Candidate | Decision | Evidence / boundary |
|---|---|---|---|
| 15 | strategy-camera | retain | 16 files in `Features/Camera/Runtime`; deps: `Grid.API` (7), `Shared.Controls` (4), `Signals`, `MapChunks.Runtime`. Pure orbit/zoom math is extractable but `CameraFocusService`/`CameraGestureArbiter` tie to grid+input policy. Gate: split math core vs scene policy needs its own pass. |
| 16 | map-chunks | retain (boundary proven) | 20 files; only external dep is `Moyva.Signals` (3 files) — world-event signals need an adapter seam before the stores/culling core can move. |
| 17 | spatial-grid | retain | Projections live inside `Kruty1918.Moyva.Grid` asmdef; extraction requires splitting the feature's own API (tile types used by pathfinding/units). Do together with map-chunks in a geometry pass. |
| 18 | spatial-occupancy (У) | merge w/ 16 | `ChunkedObjectsMapService` has no independent invariants beyond the chunk stores — stays with map-chunks/game per task guidance. |
| 19 | pathfinding | retain | `Pathfinder` + neighbour strategies consume `Moyva.Grid.API` tile types (6 dep sites) — blocked on the spatial-grid split, not standalone. |
| 20 | visibility-state | retain | 17 files, `FogOfWar.API` is one coherent module (16 self-refs + Signals); extracting the state grid alone fragments fog. |
| 21 | vision-geometry | retain | `HeightAwareVisionEngine` uses `FogOfWar.API` + `Grid.API`; needs fog/grid seams first. |
| 22 | fog-urp | retain | 24 files; URP renderer feature + Grid/MapChunks/Signals deps; requires PlayMode graphics verification — Unity 6000.6.2f1 unavailable locally. |
| 23 | world-geography | retain (near-clean) | 11 files, 1 `Generator.API` dep — extractable next pass once `Generator.API` seam (map recipe types) is defined. |
| 24 | map-recipe | retain | 6 files w/ 5 `Generator.API` + ObjectPlacement deps; recipe validator is coupled to generator-internal types. |
| 25 | chunk-terrain-mesh | retain | 31 files; TWC provider + `Generator.API`/`MapChunks.API` coupling — needs provider interface split. |
| 26 | map-preview (У) | retain | 46 files pulling `Construction.Runtime`, `Grid.API`, `Generator.API` — fails the gate (drags menu/construction). |
| 27 | spatial-scatter (У) | retain in game | single consumer (`ObjectPlacementScatterUtility`) — no standalone API justified; stays inside generator. |
| 29 | transport-core | retain | `ReliableTransportChannel`/`MultiplayerFrameCodec` live inside `Multiplayer.Runtime`; extraction needs protocol-version boundary audit (LAN/Relay regression risk) — scheduled with network pass. |
| 30 | session-lifecycle (У) | retain | `SessionManager`/`HostMigrationService` deeply coupled to `Multiplayer.Config/Lobbies/Networking/Persistence` + turn authority — gate not passed. |
| 31 | urp-render-features (У) | retain | `DayNightScreenFilterFeature` reads `Calendar.Runtime`; stylized art + shaders are game assets. Requires PlayMode graphics verification anyway. |

## Files

- Moved: ~70 source files into `Packages/com.kruty1918.*/Runtime/` (all via `git mv`, `.meta` preserved).
- Game adapters added: `MoyvaUiActionCatalog`, `MoyvaLocalizationDefaults`, `MoyvaAudioInstaller` (AudioInstaller+MusicInstaller), `UiTooltipTrigger` (game file), `MoyvaVfxQualityPolicy`, `JsonConfigRuntimeSettings` wiring, lifecycle adapters in `SharedInstaller`/`UiActionsInstaller`.
- Removed: dead `Kruty1918.Moyva.Combat.Runtime`/`Moyva.Shared.Connectivity`/`Moyva.Audio.*` namespaces, emptied `IVfxService.cs` shell, dangling `Notifications.meta` folder meta.

## Residual risks

- Unity EditMode/PlayMode not run (editor install blocked — cancelled UAC). Roslyn smoke is green but is not a Unity-compile substitute.
- Zenject lifecycle semantics changed from interface-driven (`IInitializable`/`ITickable` via `BindInterfaces*`) to explicit `OnInstantiated`/`ITickable` adapters — behavior-equivalent in ordering for the touched services, but verify boot smoke.
- `ButtonAudioComponent`/`UiTooltipTrigger` keep `ProjectContext` fallback — unchanged behavior, documented as adapter pattern.
- Next autonomous steps: Unity 6000.6.2f1 verification run → map-chunks+spatial-grid geometry pass → fog family → generator/geography/recipe/mesh → transport-core.
