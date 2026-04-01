# Visuals — Система візуалізації тайлів

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/visuals)

---

## Призначення

Система **Visuals** відповідає за візуальне відображення стану кожного тайлу на екрані. Компонент `TileView` кріпиться до кожного тайлового GameObject і реагує на зміни сітки через `SignalBus`.

---

## Як працює внутрішньо

1. Кожен тайл у сцені має прикріплений компонент `TileView` (MonoBehaviour).
2. При старті `TileView` підписується на `OnObjectsMapChangedSignal` через `SignalBus`.
3. Коли надходить сигнал, `TileView` перевіряє: чи стосується зміна **саме цього** тайлу (`IsMinePosition`).
4. Якщо так — оновлює `SpriteRenderer.color`:
   - `signal.OccupantId != null` (тайл зайнятий) → `_occupiedColor`
   - `signal.OccupantId == null` (тайл вільний) → `Color.white`
5. При кліку мишки (`OnMouseDown`) — надсилає `TileClickedSignal` зі своєю позицією.

---

## Публічний API

### Клас `TileView` (MonoBehaviour)

```csharp
namespace Kruty1918.Moyva.Visuals
{
    public class TileView : MonoBehaviour
    {
        // Налаштовує позицію тайлу в світі (викликається при генерації карти)
        public void Setup(Vector2Int position);

        // Змінює колір тайлу на "зайнятий"
        public void Occupy();

        // Повертає колір тайлу до "вільного" (білий)
        public void Vacate();
    }
}
```

### Поля Inspector

| Поле | Тип | Опис |
|---|---|---|
| `_spriteRenderer` | `SpriteRenderer` | Посилання на рендерер спрайту |
| `_occupiedColor` | `Color` | Колір тайлу, коли на ньому стоїть юніт |

---

## Вхід / Вихід

| Метод | Вхід | Вихід |
|---|---|---|
| `Setup` | `Vector2Int position` | `void` (встановлює `transform.position`) |
| `Occupy` | — | `void` (колір → `_occupiedColor`) |
| `Vacate` | — | `void` (колір → `Color.white`) |
| `OnMouseDown` (Unity) | — | Надсилає `TileClickedSignal` |

---

## Залежності

| Залежність | Причина |
|---|---|
| [`SignalBus`](signals.md) | Підписка на `OnObjectsMapChangedSignal`, надсилання `TileClickedSignal` |
| [`IObjectsMapService`](objects-map.md) | Ін'єктується через `[Inject]` (доступний для розширення логіки) |

---

## Реєстрація в Zenject (`VisualInstaller`)

`VisualInstaller` наразі порожній — `TileView` є MonoBehaviour і не потребує ручного біндингу. Ін'єкції в `TileView` відбуваються через `[Inject]`-атрибути при спавні через `DiContainer.InstantiatePrefab`.

```csharp
public class VisualInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // TileView — MonoBehaviour, ін'єктується автоматично при InstantiatePrefab
    }
}
```

---

## Приклади використання

### Спавн тайлу з ін'єкцією (Generator)

```csharp
// MapVisualInstantiator.cs
var tileObj = _container.InstantiatePrefab(prefab, worldPosition, Quaternion.identity, null);
var tileView = tileObj.GetComponent<TileView>();
tileView.Setup(gridPosition);
```

### Реакція на зміну стану тайлу

```csharp
// Автоматично через SignalBus:
// ObjectsMapService надсилає OnObjectsMapChangedSignal
//   → TileView.OnObjectsMapChanged(signal)
//   → Occupy() якщо OccupantId != null, Vacate() якщо null
```

### Клік на тайл (ланцюг)

```csharp
// TileView.OnMouseDown() → SignalBus.Fire(TileClickedSignal)
// → TileInteractionService підхоплює і обробляє
```

---

## Пов'язані системи

- [Signals](signals.md) — `OnObjectsMapChangedSignal`, `TileClickedSignal`
- [ObjectsMap](objects-map.md) — надсилає `OnObjectsMapChangedSignal`, яку `TileView` обробляє
- [Interactions](interactions.md) — отримує `TileClickedSignal` від `TileView`
- [Generator](generator.md) — спавнить `TileView` при побудові світу
