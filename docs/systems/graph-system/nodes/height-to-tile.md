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

## Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Height Map Settings** | `HeightMapSettings` (SO) | Масив шарів висоти (HeightLayer[]) |

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
