# Moyva — Документація проекту

> Онлайн-версія документації: [kruty1918dev-ai.github.io/moyva/](https://kruty1918dev-ai.github.io/moyva/#home)

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
| **Generator** | [systems/generator.md](systems/generator.md) | Процедурна генерація карти (pipeline) |
| **Graph System** | [systems/graph-system/README.md](systems/graph-system/README.md) | Візуальний нодовий генератор карт (нова система) |
| **ObjectsMap** | [systems/objects-map.md](systems/objects-map.md) | Єдине джерело правди про зайнятість позицій |
| **Visuals** | [systems/visuals.md](systems/visuals.md) | Візуальне відображення тайлів |
| **Bootstrap** | [systems/bootstrap.md](systems/bootstrap.md) | Стартова ініціалізація сцени |
| **Порядок ініціалізації** | [systems/initialization-order.md](systems/initialization-order.md) | Послідовність запуску вузлів ядра Zenject |
| **GameMode** | [systems/game-mode.md](systems/game-mode.md) | Управління ігровими режимами (Normal / Construction) |
| **Construction** | [systems/construction.md](systems/construction.md) | Система будівництва: preview, Undo/Redo, стіни |
| **FogOfWar** | [systems/fog-of-war/README.md](systems/fog-of-war/README.md) | Туман війни: сітка видимості, шейдер, FOV |
| **SaveSystem** | [systems/save-system.md](systems/save-system.md) | Збереження/завантаження стану гри у бінарний .mvs формат; ConfigService для глобального конфігу |

---

## Стандарти

| Документ | Файл | Призначення |
|---|---|---|
| **TDD: Modular Architecture** | [standarts/TDD.md](standarts/TDD.md) | Архітектурні правила модульності, DI та asmdef |

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
            │   ├── API/          ← IGridService, TileRegistrySO, ITileSettingsService
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
            │   ├── DTO/          ← TileClickedSignal, OnTileChanged, OnConstructionSignals, OnGameModeSignals
            │   └── Runtime/      ← SignalBusInstaller
            ├── Generator/
            │   ├── API/          ← IMapDataGenerator, IBiomeResolver, IWFCService, …
            │   ├── Editor/       ← WFCDataSettingsEditor, WFCRulesEditorWindow
            │   └── Runtime/      ← MapDataGenerator, BiomeResolver, WFCService, …
            ├── ObjectsMap/
            │   ├── API/          ← IObjectsMapService
            │   └── Runtime/      ← ObjectsMapService, ObjectsMapInstaller
            ├── Visuals/
            │   ├── API/          ← TileView (MonoBehaviour)
            │   └── Runtime/      ← VisualInstaller
            ├── GameMode/
            │   ├── API/          ← IGameModeService, GameModeType
            │   └── Runtime/      ← GameModeService, GameModeInstaller
            ├── Construction/
                ├── API/          ← IConstructionService, IWallPlacementService,
                │                    IConstructionInputService, IScreenToGridConverter,
                │                    BuildingCategory, BuildingPlacementState,
                │                    BuildingPreviewState, BuildingDefinition
                └── Runtime/      ← ConstructionService, WallPlacementService,
                                     ConstructionInputService, ScreenToGridConverter,
                                     BuildingRegistrySO, ConstructionInstaller
            └── SaveSystem/
                ├── API/          ← ISaveService, IConfigService, ISaveModule,
                │                    ISaveContext, SaveSlotInfo
                └── Runtime/      ← SaveService, ConfigService, SaveContext,
                                     SaveFileCodec, Crc32, SaveSystemInstaller
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
  ObjectsMapService.OnUnitMoved()
  → SignalBus.Fire(OnObjectsMapChangedSignal)
        │
        ▼
  TileView.OnObjectsMapChanged()   (оновлення кольору)
```

---

## Залежності між системами

```
Bootstrap
 └─► Units (IUnitFactory)

Interactions
 ├─► Grid (IGridService)
 ├─► ObjectsMap (IObjectsMapService)
 ├─► Units (IUnitMovementService)
 ├─► Signals (SignalBus)
 └─► Signals (GameModeChangedSignal) ← вимикається в режимі Construction

Units (UnitMovementService)
 ├─► Units (IUnitService)
 ├─► Pathfinding (IPathfinder)
 ├─► Animations (IMovementAnimationService)
 ├─► Grid (IGridService, ITileSettingsService)
 ├─► Units (IUnitClassConfig)
 └─► Signals (SignalBus)

ObjectsMap
 └─► Signals (UnitCreatedSignal, UnitMovedSignal, UnitDestroyedSignal, OnMapObjectSpawnedSignal)

Generator
 ├─► Grid (IGridService, TileRegistrySO)
 └─► Visuals (TileView)

Camera
 └─► Input System (InputActionAsset)

GameMode
 └─► Signals (GameModeChangedSignal)

Construction
 ├─► ObjectsMap (IObjectsMapService)
 ├─► Signals (BuildingPlacedSignal, BuildingCancelledSignal,
 │            BuildingPreviewChangedSignal, ShowWallHandlesSignal)
 └─► Signals (GameModeChangedSignal) ← активується в режимі Construction
```

---

## Аудит документації (2026-03-31)

Під час аудиту виконано синхронізацію документації з поточною структурою проєкту:

- додано модуль **ObjectsMap** у загальний реєстр систем;
- оновлено онлайн-посилання на актуальний маршрут `docs/#home`;
- додано розділ **Стандарти** з TDD-документом;
- оновлено структуру проєкту в README (додано `Features/ObjectsMap`);
- перевпорядковано та нормалізовано `standarts/TDD.md` у читабельний markdown-формат.

## Аудит документації (2026-04-03)

Структуризація та очищення документації:

- видалено дублікат `systems/SaveSystem.md` (старий чернетковий файл);
- додано розділ **ConfigService** до `systems/save-system.md` (IConfigService, config.mvs);
- оновлено структуру проєкту в README (додано `IConfigService`, `ConfigService`);
- підсторінки системи **Construction** перенесено до підпапки `systems/construction/` (за аналогією з `fog-of-war/`):
  - `construction-service.md` → `construction/service.md`
  - `construction-registry.md` → `construction/registry.md`
  - `wall-placement.md` → `construction/wall-placement.md`
  - `screen-to-grid.md` → `construction/screen-to-grid.md`
- оновлено всі перехресні посилання у файлах системи Construction.

