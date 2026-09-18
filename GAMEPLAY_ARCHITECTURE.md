# Gameplay Architecture

How the production gameplay loop is wired: who owns state, how input reaches it,
and how multiplayer, save/load and Fog of War stay authoritative.

## Layers

```
UI (HTML HUD / uGUI presenters)
  -> ITileInteractionService          (input intent, selection, click routing)
  -> UiActionRouter                   (button actions -> handlers)
      |
      +-- Solo/Host: canonical gameplay services (mutate state)
      +-- Client:    I*RemoteCommandRequester    (send payload, await host sync)
                          |
                    transport (GameCommandType + GameActionPayloads)
                          |
                    MultiplayerAuthorityService  (host: authorize -> execute -> broadcast)
```

Rules:

- Exactly one mutation authority per concept. UI, bots, training and network
  adapters call canonical services; they never mutate gameplay state directly.
- Clients never apply group/recruitment/combat mutations locally; they send a
  request and consume the host's authoritative sync payload.
- Host handlers validate the transport sender's identity and require the
  requested `ownerId` to match the sender before executing.

## Mutation authorities (Features/*)

| Concept | Authority | Notes |
|---|---|---|
| Units (spawn/move/die) | `IUnitService`, `IUnitMovementService` | occupancy, stamina, turn authority enforced |
| Combat | `IUnitCombatService` / `ICombatCommandService` | FoW-visible targets only |
| Groups | `IUnitGroupService` (`UnitGroupService`) | membership, formation move, save/restore |
| Recruitment | `IUnitRecruitmentService` | enqueue/cancel/deploy queue |
| Buildings/settlements | construction + capture services | capture uses `IFogOwnerStateReader` |
| Economy/resources | `EconomyManager` | owner pool + warehouse transfer |
| Turns | `ITurnService` | acts-per-turn gating |
| Fog of War | `IFogOwnerStateReader` (`FogOfWarService`) | per-owner visibility |

## End-to-end loop

1. `BootstrapGameInitializer` resolves owners, spawns the grid and castles,
   founds the starting settlement, then calls
   `BootstrapStarterPackGrantService.TryGrant(ownerId, settlementId, ...)`.
   Valid configured entries emit `GrantStarterPackResourcesSignal`;
   `EconomyManager` routes empty-`SettlementId` grants into the owner pool.
   Grant state is persisted only after a successful grant.
2. `TileInteractionService` routes clicks: select unit -> move
   (`IUnitMovementService`), enemy target -> attack
   (`ICombatCommandService` local / `ICombatRemoteCommandRequester` client),
   group member + merge armed -> merge, build/recruit panels -> their services.
3. Rejections surface as signals (`UnitMoveRejectedSignal`,
   `UnitGroupCommandRejectedSignal`, `UnitRecruitmentCommandRejectedSignal`)
   and reach HUD notifications with a readable reason.
4. Combat: `UnitCombatService.CanAttack` checks, in order: positions, self-hit,
   health registry, both healths, ownership, **target visibility for the
   attacker owner** (`TargetNotVisible`), config, range, damage, availability.
   Building attacks enforce the same visibility rule.
5. Groups: `UnitGroupService` validates ownership/membership, prevents
   duplicates, cleans up destroyed/garrisoned members, and decomposes
   `TryMoveGroup` into per-unit `IUnitMovementService` moves so stamina,
   occupancy and turn rules still apply per unit.
6. Victory/defeat and match end flow through the existing turn/settlement-loss
   pipeline; exit/menu/start-again reuse the same bootstrap.

## Multiplayer commands

`GameCommandType`: `UnitGroupCommand (17)`, `UnitGroupSync (18)`,
`UnitRecruitmentCommand (19)`, `UnitRecruitmentSync (20)` plus the existing
move/attack/turn commands. Payloads serialize via `GameActionPayloads`.

- Host: `MultiplayerAuthorityService` partials authorize sender -> execute
  canonical service -> broadcast snapshot (`UnitGroupSync`, queue-changed
  recruitment sync).
- Client: `IUnitGroupRemoteCommandRequester`,
  `IUnitRecruitmentRemoteCommandRequester`, `ICombatRemoteCommandRequester`
  send requests; rejection signals drive UI notifications.

## Persistence

`UnitsSaveModule` v6 stores group snapshots after unit spawn state; restore
rebuilds membership, drops missing units/empty groups, avoids duplicate
membership and advances the group ordinal. `RestoreReplicatedState` trusts the
host snapshot so client groups survive unit-replication lag.

## Configuration

All balance/settings stay in JSON under `Assets/Moyva/Presets/`
(`Load -> Validate -> Resolve -> Freeze -> Consume`). Starter resources live in
`BootstrapGameSettings.InitialResources`.
