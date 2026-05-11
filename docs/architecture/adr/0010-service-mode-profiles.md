# 0010 Service Mode Profiles

- Status: Accepted
- Date: 2026-05-11

## Context

Однакові runtime-політики для Menu і Gameplay режимів призводили до змішаних пріоритетів:
- Menu потребує стабільного UX і передбачуваних timeout-поведінок для lobby/join flow.
- Gameplay потребує окремих quality/logging рішень, орієнтованих на ігровий runtime.

Hardcoded timeout/logging/quality параметри в сервісах ускладнювали еволюцію поведінки для кожного режиму.

## Decision

Ввести режимні профілі сервісів (`Menu`, `Gameplay`) з централізованим провайдером у DI.

1. Додано `ServiceModeProfile` і `IServiceModeProfileProvider`.
2. Визначено дефолтні профілі для Menu/GamePlay (timeouts, logging, graphics intent).
3. Критичні HomeMenu/Gameplay точки мігровані на читання політик із профілю.
4. Graphics policy застосовується з guard: якщо активний `Custom` profile, ручні user-налаштування не перетираються.

## Consequences

Positive:
- Менше hardcoded policy-значень у runtime коді.
- Простіше керувати UX-поведінкою окремо для Menu і Gameplay.
- Єдина точка зміни timeout/logging/quality intent.

Trade-offs:
- Додано абстракцію профілів і DI-залежність у кількох сервісах.
- Потрібно підтримувати профілі в актуальному стані при зміні продуктних вимог.

## Rollback / Alternative

Rollback:
- Повернути локальні hardcoded timeout/logging/quality значення в сервіси.

Alternative:
- Винести профілі у ScriptableObject-конфіг і керувати ними через editor tool, з тим самим runtime-контрактом `IServiceModeProfileProvider`.

## Links

- `Assets/Moyva/Scripts/Shared/Common/ServiceModeProfile.cs`
- `Assets/Moyva/Scripts/Shared/SharedInstaller.cs`
- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/HomeMenuInitializer.cs`
- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/Services/JoinRoomTransportAdapter.cs`
- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/Startup/GameplayStartupPipeline.cs`
- `docs/standarts/service-mode-profiles.md`
