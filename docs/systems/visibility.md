# Visibility — Туман війни (лічильникова сітка)

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/visibility)

---

## Призначення

Система **Visibility** реалізує туман війни через спільний двовимірний масив лічильників (`int[width, height]`) на всю карту. Це єдина сітка видимості для всієї гри.

Кожна цифра в масиві — **лічильник**: скільки дружніх юнітів зараз бачать цей тайл.

| Значення лічильника | Стан тайлу |
|---|---|
| `0` | Туман — тайл невидимий |
| `≥ 1` | Видимий — хоча б один юніт спостерігає |

---

## Як працює

1. **Юніт створено** → `+1` для всіх тайлів у радіусі зору.
2. **Юніт зробив крок** → `−1` для тайлів старої позиції, `+1` для нових.
3. **Юніт знищено** → `−1` для всіх тайлів у радіусі зору.
4. **Перехід `0 → 1`** → тайл стає видимим, оновлюється піксель текстури (чорний → білий), надсилається `OnVisibilityChangedSignal`.
5. **Перехід `1 → 0`** → тайл іде в туман, піксель текстури (білий → чорний), надсилається `OnVisibilityChangedSignal`.

Якщо два юніти дивляться на один тайл — лічильник = 2. Якщо один відійшов — лічильник = 1, тайл все ще видимий. Туман настає лише тоді, коли **всі** юніти відійшли.

---

## Публічний API

### `IVisibilityService`

```csharp
public interface IVisibilityService
{
    // Чи видимий тайл (лічильник > 0)?
    bool IsVisible(Vector2Int position);

    // Поточне значення лічильника видимості
    int GetVisibilityCount(Vector2Int position);

    // Текстура видимості: кожен піксель = один тайл (білий/чорний)
    Texture2D GetVisibilityTexture();

    // Повертає копію сітки для збереження гри
    int[,] GetRawGrid();

    // Завантажує сітку з файлу збереження
    void LoadFromGrid(int[,] grid);
}
```

---

## Текстура видимості

При ініціалізації сервіс створює `Texture2D(width, height)` де:

- **Чорний піксель** `(0, 0, 0, 1)` = туман
- **Білий піксель** `(1, 1, 1, 1)` = видимий тайл

Текстуру можна підключити до шейдера туману війни. Отримати через `IVisibilityService.GetVisibilityTexture()`.

Налаштування текстури: `FilterMode.Point` (без розмиття, чіткі пікселі по тайлах).

---

## Радіус зору юніта

Радіус зору налаштовується в `UnitClassConfig` для кожного класу юніта:

```csharp
[Serializable]
public class UnitClassConfig
{
    // ...
    public int VisionRadius = 3; // Радіус у тайлах (Чебишевська відстань)
}
```

Форма зору — **квадрат** (Чебишевська відстань): `|dx| ≤ r AND |dy| ≤ r`.

---

## Сигнали

| Сигнал | Напрямок | Опис |
|---|---|---|
| `UnitCreatedSignal` | IN | Додає зону видимості для нового юніта |
| `UnitMovedSignal` | IN | Оновлює зону видимості після кроку |
| `UnitDestroyedSignal` | IN | Видаляє зону видимості знищеного юніта |
| `OnVisibilityChangedSignal` | OUT | Тайл змінив стан: `IsVisible=true` (відкрився) або `false` (туман) |

### `OnVisibilityChangedSignal`

```csharp
public struct OnVisibilityChangedSignal
{
    public Vector2Int Position;
    public bool IsVisible; // true = 0→1 (видимий); false = 1→0 (туман)
}
```

---

## Збереження та завантаження

Сервіс підтримує збереження/відновлення стану без повторного прорахунку:

```csharp
// Зберегти
int[,] savedGrid = _visibilityService.GetRawGrid();
// ... серіалізувати savedGrid у файл збереження

// Завантажити
_visibilityService.LoadFromGrid(savedGrid);
```

> Метод `LoadFromGrid` замінює весь поточний стан і повністю перебудовує текстуру.

---

## Реєстрація в Zenject (`VisibilityInstaller`)

```csharp
public class VisibilityInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<VisibilityService>()
            .AsSingle()
            .NonLazy();
    }
}
```

Додайте `VisibilityInstaller` до SceneContext у Unity Inspector.

Не забудьте також переконатись, що `OnVisibilityChangedSignal` оголошено в `SignalBusInstaller` (вже додано за замовчуванням).

---

## Вхід / Вихід

| Метод | Вхід | Вихід |
|---|---|---|
| `IsVisible` | `Vector2Int position` | `bool` |
| `GetVisibilityCount` | `Vector2Int position` | `int` |
| `GetVisibilityTexture` | — | `Texture2D` |
| `GetRawGrid` | — | `int[,]` (копія) |
| `LoadFromGrid` | `int[,] grid` | `void` |

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IGridService`](grid.md) | Розміри карти (`GridWidth`, `GridHeight`) для ініціалізації сітки |
| `IUnitClassConfig` | Радіус зору (`VisionRadius`) для кожного класу юніта |
| [`SignalBus`](signals.md) | Підписка на юніт-сигнали; надсилання `OnVisibilityChangedSignal` |

---

## Пов'язані системи

- [Units](units.md) — `UnitClassConfig.VisionRadius` визначає радіус зору
- [Grid](grid.md) — надає розміри карти
- [Signals](signals.md) — `UnitCreatedSignal`, `UnitMovedSignal`, `UnitDestroyedSignal`, `OnVisibilityChangedSignal`
- [Visuals](visuals.md) — може використовувати `GetVisibilityTexture()` у шейдері туману
