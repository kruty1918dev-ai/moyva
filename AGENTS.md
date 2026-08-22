
## JSON Configuration Policy

All Moyva-owned gameplay configuration, definitions, registries, presets, balance data, system settings and generator graphs MUST be authored in JSON under `Assets/Moyva/Presets/`.

- JSON is the single editable source of truth.
- Do not introduce new project-owned ScriptableObject configuration assets.
- Runtime loads JSON once through Load -> Validate -> Resolve -> Freeze and consumes plain C# snapshots/repositories.
- Unity assets are referenced through stable asset IDs resolved by the generated runtime asset catalog.
- Do not use `AssetDatabase` in runtime configuration loading.
- Do not generate ScriptableObject caches from JSON.
- New data-driven entities are added by adding JSON; no inspector registry list is maintained manually.
- Polymorphic module/node IDs are allow-listed stable IDs, never unrestricted CLR type names.

# Moyva Agent Instructions

## Project Context

This is the Unity project **Moyva**.

Moyva is a turn-based strategy game built with:

- Unity 6.x
- C#
- URP
- Zenject
- TextMeshPro
- Odin Inspector where appropriate
- Square grid / tile-based procedural world systems

The project must stay maintainable, modular, and safe for long-term development.

---

## Core Rule

Before writing or modifying code, understand the local architecture.

Do not make broad rewrites unless explicitly requested.

Prefer small, safe, focused changes over large changes.

Every change must preserve existing Unity serialization unless a migration is explicitly requested.

---

## Architecture Principles

Follow these principles:

- SOLID
- DRY
- KISS
- YAGNI
- Composition over inheritance
- Dependency inversion
- Clear separation of responsibilities
- Explicit data flow
- Minimal coupling between systems
- High cohesion inside each feature module

Avoid:

- God classes
- static global state
- hardcoded dependencies
- hidden side effects
- duplicated logic
- oversized MonoBehaviours
- mixing runtime logic with editor logic
- mixing game logic with UI logic
- changing unrelated systems

---

## Unity-Specific Rules

### MonoBehaviour Rules

MonoBehaviours should be thin.

Use MonoBehaviours mainly for:

- Unity lifecycle entry points
- scene references
- view/presentation glue
- serialized configuration references
- forwarding events to services/controllers

Avoid putting complex business logic directly in MonoBehaviours.

Prefer plain C# services/classes for core logic.

---

### ScriptableObject Rules

Use ScriptableObjects for:

- configuration
- presets
- balance data
- tile/building/unit definitions
- editor-authored data
- reusable settings

Do not hardcode gameplay values directly in scripts if they should be configurable.

Bad:

```csharp
private const int MaxBuildings = 12;

## Moyva clean-architecture route

# Moyva agent route

This file is intentionally small. It is the first routing layer for Codex/ChatGPT work.
Do not put long audits, generated logs, patch output, or historical plans here.

## Repository rule

- The checked-out worktree is authoritative.
- `game-process` gameplay changes must preserve current behavior unless a task explicitly changes it.
- Do not infer current architecture from historical plans or `.artifacts`.
- Generated audit/test output belongs under ignored `.artifacts/`, never beside production source.

## Canonical gameplay authorities

| Task | Start here | Authority |
|---|---|---|
| Turn progression | `Features/Turns/API` + `TurnService` | `ITurnService` / `TurnService` |
| Bot turn bridge | `Bootstrap/Runtime/TurnBotDriver.cs` | `IBotTurnExecutor` |
| Bot composition | `Features/BotAI/Runtime/BotRuntimeBindings.cs` | `BotRuntimeBindings.Install` |
| Bot decision loop | `Features/BotAI/Runtime/BotTurnExecutor.cs` | planner → candidate → canonical action executor |
| Bot planning | `Features/BotAI/Runtime/BotTurnPlanner.cs` | query-only planners |
| Build query/mutation | `Features/Construction` | construction placement query/service |
| Recruitment | `Features/Recruitment` | canonical recruitment service |
| Combat | `Features/Combat` | combat query/command service |
| Unit movement | `Features/Units` | movement query/service |
| Save orchestration | `Features/SaveSystem` | central registry/sequencing; feature modules map payloads |
| Direct/menu startup | `Bootstrap/Runtime` | launch topology → starting-position workflow → Turns |
| World generation | `Features/Generator` | generator-owned coordinator |
| Bot editor analysis | `Features/BotAI/Editor` | read-only; never gameplay authority |

## Hard invariants

1. Exactly one authoritative turn state machine.
2. `TurnBotDriver` does not construct a fallback BotAI object graph.
3. `BotRuntimeBindings` is the canonical BotAI composition root.
4. `BotTickScheduler` remains absent/unbound.
5. Bot planners query; `BotActionExecutor` delegates mutation to canonical gameplay services.
6. No Bot-only alternate construction/combat/recruitment/unit mutation path.
7. Tests do not live in Runtime folders.
8. Editor/Analyzer code never becomes runtime decision authority.
9. Direct Gameplay and menu Gameplay converge on the same participant/topology semantics.
10. A compatibility path stays only while a supported mode can reach it.
11. Do not delete serialized Unity types without GUID/reference proof.
12. Prefer semantic names: Installer=bindings, Coordinator=orchestration, Policy=decision rule,
    Resolver=input→result, Store=state, Registry=lookup, Adapter=boundary translation.
13. Avoid new `Helper`, `Utils`, generic `Manager`, or second composition roots.
14. Keep `CODEMAP.md` current and compact.

## Before deleting or merging a type

Check C# references, Unity serialized GUID references, reflection/string lookup, Zenject bindings,
save/version compatibility, asmdef references, EditMode tests, and supported runtime smoke paths.

## Typical verification

Run focused EditMode tests for the changed feature, then the full EditMode suite for architecture work.
For startup changes smoke-test both direct Gameplay and menu Gameplay.
For BotAI changes verify Human → Bot → Human turn handoff and no new Console errors.
