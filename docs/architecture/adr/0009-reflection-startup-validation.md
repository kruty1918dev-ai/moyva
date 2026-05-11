# 0009 Reflection Startup Validation

- Status: Accepted
- Date: 2026-05-11

## Context

Критичні multiplayer-шляхи використовували reflection без централізованого кешу і без ранньої валідації.
Це створювало ризик прихованих runtime-сюрпризів: помилка спливала лише під час host/join, а не на старті підсистеми.

## Decision

Ввести правило: критичні reflection-path мають бути кешовані і валідовані на старті.

Застосовано для Relay transport:
1. Додано централізований `RelayReflectionCache`.
2. `RelayNetworkProvider` використовує лише кешований reflection-резолвер.
3. `MultiplayerInstaller` виконує startup-валидацію reflection binding і при проблемі переводить систему у fallback provider.
4. `NetworkProviderFactory` додатково захищає від створення relay-провайдера з невалідним reflection metadata.

## Consequences

Positive:
- Менше runtime-сюрпризів у host/join flow.
- Ранні і діагностовані failure на старті.
- Нижчий overhead за рахунок кешу reflection metadata.

Trade-offs:
- Додатковий код підтримки reflection-кешу.
- Потрібно підтримувати валідатор при зміні зовнішніх SDK API.

## Rollback / Alternative

Rollback:
- Повернути локальні `Type.GetType/GetMethod/GetProperty` у runtime-flow без startup validation.

Alternative:
- Повністю прибрати reflection і перейти на compile-time інтеграцію через єдиний SDK API surface.

## Links

- `Assets/Moyva/Scripts/Features/Multiplayer/Runtime/RelayReflectionCache.cs`
- `Assets/Moyva/Scripts/Features/Multiplayer/Runtime/RelayNetworkProvider.cs`
- `Assets/Moyva/Scripts/Features/Multiplayer/Runtime/MultiplayerInstaller.cs`
- `Assets/Moyva/Scripts/Features/Multiplayer/Runtime/NetworkProviderFactory.cs`
- `docs/standarts/reflection-startup-validation.md`