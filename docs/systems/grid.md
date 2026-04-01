# Grid — Система тайлової сітки

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/grid)

---

## Призначення

Система **Grid** відповідає за зберігання і керування двовимірною тайловою картою гри. Зберігає тип кожного тайлу (`TileTypeId`) для пошуку ваги руху. Окупація тайлів (хто де стоїть) — виключна відповідальність [`ObjectsMapService`](objects-map.md).

---

## Як працює внутрішньо

1. При ініціалізації `GridService` створює двовимірний масив `TileData[width, height]`.
2. Кожен `TileData` зберігає тип тайлу `TileTypeId` — ID для пошуку ваги руху та візуалу.
3. `TileSettingsService` завантажує ваги руху тайлів із `TileRegistrySO` (ScriptableObject) у словник для O(1)-доступу.

---

## Публічний API

### Інтерфейс `IGridService`

```csharp
namespace Kruty1918.Moyva.Grid.API
{
    public interface IGridService
    {
        // Повертає дані тайлу за позицією; кидає виняток, якщо позиція поза межами
        TileData GetTileData(Vector2Int position);

        // Безпечна версія: повертає false замість винятку
        bool TryGetTileData(Vector2Int position, out TileData tileData);

        // Записує дані тайлу
        void SetTileData(Vector2Int position, TileData data);

        int GridWidth  { get; }
        int GridHeight { get; }
    }
}
```

### Інтерфейс `ITileSettingsService`

```csharp
namespace Kruty1918.Moyva.Grid.API
{
    public interface ITileSettingsService
    {
        // Повертає вагу (вартість руху) для зазначеного ID тайлу
        float GetTileWeight(string tileId);
    }
}
```

### Структура `TileData`

```csharp
public struct TileData
{
    public string TileTypeId  { get; set; }
}
```

### ScriptableObject `TileRegistrySO`

Призначений для налаштування в редакторі Unity. Зберігає масив `TileTypeDefinition[]`, де кожен елемент має:

| Поле | Тип | Опис |
|---|---|---|
| `Id` | `string` | Унікальний ідентифікатор типу тайлу |
| `MovementCost` | `float` | Вартість руху через цей тайл (1 = звичайний, >1 = важкий) |
| `VisualPrefab` | `GameObject` | Префаб для відображення |

---

## Вхід / Вихід

| Метод | Вхід | Вихід |
|---|---|---|
| `GetTileData` | `Vector2Int position` | `TileData` |
| `TryGetTileData` | `Vector2Int position` | `bool` + `out TileData` |
| `SetTileData` | `Vector2Int position, TileData data` | `void` |
| `GetTileWeight` | `string tileId` | `float` (вага; 0 якщо не знайдено) |

---

## Залежності

| Залежність | Причина |
|---|---|
| `TileRegistrySO` (SO) | Метадані типів тайлів (вага, префаб) |

---

## Реєстрація в Zenject (`GridInstaller`)

```csharp
public class GridInstaller : MonoInstaller
{
    [SerializeField] private TileRegistrySO tileRegistry;
    [SerializeField] private int   gridWidth  = 10;
    [SerializeField] private int   gridHeight = 10;

    public override void InstallBindings()
    {
        Container.BindInstance(tileRegistry).AsSingle();

        Container.Bind<IGridService>()
            .To<GridService>()
            .AsSingle()
            .WithArguments(gridWidth, gridHeight);

        Container.Bind<ITileSettingsService>()
            .To<TileSettingsService>()
            .AsSingle();
    }
}
```

---

## Приклади використання

### Читання стану тайлу

```csharp
// Отримати дані безпечно
if (_gridService.TryGetTileData(new Vector2Int(3, 4), out var tile))
{
    Debug.Log($"Тайл: {tile.TileTypeId}");
}
```

### Отримати вагу тайлу (використовується в патфайндері)

```csharp
float weight = _tileSettings.GetTileWeight("swamp"); // наприклад, 3.0
```

### Реальний приклад — `Pathfinder.cs`

```csharp
// Вартість кроку = відстань * вага тайла
float tileWeight       = _tileSettings.GetTileWeight(tileData.TileTypeId);
float distanceMult     = (diagonal) ? 1.414f : 1.0f;
float stepCost         = distanceMult * tileWeight;
float tentativeGScore  = GetScore(gScore, current) + stepCost;
```

---

## Пов'язані системи

- [Pathfinding](pathfinding.md) — використовує `IGridService` та `ITileSettingsService`
- [Generator](generator.md) — заповнює `TileData` через `SetTileData`
- [ObjectsMap](objects-map.md) — єдина авторитетна карта окупації тайлів
- [Visuals](visuals.md) — `TileView` підписується на `OnObjectsMapChangedSignal` для оновлення кольору
