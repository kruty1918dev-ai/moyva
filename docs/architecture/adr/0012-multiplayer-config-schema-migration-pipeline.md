# 0012 Multiplayer Config Schema + Migration Pipeline

- Status: Accepted
- Date: 2026-05-11

## Context

Multiplayer config format еволюціонує (нові поля, toggles, правила). Без явної migration-політики старі клієнтські файли ризикують ламати runtime-flow або вимагати ручного reset.

## Decision

Ввести єдиний migration pipeline для multiplayer-конфігів.

1. Persisted payload зберігає `SchemaVersion`.
2. `BinaryConfigStore.Load()` виконує послідовність:
- `ReadConfig` (raw)
- `MigrateToLatest`
- `ValidateAndFreeze`
3. Migration реалізується кроками `vN -> vN+1`.
4. Runtime отримує лише поточний формат (`CurrentSchemaVersion`).

## Consequences

Positive:
- Безболісне читання старих конфігів після оновлення клієнта.
- Менше ризику регресій при додаванні полів.
- Передбачуваний lifecycle даних конфігурації.

Trade-offs:
- Потрібно підтримувати migration-кроки при кожній новій schema.
- Додаткові тести на backward compatibility.

## Rollback / Alternative

Rollback:
- Відключити migration pipeline і читати лише поточну schema (небажано, ламає сумісність).

Alternative:
- Перейти на self-describing формат (JSON/protobuf) з versioned adapters, але зберегти обов'язковий migration stage у runtime.

## Links

- `Assets/Moyva/Scripts/Features/Multiplayer/API/MultiplayerConfigMigrationPipeline.cs`
- `Assets/Moyva/Scripts/Features/Multiplayer/Runtime/BinaryConfigStore.cs`
- `Assets/Moyva/Scripts/Features/Multiplayer/API/MultiplayerConfigLifecycle.cs`
- `docs/standarts/multiplayer-config-schema-migration.md`
