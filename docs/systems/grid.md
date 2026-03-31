# Grid — Система тайлової сітки

← [Назад до README](../README.md)

---

## Призначення

Система **Grid** відповідає за зберігання і керування двовимірною тайловою картою гри. Вона є центральним сховищем стану кожного тайлу: чи зайнятий він, яким юнітом, який тип тайлу (ID для пошуку ваги руху). Майже всі інші системи залежать від неї.

---

## Як працює внутрішньо

1. При ініціалізації `GridService` створює двовимірний масив `TileData[width, height]`.
2. Кожен `TileData` зберігає: прапор `IsOccupied`, ідентифікатор окупанта `OccupantId` і тип тайлу `TileTypeId`.
3. Після будь-якої зміни тайлу (`OccupyTile` / `VacateTile` / `SetTileData`) сервіс надсилає сигнал `OnTileChanged` через `SignalBus` — усі підписники (наприклад, `TileView`) негайно оновлюють відображення.
4. `TileSettingsService` завантажує ваги руху тайлів із `TileRegistrySO` (ScriptableObject) у словник для O(1)-доступу.

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

        // Перевіряє, чи зайнятий тайл
        bool IsTileOccupied(Vector2Int position);

        // Записує довільні дані тайлу (використовується генератором)
        void SetTileData(Vector2Int position, TileData data);

        // Позначає тайл як зайнятий і надсилає OnTileChanged
        void OccupyTile(Vector2Int position, string occupantId);

        // Звільняє тайл і надсилає OnTileChanged
        void VacateTile(Vector2Int position);

        int GridWidth  { get; }
        int GridHeight { get; }
        float TileSize { get; }
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
    public bool   IsOccupied  { get; internal set; }
    public string OccupantId  { get; internal set; }
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
| `IsTileOccupied` | `Vector2Int position` | `bool` |
| `OccupyTile` | `Vector2Int position, string occupantId` | `void` (+ сигнал `OnTileChanged`) |
| `VacateTile` | `Vector2Int position` | `void` (+ сигнал `OnTileChanged`) |
| `GetTileWeight` | `string tileId` | `float` (вага; 0 якщо не знайдено) |

---

## Залежності

| Залежність | Причина |
|---|---|
| [`SignalBus`](signals.md) | Надсилання `OnTileChanged` після зміни тайлу |
| `TileRegistrySO` (SO) | Метадані типів тайлів (вага, префаб) |

---

## Реєстрація в Zenject (`GridInstaller`)

```csharp
public class GridInstaller : MonoInstaller
{
    [SerializeField] private TileRegistrySO tileRegistry;
    [SerializeField] private int   gridWidth  = 10;
    [SerializeField] private int   gridHeight = 10;
    [SerializeField] private float tileSize   = 1f;

    public override void InstallBindings()
    {
        Container.BindInstance(tileRegistry).AsSingle();

        Container.Bind<IGridService>()
            .To<GridService>()
            .AsSingle()
            .WithArguments(gridWidth, gridHeight, tileSize);

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
    Debug.Log($"Тайл: {tile.TileTypeId}, зайнятий: {tile.IsOccupied}");
}
```

### Зайняти / звільнити тайл

```csharp
// Юніт приходить на тайл (2,5)
_gridService.OccupyTile(new Vector2Int(2, 5), "warrior_01");

// Юніт покидає тайл
_gridService.VacateTile(new Vector2Int(2, 5));
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

- [Signals](signals.md) — `OnTileChanged` сигнал
- [Pathfinding](pathfinding.md) — використовує `IGridService` та `ITileSettingsService`
- [Units](units.md) — викликає `OccupyTile` / `VacateTile`
- [Generator](generator.md) — заповнює `TileData` через `SetTileData`
- [Visuals](visuals.md) — підписується на `OnTileChanged` для перефарбування
