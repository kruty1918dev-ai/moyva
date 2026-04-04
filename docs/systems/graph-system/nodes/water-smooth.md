# Water Smooth

← [Особливості](../nodes-features.md) · [Graph System](../README.md)

**Категорія:** Features · **Файл:** `WaterSmoothNode.cs`

Згладжує **переходи між водними та сухопутними тайлами**. Використовує зовнішній `WaterPostProcessor` для створення природних берегів.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🔵 | BiomeMap | `string[,]` | Карта біомів з водними тайлами |
| **Input** 🔵 | ObjectMap | `string[,]` | (Опціонально) Карта об'єктів |
| **Input** 🟢 | HeightMap | `float[,]` | (Опціонально) Карта висот |
| **Output** 🔵 | BiomeMap | `string[,]` | Зглажена карта біомів |

## Коли використовувати

Ставте **після** `Lake Generation` та `River Generator`, **перед** `Output`:

```
Lake Gen → River → Water Smooth → AutoTile → Output
```
