# Fog of War — Architecture

Fog of War is a gameplay-information system, not a visual filter. Three
separate state planes exist, each with one authority.

## State planes

| Plane | Storage | Authority | Consumers |
|---|---|---|---|
| Local perspective | `FogOfWarService._stateGrid` | `FogOfWarService` | Screen-space texture, volume fog, renderer culling, chunk coverage, local gameplay queries (`IFogStateReader`) |
| Per-owner grids | `FogOfWarService._ownerStates` | `FogOfWarService` | Multiplayer filtering, bots, save snapshots, intel legitimacy checks (`IFogOwnerStateReader`) |
| Remembered intel | `FogIntelStore` | `FogIntelStore` | Ghost markers, bot perception, late-join/reconnect replication |

`IFogStateReader` answers for the **local perspective owner only**
(`IFogLocalPerspective.LocalPerspectiveOwnerId`). Every globally registered
vision source carries an owner tag (`_sourceOwners`); a source adds tiles to
the local grid only when `IsLocalPerspectiveOwner(sourceOwner)` is true.
Unowned sources (reveals, previews, startup fallback) always contribute.
While the local owner is unresolved, every source contributes — this is a
single-player/dev fallback; `FogLocalPerspectiveInitializer` (bootstrap)
resolves the owner before gameplay starts.

Per-owner grids stay independent: `RegisterUnit(ownerId, ...)` /
`RegisterFixedVisionArea(ownerId, ...)` write only that owner's grid.

## Vision sources

- Units: `UnitCreatedSignal` registers owner + global catalog entries;
  `UnitMovedSignal` updates both; garrison unregisters both; destroy removes
  both. `UnitDestroyedSignal` carries no owner — owner-grid cleanup resolves
  the owner via `_ownerByVisionSourceId`.
- Buildings: `ConstructionBuildingFogEffects` is the **sole** authority for
  building vision (per-definition radius/shape). It registers owner-scoped
  vision always and local-grid vision only when the building belongs to the
  local perspective owner. The fog service does **not** subscribe to
  `BuildingPlacedSignal`.
- Ownership transfer: `BuildingOwnershipTransferredSignal` →
  `TransferFixedVisionAreaOwner` moves the owner-grid source and re-tags the
  global catalog entry so the local grid gains/loses it with ownership.

## Intel (last-known memory)

`FogIntelStore` keeps authoritative truth (`_unitTruth`, `_buildingTruth`)
from signals and per-owner intel (`_intel`). Rules:

- Record only when the entity is **visible to that owner** and not their own.
- Stale records persist when the entity leaves vision.
- `CellsBecameVisible` (batched per owner, via `IFogOwnerVisibilityFeed`)
  reconciles: still there → refresh; gone → forget; newly seen → record.
- Death/demolition observed → forget immediately; unobserved → keep memory.
- Own entities are never recorded (the player sees them live).
- Readers get clones; records are never gameplay entities.

`FogIntelGhostPresenter` draws pooled `SpriteRenderer` markers for records
whose cell is `Explored` but not `Visible`. Ghosts never touch unit services,
health, occupancy, selection, combat, or replication.

## Multiplayer

All world-event replication is fail-closed (`CanPeerObserveWorldEvent`):

- Same-owner peer → always notified (their own entities).
- Other peers → only when the event cell is visible in that peer's owner grid.
- Unresolvable scope/participants → send to requester only, never broadcast.
- Hidden destinations are never transmitted; a peer discovering a unit gets a
  `UnitSpawn` payload; a remembered-but-gone unit gets `UnitVanish` after the
  peer re-observes the last-known cell.
- `WorldStateReplicationService` schema v5 adds per-owner intel records and a
  packed explored bitmap so late join/reconnect restores legitimate memory
  without revealing current hidden state.

## Save/load

`FogOfWarSaveModule` writes: local explored grid, fixed vision areas,
per-owner explored snapshots, per-owner intel snapshots (format version -4;
readers accept -2/-3/legacy). Visibility itself is never saved — it
re-derives from live sources after load.

## Bots

Bots read only `IFogOwnerStateReader` for their own owner id plus
`IFogIntelReader` memories. Movement candidates are not gated on visibility —
unexplored reachable tiles are tagged `BotIntentType.Explore` so the policy
can scout.
