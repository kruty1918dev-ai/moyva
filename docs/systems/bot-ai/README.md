# AI Bot (`BotAI`)

<- [Back to system docs](../../index.html)

## Overview

`BotAI` is turn-scoped orchestration for Bot factions. It does not own gameplay mutation and it has no wall-clock scheduler.

```text
TurnService
  -> TurnBotDriver
  -> IBotTurnExecutor
  -> BotTurnExecutor
  -> planning
  -> canonical gameplay services
```

## Runtime authority

| Concern | Authority |
|---|---|
| turn bridge / end-turn retry | `TurnBotDriver` |
| external BotAI turn boundary | `IBotTurnExecutor` |
| bot turn session orchestration | `BotTurnExecutor` |
| DI composition | `BotRuntimeBindings` |
| construction mutation | `IConstructionService` |
| recruitment/deployment mutation | `IUnitRecruitmentService` |
| movement mutation | `IUnitMovementService` |

`BotBrain` remains legacy compatibility code only; it is not the production decision loop.

## Invariants

- Bot actions run only for the active bot owner in `TurnPhase.AwaitingInput`.
- One bot execution is claimed per `(ownerId, GlobalTurn)`.
- `TurnBotDriver` owns turn ending.
- There is no `BotTickScheduler` runtime path.
- BotAI plans and queries; canonical gameplay services mutate.
- Hidden enemy state is not legal normal planning input.
- Difficulty changes planning quality, never turn ownership or legal action count.
