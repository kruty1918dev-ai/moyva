# ConstructionService — Сервіс будівництва

← [Назад до Construction](../construction.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/construction-service)

---

## Призначення

`ConstructionService` реалізує `IConstructionService` — головний сервіс системи будівництва. Він зберігає чергу `pending`-розміщень, виконує перевірку зайнятості тайлів, підтримує Undo/Redo стек і координує весь цикл від вибору будівлі до підтвердження.

Реалізація `ConstructionService` має рівень доступу `internal` у runtime-збірці.
Для тестової збірки `Kruty1918.Moyva.Tests.Construction` доступ відкривається через
`InternalsVisibleTo` в `Runtime/AssemblyInfo.cs`, щоб зберегти інкапсуляцію для інших модулів.

---

## Публічний API

### `IConstructionService`

```csharp
namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionService
    {
        /// <summary>Поточний стан сесії будівництва.</summary>
        BuildingPlacementState State { get; }

        /// <summary>
        /// Вибрати будівлю для розміщення.
        /// Перемикає State → Placing.
        /// </summary>
        void SelectBuilding(string buildingId);

        /// <summary>
        /// Спробувати розмістити preview будівлі на тайлі.
        /// Надсилає BuildingPreviewChangedSignal з актуальним BuildingPreviewState.
        /// Повертає true якщо Preview = Valid, false якщо Blocked.
        /// </summary>
        bool TryPreviewAt(Vector2Int position);

        /// <summary>
        /// Підтвердити всі pending-розміщення.
        /// Реєструє кожне в ObjectsMapService, надсилає BuildingPlacedSignal.
        /// Після Confirm дія незворотна — системи руйнування будівель поза межами цього модуля.
        /// </summary>
        void Confirm();

        /// <summary>
        /// Скасувати всю сесію будівництва.
        /// Видаляє всі pending, очищує Redo-стек, надсилає BuildingCancelledSignal.
        /// </summary>
        void Cancel();

        /// <summary>Відмінити останнє розміщення (Ctrl+Z / кнопка Undo).</summary>
        void UndoLast();

        /// <summary>Повернути скасоване розміщення (Ctrl+Y / кнопка Redo).</summary>
        void RedoLast();
    }
}
```

---

## Enum-типи

### `BuildingPlacementState`

```csharp
namespace Kruty1918.Moyva.Construction.API
{
    public enum BuildingPlacementState
    {
        Idle,       // Будівля не вибрана
        Placing,    // Гравець розставляє будівлі
        Confirmed   // Сесія підтверджена (State скидається до Idle після Confirm)
    }
}
```

### `BuildingPreviewState`

```csharp
namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Стан preview-відображення на конкретному тайлі.
    /// Використовується замість двох булевих полів (уникає неконсистентних комбінацій).
    /// </summary>
    public enum BuildingPreviewState
    {
        None,    // Підсвітка знята (preview видалено або сесія завершена)
        Valid,   // Тайл вільний — будівля буде розміщена, показати нормальний спрайт
        Blocked  // Тайл зайнятий — будівля не може бути розміщена, підсвітити червоним
    }
}
```

---

## Алгоритми

### `TryPreviewAt(position)`

```
1. Якщо State != Placing → return false
2. IObjectsMapService.IsOccupied(position)?
   ├─ Так:  Fire(BuildingPreviewChangedSignal { Position, PreviewState = Blocked })
   │         return false
   └─ Ні:   _pendingPlacements.Push((position, _selectedBuildingId))
             _redoStack.Clear()    ← нова дія інвалідує redo
             Fire(BuildingPreviewChangedSignal { Position, PreviewState = Valid })
             return true
```

### `Confirm()`

```
1. Для кожного (pos, id) у _pendingPlacements:
   a. ObjectsMapService.Register(pos, id)     ← реєстрація в карті
   b. Fire(BuildingPlacedSignal { BuildingId = id, Position = pos })
2. _pendingPlacements.Clear()
3. _redoStack.Clear()
4. State = Idle
   ← після Confirm будівлі "забуті": подальший трекінг поза межами цього модуля
```

### `Cancel()`

```
1. Для кожного (pos, _) у _pendingPlacements:
   a. Fire(BuildingPreviewChangedSignal { Position = pos, PreviewState = None })
2. _pendingPlacements.Clear()
3. _redoStack.Clear()
4. Fire(BuildingCancelledSignal)
5. State = Idle
```

### `UndoLast()`

```
1. Якщо _pendingPlacements порожній → return
2. Pop (pos, id) з _pendingPlacements
3. _redoStack.Push((pos, id))
4. Fire(BuildingPreviewChangedSignal { Position = pos, PreviewState = None })
```

### `RedoLast()`

```
1. Якщо _redoStack порожній → return
2. Pop (pos, id) з _redoStack
3. Re-call TryPreviewAt(pos)   ← перевіряє зайнятість знову
```

---

## Сигнали

### `BuildingPlacedSignal`

```csharp
public struct BuildingPlacedSignal
{
    public string BuildingId;
    public Vector2Int Position;
}
```

### `BuildingCancelledSignal`

```csharp
public struct BuildingCancelledSignal { }
```

### `BuildingPreviewChangedSignal`

```csharp
public struct BuildingPreviewChangedSignal
{
    public Vector2Int Position;
    public BuildingPreviewState PreviewState;
    // Замість двох булів: None / Valid / Blocked — без неконсистентних комбінацій
}
```

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IObjectsMapService`](../objects-map.md) | Перевірка `IsOccupied`, `Register()` при Confirm |
| [`IGameModeService`](../game-mode.md) | Підписка на `GameModeChangedSignal` для активації |
| [`SignalBus`](../signals.md) | Надсилання будівельних сигналів |
| `BuildingRegistrySO` | Пошук `BuildingDefinition` за ID |

---

## Пов'язані системи

- [Construction (огляд)](../construction.md)
- [registry.md](registry.md)
- [wall-placement.md](wall-placement.md)
- [screen-to-grid.md](screen-to-grid.md)
- [Signals](../signals.md)
- [ObjectsMap](../objects-map.md)
