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
│   ├── IConstructionService.cs        ← головний контракт (preview, confirm, undo, demolish, save)
│   ├── IWallPlacementService.cs       ← контракт розміщення стін
│   ├── IConstructionInputService.cs   ← заглушка: Ctrl+Z/Y та кнопки
│   ├── IScreenToGridConverter.cs      ← конвертація координат (world → grid)
│   ├── IBuildingRegistry.cs           ← контракт реєстру будівель
│   ├── BuildingCategory.cs            ← enum: Military, Civilian, Industrial
│   ├── BuildingPlacementState.cs      ← enum: Idle, Placing, Confirmed
│   ├── BuildingPreviewState.cs        ← enum: None, Valid, Blocked
│   └── BuildingDefinition.cs          ← DTO: id, назва, префаб, категорія
├── Runtime/
│   ├── AssemblyInfo.cs                ← InternalsVisibleTo для тестів
│   ├── ConstructionService.cs         ← логіка pending-черги, Undo/Redo, spacing, fog
│   ├── ConstructionVisualService.cs   ← ghost preview, blocked flash, placed visuals
│   ├── ConstructionSaveModule.cs      ← ISaveModule: серіалізація гравцевих будівель
│   ├── ConstructionInputService.cs    ← tick-based Ctrl+Z/Y / кнопки відміни
│   ├── WallPlacementService.cs        ← Bresenham + 8 ручок для стін
│   ├── ScreenToGridConverter.cs       ← Camera.ScreenToWorldPoint → grid
│   ├── MapVisualInstantiator.cs       ← спавн об'єктів карти (будівлі на тайлах)
│   ├── BuildingRegistrySO.cs          ← ScriptableObject: каталог будівель
│   └── ConstructionInstaller.cs       ← Zenject інсталер (runtime)
├── Editor/
│   └── ConstructionUISetupWindow.cs   ← Editor-вікно автоматичного створення UI ієрархії
└── UI/
    ├── ConstructionUIController.cs    ← адаптер UI ↔ IConstructionService ↔ GameMode
    ├── BuildingSelectionPanelUI.cs    ← список будівель з фільтром за категорією
    ├── BuildingCategoryTabsUI.cs      ← вкладки категорій будівель
    ├── BuildingButtonUI.cs            ← кнопка окремої будівлі (іконка + виділення)
    ├── ConstructionActionBarUI.cs     ← Confirm / Cancel / Undo / Redo / Знести
    ├── ConstructionStatusUI.cs        ← відображення стану preview/сесії
    ├── ConstructionUIInstaller.cs     ← Zenject інсталер (UI)
    ├── BuildingMenuFactory.cs         ← формування меню з реєстру (enum → MenuItems)
    ├── ConstructionButtonPressAnimator.cs ← DOTween анімації натиску (опціонально)
    ├── BuildingListItemData.cs        ← UI DTO для елементу списку (з Sprite Icon)
    └── ConstructionUIState.cs         ← snapshot поточного UI-стану
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
| `BuildingDemolishedSignal` | `struct` | `ConstructionService.TryDemolishAt()` | Spawner, UI |
| `ShowWallHandlesSignal` | `struct` | `WallPlacementService.ShowWallHandles()` | UI стін |

---

## Підсистеми

| Документ | Опис |
|---|---|
| [construction/service.md](construction/service.md) | Детальний опис `IConstructionService` та алгоритмів |
| [construction/registry.md](construction/registry.md) | `BuildingRegistrySO` та `BuildingDefinition` |
| [construction/wall-placement.md](construction/wall-placement.md) | `IWallPlacementService`, Bresenham, 8 ручок |
| [construction/resolver-editor.md](construction/resolver-editor.md) | Універсальний резолвер: центральний підбірник ID + спеціалізований редактор |
| [construction/screen-to-grid.md](construction/screen-to-grid.md) | `IScreenToGridConverter`, конвертація координат |
| [construction/ui.md](construction/ui.md) | UI scaffold: підключення кнопок, панелей та сигналів до `IConstructionService` |

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IObjectsMapService`](objects-map.md) | Перевірка `IsOccupied`, підтвердження `Register()` / `Unregister()` |
| [`IGameModeService`](game-mode.md) | Активація / деактивація через `GameModeChangedSignal` |
| [`SignalBus`](signals.md) | Надсилання будівельних сигналів |
| [`IFogOfWarService`](fog-of-war/README.md) | Перевірка видимості тайлу (Unexplored = блокувати) — `[InjectOptional]` |
| [`ISaveModule`](save-system.md) | `ConstructionSaveModule` — збереження/завантаження гравцевих будівель |
| `IBuildingRegistry` | Пошук `BuildingDefinition` за ID, спавн префабів |

---

## Пов'язані системи

- [GameMode](game-mode.md) — перемикає режим будівництва
- [ObjectsMap](objects-map.md) — реєстрація підтверджених будівель
- [Signals](signals.md) — будівельні сигнали
- [Visuals](visuals.md) — `TileView` реагує на `BuildingPreviewChangedSignal`
- [Interactions](interactions.md) — вимикається під час будівництва
- [FogOfWar](fog-of-war/README.md) — заборона будівництва на непрозорих тайлах
- [SaveSystem](save-system.md) — серіалізація будівель через `ConstructionSaveModule`
