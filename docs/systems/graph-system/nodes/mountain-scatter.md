# Mountain Scatter

← [Особливості](../nodes-features.md) · [Graph System](../README.md)

**Категорія:** Features · **Файл:** `MountainScatterNode.cs`

Розміщує **гірські об'єкти** (скелі, валуни, снігові вершини) за шарами висоти. Чим вище точка — тим більша щільність гірських об'єктів.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🔵 | ObjectMap | `string[,]` | Карта об'єктів |
| **Output** 🔵 | ObjectMap | `string[,]` | Карта з гірськими об'єктами |

## Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Layers** | `MountainLayer[]` | Масив шарів з діапазонами висоти та об'єктами |
| **Seed** | `int` | Зерно для генерації (за замовч. 42) |
| **Avoid Existing Objects** | `bool` | Чи уникати клітинок де вже є об'єкти (за замовч. true) |

## Структура `MountainLayer`

| Поле | Тип | Опис |
|---|---|---|
| `MinHeight` | `float` | Мінімальна висота шару (0.0–1.0) |
| `MaxHeight` | `float` | Максимальна висота шару (0.0–1.0) |
| `Density` | `float` | Базова щільність (0.0–1.0) |
| `ObjectId` | `string` | ID об'єкту (напр. `"mountain_large"`, `"mountain_small"`, `"rock"`) |

## Приклад налаштування

```
Layer 0: MinHeight=0.5, MaxHeight=0.7, Density=0.1, ObjectId="rock_small"
Layer 1: MinHeight=0.7, MaxHeight=0.85, Density=0.2, ObjectId="rock_large"
Layer 2: MinHeight=0.85, MaxHeight=1.0, Density=0.3, ObjectId="snow_peak"

Результат:
         🏔️🏔️🏔️          ← snow_peak (0.85–1.0, 30%)
       🪨🏔️🏔️🏔️🪨        ← rock_large (0.7–0.85, 20%)
     🪨🪨🪨🪨🪨🪨🪨      ← rock_small (0.5–0.7, 10%)
   grass grass grass grass    ← нижче 0.5 — без об'єктів
```
