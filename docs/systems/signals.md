# Signals — Система сигналів (SignalBus)

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/docs/#systems/signals)

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

### `OnTileChanged`

Надсилається: `GridService.SetTileData()`  
Отримується: `TileView`

```csharp
public struct OnTileChanged
{
    public Vector2Int Position; // Позиція зміненого тайлу
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

### `OnGenerationCompleteSignal`

Надсилається: (зарезервовано для генератора карти)  
Отримується: (підписники UI, камера тощо)

```csharp
public struct OnGenerationCompleteSignal
{
    public string[,] BiomeMap;  // Матриця ID біомів
    public string[,] ObjectMap; // Матриця ID об'єктів (річки тощо)
    public float[,]  HeightMap; // Матриця висот [0.0 - 1.0]
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

## Реєстрація в Zenject (`SignalBusInstaller`)

```csharp
public class SignalBusInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Zenject.SignalBusInstaller.Install(Container); // Базова установка Zenject SignalBus

        Container.DeclareSignal<TileClickedSignal>();
        Container.DeclareSignal<OnTileChanged>();
        Container.DeclareSignal<UnitCreatedSignal>();
        Container.DeclareSignal<UnitMovedSignal>();
        Container.DeclareSignal<UnitDestroyedSignal>();
        Container.DeclareSignal<InterruptMovementSignal>();
        Container.DeclareSignal<OnGenerationCompleteSignal>();
        Container.DeclareSignal<OnMapObjectSpawnedSignal>();
        Container.DeclareSignal<OnObjectsMapChangedSignal>();
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
| `OnTileChanged` | `struct` | `GridService` | `TileView` |
| `UnitCreatedSignal` | `struct` | `UnitFactory` | `UnitService`, `ObjectsMapService` |
| `UnitMovedSignal` | `struct` | `UnitMovementService` | `UnitService`, `ObjectsMapService` |
| `UnitDestroyedSignal` | `struct` | — (зарезервовано) | `UnitService`, `ObjectsMapService` |
| `InterruptMovementSignal` | `struct` | `UnitService` | `UnitMovementService` |
| `OnGenerationCompleteSignal` | `struct` | — (зарезервовано) | — |
| `OnMapObjectSpawnedSignal` | `struct` | `MapVisualInstantiator` | `ObjectsMapService` |
| `OnObjectsMapChangedSignal` | `struct` | `ObjectsMapService` | `TileView` |

---

## Пов'язані системи

- [Grid](grid.md) — надсилає `OnTileChanged`
- [Units](units.md) — надсилає / отримує більшість сигналів
- [Interactions](interactions.md) — отримує `TileClickedSignal`
- [Visuals](visuals.md) — отримує `OnObjectsMapChangedSignal`, надсилає `TileClickedSignal`
- [ObjectsMap](objects-map.md) — надсилає `OnObjectsMapChangedSignal`, отримує юніт-сигнали та `OnMapObjectSpawnedSignal`
- [Generator](generator.md) — надсилає `OnMapObjectSpawnedSignal`, `OnGenerationCompleteSignal` (заплановано)
