# BuildingDemolishedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається при успішному знесенні будівлі. Містить ID знесеної будівлі та її позицію.

---

## Оголошення

`Signals/API/OnConstructionSignals.cs`

```csharp
public struct BuildingDemolishedSignal
{
    public string BuildingId;
    public Vector2Int Position;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `BuildingId` | `string` | ID знесеної будівлі |
| `Position` | `Vector2Int` | Позиція будівлі |

---

## Хто надсилає

- `ConstructionService.TryDemolishAt()` — при успішному знесенні

## Хто отримує

- `ConstructionVisualService` — видаляє візуальне представлення будівлі

---

## Реєстрація

```csharp
Container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();
```

---

## Категорія

Construction

---

## Пов'язані сигнали

- [BuildingPlacedSignal](building-placed.md)
