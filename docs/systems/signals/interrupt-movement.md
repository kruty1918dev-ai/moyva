# InterruptMovementSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал для переривання поточного руху юніта. Використовується, коли стаміна юніта вичерпана і рух повинен бути зупинений.

---

## Оголошення

`Signals/API/OnTileChanged.cs`

```csharp
public struct InterruptMovementSignal
{
    public string UnitId;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `UnitId` | `string` | ID юніта, рух якого переривається |

---

## Хто надсилає

- `UnitService` — коли стаміна вичерпана

## Хто отримує

- `UnitMovementService` — зупиняє анімацію та рух юніта

---

## Реєстрація

```csharp
Container.DeclareSignal<InterruptMovementSignal>();
```

---

## Категорія

Ядро

---

## Пов'язані сигнали

- [UnitMovedSignal](unit-moved.md)
