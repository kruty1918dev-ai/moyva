# Fog of War — Production Plan

Goal: Fog of War becomes a complete gameplay-information system, not a visual
filter. This document is the implementation plan for branch
`feature/fog-of-war-production` (base `game-process` @ `387a07ef`).

## Audit summary (confirmed in code)

| # | Area | Finding |
|---|------|---------|
| A1 | `FogOfWarService._stateGrid` | Global grid is a **union** of every registered vision source (`RegisterVisionArea` runs for every `UnitCreatedSignal` regardless of owner). All visual consumers (screen-space texture, volume, renderer culling, chunk coverage) read this union — on host and client every player sees the union of all players' knowledge. |
| A2 | `IFogOfWarService` vs `IFogOwnerStateReader` | Two parallel APIs exist. Owner grids (`_ownerStates`) are correct and complete, but nothing scopes the global grid to the local player. |
| A3 | `MultiplayerAuthorityService.UnitCommands` | `SendUnitMoveOrRevealToVisiblePeers`: a peer that previously knew a unit receives `UnitMove` with the new **hidden** position (`known && canSeePrevious && !canSeeNew` falls through to send). Leak. |
| A4 | Broadcast fallbacks | `SendUnitCommandToPeers`, `SendConfirmedCommandToVisiblePeers`, `SendConfirmedCommandToOwnerPeers`, `SendCombatConfirmationToObservers` broadcast to `"*"` when participant data is missing/empty or nobody matched (`!sent && Count == 1`). Fail-open. |
| A5 | `CanPeerObserveWorldEvent` | `_ownerFog == null → true` — fail-open when fog binding missing. |
| A6 | `WorldStateReplicationService` | Late-join snapshot includes only own + currently-visible units/buildings. Remembered entities are dropped (reconnecting/late peers lose legitimate intel). Empty `targetOwnerId` fails open (all units). No fog explored/intel section at all. |
| A7 | Re-reveal reconciliation | Host never corrects stale client state: a building demolished while hidden stays a real entity on the client forever; no `UnitVanish`/explored-correction message exists. `GameCommandType` has no fog/intel/vanish commands. |
| A8 | Bots | `MovementBotCapability` allows moves only to `IsVisible` tiles — bots cannot scout unexplored territory. Perception/capabilities otherwise gate on owner fog correctly (`MoyvaBotPerceptionSource`, combat/capture/construction/recruitment). |
| A9 | Memory/intel | No last-known entity store exists. `FogOfWarSaveModule` persists explored flags + fixed areas only. Remembered unit/building state is not representable. |
| A10 | Renderer culling | `FogRendererCullingEngine` hides renderers unless a covered cell is `Visible`. With a correct local-perspective grid this is right for live views; remembered entities need separate proxy views. |
| A11 | Host rendering | With union grid, host sees enemy units everywhere. Fixed automatically once `_stateGrid` is the local perspective (A1). |
| A12 | Buildings | `ConstructionBuildingFogEffects` registers both global + owner vision (good), but global registration is unconditional → enemy buildings feed the union. `OnBuildingPlaced` in the service is already unsubscribed (deliberate). |

## Architecture decisions

### D1 — `_stateGrid` becomes the *local perspective* grid

`FogOfWarService` keeps exactly one "global" grid, redefined as the **local
player's perspective**. This is the minimal-diff change: every visual consumer
(`IFogStateReader`, `IFogOfWarService`, dirty-tile feed, chunk coverage,
culling) automatically becomes correct.

- Every vision source records its owner in `_sourceOwners`
  (`sourceId → ownerId | null`). `null` = unowned/local-only sources (startup
  reveals, preview, fallback) and always contributes to the local grid.
- Tiles are applied to `_stateGrid` only when the source owner is the local
  owner or `null` (`ContributesToLocalPerspective`).
- `_unitPositions` keeps *all* globally-registered sources (needed by
  silhouette-target enumeration and recalculation), but `_unitVisibleTiles`
  only holds tiles actually applied to `_stateGrid`.
- `LocalPerspectiveOwnerId` is pushed via `SetLocalPerspectiveOwnerId` from a
  Bootstrap-side initializer that resolves `ITurnService.LocalOwnerId` /
  `ISessionManager.LocalPlayerId` / `GameLaunchContext.LocalPlayerId`
  (`BootstrapOwnerIdResolver` already does this chain). When the owner is set
  or changes, `RecalculateAllVisibility` rebuilds the local grid from filtered
  sources. When unset, the service falls back to "all registered sources
  contribute" (preserves offline/no-owner behavior).
- Owner grids (`_ownerStates`) are unchanged and stay authoritative for
  per-owner queries, MP filtering, bots, save snapshots.
- `IFogOfWarService` gains `LocalPerspectiveOwnerId` +
  `IsLocalPerspectiveOwner(ownerId)` so cross-feature callers
  (`ConstructionBuildingFogEffects`, starting-position reveal) can gate global
  registration on "this source belongs to the local player".
- Signal handlers gate global registration on the signal's owner:
  `OnUnitCreated`, `OnUnitMoved` (no auto-create for unknown ids),
  `OnUnitGarrisonStateChanged`, `OnBuildingOwnershipTransferred` (moves the
  local contribution too), `OnBuildingDemolished` (unchanged — idempotent).
- `ConstructionBuildingFogEffects.ApplyVision` calls the global APIs only when
  `_fogOfWarService.IsLocalPerspectiveOwner(ownerId)`; owner-scoped calls are
  unconditional.

### D2 — `FogIntelStore` (remembered entities), signal-driven

New plain-C# service inside `Features/FogOfWar/Runtime/Intel/` (no new asm
references — it works off `SignalBus` + owner fog state only, avoiding a
Fog→Units/Construction/Combat dependency cycle).

- Per owner: `Dictionary<string, FogIntelUnitRecord>` and
  `Dictionary<string, FogIntelBuildingRecord>`.
- Unit record: `unitId, typeId, ownerId, lastKnownPosition, lastKnownHp,
  turnLastSeen`. Building record: `buildingKey (position), buildingId,
  ownerId, rotation, turnLastSeen`.
- The store tracks authoritative entity positions internally from
  `UnitCreated`/`UnitMoved`/`UnitDestroyed`/`BuildingPlaced`/
  `BuildingDemolished`/`BuildingOwnershipTransferred` signals — needed for
  reconciliation without referencing gameplay services.
- Update rules:
  - Entity position visible to owner → upsert live record (fresh).
  - Entity leaves owner's vision → record stays at last observed state.
  - Owner gains visibility of a record's cell and the entity is no longer
    there (authoritative position differs) → **record removed**
    (re-scout reconciliation).
  - Entity visible again → record refreshed to live values.
  - Hidden move/damage/death/demolition never touch the record.
- Change feed: `OwnerVisibilityGained(ownerId, cells)` notification emitted by
  `FogOfWarService` when owner-grid cells transition *into* `Visible`; the
  store uses it for reconciliation, and `MultiplayerAuthorityService` uses it
  for client corrections (D4).
- Contracts: `IFogIntelReader` (per-owner queries: remembered units/buildings),
  `IFogIntelSnapshotStore` (save/load + replication), `IFogIntelReplicationSink`
  (client-side ingest).
- Bound in `FogOfWarInstaller` (`AsSingle`, interfaces + self).

### D3 — Ghost/proxy rendering (`FogMemoryVisualService`)

- Pooled quad proxies under a dedicated root excluded from renderer culling.
- A proxy is shown when: intel record exists for local owner **and** the
  record's last-known cell is `Explored` (not `Visible`) in the local grid.
  Visible cells show the live entity; unexplored cells show nothing.
- Visual: unlit quad + procedurally generated soft marker texture; color from
  `FogOfWarSettings` (unit vs building tint). Driven by intel-store change
  events + fog version — no per-frame scans, no steady-state allocation.
- Buildings that still exist but are not currently visible are *also* proxied
  (frozen last-known representation), so the rule stays uniform: live views
  render only in `Visible` cells.

### D4 — Multiplayer hardening

- `SendUnitMoveOrRevealToVisiblePeers`: when `known && !canSeeNew` → send
  nothing (client keeps the last-known entity position; its own fog + intel
  store handle display). When `canSeeNew` → always send a `UnitSpawn` payload
  (idempotent on client: creates or re-syncs position — self-healing if the
  entity was missing). `!canSeeNew` + `!known` → skip.
- `SendUnitCommandToPeers`, `SendConfirmedCommandToVisiblePeers`,
  `SendConfirmedCommandToOwnerPeers`,
  `SendCombatConfirmationToObservers`: remove broadcast fallbacks; empty
  participant data → log + skip (host is the only sender of these paths).
- `CanPeerObserveWorldEvent`: fail-closed — `_ownerFog == null` → `false`
  (service `Initialize` already throws when fog is missing).
- `WorldStateReplicationService`:
  - `CanIncludeUnitForOwner` fail-closed when `targetOwnerId` empty.
  - Buildings: include own + visible + **remembered** (intel) buildings.
  - Units: include own + visible units at live state, plus a dedicated intel
    section (schema version bump) carrying the peer's remembered unit/building
    records — last-known positions only, never current hidden state.
  - Economy pools: fail-closed for empty owner.
- Re-reveal reconciliation: `MultiplayerAuthorityService` subscribes to
  `OwnerVisibilityGained`; for each remote peer's newly-visible cells it
  pushes corrections — spawn payloads for unknown observed units, demolition
  confirmations for remembered-but-gone buildings (tracked per peer via
  `_knownUnitsByPeer` + new `_knownBuildingsByPeer`).
- Host migration / reconnect: fog explored + intel snapshots join the existing
  save/replication carriers so a promoted host keeps every player's knowledge.

### D5 — Bots

- `MovementBotCapability`: allow any reachable tile (game rules don't forbid
  moving into unexplored cells — same as humans). Encode fog state into
  candidate features (visible/explored/unexplored) so policy can prefer or
  require scouting; `Explore` intent for unexplored targets. `Validate`
  accepts explored/unexplored destinations.
- `MoyvaBotPerceptionSource`: add remembered-enemy counts via
  `IFogIntelReader` (optional dependency — bots get last-known positions like
  humans, nothing more).
- Bot fog = bot owner id; independent from local perspective by D1.

### D6 — Save/load

- `FogOfWarSaveModule` (version bump): per-owner explored snapshots (existing)
  + per-owner intel records (new) + fixed vision areas (existing).
- Restore order: explored snapshot → vision sources re-register via normal
  gameplay signals → intel records marked stale → re-reveal reconciles.

### D7 — Performance

- Owner-grid updates already incremental per source; no full-grid scans added.
- `OwnerVisibilityGained` is batched per public mutation (dedup `HashSet`).
- Intel store is dictionary-based; per-owner reconciliation touches only
  records at newly-visible cells (cell → record index).
- Proxy pool: event-driven create/remove; fixed capacity; no per-frame allocs.
- `RecalculateAllVisibility` keeps O(sources × tiles) — only on init/resize/
  perspective-owner change.
- Large maps: dirty-cell batching unchanged; culling stays amortized
  (max renderers/frame).

## Test plan (`Kruty1918.Moyva.Tests.FogOfWar`, `Kruty1918.Moyva.Tests.Multiplayer`)

Both asmdefs are already `InternalsVisibleTo` targets; create the asmdefs +
tests under `Assets/Moyva/Tests/`.

- Grid: `Unexplored → Visible → Explored`, overlapping sources (counter
  semantics), explored persistence on snapshot roundtrip.
- Perspective: local grid excludes other owners; `SetLocalPerspectiveOwnerId`
  rebuild; ownership transfer moves local contribution.
- Intel: record on visible spawn; hidden move keeps stale record; re-scout
  empty cell removes record; hidden demolition keeps building record; visible
  demolition removes it; ownership transfer while hidden stays stale.
- MP: move-dispatch truth table (known/visible cases) via extracted pure
  policy; `CanPeerObserveWorldEvent` fail-closed; snapshot unit/building
  filter incl. intel section; fail-closed broadcast removal.
- Bots: movement enumerates unexplored reachable tiles; perception counts only
  own + visible + remembered enemies.
- Save: intel roundtrip; explored-per-owner roundtrip.
- Stability: repeated register/move/unregister cycles do not grow internal
  dictionaries beyond live sources (no leak).

## Documentation

- `FOG_OF_WAR_ARCHITECTURE.md` — final data-flow, contracts, MP threat model.
- `FOG_OF_WAR_PERFORMANCE.md` — budgets, batching, large-map/mobile notes.
