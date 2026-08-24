# Moyva Code Map

Purpose: route a human or AI to the minimum source set needed for a task.
Implementation source remains authoritative.

| Concern | Start here | Authority |
|---|---|---|
| Bootstrap / gameplay startup | `Assets/Moyva/Scripts/Bootstrap/` | launch topology -> starting-position workflow -> Turns |
| Turns / rounds | `Assets/Moyva/Scripts/Features/Turns/` | `ITurnService` / `TurnService` |
| Bot turn bridge | `Assets/Moyva/Scripts/Bootstrap/Runtime/TurnBotDriver.cs` | delegates a turn to BotAI |
| BotAI composition | `Assets/Moyva/Scripts/Features/BotAI/Runtime/BotRuntimeBindings.cs` | single BotAI object graph |
| Bot decision loop | `Assets/Moyva/Scripts/Features/BotAI/Runtime/BotTurnExecutor.cs` | planner -> candidate -> canonical action execution |
| Construction | `Assets/Moyva/Scripts/Features/Construction/` | canonical placement/query/mutation services |
| Recruitment | `Assets/Moyva/Scripts/Features/Recruitment/` | canonical recruitment service |
| Units / movement | `Assets/Moyva/Scripts/Features/Units/` | canonical unit/movement services |
| Combat | `Assets/Moyva/Scripts/Features/Combat/` | canonical combat query/command services |
| Economy | `Assets/Moyva/Scripts/Features/Economy/` | economy-owned state and operations |
| Fog / perception | `Assets/Moyva/Scripts/Features/FogOfWar/` | visibility/perception authority |
| Save / restore | `Assets/Moyva/Scripts/Features/SaveSystem/` | central sequencing; feature modules map their payloads |
| World generation | `Assets/Moyva/Scripts/Features/Generator/` | generator-owned coordinator |
| Graph authoring/runtime | `Assets/Moyva/Scripts/Features/GraphSystem/` | graph/node model and evaluation |
| Multiplayer | `Assets/Moyva/Scripts/Features/Multiplayer/` | network boundary; gameplay mutation remains canonical |
| JSON config | `Assets/Moyva/Presets/` + JSON runtime loader | JSON source -> resolved immutable runtime data |

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

## Construction reading map

Do not open every `ConstructionService` partial for a focused task.

| Task | Primary files |
|---|---|
| service lifecycle / dependencies | `ConstructionService.cs` |
| can-place / placement query / spacing | `ConstructionService.PlacementQuery.cs` |
| selection / preview / placed rotation | `ConstructionService.PlacementState.cs` |
| fog / terrain / tile placement rules | `ConstructionPlacementEnvironmentRules.cs` |
| committed-building fog reveal | `ConstructionBuildingFogEffects.cs` |
| placed footprint occupancy / origin mapping | `ConstructionFootprintStore.cs` |
| replacement / gate-wall policy | `ConstructionReplacementPolicy.cs` |
| settlement / influence-zone policy | `ConstructionInfluencePolicy.cs` |
| cost / per-player limits / turn authority | `ConstructionService.EconomyAuthority.cs` |
| confirm / demolish / undo-redo | `ConstructionService.CommitUndo.cs` |
| save restore / singleton reconstruction / footprint rollback | `ConstructionService.Persistence.cs` |
| placement diagnostics | `ConstructionService.Diagnostics.cs` |

Placement validation authority is `ConstructionService.EvaluatePlacement(...)` plus `BuildingPlacementEvaluator`; do not introduce a parallel `ConstructionPlacementValidator`.

`ConstructionService.PlacementRules.cs` no longer exists. Follow the focused ownership rows above instead of searching for a monolithic rules partial.
