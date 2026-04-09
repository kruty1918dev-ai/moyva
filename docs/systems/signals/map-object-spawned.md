# OnMapObjectSpawnedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається після спавну статичного об'єкта карти (річка, гора тощо). Містить тип об'єкта та його позицію.

---

## Оголошення

`Signals/API/OnTileChanged.cs`

```csharp
public struct OnMapObjectSpawnedSignal
{
    public string ObjectId;
    public Vector2Int Position;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `ObjectId` | `string` | TileTypeId (наприклад, `"river"`, `"mountain"`) |
| `Position` | `Vector2Int` | Позиція об'єкта на карті |

---

## Хто надсилає

- `MapVisualInstantiator` — після спавну статичного об'єкта карти

## Хто отримує

- `ObjectsMapService` — реєструє об'єкт у карті об'єктів

---

## Реєстрація

```csharp
Container.DeclareSignal<OnMapObjectSpawnedSignal>();
```

---

## Категорія

Ядро

---

## Пов'язані сигнали

- [OnObjectsMapChangedSignal](objects-map-changed.md)
- [WorldBuiltSignal](world-built.md)
