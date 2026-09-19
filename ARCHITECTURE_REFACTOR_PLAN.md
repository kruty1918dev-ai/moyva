# Moyva — Architecture Refactor Plan

Audit and staged refactoring plan for all author-written code. Goal: a codebase where a human or an AI agent needs the minimum possible context to safely change a specific system — clear module boundaries, one canonical path per operation, no dead or parallel implementations.

Key metric: not "fewest lines" but "fewest lines and files you must understand to change a given system safely".

## 1. Executive summary

Moyva has ~215K LOC of author-written C# across 1,660 files and 62 asmdefs. The feature-folder + Zenject + SignalBus skeleton is fundamentally sound — the asmdef graph is acyclic, JSON config policy is respected, namespaces map cleanly to directories. The rot is inside the features: god classes hidden by partial files, dead parallel subsystems (two event layers, two HUD presenters, two fog renderers, three DI composition roots), duplicated per-entity state, and ~91% single-implementation interfaces that blur which seams are real.

The plan sequences 26 patches in 5 phases: safety-net first (characterization tests + architecture tests), then deletion of dead/parallel paths, then state-ownership unification, then canonical-API consolidation and god-class splits, then boundary/naming/docs cleanup. Every patch is independently buildable and testable. Nothing touches third-party code. No gameplay behavior changes except where explicitly marked INTENTIONAL (dead-path deletions cannot change behavior).

## 2. Scope

Author-written C# only:

- `Assets/Moyva/Scripts/**` — 1,525 files (~200K LOC): Features/, Bootstrap/, Shared/, Infrastructure/, Jsonization/, Editor-shared
- `Assets/Moyva/AI/**` — 88 files (~12.7K LOC): Bot/, Training/
- `Assets/Moyva/Editor/**` — 41 files (~11.4K LOC): UnityCliBridge + smoke/probe tools
- `Assets/Moyva/Tests/**` — 5 test files
- `.asmdef` files (62) — boundary definitions, analyzed but not edited without explicit per-patch review
- `Assets/Moyva/Presets/**` JSON — config shape only where it forces code coupling

## 3. Explicitly excluded (third-party / assets)

Never refactored, only adapted-to if needed: DOTween (`Assets/Plugins/Demigiant`), Zenject/Extenject (`Assets/Plugins/Zenject`), Sirenix/Odin (`Assets/Plugins/Sirenix`), TextMeshPro, FlatKit, KayKit, TileWorldCreator (TWC), ML-Agents, UnityHTML (`UnityHTML.Runtime` used by HUD/menu), all of `Assets/Moyva/{Art,Audio,Prefabs,Scenes,Settings,Tiles,UI,Resources,Generated,Data,SO}` asset content, `.meta` files, `Packages/`, `Library/`, `Temp/`, `Logs/`, `tools/` scripts (author tooling — kept; reviewed only for hygiene).

## 4. Uncertain ownership

| File/Area | Why uncertain |
|---|---|
| `Assets/Moyva/PlanarReflectionRenderer.cs` | Root-orphan MonoBehaviour, generic water-reflection utility style typical of Unity community samples; no namespace, no Moyva prefix. Verify provenance before touching. |
| `MessageObservable`, `Unsubscriber`, `ReferenceEqualityComparer` inside `Features/Multiplayer/Runtime/*NetworkProvider.cs` | Vendored UniRx-style fragments embedded in author files (duplicated copies across provider files). Likely copied utility code, not authored. |
| `Assets/Moyva/Data/ScriptableObjects/Generation/{Legacy,Prototype}/` | Asset dirs; "Legacy" naming suggests archived TWC assets; asset lifecycle is out of code-audit scope. |
| `Grid/Runtime/{HexAxial,Isometric,Orthographic3D}GridProjection.cs` | Game is a square-grid strategy (Orthogonal). Alternate projections may be planned features or dead generality — needs product confirmation before removal. |

## 5. Current architecture map

**Layering (actual, not aspirational):**

- `Shared/`, `Infrastructure/InputRouting/` — utilities, audio, graphics, diagnostics
- `Jsonization/` — custom JSON runtime: `MoyvaJsonRuntime` (743 LOC), type registry, asset catalog; all config DTOs derive `MoyvaJsonConfigObject`
- `Signals/` — Zenject `SignalBus` declarations + `SignalDomainEventBridge` (dead) + `IEconomyInfoMediator` (misplaced economy contract in the Signals feature)
- `Features/` — 27 feature folders, each `API/` + `Runtime/` (+`Editor/`, `Tests/`, `UI/` where present)
- `Bootstrap/Runtime/` — gameplay scene composition root + 15-file StartingPosition pipeline
- `AI/Bot`, `AI/Training` — bot runtime + ML-Agents training; consume gameplay via `InstallSimulationBindings` static methods
- `Editor/` — UnityCliBridge (CLI automation bridge, ~11.4K LOC)

**Composition roots (3, parallel):** `BootstrapInstaller` (scene), `GameplayTrainingEpisode` + `TrainingGameplayScope` (headless training container), `MenuGameplaySimulation` (menu background world). Each feature installer exposes `InstallBindings()` (scene) and `InstallSimulationBindings(container)` (training/menu) — the dual-graph seam.

**Feature sizes (LOC / files):** Generator 31.5K/352, Construction 31.9K/218, HomeMenu 19.8K/151, FogOfWar 15.5K/144, Multiplayer 15.4K/102, GameplayHUD 11.2K/39, AI/Training 10.7K/62, Bootstrap 8.4K/53, Economy 8.3K/51, Units 7.8K/47.

## 6. Current dependency map

Namespace-level fan-out (measured, not asmdef-claimed): HomeMenu→20 namespaces, GameplayHUD→18, Construction→17, Units→14, Multiplayer→13, Interactions→12, Economy→10, Bootstrap→20 (composition root, expected).

**Notable edges:** `Units→Bootstrap` (dead import in `UnitsInstaller.cs`, layering inversion), `GameplayHUD→Multiplayer` (UI reaches into net internals), `Construction→FogOfWar` (fog rules inside placement evaluation), `Economy→{Construction,Units}` (settlement/building integration — legitimate), `Interactions→{Construction,Economy,Units,…}` (input aggregator touching everything).

**Canonical owners (current, de facto):**

| State | Owner | Problem |
|---|---|---|
| Building placement/commit | `ConstructionService` | 5+ parallel "apply placement" seams |
| Building health | `BuildingHealthService` | + derived copies in 12 signal subscribers |
| Unit state | `UnitService` | 7 parallel `Dictionary<unitId,T>` instead of a record |
| Economy/settlements | `EconomyManager` + focused services | `IEconomyInfoMediator` lives in the Signals feature |
| Fog | `FogOfWarService` | **two grids**: `_stateGrid` local vs `_ownerStates` per-owner — real divergence bug |
| Turns | `TurnService` | OK |
| World generation | `Generator` + `GraphSystem` | 38 files bind `IGraphRunner`; TWC coupling everywhere inside Generator |
| Session/multiplayer | `MultiplayerAuthorityService` | 2,782 LOC god service |
| Training episode | `GameplayTrainingEpisode` | parallel DI graph; exposes `container.Resolve` via public properties |

## 7. Major architectural problems

1. **God class by partials**: `ConstructionService` — 6,348 LOC across 23 partials, implements 18 interfaces, ~25 injected deps, ~130 public methods. Partials mask the god class; `IConstructionService` itself is labeled "compatibility facade".
2. **Three parallel DI compositions**: scene bootstrap, training scope, menu preview — each hand-maintained; feature `InstallSimulationBindings` drift from `InstallBindings`.
3. **Dead parallel event layer**: `SignalDomainEventBridge` mirrors 15 signals into `*DomainEvent` types with **zero subscribers** (stalled migration TD-001/TD-002 — delete rather than finish; signals already carry domain semantics).
4. **Dead presenters/paths**: `GameplayTurnHudPresenter` (1,532 LOC, 7 partials — never bound), `GameplayRecruitmentPanelView`, UnityHTML HomeMenu path (`UseDynamicMoyvaUi = true` const → `BindSharedSceneUi` + `UnityHTML/` dead), fog `LegacyTwcVolume` render path (~4K LOC under `Visual/Volume` + `Chunking`), `FogOfWarSettings.LegacyOverlay` partial.
5. **Editor one-off tooling**: `RecruitmentPanelFinalAuthoring` 1,278 LOC, `GameplayUiAuditService` 1,337, `GameplayUiRedesignService` 1,143, `JsonizationExportService` 1,230, Pass73/74/82/P09A/P24B/C pass-named migration scripts — ~6-8K LOC of single-use authoring that shipped to HEAD.
6. **Interface proliferation**: ~450 interfaces, ~409 single-implementation; 193 DI-bound single-impl. Real seams (turn authority, fog queries, placement appliers) drown in binding-convention interfaces. 16 `InternalsVisibleTo` point at nonexistent test assemblies; `Shared` exposes internals to `Assembly-CSharp` — global leak.
7. **Service-locator drift**: 447 `Resolve`/`TryResolve` call sites across 121 files; `BotRuntimeInstaller` hand-wires capability ctor args; `GameplayTrainingEpisode` exposes `container.Resolve` as public properties.
8. **Duplicated entity state**: `UnitService` keeps 7 parallel dictionaries (position, owner, type, stamina, vision, garrison, object); 12 subscribers each rebuild building state from `BuildingPlacedSignal`.
9. **Fragmented pipelines**: 15+ `StartingPosition*` services in Bootstrap (selector, policy, camera×2, fog reveal×2, spawn setup, workflow×2, diagnostics, terrain quality, autoload recovery…) — one startup concept scattered across a folder.
10. **HomeMenu is a second game**: 19.8K LOC containing lobby + transport + preview-worldgen + settings + two UI stacks; 28 asmdef refs.
11. **Misleading naming**: `*SO` classes are `MoyvaJsonConfigObject` (JSON DTOs, not ScriptableObjects) — `BuildingRegistrySO`, `UnitRegistrySO`, `EconomyDatabaseSO`, etc.; `TestUnitSpawner` lives in `Bootstrap/Runtime` (production assembly).

## 8. Code smells / hotspots

| Hotspot | Evidence |
|---|---|
| God classes | `ConstructionService` 6,348 LOC/23p/18 ifaces; `MultiplayerAuthorityService` 2,782/7p/16 deps; `HomeMenuBackgroundPreviewController` 2,437/9p; `BuildingPlacementEvaluator` 2,094/9p; `FogOfWarService` 1,801/9p; `EconomyManager` 1,019 |
| Constructor bloat | `TrainingScenarioFacts` 28 params; `GameplayHudReadModel` 24; `GameplayHtmlPresenter` 20; `UnitMovementService` 18; `MapVisualInstantiator` 18; `ConstructionInputService` 17; `UnitRecruitmentDeploymentController` 15 |
| Parallel APIs | 5 placement-commit seams; `IConstructionService` compat facade; `IGraphRunner` sync+async twins; `Fire`/`FireAsync` |
| Event spaghetti | 60 signal types; 12 subscribers to `BuildingPlacedSignal` each deriving state; dead DomainEvent mirror |
| Static mutable state | `SavePlayModeOptions.*`, `TwcModifierCatalog` static registries, `MainThreadDispatcher.Instance`, `TrainingModelInspectorController.Instance` |
| Hidden side effects | `BindInterfacesAndSelfTo(...).NonLazy()` `IInitializable` cascades; signal-driven state rebuild; `_setupPhase` flag in training episode |
| Tests | 12 test files for 215K LOC; InternalsVisibleTo seams exist but no test assemblies |

## 9. State ownership problems

- **Fog**: `_stateGrid` (single/local) vs `_ownerStates` (per-owner) — two truths for the same question; placement reads the local grid, gameplay reads owner-scoped. Already produced a real bug (adjacent enemy castle invisible to the learner in training setup). Needs one canonical owner-scoped model or an explicit documented single accessor.
- **Buildings**: canonical placed set in `ConstructionService.SessionStore` / `ConstructionFootprintStore`, but health (`BuildingHealthService`), occupancy (`IObjectsMapService`), combat targets, settlements (`EconomySettlementRegistryService`), walls (`WallTopologyService`), HUD read-model each keep derived copies — synchronized only via signal ordering.
- **Units**: 7 parallel dictionaries in `UnitService` — no single `Unit` aggregate; every new attribute adds another map (drift risk).
- **Economy**: `EconomyManager` + `EconomyRuntimeApi` facade + `IEconomyInfoMediator` in Signals — three overlapping query surfaces over the same state.

## 10. AI-agent navigation problems

- "How do I place a building?" has 5+ entry points; `TryApplyConfirmedPlacement` vs `TryPlaceAuthoritatively` vs `TryApplySetupPlacement` differ by invisible authority semantics.
- Signal→handler lookup requires searching 60 declarations and all subscribers; no per-signal doc of the canonical consumer.
- `InstallSimulationBindings` vs `InstallBindings` divergence isn't discoverable — an agent must already know the training scope exists.
- Editor pass-named scripts (`P09APatchValidationBridge`, `GameplayUiPass74Installer`) carry zero context — they look like production wiring.
- Dead presenter and live presenter share a folder and naming pattern.
- CODEMAP documents paths but not "which of the 5 placement APIs is canonical".
- 428 docs files, many stale (`docs/standards/domain-events-layer.md` describes a migration that never landed) — agents can't trust the docs.

## 11. Target architecture

Keep the proven skeleton — feature folders, `API/`+`Runtime/` split, Zenject DI, SignalBus, JSON presets — and enforce it.

**Dependency direction (enforced by asmdef + architecture test):**

```text
Bootstrap (composition) → Features → {Signals, Jsonization, Shared}
Presentation (HUD/UI/visual) → Feature APIs only
AI/Bot + AI/Training → Feature APIs + feature install contract only
Multiplayer → Feature command APIs (authority layer, never mutates directly)
Editor → Feature APIs; never a runtime authority
```

**Forbidden edges (new rules):** Feature→Bootstrap, Feature→AI/Training, Feature→other feature's Presentation, Runtime→Editor, anything→`Assembly-CSharp` internals.

**Per-module contract:**

- **Construction** — OWNS: placed-building set, footprints, placement legality, lifecycle. PUBLIC: `IConstructionPlacementQuery`, ONE commit executor + options struct, `IConstructionPlacedBuildingQuery`, demolition/destruction seams. DEPENDS ON: Grid, ObjectsMap, Turns, FogOfWar (query), Economy (costs via mediator), Signals. MUST NOT: Units, Economy internals, presentation.
- **Units** — OWNS: `UnitState` aggregate (position/owner/type/stamina/vision/garrison), movement, recruitment queue, combat commands. PUBLIC: `IUnitService`, `IUnitMovementService`, `IUnitRecruitmentService`, `IUnitCombatService`, ownership/movement queries. MUST NOT: Bootstrap (fix existing edge), Construction internals.
- **FogOfWar** — OWNS: owner-scoped fog state — a single grid model keyed by owner; "local" = local-player alias, not a second grid. PUBLIC: `IFogOfWarService`, `IFogOwnerStateReader`, reveal/vision-source registries.
- **Economy** — OWNS: settlements, resource pools, production, residents. PUBLIC: `IEconomyRuntimeApi` (single facade), `ISettlementRegistry`, `ISettlementCaptureService`.
- **Turns** — OWNS: turn state/history/blockers. PUBLIC: `ITurnService`, `ITurnEndQuery`, participant/blocker SPIs.
- **Generator** — OWNS: world data production; TWC confined behind `IMapDataGenerator` + `MapVisual` — `Twc*` types must not leak into other features (currently contained inside Generator — acceptable, but the boundary must be sealed).
- **Bootstrap** — composition + launch pipeline; collapses `StartingPosition*` into one pipeline folder with ≤5 collaborators.
- **AI/Bot** — decision-frame consumption only; `AI/Training` composes the sim via the SAME feature install contract the scene uses.
- **Multiplayer** — authority/transport over feature command APIs; no direct state mutation.
- **HomeMenu** — menu shell + navigation + world preview; lobby/transport split into its own boundary.

## 12. Target dependency rules

1. One composition contract per feature: `Install(DiContainer, FeatureInstallMode)` where mode ∈ {Full, Simulation, Preview} — scene bootstrap, training, and menu preview all call the same method; `InstallSimulationBindings` becomes a thin wrapper and is deleted once callers migrate.
2. Feature APIs are the only cross-feature surface; `internal` runtime types get `InternalsVisibleTo` only for test assemblies that exist.
3. Signals are the domain-event layer — delete the DomainEvent mirror instead of finishing it (YAGNI; the bridge has had zero subscribers for two documented iterations).
4. No new per-attribute dictionaries; entity state = one record per aggregate.
5. Editor code never carries production logic; one-off migration tooling moves to `tools/` or is deleted after its release.
6. JSON presets remain the only config source; `*SO`-named DTOs get renamed `*Config` (mechanical rename, preserves serialized JSON since these aren't Unity-serialized assets — verify `MoyvaJsonTypeRegistry` resolves by stable ID in-patch).

## 13. Canonical APIs (one per operation)

| Operation | Canonical entry |
|---|---|
| Place building | `IConstructionCommitService.TryCommit(ConstructionCommitRequest)` — absorbs all 5 applier/executor seams; authority/prepaid/setup become `request.Mode` flags |
| Demolish building | `IConstructionPlacedBuildingDestruction` (already canonical) |
| Move unit | `IUnitMovementService` (+ turn-authority decorator, already canonical) |
| Recruit unit | `IUnitRecruitmentService` (canonical) |
| Attack | `ICombatCommandService` (canonical) |
| Capture | `ISettlementCaptureService` (canonical) |
| Query fog | `IFogOwnerStateReader` (canonical; local-grid methods deprecated then removed) |
| Economy query | `IEconomyRuntimeApi` (canonical; `EconomyManager` internals hidden) |
| End turn | `ITurnService` (canonical) |
| Compose world | feature `Install(container, mode)` — the single seam |

## 14. Systems that should remain unchanged

- `Jsonization/` runtime (works; JSON policy is clean)
- `Signals/` SignalBus declarations (minus the dead bridge)
- `SaveSystem` module-registry pattern
- `Turns` (7 files, clean contracts)
- `Economy` service decomposition (focused services — keep; only the facade surface shrinks)
- `Grid`/`MapChunks`/`ObjectsMap` three-layer storage split (reasonable)
- `Pathfinding`, `Calendar`, `Combat` (small, focused)
- `GraphSystem` engine (in-house node graph — Generator's runtime; not a generic framework)
- `tools/ai` verification scripts

## 15. Systems to simplify

- `ConstructionService` → split into `ConstructionSessionService` (preview/commit/undo), `PlacementEvaluationService` (partially exists as `BuildingPlacementEvaluator`), `ConstructionPersistenceService`, `ConstructionAuthorityService`; `IConstructionService` compat facade deleted after callers migrate.
- `UnitService` dictionaries → single `UnitState` record.
- `FogOfWarService` → merge the local grid into owner-scoped state; collapse 9 partials into 3–4 cohesive files.
- `StartingPosition*` (15 files) → one pipeline + ≤5 collaborators.
- `GameplayHUD` → delete dead presenter; split `GameplayHudReadModel` (24 deps) into per-panel read models.
- `HomeMenu` → extract `Lobby/` into its own feature assembly; delete UnityHTML path; preview keeps `MenuGameplaySimulation` but via the shared install contract.
- `MultiplayerAuthorityService` → per-domain command handlers behind one dispatch table.
- Editor pass-scripts → move to `tools/` or delete.

## 16. Systems/code to remove (after in-patch verification)

- `SignalDomainEventBridge` + `GameplayDomainEvents` (~310 LOC + per-signal fire overhead)
- `GameplayTurnHudPresenter.*` + `GameplayRecruitmentPanelView` (~1,700 LOC)
- `HomeMenu/Runtime/UnityHTML/` + `BindSharedSceneUi` + `UseDynamicMoyvaUi` const (~500+ LOC)
- Fog `LegacyTwcVolume` path + `FogVisualUpdaterRouter` legacy branch + `FogOfWarSettings.LegacyOverlay` (~4K LOC — gated on confirming ScreenSpace is the shipping path)
- Editor one-offs: `P09APatchValidationBridge`, `Pass82CompileEpoch`, `GameplayUiPass73/74`, `RecruitmentHudP24*`, `RecruitmentPanel*Authoring` (verify no active pipeline dependency per file)
- Vendored `MessageObservable`/`Unsubscriber` duplicates in Multiplayer providers → one internal file or delete if unused
- Dead `using Kruty1918.Moyva.Bootstrap.Runtime` in `UnitsInstaller` + the corresponding asmdef GUID ref
- 12+ `InternalsVisibleTo` for nonexistent test assemblies; `InternalsVisibleTo("Assembly-CSharp")` on Shared

## 17. Test strategy

- **Phase 0 gate**: no refactor patch merges without characterization tests for its touched path.
- Existing: 59 training tests, Generator env-decoration tests, Shared controls tests, Startup tests.
- **Add first**: `Kruty1918.Moyva.Tests.{Turns,Construction,Units,Economy,FogOfWar}` EditMode assemblies (the `InternalsVisibleTo` seams already exist) — characterization tests for: placement commit legality + demolition, economy tick + settlement create/deactivate, unit move + stamina + combat, fog owner visibility + LOS, save→restore round-trip.
- **Architecture tests** (EditMode, reflection-based): asmdef reference rules (no Feature→Bootstrap), no type referencing another feature's `*.Runtime` internals outside an `I*` contract, public-surface budget per feature.
- Reuse the `InstallSimulationBindings` composition for fast headless tests — it already works.

## 18. Documentation strategy

- `CODEMAP.md` (≤16 KiB) becomes the **canonical-operation map**: "To do X → API Y → service Z". Replace path-lists with operation-lists.
- `AGENTS.md` stays ≤6 KiB; rules only.
- Module README only where non-obvious invariants exist (Construction commit modes, Fog owner-grid model, Economy settlement lifecycle).
- Delete/archive stale docs: `docs/standards/domain-events-layer.md` (superseded), pass-migration docs. Target ≤100 docs files.
- Add `docs/architecture/canonical-apis.md` — the §13 table, kept checkable.

## 19. Migration plan — ordered patches

### Phase 0 — Safety

**P0.1 Characterization tests** — new EditMode assemblies for Turns/Construction/Units/Economy/FogOfWar; cover the §13 canonical paths via the simulation binding composition. Risk: LOW. Size: M–L. **Blocks everything.**

**P0.2 Architecture-test harness** — reflection-based asmdef/dependency rules + public-surface snapshot. Catches regressions in every later patch. Size: S–M.

**P0.3 Baseline metrics script** — `tools/ai/` script producing the §22 numbers; run before/after each patch. Size: S.

### Phase 1 — Deletion (highest value/effort ratio)

**P1.1 Dead HUD presenter removal** — delete `GameplayTurnHudPresenter.*` (7 files), `GameplayRecruitmentPanelView`, and its Editor authoring references. Verify no scene refs via GUID search. Behavior: NONE. Risk: LOW (never bound).

**P1.2 UnityHTML HomeMenu path removal** — delete `Runtime/UnityHTML/`, `BindSharedSceneUi`, `UseDynamicMoyvaUi` const. Behavior: NONE (const-true). Risk: LOW.

**P1.3 DomainEvent bridge removal** — delete `SignalDomainEventBridge`, `GameplayDomainEvents`, installer binding. Behavior: NONE (zero subscribers). Risk: LOW. Resolves TD-001/TD-002 by deletion.

**P1.4 Legacy fog volume path removal** — after verifying ScreenSpace is the active mode (`FogVisualPresentationMode` default + scene config): delete `Visual/Volume/`, `Visual/Chunking/`, router legacy branch, `LegacyOverlay` settings partial. Risk: MEDIUM (rendering path — needs visual smoke test). Behavior: INTENTIONAL if any scene still uses legacy mode → then keep, mark deprecated.

**P1.5 Editor one-off tooling cleanup** — move/delete pass-named migration scripts (~6–8K LOC) after confirming each is post-release obsolete. Risk: LOW–MEDIUM (some may drive active content pipelines — per-file review).

**P1.6 Vendored-fragment consolidation** — `MessageObservable`/`Unsubscriber`/`ReferenceEqualityComparer` → single `Multiplayer/Runtime/Internal/` file or delete if replaceable by `Action`. Risk: LOW.

### Phase 2 — State ownership

**P2.1 Fog state unification** — make `_ownerStates` the single grid; local accessors delegate to the local-owner state; update placement fog rules to the owner-scoped query. Behavior: NONE. Risk: MEDIUM (visibility semantics). Tests: fog characterization from P0.1.

**P2.2 UnitState aggregate** — collapse `UnitService`'s 7 dictionaries into `Dictionary<string, UnitState>`; keep the public API identical. Behavior: NONE. Risk: LOW–MEDIUM. Size: M.

**P2.3 Building derived-state audit** — document which subscriber owns which derived projection; merge trivially-duplicate maps (e.g., combat-target registry vs footprint store) where safe. Risk: MEDIUM. Size: M.

### Phase 3 — Canonical APIs + god classes

**P3.1 Construction commit unification** — introduce `ConstructionCommitRequest` + `IConstructionCommitService`; reimplement the 5 seams as thin adapters over it; deprecate→delete adapters after caller migration (Multiplayer, Caravan, Bot, Training). Risk: HIGH (most-used path) — mitigated by P0.1 tests + adapter stage. Size: L.

**P3.2 ConstructionService split** — extract `ConstructionSessionService` (session/undo/preview), `ConstructionPersistenceService`, `ConstructionAuthorityService`; `ConstructionService` becomes a thin composition-facing facade (≤500 LOC) or disappears. Depends on P3.1. Risk: HIGH. Size: L.

**P3.3 MultiplayerAuthorityService split** — per-domain command handlers (construction/units/combat/caravan/capture) behind one dispatch table. Risk: MEDIUM. Size: M.

**P3.4 StartingPosition pipeline consolidation** — 15 files → `StartingPosition/` pipeline with explicit stages (select→spawn→reveal→camera→save). Risk: MEDIUM. Size: M.

**P3.5 Economy facade consolidation** — `IEconomyRuntimeApi` = single query surface; move `IEconomyInfoMediator` into the Economy feature (misplaced in Signals). Risk: LOW–MEDIUM. Size: S.

### Phase 4 — Boundaries + simplification

**P4.1 Composition-contract unification** — `Install(container, FeatureInstallMode)` per feature; `BootstrapInstaller`, `GameplayTrainingEpisode`, `MenuGameplaySimulation` all call it; delete `InstallSimulationBindings`/`InstallPreviewBindings` duplicates. Risk: HIGH (startup) — smoke-test direct Gameplay + menu→Gameplay + training episode. Size: M.

**P4.2 Units→Bootstrap edge removal** — drop the dead using + asmdef GUID ref. Risk: LOW. Size: XS.

**P4.3 GameplayHUD→Multiplayer decoupling** — HUD consumes `IMultiplayer*` query contracts via the API namespace only; move net-internal type usage behind contracts. Risk: MEDIUM. Size: S–M.

**P4.4 Service-locator reduction** — replace `TryResolve` ctor-wiring in `BotRuntimeInstaller`/`GameplayTrainingEpisode` with real bindings where the container exists; keep the factory pattern only where genuinely dynamic. Risk: LOW–MEDIUM. Size: M.

**P4.5 Interface diet** — collapse single-impl interfaces that aren't real boundaries (Zenject convention or hypothetical seams); keep interfaces at genuine boundaries (feature APIs, test seams, SPIs). Target: ~30–40% reduction. Per-feature, bundled by subsystem. Risk: LOW per file, HIGH aggregate churn. Size: M.

### Phase 5 — Naming, docs, hygiene

**P5.1 Naming cleanup** — `*SO` JSON DTOs → `*Config` (mechanical rename + JSON reserialize check); `TestUnitSpawner` → `Tests/` or `Development/` folder; pass-named Editor files renamed to their function or deleted per P1.5. Risk: LOW (verify `MoyvaJsonTypeRegistry` name-binding first). Size: S–M.

**P5.2 InternalsVisibleTo hygiene** — delete nonexistent-assembly declarations; remove `Assembly-CSharp` exposure on Shared. Risk: LOW. Size: XS.

**P5.3 Docs consolidation** — CODEMAP → operation-map rewrite; delete stale standards docs; module READMEs only for invariants. Size: S.

**P5.4 Architecture regression rules** — CI step running P0.2 tests + `tools/ai/check-context-hygiene.sh`. Size: XS.

## 20. Patch-by-patch table

| Patch | Scope | Risk | Expected benefit | Depends on | Est. size |
|---|---|---|---|---|---|
| P0.1 Characterization tests | Tests/{Turns,Construction,Units,Economy,FogOfWar} | LOW | enables all later patches | — | M–L |
| P0.2 Architecture tests | test harness + rules | LOW | regression guard | — | S–M |
| P0.3 Metrics script | tools/ai | LOW | measurable deltas | — | S |
| P1.1 Dead HUD presenter | GameplayHUD | LOW | −1.7K LOC, one canonical HUD path | P0.1 | S |
| P1.2 UnityHTML menu path | HomeMenu | LOW | −500+ LOC, one menu stack | P0.1 | S |
| P1.3 DomainEvent bridge | Signals | LOW | −310 LOC, kills stalled migration | P0.1 | S |
| P1.4 Legacy fog volume | FogOfWar | MED | −4K LOC | P0.1, visual verify | M |
| P1.5 Editor one-offs | Editor + Bootstrap.Editor | LOW–MED | −6–8K LOC | per-file owner check | M |
| P1.6 Vendored fragments | Multiplayer | LOW | dedupe | — | XS |
| P2.1 Fog state unification | FogOfWar | MED | single visibility truth | P0.1 | M |
| P2.2 UnitState aggregate | Units | LOW–MED | one unit record | P0.1 | M |
| P2.3 Building derived-state audit | Construction/Economy | MED | documented ownership | P0.1 | M |
| P3.1 Commit unification | Construction | HIGH | 5→1 canonical placement API | P0.1 | L |
| P3.2 ConstructionService split | Construction | HIGH | god class → 3–4 services | P3.1 | L |
| P3.3 MPAuthorityService split | Multiplayer | MED | dispatch table, ≤1K LOC | P0.1 | M |
| P3.4 StartingPosition pipeline | Bootstrap | MED | 15→5 files | P0.1 | M |
| P3.5 Economy facade | Economy + Signals | LOW–MED | one query surface | — | S |
| P4.1 Composition unification | all features | HIGH | one install contract | P0.1, P3.x | M |
| P4.2 Units→Bootstrap edge | Units asmdef | LOW | layering rule | — | XS |
| P4.3 HUD→MP decoupling | GameplayHUD | MED | UI edge hygiene | P0.1 | S–M |
| P4.4 Service-locator reduction | Bot/Training | LOW–MED | real DI where possible | P4.1 | M |
| P4.5 Interface diet | all | LOW | −30–40% interfaces | P0.2 | M |
| P5.1 Naming cleanup | all | LOW | honest names | — | S–M |
| P5.2 InternalsVisibleTo hygiene | all | LOW | sealed internals | — | XS |
| P5.3 Docs consolidation | docs + CODEMAP | LOW | trustworthy map | most patches | S |
| P5.4 Regression rules | CI/tools | LOW | keeps gains | P0.2 | XS |

## 21. Risks

- **P0.1 is the risk-control patch**: without characterization tests, the P3.x god-class splits are unsafe. Do not reorder.
- **Fog unification (P2.1)** touches visibility semantics — the local-vs-owner duality may be load-bearing for single-player visual state. Mitigate with characterization tests + an explicit `LocalOwner` alias rather than a behavioral merge.
- **Construction split (P3.1/3.2)** is the highest-blast-radius patch; the adapter-stage migration keeps every commit green.
- **Legacy-path deletions (P1.4/1.5)** each need one manual verification (visual smoke / pipeline-owner confirm) — cheap vs the LOC recovered.
- **Interface diet (P4.5)** — mechanical but wide diff; gate on compile+tests, not on reviewers reading every file.
- **Serialization**: `*SO` renames must verify `MoyvaJsonTypeRegistry` resolves by stable ID, not CLR name (AGENTS says polymorphic IDs are allow-listed — likely safe, verify in-patch).

## 22. Baseline metrics (measured at audit)

| Metric | Value |
|---|---|
| Author C# files | 1,660 |
| Author LOC | ~215,359 |
| Assemblies (asmdef) | 62 |
| Files >500 LOC | 76 |
| Files >300 LOC | 189 |
| Largest class | `ConstructionService` — 6,348 LOC / 23 partials |
| Interfaces | ~450 |
| Single-impl interfaces | ~409 (91%) |
| DI-bound single-impl | ~193 |
| Signal types | ~60 |
| `Resolve`/`TryResolve` call sites | 447 in 121 files |
| `GetComponent*` calls | 565 |
| `FindObjectOfType*` calls | 21 |
| Static mutable fields (pub/internal) | ~20 |
| MonoBehaviour files | 97 |
| Test files / test assemblies | 12 / 4 existing (16 declared) |
| JSON presets | 398 |
| Docs .md | 428 |
| Asmdef dependency edges | 104 (0 cycles) |

## 23. Expected final metrics

| Metric | Target | How |
|---|---|---|
| LOC | −8–12% (~190–198K) | dead-code patches alone ~8K; interface diet + consolidation ~5K more |
| Files | −150–250 | deletions + consolidations |
| God classes >3K LOC | 0 | P3.x splits |
| Interfaces | ~270–300 | P4.5 diet |
| Public surface per feature | documented, bounded | P0.2 snapshot + rules |
| Test assemblies | 9+ | P0.1 + per-patch additions |
| Parallel composition roots | 1 contract | P4.1 |
| Placement-commit APIs | 1 | P3.1 |
| Dead-event overhead | 0 | P1.3 |
| Docs files | ≤100 | P5.3 |

## 24. Definition of Done

Per patch: compiles, full EditMode suite green, architecture tests green, no new Console errors, direct-Gameplay + menu→Gameplay smoke where startup-touched, metrics delta recorded.

Global: every §13 canonical API is the single documented entry point; §14–16 lists fully resolved; an agent can answer "how do I do X" from CODEMAP alone for the top-20 operations.

## TOP 10 highest-value refactorings

1. **P0.1 characterization tests** — precondition for everything; the InternalsVisibleTo seams already exist.
2. **P3.1 + P3.2 Construction commit/service split** — kills the worst god class AND the 5-API confusion.
3. **P1.3 DomainEvent bridge deletion** — deletes a whole parallel event layer with zero subscribers.
4. **P2.1 fog state unification** — removes the two-truths visibility model that already caused a real bug.
5. **P1.1 + P1.2 dead presenter/menu-stack removal** — ~2.2K LOC of misleading parallel UI.
6. **P4.1 composition-contract unification** — one install seam; training/menu/scene stop drifting.
7. **P1.4 legacy fog-volume deletion** — ~4K LOC behind a runtime router.
8. **P2.2 UnitState aggregate** — kills the 7-dictionary drift pattern.
9. **P3.4 StartingPosition consolidation** — the most fragmented pipeline becomes one readable flow.
10. **P4.5 interface diet** — restores signal-to-noise on real boundaries (after P0.2 guards it).
