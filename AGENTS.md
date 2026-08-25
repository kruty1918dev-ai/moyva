# Moyva — Agent Rules

This file is the canonical instruction entry point for AI/code agents.
Keep it short. Do not add audits, generated inventories, patch logs, or historical plans here.

## Project

- Unity 6.x, C#, URP.
- Turn-based strategy on a square/tile procedural world.
- Zenject is used for dependency injection.
- Production gameplay source lives under `Assets/Moyva/Scripts/`.
- Current architecture favors feature ownership, explicit data flow, and one authoritative mutation path per gameplay concept.

## Source of truth

1. Current checked-out C# source and Unity serialized assets are authoritative.
2. `CODEMAP.md` is the routing map, not a second specification.
3. Moyva-owned gameplay configuration/definitions/presets are JSON under `Assets/Moyva/Presets/`.
4. Do not introduce new project-owned ScriptableObject configuration as an alternative source of truth.
5. Historical plans, generated audits, inventory dumps, backups, and archived patch output are not architecture authority.

## Before changing code

Read only the smallest relevant slice:

1. `CODEMAP.md`.
2. The target feature's `API/`.
3. Its installer/composition root.
4. The concrete runtime implementation being changed.
5. Focused tests for that feature.

Do not recursively read the whole repository unless the task genuinely requires it.

### Default context exclusions

Unless the task explicitly concerns them, do not read:

- `**/Development/**`
- `**/Editor/**`
- `**/Tests/**` before the production path is identified
- historical migration reports or archived documentation
- generated site output (`.docs-site/`) and docs site templates unless working on publishing
- backup/audit artifacts

Search production `API/`, composition, and `Runtime/` first. Expand to excluded paths only when a reference or failing test requires it.

## Architecture rules

- SOLID, DRY, KISS, YAGNI.
- Composition over inheritance.
- Prefer plain C# domain/services; keep MonoBehaviours thin.
- One authoritative state-mutation path per gameplay concept.
- Canonical unit recruitment lives in `Features/Units` via `IUnitRecruitmentService` / `UnitRecruitmentService`; do not reintroduce a parallel Recruitment feature.
- UI, multiplayer adapters, and editor tools must delegate to canonical gameplay services.
- Feature modules own their bindings/composition. Scene/bootstrap installers may delegate, not duplicate the graph.
- Tests stay outside production `Runtime/` folders.
- Editor/analyzer code never becomes runtime decision authority.
- Avoid hidden static global state and hidden side effects.
- Avoid generic `Manager`, `Helper`, `Utils` when a concrete responsibility can be named.
- Do not create an interface for a class unless it is an actual boundary, substitutable dependency, or test seam.
- Do not split code into tiny files merely to satisfy SRP; cohesion matters more than file count.

Naming semantics:

- `Installer` = DI bindings/composition.
- `Coordinator` / `Orchestrator` = sequencing.
- `Policy` = decision rule.
- `Resolver` = input → result.
- `Store` = owned in-memory state.
- `Registry` = keyed lookup.
- `Adapter` = boundary translation.
- `Service` = cohesive domain/application capability.

## Unity safety

Never delete, rename, or merge serialized Unity types from C# reference count alone.

Before removing a serialized type or asset, check:

- C# references;
- scene/prefab/asset GUID references;
- Zenject bindings;
- reflection/string lookup;
- asmdef references;
- save/version compatibility;
- EditMode/PlayMode tests;
- supported runtime smoke paths.

Preserve `.meta` files when moving Unity assets.

## JSON configuration policy

Moyva-owned gameplay configuration, definitions, registries, presets, balance data,
system settings, and generator graphs are authored in JSON under `Assets/Moyva/Presets/`.

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

Prefer modifying existing canonical files over adding another wrapper, facade, plan, or documentation layer.

If two files explain the same architecture, consolidate them.
If generated information can be derived from source, generate it on demand instead of tracking a snapshot.
