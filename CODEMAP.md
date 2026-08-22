# MOYVA Code Map


<!-- MOYVA_AUDIT_P0_CODEMAP_BEGIN -->
## Canonical gameplay routes

This section is intentionally compact. It exists to give humans and Codex a low-ambiguity starting point before repository-wide search.

| Task | Start here | Authority rule |
|---|---|---|
| Turn / round progression | `Assets/Moyva/Scripts/Features/Turns/` | Turns owns authoritative turn and round state. |
| Bot turn | `Assets/Moyva/Scripts/Features/BotAI/API/IBotTurnExecutor.cs` → `Assets/Moyva/Scripts/Features/BotAI/Runtime/BotTurnExecutor.cs` | The turn executor is the turn-facing BotAI boundary. |
| BotAI composition | `Assets/Moyva/Scripts/Features/BotAI/Runtime/BotRuntimeBindings.cs` | One BotAI binding graph. `BotInstaller.cs` may only delegate to it as a scene adapter. |
| Combat | `Assets/Moyva/Scripts/Features/Combat/API/` | Queries read combat state; canonical combat command/service performs mutation. |
| Unit movement | `Assets/Moyva/Scripts/Features/Units/API/` | Movement queries are read-only; movement mutation remains in the canonical Units service. |
| Recruitment | `Assets/Moyva/Scripts/Features/Recruitment/` | UI and BotAI must use the same authoritative recruitment service. |
| Construction | `Assets/Moyva/Scripts/Features/Construction/` | Placement query and mutation paths remain canonical; AI does not create a private mutation API. |
| Fog / perception | `Assets/Moyva/Scripts/Features/FogOfWar/` + BotAI perception adapters | Enemy tactical discovery uses currently visible enemies; exploration is terrain memory, not enemy position memory. |
| Save / restore | `Assets/Moyva/Scripts/Features/SaveSystem/` + feature save modules | SaveSystem sequences/version-registers; feature modules own feature payload mapping. |
| Gameplay startup | `Assets/Moyva/Scripts/Bootstrap/` | Startup orchestration must converge on one participant/session topology before Turns is mutated. |

## Hard architecture invariants

- Keep one authoritative state-mutation path per gameplay concept.
- Bot planners are query/planning code; mutations go through canonical gameplay services/executor boundaries.
- Do not introduce a wall-clock `BotTickScheduler` alongside turn-driven BotAI.
- Tests belong under test folders/assemblies, never under feature `Runtime` folders.
- Generated audit output, test-run output, console dumps and temporary patch artifacts are not tracked source.
- A feature owns its composition graph. A scene installer may delegate to that graph, but must not re-declare a second graph.
- `Installer` binds; `Coordinator` sequences; `Policy` decides; `Resolver` maps; `Store` owns in-memory state; `Registry` performs keyed lookup; `Adapter` translates boundaries.
- Avoid new domain `Helper`, `Utils`, or generic `Manager` types when a concrete responsibility can be named.

## Before deleting compatibility or fallback code

Prove all of the following first:

1. no required C# call sites remain;
2. no Unity serialized references remain in scenes, prefabs or assets;
3. no reflection/string lookup depends on the type/member;
4. no Zenject binding or installer path depends on it;
5. no save payload/version compatibility depends on it;
6. no asmdef reference exists solely for it;
7. focused EditMode tests pass;
8. relevant runtime/PlayMode smoke path passes.

Do not delete a compatibility path from filename/reference-count evidence alone.

## Generated output policy

These paths are local/CI artifacts, not source:

- `.codex-audit/`
- `.all-editmode-tests/`
- `.artifacts/`

The authoritative implementation is the C# source at the current checked-out commit, not historical generated logs or audit copies.
<!-- MOYVA_AUDIT_P0_CODEMAP_END -->

<!-- MOYVA_CLEAN_ARCH_V1_START -->
## Clean architecture v1 canonical route

- Startup topology: `Bootstrap/Runtime/GameplayLaunchTopology.cs`
- Starting-position policy: `Bootstrap/Runtime/StartingPositionPolicy.cs`
- Assignment mapping: `Bootstrap/Runtime/StartingPositionAssignmentFactory.cs`
- Turn authority: `Features/Turns/Runtime/TurnService.cs`
- Bot turn bridge: `Bootstrap/Runtime/TurnBotDriver.cs`
- Bot composition root: `Features/BotAI/Runtime/BotRuntimeBindings.cs`
- Bot decision coordinator: `Features/BotAI/Runtime/BotTurnExecutor.cs`
- Deterministic AI primitives: `Features/BotAI/Runtime/BotDeterministicGeometry.cs`

One composition root; one turn authority; planners query; canonical gameplay services mutate;
tests stay outside Runtime; generated output stays under ignored `.artifacts`.
<!-- MOYVA_CLEAN_ARCH_V1_END -->
