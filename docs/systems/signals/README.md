# Signals — Система сигналів (SignalBus)

← [Назад до README](../../README.md)

---

## Призначення

Система **Signals** забезпечує слабке зв'язування між підсистемами гри через Zenject `SignalBus`. Замість прямих посилань між сервісами, системи надсилають і приймають типізовані сигнали — повідомлення-події.

---

## Як працює внутрішньо

1. `SignalBusInstaller` встановлює Zenject SignalBus і **декларує** кожен сигнал у контейнері.
2. Будь-який сервіс може **підписатися** (`Subscribe<T>`) або **надіслати** (`Fire<T>`) сигнал.
3. Підписка / відписка керується вручну (зазвичай у `IInitializable.Initialize` / `IDisposable.Dispose`).

---

## Реєстрація сигналів

`SignalBusInstaller` декларує всі сигнали проєкту при запуску SceneContext.
Деякі сигнали позначені `.OptionalSubscriber()` — відсутність підписника не викликає помилку.

```csharp
public class SignalBusInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Zenject.SignalBusInstaller.Install(Container);

        // Ядро
        Container.DeclareSignal<TileClickedSignal>();
        Container.DeclareSignal<UnitCreatedSignal>();
        Container.DeclareSignal<UnitMovedSignal>();
        Container.DeclareSignal<UnitDestroyedSignal>();
        Container.DeclareSignal<InterruptMovementSignal>();
        Container.DeclareSignal<OnMapObjectSpawnedSignal>();
        Container.DeclareSignal<OnObjectsMapChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<WorldBuiltSignal>();
        Container.DeclareSignal<WorldGeneratedDataSignal>().OptionalSubscriber();

        // GameMode
        Container.DeclareSignal<GameModeChangedSignal>();
        Container.DeclareSignal<GameModeChangeRequestedSignal>();

        // Construction
        Container.DeclareSignal<BuildingPlacedSignal>();
        Container.DeclareSignal<BuildingCancelledSignal>();
        Container.DeclareSignal<BuildingPreviewChangedSignal>();
        Container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();
        Container.DeclareSignal<ShowWallHandlesSignal>();

        // FogOfWar
        Container.DeclareSignal<FogStateChangedSignal>();

        // SaveSystem
        Container.DeclareSignal<SaveRequestedSignal>();
        Container.DeclareSignal<LoadRequestedSignal>();
        Container.DeclareSignal<SaveCompletedSignal>().OptionalSubscriber();
    }
}
```

---

## Усі сигнали проекту (20)

Кожен сигнал має окрему сторінку з повним описом: поля, хто надсилає, хто отримує, реєстрація, пов'язані сигнали.

### Ядро: взаємодія та карта (9)

| Сигнал | Опис | Деталі |
|---|---|---|
| `TileClickedSignal` | Клік по тайлу на карті | [→ tile-clicked](tile-clicked.md) |
| `UnitCreatedSignal` | Фабрика створила юніта | [→ unit-created](unit-created.md) |
| `UnitMovedSignal` | Юніт перемістився на нову позицію | [→ unit-moved](unit-moved.md) |
| `UnitDestroyedSignal` | Юніт знищений (зарезервовано) | [→ unit-destroyed](unit-destroyed.md) |
| `InterruptMovementSignal` | Переривання руху юніта (стаміна) | [→ interrupt-movement](interrupt-movement.md) |
| `OnMapObjectSpawnedSignal` | Спавн статичного об'єкта карти | [→ map-object-spawned](map-object-spawned.md) |
| `OnObjectsMapChangedSignal` | Зміна карти об'єктів | [→ objects-map-changed](objects-map-changed.md) |
| `WorldBuiltSignal` | Завершення побудови світу | [→ world-built](world-built.md) |
| `WorldGeneratedDataSignal` | Дані генерації карти | [→ world-generated-data](world-generated-data.md) |

### GameMode: режими гри (2)

| Сигнал | Опис | Деталі |
|---|---|---|
| `GameModeChangedSignal` | Зміна ігрового режиму | [→ game-mode-changed](game-mode-changed.md) |
| `GameModeChangeRequestedSignal` | Запит на зміну режиму | [→ game-mode-change-requested](game-mode-change-requested.md) |

### Construction: будівництво (5)

| Сигнал | Опис | Деталі |
|---|---|---|
| `BuildingPlacedSignal` | Підтвердження розміщення будівлі | [→ building-placed](building-placed.md) |
| `BuildingCancelledSignal` | Скасування сесії будівництва | [→ building-cancelled](building-cancelled.md) |
| `BuildingPreviewChangedSignal` | Зміна стану preview будівлі | [→ building-preview-changed](building-preview-changed.md) |
| `BuildingDemolishedSignal` | Успішне знесення будівлі | [→ building-demolished](building-demolished.md) |
| `ShowWallHandlesSignal` | Показ/приховування ручок стін | [→ show-wall-handles](show-wall-handles.md) |

### FogOfWar: туман війни (1)

| Сигнал | Опис | Деталі |
|---|---|---|
| `FogStateChangedSignal` | Зміна стану видимості тумана | [→ fog-state-changed](fog-state-changed.md) |

### SaveSystem: збереження (3)

| Сигнал | Опис | Деталі |
|---|---|---|
| `SaveRequestedSignal` | Запит на збереження у слот | [→ save-requested](save-requested.md) |
| `LoadRequestedSignal` | Запит на завантаження зі слоту | [→ load-requested](load-requested.md) |
| `SaveCompletedSignal` | Результат операції збереження | [→ save-completed](save-completed.md) |

---

## Приклади використання

### Надіслати сигнал

```csharp
_signalBus.Fire(new TileClickedSignal { Position = new Vector2Int(3, 4) });
```

### Підписатися на сигнал (у `Initialize`)

```csharp
public void Initialize()
{
    _signalBus.Subscribe<TileClickedSignal>(OnTileClicked);
    _signalBus.Subscribe<InterruptMovementSignal>(OnInterruptRequested);
}
```

### Відписатися (у `Dispose`)

```csharp
public void Dispose()
{
    _signalBus.Unsubscribe<TileClickedSignal>(OnTileClicked);
    _signalBus.Unsubscribe<InterruptMovementSignal>(OnInterruptRequested);
}
```

### Реальний ланцюг сигналів: клік → рух → списання стаміни

```
TileView.OnMouseDown()
  → Fire(TileClickedSignal { Position })

TileInteractionService.OnTileClicked()
  → MoveUnitAsync(...)

MovementAnimationService.OnStepCompleted()
  → Fire(UnitMovedSignal { UnitId, NewPosition, Cost })

UnitService.OnUnitMoved()
  → _stamina -= signal.Cost
  → якщо стаміна 0: Fire(InterruptMovementSignal { UnitId })

UnitMovementService.OnInterruptRequested()
  → cts.Cancel()
```

---

## Зведена таблиця сигналів

| Сигнал | Тип | Надсилає | Отримує |
|---|---|---|---|
| `TileClickedSignal` | `class` | `TileView` | `TileInteractionService` |
| `UnitCreatedSignal` | `struct` | `UnitFactory` | `UnitService`, `FogOfWarService` |
| `UnitMovedSignal` | `struct` | `UnitMovementService` | `UnitService`, `FogOfWarService` |
| `UnitDestroyedSignal` | `struct` | — (зарезервовано) | `UnitService`, `FogOfWarService` |
| `InterruptMovementSignal` | `struct` | `UnitService` | `UnitMovementService` |
| `OnMapObjectSpawnedSignal` | `struct` | `MapVisualInstantiator` | `ObjectsMapService` |
| `OnObjectsMapChangedSignal` | `struct` | `ObjectsMapService` | `TileView` |
| `WorldBuiltSignal` | `struct` | `MapVisualInstantiator` | FogOfWar, Bootstrap |
| `WorldGeneratedDataSignal` | `struct` | Генератор карти | Підписники карти |
| `GameModeChangedSignal` | `struct` | `GameModeService` | `TileInteractionService`, `ConstructionService` |
| `GameModeChangeRequestedSignal` | `struct` | UI-контролери | `GameModeChangeRequestRouter` |
| `BuildingPlacedSignal` | `struct` | `ConstructionService` | `ConstructionVisualService`, UI |
| `BuildingCancelledSignal` | `struct` | `ConstructionService` | `ConstructionVisualService`, UI |
| `BuildingPreviewChangedSignal` | `struct` | `ConstructionService` | `ConstructionVisualService`, UI |
| `BuildingDemolishedSignal` | `struct` | `ConstructionService` | `ConstructionVisualService` |
| `ShowWallHandlesSignal` | `struct` | `WallPlacementService` | UI стін |
| `FogStateChangedSignal` | `struct` | `FogOfWarService` | Підписники тумана |
| `SaveRequestedSignal` | `struct` | UI / hotkeys | `SaveService` |
| `LoadRequestedSignal` | `struct` | UI / hotkeys | `SaveService` |
| `SaveCompletedSignal` | `struct` | `SaveService` | UI-підписники |

---

## Пов'язані системи

- [Grid](../grid.md) — зберігання стану тайлів
- [Units](../units.md) — надсилає / отримує більшість сигналів
- [Interactions](../interactions.md) — отримує `TileClickedSignal`, `GameModeChangedSignal`
- [Visuals](../visuals.md) — отримує `OnObjectsMapChangedSignal`, `BuildingPreviewChangedSignal`; надсилає `TileClickedSignal`
- [ObjectsMap](../objects-map.md) — надсилає `OnObjectsMapChangedSignal`, отримує юніт-сигнали та `OnMapObjectSpawnedSignal`
- [Generator](../generator.md) — надсилає `OnMapObjectSpawnedSignal`
- [GameMode](../game-mode.md) — надсилає `GameModeChangedSignal`
- [Construction](../construction.md) — надсилає `BuildingPlacedSignal`, `BuildingCancelledSignal`, `BuildingPreviewChangedSignal`, `ShowWallHandlesSignal`
- [SaveSystem](../save-system/README.md) — `SaveRequestedSignal`, `LoadRequestedSignal`, `SaveCompletedSignal`
