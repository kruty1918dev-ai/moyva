# Signals — Система сигналів (SignalBus)

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/signals)

---

## Призначення

Система **Signals** забезпечує слабке зв'язування між підсистемами гри через Zenject `SignalBus`. Замість прямих посилань між сервісами, системи надсилають і приймають типізовані сигнали — повідомлення-події.

---

## Як працює внутрішньо

1. `SignalBusInstaller` встановлює Zenject SignalBus і **декларує** кожен сигнал у контейнері.
2. Будь-який сервіс може **підписатися** (`Subscribe<T>`) або **надіслати** (`Fire<T>`) сигнал.
3. Підписка / відписка керується вручну (зазвичай у `IInitializable.Initialize` / `IDisposable.Dispose`).

---

## Усі сигнали проекту

### `TileClickedSignal`

Надсилається: `TileView.OnMouseDown()`  
Отримується: `TileInteractionService`

```csharp
public class TileClickedSignal
{
    public Vector2Int Position; // Координати тайлу, на який клікнули
}
```

---

### `UnitCreatedSignal`

Надсилається: `UnitFactory.CreateUnit()`  
Отримується: `UnitService`

```csharp
public struct UnitCreatedSignal
{
    public string     UnitId;      // Унікальний ID нового юніта ("warrior_01_123456")
    public string     UnitTypeId;  // Клас юніта ("warrior")
    public Vector2Int Position;    // Стартова позиція на сітці
    public GameObject UnitObject;  // Посилання на spawned GameObject
}
```

---

### `UnitMovedSignal`

Надсилається: `UnitMovementService` (через `OnStepCompleted` анімації)  
Отримується: `UnitService`

```csharp
public struct UnitMovedSignal
{
    public string     UnitId;       // ID юніта, що рухався
    public Vector2Int NewPosition;  // Нова позиція на сітці
    public float      Cost;         // Списана стаміна (вага тайлу)
}
```

---

### `UnitDestroyedSignal`

Надсилається: (зарезервовано для системи смерті)  
Отримується: `UnitService`

```csharp
public struct UnitDestroyedSignal
{
    public string UnitId; // ID знищеного юніта
}
```

---

### `InterruptMovementSignal`

Надсилається: `UnitService` (коли стаміна вичерпана)  
Отримується: `UnitMovementService`

```csharp
public struct InterruptMovementSignal
{
    public string UnitId; // ID юніта, рух якого треба перервати
}
```

---

### `OnMapObjectSpawnedSignal`

Надсилається: `MapVisualInstantiator` (після спавну статичного об'єкта карти)  
Отримується: `ObjectsMapService`

```csharp
public struct OnMapObjectSpawnedSignal
{
    public string ObjectId;        // TileTypeId, наприклад "river", "mountain"
    public Vector2Int Position;
}
```

---

### `OnObjectsMapChangedSignal`

Надсилається: `ObjectsMapService` (після будь-якої зміни карти об'єктів)  
Отримується: `TileView` та інші підписники

```csharp
public struct OnObjectsMapChangedSignal
{
    public Vector2Int Position;
    public string OccupantId;      // null якщо тайл звільнено
}
```

---

### `GameModeChangedSignal`

Надсилається: `GameModeService.SetMode()`
Отримується: `TileInteractionService`, `ConstructionService`

```csharp
public struct GameModeChangedSignal
{
    public GameModeType NewMode; // Normal або Construction
}
```

---

### `BuildingPlacedSignal`

Надсилається: `ConstructionService.Confirm()`
Отримується: підписники (спавнер об'єктів, UI)

```csharp
public struct BuildingPlacedSignal
{
    public string BuildingId;
    public Vector2Int Position;
}
```

---

### `BuildingCancelledSignal`

Надсилається: `ConstructionService.Cancel()`
Отримується: UI (закриває сесію будівництва)

```csharp
public struct BuildingCancelledSignal { }
```

---

### `BuildingPreviewChangedSignal`

Надсилається: `ConstructionService.TryPreviewAt()`
Отримується: `TileView` (змінює стан відображення тайлу)

```csharp
public struct BuildingPreviewChangedSignal
{
    public Vector2Int Position;
    public BuildingPreviewState PreviewState;
    // None = підсвітку знято
    // Valid = тайл вільний, preview активний
    // Blocked = тайл зайнятий, підсвічується червоним
}
```

---

### `ShowWallHandlesSignal`

Надсилається: `WallPlacementService.ShowWallHandles()` / `EndDrag()`
Отримується: UI-компонент ручок стін

```csharp
public struct ShowWallHandlesSignal
{
    public Vector2Int Center;
    public bool Hide; // true — приховати ручки
}
```

---

## Реєстрація в Zenject (`SignalBusInstaller`)

```csharp
public class SignalBusInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Zenject.SignalBusInstaller.Install(Container); // Базова установка Zenject SignalBus

        Container.DeclareSignal<TileClickedSignal>();
        Container.DeclareSignal<UnitCreatedSignal>();
        Container.DeclareSignal<UnitMovedSignal>();
        Container.DeclareSignal<UnitDestroyedSignal>();
        Container.DeclareSignal<InterruptMovementSignal>();
        Container.DeclareSignal<OnMapObjectSpawnedSignal>();
        Container.DeclareSignal<OnObjectsMapChangedSignal>();

        // GameMode
        Container.DeclareSignal<GameModeChangedSignal>();

        // Construction
        Container.DeclareSignal<BuildingPlacedSignal>();
        Container.DeclareSignal<BuildingCancelledSignal>();
        Container.DeclareSignal<BuildingPreviewChangedSignal>();
        Container.DeclareSignal<ShowWallHandlesSignal>();
    }
}
```

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

## Таблиця сигналів

| Сигнал | Тип | Надсилає | Отримує |
|---|---|---|---|
| `TileClickedSignal` | `class` | `TileView` | `TileInteractionService` |
| `UnitCreatedSignal` | `struct` | `UnitFactory` | `UnitService`, `ObjectsMapService` |
| `UnitMovedSignal` | `struct` | `UnitMovementService` | `UnitService`, `ObjectsMapService` |
| `UnitDestroyedSignal` | `struct` | — (зарезервовано) | `UnitService`, `ObjectsMapService` |
| `InterruptMovementSignal` | `struct` | `UnitService` | `UnitMovementService` |
| `OnMapObjectSpawnedSignal` | `struct` | `MapVisualInstantiator` | `ObjectsMapService` |
| `OnObjectsMapChangedSignal` | `struct` | `ObjectsMapService` | `TileView` |
| `GameModeChangedSignal` | `struct` | `GameModeService` | `TileInteractionService`, `ConstructionService` |
| `BuildingPlacedSignal` | `struct` | `ConstructionService` | підписники (спавнер) |
| `BuildingCancelledSignal` | `struct` | `ConstructionService` | UI |
| `BuildingPreviewChangedSignal` | `struct` | `ConstructionService` | `TileView` |
| `ShowWallHandlesSignal` | `struct` | `WallPlacementService` | UI стін |

---

## Пов'язані системи

- [Grid](grid.md) — зберігання стану тайлів
- [Units](units.md) — надсилає / отримує більшість сигналів
- [Interactions](interactions.md) — отримує `TileClickedSignal`, `GameModeChangedSignal`
- [Visuals](visuals.md) — отримує `OnObjectsMapChangedSignal`, `BuildingPreviewChangedSignal`; надсилає `TileClickedSignal`
- [ObjectsMap](objects-map.md) — надсилає `OnObjectsMapChangedSignal`, отримує юніт-сигнали та `OnMapObjectSpawnedSignal`
- [Generator](generator.md) — надсилає `OnMapObjectSpawnedSignal`
- [GameMode](game-mode.md) — надсилає `GameModeChangedSignal`
- [Construction](construction.md) — надсилає `BuildingPlacedSignal`, `BuildingCancelledSignal`, `BuildingPreviewChangedSignal`, `ShowWallHandlesSignal`
