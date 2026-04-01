# Construction — Система будівництва

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/construction)

---

## Призначення

Система **Construction** реалізує повний цикл розміщення будівель на ігровій карті:

- вибір будівлі з меню (за категорією: `Military`, `Civilian`, `Industrial`);
- preview розміщення з візуальним зворотним зв'язком (`Valid` / `Blocked`);
- підтвердження (Confirm) або скасування (Cancel) усієї черги;
- Undo/Redo для крокового повернення;
- будування стін через 8 точок-ручок або drag із алгоритмом Bresenham.

Система **не керує** юнітами, камерою чи UI — вона надає чистий API, а підписники обробляють сигнали самостійно.

---

## Архітектура

```
Construction/
├── API/
│   ├── IConstructionService.cs        ← головний контракт
│   ├── IWallPlacementService.cs       ← контракт розміщення стін
│   ├── IConstructionInputService.cs   ← заглушка: Ctrl+Z/Y та кнопки
│   ├── IScreenToGridConverter.cs      ← конвертація координат (world → grid)
│   ├── BuildingCategory.cs            ← enum: Military, Civilian, Industrial
│   ├── BuildingPlacementState.cs      ← enum: Idle, Placing, Confirmed
│   ├── BuildingPreviewState.cs        ← enum: None, Valid, Blocked
│   └── BuildingDefinition.cs          ← DTO: id, назва, префаб, категорія
└── Runtime/
    ├── ConstructionService.cs         ← логіка pending-черги, Undo/Redo
    ├── WallPlacementService.cs        ← Bresenham + 8 ручок для стін
    ├── ConstructionInputService.cs    ← stub: Ctrl+Z/Y / кнопки відміни
    ├── ScreenToGridConverter.cs       ← Camera.ScreenToWorldPoint → grid
    ├── BuildingRegistrySO.cs          ← ScriptableObject: каталог будівель
    └── ConstructionInstaller.cs       ← Zenject інсталер
```

---

## Основний потік будівництва

```
[Гравець відкрив меню будівництва]
    │
    ▼
GameModeService.SetMode(Construction)
    → GameModeChangedSignal { NewMode = Construction }
    → TileInteractionService вимикається
    → ConstructionService активується

[Гравець вибрав будівлю "barracks"]
    │
    ▼
IConstructionService.SelectBuilding("barracks")
    → _state = Placing

[Гравець натиснув на тайл (3, 4)]
    │
    ▼
IConstructionService.TryPreviewAt(new Vector2Int(3, 4))
    │
    ├─ [тайл вільний]  → _pendingPlacements.Push(...)
    │                     Fire(BuildingPreviewChangedSignal { PreviewState = Valid })
    └─ [тайл зайнятий] → Fire(BuildingPreviewChangedSignal { PreviewState = Blocked })

[Гравець натиснув "✓ Підтвердити"]
    │
    ▼
IConstructionService.Confirm()
    → ObjectsMapService.Register() для кожного pending
    → Fire(BuildingPlacedSignal) для кожного
    → _pendingPlacements.Clear()
    → GameModeService.SetMode(Normal)
```

---

## Сигнали

| Сигнал | Тип | Надсилає | Отримує |
|---|---|---|---|
| `BuildingPlacedSignal` | `struct` | `ConstructionService.Confirm()` | ObjectsMap, Spawner |
| `BuildingCancelledSignal` | `struct` | `ConstructionService.Cancel()` | UI |
| `BuildingPreviewChangedSignal` | `struct` | `ConstructionService.TryPreviewAt()` | `TileView` |
| `ShowWallHandlesSignal` | `struct` | `WallPlacementService.ShowWallHandles()` | UI стін |

---

## Підсистеми

| Документ | Опис |
|---|---|
| [construction-service.md](construction-service.md) | Детальний опис `IConstructionService` та алгоритмів |
| [construction-registry.md](construction-registry.md) | `BuildingRegistrySO` та `BuildingDefinition` |
| [wall-placement.md](wall-placement.md) | `IWallPlacementService`, Bresenham, 8 ручок |
| [screen-to-grid.md](screen-to-grid.md) | `IScreenToGridConverter`, конвертація координат |

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IObjectsMapService`](objects-map.md) | Перевірка `IsOccupied`, підтвердження `Register()` |
| [`IGameModeService`](game-mode.md) | Активація / деактивація через `GameModeChangedSignal` |
| [`SignalBus`](signals.md) | Надсилання будівельних сигналів |

---

## Пов'язані системи

- [GameMode](game-mode.md) — перемикає режим будівництва
- [ObjectsMap](objects-map.md) — реєстрація підтверджених будівель
- [Signals](signals.md) — будівельні сигнали
- [Visuals](visuals.md) — `TileView` реагує на `BuildingPreviewChangedSignal`
- [Interactions](interactions.md) — вимикається під час будівництва
