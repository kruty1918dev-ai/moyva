# Multiplayer Config Schema + Migration Pipeline

Мета: зробити оновлення клієнтів безболісними, коли формат multiplayer-конфігів змінюється між версіями.

## Правило

Кожен persisted multiplayer config має:
1. `SchemaVersion` у payload.
2. Явний migration pipeline `vN -> vN+1` до `CurrentSchemaVersion`.
3. Єдину freeze-форму в runtime (після migration + validate).

## Практичний контракт

1. `BinaryConfigStore.ReadConfig(...)` читає сирий payload відповідно до schema.
2. `MultiplayerConfigMigrationPipeline.MigrateToLatest(...)` піднімає конфіг до останньої версії.
3. `MultiplayerConfigLifecycle.ValidateAndFreeze(...)` нормалізує поля і гарантує immutable runtime snapshot.
4. Runtime сервіси працюють лише з поточною schema-формою.

## Гарантії сумісності

- Старі файли (`v1`, `v2`, `v3`) читаються і мігрують без ручних дій користувача.
- Нові поля отримують безпечні дефолти під час migration.
- Після `Load()` конфіг завжди повертається в `CurrentSchemaVersion`.

## Поточне застосування

- `MultiplayerConfigMigrationPipeline`:
  - `v1 -> v2`
  - `v2 -> v3`
  - `v3 -> v4`
- `BinaryConfigStore.Load()`:
  - `read raw -> migrate -> validate/freeze`
- `MultiplayerConfigLifecycle`:
  - у freeze-результаті завжди фіксує поточну schema.
