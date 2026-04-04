# Forest Cluster

← [Особливості](../nodes-features.md) · [Graph System](../README.md)

**Категорія:** Features · **Файл:** `ForestClusterNode.cs`

Розміщує **ліси** кластерами, використовуючи Perlin noise для визначення щільності. Враховує висоту: ліси ростуть не на воді і не на високих горах. Опціонально підтримує маску для обмеження зон розміщення.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🔵 | BiomeMap | `string[,]` | Карта біомів (буде модифікована) |
| **Input** 🟡 | Mask | `bool[,]` | (Опціонально) Маска дозволених зон для лісу |
| **Output** 🔵 | BiomeMap | `string[,]` | Карта з лісовими тайлами |
| **Output** 🔵 | ObjectMap | `string[,]` | Карта з об'єктами дерев |

## Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Density Noise** | `DataNoiseSettings` | — | (Опціонально) SO для шуму щільності. Якщо не задано — використовується простий Perlin noise |
| **Density Threshold** | `float` | 0.45 | Поріг шуму для розміщення лісу (нижче = більше лісу) |
| **Min Height** | `float` | 0.25 | Мінімальна висота для лісу |
| **Max Height** | `float` | 0.65 | Максимальна висота для лісу |
| **Tree Objects** | `string[]` | `["tree_oak", "tree_pine", "tree_birch"]` | Масив ID об'єктів дерев (вибираються випадково) |
| **Dense Forest Tile** | `string` | `"forest_dense"` | ID тайлу густого лісу |
| **Sparse Forest Tile** | `string` | `"forest_sparse"` | ID тайлу рідкого лісу |
| **Dense Threshold** | `float` | 0.65 | Поріг шуму для густого лісу (вище = густий) |
| **Seed** | `int` | 42 | Зерно для вибору типу дерева |

## Алгоритм

```
Для кожної клітинки:
  1. height = heightMap[x, y]
  2. Якщо height < minHeight або height > maxHeight → пропустити (немає лісу)
  3. noise = PerlinNoise(x/noiseScale, y/noiseScale)
  4. Якщо noise > densityThreshold + 0.15:
     → TileMap = denseForestTile, ObjectMap = treeObject  (густий ліс)
  5. Якщо noise > densityThreshold:
     → TileMap = sparseForestTile  (рідкий ліс)
  6. Інакше → не змінювати
```

## Візуалізація

```
Perlin noise clusters:         Результат на карті:
░░░░░░░░░░                    ░░░░░░░░░░
░░▒▓█▓▒░░░                    ░░🌳🌲🌲🌳░░░
░▒▓███▓▒░░                    ░🌳🌲🌲🌲🌳░░
░░▒▓█▓▒░░░     →              ░░🌳🌲🌲🌳░░░
░░░░░░░░░░                    ░░░░░░░░░░
░░░▒▓▓▒░░░                    ░░░🌳🌲🌳░░░
░░░░▒▒░░░░                    ░░░░🌳🌳░░░░

▓█ = густий ліс, 🌲 = dense_forest + tree
▒  = рідкий ліс, 🌳 = sparse_forest
```
