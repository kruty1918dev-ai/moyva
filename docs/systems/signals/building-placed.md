# BuildingPlacedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається при підтвердженні розміщення будівлі. Містить ID будівлі та позицію на карті.

---

## Оголошення

`Signals/API/OnConstructionSignals.cs`

```csharp
public struct BuildingPlacedSignal
{
    public string BuildingId;
    public Vector2Int Position;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `BuildingId` | `string` | ID будівлі |
| `Position` | `Vector2Int` | Позиція розміщення |

---

## Хто надсилає

- `ConstructionService.Confirm()` — для кожного підтвердженого розміщення

## Хто отримує

- `ConstructionVisualService` — створює візуальне представлення будівлі
- `ConstructionUIController` — оновлює UI будівництва

---

## Реєстрація

```csharp
Container.DeclareSignal<BuildingPlacedSignal>();
```

---

## Категорія

Construction

---

## Пов'язані сигнали

- [BuildingCancelledSignal](building-cancelled.md)
- [BuildingPreviewChangedSignal](building-preview-changed.md)
- [BuildingDemolishedSignal](building-demolished.md)
