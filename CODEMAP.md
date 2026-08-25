# Moyva Code Map

Purpose: route a human or AI to the minimum source set needed for a task.
Implementation source remains authoritative.

## Composition roots

| Scope | Root | Installs / delegates to |
|---|---|---|
| Project lifetime | `Assets/Moyva/Scripts/Bootstrap/Runtime/ProjectServicesInstaller.cs` | diagnostics, shared services, audio, SaveSystem, Multiplayer |
| Gameplay scene | `Assets/Moyva/Scripts/Bootstrap/Runtime/BootstrapInstaller.cs` | gameplay bootstrap and starting-position workflow; delegates presentation to `GameplayHudBindings` and feature state/save composition to feature installers |
| Gameplay signals | `Assets/Moyva/Scripts/Features/Signals/Runtime/SignalBusInstaller.cs` | Zenject SignalBus, gameplay signals, legacy-to-domain-event bridge, cached world-generation signals |
| Home menu | `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/HomeMenuInstaller.cs` | menu UI/services, gameplay session, menu-to-gameplay startup pipeline |
| World generation | `Assets/Moyva/Scripts/Features/Generator/Runtime/GeneratorInstaller.cs` | graph map-data pipeline, TileWorldCreator bridge, map visuals, generator save module |

Scene-authored feature installers still compose their own modules. Project and bootstrap installers may call idempotent feature binding methods, but must not duplicate feature graphs.

## Runtime ownership map

| Concern | Minimal start set | Runtime owner / mutation authority |
|---|---|---|
| Gameplay startup | `Bootstrap/Runtime/BootstrapInstaller.cs`; `Bootstrap/Runtime/StartingPositionInitializer.cs`; `Bootstrap/Runtime/StartingPositionWorkflowService.cs`; `Bootstrap/Runtime/StartingPositionWorkflowService.Client.cs` | `StartingPositionWorkflowService` sequences spawn assignment, load/new-world handling, fog reveal and camera framing; it delegates gameplay mutations |
| Launch topology | `Bootstrap/Runtime/GameplayLaunchTopology.cs`; `Features/SaveSystem/Runtime/SavePlayModeOptions.cs` | `GameplayLaunchTopology` is pure participant policy; `GameLaunchContext` carries selected launch settings into Gameplay |
| Turns / rounds | `Features/Turns/API/TurnContracts.cs`; `Features/Turns/Runtime/TurnBindings.cs`; `Features/Turns/Runtime/TurnService.cs` | `TurnService` owns active faction, phase, round, action count and transitions; `RoundResolutionService` orders round callbacks then advances Calendar |
| Bot turn bridge | `Bootstrap/Runtime/TurnBotDriver.cs` | starts one BotAI executor epoch and retries canonical `TryEndTurn`; owns no strategy or gameplay state |
| BotAI | `Features/BotAI/API/IBotTurnExecutor.cs`; `Features/BotAI/Runtime/BotRuntimeBindings.cs`; `Features/BotAI/Runtime/BotTurnExecutor.cs`; `Features/BotAI/Runtime/BotActionExecutor.cs` | Bot stores/planners own AI knowledge and decisions; `BotActionExecutor` delegates every mutation to canonical gameplay APIs |
| Units: state / identity | `Features/Units/API/IUnitService.cs`; `Features/Units/Runtime/UnitsInstaller.cs`; `Features/Units/Runtime/UnitService.cs`; `Features/Units/Runtime/UnitFactory.cs` | `UnitService` owns unit position/type/owner/stamina indexes; `UnitFactory` is the creation boundary |
| Units: movement | `Features/Units/API/IUnitMovementService.cs`; `Features/Units/API/Movement/IUnitMovementQuery.cs`; `Features/Units/Runtime/UnitMovementService.cs`; `Features/Units/Runtime/UnitMovementRangeQuery.cs` | `UnitMovementService` executes movement; `UnitMovementRangeQuery` owns reachable-tile queries; `UnitTurnAuthorityMovementService` decorates commands with turn/owner checks |
| Units: recruitment | `Features/Units/API/IUnitRecruitmentService.cs`; `Features/Units/Runtime/UnitRecruitmentService.cs`; `Features/Units/Runtime/UnitRecruitmentQueueStateMachine.cs`; `Features/Units/Runtime/UnitRecruitmentDeploymentService.cs` | `UnitRecruitmentService` is the only enqueue/progress/deploy application boundary; the queue state machine owns paid queue state |
| Gameplay HUD / recruitment presentation | `Features/GameplayHUD/Runtime/GameplayHudBindings.cs`; `GameplayTurnHudPresenter.cs`; `UnitRecruitmentDeploymentController.cs` | scene-authored turn and recruitment presentation; all mutations delegate to Turns and Units APIs |
| Unit combat | `Features/Combat/API/ICombatCommandService.cs`; `Features/Units/API/IUnitCombatService.cs`; `Features/Units/Runtime/UnitCombatCommandService.cs`; `Features/Units/Runtime/UnitCombatService.cs` | `UnitCombatCommandService` is the turn/owner-aware command boundary; `UnitCombatService` validates and applies unit attacks |
| Health | `Features/Combat/API/IHealthRegistry.cs`; `Features/Combat/Runtime/CombatInstaller.cs`; `Features/Combat/Runtime/HealthRegistry.cs`; `Features/Construction/Runtime/Core/Health/BuildingHealthService.cs` | `HealthRegistry` indexes entity health; building health and garrison state are owned by `BuildingHealthService` |
| Construction | `Features/Construction/API/Contracts/Core/ConstructionSessionContracts.cs`; `Features/Construction/API/Contracts/Core/ConstructionPersistenceContracts.cs`; `Features/Construction/Runtime/Core/Installers/ConstructionInstaller.cs`; `Features/Construction/Runtime/Core/Service/ConstructionService.cs` | one `ConstructionService` singleton implements the narrow session/query/persistence boundaries and remains the canonical mutation authority |
| Construction lifecycle | `Features/Construction/API/Contracts/Core/IConstructionLifecycle.cs`; `Features/Construction/Runtime/Core/Service/ConstructionLifecycleService.cs`; `Features/Construction/Runtime/Core/Service/ConstructionLifecycleStateMachine.cs` | owns build progress, operational transitions and their persistence payload |
| Economy | `Features/Economy/Runtime/EconomyInstaller.cs`; `Features/Economy/Runtime/EconomyManager.cs`; `Features/Economy/Runtime/EconomySettlementRegistryService.cs`; `Features/Economy/Runtime/EconomyOwnerResourcePoolService.cs` | `EconomyManager` coordinates construction/calendar signals; settlement states/registry and owner resource pool are authoritative stores |
| Economy queries | `Features/Economy/Runtime/IEconomyRuntimeApi.cs`; `Features/Economy/Runtime/EconomyRuntimeApi.cs`; `Features/Economy/API/IMapObjectEconomyService.cs` | read-only projections for UI/BotAI and map-object inspection; mutations stay behind Economy services |
| Factions | `Features/Faction/API/`; `Features/Faction/Runtime/FactionInstaller.cs`; `Features/Faction/Runtime/FactionOwnershipService.cs` | `FactionRegistry` owns definitions; `FactionOwnershipService` owns the unit-to-faction index derived from lifecycle signals |
| Fog / perception | `Features/FogOfWar/API/Contracts/Core/IFogOfWarService.cs`; `Features/FogOfWar/Runtime/Installers/FogOfWarInstaller.cs`; `Features/FogOfWar/Runtime/Core/Service/FogOfWarService.cs` | `FogOfWarService` owns explored/visible state and vision sources; `FogVisualUpdaterRouter` delegates presentation |
| Grid | `Features/Grid/API/IGridService.cs`; `Features/Grid/Runtime/GridInstaller.cs`; `Features/Grid/Runtime/ChunkedGridService.cs` | `ChunkedGridService` is the bound grid API; tile storage is `Features/MapChunks/Runtime/Grid/ChunkedTileStore.cs` |
| Object occupancy | `Features/ObjectsMap/API/IObjectsMapService.cs`; `Features/ObjectsMap/Runtime/ObjectsMapInstaller.cs`; `Features/ObjectsMap/Runtime/ChunkedObjectsMapService.cs` | bound occupancy API; mirrors unit/map-object lifecycle signals into `ChunkedObjectStore` |
| Map chunks | `Features/MapChunks/API/`; `Features/MapChunks/Runtime/Installers/MapChunkFeatureBindings.cs`; `Features/MapChunks/Runtime/Core/MapChunkLayoutService.cs` | layout, chunked tile/object stores and visual chunk registries; bindings are reused by Grid, ObjectsMap, Camera, Fog and Generator |
| Pathfinding | `Features/Pathfinding/API/IPathfinder.cs`; `Features/Pathfinding/Runtime/PathfinderInstaller.cs`; `Features/Pathfinding/Runtime/Pathfinder.cs` | path queries only; callers provide traversal/occupancy rules, so it is not a movement mutation path |
| Calendar | `Features/Calendar/API/ICalendarService.cs`; `Features/Calendar/Runtime/CalendarInstaller.cs`; `Features/Calendar/Runtime/GameCalendarService.cs` | `GameCalendarService` owns game date/time; round resolution advances it exactly once per completed round |
| Game mode / pause | `Features/GameMode/API/`; `Features/GameMode/Runtime/GameModeInstaller.cs`; `Features/GameMode/Runtime/GameModeService.cs`; `Features/GameMode/Runtime/GameStateService.cs` | `GameModeService` owns interaction mode; `GameStateService` owns playing/paused/game-over lifecycle; exit sequencing is `ExitMatchCoordinator` |
| Save / restore | `Features/SaveSystem/API/`; `Features/SaveSystem/Runtime/SaveSystemInstaller.cs`; `Features/SaveSystem/Runtime/SaveService.cs`; `Features/SaveSystem/Runtime/SaveModuleRegistry.cs` | `SaveService` sequences registered `ISaveModule` payloads; feature modules capture/restore their own state |
| Multiplayer session | `Features/Multiplayer/API/ISessionManager.cs`; `Features/Multiplayer/Runtime/MultiplayerInstaller.cs`; `Features/Multiplayer/Runtime/SessionManager.cs`; `SessionManager.Connection.cs` | `SessionManager` owns session lifecycle and delegates participant/reconnect state to focused partials; switchable providers own transport selection |
| Multiplayer commands | `Features/Multiplayer/API/IGameCommandSyncService.cs`; `Features/Multiplayer/Runtime/GameCommandSyncService.cs`; `Features/Multiplayer/Runtime/MultiplayerAuthorityService.cs`; `MultiplayerAuthorityService.Authorization.cs` | routes commands and host confirmations; Construction and Units handlers delegate mutations to canonical gameplay services |
| World creation settings | `Features/WorldCreation/API/IWorldCreationService.cs`; `Features/WorldCreation/Runtime/WorldCreationInstaller.cs`; `Features/WorldCreation/Runtime/WorldCreationService.cs` | owns editable menu configuration; menu startup freezes it into session/launch settings |
| Home menu / launch | `Features/HomeMenu/API/`; `Features/HomeMenu/Runtime/HomeMenuInstaller.cs`; `Features/HomeMenu/Runtime/HomeMenuGameStarter.cs`; `Features/HomeMenu/Runtime/Startup/GameplayStartupPipeline.cs` | menu services prepare `GameplaySession`; startup sets `GameLaunchContext`, preloads and activates Gameplay |
| Graph model / evaluation | `Features/GraphSystem/API/GraphAsset.cs`; `Features/GraphSystem/API/IGraphRunner.cs`; `Features/GraphSystem/Runtime/GraphRunner.cs`; `Features/GraphSystem/Runtime/GraphValidator.cs` | `GraphAsset` owns graph/node data; `GraphRunner` evaluates it. Generator constructs it through `GraphEvaluationPipeline` |
| World generation | `Features/Generator/API/IMapDataGenerator.cs`; `Features/Generator/Runtime/GeneratorInstaller.cs`; `Features/Generator/Runtime/MapVisualInstantiator.cs`; `Features/Generator/Runtime/MapVisual/MapVisualWorldBuildOrchestrator.cs` | orchestrator sequences generate/restore, TileWorldCreator build, grid write and world signals; `GraphTwcMapDataGenerator` is the normal provider |
| Signals / domain events | `Features/Signals/API/`; `Features/Signals/Runtime/SignalBusInstaller.cs`; `Features/Signals/Runtime/SignalDomainEventBridge.cs` | transport and notification only; signals must not become alternative state authority |
| Input routing | `Infrastructure/InputRouting/API/GameplayInputPolicy.cs`; `Infrastructure/InputRouting/Runtime/InputRoutingBindings.cs`; `Infrastructure/InputRouting/Runtime/GameplayInputPolicy.cs` | owns scoped input-block leases; feature input services still interpret allowed input |
| UI action routing | `Features/UIActions/API/`; `Features/UIActions/Runtime/UiActionsInstaller.cs`; `Features/UIActions/Runtime/UiActionRouter.cs` | context/escape/hotkey routing and action journal only; handlers delegate gameplay changes |
| Interactions / selection | `Features/Interactions/API/ITileInteractionService.cs`; `Features/Interactions/Runtime/InteractionsInstaller.cs`; `Features/Interactions/Runtime/WorldInfoSelectionCoordinator.cs` | translates pointer/tile selection into presentation requests; owns selection coordination, not map-object state |
| Info panel | `Features/InfoPanel/UI/WorldInfoPanelInstaller.cs` | presentation-only world/building/unit information UI |
| Notifications | `Features/Notifications/API/IGameplayNotificationService.cs`; `Features/Notifications/Runtime/NotificationsInstaller.cs`; `Features/Notifications/Runtime/GameplayNotificationService.cs` | owns notification queue/deduplication and delegates rendering to its presenter |
| Camera | `Features/Camera/API/`; `Features/Camera/Runtime/CameraInstaller.cs`; `Features/Camera/Runtime/CameraMovement.cs`; `Features/Camera/Runtime/CameraZoom.cs` | owns camera presentation state and input; reads Grid/MapChunks but owns no gameplay state |
| Animations | `Features/Animations/API/IMovementAnimationService.cs`; `Features/Animations/Runtime/AnimationsInstaller.cs`; `Features/Animations/Runtime/MovementAnimationService.cs` | movement presentation only; canonical unit movement awaits completion |
| Clouds | `Features/Clouds/API/ICloudsService.cs`; `Features/Clouds/Runtime/CloudsInstaller.cs`; `Features/Clouds/Runtime/CloudsService.cs` | owns transient cloud presentation instances only |
| Visuals / day-night | `Features/Visuals/Runtime/VisualInstaller.cs`; `Features/Visuals/Runtime/DayNightShaderController.cs` | presentation derived from Calendar; delegates default calendar composition to `CalendarInstaller.InstallDefaultIfMissing` |
| Shared runtime services | `Shared/SharedInstaller.cs`; `Shared/Audio/AudioContracts.cs`; `Shared/Audio/AudioService.cs`; `Shared/GraphicsSettingsService.cs`; `Shared/Performance/`; `Shared/UI/` | project-lifetime connectivity, graphics, audio, health, performance and UI policies; no gameplay mutation authority |
| Diagnostics | `Infrastructure/Diagnostics/API/`; `Infrastructure/Diagnostics/Runtime/DiagnosticsInstaller.cs` | observes and reports flows; diagnostics never decide gameplay outcomes |
| JSON runtime | `Jsonization/Runtime/MoyvaJsonRuntime.cs`; `Jsonization/Runtime/MoyvaJsonTypeRegistry.cs`; `Jsonization/Runtime/MoyvaJsonAssetCatalog.cs` | loads, validates and resolves JSON-backed configuration; JSON under `Assets/Moyva/Presets/` is editable source of truth |

## Authoritative runtime flows

### Project startup

`ProjectServicesInstaller`
→ `DiagnosticsInstaller`
→ `SharedInstaller` / `AudioInstaller`
→ `SaveSystemInstaller`
→ `MultiplayerInstaller`

### Home menu to Gameplay

`HomeMenu UI/services`
→ `GameplaySession`
→ `HomeMenuGameStarter`
→ `GameplayStartupPipeline` (`Preload → Bind → Warmup → SceneActivate`)
→ `GameLaunchContext`
→ Gameplay scene installers

### New or restored world

`GeneratorWorldStartupBuilder`
→ `MapVisualInstantiator`
→ `MapVisualWorldBuildOrchestrator`
→ pending save data or `GraphTwcMapDataGenerator`
→ `GraphEvaluationPipeline` / `GraphRunner`
→ optional TileWorldCreator bridge
→ `IGridService`
→ `WorldGeneratedDataSignal` + `WorldSpawnPositionsSignal`
→ `StartingPositionInitializer`
→ `StartingPositionWorkflowService`
→ load/new-world reveal, camera and spawn setup
→ `TurnService` becomes ready from spawn assignments

### Round and turn handoff

`TurnService.StartCurrentTurn`
→ ordered `ITurnParticipant.OnTurnStarted`
→ canonical player/BotAI actions
→ `ITurnBlocker` checks
→ ordered `ITurnParticipant.OnTurnEnding`
→ next faction, or `RoundResolutionService`
→ `ITurnParticipant.OnRoundCompleted`
→ `ICalendarService.AdvanceTurn`
→ calendar-driven Economy tick
→ next round

### Human gameplay mutation

`input/UI`
→ input policy and feature query/preflight
→ canonical command/application service
→ authoritative store/state update
→ gameplay signal/domain event
→ presentation and derived indexes

Canonical command boundaries are Construction (`IConstructionSessionCommands` / `IConstructionPlacementQuery`), movement (`IUnitMovementService` / `IUnitMovementQuery`), recruitment (`IUnitRecruitmentService`) and combat (`ICombatCommandService`).

### Bot turn

`TurnService`
→ `TurnBotDriver`
→ `BotTurnExecutor`
→ world snapshot + strategic/turn planners
→ ranked `BotActionCandidate`
→ `BotActionExecutor`
→ canonical Construction / Units / Combat APIs
→ normal blockers settle
→ `TurnBotDriver`
→ `ITurnService.TryEndTurn`

### Multiplayer gameplay command

`client request`
→ `MultiplayerAuthorityService`
→ `GameCommandSyncService`
→ host validation
→ canonical Construction / Units service
→ confirmed command/event
→ clients apply confirmed state through the same feature boundary

### Save / restore

`ISaveService`
→ snapshot of DI-provided and `SaveModuleRegistry` modules
→ deterministic module order
→ `SaveWriteService` or `SaveLoadService`
→ each feature's `ISaveModule`
→ explicit feature restore boundary without replaying costs or gameplay commands

## Cross-feature dependency rules

- Bootstrap sequences startup; it does not become a second gameplay domain.
- UI, BotAI, Multiplayer, save adapters and editor tools delegate mutations to canonical feature services.
- Signals publish completed transitions or requests; subscribers do not silently establish a second source of truth.
- Grid owns tile identity. ObjectsMap owns occupancy. Units and Construction own their domain state and keep derived occupancy indexes synchronized through canonical signals/services.
- Calendar advances from round resolution. Economy reacts to Calendar and Construction; it does not advance Turns.
- GraphSystem evaluates graphs. Generator owns world-generation orchestration and writes the resolved world to Grid.
- Shared and Infrastructure contain cross-cutting policies only; feature-specific behavior stays in its feature.

## Authority and consumers

| Authoritative state | Mutation entry points | Main readers / derived consumers |
|---|---|---|
| world and tile IDs | Generator world build or generated-world restore → `IGridService` | Construction, Units traversal, Pathfinding, Fog, Camera, BotAI |
| occupancy | `IObjectsMapService`, synchronized from unit/map-object lifecycle | Construction placement, Units movement, BotAI snapshots, world-info UI |
| turn state | `ITurnService`; `ITurnStateRestorer` only for persistence | Units and Construction participants, BotAI driver, HUD, SaveSystem |
| unit state | `IUnitFactory`, `IUnitMovementService`, `IUnitRecruitmentService`, `ICombatCommandService`, unit restore boundary | ObjectsMap, Faction ownership, Fog, BotAI, Multiplayer adapters, UI |
| construction state | `IConstructionSessionCommands`, confirmed multiplayer apply contracts, construction restore contracts | Economy, Fog, ObjectsMap, Units traversal/garrison, BotAI, UI |
| economy state | Economy construction/calendar integration, starter-pack grant, economy restore module | Construction affordability, UI summaries, BotAI queries |
| fog state | fog map/reveal/vision-source contracts and fog restore module | Construction rules, BotAI perception, renderer culling and fog visuals |
| calendar state | `RoundResolutionService`; calendar restore/sync boundaries | Economy tick and day/night visuals |
| faction definitions / ownership | `FactionInstaller`; unit lifecycle-derived ownership registration | Turns, BotAI, unit queries and presentation |
| session / participants | menu session preparation and `ISessionManager` | launch topology, local-owner resolution, multiplayer authority, pause policy |
| game mode / lifecycle | `IGameModeService`, `IGameStateService`, `IExitMatchCoordinator` | input routing, pause/menu UI and scene exit |

## Invariants

- One mutation authority per gameplay concept.
- BotAI plans/queries; canonical gameplay services mutate.
- No parallel Bot-only construction/combat/recruitment/movement implementation.
- One BotAI composition root.
- Turn-driven BotAI; no second wall-clock bot scheduler.
- Tests are not production Runtime code.
- Editor/analyzer code is never gameplay authority.
- Direct Gameplay and menu Gameplay converge on the same participant/session semantics.
- Compatibility paths remain only while a supported path can reach them.
- Serialized Unity types require GUID/reference proof before deletion.
- Generated inventories/audits are not tracked architecture documentation.

## Minimal reading pattern

For most changes, read:

`CODEMAP -> feature API -> installer/bindings -> target implementation -> focused tests`

Only expand to callers/consumers when the change crosses a feature boundary.

## Context-budget guardrails

- Search production first: exclude `Development`, `Editor` and `Tests` until a concrete runtime path requires them.
- Treat a file above 500 lines as a review signal, not an automatic split target. Split mixed ownership; keep cohesive mesh, validation and rendering algorithms together.
- Do not open large Fog, Generator mesh or Graph evaluation implementations to answer composition/authority questions. Start from their API and installer row above.
- Construction uses responsibility-based partials; use the focused map below instead of loading every partial.
- Runtime code with no C# caller still requires GUID, DI, reflection, JSON ID and save-compatibility checks before removal.
- Disabled or generated sources must be reproducible and actively compiled; do not track inert compatibility corpora.

On-demand size scan (do not commit its output):

```sh
rg --files Assets/Moyva/Scripts -g '*.cs' \
  -g '!**/Development/**' -g '!**/Editor/**' -g '!**/Tests/**' \
  | xargs wc -l | sort -nr
```

On-demand structural clone scan:

```sh
npx --yes jscpd@4.0.5 Assets/Moyva/Scripts \
  --pattern '**/*.cs' \
  --ignore '**/Development/**,**/Editor/**,**/Tests/**' \
  --min-lines 20 --min-tokens 100 --mode mild
```

## Graph and Generator reading map

| Task | Primary files |
|---|---|
| graph serialized model / read queries | `GraphSystem/API/GraphAsset.cs` |
| graph state normalization / layer indexes | `GraphSystem/API/GraphAsset.State.cs` |
| graph editor mutations / repair | `GraphSystem/API/GraphAsset.Authoring.cs` |
| sync/async graph execution | `GraphSystem/Runtime/GraphRunner.Execution.cs` |
| execution planning / participation diagnostics | `GraphSystem/Runtime/GraphRunner.cs` |
| input/output port contracts | `GraphSystem/Runtime/GraphRunner.Contracts.cs` |
| generic structural validation | `GraphSystem/Runtime/GraphValidator.cs` |
| Generator node/TWC semantics | `Generator/Runtime/GeneratorGraphSemanticValidator.cs` |
| Generator scene composition | `Generator/Runtime/GeneratorInstaller.cs`; `GeneratorBindingGroups.cs` |
| startup world-build decision | `Generator/Runtime/GeneratorWorldStartupBuilder.cs` |
| Add node settings / ports | `Generator/Runtime/Nodes/AddNode.cs` |
| Add map arithmetic | `Generator/Runtime/Nodes/AddNode.Evaluator.cs` |

Large chunk mesh builders are leaf algorithms. Do not open them for graph,
composition, startup, or validation work.

## Construction reading map

Do not open every `ConstructionService` partial for a focused task.

| Task | Primary files |
|---|---|
| external session commands / state | `API/Contracts/Core/ConstructionSessionContracts.cs` |
| placed-state and persistence contracts | `API/Contracts/Core/ConstructionPersistenceContracts.cs` |
| service lifecycle / dependencies | `ConstructionService.cs` |
| canonical session state | `ConstructionService.SessionStore.cs` |
| placement query facade / global selection | `ConstructionService.PlacementQuery.cs`; `ConstructionService.PlacementSelection.cs` |
| spatial evaluation / cache adapters | `ConstructionService.PlacementEvaluation.cs`; `ConstructionService.PlacementQueryCache.cs` |
| selection / owner / bootstrap castle | `ConstructionService.PlacementState.cs` |
| preview commands / pending mutations | `ConstructionService.PreviewSession.cs`; `ConstructionService.PendingMutations.cs` |
| fog / terrain / tile placement rules | `ConstructionPlacementEnvironmentRules.cs` |
| committed-building fog reveal | `ConstructionBuildingFogEffects.cs` |
| placed footprint occupancy / origin mapping | `ConstructionFootprintStore.cs` |
| replacement / gate-wall policy | `ConstructionReplacementPolicy.cs` |
| settlement / influence-zone policy | `ConstructionInfluencePolicy.cs` |
| costs / resource projection | `ConstructionService.Economy.cs` |
| prerequisites / limits | `ConstructionService.Prerequisites.cs`; `ConstructionService.BuildingLimits.cs` |
| turn / owner authority | `ConstructionTurnAuthority.cs`; `ConstructionService.Authority.cs` |
| confirm / demolition / undo-redo | `ConstructionService.CommitUndo.cs`; `ConstructionService.DemolitionSession.cs`; `ConstructionService.UndoSession.cs` |
| save restore / authoritative placement | `ConstructionService.Persistence.cs`; `ConstructionService.AuthoritativePlacement.cs` |
| replica apply / relocation / destruction | `ConstructionService.ReplicaPlacement.cs`; `ConstructionService.UniqueRelocation.cs`; `ConstructionService.Destruction.cs` |
| placement diagnostics | `ConstructionService.Diagnostics.cs` |

Placement validation authority is `ConstructionService.EvaluatePlacement(...)` plus the rule-group partials under `Runtime/Placement/Evaluation`; do not introduce a parallel validator.

`ConstructionService.PlacementRules.cs` no longer exists. Follow the focused ownership rows above instead of searching for a monolithic rules partial.

## FogOfWar reading map

Keep core visibility authority and presentation work in separate task packets.

| Task | Primary files |
|---|---|
| fog state / reveal / vision sources | `API/Contracts/Core/IFogOfWarService.cs`; `Runtime/Core/Service/FogOfWarService.cs` |
| DI and presentation selection | `Runtime/Installers/FogOfWarInstaller.cs`; `Runtime/Visual/FogVisualUpdaterRouter.cs` |
| screen-space lifecycle / texture state | `FogScreenSpaceTextureUpdater.cs`; `FogScreenSpaceTextureUpdater.Lifecycle.cs`; `FogScreenSpaceTextureUpdater.StateBuffer.cs` |
| shader publication / world transform | `FogScreenSpaceTextureUpdater.ShaderPublisher.cs`; `FogScreenSpaceTextureUpdater.Transform.cs` |
| curtain mesh / surface calibration | `FogBoundaryCurtainRenderer.Geometry.cs`; `FogBoundaryCurtainRenderer.SurfaceCalibration.cs`; `FogBoundaryCurtainRenderer.SurfaceGrid.cs` |
| curtain material / top cap | `FogBoundaryCurtainRenderer.Presentation.cs`; `FogBoundaryCurtainRenderer.TopCap.cs` |
| volume host / scene context | `Volume/Controller/FogOfWarVolumeController.cs`; `Volume/Context/FogVolumeSceneContextBuilder.cs` |
| volume request lifecycle / scheduling | `FogVolumeVisualUpdateEngine.Lifecycle.cs`; `FogVolumeVisualUpdateEngine.Scheduling.cs` |
| TWC runtime layers / build | `FogVolumeVisualUpdateEngine.RuntimeLayers.cs`; `FogVolumeVisualUpdateEngine.TwcBuild.cs`; `FogVolumeVisualUpdateEngine.Height.cs` |
| optional presentation diagnostics | `ScreenSpace/Diagnostics/`; `Volume/Diagnostics/` |

The geometry and TWC build files are leaf algorithms. Do not load diagnostics for normal fog-state or composition changes.

## Units reading map

Do not open every Units runtime service for a focused task.

| Task | Primary files |
|---|---|
| unit state / position / ownership | `UnitService.cs` |
| unit creation | `UnitFactory.cs` |
| movement command / animation execution | `UnitMovementService.cs` |
| turn / owner movement authority | `UnitTurnAuthorityMovementService.cs` |
| reachable movement tiles / range cache | `UnitMovementRangeQuery.cs` |
| traversal cost / terrain / construction passage | `UnitTraversalPolicy.cs` |
| recruitment enqueue / economy / turn progression / signals | `UnitRecruitmentService.cs` |
| recruitment queue state / persistence identity | `UnitRecruitmentQueueStateMachine.cs` |
| unit and recruitment save payload | `UnitsSaveModule.cs` |
| recruitment building / module / recipe context | `UnitRecruitmentBuildingContextResolver.cs` |
| ready-unit deployment query / commit | `UnitRecruitmentDeploymentService.cs` |
| deployment tile validity | `UnitPlacementValidator.cs` |
| combat query / damage application | `UnitCombatService.cs` |
| combat command / turn authority | `UnitCombatCommandService.cs` |
| DI / canonical composition | `UnitsInstaller.cs` |

Recruitment is owned by `Features/Units`; do not create a parallel `Features/Recruitment` mutation path.
`IUnitMovementQuery` is implemented by `UnitMovementRangeQuery`; `IUnitMovementService` owns execution.

## Bootstrap and GameplayHUD reading map

| Task | Primary files |
|---|---|
| scene composition | `Bootstrap/Runtime/BootstrapInstaller.cs` |
| launch-context fallback | `Bootstrap/Runtime/DirectGameplayLaunchModeInitializer.cs`; `GameplayLaunchTopology.cs` |
| starting-position sequencing | `StartingPositionInitializer.cs`; `StartingPositionWorkflowService.cs`; `StartingPositionWorkflowService.Client.cs` |
| initial new-world save | `Bootstrap/Runtime/InitialWorldSaveService.cs` |
| HUD composition / lifecycle | `Features/GameplayHUD/Runtime/GameplayHudBindings.cs`; `GameplayTurnHudPresenter.cs` |
| recruitment recipes / enqueue | `GameplayTurnHudPresenter.Recruitment.cs`; `GameplayTurnHudPresenter.RecruitmentAuthority.cs` |
| recruitment queue / selection | `GameplayTurnHudPresenter.RecruitmentQueue.cs`; `GameplayTurnHudPresenter.RecruitmentSelection.cs` |
| ready-unit deployment session | `UnitRecruitmentDeploymentController.cs`; `UnitRecruitmentDeploymentController.Session.cs` |
| deployment preview / controls | `UnitRecruitmentDeploymentController.Preview.cs`; `UnitRecruitmentDeploymentController.Controls.cs`; `UnitRecruitmentDeploymentController.UiActions.cs` |

`Features/GameplayHUD` uses an asmref to the existing Bootstrap assembly so moved scene components retain their serialized assembly identity.

## HomeMenu reading map

| Task | Primary files |
|---|---|
| composition | `Runtime/HomeMenuInstaller.cs` |
| shell creation / navigation | `Runtime/Shell/HomeMenuInitializer.cs`; `HomeMenuRuntimeUiFactory.cs`; `HomeMenuNavigation.cs` |
| lobby room list | `Runtime/Lobby/JoinRoomPanelService.cs`; `JoinRoomPanelService.RoomList.cs` |
| lobby join transaction | `JoinRoomPanelService.JoinPipeline.cs`; `JoinRoomTransportAdapter.cs`; `MultiplayerRoomLifecycle.cs` |
| join target / password / feedback | `JoinRoomPanelService.TargetResolution.cs`; `JoinRoomPanelService.Feedback.cs`; `PasswordPanelService.cs` |
| host lobby workflow | `CreateRoomPanelService.cs`; `LobbyPanelService.cs`; `GameStartListenerService.cs` |
| world setup | `Runtime/Services/WorldCreationPanelService.cs`; `GameSettingsPanelService.cs` |
| menu world preview lifecycle / generation | `UI/Preview/HomeMenuBackgroundPreviewController.cs`; `HomeMenuBackgroundPreviewController.Lifecycle.cs`; `HomeMenuBackgroundPreviewController.Generation.cs` |
| live preview mesh / camera | `HomeMenuBackgroundPreviewController.LiveMeshBuild.cs`; `HomeMenuBackgroundPreviewController.LiveMeshAssets.cs`; `HomeMenuBackgroundPreviewController.LiveMeshPresentation.cs` |
| preview texture / clouds | `HomeMenuBackgroundPreviewController.TexturePresentation.cs`; `HomeMenuBackgroundPreviewController.Clouds.cs` |
| gameplay scene transition | `Runtime/HomeMenuGameStarter.cs`; `Runtime/Startup/GameplayStartupPipeline.cs` |

Lobby, preview and startup are separate task packets. Do not load preview mesh/cloud files for room or scene-transition work.

## Multiplayer reading map

| Task | Primary files |
|---|---|
| public session / lobby contracts | `API/ISessionManager.cs`; `API/Lobby/ILobbyService.cs`; `API/Lobby/LobbyDtos.cs` |
| composition / provider selection | `Runtime/MultiplayerInstaller.cs`; `SwitchableLobbyService.cs`; `SwitchableNetworkProvider.cs` |
| host / join connection flow | `SessionManager.cs`; `SessionManager.Connection.cs` |
| participant state / lobby updates | `SessionManager.Participants.cs` |
| reconnect / migration / cleanup | `SessionManager.Reconnect.cs`; `HostMigrationService.cs` |
| offline fallback / option normalization | `SessionManager.Fallback.cs` |
| lobby implementations | `Runtime/Lobby/LanLobbyService.cs`; `UgsLobbyService.cs`; `OfflineLobbyService.cs` |
| command router / endpoint lifecycle | `MultiplayerAuthorityService.cs`; `MultiplayerAuthorityService.Authorization.cs` |
| Construction network commands | `MultiplayerAuthorityService.ConstructionCommands.cs` |
| Units network commands | `MultiplayerAuthorityService.UnitCommands.cs` |
| LAN / Relay frame and pump lifecycle | `MultiplayerFrameCodec.cs`; `MultiplayerTransportPump.cs`; the selected provider only |

Concrete lobby implementations stay out of `API`. Do not load both LAN and Relay providers unless changing their shared frame or pump contract.
