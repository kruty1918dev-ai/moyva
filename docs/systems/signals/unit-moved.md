# UnitMovedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається при переміщенні юніта на нову позицію. Містить ID юніта, нову позицію та витрачену стаміну.

---

## Оголошення

`Signals/API/OnTileChanged.cs`

```csharp
public struct UnitMovedSignal
{
    public string UnitId;
    public Vector2Int NewPosition;
    public float Cost;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `UnitId` | `string` | ID юніта |
| `NewPosition` | `Vector2Int` | Нова позиція |
| `Cost` | `float` | Списана стаміна |

---

## Хто надсилає

- `UnitMovementService` — через `OnStepCompleted` анімації

## Хто отримує

- `UnitService` — оновлює позицію юніта в реєстрі
- `FogOfWarService` — перераховує видимість навколо нової позиції

---

## Реєстрація

```csharp
Container.DeclareSignal<UnitMovedSignal>();
```

---

## Категорія

Ядро

---

## Пов'язані сигнали

- [UnitCreatedSignal](unit-created.md)
- [InterruptMovementSignal](interrupt-movement.md)
