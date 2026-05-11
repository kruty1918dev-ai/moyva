# Domain Events Layer

## Мета

Розділити сигнали за межами відповідальності:
- `UI signals` описують наміри/стан екранів і панелей;
- `Domain events` описують бізнес-події гри (стан світу, економіки, сесії).

Це зменшує зв'язаність між UI і gameplay, спрощує тестування та rollback ризикових інтеграцій.

## Правила

1. Domain event не містить UI-специфіки.
2. Domain event живе в `Kruty1918.Moyva.Signals.DomainEvents`.
3. UI signal живе в `Kruty1918.Moyva.Signals` та не використовується як джерело доменної логіки.
4. Для безпечної міграції дозволено перехідний міст `legacy signal -> domain event`.
5. Нові gameplay-підписники повинні слухати domain events, а не UI/legacy gameplay signals.

## Поточна реалізація

- Додано шар `DomainEvents`:
  - `UnitCreatedDomainEvent`
  - `UnitMovedDomainEvent`
  - `UnitDestroyedDomainEvent`
  - `WorldBuiltDomainEvent`
  - `GameModeChangedDomainEvent`
  - `BuildingPlacedDomainEvent`
  - `BuildingDemolishedDomainEvent`
  - `EconomyTickCompletedDomainEvent`
  - `SettlementCreatedDomainEvent`
  - `SettlementDeactivatedDomainEvent`
  - `SettlementResourceChangedDomainEvent`
  - `ResourceDeficitDomainEvent`
  - `GameStartedDomainEvent`
  - `GameEndedDomainEvent`
  - `GamePausedDomainEvent`

- У `SignalBusInstaller` декларації розділені на:
  - legacy/UI сигнали;
  - domain events.

- Додано `SignalDomainEventBridge`, який дублює ключові legacy gameplay signals у domain events для поступової міграції без breaking changes.

## Міграційна стратегія

1. Новий gameplay код пишемо лише на `DomainEvents`.
2. Існуючі legacy підписники залишаються робочими через міст.
3. Поетапно переводимо старі підписники на `DomainEvents`.
4. Після повної міграції видаляємо міст і зайві legacy gameplay signals.

## Антипатерни

- UI-панель слухає `BuildingPlacedSignal` і напряму керує доменною дією.
- Domain-сервіс слухає `WorldInfoPanelRequestedSignal`.
- Нові gameplay інтеграції будуються на legacy сигналах замість `DomainEvents`.
