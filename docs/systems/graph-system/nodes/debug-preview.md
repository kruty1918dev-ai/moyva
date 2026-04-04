# Debug Preview

← [Утиліти](../nodes-utility.md) · [Graph System](../README.md)

**Категорія:** Debug · **Файл:** `DebugPreviewNode.cs`

**Pass-through** нод для дебагу — пропускає дані наскрізь, логуючи статистику в Console. Приймає три типи даних одночасно.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | (опціонально) Карта висот для аналізу |
| **Input** 🔵 | TileMap | `string[,]` | (опціонально) Карта тайлів для аналізу |
| **Input** 🟡 | Mask | `bool[,]` | (опціонально) Булева маска для аналізу |
| **Output** 🟢 | HeightMap Pass | `float[,]` | Прохідне (те ж саме, що на вході) |
| **Output** 🔵 | TileMap Pass | `string[,]` | Прохідне |
| **Output** 🟡 | Mask Pass | `bool[,]` | Прохідне |

## Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Label** | `string` | `"Debug"` | Позначка для логу |
| **Log To Console** | `bool` | `true` | Чи виводити логи в Console |

## Що логує

Для HeightMap:

```
[Debug] HeightMap [64x64] min=0.023 max=0.951 avg=0.472
```

Для TileMap:

```
[Debug] TileMap [64x64] unique tiles: 12
  grass: 450
  water: 320
  mountain: 120
  ...
```

Для Mask:

```
[Debug] Mask [64x64] true=2048 false=2048 (50.0%)
```

## Коли використовувати

- Вставте між нодами щоб перевірити проміжні результати
- Діагностика: "чому тут немає лісу?" → поставте Debug між HeightSource і ForestCluster
- Не впливає на результат — лише логує
