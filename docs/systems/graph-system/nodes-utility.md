# Довідник нодів: Утиліти, Логіка, Вихід, Дебаг

← [Назад до Graph System](README.md)

---

Допоміжні ноди, що виконують трансформації, маскування, масштабування та фінальний збір результатів.

---

## Map Scale

**Категорія:** Utility · **Файл:** `MapScaleNode.cs`

Масштабує будь-яку карту до іншого розміру за допомогою **білінійної інтерполяції** або **Nearest Neighbor**.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Вхідна карта |
| **Input** ⚪ | TargetWidth | `int` | Бажана ширина |
| **Input** ⚪ | TargetHeight | `int` | Бажана висота |
| **Output** 🟢 | ScaledMap | `float[,]` | Масштабована карта |

### Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Interpolation** | `enum` | Bilinear | Bilinear (плавне) або NearestNeighbor (різке) |

### Коли використовувати

- **Попередній перегляд:** Генеруйте на малому розмірі (32×32), потім масштабуйте для перегляду
- **Деталізація:** Генеруйте грубу основу 64×64, масштабуйте до 256×256, потім додайте деталі
- **Nearest Neighbor** — зберігає різкі межі тайлів (для піксельної графіки)
- **Bilinear** — плавне масштабування для HeightMap

---

## Mask

**Категорія:** Utility · **Файл:** `MaskNode.cs`

Створює **булеву маску** (`bool[,]`) з карти висот за пороговою умовою. Маска визначає зони, де інші ноди можуть або не можуть діяти.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Output** 🟡 | Mask | `bool[,]` | Булева маска |

### Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Mode** | `enum` | GreaterThan | Тип порівняння |
| **Threshold** | `float` | 0.5 | Порогове значення |
| **Upper Threshold** | `float` | 0.8 | Верхній поріг (тільки для Between) |

### Режими

| Режим | Формула | Результат |
|---|---|---|
| **GreaterThan** | `height > threshold` | true де висота вище порогу |
| **LessThan** | `height < threshold` | true де висота нижче порогу |
| **Between** | `threshold < height < upperThreshold` | true в діапазоні |

### Приклад

```
HeightMap:                    Mask (GreaterThan, 0.5):
0.2  0.4  0.6  0.8          false false true  true
0.3  0.5  0.7  0.9     →    false false true  true
0.1  0.3  0.5  0.7          false false false true
```

### Ігрове застосування

- `GreaterThan 0.6` → маска "гірських зон" → для MountainScatter
- `LessThan 0.3` → маска "низин" → для LakeGeneration
- `Between 0.3–0.6` → маска "рівнин" → для ForestCluster або POI

---

## Border Frame

**Категорія:** Utility · **Файл:** `BorderFrameNode.cs`

Створює **маску рамки** по краях карти — true в центрі, false по периметру.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** ⚪ | MapWidth | `int` | Ширина карти |
| **Input** ⚪ | MapHeight | `int` | Висота карти |
| **Output** 🟡 | FrameMask | `bool[,]` | Маска: true = всередині, false = край |

### Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Thickness** | `int` | 2 | Товщина рамки в тайлах |

### Візуалізація (Thickness = 2)

```
0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0
0 0 1 1 1 1 0 0
0 0 1 1 1 1 0 0    0 = false (рамка)
0 0 1 1 1 1 0 0    1 = true  (внутрішня зона)
0 0 1 1 1 1 0 0
0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0
```

### Ігрове застосування

- Рамка з водою навколо карти (**острів**)
- Рамка з горами (**закрита долина**)
- Обмеження зони генерації — комбінуйте з `Overlay`

---

## Overlay

**Категорія:** Utility · **Файл:** `OverlayNode.cs`

Накладає **нову карту тайлів поверх базової**, використовуючи маску для вибору зон.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🔵 | BaseMap | `string[,]` | Базова карта тайлів |
| **Input** 🔵 | OverlayMap | `string[,]` | Карта для накладання |
| **Input** 🟡 | Mask | `bool[,]` | Маска: де true → використовуємо OverlayMap |
| **Output** 🔵 | ResultMap | `string[,]` | Результат |

### Формула

```
result[x,y] = mask[x,y] ? overlayMap[x,y] : baseMap[x,y]
```

### Приклад

```
BaseMap:      OverlayMap:     Mask:              Результат:
grass grass   water water    false false         grass grass
grass grass   water water    false true      →   grass water
grass grass   water water    true  true          water water
```

---

## Tile Replace

**Категорія:** Utility · **Файл:** `TileReplaceNode.cs`

Простий **пошук і заміна** — замінює всі входження одного тайлу на інший.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🔵 | TileMap | `string[,]` | Карта тайлів |
| **Output** 🔵 | TileMap | `string[,]` | Карта з замінами |

### Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Find Tile** | `string` | ID тайлу для пошуку |
| **Replace Tile** | `string` | Новий ID |

### Приклад

```
Find: "grass"  →  Replace: "meadow"

До:    grass  sand  grass  water
Після: meadow sand  meadow water
```

---

## Conditional Switch

**Категорія:** Logic · **Файл:** `ConditionalSwitchNode.cs`

**Мультиплексор** — обирає між двома картами тайлів попіксельно за маскою.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🔵 | TrueMap | `string[,]` | Карта для true-зон |
| **Input** 🔵 | FalseMap | `string[,]` | Карта для false-зон |
| **Input** 🟡 | Condition | `bool[,]` | Маска вибору |
| **Output** 🔵 | ResultMap | `string[,]` | Результат |

### Формула

```
result[x,y] = condition[x,y] ? trueMap[x,y] : falseMap[x,y]
```

### Приклад: Два біоми за висотою

```
HeightSource → Mask(>0.5) → ConditionalSwitch
                                ├── TrueMap  ← "mountain" (HeightToTile з гірськими налаштуваннями)
                                └── FalseMap ← "plains"   (HeightToTile з рівнинними налаштуваннями)
```

---

## Output

**Категорія:** Core · **Файл:** `OutputNode.cs`

**Фінальний вузол** графа — збирає всі три основні карти та передає їх в ігрову систему.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🔵 | BiomeMap | `string[,]` | Фінальна карта біомів/тайлів |
| **Input** 🔵 | ObjectMap | `string[,]` | Фінальна карта об'єктів |
| **Input** 🟢 | HeightMap | `float[,]` | Фінальна карта висот |

### Важливо

- **Кожен граф повинен мати рівно один Output нод**
- Якщо порт не підключений — повертається `null` (допустимо для ObjectMap)
- `GraphBasedMapDataGenerator` шукає саме цей нод для отримання результатів

---

## Debug Preview

**Категорія:** Debug · **Файл:** `DebugPreviewNode.cs`

**Pass-through** нод для дебагу — пропускає дані наскрізь, логуючи статистику в Console.

### Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | (опціонально) Карта висот для аналізу |
| **Input** 🔵 | TileMap | `string[,]` | (опціонально) Карта тайлів для аналізу |
| **Output** 🟢 | HeightMap | `float[,]` | Прохідне (те ж саме, що на вході) |
| **Output** 🔵 | TileMap | `string[,]` | Прохідне |

### Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Label** | `string` | `"Debug"` | Позначка для логу |

### Що логує

Для HeightMap:
```
[DebugPreview: "After Erosion"] HeightMap 64x64 | Min: 0.02 | Max: 0.95 | Avg: 0.47
```

Для TileMap:
```
[DebugPreview: "Final Tiles"] TileMap 64x64 | Unique tiles: 12 | Top: grass(450), water(320), mountain(120)
```

### Коли використовувати

- Вставте між нодами щоб перевірити проміжні результати
- Діагностика: "чому тут немає лісу?" → поставте Debug між HeightSource і ForestCluster
- Не впливає на результат — лише логує
