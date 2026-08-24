# Service Mode Profiles

Мета: розділити runtime-політики між Menu і Gameplay режимами, щоб уникнути конфліктів UX-пріоритетів.

## Правило

Критичні сервіси мають читати політики з профілю режиму (`Menu` або `Gameplay`), а не тримати hardcoded значення в бізнес-коді.

Профіль режиму визначає:
1. Політику логування (`Quiet/Standard/Verbose`).
2. Таймаути і polling-інтервали для runtime-flow.
3. Quality intent (бажаний graphics profile для режиму).

## Практичний контракт

1. Доступ до політик має йти через єдиний DI-сервіс `IServiceModeProfileProvider`.
2. Menu-сервіси використовують профіль `Menu`.
3. Gameplay startup/сервіси використовують профіль `Gameplay`.
4. Для graphics policy дозволено поважати `Custom` user profile (не перетирати ручні налаштування).

## Поточне застосування

- `HomeMenuInitializer`:
  - connectivity wait/quick-probe таймаути з Menu профілю;
  - застосування Menu graphics profile з повагою до `Custom`.
- `JoinRoomTransportAdapter`:
  - timeout/interval для пошуку join code з Menu профілю.
- `GameplayStartupPipeline`:
  - застосування Gameplay graphics profile з повагою до `Custom`;
  - менш шумне логування фаз (через gameplay logging policy).
- `SharedInstaller`:
  - централізований bind `IServiceModeProfileProvider`.
