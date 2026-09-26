# Economy Action Guidance Plan

Date: 2026-09-26. Scope: shared "what is blocking my action and how do I fix it"
system across construction, unit recruitment and notifications.

All findings below verified against code, not assumed.

## 1. Economy as it actually works

### Resources

- ~90 resources defined in `Presets/Economy/economy-resource/*.json`
  (`displayName`, `category` Food/Materials/…, `icon`, `stackLimit`); the
  database `economydatabase.json` only references ids.
- Similar names exist and must stay distinct: `walnut-wood-materials-resources`
  (raw logs) vs `hardwood-materials-resources` (planks; sawmill converts 2→3),
  `wheat-bundle-food-resources` (crop) vs `steak-food-resources` (cooked meal).

### Ways to obtain a resource (all real mechanics)

| Path | Status |
|---|---|
| Building production recipes | Yes — building module `$type:"production"` with `recipes[]` (inputs/outputs/turnsPerCycle/requiresWorkers/requiresStorageSpace). Producers: farm/watermill/windmill-01 → wheat; wood-camp → walnut-wood; sawmill → hardwood; stone-quarry → stone; iron-mine → ore; smelter → ingots; weapon/armor-workshop → gear; tavern → gold. |
| Starter packs / scenarios | Yes — `BootstrapGameSettings` (140 steak, 180 walnut wood, 120 hardwood, 100 stone, 20 ore, 20 ingots, 50 gold). |
| Supply wagons | Yes — `IConstructionSupplyService` moves resources settlement→settlement; HUD already exposes `OpenSupply`. |
| Caravan routes | Yes — `ICaravanService` transfers/routes between own settlements. |
| Map-object harvesting | API exists (`IMapObjectEconomyService`) but `yieldsResource=true` count is **0** — no harvestable source today. |
| Market trading | Only base prices exist in the database; no buy/sell action exists. |

### Spending / reservation

- Construction: `ConstructionService.TryConsumeConstructionResources` —
  settlement stock (`GetSettlementResourcesForPlacement`, includes that
  placement's own delivered supply) or owner pool when the owner has no
  warehouse. Pending placements reserve via `BuildReservedConstructionCosts`.
- Recruitment: `UnitRecruitmentService.TryEnqueue` reserves resources +
  population at enqueue; `TryCancel` refunds.

### Canonical validators (reuse, do not duplicate)

- `IConstructionSessionCommands.GetResourceProjection(pos)` →
  `ConstructionResourceProjection{Balances[ResourceId,Available,Reserved,
  Remaining,IsDeficit], HasDeficit, HasSettlement, SettlementId/Name}`.
- `IConstructionSessionCommands.TryGetPendingPlacementStatus` → per-placement
  spatial/affordability reason.
- `IConstructionSelectionAvailabilityQuery.EvaluateSelectionAvailability` →
  `GlobalAvailabilityValid / ResourcesValid / Reason / ReasonCode`.
- `IUnitRecruitmentQuery.TryGetEnqueueShortages` → all
  `UnitRecruitmentShortage` entries (resource id, required, available,
  reserved, missing, `IsPopulation`, `PopulationGrowthBlocker` Housing/Food).
- `ProducerFeasibilityResolver.Suggest` → bounded resource→producer search,
  cycle-safe (visited set, depth 4), kinds Direct/ViaPrerequisite/Impossible/
  None. Already used for recruitment card hints.
- `IConstructionPortfolioQuery.GetOwnerPlacements` +
  `IConstructionLifecycle.IsOperational/TryGetProgress` → producer exists /
  under construction / operational.
- `IEconomyRuntimeApi.GetOwnerProductionSnapshot` →
  `ActiveProducerBuildingsByType`, `ProductionPerTurn` → producer actually
  producing right now.
- Population blockers: `PopulationGrowthBlocker.Housing` (beds full →
  `HousingBuildingHint`) / `.Food` (need `Food`-category stock > 0).
- Worker staffing is **automatic** (`EconomyWorkerAllocationService.Allocate`
  each tick); no manual assign UI — guidance must say "needs workers /
  population", not "assign a worker".

### Confirmed data gap: `steak-food-resources` has no source

- Required by barrack recipes (warrior 10, spearman 12, archer 10) and stable
  (light-cavalry 20).
- No building produces it; no map-object yields it; only starter stock (140).
- Once starters are spent, recruitment becomes permanently impossible —
  a real deadlock, and guidance would honestly show "no way to obtain".

**Fix (data, not free resources):** add recipe `tavern:cook-steak` to
`Presets/Buildings/tavern.json` — inputs `wheat-bundle-food-resources` ×2 →
outputs `steak-food-resources` ×3, `turnsPerCycle: 1`, `requiresWorkers:
false` (tavern is `workerless`). Wheat→steak stays inside the `Food` category
so population food accounting is preserved; the producer filter already
collects recipe outputs (`ResolveProducedResourceIds`), so the tavern appears
as a steak producer with no code change.

## 2. What exists today vs the requirement

Already present:

- Structured shortage data both sides (projection balances; recruitment
  shortages incl. population blocker).
- Recruitment cards show per-resource missing text + "Find X producer"
  (`ShowProducersFor` opens construction filtered by produced resource).
- Recruitment rejection pins a warning notification.
- Notifications with a position open camera focus + building info panel
  (`OpenNotification`).
- `HasPendingSupplyDeficit` → SUPPLY button → supply panel.
- Presenter re-renders on `SettlementResourceChangedSignal` /
  `EconomyTickCompletedSignal` — live updates are free if the popup reads
  from the read model every render.

Missing:

- A guidance popup: single overlay listing **all** blockers with
  Need/Have/Missing per resource, per-blocker actions, Next/Prev + count,
  live recompute, "return to goal" CTA.
- Construction-side structured guidance: `ConfirmPlacement` rejection only
  produces a flat feedback string today.
- Producer state awareness: today the suggestion only knows "buildable now";
  it cannot say "you already have one producing/under construction/idle".
- Goal persistence (what the player originally wanted + where).
- Interactive deficit notifications (notification → reopen this popup).
- Scroll-to/highlight a suggested building card in the construction list.

## 3. Design

### `GameplayGuidanceResolver` (new, `GameplayHUD/Runtime`, pure C#)

Composes canonical queries into a view model — never mutates, never pays:

```
GuidanceModel { Goal, Blockers[] }
Goal { Kind: BuildPlacement|Recruit, BuildingId/UnitTypeId, Position,
       SettlementId, Label }
Blocker { Kind: Resource|Population|Placement|Eligibility|Generic,
          ResourceId, Required, Available, Reserved, Missing,
          Resolved, Text, Options[] }
ResolutionOption { Kind: BuildProducer|FocusExisting|UnderConstruction|
                         OpenSupply|BuildHousing|ProduceFood|Unobtainable,
                   BuildingId/ResourceId/Position, Label, Hint }
```

- `BuildForPlacement(ownerId, positions)`: per pending placement —
  `TryGetPendingPlacementStatus` (spatial/affordability reason) +
  `GetResourceProjection` (per-resource balances incl. other reservations) +
  `EvaluateSelectionAvailability` (global prereq).
- `BuildForRecruitment(ownerId, buildingPos, unitTypeId)`:
  `TryGetEnqueueShortages` → resource/population blockers; plain `reason`
  (queue full, wrong building, eligibility) → Generic blocker.
- `ResolveOptions(blocker, ownerId, settlementId)`:
  - `ProducerFeasibilityResolver.Suggest` → buildable producer or
    prerequisite chain (`ViaPrerequisite` names the intermediate resource —
    that is the honest dependency chain the spec asks for).
  - Enrich with portfolio+lifecycle+production snapshot:
    producing now → `FocusExisting` ("go to it"); placed but not operational →
    `UnderConstruction` (progress hint); operational but not producing →
    `FocusExisting` with "needs workers/inputs" hint (staffing is automatic —
    say population, not an assign-worker action).
  - Settlement-funded deficit → `OpenSupply` alternative when the supply
    service can reach the settlement.
  - `PopulationGrowthBlocker.Housing` → `BuildHousing` (first selectable
    housing building — reuse `HousingBuildingHint` logic);
    `.Food` → producer search on a `Food` resource.
  - `None`/`Impossible` producer → `Unobtainable` with honest text.

### `GameplayHtmlState` additions

- `GuidanceSession` — `Goal` + `Open` flag + `FocusIndex`. Blockers are NOT
  cached in state; `GameplayHudReadModel` recomputes them inside `Capture`
  each render → fulfilled blockers mark themselves `Resolved`, and when all
  are resolved the popup shows the resume-goal CTA.
- `ConstructionFocusBuildingId` — markup paginates the filtered list to the
  page containing it and adds a `guidance-focus` class (short highlight).
- Notification link: extend `GameplayNotificationViewSnapshot` with a stored
  `GuidanceGoal` (kind + ids + position). `OpenNotification` for a
  guidance-linked item rebuilds the model; if no blockers remain → feedback
  "already resolved", CTA offers resume.

### Bridge (`GameplayHtmlBridge`)

- `ConfirmPlacement`: on rejection keep the feedback line, and when a
  resource/placement deficit is confirmed build the placement goal and open
  the popup (explicit click only — preview/hover never opens it).
- `Recruit`: on `InsufficientResources` keep the pinned notification but give
  it a guidance goal + open the popup.
- `GuidanceNext/Prev/GoTo/Close`, `GuidanceAction(optionIndex)` dispatches:
  BuildProducer → open construction + focus+select the card (goal preserved);
  FocusExisting/UnderConstruction → camera focus + building panel via
  `BuildingInfoPanelRequestedSignal`; OpenSupply → existing supply panel;
  BuildHousing/ProduceFood → same as BuildProducer.
- `ResumeGoal`: re-validates and re-enters the original action — reselect
  building + `TryPreviewAt(savedPosition)` (construction) or reopen the
  recruiting building panel on the Recruit tab; stale context → feedback
  explains.
- Repeated rejection on the same goal replaces the session (single popup).

### Markup / CSS

- `AppendGuidance` in the overlay region (scrim+dialog pattern, like
  `AppendDashboard`): title "Cannot {goal label}" + "N blockers" counter +
  blocker cards (icon, Need/Have/Missing, reason) + per-blocker action
  buttons + Next/Prev list nav + Close + "Back to {goal}" when resolved.
- `.guidance-*` classes + `.building-row.guidance-focus` pulse border in
  `GameplayHud.css.txt`.
- Esc: `Diagnostics.PanelClose` (presenter handler) dismisses the popup
  before panels; `ClosePanel` bridge handles the same order.

### Out of scope / honest limits

- No market trading and no harvestable map objects exist — alternatives are
  supply wagons and building producers only.
- No worker assignment UI — staffing guidance is informational.
- Client role: `TryGetEnqueueShortages`/`GetResourceProjection` read
  replicated state as today; remote rejections degrade to a reason-string
  blocker.
- Notifications are in-memory (cap 20) — no persistence between sessions, so
  no save-load link repair needed.

## 4. Integration points (files)

- `GameplayHUD/Runtime/GameplayGuidanceResolver.cs` — new resolver + DTOs.
- `GameplayHUD/Runtime/GameplayHtmlState.cs` — session, focus id,
  notification goal field.
- `GameplayHUD/Runtime/GameplayHudReadModel.cs` — build `GuidanceModel` in
  `Capture`; reuse `ResolveProducerActionIds`/`HousingBuildingHint`.
- `GameplayHUD/Runtime/GameplayHtmlBridge.cs` — endpoints above.
- `GameplayHUD/Runtime/GameplayHtmlMarkup.Core.cs` (+ maybe a `.Guidance`
  partial) — popup + focus-class rendering.
- `GameplayHUD/Runtime/GameplayHtmlPresenter.cs` — PanelClose ordering.
- `UI/Gameplay/GameplayHud.css.txt` — classes.
- `Presets/Buildings/tavern.json` — `tavern:cook-steak` recipe.
- Tests: `Bootstrap/Tests/Editor/GameplayGuidanceResolverTests.cs` (new);
  `GameplayHtmlActionMapTests` sweep auto-covers new buttons — every enabled
  control must have an observable effect.

## 5. Acceptance criteria

- Deliberate failed `ConfirmPlacement` (insufficient resources) opens one
  popup listing every deficit resource with Need/Have/Missing + a suggested
  action per blocker; repeated clicks reuse the same popup; hover/preview
  never opens it.
- Failed `Recruit` does the same for shortages + population blocker.
- "Build {producer}" opens construction, clears conflicting filters, pages
  to and highlights the card, selects nothing destructive, preserves the
  goal; a goal chip/CCA lets the player return.
- Existing producer → camera focus + building panel; under-construction →
  progress hint; producing → honest "already producing, see output".
- All resolved → "Back to {goal}" re-enters placement/recruitment with
  revalidated context; stale context explains itself.
- Popup content refreshes on `SettlementResourceChangedSignal`/economy tick;
  resolved blockers render as resolved.
- Cyclic producer graphs terminate (existing resolver tests stay green);
  steak shows a real producer path after the tavern recipe.
- Esc/X closes popup first, then panels; single popup instance.
- Client role: no local mutation; actions route through existing canonical
  paths (remote requester for recruit).
- EditMode: focused guidance tests + `ProducerFeasibilityResolverTests` +
  `GameplayHtmlActionMapTests` + full suite green; smoke compile clean.

## 6. Implementation status (2026-09-26)

Implemented, all pieces wired:

- `GameplayGuidanceResolver` (`GameplayHUD/Runtime`) — `BuildPlacement` merges
  `TryGetPendingPlacementStatus` + `GetResourceProjection` per pending
  placement (spatial `Placement` blocker, missing-settlement `Eligibility`
  blocker, one `Resource` blocker per deficit balance); `BuildRecruitment`
  maps every `UnitRecruitmentShortage` (resource + population Housing/Food);
  `Refresh` re-marks `Resolved` from live totals each capture. Options:
  producing→`FocusProducer`, constructing→`ProducerConstructing`,
  idle→`FocusProducer`+staffing hint, feasible→`BuildProducer`,
  blocked→`ViaPrerequisite` build step, cycles→honest `Unobtainable`,
  settlement-funded deficit→`OpenSupply`, reserved stock→`OpenQueue`,
  housing→`BuildHousing`, food blocker→`ProduceFood`.
- `GameplayHtmlState` — `GuidanceSession{Goal,Open,FocusIndex}` (single
  popup instance; repeat rejection replaces the session), goal-linked
  notifications (`GameplayNotificationViewSnapshot.Goal`), goal-keyed
  notification dedup in `AddNotification`, blocker focus toggle
  (re-click collapses), `SetConstructionFocus` → one-shot page jump +
  ~6 s `HighlightedConstructionId` highlight cleared on filter/select.
- `GameplayHudReadModel` — `CaptureGuidance` rebuilds blockers from
  canonical queries every capture (only pendings for the goal's building);
  `TryBuildPlacementGuidance`/`TryBuildRecruitmentGuidance` probes used by
  the bridge return false when nothing is explainable.
- `GameplayHtmlBridge` — `ConfirmPlacement` rejection → feedback +
  guidance popup (explicit confirm only); `Recruit` `InsufficientResources`
  → goal-linked deduped notification + popup; endpoints `GuidanceMove/
  Select/Close/DismissGoal/Reopen/BuildProducer/FocusBuilding/Supply/
  OpenQueue/ResumeGoal`; `OpenNotification` reopens guidance for
  goal-linked items (live recompute — resolved goals show the resume CTA);
  `ClosePanel` peels the popup before any panel; client role never opens
  local guidance for remote-requested actions.
- `GameplayHtmlMarkup.Guidance.cs` — scrim+dialog: goal title,
  "N blockers remain" counter, blocker cards (icon, Need/Have/Missing,
  reserved note, detail) with per-card action buttons only on the focused
  card (no nested buttons), PREV/NEXT + index, BACK TO GOAL CTA.
  `Core`: goal chip in the command bar while minimized, DETAILS button on
  goal-linked notifications, `guidance-focus` highlight + one-shot page
  jump in the building list. `Regions`: overlay renders after dashboard.
- `GameplayHtmlAnchor` — open guidance = full-screen input shield (map
  clicks cannot leak through).
- `GameplayHtmlPresenter` — `Diagnostics.PanelClose` peels popup first.
- `GameplayHud.css.txt` — `.guidance-*` block + `.building-row.guidance-focus`.
- `Presets/Buildings/tavern.json` — `tavern:cook-steak` (2 wheat → 3 steak,
  1 turn, workerless) — removes the steak deadlock via the real producer.

Verified:

- `smoke_compile.py` clean (0 errors).
- `GameplayGuidanceResolverTests` 13 + `GameplayGuidanceStateTests` 12 +
  action-map guidance sweep 1 = 27/27 pass (filter `GameplayGuidance`).
- `GameplayHtmlActionMapTests` 12/12 — every enabled guidance control
  produces an observable effect; no dead buttons.
- `GameplayHtmlClosePanelTests` 22/22, `ProducerFeasibilityResolverTests`
  8/8, `UnitRecruitmentShortageTests` 7/7, `ConstructionSelectionAvailability`
  5/5.

Residual limits (unchanged scope):

- Page jump is instant (pager), not an animated smooth scroll — the HTML
  list is paged, there is no scroll-position API to animate.
- Client role: remote rejection reasons still surface as text only.
- Notifications remain in-memory; no cross-session persistence needed.
- QuickJS teardown flake ("enqueue pending action after runtime shutdown")
  observed once in the sweep — GC finalizer race on host dispose,
  nondeterministic, unrelated to guidance logic; reruns pass.
