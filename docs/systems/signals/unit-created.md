# UnitCreatedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається при створенні нового юніта. Містить повну інформацію про створений юніт: ID, тип, позицію, радіус бачення та посилання на GameObject.

---

## Оголошення

`Signals/API/OnTileChanged.cs`

```csharp
public struct UnitCreatedSignal
{
    public string UnitId;
    public string UnitTypeId;
    public Vector2Int Position;
    public int VisionRange;
    public GameObject UnitObject;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `UnitId` | `string` | Унікальний ID (наприклад, `"warrior-01_1"`) |
| `UnitTypeId` | `string` | Клас юніта (наприклад, `"warrior"`) |
| `Position` | `Vector2Int` | Стартова позиція |
| `VisionRange` | `int` | Радіус бачення |
| `UnitObject` | `GameObject` | Spawned GameObject |

---

## Хто надсилає

- `UnitFactory.CreateUnit()` — після створення юніта на сцені

## Хто отримує

- `UnitService` — реєструє юніта в системі
- `FogOfWarService` — оновлює туман війни навколо нового юніта

---

## Реєстрація

```csharp
Container.DeclareSignal<UnitCreatedSignal>();
```

---

## Категорія

Ядро

---

## Пов'язані сигнали

- [UnitMovedSignal](unit-moved.md)
- [UnitDestroyedSignal](unit-destroyed.md)
