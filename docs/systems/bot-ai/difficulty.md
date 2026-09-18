# Складність бота (`DifficultyLevel`)

## Огляд

Складність задається через `IBotDifficultySettings` / `BotDifficultySettings` і planning profile, які реєструє `BotRuntimeBindings`.

Difficulty впливає на **якість та межі планування**, а не запускає окремий scheduler і не дає боту додаткових ходів.

## Актуальні принципи

- `Easy`, `Normal`, `Hard` — детерміновані presets.
- Turn cadence визначає `TurnService`, а не AI timer.
- Production BotAI не використовує wall-clock `BotTickScheduler`.
- Поля на кшталт старих timer/threshold settings, якщо ще збережені для source compatibility, не є runtime scheduling authority.
- Для зміни default preset редагуйте `BotRuntimeBindings.BindDifficulty`, а не scene installer.

## Composition

```text
BootstrapInstaller
  -> BotRuntimeBindings.Install(container)
  -> IBotDifficultySettings
  -> BotPlanningProfile
  -> planners / BotTurnExecutor
```

Не створюйте другий BotAI installer або timer loop для окремої складності.
