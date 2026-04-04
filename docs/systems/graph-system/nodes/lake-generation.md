# Lake Generation

← [Особливості](../nodes-features.md) · [Graph System](../README.md)

**Категорія:** Features · **Файл:** `LakeGenerationNode.cs`

Генерує **озера** за допомогою flood-fill алгоритму — знаходить низини на карті та заповнює їх водою. Створює три зони: глибоке, мілке та берегове.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🔵 | BiomeMap | `string[,]` | Базова карта біомів |
| **Output** 🔵 | BiomeMap | `string[,]` | Карта з озерами |
| **Output** 🟡 | WaterMask | `bool[,]` | Маска водних клітинок |

## Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Water Level** | `float` | 0.3 | Поріг висоти — все нижче стає водою (0.0–0.5) |
| **Min Lake Size** | `int` | 4 | Мінімальний розмір озера в тайлах (1–10) |
| **Deep Water Tile** | `string` | `"water_deep"` | ID глибокої води |
| **Shallow Water Tile** | `string` | `"water_shallow"` | ID мілкої води |
| **Shallow Depth** | `float` | 0.05 | Глибина, нижче якої вода вважається мілкою (0.0–0.1) |
| **Shore Tile** | `string` | `"shore"` | ID берегового тайлу |

## Алгоритм

```
1. Знайти всі клітинки де height < waterLevel
2. BFS flood-fill — об'єднати в групи (кожна група = потенційне озеро)
3. Відкинути групи менші за minLakeSize
4. Для кожної клітинки озера:
   - Центр (далеко від берега) → deepWaterTile
   - Ближче до берега → shallowWaterTile
   - Перший ряд біля суші → shoreTile
5. Сформувати WaterMask (true для всіх водних клітинок)
```

## Візуалізація

```
HeightMap (висоти):          Результат:
0.5 0.4 0.3 0.4 0.5        grass  grass  shore  grass  grass
0.4 0.2 0.1 0.2 0.4        grass  shal.  deep   shal.  grass
0.3 0.1 0.0 0.1 0.3   →    shore  deep   deep   deep   shore
0.4 0.2 0.1 0.2 0.4        grass  shal.  deep   shal.  grass
0.5 0.4 0.3 0.4 0.5        grass  grass  shore  grass  grass
```

> **Порада:** Використовуйте `WaterMask` вихід для подальших нодів — `FertilityMap` (бонус біля води) та `ForestCluster` (уникнення води).
