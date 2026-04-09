# LoadRequestedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал-запит на завантаження збереженої гри. Містить номер слоту для завантаження.

---

## Оголошення

`Signals/API/OnSaveSignals.cs`

```csharp
public struct LoadRequestedSignal
{
    public int Slot;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `Slot` | `int` | Номер слоту завантаження |

---

## Хто надсилає

- UI або гарячі клавіші — запит на завантаження

## Хто отримує

- `SaveService` — виконує операцію завантаження

---

## Реєстрація

```csharp
Container.DeclareSignal<LoadRequestedSignal>();
```

---

## Категорія

SaveSystem

---

## Пов'язані сигнали

- [SaveRequestedSignal](save-requested.md)
- [SaveCompletedSignal](save-completed.md)
