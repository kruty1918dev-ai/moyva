# Gameplay Production Report

Branch `feature/gameplay-production-polish` vs `game-process`.
Goal: make the full player loop (start -> economy -> recruit -> group -> move ->
fog -> combat -> capture -> victory/defeat -> exit/restart) work end-to-end
through authoritative services in Solo, Human-vs-Bot and Multiplayer.

## Changes

### Bootstrap funding (deadlock fix)

`BootstrapStarterPackGrantService`: skips invalid entries (null, blank id,
non-positive amount), treats empty/null lists as a valid no-op, emits
`GrantStarterPackResourcesSignal` only when entries exist, and rejects empty
owner IDs. `BootstrapGameInitializer` persists grant state only after a
successful grant, so a failed grant no longer marks the owner as funded.

### Combat + Fog of War

`UnitCombatService.CanAttack` now rejects targets not visible to the attacker
owner (`TargetNotVisible`) via `IFogOwnerStateReader`; the building-attack path
in `UnitCombatCommandService` enforces the same rule. `TileInteractionService`
attack clicks route through `ICombatCommandService` (solo/host) or
`ICombatRemoteCommandRequester` (client) instead of calling combat directly.

### Unit groups/stacks (new subsystem)

`IUnitGroupService` + `UnitGroupService`: create/disband/add/remove/move,
ownership + duplicate-membership validation, destroyed/garrisoned cleanup,
save/restore (`UnitsSaveModule` v6), and `TryMoveGroup` that decomposes into
canonical per-unit `IUnitMovementService` moves (formation cells, nearest
member takes target). `RestoreReplicatedState` keeps members whose unit
replication has not arrived yet.

### Movement rejection feedback

`UnitMovementService` fires `UnitMoveRejectedSignal` on every rejection;
`TileInteractionService` and group moves propagate reasons to HUD
notifications.

### Multiplayer convergence

`MultiplayerAuthorityService` gained group commands
(create/disband/add/remove/move) and recruitment commands
(enqueue/cancel/deploy) with sender-identity authorization and authoritative
snapshot broadcasts (`UnitGroupSync`, recruitment queue sync). Clients use
`IUnitGroupRemoteCommandRequester` / `IUnitRecruitmentRemoteCommandRequester`
and never mutate local authoritative state; rejections reach the UI via
`UnitGroupCommandRejectedSignal` / `UnitRecruitmentCommandRejectedSignal`.

### HUD

Group snapshot fields + merge/disband buttons, merge-mode state that resets on
selection/mode change, recruitment enqueue/cancel/deploy routing through the
requester boundary on clients, and notification surfacing for all new
rejection signals.

## Tests added (EditMode)

- `Tests.Units`: `UnitGroupServiceTests` (membership invariants, ownership,
  per-unit move decomposition, formation blocking, restore filtering,
  replicated restore), `UnitCombatServiceFogTests` (fog-gated attack).
- `Tests.Multiplayer`: `GameActionPayloadTests` (group/recruitment payload
  round-trips), `MultiplayerAuthorizationTests` (sender/owner matching).
- `Tests.Bootstrap`: `BootstrapStarterPackGrantServiceTests` (grant/no-op/empty
  owner/pool routing).

## Verification

Unity `6000.3.10f1`, EditMode suite, filter `Kruty1918.Moyva`:

- 164 / 171 passing.
- 7 failures are pre-existing on `game-process` and unrelated to this branch:
  - `AI.Training.Tests.FullGameIntegrationTests` x5 — `Unknown config reference
    'TileTypeConfig/water'` while deserializing `testgeneratorgraph` in the
    training test harness (files unmodified by this branch);
  - `Tests.Startup.HumanVsBotTests` x2 — stale test invokes
    `Activator.CreateInstance(BotController, 3 args)` but the ctor takes 5
    (`MissingMethodException`; both files unmodified by this branch).
- All 28 newly added tests pass; the remaining suite is unaffected.

## Known limits / follow-ups

- Pre-existing test failures above should be fixed on `game-process`
  (training test config registration, stale `HumanVsBotTests` reflection call).
- Group move is sequential per-unit; a member that cannot reach its formation
  cell is skipped rather than blocking the whole group.
- Merge requires clicking a second own unit while merge is armed; there is no
  drag-select yet.
