# Construction — Система будівництва

## Призначення

Система **Construction** відповідає за повний цикл будівництва в грі:
- **Меню будівництва** з трьома категоріями: Військові, Цивільні, Індустріальні будівлі
- **Режим розміщення**: вибір будівлі, попередній перегляд на тайлі (зелений / червоний)
- **Pending-будівлі**: розміщені, але ще не підтверджені
- **Підтвердження / скасування** всіх pending-будівель
- **Undo / Redo**: Ctrl+Z та Ctrl+Y у режимі будівництва
- **Стіни**: спеціальний механізм з 8 точками з'єднання та прокладанням шляху дотиком

---

## Структура файлів

```
Features/Construction/
├── API/
│   ├── BuildingCategory.cs          ← enum: Military, Civilian, Industrial
│   ├── BuildingConfig.cs            ← [Serializable] конфіг будівлі
│   ├── PendingBuilding.cs           ← readonly struct (TypeId, Position, TempId)
│   ├── IBuildingConstructionService.cs ← головний інтерфейс
│   └── IWallConnectionService.cs    ← інтерфейс механіки стін
└── Runtime/
    ├── BuildingRegistrySO.cs        ← ScriptableObject реєстр конфігів
    ├── BuildingConstructionService.cs ← реалізація (ITickable для Ctrl+Z/Y)
    ├── WallConnectionService.cs     ← Bresenham-шлях + 8 точок з'єднання
    └── ConstructionInstaller.cs     ← Zenject MonoInstaller
```

---

## Конфігурація

### `BuildingConfig`

| Поле | Тип | Опис |
|---|---|---|
| `TypeId` | `string` | Унікальний ID (без підкреслень) |
| `Name` | `string` | Відображувана назва |
| `Category` | `BuildingCategory` | Military / Civilian / Industrial |
| `Prefab` | `GameObject` | Префаб будівлі |
| `Size` | `Vector2Int` | Розмір в тайлах (за замовчуванням 1×1) |
| `IsWall` | `bool` | Чи є ця будівля типом "стіна" |

### `BuildingRegistrySO`

ScriptableObject-реєстр. Створити через **Assets › Create › Moyva/Construction/BuildingRegistry**.  
Призначити у `ConstructionInstaller` через Inspector.

---

## Публічний API

### `IBuildingConstructionService`

```csharp
bool IsInConstructionMode { get; }
string SelectedBuildingTypeId { get; }
IReadOnlyList<PendingBuilding> PendingBuildings { get; }

void StartPlacement(string buildingTypeId);   // Відкрити режим для типу
void PreviewAt(Vector2Int position);           // Оновити попередній перегляд
void PlaceAt(Vector2Int position);             // Розмістити (pending)
void CancelAll();                              // Скасувати всі pending
void ConfirmAll();                             // Підтвердити всі pending
void Undo();                                   // Ctrl+Z
void Redo();                                   // Ctrl+Y
```

### `IWallConnectionService`

```csharp
void ShowConnectionPoints(Vector2Int wallPosition); // Показати 8 кіл навколо стіни
void HideConnectionPoints();                         // Приховати
void PlaceWallPath(Vector2Int from, Vector2Int to);  // Провести шлях стін
```

---

## Сигнали

| Сигнал | Поля | Коли надсилається |
|---|---|---|
| `BuildingModeStartedSignal` | `TypeId` | Після `StartPlacement()` |
| `BuildingPreviewMovedSignal` | `Position`, `TypeId`, `IsBlocked` | Після кожного `PreviewAt()` |
| `BuildingPlacedSignal` | `TempId`, `TypeId`, `Position` | При `PlaceAt()` |
| `BuildingUndoneSignal` | `TempId`, `Position` | При `Undo()` |
| `BuildingRedoneSignal` | `TempId`, `Position` | При `Redo()` |
| `BuildingCancelledSignal` | `TempIds[]`, `Positions[]` | При `CancelAll()` |
| `BuildingConfirmedSignal` | `TempIds[]`, `TypeIds[]`, `Positions[]` | При `ConfirmAll()` |
| `WallConnectionPointsShownSignal` | `WallPosition`, `ConnectionPoints[]` | При `ShowConnectionPoints()` |
| `WallConnectionPointsHiddenSignal` | — | При `HideConnectionPoints()` |

---

## Потік даних

```
[Гравець натискає на тип будівлі в меню]
        │
        ▼
IBuildingConstructionService.StartPlacement("barracks")
        │ → Fire(BuildingModeStartedSignal)
        ▼
[Гравець наводить на тайл]
IBuildingConstructionService.PreviewAt(position)
        │ → перевіряє ObjectsMapService.IsOccupied()
        │ → Fire(BuildingPreviewMovedSignal { IsBlocked })
        ▼
[UI відображає зелений/червоний превʼю]

[Гравець натискає на тайл]
IBuildingConstructionService.PlaceAt(position)
        │ → додає до _pendingBuildings
        │ → Fire(BuildingPlacedSignal)
        ▼
[Ctrl+Z]  → Undo() → Fire(BuildingUndoneSignal)
[Ctrl+Y]  → Redo() → Fire(BuildingRedoneSignal)

[Кнопка "Підтвердити"]
IBuildingConstructionService.ConfirmAll()
        │ → ObjectsMapService.Register() для кожної pending-будівлі
        │ → Fire(BuildingConfirmedSignal)
        ▼
ObjectsMapService → Fire(OnObjectsMapChangedSignal) → TileView оновлюється

[Кнопка "Скасувати"]
IBuildingConstructionService.CancelAll()
        │ → очищає _pendingBuildings
        │ → Fire(BuildingCancelledSignal)
```

---

## Механіка стін

### Алгоритм Брезенхема (`WallConnectionService`)

При розміщенні стіни:
1. Сервіс показує **8 точок з'єднання** навколо розміщеної стіни через `ShowConnectionPoints(pos)`.
2. UI відображає кола в 8 сусідніх позиціях.
3. Гравець **натискає або тягне** одне з кіл.
4. При перетягуванні до кінцевої позиції UI викликає `PlaceWallPath(from, to)`.
5. Сервіс прокладає ланцюг стін за алгоритмом Брезенхема між двома точками.

```
(0,0)──(1,1)──(2,2)──(3,3)  ← приклад діагонального шляху
```

---

## Встановлення

1. Додати `ConstructionInstaller` як компонент до кореневого GameObject сцени.
2. Призначити `BuildingRegistry` ScriptableObject у полі Inspector.
3. Зареєструвати `ConstructionInstaller` у списку інсталерів сцени.

---

## Пов'язані системи

- [Grid](grid.md) — перевірка валідності позиції тайлу
- [ObjectsMap](objects-map.md) — реєстрація підтверджених будівель
- [Signals](signals.md) — усі будівельні сигнали
