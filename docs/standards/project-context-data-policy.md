# ProjectContext Data Policy

Мета: мінімізувати витоки стану між сценами і зробити крос-сценові дані передбачуваними.

## Scope

Policy застосовується до:
- ProjectContext singleton-сервісів (Zenject AsSingle у ProjectContext).
- Статичних runtime-контекстів запуску (наприклад, `GameLaunchContext`).
- Даних, що живуть довше однієї сцени.

## Що дозволено в ProjectContext

Дозволяються лише:
1. Інфраструктурні сервіси без сценарно-специфічного стану:
- мережеві провайдери/обгортки,
- save/audio/connectivity,
- адаптери платформи.
2. Короткоживучий launch/session контекст, потрібний для переходу Menu -> Gameplay.

Забороняється:
1. UI-стан панелей, вибрані елементи, тимчасові фільтри.
2. Сцено-локальні кеші візуалізації/редактора.
3. Доменний mutable state без явного lifecycle/reset.

## TTL (Time-To-Live)

Для launch-контексту (`GameLaunchContext`):
- default TTL: 30 хвилин;
- якщо контекст протермінований, він вважається невалідним і підлягає автоматичному reset.

Реалізація:
- `ConfiguredAtUtc`, `ExpiresAtUtc`, `IsExpired`;
- `EnsureNotExpired()` перед читанням policy-критичних даних;
- `RefreshTtl()` для контрольованого продовження життя контексту.

## Reset логіка

Обов'язкові reset-точки:
1. Вхід у HomeMenu:
- `GameLaunchContext.Reset()`;
- `IGameplaySession.Clear()`.
2. TTL-expire:
- автоматичний `GameLaunchContext.Reset()` при `EnsureNotExpired()`.
3. Явний сценарний reset:
- після завершення/скасування крос-сценового сценарію викликати `Reset()`.

## Практичні правила для нових сервісів

1. Якщо сервіс у ProjectContext має mutable state, додай метод `Clear/Reset`.
2. Опиши lifecycle state у коментарі класу: хто ініціює, хто очищає.
3. Для тимчасових даних між сценами використовуй TTL + auto-reset.
4. Не зберігай scene-specific дані у статичних полях без reset-політики.

## Reference Implementation

- `Assets/Moyva/Scripts/Features/SaveSystem/Runtime/SavePlayModeOptions.cs`
- `Assets/Moyva/Scripts/Features/HomeMenu/Runtime/HomeMenuInitializer.cs`
