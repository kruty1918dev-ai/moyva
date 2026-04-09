# ObjectsMap — Єдина карта обʼєктів

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/objects-map)

---

## Призначення

`ObjectsMapService` — єдине авторитетне джерело правди про те, **хто де стоїть** на ігровій сітці.  
Не розрізняє тип окупанта (юніт, будівля, ресурс) — тільки факт присутності та ідентифікатор.

Замінює дублювання позицій між `GridService` та `UnitService._unitPositions`, усуваючи потенційні точки розсинхронізації при масштабуванні (будівлі, туман війни, FOV).

---

## Як працює внутрішньо

1. Два словники: `position → occupantId` та `occupantId → position` для O(1)-доступу в обох напрямках.
2. Підписується на `UnitCreatedSignal`, `UnitMovedSignal`, `UnitDestroyedSignal` — автоматично відстежує юніти.
3. Підписується на `OnMapObjectSpawnedSignal` — реєструє статичні обʼєкти карти (річки, гори тощо).
4. При кожній зміні надсилає `OnObjectsMapChangedSignal` — підписники (наприклад, `TileView`, `Pathfinder`) реагують самостійно.

`ObjectsMapService` **не залежить** від `IGridService` — він є повністю незалежною структурою даних.

---

## Публічний API

### `IObjectsMapService`

```csharp
namespace Kruty1918.Moyva.ObjectsMap.API
{
    public interface IObjectsMapService
    {
        // Чи зайнята позиція будь-яким обʼєктом?
        bool IsOccupied(Vector2Int position);

        // Отримати ID окупанта. Повертає false якщо тайл вільний.
        bool TryGetOccupant(Vector2Int position, out string occupantId);

        // Зареєструвати обʼєкт на позиції. Кидає InvalidOperationException якщо позиція зайнята.
        void Register(Vector2Int position, string occupantId);

        // Перемістити обʼєкт з from → to. Кидає InvalidOperationException якщо to зайнято або from вільно.
        void Move(Vector2Int from, Vector2Int to);

        // Звільнити позицію. Нічого не робить якщо позиція вже вільна.
        void Unregister(Vector2Int position);

        // Знайти позицію за ID окупанта. False якщо не зареєстровано.
        bool TryGetPosition(string occupantId, out Vector2Int position);
    }
}
```

---

## Сигнали

| Сигнал | Напрямок | Опис |
|---|---|---|
| `UnitCreatedSignal` | IN | Реєструє юніт на стартовій позиції |
| `UnitMovedSignal` | IN | Переміщує запис у карті |
| `UnitDestroyedSignal` | IN | Видаляє запис |
| `OnMapObjectSpawnedSignal` | IN | Реєструє статичний обʼєкт карти (із `MapVisualInstantiator`) |
| `OnObjectsMapChangedSignal` | OUT | Сповіщає підписників про зміну (позиція + новий ID або null якщо звільнено) |

### `OnMapObjectSpawnedSignal`

```csharp
public struct OnMapObjectSpawnedSignal
{
    public string ObjectId;       // TileTypeId, наприклад "river", "mountain"
    public Vector2Int Position;
}
```

### `OnObjectsMapChangedSignal`

```csharp
public struct OnObjectsMapChangedSignal
{
    public Vector2Int Position;
    public string OccupantId;     // null якщо тайл звільнено
}
```

---

## Реєстрація в Zenject

Додайте `ObjectsMapInstaller` до SceneContext у Unity Inspector поряд з іншими інсталерами.

```csharp
// ObjectsMapInstaller.cs
public class ObjectsMapInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<ObjectsMapService>()
            .AsSingle()
            .NonLazy();
    }
}
```

---

## Вхід / Вихід

| Метод | Вхід | Вихід |
|---|---|---|
| `IsOccupied` | `Vector2Int position` | `bool` |
| `TryGetOccupant` | `Vector2Int position` | `bool` + `out string occupantId` |
| `Register` | `Vector2Int position, string occupantId` | `void` (+ сигнал `OnObjectsMapChangedSignal`) |
| `Move` | `Vector2Int from, Vector2Int to` | `void` (+ два сигнали `OnObjectsMapChangedSignal`) |
| `Unregister` | `Vector2Int position` | `void` (+ сигнал `OnObjectsMapChangedSignal`) |
| `TryGetPosition` | `string occupantId` | `bool` + `out Vector2Int position` |

---

## Залежності

| Залежність | Причина |
|---|---|
| [`SignalBus`](signals/README.md) | Підписка на юніт-сигнали; надсилання `OnObjectsMapChangedSignal` |

---

## Пов'язані системи

- [Units](units.md) — `UnitService` більше не веде власний трекінг позицій через `GridService`
- [Generator](generator.md) — `MapVisualInstantiator` надсилає `OnMapObjectSpawnedSignal` після спавну статичних обʼєктів
- [Pathfinding](pathfinding.md) — `Pathfinder` використовує `IObjectsMapService.IsOccupied()` для обходу перешкод
- [Visuals](visuals.md) — `TileView` підписується на `OnObjectsMapChangedSignal` для оновлення кольору
- [Signals](signals/README.md) — нові сигнали `OnMapObjectSpawnedSignal`, `OnObjectsMapChangedSignal`
- [Construction](construction.md) — `ConstructionService.Confirm()` викликає `Register()` для підтверджених будівель
