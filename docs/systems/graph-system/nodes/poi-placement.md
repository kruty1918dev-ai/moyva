# POI Placement

← [Особливості](../nodes-features.md) · [Graph System](../README.md)

**Категорія:** Features · **Файл:** `POIPlacementNode.cs`

Розміщує **Points of Interest** (ключові точки) — села, замки, рудники, торгові пости — на оптимальних позиціях. Використовує систему скорінгу для вибору найкращих місць.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🟡 | WaterMask | `bool[,]` | (Опціонально) Маска водних клітинок |
| **Input** 🔵 | ObjectMap | `string[,]` | (Опціонально) Існуюча карта об'єктів |
| **Output** 🔵 | ObjectMap | `string[,]` | Об'єкти POI на карті |
| **Output** 🟠 | POIMap | `int[,]` | ID розміщених POI (0 = немає, 1+ = POI id) |

## Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Rules** | `POIRule[]` | Масив правил розміщення |
| **Min Distance** | `int` | Мінімальна відстань між будь-якими POI |

## Структура `POIRule`

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

## Алгоритм скорінгу

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

## Greedy Placement

POI розміщуються **жадібно** (greedy):

1. Обчислити скор для всіх клітинок
2. Обрати клітинку з найвищим скором
3. Розмістити POI, "заблокувати" клітинки в радіусі `minDistance`
4. Повторити для наступної POI

## Приклад налаштування

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
