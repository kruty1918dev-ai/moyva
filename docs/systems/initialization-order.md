# Порядок ініціалізації ядра (Zenject)

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/initialization-order)

---

## Чому порядок важливий?

У Zenject усі `MonoInstaller`-и сцени виконуються **у порядку їхнього розміщення в списку Scene Context**. Кожен `InstallBindings()` лише **реєструє** залежності — справжня побудова об'єктів починається після того, як **усі** інсталятори завершили роботу.

`IInitializable.Initialize()` викликається Zenject-ом за допомогою внутрішнього планувальника після повної побудови контейнера. Якщо порядок `Initialize()` не задати явно через `BindExecutionOrder`, Zenject не гарантує, хто першим отримає виклик.

**Критичний сценарій у Moyva:**

1. `TestUnitSpawner.Initialize()` створює юнітів через `IUnitFactory`, що стріляє `UnitCreatedSignal`.
2. `ObjectsMapService` підписується на `UnitCreatedSignal` у власному `Initialize()`.
3. Якщо `ObjectsMapService.Initialize()` ще **не виконався** до моменту пострілу сигналу — юніти виявляться не зареєстрованими на карті.

---

## Поточна послідовність

### Крок 1 — `SignalBusInstaller` (найперший)

**Причина:** SignalBus повинен існувати до будь-якого `DeclareSignal` і `Subscribe`. Якщо інший інсталятор спробує підписатись на сигнал до його оголошення — виключення під час розбудови контейнера.

```csharp
// SignalBusInstaller.cs
public override void InstallBindings()
{
    Zenject.SignalBusInstaller.Install(Container); // глобальний шину
    Container.DeclareSignal<UnitCreatedSignal>();
    Container.DeclareSignal<UnitMovedSignal>();
    Container.DeclareSignal<UnitDestroyedSignal>();
    Container.DeclareSignal<OnMapObjectSpawnedSignal>();
    Container.DeclareSignal<OnObjectsMapChangedSignal>();
    Container.DeclareSignal<TileClickedSignal>();
    Container.DeclareSignal<InterruptMovementSignal>();
}
```

---

### Крок 2 — Базова інфраструктура (Grid, Pathfinding, Animations)

Ці системи **не підписуються** на сигнали при ініціалізації і не потребують жодних зовнішніх залежностей крім налаштувань (ScriptableObject).

| Інсталятор | Що реєструє | Особливості |
|---|---|---|
| `GridInstaller` | `IGridService`, `ITileSettingsService` | Залежить від `TileRegistrySO` |
| `PathfinderInstaller` | `IPathfinder` | Чиста логіка, без залежностей |
| `AnimationsInstaller` | `IMovementAnimationService` | Чиста логіка, без залежностей |

---

### Крок 3 — Бізнес-логіка (Units, Camera, Interactions, Visuals, Generator)

Ці системи залежать від Grid і можуть стріляти/слухати сигнали, але їхні `Initialize()` **не мають критичних порядкових вимог** між собою.

| Інсталятор | Що реєструє |
|---|---|
| `UnitsInstaller` | `UnitService`, `IUnitFactory`, `UnitMovementService`, `IUnitClassConfig` |
| `CameraInstaller` | `CameraMovement`, `CameraZoom`, `CameraFocused`, `CameraPlayerController` |
| `InteractionsInstaller` | `TileInteractionService` |
| `VisualInstaller` | (порожній, TileView реєструється через MonoBehaviour) |
| `GeneratorInstaller` | `IMapDataGenerator`, `WFCService`, генератори рельєфу тощо |

---

### Крок 4 — `ObjectsMapInstaller` (з явним `ExecutionOrder`)

`ObjectsMapService` підписується на `UnitCreatedSignal`, `UnitMovedSignal`, `UnitDestroyedSignal`, `OnMapObjectSpawnedSignal`. Він **обов'язково** має завершити `Initialize()` до того, як `TestUnitSpawner` почне стріляти `UnitCreatedSignal`.

```csharp
// ObjectsMapInstaller.cs
public override void InstallBindings()
{
    Container.BindInterfacesAndSelfTo<ObjectsMapService>()
        .AsSingle()
        .NonLazy();                          // гарантує виклик Initialize() без явного Resolve

    Container.BindExecutionOrder<ObjectsMapService>(-10); // -10 = ініціалізується РАНІШЕ за за замовчуванням (0)
}
```

---

### Крок 5 — `BootstrapInstaller` (останній)

`TestUnitSpawner` є точкою входу, що запускає ланцюжок подій. Він повинен виконатись **після** `ObjectsMapService`.

```csharp
// BootstrapInstaller.cs
public override void InstallBindings()
{
    Container.BindInterfacesTo<TestUnitSpawner>()
        .AsSingle()
        .NonLazy();

    Container.BindExecutionOrder<TestUnitSpawner>(100); // 100 = ініціалізується ПІСЛЯ за замовчуванням (0)
}
```

---

## Діаграма послідовності

```
SceneContext (Zenject)
│
├── InstallBindings() усіх MonoInstaller-ів (порядок зі Scene Context)
│   ├── SignalBusInstaller   → DeclareSignal × 7
│   ├── GridInstaller        → Bind GridService, TileSettingsService
│   ├── PathfinderInstaller  → Bind Pathfinder
│   ├── AnimationsInstaller  → Bind MovementAnimationService
│   ├── UnitsInstaller       → Bind UnitService, UnitFactory, UnitMovementService
│   ├── ObjectsMapInstaller  → Bind ObjectsMapService  [ExecutionOrder = -10]
│   └── BootstrapInstaller   → Bind TestUnitSpawner    [ExecutionOrder = 100]
│
└── Initialize() у порядку ExecutionOrder (менше = раніше)
    ├── ObjectsMapService.Initialize()  [-10] → Subscribe(UnitCreatedSignal, ...)
    ├── (інші IInitializable за замовчуванням) [0]
    └── TestUnitSpawner.Initialize()    [100] → IUnitFactory.CreateUnit(...)
                                                 → Fire(UnitCreatedSignal)
                                                 → ObjectsMapService реєструє юніта ✓
```

---

## Як додати новий вузол ініціалізації ядра

### Сценарій: новий сервіс `GameStateService`, що залежить від `ObjectsMapService` і повинен ініціалізуватись до `TestUnitSpawner`, але після `ObjectsMapService`

**Крок 1.** Створіть сервіс та реалізуйте `IInitializable`:

```csharp
// Features/GameState/Runtime/GameStateService.cs
using Zenject;

namespace Kruty1918.Moyva.GameState.Runtime
{
    public class GameStateService : IInitializable
    {
        private readonly IObjectsMapService _objectsMap;

        public GameStateService(IObjectsMapService objectsMap)
        {
            _objectsMap = objectsMap;
        }

        public void Initialize()
        {
            // Логіка, що потребує готового ObjectsMapService
        }
    }
}
```

**Крок 2.** Створіть інсталятор із відповідним `ExecutionOrder`:

```csharp
// Features/GameState/Runtime/GameStateInstaller.cs
using Zenject;

namespace Kruty1918.Moyva.GameState.Runtime
{
    public class GameStateInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameStateService>()
                .AsSingle()
                .NonLazy();

            // Між ObjectsMapService (-10) і TestUnitSpawner (100)
            Container.BindExecutionOrder<GameStateService>(50);
        }
    }
}
```

**Крок 3.** Додайте `GameStateInstaller` до **Scene Context → Mono Installers** в Unity Inspector — після `ObjectsMapInstaller`, але до `BootstrapInstaller` (для наочності, хоча `ExecutionOrder` є головним захистом).

**Крок 4.** Оголосіть нові сигнали (якщо потрібно) у `SignalBusInstaller`:

```csharp
Container.DeclareSignal<GameStateChangedSignal>();
```

**Крок 5.** Оновіть цей документ — додайте новий рядок до таблиці в Кроці 3 або Кроці 4, залежно від рівня залежностей нового сервісу.

---

## Довідка по `BindExecutionOrder`

| Значення | Момент виклику `Initialize()` |
|---|---|
| `-10` | Раніше за `0` (наприклад, `ObjectsMapService`) |
| `0` | За замовчуванням (більшість сервісів) |
| `50` | Пізніше за `0`, раніше за `100` |
| `100` | Останнім (наприклад, `TestUnitSpawner`) |

> Значення — це **відносний пріоритет**, а не абсолютна позиція. Менше значення = раніше.

---

## Пов'язані системи

- [Bootstrap](bootstrap.md) — `BootstrapInstaller`, `TestUnitSpawner`
- [Signals](signals.md) — усі сигнали, що стріляються при ініціалізації
- [ObjectsMap](objects-map.md) — підписка на сигнали юнітів
- [Units](units.md) — `UnitService`, `IUnitFactory`
