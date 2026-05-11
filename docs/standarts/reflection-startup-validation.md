# Reflection Startup Validation

Мета: прибрати приховані reflection-path у критичних runtime-місцях і зменшити ризик late-failure під час гри.

## Правило

Для критичних runtime-flow reflection дозволено лише за умов:
1. Reflection metadata кешується централізовано.
2. Валідація reflection-binding виконується на старті підсистеми.
3. При невалідному reflection-path система переходить у явний fallback, а не продовжує роботу до випадкового runtime-crash.

## Що вважати критичним місцем

- мережеві transport/lobby шляхи;
- bootstrap-логіка, що визначає провайдери або режими запуску;
- код, який може вплинути на входження в сесію або синхронізацію клієнтів.

## Практичний контракт

1. Reflection-резолвер має бути один на bounded context (наприклад, Relay).
2. Доступ до `Type.GetType`, `GetMethod`, `GetProperty` з бізнес-коду заборонений — тільки через кешований резолвер.
3. Startup installer викликає `TryValidate...` і логує причину невалідності.
4. У разі невалідності обирається безпечний fallback provider.

## Поточне застосування

- `RelayReflectionCache` (кеш + валідація метаданих)
- `RelayNetworkProvider` (використання лише через кеш)
- `MultiplayerInstaller` (startup validation + fallback)
- `NetworkProviderFactory` (guard від невалідного relay reflection)