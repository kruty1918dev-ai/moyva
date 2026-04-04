# Довідник нодів: Особливості (Features)

← [Назад до Graph System](README.md)

---

Ноди особливостей **додають ігрові елементи** на карту: озера, річки, ліси, гори, села, дороги. Вони працюють поверх базового ландшафту — приймають TileMap/HeightMap і розміщують нові об'єкти та модифікують тайли.

## Ноди цієї категорії

| Нод | Файл | Що робить |
|---|---|---|
| [Lake Generation](nodes/lake-generation.md) | `LakeGenerationNode.cs` | Озера через flood-fill |
| [River Generator](nodes/river-generator.md) | `RiverNode.cs` | Річки через A* pathfinding |
| [Water Smooth](nodes/water-smooth.md) | `WaterSmoothNode.cs` | Згладжування водних переходів |
| [Forest Cluster](nodes/forest-cluster.md) | `ForestClusterNode.cs` | Ліси кластерами через Perlin noise |
| [Mountain Scatter](nodes/mountain-scatter.md) | `MountainScatterNode.cs` | Гірські об'єкти за шарами висоти |
| [Multi-Layer Scatter](nodes/multi-layer-scatter.md) | `MultiLayerScatterNode.cs` | Багатошаровий розкид об'єктів |
| [Random Scatter](nodes/random-scatter.md) | `RandomScatterNode.cs` | Рівномірний випадковий розкид |
| [POI Placement](nodes/poi-placement.md) | `POIPlacementNode.cs` | Розміщення ключових точок (села, замки) |
| [Road / Path](nodes/road-path.md) | `RoadPathNode.cs` | Дороги між POI через A* + MST |

---

## Lake Generation

**Категорія:** Features · **Файл:** `LakeGenerationNode.cs`

Генерує **озера** за допомогою flood-fill алгоритму — знаходить низини на карті та заповнює їх водою. Створює три зони: глибоке, мілке та берегове.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🔵 | BiomeMap | `string[,]` | Базова карта біомів |
| **Output** 🔵 | BiomeMap | `string[,]` | Карта з озерами |
| **Output** 🟡 | WaterMask | `bool[,]` | Маска водних клітинок |

### Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Water Level** | `float` | 0.3 | Поріг висоти — все нижче стає водою (0.0–0.5) |
| **Min Lake Size** | `int` | 4 | Мінімальний розмір озера в тайлах (1–10) |
| **Deep Water Tile** | `string` | `"water_deep"` | ID глибокої води |
| **Shallow Water Tile** | `string` | `"water_shallow"` | ID мілкої води |
| **Shallow Depth** | `float` | 0.05 | Глибина, нижче якої вода вважається мілкою (0.0–0.1) |
| **Shore Tile** | `string` | `"shore"` | ID берегового тайлу |

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
| **Input** 🔵 | BiomeMap | `string[,]` | Базова карта біомів |
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот (визначає шлях річки) |
| **Output** 🔵 | BiomeMap | `string[,]` | Карта з тайлами річок |
| **Output** 🔵 | ObjectMap | `string[,]` | Карта з об'єктами вздовж річки (мости тощо) |

### Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **River Config** | `RiverDataConfig` (SO) | ScriptableObject з налаштуваннями річок |

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
| **Input** 🔵 | BiomeMap | `string[,]` | Карта біомів з водними тайлами |
| **Input** 🔵 | ObjectMap | `string[,]` | (Опціонально) Карта об'єктів |
| **Input** 🟢 | HeightMap | `float[,]` | (Опціонально) Карта висот |
| **Output** 🔵 | BiomeMap | `string[,]` | Зглажена карта біомів |

### Коли використовувати

Ставте **після** `Lake Generation` та `River Generator`, **перед** `Output`:

```
Lake Gen → River → Water Smooth → AutoTile → Output
```

---

## Forest Cluster

**Категорія:** Features · **Файл:** `ForestClusterNode.cs`

Розміщує **ліси** кластерами, використовуючи Perlin noise для визначення щільності. Враховує висоту: ліси ростуть не на воді і не на високих горах. Опціонально підтримує маску для обмеження зон розміщення.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🔵 | BiomeMap | `string[,]` | Карта біомів (буде модифікована) |
| **Input** 🟡 | Mask | `bool[,]` | (Опціонально) Маска дозволених зон для лісу |
| **Output** 🔵 | BiomeMap | `string[,]` | Карта з лісовими тайлами |
| **Output** 🔵 | ObjectMap | `string[,]` | Карта з об'єктами дерев |

### Параметри

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
| **Seed** | `int` | Зерно для генерації (за замовч. 42) |
| **Avoid Existing Objects** | `bool` | Чи уникати клітинок де вже є об'єкти (за замовч. true) |

### Структура `MountainLayer`

| Поле | Тип | Опис |
|---|---|---|
| `MinHeight` | `float` | Мінімальна висота шару (0.0–1.0) |
| `MaxHeight` | `float` | Максимальна висота шару (0.0–1.0) |
| `Density` | `float` | Базова щільність (0.0–1.0) |
| `ObjectId` | `string` | ID об'єкту (напр. `"mountain_large"`, `"mountain_small"`, `"rock"`) |

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
| **Input** 🟡 | Mask | `bool[,]` | (Опціонально) Маска дозволених зон |
| **Output** 🔵 | ObjectMap | `string[,]` | Карта з доданими об'єктами |

### Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Layers** | `ScatterLayer[]` | Масив шарів |
| **Seed** | `int` | Зерно для генерації (за замовч. 42) |
| **Avoid Existing Objects** | `bool` | Чи уникати клітинок де вже є об'єкти (за замовч. true) |

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
| **Input** 🟡 | Mask | `bool[,]` | (Опціонально) Маска дозволених зон |
| **Input** ⚪ | MapWidth | `int` | Ширина карти (використовується якщо Mask не підключено) |
| **Input** ⚪ | MapHeight | `int` | Висота карти (використовується якщо Mask не підключено) |
| **Output** 🔵 | ObjectMap | `string[,]` | Карта з розміщеними об'єктами |

### Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Density** | `float` | 0.1 | Ймовірність розміщення на кожній клітинці (0.1 = 10%) |
| **Seed** | `int` | 0 | Зерно генератора випадкових чисел |
| **Object Id** | `string` | `"tree"` | ID об'єкта для розміщення |

### Як працює

Розмір карти визначається з маски (якщо підключена) або з портів `MapWidth`/`MapHeight`. Для кожної клітинки — простий кидок кубика: `random < density → розмістити`.

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
| **Input** 🟡 | WaterMask | `bool[,]` | (Опціонально) Маска водних клітинок |
| **Input** 🔵 | ObjectMap | `string[,]` | (Опціонально) Існуюча карта об'єктів |
| **Output** 🔵 | ObjectMap | `string[,]` | Об'єкти POI на карті |
| **Output** 🟠 | POIMap | `int[,]` | ID розміщених POI (0 = немає, 1+ = POI id) |

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
| **Input** 🔵 | BiomeMap | `string[,]` | Базова карта біомів |
| **Input** 🟠 | POIMap | `int[,]` | Карта POI (від POIPlacement) |
| **Input** 🟡 | WaterMask | `bool[,]` | (Опціонально) Маска води (дорога уникає води) |
| **Output** 🔵 | BiomeMap | `string[,]` | Карта з дорогами |
| **Output** 🟡 | RoadMask | `bool[,]` | Маска доріг (true = дорога) |

### Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Road Tile** | `string` | `"road"` | ID тайлу дороги |
| **Hill Penalty** | `float` | 5.0 | Штраф за різницю висот (0.5–20, вище = дорога обходить гори) |
| **Water Penalty** | `float` | 30.0 | Штраф за водні клітинки (0.5–50, вище = дорога обходить воду) |
| **Water Tiles** | `string[]` | `["water_deep", "water_shallow", "sea"]` | Список ID водних тайлів |
| **Connect All POIs** | `bool` | `true` | Якщо true — MST (мінімальне остовне дерево), інакше послідовне з'єднання |
| **Road Width** | `int` | 1 | Ширина дороги в тайлах (1–3) |

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
