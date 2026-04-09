# UnitDestroyedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається при знищенні юніта. Містить ID знищеного юніта для очищення з усіх систем.

---

## Оголошення

`Signals/API/OnTileChanged.cs`

```csharp
public struct UnitDestroyedSignal
{
    public string UnitId;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `UnitId` | `string` | ID знищеного юніта |

---

## Хто надсилає

- Зарезервовано для системи смерті

## Хто отримує

- `UnitService` — видаляє юніта з реєстру
- `FogOfWarService` — перераховує видимість після втрати юніта

---

## Реєстрація

```csharp
Container.DeclareSignal<UnitDestroyedSignal>();
```

---

## Категорія

Ядро

---

## Пов'язані сигнали

- [UnitCreatedSignal](unit-created.md)
- [UnitMovedSignal](unit-moved.md)
