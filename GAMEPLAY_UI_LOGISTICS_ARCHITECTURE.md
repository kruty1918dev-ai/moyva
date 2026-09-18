# Gameplay UI + Local Logistics — Architecture

Companion to `GAMEPLAY_UI_LOGISTICS_PLAN.md`. Describes the shipped design.

## Core principle

Construction spends **the local settlement's** resources, not an abstract
kingdom pool. Deficits are covered only by physically transporting resources
with wagons between settlements.

```
Settlement stock ──reserve at dispatch──> wagon route ──deliver──> reserved for placement ──consume──> construction
```

## Domain

### Transport capability (`Units`)

- `UnitClassConfig.CanTransportCargo` marks transport-only units
  (`caravan-wagon`, `boat` presets).
- `CaravanGameplayAccess.TryGetWagon` requires the flag **and** a finite
  positive `CargoCapacity`.
- `UnitCombatService.CanAttack` / `UnitCombatCommandService` reject transport
  units (`AttackerNotCombatCapable` / "Transport units cannot attack.").
- `SettlementCaptureService` rejects non-Military units and any unit with
  `CanTransportCargo`.

### Settlement reservations (`Economy`)

- `EconomySettlementState` owns warehouse-reserved pools
  (`ReserveWarehouseResource` / `ReleaseWarehouseResource` /
  `GetReservedResourceAt`) plus settlement-level reservation math
  (`GetAvailableResource` = pool − reserved).
- `EconomyManager.ConsumeResource` and warehouse loads never drain reserved
  stock. `TryConsumeSettlementResources` validates against *available*.
- Reservations persist through `EconomySaveModule` schema **v5**
  (`SettlementSaveSnapshot.ReservedWarehouses`); v1–v4 loads remain readable.

### Construction supply orders (`ConstructionSupplyService`)

`IConstructionSupplyService` (Economy API) owns the order lifecycle:

- `Evaluate` — required vs. locally-available vs. delivered vs. deficit for a
  pending placement; enumerates eligible source warehouses (excluding the
  target settlement, minus existing reservations) and eligible wagons
  (capacity, cargo used, busy status).
- `DispatchSupply` — reserves the selected source warehouse stock, registers
  the order (`SourceReservations`, `DeliveredResources`, wagon ids), and
  starts a real `CaravanRouteRequest`.
- Delivery adds cargo to the target warehouse **reserved for this placement**
  (`SettlementResourceChangedSignal` keeps UI fresh); when the deficit is
  covered the order becomes Ready and fires `ConstructionSupplyReadySignal`.
- `CancelOrderAt` / `CloseOrderAt` release source + delivered reservations
  and stop associated routes; `ConstructionSupplyOrderClosedSignal` is the
  multiplayer replication hook.
- `ReleaseRouteLoadReservations` — called by `CaravanService` immediately
  before a route leg's `Load` validation so the wagon can take the stock the
  order reserved for it.
- Persistence: save module v2 serializes source reservations, order identity,
  owner, building, position, target settlement/warehouse, status, required /
  delivered / remaining amounts, wagon ids.

### Caravan routes (`CaravanService`)

- `SetRoute`/`CanSetRoute` validate wagon capability, finite capacity,
  settlement/warehouse ownership, and cargo ≤ capacity.
- `Load` legs check *available* stock (warehouse + settlement pool minus
  reservations) — after releasing the route's own source reservations.
- Cargo survives route interruption/failure.
- `GetRoutes(ownerId)` feeds the HUD logistics tab.

### Construction funding (`ConstructionService.Economy`)

Confirm order:

1. resolve target settlement (`TryResolveConstructionSettlement`);
2. placement-aware projection (`GetSettlementResourcesForPlacement` =
   local stock − other reservations + this placement's delivered cargo);
3. validate every cost;
4. release *this placement's* delivery reservations;
5. `TryConsumeSettlementResources`;
6. invalidate the resource-validation cache.

A failed confirm no longer destroys in-flight supply reservations. Cancelling
a pending placement calls `IConstructionSupplyService.CancelOrderAt`.

## Multiplayer (host-authoritative)

- `CaravanCommandAction.SupplyDispatch` / `CancelSupply` ride the existing
  `CaravanCommandPayload` (position is packed in `TargetWarehouseKey`).
- Client `DispatchSupply` → `TryRequestSupplyDispatch` → host
  `ExecuteAuthorizedCaravanCommand` → authoritative order + route.
- Host then broadcasts (a) confirmed `SupplyDispatch` so the requester mirrors
  the order via `ApplyConfirmedDispatch`, and (b) a confirmed `StartRoute`
  carrying the actual shipment so peers mirror the route.
- `CancelOrderAt` on the host fires `ConstructionSupplyOrderClosedSignal` →
  confirmed `CancelSupply` per wagon → `ApplyConfirmedCancelOrder` stops
  mirrored routes without re-requesting.

## HUD (UnityHTML)

- Pending placement with a local deficit shows a **SUPPLY** CTA
  (`GameplayHtmlMarkup.Core`); top bar is labelled as kingdom-wide totals;
  unaffordable text names the settlement.
- `GameplaySupplyPanel` (action `ui.logistics.supply`, bridge
  `OpenSupply`/`SetSupplySource`/`SetSupplyWagon`/`DispatchSupply`/
  `FocusWarehouse`): local coverage per resource, source warehouses, wagons
  with free capacity, current order state, dispatch rejection reason.
  Markup states "Resources move only by wagon — deliveries are reserved for
  this construction."
- Kingdom dashboard **LOGISTICS** tab lists active caravan routes and supply
  orders (`CaptureLogistics` in `GameplayHudReadModel`).
- `ConstructionSupplyReadySignal` → success notification naming the building
  and settlement.
- Selection cleanup (`WorldInfoSelectionCoordinator`): unit destroyed →
  refresh/clear; building demolished → position-based clear (building id is a
  type id); settlement captured → close panel when the selected map object's
  center matches.

## Tests

`Assets/Moyva/Tests/Editor/Economy/` (`Kruty1918.Moyva.Tests.Economy`):

- `SettlementReservationTests` — reserve/release math, consumption never
  drains reserved stock.
- `TransportCapabilityTests` — wagons can't attack/capture; non-transport
  cargo units are rejected.
- `ConstructionSupplyServiceTests` — dispatch → route → load → deliver →
  reserve → ready end-to-end with shared fakes (`EconomyTestDoubles`).

The test asmdef needs `precompiledReferences: ["Zenject-usage.dll"]` because
test assemblies don't auto-reference the Editor-disabled plugin DLL that
hosts `IInitializable`/`LazyInject`.
