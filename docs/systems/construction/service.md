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

        /// <summary>Чи активний режим знесення.</summary>
        bool IsDemolishMode { get; }

        /// <summary>
        /// Вибрати будівлю для розміщення.
        /// Перемикає State → Placing.
        /// </summary>
        void SelectBuilding(string buildingId);

        /// <summary>
        /// Спробувати розмістити preview будівлі на тайлі.
        /// Перевіряє зайнятість, мінімальну відстань (spacing), туман війни (fog).
        /// Надсилає BuildingPreviewChangedSignal з актуальним BuildingPreviewState.
        /// Повертає true якщо Preview = Valid, false якщо Blocked.
        /// </summary>
        bool TryPreviewAt(Vector2Int position);

        /// <summary>
        /// Підтвердити всі pending-розміщення.
        /// Реєструє кожне в ObjectsMapService, надсилає BuildingPlacedSignal.
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

        /// <summary>Перемикає режим знесення (тільки будівлі, поставлені гравцем).</summary>
        void ToggleDemolishMode();

        /// <summary>Спробувати знести будівлю на позиції. Працює тільки для гравцевих будівель.</summary>
        bool TryDemolishAt(Vector2Int position);

        /// <summary>Отримати всі підтверджені будівлі, поставлені гравцем.</summary>
        IReadOnlyDictionary<Vector2Int, string> GetPlayerPlacedBuildings();

        /// <summary>Відновити будівлю зі збереження (без валідації).</summary>
        void RestoreFromSave(Vector2Int position, string buildingId);
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
   └─ Так:  Fire(BuildingPreviewChangedSignal { Position, BuildingId, PreviewState = Blocked })
             return false
3. IsBlockedBySpacing(position)?                     ← Chebyshev-відстань
   └─ Так:  Fire(BuildingPreviewChangedSignal { PreviewState = Blocked })
             return false
4. IsBlockedByFog(position)?                         ← Туман війни
   └─ Так:  Fire(BuildingPreviewChangedSignal { PreviewState = Blocked })
             return false
5. _pendingPlacements.Push((position, _selectedBuildingId))
   _pendingPositions.Add(position)
   _redoStack.Clear()    ← нова дія інвалідує redo
   Fire(BuildingPreviewChangedSignal { Position, BuildingId, PreviewState = Valid })
   return true
```

### `IsBlockedBySpacing(position)`

Chebyshev-дистанція (квадратна область):

```
Якщо _minSpacing <= 0 → return false
Для dx = -minSpacing..+minSpacing, dy = -minSpacing..+minSpacing:
    Якщо (dx, dy) == (0, 0) → skip
    neighbor = position + (dx, dy)
    Якщо IsOccupied(neighbor) АБО _pendingPositions.Contains(neighbor) → return true
return false
```

Параметр `minSpacing` інжектується через `[Inject(Id = "minSpacing")]`.

### `IsBlockedByFog(position)`

```
Якщо _fogOfWarService == null → return false    (FogOfWar не підключений)
fogState = _fogOfWarService.GetFogState(position)
return fogState == FogStateType.Unexplored       (Explored/Visible — дозволено)
```

### `Confirm()`

```
1. Для кожного (pos, id) у _pendingPlacements:
   a. ObjectsMapService.Register(pos, id)     ← реєстрація в карті
   b. _playerPlacedBuildings.Add(pos, id)     ← трекінг гравцевих будівель
   c. Fire(BuildingPlacedSignal { BuildingId = id, Position = pos })
2. _pendingPlacements.Clear()
3. _pendingPositions.Clear()
4. _redoStack.Clear()
5. State = Idle
```

### `Cancel()`

```
1. Для кожного (pos, _) у _pendingPlacements:
   a. Fire(BuildingPreviewChangedSignal { Position = pos, PreviewState = None })
2. _pendingPlacements.Clear()
3. _pendingPositions.Clear()
4. _redoStack.Clear()
5. Fire(BuildingCancelledSignal)
6. State = Idle
```

### `UndoLast()`

```
1. Якщо _pendingPlacements порожній → return
2. Pop (pos, id) з _pendingPlacements
3. _pendingPositions.Remove(pos)
4. _redoStack.Push((pos, id))
5. Fire(BuildingPreviewChangedSignal { Position = pos, PreviewState = None })
```

### `RedoLast()`

```
1. Якщо _redoStack порожній → return
2. Pop (pos, id) з _redoStack
3. Re-call TryPreviewAt(pos)   ← перевіряє зайнятість/spacing/fog знову
```

### `TryDemolishAt(position)`

```
1. Якщо !IsDemolishMode → return false
2. Якщо !_playerPlacedBuildings.ContainsKey(position) → return false
3. buildingId = _playerPlacedBuildings[position]
4. _playerPlacedBuildings.Remove(position)
5. ObjectsMapService.Unregister(position)
6. Fire(BuildingDemolishedSignal { BuildingId, Position })
7. return true
```

### `OnGameModeChanged(signal)`

```
1. _isActive = signal.NewMode == Construction
2. Якщо !_isActive && State != Idle → Cancel()
3. Якщо !_isActive → IsDemolishMode = false
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
    public string BuildingId;
    public BuildingPreviewState PreviewState;
}
```

### `BuildingDemolishedSignal`

```csharp
public struct BuildingDemolishedSignal
{
    public string BuildingId;
    public Vector2Int Position;
}
```

---

## Збереження (ConstructionSaveModule)

`ConstructionSaveModule` реалізує `ISaveModule` і серіалізує гравцеві будівлі:

**Формат:**
- `int32` — кількість записів
- Для кожного:
  - `int32` — X позиції
  - `int32` — Y позиції
  - `string` — buildingId (UTF-8)

При завантаженні (`OnLoad`) кожна будівля відновлюється через `IConstructionService.RestoreFromSave()`.

---

## ConstructionVisualService

Відповідає за створення та управління візуальними об'єктами будівель:

- **Ghost preview** — напівпрозорий спрайт (α = 0.55) будівлі при `TryPreviewAt`:
  - Зелений (`Valid`) — тайл вільний
  - Червоний (`Blocked`) — тайл зайнятий / порушення spacing / fog
- **Blocked flash** — червоний спалах (0.35с) при спробі поставити на заблокований тайл
- **Solid placement** — при `Confirm` ghost замінюється на непрозорий об'єкт
- **Demolish cleanup** — при знесенні об'єкт видаляється

Сервіс реалізує `ITickable` для управління таймерами flash-ефектів.

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IObjectsMapService`](../objects-map.md) | Перевірка `IsOccupied`, `Register()` / `Unregister()` |
| [`SignalBus`](../signals.md) | Надсилання будівельних сигналів |
| [`IFogOfWarService`](../fog-of-war/README.md) | Перевірка видимості тайлу (`[InjectOptional]`) |
| `IBuildingRegistry` | Пошук `BuildingDefinition` за ID (для ConstructionVisualService) |
| `minSpacing` (int) | Мінімальна відстань між будівлями (`[Inject(Id = "minSpacing")]`) |

---

## Пов'язані системи

- [Construction (огляд)](../construction.md)
- [registry.md](registry.md)
- [wall-placement.md](wall-placement.md)
- [screen-to-grid.md](screen-to-grid.md)
- [ui.md](ui.md) — Construction UI Controller
- [Signals](../signals.md)
- [ObjectsMap](../objects-map.md)
- [FogOfWar](../fog-of-war/README.md)
- [SaveSystem](../save-system.md)
