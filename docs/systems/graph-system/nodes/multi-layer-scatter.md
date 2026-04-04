# Multi-Layer Scatter

← [Особливості](../nodes-features.md) · [Graph System](../README.md)

**Категорія:** Features · **Файл:** `MultiLayerScatterNode.cs`

Універсальний нод для **багатошарового розкиду об'єктів** — кожен шар має свій діапазон висоти, тип об'єкта та кластеризацію через Perlin noise.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🔵 | ObjectMap | `string[,]` | Існуюча карта об'єктів |
| **Input** 🟡 | Mask | `bool[,]` | (Опціонально) Маска дозволених зон |
| **Output** 🔵 | ObjectMap | `string[,]` | Карта з доданими об'єктами |

## Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Layers** | `ScatterLayer[]` | Масив шарів |
| **Seed** | `int` | Зерно для генерації (за замовч. 42) |
| **Avoid Existing Objects** | `bool` | Чи уникати клітинок де вже є об'єкти (за замовч. true) |

## Структура `ScatterLayer`

| Поле | Тип | За замовч. | Опис |
|---|---|---|---|
| `ObjectId` | `string` | — | ID об'єкта для розміщення |
| `MinHeight` | `float` | 0 | Мінімальна висота |
| `MaxHeight` | `float` | 1 | Максимальна висота |
| `Density` | `float` | 0.1 | Базова щільність (0–1) |
| `ClusterStrength` | `float` | 0.5 | Сила кластеризації (0 = рівномірно, 1 = сильні кластери) |

## Різниця з MountainScatter

| | MountainScatter | MultiLayerScatter |
|---|---|---|
| **Кластеризація** | Ні (рівномірний розкид) | Так (Perlin noise) |
| **Scale with height** | Так (density * height) | Ні (фіксована density) |
| **Призначення** | Гірські об'єкти | Будь-які об'єкти |

## Приклад: Квіти на рівнинах + гриби в лісах

```
Layer 0: ObjectId="flower",   MinHeight=0.25, MaxHeight=0.45, Density=0.15, Cluster=0.6
Layer 1: ObjectId="mushroom", MinHeight=0.3,  MaxHeight=0.55, Density=0.08, Cluster=0.8
```
