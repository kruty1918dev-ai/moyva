# Pathfinding — Система пошуку шляху

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/pathfinding)

---

## Призначення

Система **Pathfinding** реалізує алгоритм **A\*** для пошуку оптимального маршруту між двома тайлами на сітці. Вона враховує вагу тайлів (стаміну) і перешкоди (зайняті тайли), підтримує 8-напрямковий рух.

---

## Як працює внутрішньо

### Алгоритм A\*

1. **Ініціалізація**: стартовий вузол додається до `openSet`; `gScore[start] = 0`.
2. **Ітерація**: на кожному кроці береться вузол з найменшим `fScore = gScore + heuristic`.
3. **Сусіди**: 8 напрямків (включно з діагоналями). Перевіряється:
   - Чи є тайл у межах сітки (`TryGetTileData`).
   - Чи не зайнятий тайл (окрім старту і фінішу).
4. **Вартість кроку**: `distanceMultiplier × tileWeight`, де:
   - `distanceMultiplier = 1.414` для діагоналей, `1.0` для прямих.
   - `tileWeight` — вага тайлу з `ITileSettingsService`.
5. **Евристика**: октильна відстань (оптимальна для 8-напрямкового руху).
6. **Відновлення шляху**: список `cameFrom` розгортається від фінішу до старту.

### Особливості

- Якщо `start == end` — повертається список з одного елемента.
- Якщо шлях не знайдено — повертається порожній `List<Vector2Int>`.
- Зайняті тайли є непрохідними, **крім** стартового і цільового вузлів.

---

## Публічний API

### Інтерфейс `IPathfinder`

```csharp
namespace Kruty1918.Moyva.Pathfinding.API
{
    public interface IPathfinder
    {
        // Повертає список координат від старту до фінішу (включно).
        // Повертає порожній список, якщо шляху немає.
        List<Vector2Int> FindPath(Vector2Int start, Vector2Int end);

        // Повертає усіх сусідів (до 8) для заданої координати.
        IEnumerable<Vector2Int> GetNeighbors(Vector2Int position);
    }
}
```

---

## Вхід / Вихід

| Метод | Вхід | Вихід |
|---|---|---|
| `FindPath` | `Vector2Int start, Vector2Int end` | `List<Vector2Int>` (шлях або порожній список) |
| `GetNeighbors` | `Vector2Int position` | `IEnumerable<Vector2Int>` (до 8 вузлів) |

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IGridService`](grid.md) | Перевірка меж сітки (`TryGetTileData`) |
| [`ITileSettingsService`](grid.md) | Вага тайлу для розрахунку вартості кроку |
| [`IObjectsMapService`](objects-map.md) | Перевірка окупації тайлів (`IsOccupied`) для обходу перешкод |

---

## Реєстрація в Zenject (`PathfinderInstaller`)

```csharp
public class PathfinderInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IPathfinder>()
            .To<Pathfinder>()
            .AsSingle();
    }
}
```

---

## Приклади використання

### Знайти шлях між двома точками

```csharp
[Inject] private IPathfinder _pathfinder;

var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(5, 5));

if (path.Count == 0)
    Debug.Log("Шлях не знайдено");
else
    Debug.Log($"Шлях: {string.Join(" → ", path)}");
```

### Реальний приклад — `UnitMovementService.cs`

```csharp
// 1. Отримати поточну позицію юніта
if (!_unitService.TryGetUnitPosition(unitId, out var startPosition)) return;

// 2. Побудувати шлях
var path = _pathfinder.FindPath(startPosition, targetPosition);
if (path == null || path.Count <= 1) return;

// 3. Передати шлях в анімацію
await _animationService.MoveAlongPathAsync(unitObj.transform, path, settings, token);
```

### Отримати сусідів (для перевірки доступних ходів)

```csharp
var neighbors = _pathfinder.GetNeighbors(new Vector2Int(3, 3));
foreach (var n in neighbors)
    Debug.Log($"Сусід: {n}");
```

---

## Деталі реалізації евристики

```csharp
// Октильна евристика (оптимальна для 8-напрямкового руху)
private float Heuristic(Vector2Int a, Vector2Int b)
{
    float dx = Mathf.Abs(a.x - b.x);
    float dy = Mathf.Abs(a.y - b.y);
    return (dx + dy) + (1.414f - 2) * Mathf.Min(dx, dy);
}
```

---

## Пов'язані системи

- [Grid](grid.md) — джерело даних сітки
- [ObjectsMap](objects-map.md) — перевірка окупації тайлів
- [Units](units.md) — використовує `IPathfinder` у `UnitMovementService`
- [Animations](animations.md) — отримує готовий шлях для анімації
