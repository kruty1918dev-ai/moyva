# Fog of War — Порядок ініціалізації

← [README](README.md) · [Глобальний порядок ініціалізації](../initialization-order.md)

---

## Місце FogOfWar у Zenject-послідовності

`FogOfWarService` використовує `BindExecutionOrder<FogOfWarService>(-5)`, що означає:

- Ініціалізується **після** `ObjectsMapService` (ExecutionOrder = -10)
- Ініціалізується **до** більшості сервісів за замовчуванням (0)
- Ініціалізується **набагато раніше** `TestUnitSpawner` (100)

---

## Оновлена таблиця ExecutionOrder

| Сервіс | ExecutionOrder | Що робить у Initialize() |
|---|---|---|
| `ObjectsMapService` | -10 | Підписується на Unit* сигнали |
| **`FogOfWarService`** | **-5** | **Підписується на Unit* сигнали** |
| Інші сервіси | 0 | За замовчуванням |
| `TestUnitSpawner` | +100 | Стріляє `UnitCreatedSignal` |

---

## Чому -5?

`FogOfWarService` повинен **підписатись на `UnitCreatedSignal`** до того, як `TestUnitSpawner` стріляє цей сигнал при старті гри. ExecutionOrder = -5 гарантує це.

---

## Послідовність у сцені

```
SceneContext
├── SignalBusInstaller       (DeclareSignal × N, включно з FogStateChangedSignal)
├── GridInstaller            (IGridService)
├── FogOfWarInstaller        (IFogOfWarService, IFogVisibilityResolver, ...)
│    └── FogOfWarService     [ExecutionOrder = -5]
├── ObjectsMapInstaller      [ExecutionOrder = -10]
├── ... інші інсталятори ...
└── BootstrapInstaller       [ExecutionOrder = +100]

Initialize() порядок:
├── ObjectsMapService [-10]  → Subscribe(Unit* signals)
├── FogOfWarService   [-5]   → Subscribe(Unit* signals)
├── ... [0] ...
└── TestUnitSpawner   [+100] → Fire(UnitCreatedSignal) → обидва сервіси отримують ✓
```

---

## FogQuadController.Start()

`FogQuadController` є MonoBehaviour — його `Start()` викликається Unity Engine, **після** всіх Zenject `Initialize()`. Саме тому виклик `fogService.Initialize(w, h)` безпечний: на цей момент всі підписки вже встановлені.
