# Tile Replace

← [Утиліти](../nodes-utility.md) · [Graph System](../README.md)

**Категорія:** Utility · **Файл:** `TileReplaceNode.cs`

Простий **пошук і заміна** — замінює всі входження одного тайлу на інший. Опціонально обмежує заміну маскою.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🔵 | TileMap | `string[,]` | Карта тайлів |
| **Input** 🟡 | Mask | `bool[,]` | (Опціонально) Маска — заміна відбувається тільки де `true` |
| **Output** 🔵 | Result | `string[,]` | Карта з замінами |

## Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Find Tile** | `string` | ID тайлу для пошуку |
| **Replace Tile** | `string` | Новий ID |

## Як працює

```csharp
// Для кожної клітинки:
if (mask != null && !mask[x, y]) continue;   // пропустити якщо маска = false
if (tileMap[x, y] == findTile)
    result[x, y] = replaceTile;
```

## Приклад

```
Find: "grass"  →  Replace: "meadow"

До:    grass  sand  grass  water
Після: meadow sand  meadow water
```
