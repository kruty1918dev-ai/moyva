# 0002 HomeMenu Service-UI Boundary

- Status: Accepted
- Date: 2026-05-11
- Decision Makers: Core Team

## Context

Сервіси HomeMenu знали деталі UI (конкретні кнопки/панелі), що ускладнювало рефакторинг і викликало побічні ефекти при зміні інтерфейсу.

## Decision

Розділити бізнес-логіку і UI-деталі:

- сервіс працює через intent-орієнтований контракт (`OnJoinRequested`, `SetJoinInteractable`);
- панельна навігація винесена в окремий UI gateway (`IJoinRoomUiGateway`).

## Consequences

- Positive:
  - Менша зв'язаність між service та view.
  - Зміни UI не ламають бізнес-правила.
- Negative / Trade-offs:
  - Більше абстракцій і інтерфейсів.
- Risks:
  - Потрібна дисципліна: не повертати UI-знання назад у сервіси.

## Rollback / Alternative

Rollback: повернути прямі залежності на `Button/Navigation` у сервісі. Небажано.

## Links

- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/Services/JoinRoomPanelService.cs`
- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/Services/IJoinRoomUiGateway.cs`
- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/Services/JoinRoomUiGateway.cs`
