# Fog of War — Алгоритм видимості

← [README](README.md)

---

## Symmetric Shadowcasting

Реалізовано алгоритм **Symmetric Shadowcasting** (8 октантів) за [Bob Nystrom](https://journal.stuffwithstuff.com/2015/09/07/what-the-hero-sees/).

### Чому симетричний?

Якщо юніт A бачить юніта B — то й юніт B бачить юніта A. Класичний shadowcasting цього не гарантує. Симетрична версія виправляє цю проблему за рахунок перевірки нахилів тіней.

---

## 8 Октантів

```
 \ 7 | 0 /
  \  |  /
6  \ | / 1
    \|/
----[O]----
    /|\
5  / | \ 2
  /  |  \
 / 4 | 3 \
```

Кожен октант охоплює 45°. Разом 360° навколо джерела зору.

---

## Логіка одного ряду

Для кожного ряду `row` (від 1 до `visionRange`):

1. Для кожного стовпця `col` від 0 до `row`:
   - Обчислити кут видимості тайлу: `slope1 = (col-0.5)/(row+0.5)`, `slope2 = (col+0.5)/(row-0.5)`
   - Якщо тайл повністю в тіні — пропустити
   - Інакше — додати в результат
   - Якщо тайл блокує (стіна) — додати до списку тіней

---

## Стіни (wall blocking)

Наразі `IGridService` не має wall API → всі тайли прохідні. Коли буде додано, потрібно:

```csharp
// У CastOctant — після result.Add(tile):
if (IsBlocking(tile))
    AddShadow(shadows, tileSlope1, tileSlope2);
```

---

## Межі карти

`FogVisibilityResolver` завжди перевіряє `IsInBounds(tile, mapWidth, mapHeight)`. Тайли поза межами ніколи не потрапляють у результат.

---

## Fallback (null gridService)

Якщо `IGridService == null`, `FogVisibilityResolver` будує **кругову зону** без блокування (radius-based), логуючи WARNING.

---

## Посилання

- [Bob Nystrom — What the Hero Sees](https://journal.stuffwithstuff.com/2015/09/07/what-the-hero-sees/)
- [Red Blob Games — Field of View](https://www.redblobgames.com/grids/hexagons/#field-of-view)
