# SaveCompletedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається після завершення операції збереження. Містить результат операції: номер слоту, успішність та повідомлення про помилку (якщо є).

---

## Оголошення

`Signals/API/OnSaveSignals.cs`

```csharp
public struct SaveCompletedSignal
{
    public int Slot;
    public bool Success;
    public string ErrorMessage;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `Slot` | `int` | Номер слоту збереження |
| `Success` | `bool` | Чи успішно завершено збереження |
| `ErrorMessage` | `string` | Повідомлення про помилку (якщо `Success == false`) |

---

## Хто надсилає

- `SaveService` — після завершення операції збереження

## Хто отримує

- UI-підписники — відображають результат збереження користувачу

---

## Реєстрація

```csharp
Container.DeclareSignal<SaveCompletedSignal>().OptionalSubscriber();
```

---

## Категорія

SaveSystem

---

## Пов'язані сигнали

- [SaveRequestedSignal](save-requested.md)
- [LoadRequestedSignal](load-requested.md)
