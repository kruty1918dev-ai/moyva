# 0011 Feature Toggles for Risky Features

- Status: Accepted
- Date: 2026-05-11

## Context

Ризикові multiplayer-фічі (Relay path, host migration) вимагали кодових hotfix при регресіях у рантаймі.
Це уповільнювало rollback і збільшувало операційний ризик.

## Decision

Ввести конфігуровані feature toggles для ризикових фіч і застосовувати їх у критичних runtime-flow.

1. Додано toggles у `MultiplayerConfig`:
- `EnableRelayProvider`
- `EnableHostMigration`
2. Забезпечено backward-compatible збереження/читання у `BinaryConfigStore` (schema v4).
3. Додано UI-керування у `MultiplayerConfigEditorWindow`.
4. Runtime застосовує toggles як authoritative policy:
- Relay toggle: fallback без Relay;
- Host migration toggle: завершення сесії замість міграції.

## Consequences

Positive:
- Швидкий rollback ризикових фіч через config.
- Менше потреби у hotfix-патчах для аварійного вимкнення.
- Прозора керованість ризиком у прод-сценаріях.

Trade-offs:
- Додаткова складність конфігурації та перевірок.
- Потрібно підтримувати узгоджені fallback-стратегії.

## Rollback / Alternative

Rollback:
- Прибрати toggles і повернути hardcoded runtime-поведінку.

Alternative:
- Винести toggles у remote config / feature-flag service із тим самим локальним fallback-контрактом.

## Links

- `Assets/Moyva/Scripts/Features/Multiplayer/API/MultiplayerConfig.cs`
- `Assets/Moyva/Scripts/Features/Multiplayer/Runtime/BinaryConfigStore.cs`
- `Assets/Moyva/Scripts/Features/Multiplayer/Runtime/MultiplayerInstaller.cs`
- `Assets/Moyva/Scripts/Features/Multiplayer/Runtime/NetworkProviderFactory.cs`
- `Assets/Moyva/Scripts/Features/Multiplayer/Runtime/SessionManager.cs`
- `Assets/Moyva/Scripts/Features/Multiplayer/Editor/MultiplayerConfigEditorWindow.cs`
- `docs/standarts/feature-toggles-risky-features.md`
