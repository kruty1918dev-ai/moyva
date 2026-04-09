# BuildingPreviewChangedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається при зміні стану прев'ю будівлі. Містить позицію, ID будівлі та стан прев'ю (None/Valid/Blocked).

---

## Оголошення

`Signals/API/OnConstructionSignals.cs`

```csharp
public struct BuildingPreviewChangedSignal
{
    public Vector2Int Position;
    public string BuildingId;
    public BuildingPreviewState PreviewState;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `Position` | `Vector2Int` | Позиція прев'ю |
| `BuildingId` | `string` | ID будівлі |
| `PreviewState` | `BuildingPreviewState` | Стан прев'ю (див. нижче) |

### BuildingPreviewState

| Значення | Опис |
|---|---|
| `None` | Підсвітку знято |
| `Valid` | Тайл вільний — можна будувати |
| `Blocked` | Тайл зайнятий — червоне підсвічення |

---

## Хто надсилає

- `ConstructionService.TryPreviewAt()` — при наведенні курсору на тайл у режимі будівництва

## Хто отримує

- `ConstructionVisualService` — відображає прев'ю будівлі з відповідним кольором
- `ConstructionUIController` — оновлює UI індикатор стану

---

## Реєстрація

```csharp
Container.DeclareSignal<BuildingPreviewChangedSignal>();
```

---

## Категорія

Construction

---

## Пов'язані сигнали

- [BuildingPlacedSignal](building-placed.md)
- [BuildingCancelledSignal](building-cancelled.md)
