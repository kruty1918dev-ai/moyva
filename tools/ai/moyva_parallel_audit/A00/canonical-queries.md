# A00 — Canonical queries and delta/event trace

Verified against base `98d86e11f1fd905d7e90b6b7a4e8c30dc6b20e38`.

Scope: what audit/verification code is allowed to *read* and how it must observe
*change*. Binding rule: **a scan never mutates**. Any API that creates, moves,
previews, confirms or destroys state is forbidden inside scan/snapshot code —
most importantly `IConstructionSessionCommands.TryPreviewAt`
(`ConstructionSessionContracts.cs`, implemented by `ConstructionService`), which
writes preview session state (ghost placement, `BuildingPreviewChangedSignal`),
so a "scan" built on it fabricates evidence instead of reading reality.

## Read-only canonical query surface

Per-concept read authority (consume the resolved plain-C# service via Zenject;
see CODEMAP.md for file paths):

| Concept | Canonical read API |
|---|---|
| Grid / tiles | `IGridService` (tile id/height per position, dims) |
| Occupancy | `IObjectsMapService` (occupant per tile) |
| Path / reachability | `IPathfinder`; per-unit `IUnitMovementQuery.GetMovementTiles(unitId)` → `UnitMovementTileSnapshot` |
| Units | `IUnitService` (units, positions, owner) |
| Unit combat preview | `IUnitCombatService.TryPreviewAttack(attackerUnitId, defenderUnitId, out UnitCombatBreakdown)` — pure out-param read, no state write (allowed; it is the *combat preview contract*, distinct from `TryPreviewAt` which mutates preview session state) |
| Recruitment | `IUnitRecruitmentService` query members |
| Construction reads | `IConstructionSessionCommands` getters only: `State`, `GetSelectedBuildingId`, `GetPendingPlacements`, `TryGetPendingPlacementStatus`, `GetResourceProjection`, `GetBuildingResourceCosts` — never the `Try*` mutation calls below |
| Economy | `IEconomyRuntimeApi`: `GetOwnerCategoryTotals`, `GetOwnerResourceTotals`, `GetOwnerSettlementSnapshots`, `GetOwnerWarehouseSnapshots`, `GetOwnerProductionSnapshot` |
| Settlement capture | `ISettlementCaptureQuery.TryEvaluateCapture` — side-effect-free eligibility (`ISettlementCaptureService.CaptureWithUnit` is the mutation, forbidden in scan) |
| Fog of war | `IFogOfWarService` visibility queries |
| Selection / hover | `ITileInteractionService` state |
| Game state / pause | `IGameStateService.CurrentState` (`GameStateType.Idle/Playing/Paused/GameOver`); `IGamePauseModePolicy.IsMultiplayerSessionActive` explains why `timeScale` may stay 1 during a real pause |
| World config / seed | `IWorldCreationService` pending `WorldCreationConfig`; `GameLaunchContext.Seed`/`Size`/`Width`/`Height`/`MapType`; `GlobalSeed.Current` |
| Generation resolution | `MapSeedService.Resolve` precedence launch→recipe→`DefaultSeed(42)`; `MapSizeResolver` launch→`SharedSettings`→requested then `MapChunkSizePolicy.CropAxis`; recipe id from `IMapGenerationEnvironment.Recipe` |
| Multiplayer commands | `IGameCommandSyncService` / `MultiplayerAuthorityService` for the command ledger |

## Forbidden inside scans (state-writing APIs)

- `IConstructionSessionCommands.TryPreviewAt` / `Confirm` / `ConfirmPending` /
  `TryDemolishAt` / `Cancel` / `SelectBuilding` / `SetActiveOwner` /
  `UndoLast` / `RedoLast` / `ToggleDemolishMode` / `TryMovePendingPlacement` /
  `RemovePendingAt` — preview *session* mutation and commits.
- `IUnitMovementService.MoveUnitAsync`, `IUnitCombatService` attack commands,
  `ISettlementCaptureService.CaptureWithUnit`, `IUnitRecruitmentService` enqueue
  mutations — command path, callable only as the audited action itself, never
  as a probe.
- `SignalBus.Fire(...)` — never emit synthetic signals to make a trace look alive.
- Editor mutations (`moyva-ui-set-*`, `moyva-ui-create-*`, `moyva-ui-reparent`,
  `moyva-ui-delete`, `moyva-ui-save-scene` pipeline commands, `eval` writes) —
  outside gameplay evidence entirely.

## Delta / event trace (actual, not reconstructed)

Subscribe the harness to Zenject `SignalBus` (declared in
`Signals/Runtime/SignalBusInstaller.cs`) and record every fired signal as a
JSONL row per `event-trace.schema.json` (`kind=signal`). Canonical delta events,
all declared at base SHA:

- Lifecycle: `GameStartedSignal`, `GameEndedSignal`, `GamePausedSignal{IsPaused}`
- World: `WorldGeneratedDataSignal` (carries `StartupSequence` *(long)*,
  `SnapshotRevision`, `StartupSessionId`, `PublishedFrame`,
  `PublishedAtUtcTicks`, `Width`, `Height`, `GridTopology`, `ProjectionMode`,
  `RenderMode`, `NeighborhoodMode`, `CellSize` — reuse as snapshot identity),
  `WorldSpawnPositionsSignal`, `WorldBuiltSignal`, `OnMapObjectSpawnedSignal`
- Tiles/objects/fog: `GridTileChangedSignal`, `OnObjectsMapChangedSignal`,
  `FogStateChangedSignal`
- Units: `UnitCreatedSignal`, `UnitMovedSignal`, `UnitDestroyedSignal`,
  `UnitMoveRejectedSignal`, `MoveUnitRequestSignal`, `MoveGroupRequestSignal`,
  `UnitGroupChangedSignal`, `UnitGarrisonStateChangedSignal`,
  `InterruptMovementSignal`, `LocalUnitSelectionChangedSignal`
- Construction: `BuildingPlacedSignal`, `BuildingCancelledSignal`,
  `BuildingDemolishedSignal`, `BuildingPreviewChangedSignal`,
  `BuildingOperationalSignal`, `BuildingOwnershipTransferredSignal`,
  `ConstructionPlacementRejectedSignal`, `PlaceBuildingConfirmRequestSignal`
- Economy/turns/save/world-creation: `SettlementCapturedSignal`,
  `EconomyTickCompletedSignal`, `SettlementResourceChangedSignal`,
  `SaveRequestedSignal`/`LoadRequestedSignal`/`SaveCompletedSignal`,
  `WorldCreationConfirmedSignal`/`WorldCreationCancelledSignal` families
  (see `OnEconomySignals.cs`, `OnSaveSignals.cs`, `OnWorldCreationSignals.cs`).

Every row is stamped `{sequence, frame, gameTime, wallTimeUtc}` from the run
header's snapshot clock. `provenance` marks `live-game` vs `bridge-injected`
rows; only `live-game` rows count toward acceptance.

## Capture commands (actual, verified in `Assets/Moyva/Editor/UnityCliBridge/`)

- Live GameView bitmap: `unity command moyva-gameplay-ui-capture-window --path <png> --view game`
- Play-mode screenshot: `unity command moyva-gameplay-ui-capture-runtime --path <png>`
- Live UI/canvas state dump: `unity command moyva-gameplay-ui-capture-state`
- Editor discovery: `unity status --format json` (pid/port per project — never
  signal a foreign `Unity.exe`)
- Ad-hoc reads: `tools/unity-cli/moyva-unity eval '<csharp>'` /
  `eval-file <script>` (see `capture_runtime_snapshot.cs`)
