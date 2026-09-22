# Moyva telemetry coverage

Adapter: `Assets/Moyva/Scripts/Features/Telemetry/` (`Kruty1918.Moyva.Telemetry`).
Bound by `BootstrapInstaller` → `MoyvaTelemetryInstaller.Install(Container)`.

## Boundaries instrumented

| Boundary | Source surface | Event type(s) |
|---|---|---|
| Session lifecycle | `GameStarted/Ended/PausedSignal` | `moyva.match.started/.ended/.paused` |
| Game mode | `GameModeChangedSignal` | `moyva.match.modeChanged` |
| World generation | `WorldGeneratedDataSignal` | `moyva.world.generated` |
| Turns | `ITurnParticipant` (order 10_000) | `moyva.turn.lifecycle` |
| Construction | `BuildingPlaced/Cancelled/Demolished`, `ConstructionPlacementRejected`, `BuildingOwnershipTransferred` | `moyva.construction.*` |
| Units | `UnitCreated/Moved/Destroyed`, `UnitMoveRejected` | `moyva.units.*` |
| Recruitment | `UnitRecruitmentQueueChanged/Ready/Deployed/CommandRejected` | `moyva.recruitment.*` |
| Economy | `EconomyTickCompleted`, `SettlementCreated/Deactivated/Captured`, `ResourceDeficit` | `moyva.economy.*` |
| Fog of war | `FogStateChangedSignal` (aggregate count only) | `moyva.fog.changed` |
| Save/load | `SaveCompleted`, `LoadRequested` | `moyva.save.*` |
| UI | world/building/unit/map-object info panels, selection | `moyva.ui.panel` |
| Bot decisions | `BotTelemetryHub.TraceRecorded` (adapted, not duplicated) | `moyva.ai.bot.decision` |
| Training episodes | `BotRunMetrics.EpisodeRecorded` | `moyva.ai.bot.episode` |
| Multiplayer | `INetworkProvider.PeerConnected/Disconnected` (id hashed) | `moyva.net.peer` |
| App lifecycle / errors | `TelemetryHostBehaviour` (pause/quit/exception) | `app.lifecycle`, `app.error` |

## Deliberately not tracked

- `TileClickedSignal`, hover/preview/drag signals — high-frequency, low value;
  covered by outcome signals (placed/moved/rejected).
- Raw peer ids, map payloads, player names — privacy edge hashing / omission.
- `BuildingPreview*`, `ShowWallHandles` — pure visual noise.

## Single-authority guarantees

- Bot telemetry: adapter subscribes to the existing hub; no second trace buffer.
- Recruitment: reads `IUnitRecruitmentService` signals only; no parallel path.
- Construction/units: reads post-commit signals; telemetry never mutates.
