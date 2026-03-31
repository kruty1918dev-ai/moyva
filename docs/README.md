# Moyva — Документація проекту

> Онлайн-версія документації: [kruty1918dev-ai.github.io/moyva/docs/](https://kruty1918dev-ai.github.io/moyva/#home) 

---

## Загальний опис

**Moyva** — це покрокова стратегічна гра (Unity 2D), побудована на архітектурі **Dependency Injection** (Zenject) і принципах модульного дизайну. Гра містить процедурно згенерований тайловий світ, юнітів із системою витривалості, алгоритм пошуку шляху A\* і керування камерою.

---

## Архітектурні принципи

| Принцип | Деталі |
|---|---|
| **DI-контейнер** | [Zenject](https://github.com/modesttree/Zenject) — всі залежності прив'язані через `MonoInstaller` |
| **Розділення API/Runtime** | Кожна фіча містить папки `API/` (інтерфейси, DTO) та `Runtime/` (реалізація + Installer) |
| **Signal Bus** | Між-системна комунікація через `SignalBus` (подієва модель) |
| **Async/Await** | Рух юнітів і анімації використовують `Task` + `CancellationToken` |

---

## Системи та модулі

| Система | Файл документації | Короткий опис |
|---|---|---|
| **Grid** | [systems/grid.md](systems/grid.md) | Керування тайловою сіткою |
| **Units** | [systems/units.md](systems/units.md) | Фабрика, сервіс та рух юнітів |
| **Interactions** | [systems/interactions.md](systems/interactions.md) | Обробка кліків по тайлах |
| **Pathfinding** | [systems/pathfinding.md](systems/pathfinding.md) | Алгоритм A\* пошуку шляху |
| **Animations** | [systems/animations.md](systems/animations.md) | Плавна анімація руху |
| **Camera** | [systems/camera.md](systems/camera.md) | Рух, зум і фокус камери |
| **Signals** | [systems/signals.md](systems/signals.md) | Усі сигнали / події SignalBus |
| **Generator** | [systems/generator.md](systems/generator.md) | Процедурна генерація карти |
| **Visuals** | [systems/visuals.md](systems/visuals.md) | Візуальне відображення тайлів |
| **Bootstrap** | [systems/bootstrap.md](systems/bootstrap.md) | Стартова ініціалізація сцени |

---

## Структура проекту

```
Assets/
└── Moyva/
    └── Scripts/
        ├── Bootstrap/
        │   └── Runtime/
        │       ├── BootstrapInstaller.cs
        │       └── TestUnitSpawner.cs
        └── Features/
            ├── Grid/
            │   ├── API/          ← IGridService, TileData, TileRegistrySO, ITileSettingsService
            │   └── Runtime/      ← GridService, TileSettingsService, GridInstaller
            ├── Units/
            │   ├── API/          ← IUnitService, IUnitFactory, IUnitMovementService, IUnitClassConfig
            │   └── Runtime/      ← UnitService, UnitFactory, UnitMovementService, UnitClassConfig…
            ├── Interactions/
            │   ├── API/          ← ITileInteractionService
            │   └── Runtime/      ← TileInteractionService, InteractionsInstaller
            ├── Pathfinding/
            │   ├── API/          ← IPathfinder
            │   └── Runtime/      ← Pathfinder, PathfinderInstaller
            ├── Animations/
            │   ├── API/          ← IMovementAnimationService, PathAnimationSettings
            │   └── Runtime/      ← MovementAnimationService, AnimationsInstaller
            ├── Camera/
            │   ├── API/          ← ICameraMovement, ICameraZoom, ICameraFocused, CameraSettingsSO
            │   └── Runtime/      ← CameraMovement, CameraZoom, CameraFocused, CameraPlayerController, CameraInstaller
            ├── Signals/
            │   ├── DTO/          ← OnTileChanged, TileClickedSignal (+ інші сигнали)
            │   └── Runtime/      ← SignalBusInstaller
            ├── Generator/
            │   ├── API/          ← IMapDataGenerator, IBiomeResolver, IWFCService, …
            │   ├── Editor/       ← WFCDataSettingsEditor, WFCRulesEditorWindow
            │   └── Runtime/      ← MapDataGenerator, BiomeResolver, WFCService, …
            └── Visuals/
                ├── API/          ← TileView (MonoBehaviour)
                └── Runtime/      ← VisualInstaller
```

---

## Потік даних

```
[Гравець клікає на тайл]
        │
        ▼
  TileView.OnMouseDown()
  → SignalBus.Fire(TileClickedSignal)
        │
        ▼
  TileInteractionService.OnTileClicked()
  → вибір юніта АБО наказ на рух
        │
        ▼
  UnitMovementService.MoveUnitAsync()
  → Pathfinder.FindPath()          (A*)
  → MovementAnimationService       (Task/lerp)
  → SignalBus.Fire(UnitMovedSignal) (UnitService списує стаміну)
        │
        ▼
  GridService.OccupyTile / VacateTile
  → SignalBus.Fire(OnTileChanged)
        │
        ▼
  TileView.OnTileChanged()         (оновлення кольору)
```

---

## Залежності між системами

```
Bootstrap
 └─► Units (IUnitFactory)

Interactions
 ├─► Grid (IGridService)
 ├─► Units (IUnitMovementService)
 └─► Signals (SignalBus)

Units (UnitMovementService)
 ├─► Pathfinding (IPathfinder)
 ├─► Animations (IMovementAnimationService)
 ├─► Grid (IGridService, ITileSettingsService)
 └─► Signals (SignalBus)

Generator
 ├─► Grid (IGridService, TileRegistrySO)
 └─► Visuals (TileView)

Camera
 └─► Input System (InputActionAsset)
```

---

