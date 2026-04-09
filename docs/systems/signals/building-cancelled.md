# BuildingCancelledSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається при скасуванні сесії будівництва. Порожня структура-маркер без полів.

---

## Оголошення

`Signals/API/OnConstructionSignals.cs`

```csharp
public struct BuildingCancelledSignal { }
```

---

## Хто надсилає

- `ConstructionService.Cancel()` — при скасуванні сесії будівництва

## Хто отримує

- `ConstructionVisualService` — видаляє прев'ю будівлі
- `ConstructionUIController` — скидає UI будівництва

---

## Реєстрація

```csharp
Container.DeclareSignal<BuildingCancelledSignal>();
```

---

## Категорія

Construction

---

## Пов'язані сигнали

- [BuildingPlacedSignal](building-placed.md)
- [BuildingPreviewChangedSignal](building-preview-changed.md)
