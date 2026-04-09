# ShowWallHandlesSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал для показу або приховування ручок розміщення стін. Використовується під час drag-and-drop будівництва стін.

---

## Оголошення

`Signals/API/OnConstructionSignals.cs`

```csharp
public struct ShowWallHandlesSignal
{
    public Vector2Int Center;
    public bool Hide;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `Center` | `Vector2Int` | Центральна позиція ручок |
| `Hide` | `bool` | `true` — приховати ручки |

---

## Хто надсилає

- `WallPlacementService.ShowWallHandles()` — показати ручки
- `WallPlacementService.EndDrag()` — приховати ручки після завершення drag

## Хто отримує

- UI-компонент ручок стін — відображає/приховує інтерактивні ручки

---

## Реєстрація

```csharp
Container.DeclareSignal<ShowWallHandlesSignal>();
```

---

## Категорія

Construction

---

## Пов'язані сигнали

- [BuildingPlacedSignal](building-placed.md)
- [BuildingPreviewChangedSignal](building-preview-changed.md)
