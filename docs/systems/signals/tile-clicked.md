# TileClickedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається при натисканні на тайл карти. Містить координати тайлу, на який клікнув гравець.

---

## Оголошення

`Signals/API/TileClickedSignal.cs`

```csharp
public class TileClickedSignal
{
    public Vector2Int Position;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `Position` | `Vector2Int` | Координати тайлу |

---

## Хто надсилає

- `TileView.OnMouseDown()` — при натисканні миші на тайл

## Хто отримує

- `TileInteractionService` — обробляє взаємодію з тайлом

---

## Реєстрація

```csharp
Container.DeclareSignal<TileClickedSignal>();
```

---

## Категорія

Ядро

---

## Пов'язані сигнали

- [UnitMovedSignal](unit-moved.md)
- [GameModeChangedSignal](game-mode-changed.md)
