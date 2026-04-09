# WorldGeneratedDataSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається після генерації даних карти. Містить повні дані згенерованого світу: розміри, карту тайлів, карту об'єктів та карту висот.

---

## Оголошення

`Signals/API/OnTileChanged.cs`

```csharp
public struct WorldGeneratedDataSignal
{
    public int Width;
    public int Height;
    public string[,] TileMap;
    public string[,] ObjectMap;
    public float[,] HeightMap;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `Width` | `int` | Ширина карти |
| `Height` | `int` | Висота карти |
| `TileMap` | `string[,]` | 2D-масив типів тайлів |
| `ObjectMap` | `string[,]` | 2D-масив об'єктів на карті |
| `HeightMap` | `float[,]` | 2D-масив висот |

---

## Хто надсилає

- Генератор карти — після генерації даних

## Хто отримує

- Підписники відображення карти — використовують дані для побудови візуального представлення

---

## Реєстрація

```csharp
Container.DeclareSignal<WorldGeneratedDataSignal>().OptionalSubscriber();
```

---

## Категорія

Ядро

---

## Пов'язані сигнали

- [WorldBuiltSignal](world-built.md)
