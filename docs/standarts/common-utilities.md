# Common Utilities (Lightweight Packages)

Мета: централізувати базові утиліти (time, id, retry, validation), щоб уникати дублювання і різночитань по модулях.

## Розміщення

- Пакет: `Assets/Moyva/Scripts/Shared/Common/`
- Namespace: `Kruty1918.Moyva.Shared.Common`

## Поточні утиліти

- `MoyvaClock` — єдине джерело `UtcNow`.
- `MoyvaId` — генерація `Guid` та коротких trace-id.
- `RetryHelper` — polling/retry для async сценаріїв.
- `Guard` — базова валідація вхідних параметрів.

## Правила використання

- Не дублювати локальні helper-методи для `Guid/NewId`, `UtcNow`, retry-loop, null-check guard.
- Для нових runtime модулів використовувати утиліти з `Shared.Common` за замовчуванням.
- Якщо додається новий utility — він має бути маленьким, без зовнішніх побічних ефектів і з очевидним API.

## Приклади інтеграції

- `JoinRoomPanelService`: генерація trace-id через `MoyvaId.NewTraceId()`.
- `JoinRoomTransportAdapter`: polling через `RetryHelper.PollUntilAsync(...)`.
- Startup-класи: constructor guard через `Guard.NotNull(...)`.
