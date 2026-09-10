# Moyva Code Map

Paths: Assets/Moyva/Scripts + Base + inline path. Rules: AGENTS.md.

## Project / Shared / Infrastructure

Base: `.`

| Task | Entry points |
|---|---|
| Project lifetime | `Bootstrap/Runtime/ProjectServicesInstaller.cs` → Shared, Audio, SaveSystem, Multiplayer |
| Gameplay composition | `Bootstrap/Runtime/BootstrapInstaller.cs` → feature bindings; startup below |
| Input routing | `Infrastructure/InputRouting/API/GameplayInputPolicy.cs`; `Infrastructure/InputRouting/Runtime/InputRoutingBindings.cs`; `Infrastructure/InputRouting/Runtime/GameplayInputPolicy.cs` |
| Camera shortcuts | `Shared/PlayerControlBinding.cs`; `Shared/PlayerControlSettingsService.cs` |
| Audio / graphics / performance / UI | `Shared/SharedInstaller.cs`; `Shared/Audio/AudioContracts.cs`; `Shared/Audio/AudioService.cs`; `Shared/GraphicsSettingsService.cs`; `Shared/Performance/`; `Shared/UI/` |
| Diagnostic logging | `Shared/Diagnostics/RuntimeDiagnostics.cs` (no DI installer) |
| JSON / asset catalog | `Jsonization/Runtime/MoyvaJsonRuntime.cs`; `Jsonization/Runtime/MoyvaJsonTypeRegistry.cs`; `Jsonization/Runtime/MoyvaJsonAssetCatalog.cs` |

## Feature entry points

Base: `Features/`

| Task | Entry points |
|---|---|
| Turns / rounds | `Turns/API/TurnContracts.cs`; `Turns/Runtime/TurnBindings.cs`; `Turns/Runtime/TurnService.cs`; `Turns/Runtime/RoundResolutionService.cs` |
| Combat / health | `Combat/API/ICombatCommandService.cs`; `Combat/API/IHealthRegistry.cs`; `Combat/Runtime/CombatInstaller.cs`; `Combat/Runtime/HealthRegistry.cs` |
| Economy state | `Economy/Runtime/EconomyInstaller.cs`; `Economy/Runtime/EconomyManager.cs`; `Economy/Runtime/EconomySettlementRegistryService.cs`; `Economy/Runtime/EconomyOwnerResourcePoolService.cs` |
| Economy queries | `Economy/Runtime/IEconomyRuntimeApi.cs`; `Economy/Runtime/EconomyRuntimeApi.cs`; `Economy/API/IMapObjectEconomyService.cs` |
| Factions | `Faction/API/`; `Faction/Runtime/FactionInstaller.cs`; `Faction/Runtime/FactionOwnershipService.cs` |
| Grid / tiles | `Grid/API/IGridService.cs`; `Grid/Runtime/GridInstaller.cs`; `Grid/Runtime/ChunkedGridService.cs`; `MapChunks/Runtime/Grid/ChunkedTileStore.cs` |
| Occupancy | `ObjectsMap/API/IObjectsMapService.cs`; `ObjectsMap/Runtime/ObjectsMapInstaller.cs`; `ObjectsMap/Runtime/ChunkedObjectsMapService.cs` |
| Map chunks | `MapChunks/API/`; `MapChunks/Runtime/Installers/MapChunkFeatureBindings.cs`; `MapChunks/Runtime/Core/MapChunkLayoutService.cs` |
| Path queries | `Pathfinding/API/IPathfinder.cs`; `Pathfinding/Runtime/PathfinderInstaller.cs`; `Pathfinding/Runtime/Pathfinder.cs` |
| Calendar | `Calendar/API/ICalendarService.cs`; `Calendar/Runtime/CalendarInstaller.cs`; `Calendar/Runtime/GameCalendarService.cs` |
| Game mode / pause / exit | `GameMode/API/`; `GameMode/Runtime/GameModeInstaller.cs`; `GameMode/Runtime/GameModeService.cs`; `GameMode/Runtime/GameStateService.cs`; `GameMode/Runtime/ExitMatchCoordinator.cs` |
| Save / restore / launch options | `SaveSystem/API/`; `SaveSystem/Runtime/SaveSystemInstaller.cs`; `SaveSystem/Runtime/SaveService.cs`; `SaveSystem/Runtime/SaveModuleRegistry.cs`; `SaveSystem/Runtime/SavePlayModeOptions.cs` |
| World settings | `WorldCreation/API/IWorldCreationService.cs`; `WorldCreation/Runtime/WorldCreationInstaller.cs`; `WorldCreation/Runtime/WorldCreationService.cs` |
| Signals / events | `Signals/API/`; `Signals/Runtime/SignalBusInstaller.cs`; `Signals/Runtime/SignalDomainEventBridge.cs` |
| UI actions | `UIActions/API/`; `UIActions/Runtime/UiActionsInstaller.cs`; `UIActions/Runtime/UiActionRouter.cs` |
| Tile selection | `Interactions/API/ITileInteractionService.cs`; `Interactions/Runtime/InteractionsInstaller.cs`; `Interactions/Runtime/WorldInfoSelectionCoordinator.cs` |
| Info panel | `InfoPanel/UI/WorldInfoPanelInstaller.cs` |
| Notifications | `Notifications/API/IGameplayNotificationService.cs`; `Notifications/Runtime/NotificationsInstaller.cs`; `Notifications/Runtime/GameplayNotificationService.cs` |
| Camera | `Camera/API/`; `Camera/Runtime/CameraInstaller.cs`; `Camera/Runtime/CameraMovement.cs`; `Camera/Runtime/CameraZoom.cs` |
| Movement animation | `Animations/API/IMovementAnimationService.cs`; `Animations/Runtime/AnimationsInstaller.cs`; `Animations/Runtime/MovementAnimationService.cs` |
| Clouds | `Clouds/API/ICloudsService.cs`; `Clouds/Runtime/CloudsInstaller.cs`; `Clouds/Runtime/CloudsService.cs` |
| Day/night visuals | `Visuals/Runtime/VisualInstaller.cs`; `Visuals/Runtime/DayNightShaderController.cs` |

## Bootstrap / launch

Base: `Bootstrap/Runtime/`

| Task | Files |
|---|---|
| Scene composition | `BootstrapInstaller.cs` |
| Direct Gameplay / participants | `DirectGameplayLaunchModeInitializer.cs`; `GameplayLaunchTopology.cs` |
| Spawn / reveal / camera | `StartingPositionInitializer.cs`; `StartingPositionWorkflowService.cs`; `StartingPositionWorkflowService.Client.cs` |
| Initial save | `InitialWorldSaveService.cs` |

Menu: GameplaySession → GameplayStartupPipeline → GameLaunchContext → scene.
World/spawn signals → starting-position workflow → Turns ready.

## Units / movement / recruitment / combat

Base: `Features/Units/`

| Task | Files |
|---|---|
| Contracts | `API/IUnitService.cs`; `API/IUnitMovementService.cs`; `API/Movement/IUnitMovementQuery.cs`; `API/IUnitRecruitmentService.cs`; `API/IUnitCombatService.cs` |
| Composition | `Runtime/UnitsInstaller.cs` |
| State / identity / creation | `Runtime/UnitService.cs`; `Runtime/UnitFactory.cs` |
| Movement command / turn authority | `Runtime/UnitMovementService.cs`; `Runtime/UnitTurnAuthorityMovementService.cs` |
| Reachability / traversal | `Runtime/UnitMovementRangeQuery.cs`; `Runtime/UnitTraversalPolicy.cs` |
| Recruitment / costs / turns | `Runtime/UnitRecruitmentService.cs` |
| Queue / persistence | `Runtime/UnitRecruitmentQueueStateMachine.cs`; `Runtime/UnitsSaveModule.cs` |
| Recruitment context | `Runtime/UnitRecruitmentBuildingContextResolver.cs` |
| Deployment / tile validity | `Runtime/UnitRecruitmentDeploymentService.cs`; `Runtime/UnitPlacementValidator.cs` |
| Combat command / damage | `Runtime/UnitCombatCommandService.cs`; `Runtime/UnitCombatService.cs` |

## Construction contracts / composition

Base: `Features/Construction/`

| Task | Files |
|---|---|
| Session commands / state | `API/Contracts/Core/ConstructionSessionContracts.cs` |
| Placed state | `API/Contracts/Core/ConstructionPersistenceContracts.cs` |
| Lifecycle contract / bindings | `API/Contracts/Core/IConstructionLifecycle.cs`; `Runtime/Core/Installers/ConstructionInstaller.cs` |
| Building health / garrison | `Runtime/Core/Health/BuildingHealthService.cs` |
| Placement rule groups | `Runtime/Placement/Evaluation/` |

## Construction service partials

Base: `Features/Construction/Runtime/Core/Service/`

EvaluatePlacement owns placement decisions; SessionStore owns state.

| Task | Files |
|---|---|
| Lifecycle / state | `ConstructionService.cs`; `ConstructionService.SessionStore.cs` |
| Build progress / operations | `ConstructionLifecycleService.cs`; `ConstructionLifecycleStateMachine.cs` |
| Query facade / global selection | `ConstructionService.PlacementQuery.cs`; `ConstructionService.PlacementSelection.cs` |
| Spatial evaluation / cache | `ConstructionService.PlacementEvaluation.cs`; `ConstructionService.PlacementQueryCache.cs` |
| Selection / owner / castle | `ConstructionService.PlacementState.cs` |
| Preview / pending mutations | `ConstructionService.PreviewSession.cs`; `ConstructionService.PendingMutations.cs` |
| Fog / terrain / tiles | `ConstructionPlacementEnvironmentRules.cs`; `ConstructionBuildingFogEffects.cs` |
| Footprints | `ConstructionFootprintStore.cs` |
| Replacement / gate-wall / influence | `ConstructionReplacementPolicy.cs`; `ConstructionInfluencePolicy.cs` |
| Costs / prerequisites / limits | `ConstructionService.Economy.cs`; `ConstructionService.Prerequisites.cs`; `ConstructionService.BuildingLimits.cs` |
| Turn / owner authority | `ConstructionTurnAuthority.cs`; `ConstructionService.Authority.cs` |
| Confirm / demolition / undo-redo | `ConstructionService.CommitUndo.cs`; `ConstructionService.DemolitionSession.cs`; `ConstructionService.UndoSession.cs` |
| Restore / authoritative placement | `ConstructionService.Persistence.cs`; `ConstructionService.AuthoritativePlacement.cs` |
| Replica / relocation / destruction | `ConstructionService.ReplicaPlacement.cs`; `ConstructionService.UniqueRelocation.cs`; `ConstructionService.Destruction.cs` |

## HomeMenu / current UI / startup

Base: `Features/HomeMenu/Runtime/`

UseDynamicMoyvaUi=true selects MoyvaUI; otherwise UnityHTML.

| Task | Files |
|---|---|
| Bindings | `HomeMenuInstaller.cs` |
| Active shell / UI state | `MoyvaUI/HomeMenuMoyvaUiPresenter.cs`; `MoyvaUI/HomeMenuMoyvaUiViewController.cs`; `MoyvaUI/HomeMenuMoyvaUiState.cs` |
| Active markup / event bridge | `MoyvaUI/HomeMenuMoyvaUiMarkup.cs`; `MoyvaUI/HomeMenuMoyvaUiBridge.cs` |
| Keyboard editor | `MoyvaUI/HomeMenuControlsMarkup.cs`; `MoyvaUI/HomeMenuControlsEditor.cs` |
| HTML shell | `UnityHTML/HomeMenuHtmlShellPresenter.cs`; `UnityHTML/HomeMenuHtmlMenuBridge.cs` |
| Scene UI / navigation | `Shell/HomeMenuInitializer.cs`; `Shell/HomeMenuRuntimeUiFactory.cs`; `Shell/HomeMenuNavigation.cs` |
| Room list | `Lobby/JoinRoomPanelService.cs`; `Lobby/JoinRoomPanelService.RoomList.cs` |
| Join / transport / cleanup | `Lobby/JoinRoomPanelService.JoinPipeline.cs`; `Lobby/JoinRoomTransportAdapter.cs`; `Lobby/MultiplayerRoomLifecycle.cs` |
| Target / password / feedback | `Lobby/JoinRoomPanelService.TargetResolution.cs`; `Lobby/JoinRoomPanelService.Feedback.cs`; `Lobby/PasswordPanelService.cs` |
| Host / lobby workflow | `Lobby/CreateRoomPanelService.cs`; `Lobby/LobbyPanelService.cs`; `Lobby/GameStartListenerService.cs` |
| World setup | `Services/WorldCreationPanelService.cs`; `Services/GameSettingsPanelService.cs` |
| Gameplay transition | `HomeMenuGameStarter.cs`; `Startup/GameplayStartupPipeline.cs` |

## HomeMenu world preview

Base: `Features/HomeMenu/UI/Preview/`

Skip this section for lobby/join work.

| Task | Files |
|---|---|
| Lifecycle / generation | `HomeMenuBackgroundPreviewController.cs`; `HomeMenuBackgroundPreviewController.Lifecycle.cs`; `HomeMenuBackgroundPreviewController.Generation.cs` |
| Live mesh / camera | `HomeMenuBackgroundPreviewController.LiveMeshBuild.cs`; `HomeMenuBackgroundPreviewController.LiveMeshAssets.cs`; `HomeMenuBackgroundPreviewController.LiveMeshPresentation.cs` |
| Texture / clouds | `HomeMenuBackgroundPreviewController.TexturePresentation.cs`; `HomeMenuBackgroundPreviewController.Clouds.cs` |

## GameplayHUD / current HTML presentation

Base: `Features/GameplayHUD/Runtime/`

GameplayHudBindings selects GameplayHtmlPresenter/State.

| Task | Files |
|---|---|
| Bindings / presenter / state | `GameplayHudBindings.cs`; `GameplayHtmlPresenter.cs`; `GameplayHudReadModel.cs`; `GameplayHtmlBridge.cs`; `GameplayHtmlState.cs` |
| HTML markup | `GameplayHtmlMarkup.Core.cs`; `GameplayHtmlMarkup.Regions.cs`; `GameplayHtmlMarkup.Cargo.cs` |
| Cargo presentation | `GameplayCargoPanel.cs` |
| Deployment session | `UnitRecruitmentDeploymentController.cs`; `UnitRecruitmentDeploymentController.Session.cs` |
| Preview / controls / UI actions | `UnitRecruitmentDeploymentController.Preview.cs`; `UnitRecruitmentDeploymentController.Controls.cs`; `UnitRecruitmentDeploymentController.UiActions.cs` |
| Alternate presenter lifecycle | `GameplayTurnHudPresenter.cs` |
| Alternate recruitment / authority | `GameplayTurnHudPresenter.Recruitment.cs`; `GameplayTurnHudPresenter.RecruitmentAuthority.cs` |
| Alternate queue / selection | `GameplayTurnHudPresenter.RecruitmentQueue.cs`; `GameplayTurnHudPresenter.RecruitmentSelection.cs` |

## Multiplayer session / commands / transport

Base: `Features/Multiplayer/`

JoinRoomTransportAdapter → ISessionManager → selected LAN/Relay provider.
Host authorization → canonical Units/Construction commands.

| Task | Files |
|---|---|
| Contracts | `API/ISessionManager.cs`; `API/Lobby/ILobbyService.cs`; `API/Lobby/LobbyDtos.cs`; `API/IGameCommandSyncService.cs` |
| Bindings / provider | `Runtime/MultiplayerInstaller.cs`; `Runtime/MultiplayerModeSelector.cs`; `Runtime/SwitchableLobbyService.cs`; `Runtime/SwitchableNetworkProvider.cs` |
| Host / join connection | `Runtime/SessionManager.cs`; `Runtime/SessionManager.Connection.cs` |
| Participants / reconnect / migration | `Runtime/SessionManager.Participants.cs`; `Runtime/SessionManager.Reconnect.cs`; `Runtime/HostMigrationService.cs` |
| Offline / normalization | `Runtime/SessionManager.Fallback.cs` |
| Lobby implementations | `Runtime/Lobby/LanLobbyService.cs`; `Runtime/Lobby/UgsLobbyService.cs`; `Runtime/Lobby/OfflineLobbyService.cs` |
| LAN discovery / connect / sockets | `Runtime/Lobby/LanLobbyService.Discovery.cs`; `Runtime/Lobby/LanLobbyService.Connection.cs`; `Runtime/Lobby/LanLobbyService.Networking.cs` |
| Sync / authorization | `Runtime/GameCommandSyncService.cs`; `Runtime/MultiplayerAuthorityService.cs`; `Runtime/MultiplayerAuthorityService.Authorization.cs` |
| Construction / Units commands | `Runtime/MultiplayerAuthorityService.ConstructionCommands.cs`; `Runtime/MultiplayerAuthorityService.UnitCommands.cs` |
| Frame / pump lifecycle | `Runtime/MultiplayerFrameCodec.cs`; `Runtime/MultiplayerTransportPump.cs` |
| Host readiness / client loading barrier | `API/IMultiplayerStartupBarrier.cs`; `Runtime/MultiplayerStartupBarrier.cs`; `Runtime/StartingPositionSyncService.cs` |

## FogOfWar state / composition

Base: `Features/FogOfWar/`

| Task | Files |
|---|---|
| State / reveal / vision sources | `API/Contracts/Core/IFogOfWarService.cs`; `Runtime/Core/Service/FogOfWarService.cs` |
| Bindings / visual selection | `Runtime/Installers/FogOfWarInstaller.cs`; `Runtime/Visual/FogVisualUpdaterRouter.cs` |

## FogOfWar presentation

Base: `Features/FogOfWar/Runtime/Visual/`

Skip visual leaf algorithms for fog-state/composition tasks.

| Task | Files |
|---|---|
| Screen-space lifecycle / texture state | `ScreenSpace/FogScreenSpaceTextureUpdater.cs`; `ScreenSpace/FogScreenSpaceTextureUpdater.Lifecycle.cs`; `ScreenSpace/FogScreenSpaceTextureUpdater.StateBuffer.cs` |
| Shader / world transform | `ScreenSpace/FogScreenSpaceTextureUpdater.ShaderPublisher.cs`; `ScreenSpace/FogScreenSpaceTextureUpdater.Transform.cs` |
| Curtain geometry / calibration | `ScreenSpace/FogBoundaryCurtainRenderer.Geometry.cs`; `ScreenSpace/FogBoundaryCurtainRenderer.SurfaceCalibration.cs`; `ScreenSpace/FogBoundaryCurtainRenderer.SurfaceGrid.cs` |
| Curtain material / top cap | `ScreenSpace/FogBoundaryCurtainRenderer.Presentation.cs`; `ScreenSpace/FogBoundaryCurtainRenderer.TopCap.cs` |
| Volume host / scene context | `Volume/Controller/FogOfWarVolumeController.cs`; `Volume/Context/FogVolumeSceneContextBuilder.cs` |
| Volume lifecycle / scheduling | `Volume/Build/FogVolumeVisualUpdateEngine.Lifecycle.cs`; `Volume/Build/FogVolumeVisualUpdateEngine.Scheduling.cs` |
| TWC runtime layers / build | `Volume/Build/FogVolumeVisualUpdateEngine.RuntimeLayers.cs`; `Volume/Build/FogVolumeVisualUpdateEngine.TwcBuild.cs`; `Volume/Build/FogVolumeVisualUpdateEngine.Height.cs` |

## GraphSystem model / evaluation

Base: `Features/GraphSystem/`

| Task | Files |
|---|---|
| Model / normalization / authoring | `API/GraphAsset.cs`; `API/GraphAsset.State.cs`; `API/GraphAsset.Authoring.cs` |
| Runner contract / execution plan | `API/IGraphRunner.cs`; `Runtime/GraphRunner.cs` |
| Sync/async execution / ports | `Runtime/GraphRunner.Execution.cs`; `Runtime/GraphRunner.Contracts.cs` |
| Structural validation | `Runtime/GraphValidator.cs` |

## Generator / world build

Base: `Features/Generator/`

World build: generate/restore → Grid → signals.

| Task | Files |
|---|---|
| Contract / composition | `API/IMapDataGenerator.cs`; `Runtime/GeneratorInstaller.cs`; `Runtime/GeneratorBindingGroups.cs` |
| Startup decision / world-build sequence | `Runtime/GeneratorWorldStartupBuilder.cs`; `Runtime/MapVisualInstantiator.cs`; `Runtime/MapVisual/MapVisualWorldBuildOrchestrator.cs` |
| Graph provider / pipeline | `Runtime/GraphTwcMapDataGenerator.cs`; `Runtime/GraphEvaluationPipeline.cs` |
| Generator graph semantics | `Runtime/GeneratorGraphSemanticValidator.cs` |
| Chunk-first terrain mesh | `Runtime/ChunkFirst/Mesh/`; `Runtime/ChunkFirst/TwcAdapter/TwcTileMeshSourceProvider.cs` |
| Add node settings / map arithmetic | `Runtime/Nodes/AddNode.cs`; `Runtime/Nodes/AddNode.Evaluator.cs` |
