# Gameplay UI + Local Logistics — Implementation Plan

Base: `game-process` @ `387a07ef`. Branch: `feature/gameplay-ui-logistics`.

## CURRENT STATE

Already present (verified by audit):

- `ConstructionService.Economy` funds construction from the **nearest owned
  settlement's** `ResourcePool` via `TryResolveConstructionSettlement` +
  `TryConsumeSettlementResources`. Pending placements reserve their costs in the
  resource *projection* (`BuildReservedConstructionCosts`), preventing
  double-spend between pending buildings.
- Owner pool (`EconomyOwnerResourcePoolService`) is a bootstrap-only wallet:
  used only while the owner has **no warehouse** (`OwnerHasAnyWarehouse`),
  transferred into the first warehouse automatically.
- `CaravanService` (Economy): wagon cargo state, capacity check, warehouse
  capacity + accepted-resource policy, adjacency check, atomic load/unload,
  dropped loot, automatic routes (async movement legs), settlement founding,
  staged save module (v3) with validation, multiplayer `ApplyConfirmed*`
  replica path.
- `CaravanGameplayAccess` (Bootstrap): real unit/ownership/turn/authority
  integration; wagon identified by `CargoCapacity > 0` only.
- `MultiplayerAuthorityService.CaravanCommands`: request/confirmed/rejected
  pipeline, request-id idempotency, owner authorization.
- `GameplayHudReadModel`/`GameplayHtmlState`/markup: selection inspector,
  recruitment tabs, cargo/route tabs, kingdom dashboard (incl. per-settlement
  resources + warehouses), notifications with focus, pause/game-over modals,
  viewport classes (vp-compact/vp-wide/vp-short), safe-area layout component.
- `caravan-depot` building already recruits `caravan-wagon` via canonical
  `unit-recruitment` module; recruitment consumes settlement-local resources
  (`TryConsumeRecruitmentCosts`).

## PROBLEMS

1. `TryGetWagon` accepts **any** unit with `CargoCapacity > 0`; no formal
   transport capability.
2. `caravan-wagon` (`crushingDamage 2`, `attackRange 1`) and `boat` can attack;
   `SettlementCaptureService` never checks the unit role — a Worker wagon can
   capture settlements. No transport-only enforcement at domain level.
3. UI messages say "The kingdom cannot afford this placement" although funding
   is settlement-local — misleading (kingdom total is not spendable remotely).
4. Top bar shows kingdom aggregate without marking it as a non-spendable total;
   the building/settlement inspector does not show the settlement's local pool.
5. No supply planning: when a settlement lacks resources there is no UX to find
   sources and dispatch real wagon transport toward the deficit.
6. Pending-placement reservation exists only in the projection; delivered
   resources can be stolen by recruitment/other consumption before confirm.
7. Selection lifecycle: `WorldInfoSelectionCoordinator` does not clear selection
   when the selected unit dies / building is demolished / settlement captured.
8. No construction supply order tracking (delivered-vs-required, reservation,
   Ready notification).
9. No logistics overview (active routes/wagons) in the kingdom dashboard.
10. Unit groups: dashboard `UnitGroups` is type aggregation only; no
    commandable group system exists.

## TARGET UX

- One context inspector (existing side panel) for unit/building/settlement.
- Context actions with explicit disabled reasons (attack/capture rows already
  render reasons — extended to transport-only units).
- Kingdom top bar clearly labelled as kingdom totals; settlement inspector
  shows LOCAL pool + reserved amounts.
- Construction: pending placement with deficit shows per-resource
  required/available/missing + "REQUEST SUPPLY" → supply dialog → real caravan
  routes; no instant remote build.
- Wagon inspector: transport role, cargo used/capacity, route status/actions.

## LOCAL ECONOMY MODEL

- `EconomySettlementState.ResourcePool` = settlement truth; warehouse pools
  mirror it per warehouse key.
- NEW `WarehouseReservedPools`: reservations created by supply deliveries,
  keyed per warehouse. Settlement reserved total = sum over warehouses;
  "available" = pool − reserved.
- `TryConsumeSettlementResources` validates against available (pool −
  reserved). Construction confirm releases the placement's reservation
  immediately before consuming (`IEconomyInfoMediator
  .ReleaseConstructionReservations(position)`), so it consumes its own
  delivery. Recruitment and other consumers cannot touch reserved stock.
- Caravan Load validates `warehouse[res] − reserved[wh][res]`.
- Cancel pending placement → release reservation + cancel supply order.
- Owner pool remains bootstrap-only (no warehouse ⇒ owner pool; first
  warehouse drains it). Not changed.

## WAGON MODEL

- `UnitClassConfig.CanTransportCargo` (JSON `canTransportCargo`). Set on
  `caravan-wagon` and `boat`; both get zero attack damage.
- `TryGetWagon` requires `CanTransportCargo && CargoCapacity > 0`.
- Domain enforcement (not UI hiding):
  - `UnitCombatService.CanAttack` rejects transport units
    (`AttackerNotCombatCapable` reason).
  - `UnitCombatCommandService.TryValidateBuildingAttack` same.
  - `SettlementCaptureService.TryEvaluateCapture` requires `Role == Military`
    and rejects transport units.
- Wagons come only from `caravan-depot` recruitment (existing).

## SUPPLY MODEL

- `ConstructionSupplyService` (Economy runtime, bound in EconomyInstaller):
  - `Evaluate(ownerId, buildingId, position)` → target settlement, per-resource
    required/available/deficit, source options (settlement+warehouse+amount),
    wagon options (id, capacity, free/busy, position).
  - `RequestSupply(...)` (authoritative only) → creates order, reserves nothing
    yet, dispatches `ICaravanService.SetRoute` (repeat) for chosen wagon:
    source warehouse → target warehouse, shipment = min(deficit, capacity).
  - On `CaravanDeliveryCompletedSignal` matching order target: add delivered
    amounts to `WarehouseReservedPools[deliveryKey]`, decrement order remaining;
    when remaining = 0 → order Ready → notification.
  - Multi-trip/multi-wagon: shipment per trip = capacity-limited; order tracks
    cumulative delivery; order stops route when fulfilled.
  - Client peers: TRANSFER issues canonical `TryRequestSetRoute`; orders on the
    client are display-only mirrors derived from replicated routes/deliveries.
- Orders saved via own `SaveModuleId` (wagon ids, target, remaining, reserved).

## MULTIPLAYER AUTHORITY

- No new wire commands: supply dispatch reuses `TryRequestSetRoute` (already
  idempotent + authorized). Orders are host-truth; client state is derived.
- All cargo/route mutations remain host-only via existing pipeline.

## SAVE / PERSISTENCE

- `CaravanService` module keeps cargo/loot/routes (v3).
- New `ConstructionSupplyService` save module: orders + per-warehouse
  reservations (stored on `EconomySettlementState`, serialized inside the
  supply module to avoid touching `EconomySaveModule` format).

## UI CHANGES

- ReadModel: settlement context facts in building inspector (local pool,
  reserved); transport unit facts (cargo used/capacity, no combat stats);
  fixed unaffordable message naming the settlement.
- Markup: "KINGDOM TOTAL" caption; supply panel (`GameplayHtmlPanel.Supply`)
  with deficit rows, source/wagon selects, trips estimate, TRANSFER; logistics
  tab in kingdom dashboard (active routes, wagons); pending-placement deficit
  block with REQUEST SUPPLY CTA.
- Selection lifecycle: clear/refresh on `UnitDestroyedSignal`,
  `BuildingDemolishedSignal`, `SettlementCapturedSignal`.

## PATCH ORDER

1. Plan doc (this file).
2. Transport capability + combat/capture enforcement + JSON/schema.
3. Settlement reservations + release-on-confirm/cancel + caravan load guard.
4. `ConstructionSupplyService` + save module + signals.
5. HUD: inspectors, labeling, supply panel, logistics tab, lifecycle.
6. Tests + docs + PR.

## TEST MATRIX (implemented subset)

- CaravanService: wagon-only recognition (flag), capacity, atomic load/unload,
  warehouse policy, route validation, loot, save/load roundtrip.
- Reservations: consume blocked by reservation; release enables consumption;
  caravan load cannot take reserved stock.
- Supply: deficit detection, source ranking, wagon capacity split, delivery →
  reservation → ready; cancel releases.
- Capture: transport/worker rejected; combat: transport attacker rejected.
- Markup/state: supply snapshot fields, tab gating, message strings.

## DEFINITION OF DONE (this patch)

- Wagons are transport-only at domain level; produced via depot.
- Construction is settlement-local; kingdom total is informational only and
  labelled as such; deficit UI is settlement-specific.
- Delivered construction resources are reserved and cannot be stolen or
  double-spent; cancel releases.
- Supply dialog executes real wagon transport; construction becomes ready only
  after physical delivery.
- Multiplayer/save semantics preserved; EditMode tests cover new domain rules.
- Known limitation (documented in PR): commandable unit groups/multi-select
  not included in this patch.
