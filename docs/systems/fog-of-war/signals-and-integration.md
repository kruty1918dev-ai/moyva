# Fog of War — Сигнали та інтеграція

← [README](README.md)

---

## Таблиця підписок

`FogOfWarService` підписується на ці сигнали у `Initialize()` (Zenject lifecycle):

| Сигнал | Дія |
|---|---|
| `UnitCreatedSignal` | `RegisterUnit(signal.UnitId, signal.Position, defaultVisionRange)` |
| `UnitMovedSignal` | `UpdateUnitPosition(signal.UnitId, signal.NewPosition)` |
| `UnitDestroyedSignal` | `UnregisterUnit(signal.UnitId)` |

Відписка відбувається у `Dispose()`.

---

## Новий сигнал

| Сигнал | Namespace | Поля |
|---|---|---|
| `FogStateChangedSignal` | `Kruty1918.Moyva.Signals` | `int ChangedTilesCount` |

Оголошується у `SignalBusInstaller`:

```csharp
Container.DeclareSignal<FogStateChangedSignal>();
```

> Примітка: `FogOfWarService` наразі не стріляє `FogStateChangedSignal` автоматично. Це зарезервовано для майбутньої інтеграції (наприклад, UI, збереження при зміні).

---

## Data Flow діаграма

```
[UnitMovementService]
    │
    └── Fire(UnitMovedSignal)
            │
            ▼
    [FogOfWarService.OnUnitMoved]
        → decrement old tiles
        → compute new visible tiles
        → increment new tiles
        → mark explored
        │
        ▼
    [FogTextureUpdater.UpdateDirtyTiles]
        → _buffer[idx] = StateToPixel(state)
        → Texture2D.SetPixelData + Apply
        → material.SetTexture("_FogTex", ...)
            │
            ▼
    [GPU — Moyva/FogOfWar Shader]
        → читає R8 текстуру
        → розраховує alpha + noise + edge bleed
        → відображає туман
```

---

## Порядок ініціалізації

Детальніше: [initialization-order.md](initialization-order.md)

| Крок | Що відбувається |
|---|---|
| 1 | `SignalBusInstaller` оголошує всі сигнали включно з `FogStateChangedSignal` |
| 2 | `FogOfWarInstaller.InstallBindings()` реєструє сервіси |
| 3 | `FogOfWarService.Initialize()` підписується на сигнали (ExecutionOrder = -5) |
| 4 | `FogQuadController.Start()` викликає `fogService.Initialize(w,h)` |
