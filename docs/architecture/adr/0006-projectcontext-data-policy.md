# 0006 ProjectContext Data Policy

- Status: Accepted
- Date: 2026-05-11

## Context

Project-level runtime state occasionally leaked between scene transitions (menu/gameplay/menu). This created stale launch/session behavior and hard-to-reproduce bugs.

## Decision

Introduce a unified ProjectContext data policy:

1. Keep only infrastructure services and explicit short-lived cross-scene context in ProjectContext.
2. For launch context (`GameLaunchContext`), enforce TTL (30 minutes) and automatic reset on expiry.
3. Reset transient cross-scene state when entering HomeMenu:
- `GameLaunchContext.Reset()`
- `IGameplaySession.Clear()`

## Consequences

Positive:
- Fewer stale state leaks across scene boundaries.
- Deterministic lifecycle of launch/session context.
- Clear rules for what can live in ProjectContext.

Trade-offs:
- Slightly stricter reset semantics (context is intentionally ephemeral).
- Additional lifecycle code (TTL checks and reset points).

## Rollback / Alternative

Rollback:
- Remove TTL/reset checks and return to purely manual context management.

Alternative:
- Move all cross-scene context into dedicated lifecycle services with explicit scene ownership and no static state.

## Links

- `Assets/Moyva/Scripts/Features/SaveSystem/Runtime/SavePlayModeOptions.cs`
- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/HomeMenuInitializer.cs`
- `docs/standarts/project-context-data-policy.md`
