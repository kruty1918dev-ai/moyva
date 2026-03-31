# Units — Система юнітів

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/docs/#systems/units)

---

## Призначення

Система **Units** відповідає за весь життєвий цикл ігрових юнітів: створення (фабрика), реєстрацію в реєстрі, відстеження позиції й витривалості (стаміни), а також оркестрування асинхронного руху по карті.

---

## Підсистеми

| Клас / Інтерфейс | Роль |
|---|---|
| `IUnitFactory` / `UnitFactory` | Спавн нового юніта на карті |
| `IUnitService` / `UnitService` | Реєстр стану всіх юнітів (стаміна, позиція, GameObject) |
| `IUnitMovementService` / `UnitMovementService` | Координація руху: патфайндинг → анімація → оновлення стану |
| `IUnitClassConfig` / `UnitClassConfigService` | Читання конфігів класів юнітів із `UnitRegistrySO` |
| `UnitClassConfig` (SO entry) | Дані класу: префаб, базова стаміна, діапазон рандому |
| `UnitRegistrySO` | ScriptableObject-реєстр усіх класів юнітів |

---

## Як працює внутрішньо

### Створення юніта (`UnitFactory`)

1. Знаходить конфіг класу за `typeId` через `IUnitClassConfig`.
2. Спавнить префаб через `DiContainer.InstantiatePrefab` (щоб інжекції в сам юніт теж працювали).
3. Генерує унікальний ID: `warrior_01_123456`.
4. Надсилає сигнал `UnitCreatedSignal` — його підхопить `UnitService`.

### Реєстрація стану (`UnitService`)

- Підписується на `UnitCreatedSignal`, `UnitMovedSignal`, `UnitDestroyedSignal`.
- Зберігає стаміну, позицію, тип і `GameObject` у внутрішніх словниках.
- Після `UnitMovedSignal` перевіряє стаміну: якщо не вистачає — надсилає `InterruptMovementSignal`.

### Рух юніта (`UnitMovementService`)

1. Отримує поточну позицію юніта через `IUnitService.TryGetUnitPosition`.
2. Будує шлях через `IPathfinder.FindPath` (A\*).
3. Передає шлях в `IMovementAnimationService.MoveAlongPathAsync` (плавний `Lerp`).
4. На кожному кроці анімація викликає `OnStepCompleted` → надсилається `UnitMovedSignal` → `UnitService` списує стаміну.
5. Слухає `InterruptMovementSignal` для зупинки руху будь-якого юніта.

---

## Публічний API

### `IUnitFactory`

```csharp
public interface IUnitFactory
{
    // Створює юніта заданого класу на зазначеній позиції сітки.
    // Повертає унікальний ID (наприклад, "warrior_01_123456")
    string CreateUnit(string typeId, Vector2Int gridPosition);
}
```

### `IUnitService`

```csharp
public interface IUnitService
{
    // Поточна стаміна юніта (0 якщо не знайдено)
    float GetStamina(string unitId);

    // Отримати поточну позицію юніта на сітці
    bool TryGetUnitPosition(string unitId, out Vector2Int position);

    // Отримати посилання на GameObject юніта (для анімацій)
    GameObject GetUnitObject(string unitId);
}
```

### `IUnitMovementService`

```csharp
public interface IUnitMovementService
{
    // Асинхронно переміщує юніта до targetPosition (через патфайндинг + анімацію)
    Task MoveUnitAsync(string unitId, Vector2Int targetPosition, CancellationToken token = default);
}
```

### `IUnitClassConfig`

```csharp
public interface IUnitClassConfig
{
    // Повертає UnitClassConfig (SO-дані) за typeId; null якщо не знайдено
    UnitClassConfig GetConfig(string typeId);
}
```

### `UnitClassConfig` (ScriptableObject-елемент)

| Поле | Тип | Опис |
|---|---|---|
| `TypeId` | `string` | Ідентифікатор класу (`"warrior"`) |
| `Prefab` | `GameObject` | Ігровий префаб юніта |
| `BaseStamina` | `float` | Базова стаміна при старті |
| `StaminaRandomRange` | `Vector2` | Діапазон рандомного модифікатора стаміни |
| `AnimationSettings` | `PathAnimationSettings` | Налаштування анімації руху |

---

## Вхід / Вихід

| Метод | Вхід | Вихід |
|---|---|---|
| `CreateUnit` | `string typeId, Vector2Int pos` | `string unitId` |
| `GetStamina` | `string unitId` | `float` |
| `TryGetUnitPosition` | `string unitId` | `bool` + `out Vector2Int` |
| `GetUnitObject` | `string unitId` | `GameObject` (або `null`) |
| `MoveUnitAsync` | `string unitId, Vector2Int target, CancellationToken` | `Task` |

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IGridService`](grid.md) | Зайняти / звільнити тайли |
| [`ITileSettingsService`](grid.md) | Вага тайлу для перевірки стаміни |
| [`IPathfinder`](pathfinding.md) | Пошук шляху перед рухом |
| [`IMovementAnimationService`](animations.md) | Плавна анімація кроку |
| [`SignalBus`](signals.md) | `UnitCreatedSignal`, `UnitMovedSignal`, `InterruptMovementSignal` |
| `UnitRegistrySO` | Конфіги класів юнітів |

---

## Реєстрація в Zenject (`UnitsInstaller`)

```csharp
public class UnitsInstaller : MonoInstaller
{
    [SerializeField] private UnitRegistrySO _unitRegistry;

    public override void InstallBindings()
    {
        Container.BindInstance(_unitRegistry).AsSingle();

        Container.Bind<IUnitClassConfig>()
            .To<UnitClassConfigService>()
            .AsSingle();

        Container.BindInterfacesAndSelfTo<UnitService>()
            .AsSingle();

        Container.Bind<IUnitFactory>()
            .To<UnitFactory>()
            .AsSingle();

        Container.BindInterfacesAndSelfTo<UnitMovementService>()
            .AsSingle();
    }
}
```

---

## Приклади використання

### Створити юніта (Bootstrap)

```csharp
// TestUnitSpawner.cs
public void Initialize()
{
    _unitFactory.CreateUnit("warrior", new Vector2Int(5, 5));
    _unitFactory.CreateUnit("warrior", new Vector2Int(7, 3));
    _unitFactory.CreateUnit("warrior", new Vector2Int(2, 8));
}
```

### Перемістити юніта (Interactions)

```csharp
// TileInteractionService.cs
await _unitMovementService.MoveUnitAsync(unitToMove, position, _moveCts.Token);
```

### Перевірити стаміну

```csharp
float stamina = _unitService.GetStamina("warrior_01_123456");
if (stamina <= 0) Debug.Log("Юніт не може рухатись — немає стаміни");
```

### Отримати позицію юніта

```csharp
if (_unitService.TryGetUnitPosition("warrior_01_123456", out var pos))
    Debug.Log($"Юніт на позиції {pos}");
```

---

## Пов'язані системи

- [Grid](grid.md) — зберігає окупацію тайлів
- [Pathfinding](pathfinding.md) — будує маршрут
- [Animations](animations.md) — виконує плавний рух
- [Interactions](interactions.md) — ініціює команду руху
- [Signals](signals.md) — `UnitCreatedSignal`, `UnitMovedSignal`, `UnitDestroyedSignal`, `InterruptMovementSignal`
- [Bootstrap](bootstrap.md) — тестовий спавн юнітів при старті
