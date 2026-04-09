# SaveRequestedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал-запит на збереження гри. Містить номер слоту для збереження.

---

## Оголошення

`Signals/API/OnSaveSignals.cs`

```csharp
public struct SaveRequestedSignal
{
    public int Slot;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `Slot` | `int` | Номер слоту збереження |

---

## Хто надсилає

- UI або гарячі клавіші — запит на збереження

## Хто отримує

- `SaveService` — виконує операцію збереження

---

## Реєстрація

```csharp
Container.DeclareSignal<SaveRequestedSignal>();
```

---

## Категорія

SaveSystem

---

## Пов'язані сигнали

- [LoadRequestedSignal](load-requested.md)
- [SaveCompletedSignal](save-completed.md)
