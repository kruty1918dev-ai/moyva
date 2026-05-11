# 0003 Startup Pipeline Phases Contract

- Status: Accepted
- Date: 2026-05-11
- Decision Makers: Core Team

## Context

Запуск сцени мав приховані ініціалізації та неявний порядок кроків, що ускладнювало діагностику стартових проблем і передбачуваність поведінки.

## Decision

Ввести явний контракт startup-фаз:

1. Preload
2. Bind
3. Warmup
4. SceneActivate

Контракт інкапсульований у `IGameplayStartupPipeline` і виконується централізовано через `GameplayStartupPipeline`.

## Consequences

- Positive:
  - Прогнозований lifecycle запуску.
  - Легше локалізувати проблемну фазу.
- Negative / Trade-offs:
  - Додатковий рівень абстракції.
- Risks:
  - Фази повинні залишатися семантично чистими без змішування обов'язків.

## Rollback / Alternative

Альтернатива: тримати всю логіку в `HomeMenuGameStarter`. Відхилено через знижену читабельність і контроль фаз.

## Links

- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/Startup/IGameplayStartupPipeline.cs`
- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/Startup/GameplayStartupPipeline.cs`
- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/HomeMenuGameStarter.cs`
