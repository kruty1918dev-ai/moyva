# AI Bot (`BotAI`)

<- [Back to system docs](../../index.html)

## Overview

`BotAI` is a turn-scoped orchestration layer for Bot factions. It does not run from a wall-clock scheduler and it does not mutate gameplay state directly.

Runtime bot decisions flow through the authoritative turn loop:

```text
TurnService
  -> TurnBotDriver
  -> IBotTurnExecutor.TryBeginTurn(ownerId, globalTurn)
  -> Bot planning/session code
  -> canonical gameplay services
```

The bot may propose construction, recruitment, deployment, movement, combat, scouting, and hold actions, but final mutation must go through the same domain services used by player gameplay.

## Runtime Components

| Component | Role |
|---|---|
| `TurnBotDriver` | Thin turn bridge. Starts one bot execution for the active bot owner/global turn, then retries turn end on later ticks. |
| `IBotTurnExecutor` | Stable external BotAI turn boundary. |
| `BotTurnExecutor` | Authoritative executor shell with owner, phase, epoch, and idempotency validation. |
| `BotTickScheduler` | Legacy inert compatibility shim. It is intentionally not bound as `IInitializable` or `ITickable`. |
| `BotBrain` | Legacy compatibility FSM only. It is not the production runtime brain and direct unit spawn is disabled. |
| `IBotDifficultySettings` | Difficulty and planning-quality parameters. Timer fields remain only for legacy compile compatibility. |
| `BotFogInitializer` | Registers fog services for Bot factions. |
| `BotInstaller` | Zenject installer for BotAI services. |

## Invariants

- Bot actions happen only during the active bot faction's `TurnPhase.AwaitingInput`.
- `TryBeginTurn` is idempotent per `(ownerId, GlobalTurn)`.
- `TurnBotDriver` owns turn ending; planners and executors do not call `ITurnService.TryEndTurn`.
- `BotTickScheduler` remains unbound and inert.
- Normal AI construction uses `IConstructionService.TryDirectPlace`.
- Normal AI recruitment uses `IUnitRecruitmentService` queue/deployment APIs.
- Normal AI movement uses `IUnitMovementService`.
- BotAI must not call `IUnitFactory` for normal recruitment.
- BotAI must not edit resource dictionaries, health, unit registries, object maps, or transforms directly.
- Fog-visible enemy observations must be used for normal difficulty behavior; hidden enemies are not legal planning input.

## Difficulty

Difficulty controls planning quality, not how many legal turns/actions the bot receives.

| Setting | Purpose |
|---|---|
| `MaxDecisionIterations` | Safety loop bound for one bot turn. |
| `MaxSuccessfulMutations` | Maximum committed mutations for one bot turn. |
| `MaxFailedMutations` | Failure budget before stopping the session. |
| `DeterministicNoiseMagnitude` | Bounded deterministic score noise. Easy may use more, Hard may use none. |
| `MinUtilityToAct` | Minimum useful candidate score. |

`TickInterval`, `AttackThreshold`, and `DefendThreshold` are legacy compatibility values for old `BotBrain` tests/docs only.

## Legacy Notes

Older documentation described this runtime:

```text
BotInstaller -> BotTickScheduler -> BotBrain.Tick() -> IUnitFactory/IUnitMovementService
```

That path is obsolete. `BotTickScheduler` is a no-op compatibility shim, and `BotBrain` is marked obsolete. Production bot behavior must remain turn-scoped through `IBotTurnExecutor`.
