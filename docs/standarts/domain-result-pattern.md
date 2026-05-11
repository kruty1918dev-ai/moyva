# Domain Result Pattern

Мета: очікувані доменні фейли мають керувати flow через `Result`, а не через винятки.

## Коли використовувати Result

Використовувати `Result` / `Result<T>` для expected-failure сценаріїв:
- невірний пароль;
- не знайдено цільовий ресурс (кімната/сесія);
- валідаційні помилки вхідних даних;
- контрольовані network-failure, де потрібно повернути причину у UI.

## Коли залишати Exception

Exception лишаються для неочікуваних або інфраструктурних збоїв:
- помилки програмування (null invariant, invalid state);
- критичні runtime failure, які не є частиною доменного контракту;
- помилки сторонніх SDK, які неможливо перевести у доменну причину на цьому шарі.

## Контракт

1. Метод доменного рівня повертає `Result`/`Result<T>`.
2. Для фейлу заповнюється `DomainErrorCode` + коротке повідомлення.
3. Call-site обробляє `IsFailure` без `catch` для expected-flow.
4. `throw` у доменному expected-flow заборонено.

## Базові типи

- `DomainErrorCode`
- `DomainError`
- `Result`
- `Result<T>`

## Поточне застосування

- HomeMenu Join flow:
  - `JoinRoomPanelService` — пароль/аліас/lookup-flow;
  - `JoinRoomTransportAdapter` — transport join phase.