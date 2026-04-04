# Graph System — Нодовий генератор карт

← [Назад до README](../../README.md) · [Стара pipeline-система →](../generator.md)

---

## Що це?

**Graph System** — це візуальний нодовий редактор для процедурної генерації 2D тайлових карт у грі **Moyva**. Замість написання коду, ви з'єднуєте вузли (ноди) у графі — кожен вузол виконує одну операцію (генерація шуму, розміщення лісів, прокладання доріг тощо), а система автоматично виконує їх у правильному порядку.

**Аналогія:** Уявіть Photoshop, де замість шарів і фільтрів — ноди. Один нод генерує ландшафт, інший додає ліси, третій прокладає дороги — і всі вони з'єднані стрілками, що показують потік даних.

```
┌──────────────┐     ┌──────────┐     ┌─────────────┐
│ Height Source ├────►│ Smooth   ├────►│ Height→Tile │
│  (Perlin)    │     │ (blur)   │     │  (tile IDs) │
└──────────────┘     └──────────┘     └──────┬──────┘
                                             │
                   ┌─────────────┐           │
                   │ Sea/Coast   │◄──────────┤
                   │ (берег)     │           │
                   └──────┬──────┘     ┌─────┴──────┐
                          │            │ Lake Gen   │
                          │            │ (озера)    │
                          ▼            └─────┬──────┘
                   ┌──────────────┐          │
                   │  Forest      │◄─────────┘
                   │  Cluster     │
                   └──────┬───────┘
                          │
                   ┌──────┴──────┐     ┌────────────┐
                   │   Output    │◄────┤ Road Path  │
                   │ (BiomeMap,  │     │ (A* дороги)│
                   │  ObjectMap, │     └────────────┘
                   │  HeightMap) │
                   └─────────────┘
```

---

## Навігація по документації

| Документ | Що містить |
|---|---|
| **Ви тут** | Загальний огляд системи |
| [Швидкий старт](quickstart.md) | Покроковий гайд: створити перший граф за 5 хвилин |
| [Редактор графів](editor-guide.md) | Як користуватися графовим редактором (тулбар, гарячі клавіші, міні-карта тощо) |
| [API та архітектура](api-reference.md) | Базові класи, інтерфейси, як створювати власні ноди |
| [Довідник нодів: Генератори](nodes-generators.md) | HeightSource, BiomeResolver, FertilityMap, Voronoi |
| [Довідник нодів: Обробка](nodes-processing.md) | Smooth, Erosion, Terrace, NoiseCombiner, CellularAutomata |
| [Довідник нодів: Терейн](nodes-terrain.md) | SeaCoastline, AutoTileTransition |
| [Довідник нодів: Особливості](nodes-features.md) | Lake, River, Forest, Mountain, POI, Road, MultiLayerScatter, RandomScatter |
| [Довідник нодів: Утиліти](nodes-utility.md) | Mask, BorderFrame, Overlay, TileReplace, ConditionalSwitch, MapScale, Output, Debug |
| [Довідник нодів: WFC](nodes-wfc.md) | WaveFunctionCollapse, ConstraintPolish |
| [Довідник нодів: Аналіз](nodes-analysis.md) | ChokepointAnalyzer, FertilityMap |

---

## Три шари даних карти

Вся система працює з **трьома основними типами карти**, які проходять через граф від джерела до виходу:

| Тип даних | C# тип | Колір порту | Опис |
|---|---|---|---|
| **HeightMap** | `float[,]` | 🟢 Зелений | Карта висот. Значення від 0.0 (найнижче) до 1.0 (найвище). Визначає рельєф: долини, пагорби, гори |
| **TileMap / BiomeMap** | `string[,]` | 🔵 Блакитний | Карта тайлів. Кожна клітинка містить текстовий ID тайлу: `"grass"`, `"water_deep"`, `"mountain"` тощо |
| **ObjectMap** | `string[,]` | 🔵 Блакитний | Карта об'єктів. ID об'єктів поверх тайлів: `"tree_oak"`, `"rock"`, `"village"` тощо |

Додаткові типи:

| Тип даних | C# тип | Колір порту | Опис |
|---|---|---|---|
| **Mask** | `bool[,]` | 🟡 Жовтий | Маска true/false. Використовується для обмеження зони дії нодів |
| **IntMap** | `int[,]` | 🟠 Оранжевий | Цілочисельна карта. POI ID, регіони Вороного тощо |
| **Скаляр** | `int` / `float` | ⚪ Сірий | Одне число: ширина карти, порогове значення тощо |
| **Вектор** | `Vector2Int` | 🟣 Фіолетовий | Двовимірне ціле число (координати) |

---

## Як система працює (для програмістів)

### 1. Граф як ScriptableObject

Граф зберігається як **GraphAsset** — ScriptableObject, що містить список вузлів та з'єднань. Кожен вузол теж є окремим ScriptableObject (sub-asset), що дозволяє редагувати параметри вузла прямо в Unity Inspector.

### 2. Топологічне сортування

Перед виконанням, **GraphRunner** визначає правильний порядок — алгоритм Кана (Kahn's algorithm) гарантує, що кожен вузол виконується лише після всіх своїх залежностей:

```
HeightSource (0 залежностей) → виконується першим
    ↓
Smooth (залежить від HeightSource) → другим
    ↓
HeightToTile (залежить від Smooth) → третім
    ↓
...і так далі
```

### 3. Виконання

Для кожного вузла в порядку:
1. Збираються значення з input-портів (з кешу результатів попередніх вузлів)
2. Викликається `Execute(inputs, context)` → повертається `NodeOutput`
3. Результат кешується для наступних вузлів

### 4. Інтеграція з грою

**GraphBasedMapDataGenerator** — адаптер, що:
- Отримує залежності через Zenject (INoiseProvider, IRiverPathfinder, тощо)
- Реєструє їх у `NodeContext` як сервіси
- Запускає `GraphRunner.Execute()`
- Читає результат з `OutputNode`: BiomeMap + ObjectMap + HeightMap
- Передає результат далі для побудови ігрового світу

Перемикання між старою pipeline та графовим генератором — один чекбокс у `GeneratorInstaller`:

```csharp
// GeneratorInstaller.cs
[SerializeField] private bool _useGraphGenerator;
```

---

## Що нового порівняно з main

Ця гілка (`generator-modify`) додає **126 файлів** та **5867 рядків** нового коду:

### Коміт 1: `d4e2970` — Ядро Graph System

| Категорія | Файли | Опис |
|---|---|---|
| **GraphSystem API** | 11 файлів | NodeBase, GraphAsset, Connection, PortDefinition, NodeContext, NodeOutput, NodeInfoAttribute, interfaces |
| **GraphSystem Runtime** | 2 файли | GraphRunner (топологічне сортування + виконання), GraphValidator (цикли, типи, порти) |
| **GraphSystem Editor** | 7 файлів | GraphEditorWindow, GeneratorGraphView, GeneratorNodeView, GeneratorPort, NodeSearchProvider, NodeBaseEditor, GraphAssetEditor |
| **Assembly definitions** | 2 файли | Kruty1918.Moyva.GraphSystem.asmdef, Kruty1918.Moyva.GraphSystem.Editor.asmdef |
| **Базові ноди** | 11 файлів | HeightSource, BiomeResolver, HeightToTile, Smooth, Erosion, River, WaterSmooth, ConstraintPolish, Overlay, Mask, Output |
| **WFC ноди** | 2 файли | WaveFunctionCollapseNode, WFCAlgorithm |
| **Інтеграція** | 1 файл | GraphBasedMapDataGenerator |

### Коміт 2: `9f42407` — 20 нових нодів + 7 покращень редактора

| Категорія | Файли | Опис |
|---|---|---|
| **Утилітні ноди** | 10 | TileReplace, NoiseCombiner, Terrace, RandomScatter, BorderFrame, VoronoiRegions, CellularAutomata, MapScale, ConditionalSwitch, DebugPreview |
| **Терейн-ноди** | 10 | AutoTileTransition, ForestCluster, LakeGeneration, SeaCoastline, POIPlacement, RoadPath, MountainScatter, FertilityMap, ChokepointAnalyzer, MultiLayerScatter |
| **Покращення редактора** | 3 файли змінено | Міні-карта, Copy/Paste, Groups, Sticky Notes, Auto-Layout, Run з таймінгом, тултіпи портів |

---

## Повний список нодів (31 штука)

| # | Нод | Категорія | Коротко | Детальна документація |
|---|---|---|---|---|
| 1 | Height Source | Generators | Генерує карту висот через Perlin noise | [→](nodes-generators.md#height-source) |
| 2 | Biome Resolver | Generators | Призначає біоми за висотою та вологістю | [→](nodes-generators.md#biome-resolver) |
| 3 | Fertility Map | Generators | Карта родючості (шум + висота + вода) | [→](nodes-generators.md#fertility-map) |
| 4 | Voronoi Regions | Generators | Діаграма Вороного з регіонами | [→](nodes-generators.md#voronoi-regions) |
| 5 | Height To Tile | Converters | Перетворює висоти в ID тайлів | [→](nodes-generators.md#height-to-tile) |
| 6 | Noise Combiner | Processing | Комбінує дві карти висот (Add/Mul/Lerp) | [→](nodes-processing.md#noise-combiner) |
| 7 | Smooth | Processing | Згладжування (Box blur) | [→](nodes-processing.md#smooth) |
| 8 | Terrace | Processing | Терасування — створює плато | [→](nodes-processing.md#terrace) |
| 9 | Erosion | Processing | Термальна ерозія | [→](nodes-processing.md#erosion) |
| 10 | Cellular Automata | Processing | Клітинний автомат (для печер/островів) | [→](nodes-processing.md#cellular-automata) |
| 11 | Sea / Coastline | Terrain | Море, берегова лінія, пляж | [→](nodes-terrain.md#sea--coastline) |
| 12 | AutoTile Transitions | Terrain | Автоматичні переходи між тайлами | [→](nodes-terrain.md#autotile-transitions) |
| 13 | Lake Generation | Features | Озера з flood-fill | [→](nodes-features.md#lake-generation) |
| 14 | River Generator | Features | Річки через A* pathfinding | [→](nodes-features.md#river-generator) |
| 15 | Water Smooth | Features | Згладжування водних переходів | [→](nodes-features.md#water-smooth) |
| 16 | Forest Cluster | Features | Ліси з кластеризацією | [→](nodes-features.md#forest-cluster) |
| 17 | Mountain Scatter | Features | Гірські об'єкти за висотою | [→](nodes-features.md#mountain-scatter) |
| 18 | Multi-Layer Scatter | Features | Багатошаровий розкид об'єктів | [→](nodes-features.md#multi-layer-scatter) |
| 19 | Random Scatter | Features | Рівномірний випадковий розкид | [→](nodes-features.md#random-scatter) |
| 20 | POI Placement | Features | Розміщення ключових точок (села, замки) | [→](nodes-features.md#poi-placement) |
| 21 | Road / Path | Features | Дороги між POI через A* + MST | [→](nodes-features.md#road--path) |
| 22 | Chokepoint Analyzer | Analysis | Аналіз вузьких проходів та оборонних позицій | [→](nodes-analysis.md#chokepoint-analyzer) |
| 23 | Map Scale | Utility | Масштабування карти | [→](nodes-utility.md#map-scale) |
| 24 | Mask | Utility | Порогова маска з HeightMap | [→](nodes-utility.md#mask) |
| 25 | Border Frame | Utility | Маска рамки по краях карти | [→](nodes-utility.md#border-frame) |
| 26 | Overlay | Utility | Накладання тайлів через маску | [→](nodes-utility.md#overlay) |
| 27 | Tile Replace | Utility | Пошук і заміна тайлів | [→](nodes-utility.md#tile-replace) |
| 28 | Conditional Switch | Logic | Вибір між двома картами за маскою | [→](nodes-utility.md#conditional-switch) |
| 29 | Output | Core | Фінальний вузол — збирає результат | [→](nodes-utility.md#output) |
| 30 | Debug Preview | Debug | Логування статистики, pass-through | [→](nodes-utility.md#debug-preview) |
| 31 | WFC Generator | WFC | Wave Function Collapse — генерація з прикладу | [→](nodes-wfc.md#wfc-generator) |
| 32 | WFC Constraint Polish | WFC | WFC-полірування переходів | [→](nodes-wfc.md#constraint-polish) |

---

## Швидке посилання для дизайнерів

Якщо ви хочете:
- **Створити карту з нуля** → [Швидкий старт](quickstart.md)
- **Зрозуміти редактор** → [Гайд по редактору](editor-guide.md)
- **Додати ліси на карту** → [Forest Cluster](nodes-features.md#forest-cluster)
- **Зробити острів з морем** → [Sea/Coastline](nodes-terrain.md#sea--coastline)
- **Розмістити села та замок** → [POI Placement](nodes-features.md#poi-placement)
- **З'єднати села дорогами** → [Road/Path](nodes-features.md#road--path)
- **Написати свій нод** → [API Reference](api-reference.md#як-створити-свій-нод)
