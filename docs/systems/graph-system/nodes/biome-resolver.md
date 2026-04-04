# Biome Resolver

← [Генератори](../nodes-generators.md) · [Graph System](../README.md)

**Категорія:** Generators · **Файл:** `BiomeResolverNode.cs`

Призначає **біоми** (типи тайлів) на основі висоти та вологості. Використовує зовнішній сервіс `IBiomeResolver`, який додатково генерує карту вологості через Perlin noise.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Input** 🔵 | BaseTileMap | `string[,]` | Базова карта тайлів (буде модифікована) |
| **Output** 🔵 | BiomeMap | `string[,]` | Результуюча карта біомів |

## Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Biomes Settings** | `DataBiomesSettings` (SO) | Масив правил біомів з діапазонами висоти/вологості |

## Як працює

```
Для кожної клітинки:
  height = heightMap[x, y]        → наприклад, 0.45
  moisture = noise(x, y)          → наприклад, 0.7

  Перебір BiomeData[]:
    "desert":    height 0.3–0.5, moisture 0.0–0.3  → НЕ підходить (moisture 0.7 > 0.3)
    "grassland": height 0.3–0.6, moisture 0.4–0.8  → ПІДХОДИТЬ! → biomeMap[x,y] = "grassland"
```

## Типовий результат

| Висота ↓ / Вологість → | 0.0–0.3 | 0.3–0.6 | 0.6–1.0 |
|---|---|---|---|
| **0.0–0.2** (низовини) | `"sand"` | `"swamp"` | `"swamp"` |
| **0.2–0.5** (рівнини) | `"desert"` | `"grassland"` | `"forest"` |
| **0.5–0.8** (пагорби) | `"rock"` | `"hills"` | `"mountain_forest"` |
| **0.8–1.0** (гори) | `"snow"` | `"snow"` | `"snow"` |
