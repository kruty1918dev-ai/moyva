# Довідник нодів: Особливості (Features)

← [Назад до Graph System](README.md)

---

Ноди особливостей **додають ігрові елементи** на карту: озера, річки, ліси, гори, села, дороги. Вони працюють поверх базового ландшафту — приймають TileMap/HeightMap і розміщують нові об'єкти та модифікують тайли.

---

## Lake Generation

**Категорія:** Features · **Файл:** `LakeGenerationNode.cs`

Генерує **озера** за допомогою flood-fill алгоритму — знаходить низини на карті та заповнює їх водою. Створює три зони: глибоке, мілке та берегове.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🔵 | TileMap | `string[,]` | Базова карта тайлів |
| **Output** 🔵 | TileMap | `string[,]` | Карта з озерами |
| **Output** 🟡 | WaterMask | `bool[,]` | Маска водних клітинок |

### Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Water Level** | `float` | 0.3 | Поріг висоти — все нижче стає водою |
| **Min Lake Size** | `int` | 10 | Мінімальний розмір озера в тайлах |
| **Deep Water Tile** | `string` | `"water_deep"` | ID глибокої води |
| **Shallow Water Tile** | `string` | `"water_shallow"` | ID мілкої води |
| **Shore Tile** | `string` | `"water_shore"` | ID берега |
| **Shore Width** | `int` | 1 | Ширина берегової зони |

### Алгоритм

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

### Візуалізація

```
HeightMap (висоти):          Результат:
0.5 0.4 0.3 0.4 0.5        grass  grass  shore  grass  grass
0.4 0.2 0.1 0.2 0.4        grass  shal.  deep   shal.  grass
0.3 0.1 0.0 0.1 0.3   →    shore  deep   deep   deep   shore
0.4 0.2 0.1 0.2 0.4        grass  shal.  deep   shal.  grass
0.5 0.4 0.3 0.4 0.5        grass  grass  shore  grass  grass
```

> **Порада:** Використовуйте `WaterMask` вихід для подальших нодів — `FertilityMap` (бонус біля води) та `ForestCluster` (уникнення води).

---

## River Generator

**Категорія:** Features · **Файл:** `RiverNode.cs`

Генерує **річки**, делегуючи роботу зовнішньому сервісу `RiverFeatureGenerator` з A* pathfinding. Річки прокладаються від високих точок до низьких, обираючи найбільш природний шлях.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот (визначає шлях річки) |
| **Input** 🔵 | TileMap | `string[,]` | Базова карта тайлів |
| **Output** 🔵 | TileMap | `string[,]` | Карта з тайлами річок |

### Як працює

1. Сервіс `IRiverPathfinder` знаходить шлях від витоку до гирла через A*
2. При пошуку шляху, клітинки з меншою висотою мають менший "cost" → річка тече вниз
3. Тайли вздовж шляху змінюються на водні

> **Зверніть увагу:** Річка залежить від зовнішнього сервісу, який реєструється через `GraphBasedMapDataGenerator` та Zenject.

---

## Water Smooth

**Категорія:** Features · **Файл:** `WaterSmoothNode.cs`

Згладжує **переходи між водними та сухопутними тайлами**. Використовує зовнішній `WaterPostProcessor` для створення природних берегів.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🔵 | TileMap | `string[,]` | Карта з водними тайлами |
| **Output** 🔵 | TileMap | `string[,]` | Зглажена карта |

### Коли використовувати

Ставте **після** `Lake Generation` та `River Generator`, **перед** `Output`:

```
Lake Gen → River → Water Smooth → AutoTile → Output
```

---

## Forest Cluster

**Категорія:** Features · **Файл:** `ForestClusterNode.cs`

Розміщує **ліси** кластерами, використовуючи Perlin noise для визначення щільності. Враховує висоту: ліси ростуть не на воді і не на високих горах.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🔵 | TileMap | `string[,]` | Карта тайлів |
| **Input** 🔵 | ObjectMap | `string[,]` | Карта об'єктів |
| **Output** 🔵 | TileMap | `string[,]` | Карта з лісовими тайлами |
| **Output** 🔵 | ObjectMap | `string[,]` | Карта з об'єктами дерев |

### Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Density Threshold** | `float` | 0.5 | Поріг шуму для розміщення лісу (нижче = більше лісу) |
| **Noise Scale** | `float` | 30 | Масштаб Perlin noise (більше = більші кластери) |
| **Min Height** | `float` | 0.2 | Мінімальна висота для лісу |
| **Max Height** | `float` | 0.7 | Максимальна висота для лісу |
| **Dense Forest Tile** | `string` | `"forest_dense"` | ID тайлу густого лісу |
| **Sparse Forest Tile** | `string` | `"forest_sparse"` | ID тайлу рідкого лісу |
| **Tree Object** | `string` | `"tree_oak"` | ID об'єкту дерева |

### Алгоритм

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

### Візуалізація

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

---

## Mountain Scatter

**Категорія:** Features · **Файл:** `MountainScatterNode.cs`

Розміщує **гірські об'єкти** (скелі, валуни, снігові вершини) за шарами висоти. Чим вище точка — тим більша щільність гірських об'єктів.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🔵 | ObjectMap | `string[,]` | Карта об'єктів |
| **Output** 🔵 | ObjectMap | `string[,]` | Карта з гірськими об'єктами |

### Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Layers** | `MountainLayer[]` | Масив шарів з діапазонами висоти та об'єктами |

### Структура `MountainLayer`

| Поле | Тип | Опис |
|---|---|---|
| `MinHeight` | `float` | Мінімальна висота шару |
| `MaxHeight` | `float` | Максимальна висота шару |
| `Density` | `float` | Базова щільність (0.0–1.0) |
| `ObjectId` | `string` | ID об'єкту (напр. `"rock"`, `"boulder"`, `"snow_peak"`) |
| `ScaleWithHeight` | `bool` | Чи збільшувати щільність з висотою |

### Приклад налаштування

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

---

## Multi-Layer Scatter

**Категорія:** Features · **Файл:** `MultiLayerScatterNode.cs`

Універсальний нод для **багатошарового розкиду об'єктів** — кожен шар має свій діапазон висоти, тип об'єкта та кластеризацію через Perlin noise.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🔵 | ObjectMap | `string[,]` | Існуюча карта об'єктів |
| **Output** 🔵 | ObjectMap | `string[,]` | Карта з доданими об'єктами |

### Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Layers** | `ScatterLayer[]` | Масив шарів |
| **Base Noise Scale** | `float` | Масштаб Perlin noise для кластеризації |

### Структура `ScatterLayer`

| Поле | Тип | За замовч. | Опис |
|---|---|---|---|
| `ObjectId` | `string` | — | ID об'єкта для розміщення |
| `MinHeight` | `float` | 0 | Мінімальна висота |
| `MaxHeight` | `float` | 1 | Максимальна висота |
| `Density` | `float` | 0.1 | Базова щільність (0–1) |
| `ClusterStrength` | `float` | 0.5 | Сила кластеризації (0 = рівномірно, 1 = сильні кластери) |

### Різниця з MountainScatter

| | MountainScatter | MultiLayerScatter |
|---|---|---|
| **Кластеризація** | Ні (рівномірний розкид) | Так (Perlin noise) |
| **Scale with height** | Так (density * height) | Ні (фіксована density) |
| **Призначення** | Гірські об'єкти | Будь-які об'єкти |

### Приклад: Квіти на рівнинах + гриби в лісах

```
Layer 0: ObjectId="flower", MinHeight=0.25, MaxHeight=0.45, Density=0.15, Cluster=0.6
Layer 1: ObjectId="mushroom", MinHeight=0.3, MaxHeight=0.55, Density=0.08, Cluster=0.8
```

---

## Random Scatter

**Категорія:** Features · **Файл:** `RandomScatterNode.cs`

Найпростіший спосіб розмістити **об'єкти випадково** по карті. Рівномірний розподіл, без кластеризації.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🔵 | ObjectMap | `string[,]` | Існуюча карта об'єктів |
| **Input** 🟡 | Mask | `bool[,]` | (Опціонально) Маска дозволених зон |
| **Output** 🔵 | ObjectMap | `string[,]` | Карта з доданими об'єктами |

### Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Object Id** | `string` | `"stone"` | ID об'єкта |
| **Density** | `float` | 0.05 | Ймовірність розміщення (0.05 = 5% клітинок) |

### Коли використовувати

- Декоративні елементи (камінці, квіточки, гілки)
- Ресурси з рівномірним розподілом
- В комбінації з `Mask` — розмістити тільки на певних тайлах

---

## POI Placement

**Категорія:** Features · **Файл:** `POIPlacementNode.cs`

Розміщує **Points of Interest** (ключові точки) — села, замки, рудники, торгові пости — на оптимальних позиціях. Використовує систему скорінгу для вибору найкращих місць.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🟡 | WaterMask | `bool[,]` | Маска водних клітинок |
| **Output** 🟠 | POIMap | `int[,]` | ID розміщених POI (-1 = немає) |
| **Output** 🔵 | ObjectMap | `string[,]` | Об'єкти POI на карті |

### Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Rules** | `POIRule[]` | Масив правил розміщення |
| **Min Distance** | `int` | Мінімальна відстань між будь-якими POI |

### Структура `POIRule`

| Поле | Тип | Опис |
|---|---|---|
| `ObjectId` | `string` | ID об'єкта (наприклад, `"village"`) |
| `Count` | `int` | Кількість для розміщення |
| `MinHeight` | `float` | Мінімальна висота |
| `MaxHeight` | `float` | Максимальна висота |
| `PreferFlat` | `bool` | Чи надавати перевагу пласким ділянкам |
| `PreferNearWater` | `bool` | Чи надавати перевагу місцям біля води |
| `FlatnessWeight` | `float` | Вага "пласкості" в скорінгу |
| `WaterWeight` | `float` | Вага "близькості до води" |
| `RandomWeight` | `float` | Вага випадковості |

### Алгоритм скорінгу

```
Для кожної клітинки-кандидата:

score = flatnessScore * flatnessWeight      ← наскільки рівна місцевість
      + waterScore    * waterWeight          ← наскільки близька до води
      + randomScore   * randomWeight         ← випадковість для різноманіття

де:
  flatnessScore = 1.0 - дисперсія висот в радіусі 3 клітинки
  waterScore    = 1.0 - (BFS відстань до води / maxDistance)  
  randomScore   = Random(0, 1)
```

### Greedy Placement

POI розміщуються **жадібно** (greedy): 
1. Обчислити скор для всіх клітинок
2. Обрати клітинку з найвищим скором
3. Розмістити POI, "заблокувати" клітинки в радіусі `minDistance`
4. Повторити для наступної POI

### Приклад налаштування

```
Rule 0: "village",  Count=4, PreferFlat=true, PreferNearWater=true
Rule 1: "castle",   Count=1, PreferFlat=true, PreferNearWater=false (висота 0.6–0.8)
Rule 2: "mine",     Count=2, PreferFlat=false, MinHeight=0.7 (в горах)

MinDistance: 10

Результат:
     🏰                    ← castle на пагорбі
  🏘️     ⛏️                ← village біля води, mine в горах
     ~~~~~                 ← вода
  🏘️     🏘️                ← села на рівнинах біля води
         ⛏️                ← mine в горах
     🏘️                    ← четверте село
```

---

## Road / Path

**Категорія:** Features · **Файл:** `RoadPathNode.cs`

Прокладає **дороги між POI**, використовуючи A* pathfinding та мінімальне остовне дерево (Prim's MST) для оптимальної мережі.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот (для розрахунку вартості шляху) |
| **Input** 🔵 | TileMap | `string[,]` | Базова карта тайлів |
| **Input** 🟡 | WaterMask | `bool[,]` | Маска води (дорога уникає води) |
| **Input** 🟠 | POIMap | `int[,]` | Карта POI (від POIPlacement) |
| **Output** 🔵 | TileMap | `string[,]` | Карта з дорогами |

### Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Road Tile** | `string` | `"road"` | ID тайлу дороги |
| **Slope Penalty** | `float` | 2.0 | Штраф за різницю висот (вище = дорога обходить гори) |
| **Water Penalty** | `float` | 10.0 | Штраф за водні клітинки (вище = дорога обходить воду) |

### Алгоритм

```
1. Знайти всі POI точки з POIMap
2. Побудувати граф відстаней між усіма парами POI
3. Prim's MST — знайти мінімальний набір з'єднань
   (кожне село досяжне, але без зайвих доріг)
4. Для кожної пари з'єднаних POI:
   A* pathfinding з cost функцією:
     cost(a→b) = 1.0
              + abs(height[a] - height[b]) * slopePenalty
              + (waterMask[b] ? waterPenalty : 0)
5. Тайли вздовж шляху → roadTile
```

### Візуалізація

```
🏘️─ ─ ─ ─ ─ ─ ─🏰      POI з'єднання (MST)
│                 │
│    🏔️🏔️🏔️         ← гори — дорога обходить
│   ╱        ╲
🏘️─╱──────────╲─🏘️    A* прокладає шлях навколо гір
                │
               ⛏️
```

### DuplicateKeyComparer

Використовує спеціальний компаратор для `SortedList` в A*, що дозволяє однакові пріоритети (без нього C# кидає виключення на дублікати ключів).

---

## Каскадне з'єднання Features

Для повноцінної карти з'єднайте ноди Features у правильному порядку:

```
HeightSource → Smooth → HeightToTile
                                ↓
              Sea/Coastline ←───┤
                    ↓           ↓
              Lake Gen    ←─── HeightMap
                    ↓
              River ← ──────── HeightMap
                    ↓
              Water Smooth
                    ↓
              Forest Cluster ←── HeightMap
                    ↓
              Mountain Scatter ←─ HeightMap
                    ↓
              POI Placement ←──── HeightMap + WaterMask
                    ↓
              Road/Path ←──────── HeightMap + WaterMask + POIMap
                    ↓
              AutoTile Transition
                    ↓
                 Output
```
