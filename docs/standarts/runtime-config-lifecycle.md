# Runtime Config Lifecycle

Мета: зробити runtime-конфіги детермінованими, валідними і незмінними після старту сценарію.

## Контракт

Кожен runtime-конфіг проходить три етапи:
1. Load: зчитування з джерела (store, ScriptableObject, remote payload).
2. Validate: перевірка й нормалізація полів до коректного діапазону.
3. Freeze: побудова immutable snapshot, який і використовується runtime-сервісами.

## Правила

1. Runtime-код не працює напряму з raw-конфігом із джерела.
2. Після етапу Freeze конфіг вважається read-only.
3. Якщо конфіг невалідний, застосовується safe normalization або fallback до `Default()`.
4. Validate не повинен кидати винятки для recoverable-помилок даних; замість цього виконується нормалізація і логування.
5. Всі місця завантаження конфігів у runtime мають використовувати lifecycle-хелпер.

## Reference Implementation

- Multiplayer:
  - `MultiplayerConfigLifecycle.LoadValidateFreeze(...)`
  - `MultiplayerConfigLifecycle.ValidateAndFreeze(...)`
- Calendar:
  - `CalendarConfigLifecycle.LoadValidateFreeze(...)`
  - `CalendarConfigLifecycle.ValidateAndFreeze(...)`

## Rollout

Для нових модулів:
1. Ввести `*ConfigLifecycle` у API/Runtime шарі.
2. Підключити lifecycle в installer/store (а не тільки в editor tooling).
3. Передавати в сервіси лише frozen snapshot.