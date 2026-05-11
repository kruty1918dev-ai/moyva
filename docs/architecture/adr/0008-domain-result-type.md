# 0008 Domain Result Type

- Status: Accepted
- Date: 2026-05-11

## Context

У кількох runtime-flow очікувані доменні фейли (наприклад, невірний пароль або недоступна кімната) оброблялися через exception-flow.
Це ускладнювало читання control flow і змішувало expected та unexpected помилки.

## Decision

Уніфікувати expected-failure через `Result`-тип:

1. Доменні операції повертають `Result` / `Result<T>`.
2. Причина фейлу кодується через `DomainErrorCode` + повідомлення.
3. Exception не використовуються для expected-фейлів.

Початкове застосування: HomeMenu join flow (join + transport stage).

## Consequences

Positive:
- Чистіший control flow без try/catch у бізнес-гілках.
- Явний контракт помилок для UI-реакцій.
- Менше ризику приховати реальний unexpected-failure під domain catch.

Trade-offs:
- Додаткові Result-перевірки на call-site.
- Поступова міграція існуючих API, які ще кидають exception.

## Rollback / Alternative

Rollback:
- Повернути exception-flow для expected-доменних фейлів.

Alternative:
- Ввести richer Result з типізованими failure-reason per bounded context.

## Links

- `Assets/Moyva/Scripts/Shared/Common/Result.cs`
- `Assets/Moyva/Scripts/Shared/Common/DomainError.cs`
- `Assets/Moyva/Scripts/Shared/Common/DomainErrorCode.cs`
- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/Services/JoinRoomPanelService.cs`
- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/Services/JoinRoomTransportAdapter.cs`
- `docs/standarts/domain-result-pattern.md`