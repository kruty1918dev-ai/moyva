# FogStateChangedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається після оновлення стану видимості (туман війни). Містить кількість змінених тайлів.

---

## Оголошення

`Signals/API/OnTileChanged.cs`

```csharp
public struct FogStateChangedSignal
{
    public int ChangedTilesCount;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `ChangedTilesCount` | `int` | Кількість тайлів, стан видимості яких змінився |

---

## Хто надсилає

- `FogOfWarService` — після оновлення стану видимості

## Хто отримує

- Підписники оновлення тумана — перемальовують тайли з новим станом видимості

---

## Реєстрація

```csharp
Container.DeclareSignal<FogStateChangedSignal>();
```

---

## Категорія

FogOfWar

---

## Пов'язані сигнали

- [UnitMovedSignal](unit-moved.md)
- [WorldBuiltSignal](world-built.md)
