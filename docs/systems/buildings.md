# Buildings — Система будівництва

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/buildings)

---

## Призначення

Система **Buildings** реалізує повний цикл будівництва: вибір будівлі з тематичного меню, попередній перегляд на карті, підтвердження або скасування сесії, Undo/Redo, а також спеціальний режим побудови стін із 8-напрямковими індикаторами та drag-to-build.

---

## Меню будівництва (три категорії)

| Категорія | `BuildingCategory` | Призначення |
|---|---|---|
| Військові | `Military` | Казарми, вежі, укріплення |
| Цивільні | `Civilian` | Будинки, ринки, ферми |
| Індустріальні | `Industrial` | Шахти, заводи, склади |

Кожна будівля описується `BuildingConfig` (ScriptableObject), зареєстрованим у `BuildingRegistrySO`.

---

## Як працює внутрішньо

### Сесія розміщення

```
Гравець вибирає будівлю в меню
    │
    ▼
IBuildingPlacementService.SelectBuilding(buildingId)
    → IsPlacingMode = true

Гравець наводить курсор / палець на тайл
    │
    ▼
BuildingPreviewView.UpdatePreview(hoveredPosition, sprite)
    → CanPlaceAt(pos) == true  → звичайний спрайт
    → CanPlaceAt(pos) == false → червоний спрайт

Гравець клікає на тайл (TileClickedSignal)
    │
    ▼
IBuildingPlacementService.PlaceBuilding(position)
    → запис у _pending list
    → резервування тайлів

[Підтвердження] Confirm()
    → ObjectsMapService.Register(pos, instanceId) для кожного pending
    → Fire(BuildingPlacedSignal) для кожного
    → очищення сесії

[Скасування] Cancel()
    → звільнення всіх reserved позицій
    → Fire(BuildingCancelledSignal) для кожного
    → очищення сесії
```

---

## Undo / Redo

| Дія | Метод | Клавіші (ПК) |
|---|---|---|
| Скасувати останнє | `Undo()` | `Ctrl+Z` |
| Повторити скасоване | `Redo()` | `Ctrl+Y` |
| Скасувати всю сесію | `Cancel()` | `Escape` |

Клавіатурні скорочення обробляються `BuildingInputHandler` (MonoBehaviour, підключається до об'єкта на сцені).

---

## Система стін

Після кожного підтвердженого розміщення стіни (`wall-stone` або будь-який ID, налаштований у `WallPlacementController`) навколо неї з'являються **8 кружечків-індикаторів** (по одному в кожному напрямку).

### Взаємодія з кружечками

| Дія | Результат |
|---|---|
| Клік на кружечок | Розміщення стіни у відповідному напрямку |
| Утримання + перетягування | Будування ланцюга стін за алгоритмом Bresenham від поточної стіни до позиції пальця/курсора |

```
Поставлена стіна @ (5,5)
    │
    ▼
WallPlacementController.ShowCircles((5,5))
    → 8 × WallCircleHandler spawn

Гравець натискає і тягне коло NE
    │
    ▼
WallCircleHandler.OnDragUpdate(tilePos)
    → WallPlacementController.BuildLinePath((5,5), tilePos)
    → PlaceBuilding для кожної вільної позиції на шляху
```

---

## Публічний API

### `IBuildingPlacementService`

```csharp
public interface IBuildingPlacementService
{
    bool IsPlacingMode { get; }
    string SelectedBuildingId { get; }

    void SelectBuilding(string buildingId);
    void ExitPlacingMode();

    bool CanPlaceAt(Vector2Int position);
    void PlaceBuilding(Vector2Int position);

    void Confirm();
    void Cancel();

    void Undo();
    void Redo();
}
```

---

## Сигнали

| Сигнал | Тип | Надсилає | Отримує |
|---|---|---|---|
| `BuildingPlacedSignal` | `struct` | `BuildingPlacementService` (Confirm) | ObjectsMapService, UI |
| `BuildingCancelledSignal` | `struct` | `BuildingPlacementService` (Cancel / Undo) | UI |

---

## ScriptableObject-конфігурація

### `BuildingConfig`

```csharp
[CreateAssetMenu(menuName = "Moyva/Buildings/BuildingConfig")]
public class BuildingConfig : ScriptableObject, IBuildingConfig
{
    string BuildingId;       // наприклад "barracks-01"
    string DisplayName;      // "Казарма"
    BuildingCategory Category;
    Sprite Sprite;
    Vector2Int Size;         // 1×1 для більшості будівель
    GameObject Prefab;
}
```

### `BuildingRegistrySO`

```csharp
[CreateAssetMenu(menuName = "Moyva/Buildings/BuildingRegistry")]
public class BuildingRegistrySO : ScriptableObject
{
    BuildingConfig[] Buildings;
    BuildingConfig GetById(string id);
}
```

---

## Реєстрація в Zenject (`BuildingsInstaller`)

```csharp
public class BuildingsInstaller : MonoInstaller
{
    [SerializeField] private BuildingRegistrySO _buildingRegistry;

    public override void InstallBindings()
    {
        Container.BindInstance(_buildingRegistry).AsSingle();
        Container.BindInterfacesAndSelfTo<BuildingPlacementService>().AsSingle();
    }
}
```

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IGridService`](grid.md) | Перевірка валідності позиції тайлу |
| [`IObjectsMapService`](objects-map.md) | Перевірка зайнятості та реєстрація будівель |
| [`SignalBus`](signals.md) | Надсилання `BuildingPlacedSignal`, `BuildingCancelledSignal`; підписка на `TileClickedSignal` |

---

## Пов'язані системи

- [Grid](grid.md) — перевіряє валідність позиції тайлу
- [ObjectsMap](objects-map.md) — реєструє підтверджені будівлі
- [Signals](signals.md) — `BuildingPlacedSignal`, `BuildingCancelledSignal`, `TileClickedSignal`
- [Visuals](visuals.md) — `TileView` надсилає `TileClickedSignal`
- [Interactions](interactions.md) — може інтегруватися з TileClickedSignal
