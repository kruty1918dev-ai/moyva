# Moyva — Agent Rules

Canonical agent instructions. No audits, inventories, patch logs or historical plans here.

## Project

- Unity 6.x, C#, URP.
- Turn-based strategy, procedural square grid.
- Zenject is used for dependency injection.
- Production gameplay source lives under `Assets/Moyva/Scripts/`.
- Feature ownership, explicit data flow, one mutation authority per concept.

## Source of truth

1. Checked-out C# and Unity serialized assets are authoritative.
2. `CODEMAP.md` is the routing map, not a second specification.
3. Moyva-owned gameplay configuration/definitions/presets are JSON under `Assets/Moyva/Presets/`.
4. Do not introduce new project-owned ScriptableObject configuration as an alternative source of truth.
5. Historical plans, audits, inventories and backups are not architecture authority.

## Before changing code

Start with about 3–6 relevant files; expand only along concrete dependencies:

1. Find the task/feature in `CODEMAP.md`; read that section, not the whole map.
2. The specific API contract(s), not the whole `API/` directory.
3. Its installer/composition root.
4. The target implementation: relevant methods and surrounding lines.
5. Focused tests for that feature.

Use scoped `rg --files` or `rg -n` to locate files/symbols before reading content.
Never load all feature services or partials just because they share a directory/type.

### Default context exclusions

Unless the task explicitly concerns them, do not read:

- `**/Development/**`
- `**/Editor/**`
- `**/Tests/**` before the production path is identified
- `docs/**`, historical migration reports and archived documentation
- generated site output (`.docs-site/`) and docs site templates unless working on publishing
- backup/audit artifacts

Search production API, composition and Runtime first; expand on references or failing tests.
Exclude serialized assets from source searches; inspect them for GUID/compatibility work.

## Architecture rules

- SOLID, DRY, KISS, YAGNI.
- Composition over inheritance.
- Prefer plain C# domain/services; keep MonoBehaviours thin.
- One authoritative state-mutation path per gameplay concept.
- Recruitment: `Features/Units`, `IUnitRecruitmentService` / `UnitRecruitmentService`; no parallel Recruitment feature.
- UI, multiplayer adapters, and editor tools must delegate to canonical gameplay services.
- Feature modules own their bindings/composition. Scene/bootstrap installers may delegate, not duplicate the graph.
- Tests stay outside production `Runtime/` folders.
- Editor/analyzer code never becomes runtime decision authority.
- Avoid hidden static global state and hidden side effects.
- Avoid generic `Manager`, `Helper`, `Utils` when a concrete responsibility can be named.
- Do not create an interface for a class unless it is an actual boundary, substitutable dependency, or test seam.
- Keep cohesive files; do not split just to satisfy SRP.

Naming semantics:

`Installer` = DI; `Coordinator` / `Orchestrator` = sequencing; `Policy` = decision rule;
`Resolver` = input → result; `Store` = owned state; `Registry` = keyed lookup;
`Adapter` = boundary translation; `Service` = cohesive capability.

## Unity safety

Never delete, rename, or merge serialized Unity types from C# reference count alone.

Before removal, check C# and scene/prefab/asset GUID references, Zenject bindings,
reflection/string lookup, asmdefs, save/version compatibility, EditMode/PlayMode tests
and supported runtime smoke paths.

Preserve `.meta` files when moving Unity assets.

## JSON configuration policy

Configuration, definitions, registries, presets, balance, settings and generator graphs
are authored in JSON under `Assets/Moyva/Presets/`.

Runtime path:

`Load -> Validate -> Resolve -> Freeze -> Consume`

Rules:

- JSON is the editable source of truth.
- Runtime consumes resolved plain C# snapshots/repositories.
- Unity object references use stable asset IDs resolved through the runtime asset catalog.
- No `AssetDatabase` in runtime loading.
- No generated ScriptableObject cache mirroring JSON.
- Polymorphic IDs are allow-listed stable IDs, not unrestricted CLR type names.

## Verification

For a focused change:

1. compile;
2. run focused EditMode tests;
3. inspect affected Unity serialization/bindings when relevant.

For architecture/startup changes additionally:

- run the full relevant EditMode suite;
- smoke-test direct Gameplay and menu -> Gameplay;
- verify no new Console errors.

## Context discipline

Modify canonical files; consolidate duplicate explanations. Generate inventories on
demand, never as tracked snapshots. Keep AGENTS within 6 KiB and CODEMAP within 16 KiB.

## Agent cost hygiene

Exclude from ordinary reads; opt in for relevant tasks:

- `Assets/ThirdParty/**`
- `Assets/FlatKit/**`
- `Assets/KayKit/**`
- `Assets/TextMesh Pro/**`
- `Assets/Plugins/**`
- `Library/**`, `Temp/**`, `Logs/**`, `Obj/**`, `Build/**`, `Builds/**`
- generated audit, recovery, and test-output artifacts

Project Codex defaults: Astra, medium reasoning, one agent. Use the client model/effort
selector for a difficult stage (high); do not rewrite config to change a running turn.
Keep related work in one session; start a new session for an independent task.

Return short tool results (normally <=2,000 tokens). Narrow an oversized query instead
of repeating it with a larger output limit; retrieve missing details by symbol/range.
Use quiet verification scripts under `tools/ai/`: full logs in ignored `Temp/ai/`,
only failures and a short summary in context. Do not repeat passed checks without cause.

When measuring cost, report task deltas for input/cached input/output, peak request
context, tool-output size and agent count. Exclude inherited counters and duplicate
events; compare completed tasks with equivalent verification, not raw token totals.

After creating logs, reports or temporary context, run `tools/ai/check-context-hygiene.sh`.
Remove generated artifacts introduced by the task from tracked paths; preserve user work.
