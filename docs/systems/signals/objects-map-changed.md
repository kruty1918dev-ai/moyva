# OnObjectsMapChangedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається після будь-якої зміни карти об'єктів. Містить позицію зміненого тайлу та ID нового або видаленого об'єкта.

---

## Оголошення

`Signals/API/OnTileChanged.cs`

```csharp
public struct OnObjectsMapChangedSignal
{
    public Vector2Int Position;
    public string OccupantId;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `Position` | `Vector2Int` | Позиція зміненого тайлу |
| `OccupantId` | `string` | ID об'єкта або `null`, якщо тайл звільнено |

---

## Хто надсилає

- `ObjectsMapService` — після будь-якої зміни карти об'єктів

## Хто отримує

- `TileView` та інші підписники — оновлюють візуальне відображення тайлів

---

## Реєстрація

```csharp
Container.DeclareSignal<OnObjectsMapChangedSignal>().OptionalSubscriber();
```

---

## Категорія

Ядро

---

## Пов'язані сигнали

- [OnMapObjectSpawnedSignal](map-object-spawned.md)
