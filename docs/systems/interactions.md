# Interactions — Система взаємодії з тайлами

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/interactions)

---

## Призначення

Система **Interactions** є «посередником» між гравцем (клік миші / дотик) і ігровою логікою. Вона отримує сигнал `TileClickedSignal`, визначає намір гравця (вибрати юніта або наказати рух) і делегує виконання системі юнітів.

---

## Як працює внутрішньо

### Двокроковий механізм вибору

| Крок | Стан `_selectedUnitId` | Дія |
|---|---|---|
| **1-й клік** (на зайнятий тайл) | `null` → `"warrior_01"` | Запам'ятовує ID вибраного юніта |
| **2-й клік** (на будь-який тайл) | `"warrior_01"` → `null` | Відправляє команду руху |

### Потік виконання

```
TileClickedSignal
    │
    ▼
TileInteractionService.OnTileClicked()
    │
    ├─ [немає виділеного юніта + тайл зайнятий]
    │       → _selectedUnitId = signal.OccupantId
    │
    └─ [є виділений юніт]
            → CancelMovement()      (якщо старий рух ще йде)
            → UnitMovementService.MoveUnitAsync(...)
```

- Кожен новий наказ руху скасовує попередній через `CancellationTokenSource`.
- Сервіс реалізує `IInitializable` та `IDisposable` — підписка / відписка відбуваються автоматично через Zenject.

---

## Публічний API

### Інтерфейс `ITileInteractionService`

```csharp
namespace Kruty1918.Moyva.Interactions.API
{
    public interface ITileInteractionService
    {
        // Обробляє клік по тайлу. Може викликатися з UI або будь-якої іншої системи вводу.
        void HandleTileClick(Vector2Int position);
    }
}
```

---

## Вхід / Вихід

| Метод | Вхід | Вихід |
|---|---|---|
| `HandleTileClick` | `Vector2Int position` | `void` (сайд-ефект: вибір юніта або запуск руху) |

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IGridService`](grid.md) | Перевірка валідності позиції тайлу (`TryGetTileData`) |
| [`IObjectsMapService`](objects-map.md) | Визначення окупанта тайлу при виборі юніта (`TryGetOccupant`) |
| [`IUnitMovementService`](units.md) | Надсилання команди руху |
| [`SignalBus`](signals.md) | Підписка на `TileClickedSignal` |

---

## Реєстрація в Zenject (`InteractionsInstaller`)

```csharp
internal sealed class InteractionsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // BindInterfacesAndSelfTo — підхоплює IInitializable та IDisposable автоматично
        Container.BindInterfacesAndSelfTo<TileInteractionService>()
            .AsSingle();
    }
}
```

---

## Приклади використання

### Гравець клікає на тайл (автоматично через `TileView`)

```csharp
// TileView.cs — OnMouseDown передає клік у сигнал
private void OnMouseDown()
{
    _signalBus.Fire(new TileClickedSignal
    {
        Position = new Vector2Int((int)transform.position.x, (int)transform.position.y)
    });
}
```

### Програмний виклик (наприклад, з тесту або UI-кнопки)

```csharp
[Inject] private ITileInteractionService _interactionService;

// Симулюємо клік по тайлу (3, 7)
_interactionService.HandleTileClick(new Vector2Int(3, 7));
```

### Повний сценарій: вибір + рух

```
Клік (5,5) → тайл зайнятий "warrior_01" → _selectedUnitId = "warrior_01"
Клік (8,5) → є виділений юніт             → MoveUnitAsync("warrior_01", (8,5))
```

---

## Пов'язані системи

- [Grid](grid.md) — перевіряє валідність позиції тайлу
- [ObjectsMap](objects-map.md) — визначає окупанта при виборі юніта
- [Units](units.md) — отримує команду руху
- [Signals](signals.md) — `TileClickedSignal`
- [Visuals](visuals.md) — `TileView` ініціює `TileClickedSignal` через `OnMouseDown`
