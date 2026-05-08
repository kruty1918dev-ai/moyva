# Height To Tile

← [Генератори](../nodes-generators.md) · [Graph System](../README.md)

**Категорія:** Generators · **Файл:** `HeightToTileNode.cs`

Конвертує числову карту висот (`float[,]`) в текстову карту тайлів (`string[,]`), присвоюючи кожному діапазону висот свій ID тайлу.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | HeightMap | `float[,]` | Карта висот |
| **Output** 🔵 | TileMap | `string[,]` | Карта ID тайлів |
| **Output** 🟡 | LayerMask | `bool[,]` | `true` там, де висота потрапила у шар з індексом `Mask Layer Index` |
| **Output** 🟠 | LayerIndexMap | `int[,]` | Індекс шару `HeightLayers` для кожної клітинки (`-1`, якщо шар не знайдено) |

## Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Height Map Settings** | `HeightMapSettings` (SO) | Масив шарів висоти (HeightLayer[]) |
| **Mask Layer Index** | `int` | Індекс шару `HeightLayers`, для якого формується `LayerMask` |

## Як налаштувати `HeightMapSettings`

```
HeightLayer[]:
  ┌─────────────────────────────────────────┐
  │ [0] TileID: "water_deep"  MaxHeight: 0.2│  ← 0.0–0.2 → water_deep
  │ [1] TileID: "sand"        MaxHeight: 0.3│  ← 0.2–0.3 → sand
  │ [2] TileID: "grass"       MaxHeight: 0.5│  ← 0.3–0.5 → grass
  │ [3] TileID: "hills"       MaxHeight: 0.7│  ← 0.5–0.7 → hills
  │ [4] TileID: "mountain"    MaxHeight: 1.0│  ← 0.7–1.0 → mountain
  └─────────────────────────────────────────┘
```

> **Різниця з BiomeResolver:** HeightToTile враховує тільки висоту. [BiomeResolver](biome-resolver.md) додатково враховує вологість для складніших біомів.

## Layer outputs

- `LayerMask` зручний для швидкого фільтра по одному шару (наприклад, лише зона `HeightLayers[3]`).
- `LayerIndexMap` потрібен, коли downstream-нода має працювати з кількома шарами одразу (наприклад, `Hill Generator` з `target layer indices`).
- Якщо межі шарів налаштовані невалідно і шар для клітинки не знайдено, `LayerIndexMap[x,y] = -1`.
