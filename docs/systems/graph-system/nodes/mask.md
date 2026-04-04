# Mask

← [Утиліти](../nodes-utility.md) · [Graph System](../README.md)

**Категорія:** Utility · **Файл:** `MaskNode.cs`

Створює **булеву маску** (`bool[,]`) з карти висот за пороговою умовою. Маска визначає зони, де інші ноди можуть або не можуть діяти.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Output** 🟡 | Mask | `bool[,]` | Булева маска |

## Параметри

| Параметр | Тип | За замовч. | Опис |
|---|---|---|---|
| **Mode** | `enum` | GreaterThan | Тип порівняння |
| **Threshold** | `float` | 0.5 | Порогове значення |
| **Upper Threshold** | `float` | 0.8 | Верхній поріг (тільки для Between) |

## Режими

| Режим | Формула | Результат |
|---|---|---|
| **GreaterThan** | `height > threshold` | true де висота вище порогу |
| **LessThan** | `height < threshold` | true де висота нижче порогу |
| **Between** | `threshold < height < upperThreshold` | true в діапазоні |

## Приклад

```
HeightMap:                    Mask (GreaterThan, 0.5):
0.2  0.4  0.6  0.8          false false true  true
0.3  0.5  0.7  0.9     →    false false true  true
0.1  0.3  0.5  0.7          false false false true
```

## Ігрове застосування

- `GreaterThan 0.6` → маска "гірських зон" → для MountainScatter
- `LessThan 0.3` → маска "низин" → для LakeGeneration
- `Between 0.3–0.6` → маска "рівнин" → для ForestCluster або POI
